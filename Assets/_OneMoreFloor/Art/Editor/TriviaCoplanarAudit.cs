using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace OneMoreFloor.ArtReview
{
    // Checks imported render triangles, including pairs inside a merged mesh.
    // Same-facing positive-area overlaps can flicker; opposite-facing contact
    // surfaces and shared edges cannot. Smallest tested face: 0.1 cm squared.
    public static class TriviaCoplanarAudit
    {
        class Face { public string name; public Vector3 a,b,c,n; public Vector2[] p; public float min,max,lo,hi; }
        static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        static float Intersection(Vector2[] a,Vector2[] b)
        {
            var poly=a.ToList();float sign=Mathf.Sign(Cross(b[1]-b[0],b[2]-b[0]));
            for(int edge=0;edge<3 && poly.Count>2;edge++)
            {
                Vector2 s=b[edge],v=b[(edge+1)%3]-s;var output=new List<Vector2>();
                for(int i=0;i<poly.Count;i++)
                {
                    Vector2 p=poly[i],q=poly[(i+1)%poly.Count];float dp=sign*Cross(v,p-s),dq=sign*Cross(v,q-s);
                    if(dp>=0) output.Add(p);
                    if((dp<0)!=(dq<0))output.Add(p+(q-p)*(dp/(dp-dq)));
                }
                poly=output;
            }
            float area=0;for(int i=0;i<poly.Count;i++)area+=Cross(poly[i],poly[(i+1)%poly.Count]);
            return Mathf.Abs(area)*.5f;
        }
        public static void Run(GameObject room,string phase)
        {
            var planes=new Dictionary<(int,int,int,int),List<Face>>();int total=0,small=0;
            foreach(var filter in room.GetComponentsInChildren<MeshFilter>())
            {
                if(filter.name.StartsWith("COL_"))continue;
                var verts=filter.sharedMesh.vertices.Select(v=>filter.transform.TransformPoint(v)).ToArray();var tris=filter.sharedMesh.triangles;
                for(int i=0;i<tris.Length;i+=3)
                {
                    total++;Vector3 a=verts[tris[i]],b=verts[tris[i+1]],c=verts[tris[i+2]],n=Vector3.Cross(b-a,c-a);
                    if(n.magnitude<.00002f){small++;continue;}n.Normalize();
                    var key=(Mathf.RoundToInt(n.x*1000),Mathf.RoundToInt(n.y*1000),Mathf.RoundToInt(n.z*1000),Mathf.RoundToInt(Vector3.Dot(n,a)*10000));
                    int axis=Mathf.Abs(n.x)>Mathf.Abs(n.y)?0:1;if(Mathf.Abs(n.z)>Mathf.Abs(n[axis]))axis=2;
                    Vector2 P(Vector3 v)=>axis==0?new Vector2(v.y,v.z):axis==1?new Vector2(v.x,v.z):new Vector2(v.x,v.y);
                    var p=new[]{P(a),P(b),P(c)};var f=new Face{name=filter.name,a=a,b=b,c=c,n=n,p=p,min=p.Min(v=>v.x),max=p.Max(v=>v.x),lo=p.Min(v=>v.y),hi=p.Max(v=>v.y)};
                    if(!planes.TryGetValue(key,out var group))planes[key]=group=new List<Face>();group.Add(f);
                }
            }
            var overlaps=new Dictionary<string,(int count,float area,Vector3 sample)>();
            foreach(var group in planes.Values.Where(g=>g.Count>1))
            {
                group.Sort((a,b)=>a.min.CompareTo(b.min));
                for(int i=0;i<group.Count;i++)for(int j=i+1;j<group.Count && group[j].min<group[i].max-.00001f;j++)
                {
                    var a=group[i];var b=group[j];if(a.lo>=b.hi-.00001f || b.lo>=a.hi-.00001f)continue;
                    if(Mathf.Abs(Vector3.Dot(a.n,b.a-a.a))>.0001f || Mathf.Abs(Vector3.Dot(a.n,b.b-a.a))>.0001f || Mathf.Abs(Vector3.Dot(a.n,b.c-a.a))>.0001f)continue;
                    float area=Intersection(a.p,b.p);if(area<.00001f)continue;
                    string key=string.CompareOrdinal(a.name,b.name)<0?a.name+" / "+b.name:b.name+" / "+a.name;
                    var old=overlaps.TryGetValue(key,out var value)?value:(0,0f,Vector3.zero);
                    overlaps[key]=(old.Item1+1,old.Item2+area,(a.a+a.b+a.c)/3);
                }
            }
            File.WriteAllText(TriviaRoundTwoReview.Evidence+"/"+phase+"-coplanar.json",JsonConvert.SerializeObject(new{
                importedTriangles=total,trianglesBelowAreaThreshold=small,areaThresholdSquareMetres=.00001f,
                scope="All imported render meshes, same-facing coplanar triangles; 0.1 mm plane tolerance. Reports require visibility review for intentional hidden construction overlaps.",
                overlaps=overlaps.OrderByDescending(v=>v.Value.area).Select(v=>new{pair=v.Key,count=v.Value.count,projectedArea=v.Value.area,sample=new[]{v.Value.sample.x,v.Value.sample.y,v.Value.sample.z}})
            },Formatting.Indented));
            Debug.Log("TRIVIA_COPLANAR "+phase+": "+total+" triangles, "+overlaps.Count+" candidate mesh pairs");
        }
    }
}
