using System;
using System.Collections.Generic;

namespace OneMoreFloor.Core
{
    public static class VisualTiers
    {
        /// <summary>
        /// Returns the tier for a floor. tierStartFloors lists each tier's first floor in order, so
        /// { 1, 11, 21 } makes floors 1-10 tier 1, floors 11-20 tier 2, and floor 21 onward tier 3.
        /// </summary>
        public static int TierFor(int floorNumber, IReadOnlyList<int> tierStartFloors)
        {
            if (floorNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(floorNumber), floorNumber, "Floors start at 1.");

            int tier = 1;
            for (int i = 1; i < tierStartFloors.Count && floorNumber >= tierStartFloors[i]; i++)
                tier = i + 1;

            return tier;
        }

        /// <summary>Returns the room tier to use: the highest one the floor has at or below the current tier, or null if it has none.</summary>
        public static int? BestRoomTier(int currentTier, IReadOnlyList<int> roomTiers)
        {
            int? best = null;
            foreach (int roomTier in roomTiers)
            {
                if (roomTier <= currentTier && (best == null || roomTier > best))
                    best = roomTier;
            }

            return best;
        }
    }
}
