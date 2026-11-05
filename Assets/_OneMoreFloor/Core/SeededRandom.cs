using System;

namespace OneMoreFloor.Core
{
    /// <summary>
    /// The run's random generator (the PCG32 algorithm). It is written here rather than using
    /// System.Random so that the same seed gives the same numbers on every machine and runtime.
    /// </summary>
    public sealed class SeededRandom
    {
        private const ulong Multiplier = 6364136223846793005UL;
        private const ulong DefaultStream = 54UL;

        private readonly ulong _increment;
        private ulong _state;

        public SeededRandom(ulong seed, ulong stream = DefaultStream)
        {
            unchecked
            {
                _increment = (stream << 1) | 1UL;
                _state = 0UL;
                NextUInt();
                _state += seed;
                NextUInt();
            }
        }

        public uint NextUInt()
        {
            unchecked
            {
                ulong previous = _state;
                _state = previous * Multiplier + _increment;
                uint mixed = (uint)(((previous >> 18) ^ previous) >> 27);
                int rotation = (int)(previous >> 59);
                return (mixed >> rotation) | (mixed << (-rotation & 31));
            }
        }

        /// <summary>Returns a whole number from 0 up to, but not including, maxExclusive. Every value is equally likely.</summary>
        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), maxExclusive, "Must be above 0.");

            uint bound = (uint)maxExclusive;
            uint threshold = unchecked(0u - bound) % bound;
            while (true)
            {
                uint value = NextUInt();
                if (value >= threshold)
                    return (int)(value % bound);
            }
        }
    }
}
