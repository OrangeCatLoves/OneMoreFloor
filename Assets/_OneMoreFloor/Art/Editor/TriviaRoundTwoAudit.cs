using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace OneMoreFloor.ArtReview
{
    public static class TriviaRoundTwoAudit
    {
        public static void Run(GameObject room)
        {
            var all=room.GetComponentsInChildren<Transform>();
            var problems=new List<string>();var checks=new JArray();
            Transform Find(string n)=>all.First(t=>t.name==n);
            var lighting=JArray.Parse(File.ReadAllText("art-source/trivia/lighting.json"));
            foreach(var spec in lighting.Where(s=>s["fixture_axis"]!=null))
            {
                var axis=Find((string)spec["fixture_axis"]);var d=V(spec["direction_unity"]);
                float error=Vector3.Angle(-axis.up,d);float distance=Vector3.Distance(axis.position,V(spec["position_unity"]));
                string number=axis.name.Split('_')[3];var lens=Find("SM_Trivia_Spotlight_"+number+"_Lens");
                float lensError=Vector3.Angle(lens.TransformDirection(lens.GetComponent<MeshFilter>().sharedMesh.normals[0]),d);
                float lensPositionError=Vector3.Distance(lens.position,axis.position-d*.022f);
                checks.Add(new JObject{["fixture"]=axis.name,["angle_error_degrees"]=error,["light_to_lens_m"]=distance,["actual_lens_normal_error_degrees"]=lensError,["lens_position_error_m"]=lensPositionError,["target_unity"]=spec["target_unity"].DeepClone()});
                if(error>.05f || distance>.001f || lensError>.05f || lensPositionError>.001f)problems.Add(axis.name+": optical axis/light mismatch");
            }
            foreach(var t in all.Where(t=>t.name.StartsWith("MK_Objective_")))
                if(Vector3.Angle(t.forward,Vector3.forward)>.05f || Mathf.Abs(t.position.z-1.45f)>.001f)problems.Add(t.name+": standing pose is wrong");
            float Aspect(string name)
            {
                var mesh=Find(name).GetComponent<MeshFilter>().sharedMesh;
                // Measure planar UV basis in world space, independent of tilt.
                var verts=mesh.vertices;var uv=mesh.uv;int a=Array.FindIndex(uv,v=>v.x<.01f && v.y<.5f);
                float width=Find(name).GetComponent<Renderer>().bounds.size.x;
                float area=0;var ids=mesh.triangles;
                for(int i=0;i<ids.Length;i+=3)area+=Vector3.Cross(verts[ids[i+1]]-verts[ids[i]],verts[ids[i+2]]-verts[ids[i]]).magnitude*.5f;
                // Rounded main panel has corners clipped; use maximum planar height.
                Vector3 normal=Vector3.Cross(verts[ids[1]]-verts[ids[0]],verts[ids[2]]-verts[ids[0]]).normalized;
                Vector3 horizontal=Find(name).InverseTransformDirection(Vector3.right);
                Vector3 vertical=Vector3.Cross(normal,horizontal).normalized;
                float height=verts.Max(v=>Vector3.Dot(v,vertical))-verts.Min(v=>Vector3.Dot(v,vertical));
                return width/height;
            }
            float main=Aspect("SM_Trivia_QuestionScreen"),tv=Aspect("SM_Trivia_PlayerScreen");
            if(Mathf.Abs(main-tv)>.001f)problems.Add("Shared display aspect mismatch");
            var display=Find("SM_Trivia_PlayerScreen");var surface=display.GetComponent<MeshFilter>().sharedMesh;
            var targets=surface.vertices.Select(v=>display.TransformPoint(v)).ToArray();var middle=targets.Aggregate(Vector3.zero,(a,b)=>a+b)/targets.Length;
            var normal=display.TransformDirection(surface.normals[0]);
            if(Vector3.Dot(normal,new Vector3(0,Mathf.Sin(25*Mathf.Deg2Rad),-Mathf.Cos(25*Mathf.Deg2Rad)))<.999f)problems.Add("Player TV does not tilt up toward players");
            var uvs=surface.uv;
            var upper=targets.Where((p,i)=>uvs[i].y>.99f).Aggregate(Vector3.zero,(a,b)=>a+b)/uvs.Count(v=>v.y>.99f);
            var lower=targets.Where((p,i)=>uvs[i].y<.01f).Aggregate(Vector3.zero,(a,b)=>a+b)/uvs.Count(v=>v.y<.01f);
            if((upper-lower).y<=0)problems.Add("Player TV UV up is inverted");
            var visibility=new JArray();
            float eyeHeight=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_OneMoreFloor/Game/Player/Player.prefab").GetComponentsInChildren<Transform>().First(t=>t.name=="CameraTarget").localPosition.y;
            foreach(var objective in all.Where(t=>t.name.StartsWith("MK_Objective_")))
            {
                Vector3 eye=objective.position+Vector3.up*eyeHeight;
                foreach(var point in targets.Concat(new[]{middle}))
                {
                    string blocker=FirstBlocker(room,eye,point,display);
                    visibility.Add(new JObject{["player"]=objective.name,["target"]=A(point),["blocker"]=blocker});
                    if(blocker!=null)problems.Add(objective.name+": shared TV hidden by "+blocker);
                }
            }
            int triangles=room.GetComponentsInChildren<MeshFilter>().Where(m=>!m.name.StartsWith("COL_")).Sum(m=>m.sharedMesh.triangles.Length/3);
            if(triangles>1549521)problems.Add("Triangle ceiling exceeded");
            var coplanar=JObject.Parse(File.ReadAllText(TriviaRoundTwoReview.Evidence+"/after-coplanar.json"));
            if(coplanar["overlaps"].Any(p=>((string)p["pair"]).Contains("HallFloor / SM_Trivia_LiftFloor")))problems.Add("Lift floor still overlaps");
            File.WriteAllText(TriviaRoundTwoReview.Evidence+"/unity-audit.json",new JObject{
                ["problems"]=new JArray(problems),["fixture_alignment"]=checks,["main_screen_aspect"]=main,["player_screen_aspect"]=tv,
                ["triangles"]=triangles,["triangle_ceiling"]=1549521,["host_position"]=A(Find("MK_Host").position),
                ["tv_sightlines"]=visibility,
                ["directional_fixture_scope"]="All 12 physical pan/tilt lamps. Point sconces are diffuse emitters; area sources are review fill without modeled lenses."
            }.ToString());
            if(problems.Count>0)throw new Exception(string.Join("; ",problems));
            Debug.Log("TRIVIA_ROUND_TWO_AUDIT passed");
        }
        static Vector3 V(JToken v)=>new Vector3((float)v[0],(float)v[1],(float)v[2]);
        static JArray A(Vector3 v)=>new JArray(v.x,v.y,v.z);
        static string FirstBlocker(GameObject room,Vector3 start,Vector3 end,Transform display)
        {
            var ray=new Ray(start,(end-start).normalized);float length=(end-start).magnitude;string hit=null;
            foreach(var f in room.GetComponentsInChildren<MeshFilter>())
            {
                if(f.name.StartsWith("COL_") || f.transform==display)continue;
                if(!f.GetComponent<Renderer>().bounds.IntersectRay(ray,out float near) || near>length)continue;
                var local=new Ray(f.transform.InverseTransformPoint(start),f.transform.InverseTransformDirection(ray.direction));
                var v=f.sharedMesh.vertices;var t=f.sharedMesh.triangles;
                for(int i=0;i<t.Length;i+=3)
                {
                    Vector3 e1=v[t[i+1]]-v[t[i]],e2=v[t[i+2]]-v[t[i]],p=Vector3.Cross(local.direction,e2);float determinant=Vector3.Dot(e1,p);
                    if(Mathf.Abs(determinant)<.000001f)continue;
                    Vector3 s=local.origin-v[t[i]];float u=Vector3.Dot(s,p)/determinant;if(u<0 || u>1)continue;
                    Vector3 q=Vector3.Cross(s,e1);float w=Vector3.Dot(local.direction,q)/determinant;if(w<0 || u+w>1)continue;
                    float distance=Vector3.Dot(e2,q)/determinant;
                    if(distance>.01f && distance<length-.025f){length=distance;hit=f.name;}
                }
            }
            return hit;
        }
    }
}
