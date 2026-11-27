#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FishNet;
using NUnit.Framework;
using OneMoreFloor.Core;
using OneMoreFloor.Floors.Trivia;
using OneMoreFloor.Game;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace OneMoreFloor.Tests.PlayMode
{
    /// <summary>
    /// Trivia played for real in its room, as a local host with one player: the floor clears, and hearts drain
    /// to game over, at level 1 and at its hardest level (docs/PROJECT_CONTEXT.md, section 11). The timers are
    /// shortened so a test doesn't wait 30 seconds per question.
    /// </summary>
    public class TriviaFloorTests
    {
        private const string ScenePath = "Assets/_OneMoreFloor/Art/Floors/Trivia/Review/TriviaArtReview.unity";
        private const float SecondsPerQuestion = 0.5f;
        private const float RevealSeconds = 0.15f;
        private const float Timeout = 60f;

        private readonly List<InputDevice> _disabledDevices = new List<InputDevice>();
        private PlayerController _player;
        private TriviaFloor _floor;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (!File.Exists(ScenePath))
                Assert.Ignore("Build the Trivia room scene first (tools/homepc/build-trivia.sh); it isn't kept in Git.");

            // Real mouse and keyboard input (someone at the machine, or Parsec) must not play along.
            foreach (InputDevice device in InputSystem.devices.ToArray())
            {
                if (!device.enabled || !(device is Pointer || device is Keyboard))
                    continue;
                InputSystem.DisableDevice(device);
                _disabledDevices.Add(device);
            }
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            FloorTest.Current = null;
            foreach (InputDevice device in _disabledDevices)
                InputSystem.EnableDevice(device);
            _disabledDevices.Clear();

            if (InstanceFinder.NetworkManager != null)
            {
                GameObject network = InstanceFinder.NetworkManager.gameObject;
                InstanceFinder.ClientManager.StopConnection();
                InstanceFinder.ServerManager.StopConnection(true);
                Object.Destroy(network);
            }

            Scene empty = SceneManager.CreateScene("TestEmptyScene" + Guid.NewGuid());
            SceneManager.SetActiveScene(empty);
            Scene trivia = SceneManager.GetSceneByPath(ScenePath);
            if (trivia.isLoaded)
                yield return SceneManager.UnloadSceneAsync(trivia);
        }

        [UnityTest]
        public IEnumerator Level1_AnsweringRight_ClearsTheFloor_WithoutLosingHearts() => ClearFloor(level: 1, questions: 5);

        [UnityTest]
        public IEnumerator Level3_AnsweringRight_ClearsTheFloor_WithoutLosingHearts() => ClearFloor(level: 3, questions: 12);

        [UnityTest]
        public IEnumerator Level1_NobodyAnswering_DrainsHeartsToGameOver() => DrainHearts(level: 1, questions: 5);

        [UnityTest]
        public IEnumerator Level3_NobodyAnswering_DrainsHeartsToGameOver_MidFloor() => DrainHearts(level: 3, questions: 12);

        [UnityTest]
        public IEnumerator AWrongAnswer_CostsOneHeart_AndTheFloorStillClears()
        {
            yield return StartFloor(level: 1, questions: 5);
            yield return GetIntoPositionAndStart();

            RunManager run = RunManager.Instance;
            bool first = true;
            yield return AnswerEveryQuestion(() =>
            {
                int answer = first ? (_floor.ServerCorrectAnswer + 1) % 4 : _floor.ServerCorrectAnswer;
                first = false;
                return answer;
            });

            Assert.That(run.FloorsCleared, Is.EqualTo(1));
            Assert.That(run.Hearts, Is.EqualTo(run.StartingHearts - 1));
        }

        [UnityTest]
        public IEnumerator SteppingOut_ReleasesThePlayer_AndFreesThePodium()
        {
            yield return StartFloor(level: 1, questions: 5);
            yield return GetIntoPositionAndStart(start: false);

            _floor.RequestLeavePodium();
            yield return Until(() => _floor.PodiumOf(_player.ClientId) < 0, "The podium should be freed.");
            yield return null;

            Assert.That(_player.IsHeld, Is.False, "The player should move freely again.");
            Assert.That(_floor.InPosition, Is.EqualTo(0));
            Assert.That(_floor.Stage, Is.EqualTo(TriviaStage.Waiting), "Nothing starts until everyone is in position and Ready is pressed.");
        }

        private IEnumerator ClearFloor(int level, int questions)
        {
            yield return StartFloor(level, questions);
            yield return GetIntoPositionAndStart();

            RunManager run = RunManager.Instance;
            int asked = 0;
            yield return AnswerEveryQuestion(() =>
            {
                asked++;
                return _floor.ServerCorrectAnswer;
            });

            Assert.That(asked, Is.EqualTo(questions), $"Level {level} asks {questions} questions.");
            Assert.That(run.FloorsCleared, Is.EqualTo(1));
            Assert.That(run.Hearts, Is.EqualTo(run.StartingHearts), "Right answers cost nothing.");
            Assert.That(_floor.Stage, Is.EqualTo(TriviaStage.Finished));
            yield return Until(() => !_player.IsHeld, "The player gets control back when the floor is cleared.");
        }

        private IEnumerator DrainHearts(int level, int questions)
        {
            yield return StartFloor(level, questions);
            RunManager run = RunManager.Instance;
            int startingHearts = run.StartingHearts;

            // Each question nobody answers costs 1 heart. When a floor is cleared first, the next one starts
            // (in the same room until the lift ride exists): get into position again and press Ready.
            int floorsPlayed = 0;
            while (run.Phase != RunPhase.GameOver && run.Phase != RunPhase.Results)
            {
                yield return Until(() => _floor.IsActive && _floor.Stage == TriviaStage.Waiting || run.Phase == RunPhase.GameOver,
                    "The next floor should start.", 30f);
                if (run.Phase == RunPhase.GameOver)
                    break;
                floorsPlayed++;
                yield return GetIntoPositionAndStart();
                yield return Until(() => run.Phase != RunPhase.FloorPlay, "The floor should end: cleared, or out of hearts.", Timeout);
            }

            Assert.That(run.Hearts, Is.EqualTo(0));
            Assert.That(floorsPlayed, Is.EqualTo((startingHearts + questions - 1) / questions), "Hearts drain 1 per unanswered question.");
            yield return Until(() => run.Phase == RunPhase.Results, "Game over leads to the results.", 15f);
            Assert.That(run.FloorsCleared, Is.EqualTo(floorsPlayed - 1));
        }

        // Starts a floor test of Trivia at a level, with short timers, and waits for the floor to be ready.
        private IEnumerator StartFloor(int level, int questions)
        {
            FloorTest.Current = new FloorTestRequest { floorId = "trivia", level = level, seed = 4242, skipTitleCard = true };
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));

            yield return Until(() => FindLocalPlayer() != null && RunManager.Instance != null && Object.FindAnyObjectByType<TriviaFloor>() != null,
                "The local host should spawn its player, the run and the floor.", 30f);
            _player = FindLocalPlayer();
            _floor = Object.FindAnyObjectByType<TriviaFloor>();

            // Shorter timers, then restart the floor so they apply.
            _floor.UseSettings(_floor.Settings.WithTimes(SecondsPerQuestion, RevealSeconds));
            RunManager.Instance.StartRun();
            yield return Until(() => _floor.IsActive && _floor.QuestionCount == questions, $"Level {level} should be ready with {questions} questions.");
            Assert.That(RunManager.Instance.Level, Is.EqualTo(level));
        }

        private IEnumerator GetIntoPositionAndStart(bool start = true)
        {
            TriviaRoom room = TriviaRoom.Find();
            Quaternion facing = Quaternion.Euler(0f, room.StandYaw[0], 0f);
            _player.PlaceAt(room.Stands[0] - facing * Vector3.forward * 0.6f + Vector3.up * 0.05f, room.StandYaw[0]);
            yield return new WaitForSeconds(0.2f);

            _floor.RequestEnterPodium(0);
            yield return Until(() => _floor.PodiumOf(_player.ClientId) == 0, "The player should get into position at podium 1.");
            yield return Until(() => _player.IsHeld, "In position, the player stands still at the podium.");
            Assert.That(_floor.EveryoneInPosition, Is.True);

            if (!start)
                yield break;
            _floor.RequestStart();
            yield return Until(() => _floor.Stage == TriviaStage.Question, "Ready should start the first question.");
        }

        // Votes in every question until the floor ends. choose() picks the answer, or -1 to not vote.
        private IEnumerator AnswerEveryQuestion(Func<int> choose)
        {
            RunManager run = RunManager.Instance;
            int votedRound = -1;
            float deadline = Time.realtimeSinceStartup + Timeout;
            while (run.Phase == RunPhase.FloorPlay && Time.realtimeSinceStartup < deadline)
            {
                if (_floor.Stage == TriviaStage.Question && _floor.Round != votedRound)
                {
                    votedRound = _floor.Round;
                    int answer = choose();
                    if (answer >= 0)
                        _floor.RequestVote(answer);
                }
                yield return null;
            }
            Assert.That(run.Phase, Is.Not.EqualTo(RunPhase.FloorPlay), "The floor should have ended.");
        }

        private static IEnumerator Until(Func<bool> condition, string message, float seconds = 10f)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(condition(), Is.True, message);
        }

        private static PlayerController FindLocalPlayer() =>
            Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).FirstOrDefault(player => player.IsLocalPlayer);
    }
}
#endif
