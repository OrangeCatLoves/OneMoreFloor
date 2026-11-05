using System;
using System.Collections.Generic;

namespace OneMoreFloor.Core
{
    /// <summary>The numbers the director uses. The owner tunes them in the DirectorSettings asset.</summary>
    public sealed class DirectorRules
    {
        public DirectorRules(int floorsPerDifficultyLevel, IReadOnlyList<int> tierStartFloors)
        {
            if (floorsPerDifficultyLevel < 1)
                throw new ArgumentOutOfRangeException(nameof(floorsPerDifficultyLevel), floorsPerDifficultyLevel, "Must be at least 1.");
            if (tierStartFloors == null || tierStartFloors.Count == 0 || tierStartFloors[0] != 1)
                throw new ArgumentException("The first tier must start at floor 1.", nameof(tierStartFloors));

            for (int i = 1; i < tierStartFloors.Count; i++)
            {
                if (tierStartFloors[i] <= tierStartFloors[i - 1])
                    throw new ArgumentException("Tier start floors must increase.", nameof(tierStartFloors));
            }

            FloorsPerDifficultyLevel = floorsPerDifficultyLevel;
            TierStartFloors = tierStartFloors;
        }

        public int FloorsPerDifficultyLevel { get; }
        public IReadOnlyList<int> TierStartFloors { get; }
    }
}
