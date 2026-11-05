namespace OneMoreFloor.Core
{
    /// <summary>The floor the director chose, the room tier to load and the difficulty level to play at.</summary>
    public readonly struct FloorPick
    {
        public FloorPick(string floorId, int roomTier, int difficultyLevel)
        {
            FloorId = floorId;
            RoomTier = roomTier;
            DifficultyLevel = difficultyLevel;
        }

        public string FloorId { get; }
        public int RoomTier { get; }
        public int DifficultyLevel { get; }
    }
}
