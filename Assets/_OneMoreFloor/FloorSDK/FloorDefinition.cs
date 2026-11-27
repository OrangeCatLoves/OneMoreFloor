using UnityEngine;

namespace OneMoreFloor.FloorSDK
{
    /// <summary>
    /// What the game needs to know about a floor (docs/PROJECT_CONTEXT.md, section 4). One asset per floor.
    /// Its name and goal are string-table keys, so every word on screen comes from the string tables.
    /// </summary>
    [CreateAssetMenu(menuName = "OneMoreFloor/Floor Definition", fileName = "FloorDefinition")]
    public sealed class FloorDefinition : ScriptableObject
    {
        [SerializeField, Tooltip("Stable id, e.g. trivia. Never change it once players have runs saved.")]
        private string id = "floor_id";

        [SerializeField, Tooltip("String table holding this floor's text.")]
        private string stringTable = "Game";

        [SerializeField, Tooltip("Key of the floor's name in its string table.")]
        private string nameKey = "name";

        [SerializeField, Tooltip("Key of the floor's one-line goal in its string table.")]
        private string goalKey = "goal";

        [SerializeField, Tooltip("Genre, for the floor picker's no-repeat rule.")]
        private string genre = "genre";

        [SerializeField, Min(1)] private int minPlayers = 1;
        [SerializeField, Min(1)] private int maxPlayers = 4;

        [SerializeField, Min(1), Tooltip("First floor number this floor can appear on.")]
        private int minFloor = 1;

        [SerializeField, Min(0f), Tooltip("How often the floor picker chooses this floor, relative to the others.")]
        private float selectionWeight = 1f;

        [SerializeField] private int schemaVersion = 1;

        [SerializeField, Tooltip("The networked floor controller the host spawns for this floor.")]
        private NetworkFloor floorPrefab;

        [SerializeField, Tooltip("The floor's own settings asset (its numbers per difficulty level).")]
        private ScriptableObject settings;

        [SerializeField, Tooltip("The scene that holds this floor's room, e.g. Assets/.../Trivia.unity. The Floor Tester opens it. " +
            "(Rooms become one prefab per visual tier later, section 4 of docs/PROJECT_CONTEXT.md.)")]
        private string roomScene;

        public string Id => id;
        public string StringTable => stringTable;
        public string NameKey => nameKey;
        public string GoalKey => goalKey;
        public string Genre => genre;
        public int MinPlayers => minPlayers;
        public int MaxPlayers => maxPlayers;
        public int MinFloor => minFloor;
        public float SelectionWeight => selectionWeight;
        public int SchemaVersion => schemaVersion;
        public NetworkFloor FloorPrefab => floorPrefab;
        public ScriptableObject Settings => settings;
        public string RoomScene => roomScene;

        public string DisplayName => UiText.Get(stringTable, nameKey);
        public string Goal => UiText.Get(stringTable, goalKey);
    }
}
