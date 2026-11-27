using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using OneMoreFloor.FloorSDK;
using UnityEngine;

namespace OneMoreFloor.Floors.Trivia
{
    public enum TriviaStage
    {
        /// <summary>Players are getting into position; the questions start when one of them presses Ready.</summary>
        Waiting,
        Question,
        Reveal,
        Finished,
    }

    /// <summary>
    /// Trivia's floor controller (docs/floors/trivia.md). The host runs the questions, the votes and the
    /// timer; players only ask to step into or out of a podium, vote and press Ready. Everyone reads the state
    /// from the synced values below, which <see cref="TriviaPresenter"/> shows on screen and in the room.
    /// </summary>
    public sealed class TriviaFloor : NetworkFloor
    {
        public const int PodiumCount = TriviaRoom.PodiumCount;
        public const int Nobody = -1;
        public const int NoAnswer = -1;

        private const string QuestionsMemory = "trivia.questions";

        // Players move their own characters, so the host sees them a moment late: allow a little extra reach.
        private const float ReachSlack = 1f;

        [SerializeField]
        private TriviaSettings settings;

        [SerializeField, Tooltip("The question bank (Data/TriviaQuestions.json).")]
        private TextAsset questionBank;

        private readonly SyncVar<bool> _active = new SyncVar<bool>();
        private readonly SyncVar<int> _stage = new SyncVar<int>((int)TriviaStage.Waiting);
        private readonly SyncVar<int> _round = new SyncVar<int>();
        private readonly SyncVar<int> _questionNumber = new SyncVar<int>();
        private readonly SyncVar<int> _questionCount = new SyncVar<int>();
        private readonly SyncVar<bool> _askedAgain = new SyncVar<bool>();
        private readonly SyncVar<string> _question = new SyncVar<string>(string.Empty);
        private readonly SyncList<string> _answers = new SyncList<string>();
        private readonly SyncVar<uint> _votingEndsTick = new SyncVar<uint>();
        private readonly SyncVar<int> _players = new SyncVar<int>();
        private readonly SyncList<int> _podiums = new SyncList<int>();
        private readonly SyncVar<int> _outcome = new SyncVar<int>();
        private readonly SyncVar<int> _correct = new SyncVar<int>(NoAnswer);
        private readonly SyncVar<int> _heartsLost = new SyncVar<int>();
        private readonly SyncList<int> _counts = new SyncList<int>();
        private readonly SyncList<int> _podiumVotes = new SyncList<int>();

        // Host only.
        private IFloorRun _run;
        private TriviaGame _game;
        private Coroutine _flow;
        private TriviaRoom _room;

        public TriviaSettings Settings => settings;

        /// <summary>True from the end of the title card until the floor is cleared or the run ends.</summary>
        public bool IsActive => _active.Value;

        public TriviaStage Stage => (TriviaStage)_stage.Value;

        /// <summary>Goes up each time a question is asked (again), so screens know to reset.</summary>
        public int Round => _round.Value;

        public int QuestionNumber => _questionNumber.Value;
        public int QuestionCount => _questionCount.Value;
        public bool AskedAgain => _askedAgain.Value;
        public string Question => _question.Value;
        public IReadOnlyList<string> Answers => _answers;
        public int Players => _players.Value;
        public IReadOnlyList<int> Podiums => _podiums;
        public VoteOutcome Outcome => (VoteOutcome)_outcome.Value;

        /// <summary>The right answer's position (0 to 3), shown only once time runs out; -1 before that.</summary>
        public int Correct => _correct.Value;

        public int HeartsLost => _heartsLost.Value;
        public IReadOnlyList<int> Counts => _counts;

        /// <summary>Each podium player's vote, shown once time runs out; -1 for no vote or an empty podium.</summary>
        public IReadOnlyList<int> PodiumVotes => _podiumVotes;

        public float SecondsLeft
        {
            get
            {
                if (TimeManager == null)
                    return 0f;
                double left = TimeManager.TicksToTime(_votingEndsTick.Value) - TimeManager.TicksToTime(TimeManager.Tick);
                return Mathf.Clamp((float)left, 0f, settings.SecondsPerQuestion);
            }
        }

        public int InPosition
        {
            get
            {
                int count = 0;
                foreach (int occupant in _podiums)
                    if (occupant != Nobody)
                        count++;
                return count;
            }
        }

        public bool EveryoneInPosition => _players.Value > 0 && InPosition >= _players.Value;

        public int PodiumOf(int clientId)
        {
            for (int i = 0; i < _podiums.Count; i++)
                if (_podiums[i] == clientId)
                    return i;
            return -1;
        }

        // ----- Requests from players (any machine) -----

        public void RequestEnterPodium(int podium) => ServerEnterPodium(podium);
        public void RequestLeavePodium() => ServerLeavePodium();
        public void RequestVote(int answer) => ServerVote(answer);
        public void RequestStart() => ServerStart();

        // ----- Host -----

        public override void OnStartServer()
        {
            base.OnStartServer();
            for (int i = 0; i < PodiumCount; i++)
            {
                _podiums.Add(Nobody);
                _podiumVotes.Add(NoAnswer);
            }
            for (int i = 0; i < TriviaQuestion.AnswerCount; i++)
            {
                _answers.Add(string.Empty);
                _counts.Add(0);
            }
            ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            StopFlow();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            gameObject.AddComponent<TriviaPresenter>().Show(this);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            if (TryGetComponent(out TriviaPresenter presenter))
                Destroy(presenter);
        }

