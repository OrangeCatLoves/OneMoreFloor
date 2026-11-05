using NUnit.Framework;
using OneMoreFloor.Core;

namespace OneMoreFloor.Tests.EditMode
{
    public class RunTests
    {
        private static Run NewRun(int players = 1) => new Run(seed: 1UL, playerCount: players, heartsPerPlayer: 10);

        private static Run RunInPlay(int players = 1)
        {
            Run run = NewRun(players);
            run.Start();
            run.OpenDoors();
            run.BeginPlay();
            return run;
        }

        [Test]
        public void NewRun_StartsInTheLobbyOnFloorOne()
        {
            Run run = NewRun(players: 3);

            Assert.That(run.Phase, Is.EqualTo(RunPhase.Lobby));
            Assert.That(run.FloorNumber, Is.EqualTo(1));
            Assert.That(run.FloorsCleared, Is.EqualTo(0));
            Assert.That(run.Hearts.Remaining, Is.EqualTo(30));
        }

        [Test]
        public void FloorLoop_FollowsThePhasesInOrder()
        {
            Run run = NewRun();

            Assert.That(run.Start(), Is.True);
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Elevator));
            Assert.That(run.OpenDoors(), Is.True);
            Assert.That(run.Phase, Is.EqualTo(RunPhase.FloorIntro));
            Assert.That(run.BeginPlay(), Is.True);
            Assert.That(run.Phase, Is.EqualTo(RunPhase.FloorPlay));
            Assert.That(run.ClearFloor(), Is.True);
            Assert.That(run.Phase, Is.EqualTo(RunPhase.FloorCleared));
            Assert.That(run.BoardElevator(), Is.True);
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Elevator));
        }

        [Test]
        public void ClearingFloors_RaisesTheScoreAndTheFloorNumber()
        {
            Run run = RunInPlay();

            run.ClearFloor();
            Assert.That(run.FloorsCleared, Is.EqualTo(1));
            Assert.That(run.FloorNumber, Is.EqualTo(1), "The floor count goes up on boarding the elevator.");

            run.BoardElevator();
            Assert.That(run.FloorNumber, Is.EqualTo(2));
        }

        [Test]
        public void HeartsCarryOverBetweenFloors()
        {
            Run run = RunInPlay();
            run.LoseHearts(4);
            run.ClearFloor();
            run.BoardElevator();

            Assert.That(run.Hearts.Remaining, Is.EqualTo(6));
        }

        [Test]
        public void ReachingZeroHearts_EndsTheRunMidFloor()
        {
            Run run = RunInPlay();

            run.LoseHearts(9);
            Assert.That(run.Phase, Is.EqualTo(RunPhase.FloorPlay));

            int lost = run.LoseHearts(5);

            Assert.That(lost, Is.EqualTo(1));
            Assert.That(run.Phase, Is.EqualTo(RunPhase.GameOver));
            Assert.That(run.Hearts.IsEmpty, Is.True);
        }

        [Test]
        public void GameOver_KeepsTheScoreFromClearedFloorsOnly()
        {
            Run run = RunInPlay();
            run.ClearFloor();
            run.BoardElevator();
            run.OpenDoors();
            run.BeginPlay();

            run.LoseHearts(10);

            Assert.That(run.Phase, Is.EqualTo(RunPhase.GameOver));
            Assert.That(run.FloorsCleared, Is.EqualTo(1));
            Assert.That(run.FloorNumber, Is.EqualTo(2));
        }

        [Test]
        public void ClearingAfterGameOver_DoesNothing()
        {
            Run run = RunInPlay();
            run.LoseHearts(10);

            Assert.That(run.ClearFloor(), Is.False);
            Assert.That(run.FloorsCleared, Is.EqualTo(0));
            Assert.That(run.Phase, Is.EqualTo(RunPhase.GameOver));
        }

        [Test]
        public void GameOver_LeadsToResults()
        {
            Run run = RunInPlay();
            run.LoseHearts(10);

            Assert.That(run.ShowResults(), Is.True);
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Results));
        }

        [Test]
        public void HeartsAreOnlyLostDuringPlay()
        {
            Run run = NewRun();
            Assert.That(run.LoseHearts(1), Is.EqualTo(0), "Lobby");

            run.Start();
            Assert.That(run.LoseHearts(1), Is.EqualTo(0), "Elevator");

            run.OpenDoors();
            Assert.That(run.LoseHearts(1), Is.EqualTo(0), "Title card");

            run.BeginPlay();
            run.ClearFloor();
            Assert.That(run.LoseHearts(1), Is.EqualTo(0), "After the floor is cleared");

            Assert.That(run.Hearts.Remaining, Is.EqualTo(10));
        }

        [Test]
        public void StepsFromTheWrongPhase_ChangeNothing()
        {
            Run run = NewRun();

            Assert.That(run.OpenDoors(), Is.False);
            Assert.That(run.BeginPlay(), Is.False);
            Assert.That(run.ClearFloor(), Is.False);
            Assert.That(run.BoardElevator(), Is.False);
            Assert.That(run.ShowResults(), Is.False);
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Lobby));
            Assert.That(run.FloorNumber, Is.EqualTo(1));
        }
    }
}
