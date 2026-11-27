using System;

namespace OneMoreFloor.Game
{
    /// <summary>
    /// A developer's request to play one floor straight away, at a chosen difficulty level, without the
    /// lobby, the lift ride or the floor picker. It is made in the Floor Tester window
    /// (OneMoreFloor > Floor Tester) and read by the OneMoreFloor.Dev assembly, which release builds leave
    /// out, so players can never start a floor test.
    /// </summary>
    [Serializable]
    public sealed class FloorTestRequest
    {
        /// <summary>The floor's id from its FloorDefinition, e.g. "trivia".</summary>
        public string floorId = string.Empty;

        /// <summary>The difficulty level to play (1, 2, 3, ...). It stays the same after each clear.</summary>
        public int level = 1;

        /// <summary>The run's random seed, to replay the same run; 0 picks a new seed each time.</summary>
        public int seed;

        /// <summary>Mistakes still show, but the team never runs out of hearts.</summary>
        public bool unlimitedHearts;

        /// <summary>Start playing at once, without the floor's title card.</summary>
        public bool skipTitleCard = true;
    }

    /// <summary>The floor test being played, if any. Null in normal play.</summary>
    public static class FloorTest
    {
        public static FloorTestRequest Current { get; set; }
    }
}
