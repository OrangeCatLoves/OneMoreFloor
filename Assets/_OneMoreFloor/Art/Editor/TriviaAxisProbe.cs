using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
namespace OneMoreFloor.ArtReview
{
    public static class TriviaAxisProbe
    {
        public static void Run()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var go=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_OneMoreFloor/Art/Floors/Trivia/SM_Trivia_AxisProbe.fbx"));
            var report=new JArray();
            foreach(var t in go.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("MK_") || t.name.Contains("Spotlight") || t.name=="SM_Trivia_HallFloor"))
            {
                var row=new JObject{["name"]=t.name,["position"]=A(t.position),["forward"]=A(t.forward),["up"]=A(t.up)};
                var m=t.GetComponent<MeshFilter>();
                if(m){row["normal0"]=A(t.TransformDirection(m.sharedMesh.normals[0]));row["bounds_min"]=A(m.GetComponent<Renderer>().bounds.min);row["bounds_max"]=A(m.GetComponent<Renderer>().bounds.max);}
                report.Add(row);
            }
            File.WriteAllText("art-source/trivia/axis-probe.json",report.ToString());Object.DestroyImmediate(go);
        }
        static JArray A(Vector3 v)=>new JArray(v.x,v.y,v.z);
    }
}
