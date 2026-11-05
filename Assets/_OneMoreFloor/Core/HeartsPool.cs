using System;

namespace OneMoreFloor.Core
{
    /// <summary>The team's one shared pool of hearts for a run.</summary>
    public sealed class HeartsPool
    {
        public HeartsPool(int starting)
        {
            if (starting <= 0)
                throw new ArgumentOutOfRangeException(nameof(starting), starting, "Must be above 0.");

            Starting = starting;
            Remaining = starting;
        }

        public int Starting { get; }
        public int Remaining { get; private set; }
        public bool IsEmpty => Remaining == 0;

        public static int StartingHearts(int heartsPerPlayer, int players)
        {
            if (heartsPerPlayer <= 0)
                throw new ArgumentOutOfRangeException(nameof(heartsPerPlayer), heartsPerPlayer, "Must be above 0.");
            if (players <= 0)
                throw new ArgumentOutOfRangeException(nameof(players), players, "Must be above 0.");

            return heartsPerPlayer * players;
        }

        /// <summary>Removes hearts and returns how many were actually lost. The pool never goes below 0.</summary>
        public int Lose(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Cannot be negative.");

            int lost = Math.Min(amount, Remaining);
            Remaining -= lost;
            return lost;
        }
    }
}
