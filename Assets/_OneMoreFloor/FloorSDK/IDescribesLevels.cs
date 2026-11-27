namespace OneMoreFloor.FloorSDK
{
    /// <summary>
    /// A floor's settings asset can say what changes per difficulty level, e.g. "8 questions, medium".
    /// Developer tools such as the Floor Tester show it; the game itself doesn't.
    /// </summary>
    public interface IDescribesLevels
    {
        /// <summary>How many levels the settings define. Levels past the last one play like the last one.</summary>
        int LevelCount { get; }

        /// <summary>A short summary of one level, for developers (not shown to players).</summary>
        string DescribeLevel(int level);
    }
}
