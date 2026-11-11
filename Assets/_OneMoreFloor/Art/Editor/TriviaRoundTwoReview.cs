using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object=UnityEngine.Object;

namespace OneMoreFloor.ArtReview
{
    // Fixed, unretouched Unity evidence for the owner's numbered feedback.
    public static class TriviaRoundTwoReview
    {
        public const string Evidence="art-source/renders/trivia/round-05";
        static readonly (string name,Vector3 eye,Vector3 target,float fov)[] Views={
            ("01-tieback",new Vector3(7.2f,3.4f,.4f),new Vector3(9.5f,3,-1.8f),48),
            ("02-lift-threshold",new Vector3(11,1.3f,4),new Vector3(13.4f,0,2),65),
            ("03-floor-lamps",new Vector3(11,2.3f,4.4f),new Vector3(9,1.05f,2),50),
            ("04-player-facing",new Vector3(7,4,-1.5f),new Vector3(0,1.6f,3),65),
            ("05-player-tv",new Vector3(0,2.3f,1.2f),new Vector3(0,1.2f,5.3f),65),
            ("06-lift-side-steps",new Vector3(12,3.6f,8),new Vector3(9,.7f,3),63),
            ("07-full-screen",new Vector3(0,5.4f,5.4f),new Vector3(0,5.4f,-4.15f),74),
            ("08-rear-function",new Vector3(0,3.4f,16),new Vector3(0,2.5f,24),76),
            ("08-side-function",new Vector3(0,3,14),new Vector3(13,2.5f,15),73),
            ("09-circulation",new Vector3(0,10,5),new Vector3(0,1.8f,17),80),
            ("09-gallery",new Vector3(8,7,18),new Vector3(12.5f,5,13),70),
            ("09-row-clearance",new Vector3(0,3,13),new Vector3(7,1.2f,13),65)
        };
        public static void Before()=>Capture("before");
        public static void After(){Capture("after");TriviaRoundTwoAudit.Run(GameObject.Find("Trivia_Theatre"));}
        static void Capture(string phase)
        {
            EditorSceneManager.OpenScene(TriviaReviewSceneBuilder.ScenePath);
            Directory.CreateDirectory(Evidence);
            var room=GameObject.Find("Trivia_Theatre");
            TriviaCoplanarAudit.Run(room,phase);
            var records=new JArray();
            foreach(var t in room.GetComponentsInChildren<Transform>().OrderBy(t=>t.name))
            {
                var renderer=t.GetComponent<MeshRenderer>();var mesh=t.GetComponent<MeshFilter>();
                var row=new JObject{["name"]=t.name,["position"]=V(t.position),["forward"]=V(t.forward),["up"]=V(t.up)};
                if(renderer){row["bounds_min"]=V(renderer.bounds.min);row["bounds_max"]=V(renderer.bounds.max);}
                if(mesh)row["triangles"]=mesh.sharedMesh.triangles.Length/3;
                records.Add(row);
            }
            File.WriteAllText(Evidence+"/"+phase+"-inventory.json",records.ToString());
            using var quality=new TriviaReviewRenderScope();
            var cam=Camera.main;
            TriviaReviewRenderScope.ConfigureCamera(cam);cam.aspect=16f/9;
            var rt=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32){antiAliasing=4};rt.Create();
            var resolved=new RenderTexture(1280,720,0,RenderTextureFormat.ARGB32);resolved.Create();
            var previous=RenderTexture.active;bool oldAsync=ShaderUtil.allowAsyncCompilation;
            try
            {
                ShaderUtil.allowAsyncCompilation=false;
                foreach(var view in Views)
                {
                    cam.transform.position=view.eye;cam.transform.LookAt(view.target);cam.fieldOfView=view.fov;
                    for(int k=0;k<3;k++)RenderPipeline.SubmitRenderRequest(cam,new UniversalRenderPipeline.SingleCameraRequest{destination=rt});
                    Graphics.Blit(rt,resolved);RenderTexture.active=resolved;
                    var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
                    image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
                    File.WriteAllBytes(Evidence+"/"+phase+"-"+view.name+".png",image.EncodeToPNG());Object.DestroyImmediate(image);
                }
            }
            finally{ShaderUtil.allowAsyncCompilation=oldAsync;RenderTexture.active=previous;rt.Release();resolved.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(resolved);}
            Debug.Log("TRIVIA_ROUND_TWO_CAPTURE "+phase+" "+Views.Length+" fixed Unity views");
        }
        static JArray V(Vector3 v)=>new JArray(v.x,v.y,v.z);
    }
}
