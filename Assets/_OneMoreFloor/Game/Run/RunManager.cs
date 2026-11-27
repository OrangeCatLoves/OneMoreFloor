using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using OneMoreFloor.Core;
using OneMoreFloor.FloorSDK;
using UnityEngine;

namespace OneMoreFloor.Game
{
    /// <summary>
    /// The run, on the host: hearts, the floor sequence and each floor's lifecycle
    /// (title card → play → cleared → next floor; hearts at 0 → game over → results). Clients get read-only
    /// copies for the HUD. Floors talk to it only through <see cref="IFloorRun"/>.
    /// Until the lobby and the lift ride exist (build step 4), a run starts with the session, the ride is
    /// instant and the same room serves every floor.
    /// A floor test (<see cref="FloorTest"/>, developers only) plays one floor at a chosen difficulty level.
    /// </summary>
    public sealed class RunManager : NetworkBehaviour, IFloorRun
    {
        [SerializeField]
        private GameRules rules;

        [SerializeField]
        private DirectorSettings director;

        [SerializeField, Tooltip("The floors a run can pick from. Only Trivia exists so far.")]
        private FloorDefinition[] floors;

        private readonly SyncVar<int> _phase = new SyncVar<int>((int)RunPhase.Lobby);
        private readonly SyncVar<int> _hearts = new SyncVar<int>();
        private readonly SyncVar<int> _startingHearts = new SyncVar<int>();
        private readonly SyncVar<int> _floorNumber = new SyncVar<int>(1);
        private readonly SyncVar<int> _floorsCleared = new SyncVar<int>();
        private readonly SyncVar<int> _floorIndex = new SyncVar<int>(-1);

        // Host only.
        private Run _run;
        private NetworkFloor _floor;
        private int _spawnedFloorIndex = -1;
        private SeededRandom _floorRandom;
        private bool _teamLocked;
        private Coroutine _flow;
        private FloorTestRequest _test;
        private readonly Dictionary<string, object> _runMemory = new Dictionary<string, object>();

        public static RunManager Instance { get; private set; }

        /// <summary>Client side: hearts were just lost (amount, reason).</summary>
        public event Action<int, string> HeartsLost;

        public RunPhase Phase => (RunPhase)_phase.Value;
        public int Hearts => _hearts.Value;
        public int StartingHearts => _startingHearts.Value;
        public int FloorNumber => _floorNumber.Value;
        public int FloorsCleared => _floorsCleared.Value;
        public FloorDefinition CurrentFloor => _floorIndex.Value >= 0 && _floorIndex.Value < floors.Length ? floors[_floorIndex.Value] : null;
        public IReadOnlyList<FloorDefinition> Floors => floors;
        public GameRules Rules => rules;

        /// <summary>Host only: the floor test being played, or null in normal play.</summary>
        public FloorTestRequest Test => _test;

        // IFloorRun (host side).
        public int Level => _test != null ? Mathf.Max(1, _test.level) : Difficulty.LevelFor(Mathf.Max(1, FloorNumber), FloorsPerLevel);
        public bool IsPlaying => _run != null && _run.Phase == RunPhase.FloorPlay;
        public SeededRandom Random => _floorRandom;

        private int FloorsPerLevel => director.ToRules().FloorsPerDifficultyLevel;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            Instance = this;
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
            if (Instance == this)
                Instance = null;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (FindAnyObjectByType<RunHud>() == null)
                new GameObject("RunHud").AddComponent<RunHud>();
            if (FindAnyObjectByType<LiftFloorSigns>() == null)
                new GameObject("LiftFloorSigns").AddComponent<LiftFloorSigns>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
            StartRun();
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
        }

        private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
        {
            // Until the lobby exists, players who join before the team has lost a heart or cleared a floor
            // are counted in the starting hearts.
            RecountTeam(args.ConnectionState == RemoteConnectionState.Stopped ? connection : null);
        }

        /// <summary>Host only: starts a new run (after Play again too). A floor test restarts its floor.</summary>
        public void StartRun()
        {
            if (!IsServerInitialized)
                return;
            if (_flow != null)
                StopCoroutine(_flow);
            _flow = null;
            if (_floor != null)
                _floor.EndPlay();

            _test = FloorTest.Current;
            _teamLocked = false;
            _runMemory.Clear();

            // The run's seed: everything random in the run comes from it.
            ulong seed = _test != null && _test.seed != 0 ? (ulong)(uint)_test.seed : (ulong)DateTime.UtcNow.Ticks;
            _run = new Run(seed, Mathf.Max(1, CountPlayers(null)), rules.HeartsPerPlayer, FirstFloor());
            _run.Start();
            _flow = StartCoroutine(EnterFloor());
        }

