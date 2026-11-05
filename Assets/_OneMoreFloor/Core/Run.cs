using System;

namespace OneMoreFloor.Core
{
    /// <summary>
    /// The state of one run, owned by the host. Each run is a new object, so returning to the
    /// lobby after Results means creating a new Run.
    /// </summary>
    public sealed class Run
    {
        public Run(ulong seed, int playerCount, int heartsPerPlayer)
        {
            Seed = seed;
            PlayerCount = playerCount;
            Hearts = new HeartsPool(HeartsPool.StartingHearts(heartsPerPlayer, playerCount));
        }

        public ulong Seed { get; }

        /// <summary>How many players started the run. It doesn't change if someone leaves.</summary>
        public int PlayerCount { get; }

        public HeartsPool Hearts { get; }
        public RunPhase Phase { get; private set; } = RunPhase.Lobby;

        /// <summary>The floor the team is on or riding to, starting at 1.</summary>
        public int FloorNumber { get; private set; } = 1;

        /// <summary>The run's score.</summary>
        public int FloorsCleared { get; private set; }

        // Each step returns whether it happened. A step from the wrong phase changes nothing, so a
        // floor that reports "cleared" just after the run has ended does no harm.

        public bool Start() => Move(RunPhase.Lobby, RunPhase.Elevator);

        public bool OpenDoors() => Move(RunPhase.Elevator, RunPhase.FloorIntro);

        public bool BeginPlay() => Move(RunPhase.FloorIntro, RunPhase.FloorPlay);

        public bool ClearFloor()
        {
            if (!Move(RunPhase.FloorPlay, RunPhase.FloorCleared))
                return false;

            FloorsCleared++;
            return true;
        }

        public bool BoardElevator()
        {
            if (!Move(RunPhase.FloorCleared, RunPhase.Elevator))
                return false;

            FloorNumber++;
            return true;
        }

        public bool ShowResults() => Move(RunPhase.GameOver, RunPhase.Results);

        /// <summary>
        /// Applies a mistake and returns how many hearts were lost. Hearts are only lost during play.
        /// The run ends the moment the pool reaches 0.
        /// </summary>
        public int LoseHearts(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Cannot be negative.");
            if (Phase != RunPhase.FloorPlay)
                return 0;

            int lost = Hearts.Lose(amount);
            if (Hearts.IsEmpty)
                Phase = RunPhase.GameOver;

            return lost;
        }

        private bool Move(RunPhase from, RunPhase to)
        {
            if (Phase != from)
                return false;

            Phase = to;
            return true;
        }
    }
}
