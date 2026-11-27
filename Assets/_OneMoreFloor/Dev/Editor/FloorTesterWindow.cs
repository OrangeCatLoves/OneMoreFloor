using System.IO;
using System.Linq;
using OneMoreFloor.FloorSDK;
using OneMoreFloor.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneMoreFloor.Dev
{
    /// <summary>
    /// OneMoreFloor > Floor Tester: pick a floor and a difficulty level, press Play floor, and the game starts
    /// straight in that floor: no lobby, lift ride or floor picker. For developers only; release builds don't
    /// include it.
    /// </summary>
    public sealed class FloorTesterWindow : EditorWindow
    {
        private const string PrefsPrefix = "OneMoreFloor.FloorTester.";
        private const int HighestLevel = 20;

        private FloorDefinition[] _floors = new FloorDefinition[0];
        private int _floorIndex;
        private int _level = 1;
        private bool _randomSeed = true;
        private int _seed = 1234;
        private bool _unlimitedHearts;
        private bool _skipTitleCard = true;
        private Vector2 _scroll;

        [MenuItem("OneMoreFloor/Floor Tester", priority = 0)]
        public static void Open() => GetWindow<FloorTesterWindow>("Floor Tester");

        private void OnEnable()
        {
            _level = EditorPrefs.GetInt(PrefsPrefix + "Level", 1);
            _randomSeed = EditorPrefs.GetBool(PrefsPrefix + "RandomSeed", true);
            _seed = EditorPrefs.GetInt(PrefsPrefix + "Seed", 1234);
            _unlimitedHearts = EditorPrefs.GetBool(PrefsPrefix + "UnlimitedHearts", false);
            _skipTitleCard = EditorPrefs.GetBool(PrefsPrefix + "SkipTitleCard", true);
            FindFloors();
            string lastFloor = EditorPrefs.GetString(PrefsPrefix + "Floor", string.Empty);
            _floorIndex = Mathf.Max(0, System.Array.FindIndex(_floors, f => f.Id == lastFloor));
        }

        private void FindFloors()
        {
            _floors = AssetDatabase.FindAssets("t:FloorDefinition")
                .Select(guid => AssetDatabase.LoadAssetAtPath<FloorDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(floor => floor != null)
                .OrderBy(floor => floor.Id)
                .ToArray();
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.LabelField("Play one floor straight away", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("For testing only. Players never see this: release builds leave it out.", MessageType.None);

            if (_floors.Length == 0)
            {
                EditorGUILayout.HelpBox("No floors found. Each floor needs a FloorDefinition asset (Assets/_OneMoreFloor/Data/Floors).", MessageType.Warning);
                if (GUILayout.Button("Look again"))
                    FindFloors();
                EditorGUILayout.EndScrollView();
                return;
            }

            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                _floorIndex = Mathf.Clamp(_floorIndex, 0, _floors.Length - 1);
                _floorIndex = EditorGUILayout.Popup("Floor", _floorIndex, _floors.Select(f => $"{f.name} ({f.Id})").ToArray());
                FloorDefinition floor = _floors[_floorIndex];

                var levels = floor.Settings as IDescribesLevels;
                _level = EditorGUILayout.IntSlider("Difficulty level", _level, 1, Mathf.Max(HighestLevel, levels?.LevelCount ?? 1));
                if (levels != null)
                {
                    string note = levels.DescribeLevel(_level);
                    if (_level > levels.LevelCount)
                        note += $"  (past level {levels.LevelCount}: plays like level {levels.LevelCount})";
                    EditorGUILayout.LabelField(" ", note, EditorStyles.wordWrappedMiniLabel);
                }
                if (floor.Settings != null && GUILayout.Button("Edit this floor's numbers", GUILayout.Width(200)))
                    Selection.activeObject = floor.Settings;

                EditorGUILayout.Space();
                _randomSeed = EditorGUILayout.Toggle(new GUIContent("New random run each time", "Off: replay exactly the same run (same questions, same order) with the seed below."), _randomSeed);
                using (new EditorGUI.DisabledScope(_randomSeed))
                    _seed = Mathf.Max(1, EditorGUILayout.IntField("Seed", _seed));
                _unlimitedHearts = EditorGUILayout.Toggle(new GUIContent("Unlimited hearts", "Mistakes still show, but the run never ends."), _unlimitedHearts);
                _skipTitleCard = EditorGUILayout.Toggle("Skip the title card", _skipTitleCard);

                EditorGUILayout.Space();
                string scene = floor.RoomScene;
                bool sceneExists = !string.IsNullOrEmpty(scene) && File.Exists(scene);
                if (!sceneExists)
                    EditorGUILayout.HelpBox($"{floor.name} has no room scene yet (Room Scene in its FloorDefinition).", MessageType.Error);
                using (new EditorGUI.DisabledScope(!sceneExists))
                {
                    if (GUILayout.Button("Play floor", GUILayout.Height(36)))
                        Play(floor);
                }
            }

            if (EditorApplication.isPlaying && FloorTest.Current != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox($"Testing {FloorTest.Current.floorId} at level {FloorTest.Current.level}.", MessageType.Info);
                if (GUILayout.Button("Stop"))
                    EditorApplication.ExitPlaymode();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("While playing", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "F5 restart the floor   F6 / F7 level down / up   F8 clear the floor   F9 lose a heart\n" +
                "F1 hides or shows the panel at the top right. Esc frees the mouse to click it.\n" +
                "2 players: choose the TwoPlayers scenario in Unity's Play mode menu (toolbar) first, then Play floor.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView();
        }

        private void Play(FloorDefinition floor)
        {
            EditorPrefs.SetString(PrefsPrefix + "Floor", floor.Id);
            EditorPrefs.SetInt(PrefsPrefix + "Level", _level);
            EditorPrefs.SetBool(PrefsPrefix + "RandomSeed", _randomSeed);
            EditorPrefs.SetInt(PrefsPrefix + "Seed", _seed);
            EditorPrefs.SetBool(PrefsPrefix + "UnlimitedHearts", _unlimitedHearts);
            EditorPrefs.SetBool(PrefsPrefix + "SkipTitleCard", _skipTitleCard);

            if (SceneManager.GetActiveScene().path != floor.RoomScene)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    return;
                EditorSceneManager.OpenScene(floor.RoomScene);
            }

            var request = new FloorTestRequest
            {
                floorId = floor.Id,
                level = _level,
                seed = _randomSeed ? 0 : _seed,
                unlimitedHearts = _unlimitedHearts,
                skipTitleCard = _skipTitleCard,
            };
            FloorTesterPlayMode.Start(request);
        }
    }

    /// <summary>Passes the request to the game for one Play, and forgets it afterwards so a normal Play stays normal.</summary>
    [InitializeOnLoad]
    public static class FloorTesterPlayMode
    {
        static FloorTesterPlayMode()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode)
                    SessionState.EraseString(FloorTestBoot.SessionKey);
            };
        }

        public static void Start(FloorTestRequest request)
        {
            SessionState.SetString(FloorTestBoot.SessionKey, JsonUtility.ToJson(request));
            EditorApplication.EnterPlaymode();
        }
    }
}