        public void LoseHearts(int amount, string reason, int playerClientId = -1)
        {
            if (!IsServerInitialized || _run == null || amount <= 0 || !IsPlaying)
                return;

            if (_test != null && _test.unlimitedHearts)
            {
                // A floor test with unlimited hearts: show the mistake, keep the hearts.
                ObserversHeartsLost(amount, reason ?? string.Empty);
                return;
            }

            int lost = _run.LoseHearts(amount);
            if (lost == 0)
                return;

            _teamLocked = true;
            Sync();
            ObserversHeartsLost(lost, reason ?? string.Empty);
            if (_run.Phase == RunPhase.GameOver)
            {
                if (_floor != null)
                    _floor.EndPlay();
                if (_flow != null)
                    StopCoroutine(_flow);
                _flow = StartCoroutine(ShowResultsLater());
            }
        }

        public void Cleared()
        {
            if (!IsServerInitialized || _run == null || !_run.ClearFloor())
                return;

            _teamLocked = true;
            _floor.EndPlay();
            Sync();
            _flow = StartCoroutine(NextFloorLater());
        }

        public T RunMemory<T>(string key, Func<T> create) where T : class
        {
            if (_runMemory.TryGetValue(key, out object found) && found is T existing)
                return existing;
            T created = create();
            _runMemory[key] = created;
            return created;
        }

        /// <summary>Any player can start a new run from the results screen.</summary>
        [ServerRpc(RequireOwnership = false)]
        public void ServerPlayAgain()
        {
            if (_run != null && _run.Phase == RunPhase.Results)
                StartRun();
        }

        [ObserversRpc]
        private void ObserversHeartsLost(int amount, string reason) => HeartsLost?.Invoke(amount, reason);

        private IEnumerator EnterFloor()
        {
            // No lift ride yet: the doors open at once on the next floor.
            int index = PickFloor();
            _floorIndex.Value = index;
            SpawnFloor(index);
            _run.OpenDoors();
            _floorRandom = new SeededRandom(_run.Seed, (ulong)_run.FloorNumber);
            Sync();

            if (_test == null || !_test.skipTitleCard)
                yield return new WaitForSeconds(rules.IntroSeconds);

            if (_run.BeginPlay())
            {
                Sync();
                _floor.BeginPlay(this);
            }
            _flow = null;
        }

        private IEnumerator NextFloorLater()
        {
            yield return new WaitForSeconds(rules.FloorClearedSeconds);
            if (_run.BoardElevator())
            {
                Sync();
                yield return EnterFloor();
            }
        }

        private IEnumerator ShowResultsLater()
        {
            yield return new WaitForSeconds(rules.GameOverSeconds);
            _run.ShowResults();
            Sync();
            _flow = null;
        }

        // A floor test always plays its floor. Otherwise only Trivia exists: the seeded FloorPicker (Core)
        // takes over once there's a second floor.
        private int PickFloor()
        {
            if (_test != null)
            {
                for (int i = 0; i < floors.Length; i++)
                    if (floors[i] != null && floors[i].Id == _test.floorId)
                        return i;
                Debug.LogWarning($"Floor test: no floor with id '{_test.floorId}' in the RunManager's list; playing {floors[0].Id}.");
            }
            return 0;
        }

        // A floor test starts on the first floor of its level, so the floor number on screen matches it.
        private int FirstFloor() => _test == null ? 1 : (Mathf.Max(1, _test.level) - 1) * FloorsPerLevel + 1;

        private void SpawnFloor(int index)
        {
            if (_floor != null && _spawnedFloorIndex == index)
                return;
            if (_floor != null)
                ServerManager.Despawn(_floor.gameObject);

            NetworkFloor floor = Instantiate(floors[index].FloorPrefab);
            ServerManager.Spawn(floor.gameObject);
            _floor = floor;
            _spawnedFloorIndex = index;
        }

        private int CountPlayers(NetworkConnection leaving)
        {
            int players = 0;
            foreach (NetworkConnection connection in ServerManager.Clients.Values)
                if (connection.IsActive && connection != leaving)
                    players++;
            return players;
        }

        private void RecountTeam(NetworkConnection leaving)
        {
            if (_run == null || _teamLocked)
                return;
            int players = Mathf.Max(1, CountPlayers(leaving));
            if (players == _run.PlayerCount)
                return;

            // Nothing has been lost or cleared yet, so rebuild the run for the new team size in the same phase.
            RunPhase phase = _run.Phase;
            var run = new Run(_run.Seed, players, rules.HeartsPerPlayer, _run.FloorNumber);
            run.Start();
            if (phase >= RunPhase.FloorIntro)
                run.OpenDoors();
            if (phase >= RunPhase.FloorPlay)
                run.BeginPlay();
            _run = run;
            Sync();
        }

        private void Sync()
        {
            _phase.Value = (int)_run.Phase;
            _hearts.Value = _run.Hearts.Remaining;
            _startingHearts.Value = _run.Hearts.Starting;
            _floorNumber.Value = _run.FloorNumber;
            _floorsCleared.Value = _run.FloorsCleared;
        }
    }
}
