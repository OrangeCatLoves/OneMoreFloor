using System;
using System.IO;
using System.Linq;
using FishNet.Component.Spawning;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace OneMoreFloor.ArtReview
{
    public static class TriviaReviewSceneBuilder
    {
        public const string Folder = "Assets/_OneMoreFloor/Art/Floors/Trivia/Review";
        public const string ScenePath = Folder + "/TriviaArtReview.unity";
        // Camera exposure in stops. Round 2 used 0.5. Round 3 first used 2.3 (0.36 on screen); the owner
        // found that a little too bright, so the target is now 0.29-0.34, checked by the room checks.
        public const float ReviewExposure = 2.0f;

        [MenuItem("OneMoreFloor/Art/Open Trivia Review")]
        public static void OpenReview()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath)) EditorSceneManager.OpenScene(ScenePath);
            else Build();
        }

        public static void BuildAndPreview()
        {
            Build();
            TriviaArtValidation.Preview();
            EditorSceneManager.OpenScene(ScenePath);
        }

        public static void BuildAndCapture()
        {
            Build();
            CaptureReview();
        }

        public static void CaptureReview()
        {
            // Capture the actual review lighting, without the separate import
            // validator's diagnostic directional light.
            EditorSceneManager.OpenScene(ScenePath);
            ValidateSavedReflections();
            CaptureCurrentReview("unity-reference-view-");
        }

        public static void CaptureLightingStudy()
        {
            EditorSceneManager.OpenScene(ScenePath);
            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) light.shadows=LightShadows.None;
            CaptureCurrentReview("unity-no-shadow-study-view-");
            EditorSceneManager.OpenScene(ScenePath);
            foreach (var probe in Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None)) probe.intensity=0;
            RenderSettings.reflectionIntensity=0;
            CaptureCurrentReview("unity-no-reflection-study-view-");
        }

        static void CaptureCurrentReview(string prefix)
        {
            using var quality = new TriviaReviewRenderScope();
            var camera=Camera.main;
            var position=camera.transform.position;var rotation=camera.transform.rotation;float fov=camera.fieldOfView;float aspect=camera.aspect;
            bool asyncCompilation=ShaderUtil.allowAsyncCompilation;
            var previous=RenderTexture.active;
            var render=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32) {antiAliasing=4};render.Create();
            var resolved=new RenderTexture(1280,720,0,RenderTextureFormat.ARGB32);resolved.Create();
            Directory.CreateDirectory("art-source/renders/trivia/round-04");
            try
            {
                ShaderUtil.allowAsyncCompilation=false;
                camera.aspect=1280f/720;
                int view=0;
                foreach (var spec in JObject.Parse(File.ReadAllText("art-source/trivia/review_settings.json"))["cameras"])
                {
                    Vector3 p=V(spec["position"]),t=V(spec["target"]);
                    camera.transform.position=new Vector3(-p.x,p.z,-p.y);
                    camera.transform.LookAt(new Vector3(-t.x,t.z,-t.y));
                    camera.fieldOfView=2*Mathf.Atan(36f*720/1280/(2*(float)spec["lens"]))*Mathf.Rad2Deg;
                    for (int i=0;i<3;i++) RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest {destination=render});
                    Graphics.Blit(render,resolved);RenderTexture.active=resolved;
                    var pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);
                    pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();
                    File.WriteAllBytes("art-source/renders/trivia/round-04/"+prefix+(++view)+".png",pixels.EncodeToPNG());
                    Object.DestroyImmediate(pixels);
                }
            }
            finally
            {
                ShaderUtil.allowAsyncCompilation=asyncCompilation;RenderTexture.active=previous;
                camera.transform.SetPositionAndRotation(position,rotation);camera.fieldOfView=fov;camera.aspect=aspect;
                render.Release();resolved.Release();Object.DestroyImmediate(render);Object.DestroyImmediate(resolved);
            }
        }

        [MenuItem("OneMoreFloor/Art/Build and Open Trivia Review")]
        public static void Build()
        {
            TriviaArtValidation.Run();
            Directory.CreateDirectory(Folder);
            AssetDatabase.Refresh();
            // Reuse the tested local-host player/camera setup. Save a separate
            // art-owned scene before changing any scene object.
            var scene = EditorSceneManager.OpenScene("Assets/_OneMoreFloor/Game/Scenes/MovementTest.unity");
            EditorSceneManager.SaveScene(scene, ScenePath);
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == "Arena" || root.GetComponent<Light>()) Object.DestroyImmediate(root);

            var room = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_OneMoreFloor/Art/Floors/Trivia/SM_Trivia_Theatre.fbx"));
            room.name = "Trivia_Theatre";
            PrefabUtility.UnpackPrefabInstance(room,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            var parts = room.GetComponentsInChildren<Transform>();
            // Invisible blockers that only keep players out of places (curtains, pillars, stage-edge trims)
            // go on the PlayerBlocker layer, which the camera ignores, so it doesn't snap on them.
            int blockerLayer = LayerMask.NameToLayer("PlayerBlocker");
            if (blockerLayer < 0) throw new Exception("Missing physics layer PlayerBlocker (ProjectSettings/TagManager.asset)");
            foreach (var mesh in room.GetComponentsInChildren<MeshFilter>())
            {
                if (!mesh.name.StartsWith("COL_")) continue;
                mesh.GetComponent<Renderer>().enabled = false;
                mesh.gameObject.AddComponent<MeshCollider>().sharedMesh = mesh.sharedMesh;
                if (mesh.name.StartsWith("COL_Trivia_DecorProxy") || mesh.name.StartsWith("COL_Trivia_ApronEdgeGuard") || mesh.name.StartsWith("COL_Trivia_RearPanelGuard")
                    || mesh.name.StartsWith("COL_Trivia_LiftFrameGuard"))
                    mesh.gameObject.layer = blockerLayer;
            }
            // Open display pose for this walk-through; the source FBX remains
            // closed and there is no door gameplay or network behavior here.
            // The leaves slide apart, each away from the other, behind the wall into the cab.
            var leaves = new[] { "L", "R" }.ToDictionary(side => side, side => parts.First(t => t.name == "SM_Trivia_LiftDoor_" + side));
            Vector3 apart = leaves["R"].position - leaves["L"].position;
            apart.y = 0;
            apart.Normalize();
            float slide = (float)JObject.Parse(File.ReadAllText("docs/sizes.json"))["elevator"]["door"]["width"] / 2;
            foreach (string side in new[] { "L", "R" })
            {
                var collision = parts.First(t => t.name == "COL_Trivia_LiftDoor_" + side);
                collision.SetParent(leaves[side], true);
                leaves[side].position += (side == "L" ? -apart : apart) * slide;
            }
            // Players start in the cab facing the open doors (MK_Exit).
            var spawns = Enumerable.Range(1,4).Select(i => parts.First(t => t.name == "MK_Spawn_" + i.ToString("00"))).ToArray();
            Vector3 exit = parts.First(t => t.name == "MK_Exit").position;
            Vector3 centre = spawns.Aggregate(Vector3.zero, (sum, s) => sum + s.position) / spawns.Length;
            Vector3 facing = exit - centre;
            facing.y = 0;
            foreach (var spawn in spawns)
            {
                spawn.position += Vector3.up * .08f;
                spawn.rotation = Quaternion.LookRotation(facing.normalized);
            }
            // Baked probes only include ReflectionProbeStatic renderers.
            // These flags apply to the local art review copy, including its
            // open-door display pose, not to the source model/gameplay prefab.
            foreach (var renderer in room.GetComponentsInChildren<MeshRenderer>())
                if (renderer.enabled) GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,StaticEditorFlags.ReflectionProbeStatic);
            Object.FindFirstObjectByType<PlayerSpawner>().Spawns = spawns;
            var oldSpawnRoot = scene.GetRootGameObjects().FirstOrDefault(o => o.name == "SpawnPoints");
            if (oldSpawnRoot) Object.DestroyImmediate(oldSpawnRoot);
            var camera = Camera.main;
            TriviaReviewRenderScope.ConfigureCamera(camera);
            camera.nearClipPlane = .05f;camera.farClipPlane = 100;
            camera.clearFlags = CameraClearFlags.SolidColor;camera.backgroundColor = new Color(.035f,.018f,.03f);
            new GameObject("TriviaReviewQuality").AddComponent<TriviaReviewQuality>();
            // The game's run starter (made by OneMoreFloor > Setup > Run and Trivia Floor), so pressing Play
            // here plays the Trivia floor and the Floor Tester can open this scene.
            var runStarter = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_OneMoreFloor/Game/Run/RunBootstrap.prefab");
            if (runStarter != null) PrefabUtility.InstantiatePrefab(runStarter, scene);
            AddScreenMarkers(room);
            AddLighting();
            TriviaReviewBaking.Bake(room);
            BakeReviewReflections();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("TRIVIA_REVIEW_SCENE_READY " + ScenePath);
        }

        // The game shows the questions on the room's screens. Their meshes can't be read while playing (the model
        // isn't Read/Write), so each screen gets a marker, worked out here from its mesh: MK_Screen_<name> sits at the
        // screen's centre, its forward (+Z) points out of the screen towards the viewers, and its scale is the
        // screen's width (x) and height (y) in metres.
        public static void AddScreenMarkers(GameObject room)
        {
            foreach (var old in room.GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("MK_Screen_")).ToArray())
                Object.DestroyImmediate(old.gameObject);
            foreach (var filter in room.GetComponentsInChildren<MeshFilter>())
            {
                string marker = filter.name == "SM_Trivia_QuestionScreen" ? "MK_Screen_Question"
                    : filter.name == "SM_Trivia_PlayerScreen" ? "MK_Screen_Players"
                    : filter.name.StartsWith("SM_Trivia_PodiumScreen_") ? "MK_Screen_Podium_" + filter.name.Substring("SM_Trivia_PodiumScreen_".Length)
                    : null;
                if (marker == null) continue;
                Mesh mesh = filter.sharedMesh;
                Transform t = filter.transform;
                Vector3 facing = t.TransformDirection(mesh.normals[0]).normalized;
                Quaternion rotation = Quaternion.LookRotation(facing, Vector3.up);
                Vector3 right = rotation * Vector3.right, up = rotation * Vector3.up;
                var points = mesh.vertices.Select(v => t.TransformPoint(v)).ToArray();
                float width = points.Max(p => Vector3.Dot(p, right)) - points.Min(p => Vector3.Dot(p, right));
                float height = points.Max(p => Vector3.Dot(p, up)) - points.Min(p => Vector3.Dot(p, up));
                var anchor = new GameObject(marker).transform;
                anchor.SetParent(room.transform, false);
                anchor.SetPositionAndRotation(filter.GetComponent<Renderer>().bounds.center, rotation);
                anchor.localScale = new Vector3(width, height, 1f);
            }
        }

        static void AddLighting()
        {
            RenderSettings.skybox = null;
            // A lively, well-lit studio (owner, round 3): about twice round 2's warm fill.
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.42f,.33f,.25f);
            RenderSettings.ambientEquatorColor = new Color(.34f,.22f,.15f);
            RenderSettings.ambientGroundColor = new Color(.14f,.09f,.06f);
            var root = new GameObject("Trivia_ReviewLighting").transform;
            foreach (JObject spec in JArray.Parse(File.ReadAllText("art-source/trivia/lighting.json")))
            {
                var light = new GameObject((string)spec["name"] + "_Review").AddComponent<Light>();
                light.transform.SetParent(root);
                light.transform.position = V(spec["position_unity"]);
                light.transform.rotation = Quaternion.LookRotation(V(spec["direction_unity"]));
                ColorUtility.TryParseHtmlString((string)spec["color"], out var color);light.color = color;
                string type = (string)spec["type"];
                light.type = type == "Point" ? LightType.Point : LightType.Spot;
                light.spotAngle = type == "Area" ? 135 : type == "Spot" ? (float)spec["cone_degrees"] : 60;
                light.innerSpotAngle = light.spotAngle * .6f;
                light.intensity = (float)spec["watts_blender"] / (type == "Point" ? 35f : type == "Area" ? 18f : 24f);
                // Most practicals light their surroundings; chandeliers carry a longer range in the sheet.
                light.range = spec["range_m"] != null ? (float)spec["range_m"] : type == "Point" ? 6 : 35;
                string purpose = (string)spec["purpose"];
                // Limit realtime shadows to four sources for this review. The
                // production lighting bake and performance pass remain separate.
                bool shadow = purpose.StartsWith("Broad warm stage key") || purpose.StartsWith("Audience fill") ||
                    (string)spec["name"] == "MK_Light_Spot_06" || (string)spec["name"] == "MK_Light_Spot_09";
                light.shadows = shadow ? LightShadows.Soft : LightShadows.None;
                light.shadowBias = .025f;light.shadowNormalBias = .15f;
            }
            string profilePath = Folder + "/TriviaReviewGrading.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (!profile)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,profilePath);
            }
            foreach (var component in profile.components.ToArray()) Object.DestroyImmediate(component,true);
            profile.components.Clear();
            var tone = profile.Add<Tonemapping>();tone.mode.Override(TonemappingMode.ACES);
            var bloom = profile.Add<Bloom>();bloom.intensity.Override(.16f);bloom.threshold.Override(1.2f);
            var grading = profile.Add<ColorAdjustments>();grading.postExposure.Override(ReviewExposure);grading.contrast.Override(5);grading.saturation.Override(-5);
            foreach (var component in profile.components) AssetDatabase.AddObjectToAsset(component,profile);
            EditorUtility.SetDirty(profile);
            var volume = new GameObject("Trivia_ReviewGrading").AddComponent<Volume>();volume.isGlobal = true;volume.sharedProfile = profile;
        }

        public static void RebakeReflectionsAndCapture()
        {
            EditorSceneManager.OpenScene(ScenePath);
            foreach (var probe in Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None))
                if (probe.name.StartsWith("Trivia_")) Object.DestroyImmediate(probe.gameObject);
            BakeReviewReflections();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
            CaptureReview();
        }

        static void BakeReviewReflections()
        {
            using var quality = new TriviaReviewRenderScope();
            // Bake first-bounce reflection maps without feeding the previously
            // assigned cubemap (or the first new probe) into the next capture.
            RenderSettings.customReflectionTexture=null;
            RenderSettings.defaultReflectionMode=DefaultReflectionMode.Skybox;
            RenderSettings.reflectionIntensity=0;
            try
            {
                BakeProbe("Stage",new Vector3(0,3.2f,0),new Vector3(24,10,14));
                BakeProbe("Hall",new Vector3(0,4,15),new Vector3(29,11,24));
            }
            finally
            {
                RenderSettings.reflectionIntensity=1;
                foreach (var probe in Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None)) probe.enabled=true;
            }
        }

        static void BakeProbe(string name, Vector3 position, Vector3 size)
        {
            var probe = new GameObject("Trivia_" + name + "_Reflections").AddComponent<ReflectionProbe>();
            probe.transform.position = position;probe.size = size;probe.resolution = 128;
            probe.boxProjection = true;probe.blendDistance = 2;probe.mode = ReflectionProbeMode.Baked;
            probe.clearFlags = ReflectionProbeClearFlags.SolidColor;probe.backgroundColor = new Color(.04f,.02f,.025f);
            string path = Folder + "/T_Trivia_" + name + "_Reflections.exr";
            bool asyncCompilation = ShaderUtil.allowAsyncCompilation;
            try
            {
                ShaderUtil.allowAsyncCompilation = false;
                if (!Lightmapping.BakeReflectionProbe(probe,path)) throw new Exception("Could not bake " + name + " reflection probe");
            }
            finally {ShaderUtil.allowAsyncCompilation = asyncCompilation;}
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.sRGBTexture=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            // A direct bakedTexture assignment is not serialized without a
            // LightingDataAsset bake. Custom mode retains this independently
            // baked cubemap when the local review scene is reopened.
            probe.mode = ReflectionProbeMode.Custom;
            probe.customBakedTexture = AssetDatabase.LoadAssetAtPath<Cubemap>(path);
            probe.enabled=false;
            if (!probe.customBakedTexture) throw new Exception("Missing baked cubemap " + path);
            if (name == "Stage")
            {
                RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
                RenderSettings.customReflectionTexture = probe.customBakedTexture;
            }
        }

        static void ValidateSavedReflections()
        {
            var probes=Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None);
            bool valid=probes.Length==2 && probes.All(p => p.mode==ReflectionProbeMode.Custom && p.customBakedTexture) && RenderSettings.customReflectionTexture;
            File.WriteAllText("art-source/trivia/review-lighting-validation.json",new JObject {
                ["scene_reopened"]=true,["reflection_probe_count"]=probes.Length,
                ["persistent_cubemap_assignments"]=valid,
                ["indoor_fallback"]=RenderSettings.customReflectionTexture ? RenderSettings.customReflectionTexture.name : "missing"
            }.ToString());
            if (!valid) throw new Exception("Saved art review scene lost its baked reflection cubemaps");
        }

        static Vector3 V(JToken t) => new Vector3((float)t[0],(float)t[1],(float)t[2]);
    }
}
