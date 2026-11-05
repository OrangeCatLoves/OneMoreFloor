using System;

namespace OneMoreFloor.Core
{
    public static class Difficulty
    {
        /// <summary>level = 1 + ((floor - 1) / floorsPerLevel), rounded down. With 5 floors per level, floor 7 is level 2.</summary>
        public static int LevelFor(int floorNumber, int floorsPerLevel)
        {
            if (floorNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(floorNumber), floorNumber, "Floors start at 1.");
            if (floorsPerLevel < 1)
                throw new ArgumentOutOfRangeException(nameof(floorsPerLevel), floorsPerLevel, "Must be at least 1.");

            return 1 + (floorNumber - 1) / floorsPerLevel;
        }
    }
}
