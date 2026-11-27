using System;
using System.IO;
using FishNet.Object;
using OneMoreFloor.FloorSDK;
using OneMoreFloor.Floors.Trivia;
using OneMoreFloor.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace OneMoreFloor.EditorTools
{
    /// <summary>
    /// Builds the run's and Trivia's game assets: Trivia's settings and floor definition, the networked
    /// prefabs (run manager, Trivia floor), the run starter, and puts the run starter in Trivia's room scene.
    /// Safe to run again: existing settings keep their tuned numbers.
    /// </summary>
    public static class FloorSetup
    {
        public const string RunBootstrapPrefabPath = "Assets/_OneMoreFloor/Game/Run/RunBootstrap.prefab";
        public const string TriviaRoomScene = "Assets/_OneMoreFloor/Art/Floors/Trivia/Review/TriviaArtReview.unity";

        private const string FloorsDataFolder = "Assets/_OneMoreFloor/Data/Floors";
        private const string TriviaSettingsPath = FloorsDataFolder + "/TriviaSettings.asset";
        private const string TriviaDefinitionPath = FloorsDataFolder + "/Trivia.asset";
        private const string QuestionsPath = "Assets/_OneMoreFloor/Floors/Trivia/Data/TriviaQuestions.json";
        private const string TriviaFloorPrefabPath = "Assets/_OneMoreFloor/Floors/Trivia/TriviaFloor.prefab";
        private const string RunManagerPrefabPath = "Assets/_OneMoreFloor/Game/Run/RunManager.prefab";

        [MenuItem("OneMoreFloor/Setup/Run and Trivia Floor")]
        public static void Run()
        {
            Directory.CreateDirectory(FloorsDataFolder);
            AssetDatabase.Refresh();

            TriviaSettings settings = LoadOrCreate<TriviaSettings>(TriviaSettingsPath);
            TextAsset questions = Load<TextAsset>(QuestionsPath);

            GameObject floorPrefab = SavePrefab(TriviaFloorPrefabPath, "TriviaFloor", true, go =>
            {
                var floor = go.AddComponent<TriviaFloor>();
                Set(floor, "settings", p => p.objectReferenceValue = settings);
                Set(floor, "questionBank", p => p.objectReferenceValue = questions);
            });

            FloorDefinition trivia = LoadOrCreate<FloorDefinition>(TriviaDefinitionPath);
            Set(trivia, "id", p => p.stringValue = "trivia");
            Set(trivia, "stringTable", p => p.stringValue = TriviaText.Table);
            Set(trivia, "nameKey", p => p.stringValue = TriviaText.Name);
            Set(trivia, "goalKey", p => p.stringValue = TriviaText.Goal);
            Set(trivia, "genre", p => p.stringValue = "q_and_a");
            Set(trivia, "minPlayers", p => p.intValue = 1);
            Set(trivia, "maxPlayers", p => p.intValue = 4);
            Set(trivia, "minFloor", p => p.intValue = 1);
            Set(trivia, "floorPrefab", p => p.objectReferenceValue = floorPrefab.GetComponent<TriviaFloor>());
            Set(trivia, "settings", p => p.objectReferenceValue = settings);
            Set(trivia, "roomScene", p => p.stringValue = TriviaRoomScene);

            GameObject runPrefab = SavePrefab(RunManagerPrefabPath, "RunManager", true, go =>
            {
                var run = go.AddComponent<RunManager>();
                Set(run, "rules", p => p.objectReferenceValue = Load<GameRules>("Assets/_OneMoreFloor/Data/GameRules.asset"));
                Set(run, "director", p => p.objectReferenceValue = Load<DirectorSettings>("Assets/_OneMoreFloor/Data/DirectorSettings.asset"));
                Set(run, "floors", p =>
                {
                    p.arraySize = 1;
                    p.GetArrayElementAtIndex(0).objectReferenceValue = trivia;
                });
            });

            SavePrefab(RunBootstrapPrefabPath, "RunBootstrap", false, go =>
                Set(go.AddComponent<RunBootstrap>(), "runManagerPrefab", p => p.objectReferenceValue = runPrefab.GetComponent<RunManager>()));

            AssetDatabase.SaveAssets();
            AddRunStarter(TriviaRoomScene);
            Debug.Log("FLOOR_SETUP_DONE");
        }

        /// <summary>Puts the run starter in a floor's room scene (if it isn't there yet) and saves the scene.</summary>
        public static void AddRunStarter(string scenePath)
        {
            Scene scene = SceneManager.GetActiveScene().path == scenePath ? SceneManager.GetActiveScene() : EditorSceneManager.OpenScene(scenePath);
            if (Object.FindFirstObjectByType<RunBootstrap>() == null)
            {
                PrefabUtility.InstantiatePrefab(Load<GameObject>(RunBootstrapPrefabPath), scene);
                EditorSceneManager.MarkSceneDirty(scene);
            }
            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject SavePrefab(string path, string name, bool networked, Action<GameObject> build)
        {
            var root = new GameObject(name);
            try
            {
                if (networked)
                    root.AddComponent<NetworkObject>();
                build(root);
                return PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static T Load<T>(string path) where T : Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
                throw new FileNotFoundException($"Missing {typeof(T).Name}.", path);
            return asset;
        }

        private static void Set(Object target, string field, Action<SerializedProperty> change)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
                throw new MissingFieldException(target.GetType().Name, field);
            change(property);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
