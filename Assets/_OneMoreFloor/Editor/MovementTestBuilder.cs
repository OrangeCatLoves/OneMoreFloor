using System;
using System.IO;
using FishNet.Component.Animating;
using FishNet.Component.Spawning;
using FishNet.Component.Transforming;
using FishNet.Managing;
using FishNet.Managing.Object;
using FishNet.Object;
using OneMoreFloor.Game;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace OneMoreFloor.EditorTools
{
    /// <summary>
    /// Builds the player prefab and a plain test arena from code, so they can be rebuilt at any time.
    /// Shared sizes come from docs/sizes.json.
    /// </summary>
    public static class MovementTestBuilder
    {
        private const string Root = "Assets/_OneMoreFloor/";
        private const string PlayerFolder = Root + "Game/Player/";
        private const string GreyboxFolder = Root + "Game/Greybox/";
        private const string SceneFolder = Root + "Game/Scenes/";
        private const string DataFolder = Root + "Data/";
        private const string PlayerPrefabPath = PlayerFolder + "Player.prefab";
        private const string ScenePath = SceneFolder + "MovementTest.unity";
        private const string SizesPath = "docs/sizes.json";

        private const string CharacterFolder = "Assets/Starter Assets/Runtime/ThirdPersonController/Character/";
        private const string CharacterModelPath = CharacterFolder + "Models/Armature.fbx";
        private const string CharacterAnimatorPath = CharacterFolder + "Animations/StarterAssetsThirdPerson.controller";

        private const string PlayerLayerName = "Player";
        private const string DefaultPrefabsPath = "Assets/DefaultPrefabObjects.asset";
        private const string RefreshPrefabsMenu = "Tools/Fish-Networking/Utility/Refresh Default Prefabs";

        [MenuItem("OneMoreFloor/Build Player and Movement Test Scene")]
        public static void Build()
        {
            Sizes sizes = JsonUtility.FromJson<Sizes>(File.ReadAllText(SizesPath));

            EnsureFolder(GreyboxFolder);
            EnsureFolder(SceneFolder);

            int playerLayer = EnsureLayer(PlayerLayerName);

            var movement = LoadOrCreate<MovementSettings>(DataFolder + "MovementSettings.asset");
            var camera = LoadOrCreate<PlayerCameraSettings>(DataFolder + "PlayerCameraSettings.asset");

            NetworkObject playerPrefab = BuildPlayerPrefab(sizes, playerLayer, movement, camera);
            AssetDatabase.SaveAssets();

            if (!EditorApplication.ExecuteMenuItem(RefreshPrefabsMenu))
                throw new InvalidOperationException("Couldn't refresh FishNet's default prefabs.");

            BuildScene(sizes, playerLayer, playerPrefab, camera);

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();

            Debug.Log("[MovementTestBuilder] Built the player prefab and the movement test scene.");
        }

        private static NetworkObject BuildPlayerPrefab(Sizes sizes, int playerLayer, MovementSettings movement, PlayerCameraSettings camera)
        {
            var root = new GameObject("Player");

            var body = root.AddComponent<CharacterController>();
            body.height = sizes.player.height;
            body.radius = sizes.player.radius;
            body.center = new Vector3(0f, sizes.player.height / 2f, 0f);
            body.stepOffset = sizes.stairs.stepHeight + 0.1f;

            root.AddComponent<NetworkObject>();

            var networkTransform = root.AddComponent<NetworkTransform>();
            SetProperty(networkTransform, "_synchronizeScale", property => property.boolValue = false);

            var motor = root.AddComponent<PlayerMotor>();
            motor.Settings = movement;

            var cameraTarget = new GameObject("CameraTarget").transform;
            cameraTarget.SetParent(root.transform, false);
            cameraTarget.localPosition = new Vector3(0f, camera.EyeHeight, 0f);

            var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterModelPath);
            if (modelAsset == null)
                throw new InvalidOperationException($"Character model not found at {CharacterModelPath}. Import the character package first (docs/PROJECT_CONTEXT.md, step 2d).");

            var model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset, root.transform);
            model.name = "Character";

            var animator = model.GetComponent<Animator>();
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CharacterAnimatorPath);
            animator.applyRootMotion = false;

            var playerAnimator = model.AddComponent<PlayerAnimator>();
            var networkAnimator = model.AddComponent<NetworkAnimator>();
            SetProperty(networkAnimator, "_animator", property => property.objectReferenceValue = animator);

            var controller = root.AddComponent<PlayerController>();
            SetProperty(controller, "controls", property => property.objectReferenceValue = AssetDatabase.LoadAssetAtPath<InputActionAsset>(PlayerFolder + "PlayerControls.inputactions"));
            SetProperty(controller, "cameraSettings", property => property.objectReferenceValue = camera);
            SetProperty(controller, "cameraTarget", property => property.objectReferenceValue = cameraTarget);
            SetProperty(controller, "playerAnimator", property => property.objectReferenceValue = playerAnimator);

            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            SetProperty(controller, "bodyRenderers", property =>
            {
                property.arraySize = renderers.Length;
                for (int i = 0; i < renderers.Length; i++)
                    property.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            });

            SetLayerRecursively(root, playerLayer);

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);

            return saved.GetComponent<NetworkObject>();
        }

        private static void BuildScene(Sizes sizes, int playerLayer, NetworkObject playerPrefab, PlayerCameraSettings camera)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            BuildCamera(playerLayer, camera);
            BuildNetwork(playerPrefab);
            BuildArena(sizes);

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void BuildCamera(int playerLayer, PlayerCameraSettings camera)
        {
            Camera.main.gameObject.AddComponent<CinemachineBrain>();

            var virtualCameraObject = new GameObject("PlayerCamera");
            var virtualCamera = virtualCameraObject.AddComponent<CinemachineCamera>();

            LensSettings lens = LensSettings.Default;
            lens.FieldOfView = 60f;
            lens.NearClipPlane = 0.05f;
            lens.FarClipPlane = 1000f;
            virtualCamera.Lens = lens;

            var follow = virtualCameraObject.AddComponent<CinemachineThirdPersonFollow>();
            follow.CameraSide = 1f;
            follow.VerticalArmLength = 0f;
            follow.CameraDistance = camera.StartDistance;
            follow.ShoulderOffset = camera.ShoulderOffset;
            follow.Damping = camera.FollowDamping;
            follow.AvoidObstacles = new CinemachineThirdPersonFollow.ObstacleSettings
            {
                Enabled = true,
                CollisionFilter = ~((1 << playerLayer) | (1 << LayerMask.NameToLayer("Ignore Raycast"))),
                IgnoreTag = string.Empty,
                CameraRadius = 0.2f,
                DampingIntoCollision = 0f,
                DampingFromCollision = 0.5f,
            };
        }

        private static void BuildNetwork(NetworkObject playerPrefab)
        {
            var networkObject = new GameObject("NetworkManager");
            var network = networkObject.AddComponent<NetworkManager>();
            network.SpawnablePrefabs = AssetDatabase.LoadAssetAtPath<DefaultPrefabObjects>(DefaultPrefabsPath);

            var spawner = networkObject.AddComponent<PlayerSpawner>();
            SetProperty(spawner, "_playerPrefab", property => property.objectReferenceValue = playerPrefab);

            var spawnRoot = new GameObject("SpawnPoints").transform;
            var spawnPoints = new Transform[4];
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                spawnPoints[i] = new GameObject($"MK_Spawn_{i + 1:00}").transform;
                spawnPoints[i].SetParent(spawnRoot, false);
                spawnPoints[i].position = new Vector3(-3f + i * 2f, 0.1f, 0f);
            }

            spawner.Spawns = spawnPoints;

            networkObject.AddComponent<LocalSessionStarter>();
        }

        private static void BuildArena(Sizes sizes)
        {
            Material floor = LoadOrCreateMaterial(GreyboxFolder + "M_GreyFloor.mat", new Color(0.55f, 0.55f, 0.55f));
            Material wall = LoadOrCreateMaterial(GreyboxFolder + "M_GreyWall.mat", new Color(0.75f, 0.75f, 0.75f));
            Material accent = LoadOrCreateMaterial(GreyboxFolder + "M_GreyAccent.mat", new Color(0.35f, 0.42f, 0.5f));

            Transform arena = new GameObject("Arena").transform;

            Box(arena, "Ground", new Vector3(0f, -0.5f, 0f), new Vector3(80f, 1f, 80f), floor);
            Box(arena, "WallNorth", new Vector3(0f, 3f, 40f), new Vector3(80f, 6f, 1f), wall);
            Box(arena, "WallSouth", new Vector3(0f, 3f, -40f), new Vector3(80f, 6f, 1f), wall);
            Box(arena, "WallEast", new Vector3(40f, 3f, 0f), new Vector3(1f, 6f, 80f), wall);
            Box(arena, "WallWest", new Vector3(-40f, 3f, 0f), new Vector3(1f, 6f, 80f), wall);

            BuildCorridor(arena, sizes, wall, accent);
            BuildStairs(arena, sizes, accent);
            BuildJumpTests(arena, accent);

            var ramp = Box(arena, "Ramp", new Vector3(-12f, 1f, 10f), new Vector3(4f, 0.4f, 8f), accent);
            ramp.rotation = Quaternion.Euler(-20f, 0f, 0f);
        }

        // A corridor behind a doorway, at the shared sizes, to try the camera in a tight space.
        private static void BuildCorridor(Transform arena, Sizes sizes, Material wall, Material accent)
        {
            const float StartZ = 10f;
            const float Length = 14f;

            float width = sizes.corridor.width;
            float height = sizes.ceiling.height;
            float thickness = sizes.wall.thickness;
            float middleZ = StartZ + Length / 2f;

            Box(arena, "CorridorLeft", new Vector3(-(width + thickness) / 2f, height / 2f, middleZ), new Vector3(thickness, height, Length), wall);
            Box(arena, "CorridorRight", new Vector3((width + thickness) / 2f, height / 2f, middleZ), new Vector3(thickness, height, Length), wall);
            Box(arena, "CorridorCeiling", new Vector3(0f, height + thickness / 2f, middleZ), new Vector3(width + thickness * 2f, thickness, Length), wall);

            float sidePiece = (width - sizes.door.width) / 2f;
            float sideOffset = (sizes.door.width + sidePiece) / 2f;
            Box(arena, "DoorSideLeft", new Vector3(-sideOffset, height / 2f, StartZ), new Vector3(sidePiece, height, thickness), accent);
            Box(arena, "DoorSideRight", new Vector3(sideOffset, height / 2f, StartZ), new Vector3(sidePiece, height, thickness), accent);

            float lintel = height - sizes.door.height;
            Box(arena, "DoorTop", new Vector3(0f, sizes.door.height + lintel / 2f, StartZ), new Vector3(sizes.door.width, lintel, thickness), accent);
        }

        private static void BuildStairs(Transform arena, Sizes sizes, Material accent)
        {
            const int StepCount = 15;
            const float StartX = 12f;
            const float StartZ = 4f;

            for (int i = 0; i < StepCount; i++)
            {
                float height = (i + 1) * sizes.stairs.stepHeight;
                float z = StartZ + (i + 0.5f) * sizes.stairs.stepDepth;
                Box(arena, $"Step{i + 1:00}", new Vector3(StartX, height / 2f, z), new Vector3(sizes.stairs.width, height, sizes.stairs.stepDepth), accent);
            }

            float topHeight = StepCount * sizes.stairs.stepHeight;
            float topZ = StartZ + StepCount * sizes.stairs.stepDepth + 3f;
            Box(arena, "StairsTop", new Vector3(StartX, topHeight / 2f, topZ), new Vector3(sizes.stairs.width * 2f, topHeight, 6f), accent);
        }

        // Ledges at rising heights and platforms with widening gaps, to feel out jump height and distance.
        private static void BuildJumpTests(Transform arena, Material accent)
        {
            float[] ledgeHeights = { 0.5f, 1f, 1.5f, 2f };
            for (int i = 0; i < ledgeHeights.Length; i++)
            {
                float height = ledgeHeights[i];
                Box(arena, $"Ledge{height:0.0}m", new Vector3(-24f + i * 4f, height / 2f, -12f), new Vector3(3f, height, 3f), accent);
            }

            float[] gaps = { 2f, 3f, 4f, 5f };
            float z = -24f;
            Box(arena, "GapStart", new Vector3(10f, 0.5f, z), new Vector3(4f, 1f, 4f), accent);
            foreach (float gap in gaps)
            {
                z += 4f + gap;
                Box(arena, $"GapAfter{gap:0}m", new Vector3(10f, 0.5f, z), new Vector3(4f, 1f, 4f), accent);
            }
        }

        private static Transform Box(Transform parent, string name, Vector3 position, Vector3 size, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.position = position;
            box.transform.localScale = size;
            box.GetComponent<Renderer>().sharedMaterial = material;
            return box.transform;
        }

        private static Material LoadOrCreateMaterial(string path, Color color)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static int EnsureLayer(string layerName)
        {
            int existing = LayerMask.NameToLayer(layerName);
            if (existing >= 0)
                return existing;

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            // Layers 0 to 7 are reserved by Unity.
            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(layer.stringValue))
                    continue;

                layer.stringValue = layerName;
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                return i;
            }

            throw new InvalidOperationException("No free layer for the player.");
        }

        private static void EnsureFolder(string folder)
        {
            Directory.CreateDirectory(folder);
            AssetDatabase.ImportAsset(folder.TrimEnd('/'));
        }

        private static void SetLayerRecursively(GameObject target, int layer)
        {
            target.layer = layer;
            foreach (Transform child in target.transform)
                SetLayerRecursively(child.gameObject, layer);
        }

        private static void SetProperty(UnityEngine.Object target, string propertyName, Action<SerializedProperty> set)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
                throw new InvalidOperationException($"{target.GetType().Name} has no serialized field named {propertyName}.");

            set(property);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // These match docs/sizes.json.
        [Serializable]
        private sealed class Sizes
        {
            public PlayerSize player;
            public DoorSize door;
            public CorridorSize corridor;
            public CeilingSize ceiling;
            public WallSize wall;
            public StairSize stairs;
        }

        [Serializable] private sealed class PlayerSize { public float height; public float radius; }
        [Serializable] private sealed class DoorSize { public float width; public float height; }
        [Serializable] private sealed class CorridorSize { public float width; }
        [Serializable] private sealed class CeilingSize { public float height; }
        [Serializable] private sealed class WallSize { public float thickness; }
        [Serializable] private sealed class StairSize { public float stepHeight; public float stepDepth; public float width; }
    }
}
