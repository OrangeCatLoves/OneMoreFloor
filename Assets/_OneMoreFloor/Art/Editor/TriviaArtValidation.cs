// Art pipeline verification only. Gameplay/import tools remain Claude Code's work.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;

namespace OneMoreFloor.ArtReview
{
    public static class TriviaArtValidation
    {
        const string Root = "Assets/_OneMoreFloor/Art/Floors/Trivia";
        [Serializable] class Report
        {
            public string model;
            public float[] sizeMetres;
            public int renderMeshes, triangles, colliders, buttons, lightMarkers, screens;
            public string[] problems;
        }
        [Serializable] class Marker { public string name; public Vector3 position; public Vector3 forward; public Vector3 up; }
        [Serializable] class MarkerReport { public Marker[] markers; }

        public static void Run()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            string modelPath = File.Exists(Root + "/SM_Trivia_Theatre.fbx")
                ? Root + "/SM_Trivia_Theatre.fbx" : Root + "/SM_Trivia_Podium_Pilot.fbx";
            ConfigureMaterials(modelPath);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (!model) throw new Exception("Trivia FBX was not imported: " + modelPath);
            var instance = UnityEngine.Object.Instantiate(model);
            try
            {
                var all = instance.GetComponentsInChildren<Transform>(true);
                var meshes = instance.GetComponentsInChildren<MeshFilter>(true);
                var renderers = instance.GetComponentsInChildren<MeshRenderer>(true)
                    .Where(r => !r.name.StartsWith("COL_")).ToArray();
                Bounds bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                var problems = new List<string>();
                var screens = meshes.Where(m => m.name == "SM_Trivia_QuestionScreen" || m.name.StartsWith("SM_Trivia_PodiumScreen_")).ToArray();
                foreach (var screen in screens)
                {
                    var uv = screen.sharedMesh.uv;
                    if (uv.Length == 0 || uv.Any(v => v.x < -.001f || v.x > 1.001f || v.y < -.001f || v.y > 1.001f))
                        problems.Add(screen.name + ": screen UV is outside 0-1 or absent");
                }
                int buttons = meshes.Count(m => m.name.Contains("_Button_"));
                bool full = modelPath.Contains("Theatre");
                if (buttons != (full ? 16 : 4)) problems.Add("Wrong button count: " + buttons);
                if (!full && (bounds.size.y < 1.7f || bounds.size.y > 1.95f)) problems.Add("Podium scale/up-axis incorrect: " + bounds.size);
                if (full)
                {
                    float expectedDoorHeight = (float)JObject.Parse(File.ReadAllText("docs/sizes.json"))["elevator"]["door"]["height"];
                    foreach (string marker in new[] { "MK_Host", "MK_Exit", "MK_Spawn_01", "MK_Spawn_02", "MK_Spawn_03", "MK_Spawn_04", "MK_Objective_01", "MK_Objective_02", "MK_Objective_03", "MK_Objective_04" })
                        if (!all.Any(t => t.name == marker)) problems.Add("Missing " + marker);
                    foreach (var door in renderers.Where(r => r.name == "SM_Trivia_LiftDoor_L" || r.name == "SM_Trivia_LiftDoor_R"))
                        if (Mathf.Abs(door.bounds.size.y - expectedDoorHeight) > .01f) problems.Add(door.name + ": unexpected height " + door.bounds.size.y);
                    foreach (JObject light in JArray.Parse(File.ReadAllText("art-source/trivia/lighting.json")))
                    {
                        var marker = all.FirstOrDefault(t => t.name == (string)light["name"]);
                        if (!marker || Vector3.Distance(marker.position, Vector(light["position_unity"])) > .01f)
                            problems.Add((string)light["name"] + ": lighting sheet does not match imported coordinates");
                    }
                }
                foreach (var mesh in meshes)
                    if (mesh.sharedMesh == null) problems.Add(mesh.name + ": absent mesh");
                foreach (var collider in meshes.Where(m => m.name.StartsWith("COL_")))
                {
                    var verts = collider.sharedMesh.vertices;
                    var indices = collider.sharedMesh.triangles;
                    double volume = 0;
                    for (int i = 0; i < indices.Length; i += 3)
                        volume += Vector3.Dot(verts[indices[i]], Vector3.Cross(verts[indices[i+1]], verts[indices[i+2]])) / 6.0;
                    if (volume <= 0) problems.Add(collider.name + ": collider winding is inward or has no volume");
                }
                foreach (var renderer in renderers)
                    if (renderer.sharedMaterials.Any(m => !m || m.shader.name != "Universal Render Pipeline/Lit"))
                        problems.Add(renderer.name + ": material not mapped to URP/Lit");
                var report = new Report {
                    model = modelPath, sizeMetres = new[] { bounds.size.x, bounds.size.y, bounds.size.z },
                    renderMeshes = renderers.Length,
                    triangles = meshes.Where(m => !m.name.StartsWith("COL_")).Sum(m => m.sharedMesh.triangles.Length / 3),
                    colliders = meshes.Count(m => m.name.StartsWith("COL_")), buttons = buttons,
                    lightMarkers = all.Count(t => t.name.StartsWith("MK_Light_")), screens = screens.Length,
                    problems = problems.ToArray()
                };
                Directory.CreateDirectory("art-source/trivia");
                File.WriteAllText("art-source/trivia/" + (full ? "unity-validation.json" : "unity-pilot-validation.json"), JsonUtility.ToJson(report, true));
                if (full) File.WriteAllText("art-source/trivia/unity-markers.json", JsonUtility.ToJson(new MarkerReport {
                    markers = all.Where(t => t.name.StartsWith("MK_")).Select(t => new Marker { name = t.name, position = t.position, forward = t.forward, up = t.up }).ToArray()
                }, true));
                if (full) File.WriteAllText("art-source/trivia/unity-material-bindings.json", Newtonsoft.Json.JsonConvert.SerializeObject(
                    renderers.Where(r => r.name == "SM_Trivia_StagePlank" || r.name == "SM_Trivia_BackCurtain" || r.name == "SM_Trivia_Podium_01").Select(r => new {
                        renderer = r.name, materials = r.sharedMaterials.Select(m => new { name = m.name, baseMap = m.GetTexture("_BaseMap")?.name, metallicMap = m.GetTexture("_MetallicGlossMap")?.name, keywords = m.shaderKeywords })
                    }), Newtonsoft.Json.Formatting.Indented));
                Debug.Log("TRIVIA_ART_VALIDATION " + JsonUtility.ToJson(report));
                if (problems.Count > 0) throw new Exception(string.Join("; ", problems));
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }

