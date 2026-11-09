using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace OneMoreFloor.ArtReview
{
    // Local art review lighting. All generated UV meshes, maps and settings
    // remain in the art-owned Review folder with the unapproved scene.
    public static class TriviaReviewBaking
    {
        public static void Bake(GameObject room)
        {
            string folder=TriviaReviewSceneBuilder.Folder;
            string meshFolder=folder+"/LightmapMeshes";
            Directory.CreateDirectory(meshFolder);AssetDatabase.Refresh();
            var meshes=new Dictionary<Mesh,Mesh>();
            int receivers=0;
            foreach (var renderer in room.GetComponentsInChildren<MeshRenderer>())
            {
                if (!renderer.enabled) continue;
                string n=renderer.name;
                bool moving=n.Contains("_Button_") || n.Contains("Screen") || n.Contains("LiftDoor") || n.Contains("Spotlight") || n.Contains("MarqueeChase");
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,StaticEditorFlags.ReflectionProbeStatic | (moving ? 0 : StaticEditorFlags.ContributeGI));
                bool mapped=!moving && (n.Contains("Wall") || n.Contains("Floor") || n.Contains("Ceiling") || n.Contains("Curtain") || n.Contains("StagePlank") || n.Contains("Seat") || n.StartsWith("SM_Trivia_Podium_"));
                renderer.receiveGI=mapped ? ReceiveGI.Lightmaps : ReceiveGI.LightProbes;
                renderer.lightProbeUsage=LightProbeUsage.BlendProbes;
                if (!mapped) continue;
                var filter=renderer.GetComponent<MeshFilter>();var source=filter.sharedMesh;
                if (!meshes.TryGetValue(source,out var copy))
                {
                    string path=meshFolder+"/"+source.name+".asset";
                    copy=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if (copy) EditorUtility.CopySerialized(source,copy);
                    else {copy=Object.Instantiate(source);AssetDatabase.CreateAsset(copy,path);}
                    copy.name=source.name;
                    Debug.Log("TRIVIA_UNWRAP "+source.name+" "+source.vertexCount);
                    if (!Unwrapping.GenerateSecondaryUVSet(copy)) throw new Exception("Could not unwrap "+source.name);
                    EditorUtility.SetDirty(copy);meshes.Add(source,copy);
                }
                filter.sharedMesh=copy;receivers++;
            }
            string settingsPath=folder+"/TriviaReviewLighting.asset";
            var settings=AssetDatabase.LoadAssetAtPath<LightingSettings>(settingsPath);
            if (!settings) {settings=new LightingSettings();AssetDatabase.CreateAsset(settings,settingsPath);}
            settings.bakedGI=true;settings.realtimeGI=false;
            settings.lightmapper=LightingSettings.Lightmapper.ProgressiveCPU;
            settings.lightmapResolution=10;settings.lightmapMaxSize=1024;
            settings.directSampleCount=32;settings.indirectSampleCount=128;settings.environmentSampleCount=32;
            settings.minBounces=2;settings.maxBounces=3;
            settings.mixedBakeMode=MixedLightingMode.IndirectOnly;
            settings.directionalityMode=LightmapsMode.CombinedDirectional;
            settings.ao=false;settings.indirectScale=1;
            Lightmapping.lightingSettings=settings;EditorUtility.SetDirty(settings);
            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.name.StartsWith("MK_Light_Area"))
                {
                    var spec=JArray.Parse(File.ReadAllText("art-source/trivia/lighting.json")).First(s => (string)s["name"]+"_Review"==light.name);
                    // Rectangular baked lights provide soft diffuse bounce. A
                    // restrained real-time spot retains view-dependent metal and
                    // varnish highlights, which a diffuse lightmap cannot store.
                    var highlight=new GameObject(light.name+"_Highlight").AddComponent<Light>();
                    EditorUtility.CopySerialized(light,highlight);
                    highlight.transform.SetParent(light.transform.parent);
                    highlight.transform.SetPositionAndRotation(light.transform.position,light.transform.rotation);
                    highlight.intensity=light.intensity*.65f;
                    highlight.lightmapBakeType=LightmapBakeType.Realtime;
                    light.type=LightType.Rectangle;
                    // Blender watt conversion was tuned for real-time spot stand-ins.
                    // Actual baked area emitters need a separate exposure balance.
                    light.intensity*=0.12f;
                    float size=(float)spec["size_m"];light.areaSize=new Vector2(size,size);
                    light.lightmapBakeType=LightmapBakeType.Baked;
                }
                else light.lightmapBakeType=light.name.StartsWith("MK_Light_Spot") ? LightmapBakeType.Realtime : LightmapBakeType.Baked;
            }
            var group=new GameObject("Trivia_BakedLightProbes").AddComponent<LightProbeGroup>();
            var positions=new List<Vector3>();
            foreach (float x in new[]{-9f,-5f,0,5,9})
                foreach (float z in new[]{-3f,0,3,6})
                    foreach (float y in new[]{1f,2.4f,4.5f}) positions.Add(new Vector3(x,y,z));
            foreach (float x in new[]{-10f,-5f,0,5,10})
                foreach (float z in new[]{9f,15,21})
                    foreach (float y in new[]{2.4f,4.5f,7}) positions.Add(new Vector3(x,y,z));
            foreach (float x in new[]{13.8f,16.3f})
                foreach (float z in new[]{.5f,3.5f})
                    foreach (float y in new[]{1f,2.5f}) positions.Add(new Vector3(x,y,z));
            group.probePositions=positions.ToArray();
            EditorSceneManager.SaveScene(room.scene);AssetDatabase.SaveAssets();
            var start=DateTime.UtcNow;
            Debug.Log("TRIVIA_BAKE_BEGIN receivers="+receivers+" probes="+positions.Count);
            if (!Lightmapping.Bake()) throw new Exception("Trivia review lighting bake did not complete");
            var maps=LightmapSettings.lightmaps;
            int probeCount=LightmapSettings.lightProbes ? LightmapSettings.lightProbes.bakedProbes.Length : 0;
            File.WriteAllText("art-source/trivia/baked-lighting-validation.json",new JObject {
                ["lightmap_count"]=maps.Length,["lightmap_receivers"]=receivers,["baked_probes"]=probeCount,
                ["unique_uv_meshes"]=meshes.Count,["resolution_texels_per_metre"]=10,
                ["elapsed_seconds"]=(DateTime.UtcNow-start).TotalSeconds,
                ["lightmap_sizes"]=new JArray(maps.Select(m => m.lightmapColor ? m.lightmapColor.width+"x"+m.lightmapColor.height : "missing"))
            }.ToString());
            if (maps.Length==0 || probeCount==0 || maps.Any(m=>!m.lightmapColor)) throw new Exception("Trivia bake produced incomplete maps/probes");
            Debug.Log("TRIVIA_BAKE_COMPLETE maps="+maps.Length+" probes="+probeCount);
        }
    }
}
