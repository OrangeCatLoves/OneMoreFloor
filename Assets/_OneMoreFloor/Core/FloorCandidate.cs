using System;
using System.Collections.Generic;

namespace OneMoreFloor.Core
{
    /// <summary>What the floor picker needs to know about a floor.</summary>
    public sealed class FloorCandidate
    {
        public FloorCandidate(string id, string genre, int minFloor, int minPlayers, int maxPlayers, int weight, IReadOnlyList<int> roomTiers)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("A floor needs an id.", nameof(id));
            if (string.IsNullOrEmpty(genre))
                throw new ArgumentException("A floor needs a genre.", nameof(genre));

            Id = id;
            Genre = genre;
            MinFloor = minFloor;
            MinPlayers = minPlayers;
            MaxPlayers = maxPlayers;
            Weight = weight;
            RoomTiers = roomTiers ?? throw new ArgumentNullException(nameof(roomTiers));
        }

        public string Id { get; }
        public string Genre { get; }
        public int MinFloor { get; }
        public int MinPlayers { get; }
        public int MaxPlayers { get; }

        /// <summary>A higher weight makes the floor appear more often. 0 or below means it never appears.</summary>
        public int Weight { get; }

        public IReadOnlyList<int> RoomTiers { get; }
    }
}