        public static void Preview()
        {
            using var quality = new TriviaReviewRenderScope();
            Run();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/SM_Trivia_Theatre.fbx");
            var room = UnityEngine.Object.Instantiate(model);
            foreach (var renderer in room.GetComponentsInChildren<MeshRenderer>())
                if (renderer.name.StartsWith("COL_")) renderer.enabled = false;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.18f, .13f, .20f);
            RenderSettings.ambientEquatorColor = new Color(.13f, .09f, .06f);
            RenderSettings.ambientGroundColor = new Color(.05f, .03f, .025f);
            // A diagnostic main light makes base-colour/normal verification
            // independent of the production room's pending light bake.
            var reviewKey = new GameObject("ReviewOnly_SoftKey").AddComponent<Light>();
            reviewKey.type = LightType.Directional;reviewKey.intensity = 1.4f;
            reviewKey.color = new Color(1,.86f,.71f);
            reviewKey.transform.rotation = Quaternion.Euler(45,180,0);
            // This verification light is outside the enclosed room; disabling its
            // shadows lets it reveal the imported surfaces through the ceiling.
            reviewKey.shadows = LightShadows.None;
            var lighting = JArray.Parse(File.ReadAllText("art-source/trivia/lighting.json"));
            foreach (JObject spec in lighting)
            {
                var lamp = new GameObject((string)spec["name"] + "_Review").AddComponent<Light>();
                lamp.transform.position = Vector(spec["position_unity"]);
                var direction = Vector(spec["direction_unity"]);
                if (direction.sqrMagnitude > .01f) lamp.transform.rotation = Quaternion.LookRotation(direction);
                ColorUtility.TryParseHtmlString((string)spec["color"], out var colour);
                lamp.color = colour;
                string type = (string)spec["type"];
                lamp.type = type == "Point" ? LightType.Point : LightType.Spot;
                lamp.spotAngle = type == "Area" ? 130 : type == "Spot" ? (float)spec["cone_degrees"] : 60;
                lamp.innerSpotAngle = lamp.spotAngle * .65f;
                lamp.intensity = (float)spec["watts_blender"] / (type == "Point" ? 80f : 100f);
                lamp.range = type == "Point" ? 6 : 40;
                lamp.shadows = LightShadows.Soft;
                lamp.shadowBias = .02f;
            }
            var volume = new GameObject("Trivia_Review_Grading").AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            var tone = volume.sharedProfile.Add<Tonemapping>();tone.mode.Override(TonemappingMode.Neutral);
            var bloom = volume.sharedProfile.Add<Bloom>();bloom.intensity.Override(.15f);bloom.threshold.Override(1);
            var camera = new GameObject("Trivia_Review_Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";camera.nearClipPlane = .05f;camera.farClipPlane = 150;
            camera.clearFlags = CameraClearFlags.SolidColor;camera.backgroundColor = new Color(.02f,.01f,.025f);
            TriviaReviewRenderScope.ConfigureCamera(camera);
            var cam = (JObject)JObject.Parse(File.ReadAllText("art-source/trivia/review_settings.json"))["cameras"][1];
            var pos = Vector(cam["position"]);var target = Vector(cam["target"]);
            camera.transform.position = new Vector3(-pos.x,pos.z,-pos.y);
            camera.transform.LookAt(new Vector3(-target.x,target.z,-target.y));
            camera.fieldOfView = 2 * Mathf.Atan(36f * 720 / 1280 / (2 * (float)cam["lens"])) * Mathf.Rad2Deg;
            var rt = new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32) { antiAliasing = 4 };rt.Create();
            var resolved = new RenderTexture(1280,720,0,RenderTextureFormat.ARGB32);resolved.Create();
            try
            {
                void Capture(string name)
                {
                    var request = new UniversalRenderPipeline.SingleCameraRequest { destination = rt };
                    RenderPipeline.SubmitRenderRequest(camera, request);
                    RenderPipeline.SubmitRenderRequest(camera, request);
                    Graphics.Blit(rt, resolved);
                    RenderTexture.active = resolved;
                    var image = new Texture2D(1280,720,TextureFormat.RGB24,false);
                    image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
                    File.WriteAllBytes("art-source/renders/trivia/round-01/" + name + ".png",image.EncodeToPNG());
                    UnityEngine.Object.DestroyImmediate(image);
                }
                Capture("unity-import-preview");
                var robotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Starter Assets/Runtime/SpaceRobotKyle/Prefabs/KyleRobot.prefab");
                if (robotPrefab)
                {
                    var robot = UnityEngine.Object.Instantiate(robotPrefab);
                    var renderers = robot.GetComponentsInChildren<Renderer>();
                    Bounds RobotBounds()
                    {
                        Bounds bounds = renderers[0].bounds;
                        foreach (var r in renderers) bounds.Encapsulate(r.bounds);
                        return bounds;
                    }
                    float height = (float)JObject.Parse(File.ReadAllText("docs/sizes.json"))["player"]["height"];
                    robot.transform.localScale *= height / RobotBounds().size.y;
                    var transforms = room.GetComponentsInChildren<Transform>();
                    var atPodium = transforms.First(t => t.name == "MK_Objective_02").position;
                    robot.transform.position = atPodium;
                    robot.transform.position += Vector3.up * (atPodium.y - RobotBounds().min.y);
                    robot.transform.rotation = Quaternion.LookRotation(Vector3.back);
                    camera.transform.position = atPodium + new Vector3(3,2.3f,3.8f);
                    camera.transform.LookAt(atPodium + new Vector3(0,.95f,-.6f));camera.fieldOfView = 40;
                    Capture("unity-scale-podium");
                    var atLift = transforms.First(t => t.name == "MK_Exit").position;
                    robot.transform.position = atLift;
                    robot.transform.position += Vector3.up * (atLift.y - RobotBounds().min.y);
                    robot.transform.rotation = Quaternion.LookRotation(Vector3.left);
                    camera.transform.position = atLift + new Vector3(-4,2.4f,3.8f);
                    camera.transform.LookAt(atLift + Vector3.up * 1.2f);camera.fieldOfView = 43;
                    Capture("unity-scale-lift");
                    // Same geometry, materials, lights and camera: AA alone changes.
                    Directory.CreateDirectory("art-source/renders/trivia/round-02");
                    File.Copy("art-source/renders/trivia/round-01/unity-scale-lift.png", "art-source/renders/trivia/round-02/unity-aa-on.png", true);
                    camera.GetUniversalAdditionalCameraData().antialiasing = AntialiasingMode.None;
                    quality.Pipeline.msaaSampleCount = 1;
                    rt.Release();rt.antiAliasing = 1;rt.Create();
                    Capture("unity-aa-off");
                    File.Copy("art-source/renders/trivia/round-01/unity-aa-off.png", "art-source/renders/trivia/round-02/unity-aa-off.png", true);
                    File.WriteAllText("art-source/trivia/unity-scale-check.json", "{\"stand_in\":\"Existing local KyleRobot prefab\",\"measured_height_m\":" + RobotBounds().size.y.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + ",\"scope\":\"Static art fit check; not a movement playtest\"}");
                }
            }
            finally { RenderTexture.active = null;rt.Release();resolved.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(resolved); }
            Debug.Log("TRIVIA_UNITY_PREVIEW saved; diagnostic directional fill plus approximate realtime lighting, no light bake or reflection probes");
        }