        public override void BeginPlay(IFloorRun run)
        {
            if (!IsServerInitialized)
                return;

            StopFlow();
            _run = run;
            _room ??= TriviaRoom.Find();

            TriviaSettings.Level level = settings.ForLevel(run.Level);
            QuestionPicker picker = run.RunMemory(QuestionsMemory, () => new QuestionPicker(TriviaQuestionBank.Parse(questionBank.text)));
            _game = new TriviaGame(picker.Pick(level.questions, level.difficulty, run.Random), settings.Rules);

            _questionNumber.Value = 0;
            _questionCount.Value = _game.QuestionCount;
            _askedAgain.Value = false;
            _question.Value = string.Empty;
            for (int i = 0; i < TriviaQuestion.AnswerCount; i++)
            {
                _answers[i] = string.Empty;
                _counts[i] = 0;
            }
            for (int i = 0; i < PodiumCount; i++)
                _podiumVotes[i] = NoAnswer;
            _correct.Value = NoAnswer;
            _stage.Value = (int)TriviaStage.Waiting;
            _active.Value = true;
        }

        public override void EndPlay()
        {
            if (!IsServerInitialized)
                return;

            StopFlow();
            _active.Value = false;
            for (int i = 0; i < PodiumCount; i++)
                if (_podiums[i] != Nobody)
                    _podiums[i] = Nobody;
        }

        /// <summary>Host only, for tests: the right answer's position for the question being asked, or -1.</summary>
        public int ServerCorrectAnswer => IsServerInitialized && _game != null && !_game.IsFinished ? _game.Current.Correct : NoAnswer;

        /// <summary>Host only, for tests: use other settings (e.g. short timers) from the next BeginPlay.</summary>
        public void UseSettings(TriviaSettings replacement) => settings = replacement;

        private void Update()
        {
            if (!IsServerInitialized)
                return;
            int players = FloorPlayers.All.Count;
            if (_players.Value != players)
                _players.Value = players;
        }

        [ServerRpc(RequireOwnership = false)]
        private void ServerEnterPodium(int podium, NetworkConnection sender = null)
        {
            if (!_active.Value || sender == null || podium < 0 || podium >= PodiumCount)
                return;
            if (_podiums[podium] != Nobody || PodiumOf(sender.ClientId) >= 0)
                return;

            IFloorPlayer player = FloorPlayers.ByClient(sender.ClientId);
            if (player == null || _room == null)
                return;
            if (_room.PodiumWithin(player.Body.position, settings.PodiumReach + ReachSlack) != podium)
                return;

            _podiums[podium] = sender.ClientId;
        }

        [ServerRpc(RequireOwnership = false)]
        private void ServerLeavePodium(NetworkConnection sender = null)
        {
            if (sender != null)
                FreePodiumOf(sender.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ServerVote(int answer, NetworkConnection sender = null)
        {
            if (sender == null || Stage != TriviaStage.Question || PodiumOf(sender.ClientId) < 0)
                return;
            _game.Vote(sender.ClientId, answer);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ServerStart(NetworkConnection sender = null)
        {
            if (!_active.Value || Stage != TriviaStage.Waiting || sender == null || PodiumOf(sender.ClientId) < 0 || !EveryoneInPosition)
                return;
            _flow = StartCoroutine(PlayQuestions());
        }

        private IEnumerator PlayQuestions()
        {
            IFloorRun run = _run;
            while (!_game.IsFinished)
            {
                Ask();
                uint end = _votingEndsTick.Value;
                while (TimeManager.Tick < end)
                    yield return null;

                VoteResult result = Reveal();
                if (result.HeartsLost > 0)
                {
                    run.LoseHearts(result.HeartsLost, "trivia." + result.Outcome);
                    if (!run.IsPlaying)
                    {
                        // Out of hearts: the run has ended the floor. The reveal stays up behind the game-over banner.
                        _flow = null;
                        yield break;
                    }
                }
                yield return new WaitForSeconds(settings.RevealSeconds);
            }

            _stage.Value = (int)TriviaStage.Finished;
            _flow = null;
            run.Cleared();
        }

        private void Ask()
        {
            AskedQuestion question = _game.Current;
            _questionNumber.Value = _game.QuestionNumber;
            _askedAgain.Value = _game.TiesOnThisQuestion > 0;
            _question.Value = question.Text;
            for (int i = 0; i < TriviaQuestion.AnswerCount; i++)
            {
                _answers[i] = question.Answers[i];
                _counts[i] = 0;
            }
            for (int i = 0; i < PodiumCount; i++)
                _podiumVotes[i] = NoAnswer;
            _correct.Value = NoAnswer;
            _votingEndsTick.Value = TimeManager.Tick + TimeManager.TimeToTicks(settings.SecondsPerQuestion);
            _round.Value++;
            _stage.Value = (int)TriviaStage.Question;
        }

        private VoteResult Reveal()
        {
            for (int i = 0; i < PodiumCount; i++)
            {
                int occupant = _podiums[i];
                _podiumVotes[i] = occupant != Nobody && _game.TryGetVote(occupant, out int vote) ? vote : NoAnswer;
            }

            VoteResult result = _game.CloseVoting();
            for (int i = 0; i < TriviaQuestion.AnswerCount; i++)
                _counts[i] = result.Counts[i];
            _outcome.Value = (int)result.Outcome;
            _correct.Value = result.Correct;
            _heartsLost.Value = result.HeartsLost;
            _stage.Value = (int)TriviaStage.Reveal;
            return result;
        }

        private void FreePodiumOf(int clientId)
        {
            int podium = PodiumOf(clientId);
            if (podium < 0)
                return;
            _podiums[podium] = Nobody;
            // Their vote no longer counts: the rest of the team decides.
            _game?.Withdraw(clientId);
        }

        private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
        {
            if (args.ConnectionState == RemoteConnectionState.Stopped)
                FreePodiumOf(connection.ClientId);
        }

        private void StopFlow()
        {
            if (_flow != null)
                StopCoroutine(_flow);
            _flow = null;
        }
    }
}