        static Vector3 Vector(JToken value) => new Vector3((float)value[0], (float)value[1], (float)value[2]);

        static void ConfigureMaterials(string modelPath)
        {
            string materialFolder = Root + "/Materials";
            Directory.CreateDirectory(materialFolder);
            AssetDatabase.Refresh();
            var manifest = JObject.Parse(File.ReadAllText("art-source/trivia/material_manifest.json"));
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (!shader) throw new Exception("URP/Lit shader not found");
            var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            foreach (var entry in manifest.Properties())
            {
                var spec = (JObject)entry.Value;
                string path = materialFolder + "/" + entry.Name + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!material) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
                material.shader = shader;
                material.enableInstancing = true;
                // Petals are curved thin surfaces, visible from both sides in
                // Blender. Preserve that behavior in URP instead of culling
                // the underside and creating holes in a side-on bouquet.
                material.SetFloat("_Cull", entry.Name.StartsWith("M_Trivia_Petal") ? 0 : 2);
                var maps = (JObject)spec["maps"];
                foreach (var map in maps.Properties())
                {
                    string fileName = (string)map.Value;
                    if (!fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) fileName += ".png";
                    string texturePath = Root + "/Textures/" + fileName;
                    var textureImporter = (TextureImporter)AssetImporter.GetAtPath(texturePath);
                    if (!textureImporter) throw new Exception("Missing " + texturePath);
                    bool normal = map.Name == "Normal";
                    bool srgb = map.Name == "BaseColor" || map.Name == "Emission";
                    var type = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                    if (textureImporter.textureType != type || textureImporter.sRGBTexture != srgb || !textureImporter.mipmapEnabled || textureImporter.filterMode != FilterMode.Trilinear || textureImporter.anisoLevel != 8)
                    {
                        textureImporter.textureType = type;
                        textureImporter.sRGBTexture = srgb;
                        textureImporter.mipmapEnabled = true;
                        textureImporter.filterMode = FilterMode.Trilinear;
                        textureImporter.anisoLevel = 8;
                        textureImporter.SaveAndReimport();
                    }
                    var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                    string slot = map.Name == "BaseColor" ? "_BaseMap" : map.Name == "Normal" ? "_BumpMap" : map.Name == "MetallicSmoothness" ? "_MetallicGlossMap" : map.Name == "Emission" ? "_EmissionMap" : null;
                    if (slot != null) material.SetTexture(slot, texture);
                }
                material.SetColor("_BaseColor", Color.white);
                material.SetFloat("_Metallic", (float)spec["metallic"]);
                material.SetFloat("_Smoothness", 1);
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
                material.EnableKeyword("_NORMALMAP");
                float emission = (float)spec["emission_strength"];
                if (maps["Emission"] != null)
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", Color.white * emission);
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                }
                else if (emission > 0)
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetTexture("_EmissionMap", material.GetTexture("_BaseMap"));
                    material.SetColor("_EmissionColor", Color.white * emission);
                }
                EditorUtility.SetDirty(material);
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), entry.Name), material);
            }
            AssetDatabase.SaveAssets();
            importer.SaveAndReimport();
        }
    }
}
