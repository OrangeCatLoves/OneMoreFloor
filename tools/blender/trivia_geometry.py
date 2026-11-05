"""Reusable meter-scale mesh primitives, clean transforms and UVs."""
import bpy, math
from mathutils import Vector

M={}
SPHERES={}
CYLINDERS={}
def mat(obj, material):
    obj.data.materials.append(M[material] if isinstance(material,str) else material)
    return obj

def mesh(name,verts,faces,material,uv=False):
    data=bpy.data.meshes.new(name); data.from_pydata(verts,[],faces); data.update()
    obj=bpy.data.objects.new(name,data); bpy.context.collection.objects.link(obj)
    if material: mat(obj,material)
    if uv:
        layer=data.uv_layers.new(name='UVMap')
        for p in data.polygons:
            normal=p.normal; axis=max(range(3),key=lambda i:abs(normal[i]))
            axes=[i for i in range(3) if i!=axis]
            for li in p.loop_indices:
                v=data.vertices[data.loops[li].vertex_index].co
                layer.data[li].uv=(v[axes[0]],v[axes[1]])
    return obj

def apply(obj):
    if all(abs(a)<1e-8 for a in obj.rotation_euler) and all(abs(a-1)<1e-8 for a in obj.scale):return
    obj.data=obj.data.copy()
    obj.data.transform(obj.matrix_basis.to_3x3().to_4x4())
    obj.rotation_euler=(0,0,0);obj.scale=(1,1,1)

def box(name,loc,size,material,bevel=0):
    x,y,z=[v/2 for v in size]
    o=mesh(name,[(-x,-y,-z),(-x,-y,z),(-x,y,-z),(-x,y,z),(x,-y,-z),(x,-y,z),(x,y,-z),(x,y,z)],[(2,6,4,0),(5,7,3,1),(4,5,1,0),(3,7,6,2),(1,3,2,0),(6,7,5,4)],material,True)
    o.location=loc
    if bevel:
        b=o.modifiers.new('Edge highlights','BEVEL'); b.width=bevel; b.segments=3
        o.modifiers.new('Weighted normals','WEIGHTED_NORMAL')
    return o

def sphere(name,loc,scale,material,segments=12,rings=8):
    key=(tuple(scale),material,segments,rings)
    if key in SPHERES:
        o=bpy.data.objects.new(name,SPHERES[key]);bpy.context.collection.objects.link(o)
    else:
        verts=[(scale[0]*math.sin(j*math.pi/rings)*math.cos(i*math.tau/segments),scale[1]*math.sin(j*math.pi/rings)*math.sin(i*math.tau/segments),scale[2]*math.cos(j*math.pi/rings)) for j in range(rings+1) for i in range(segments)]
        faces=[((j+1)*segments+i,(j+1)*segments+(i+1)%segments,j*segments+(i+1)%segments,j*segments+i) for j in range(rings) for i in range(segments)]
        o=mesh(name,verts,faces,material)
        layer=o.data.uv_layers.new(name='UVMap')
        for p in o.data.polygons:
            us=[(o.data.loops[li].vertex_index%segments)/segments for li in p.loop_indices]
            seam=max(us)-min(us)>.5
            for li in p.loop_indices:
                vi=o.data.loops[li].vertex_index;u=(vi%segments)/segments
                layer.data[li].uv=(1 if seam and u==0 else u,(vi//segments)/rings)
        for p in o.data.polygons:p.use_smooth=True
        SPHERES[key]=o.data
    o.location=loc
    return o

def cylinder(name,loc,radius,depth,material,vertices=16):
    key=(radius,depth,material,vertices)
    if key in CYLINDERS:
        o=bpy.data.objects.new(name,CYLINDERS[key]);bpy.context.collection.objects.link(o)
    else:
        verts=[(radius*math.cos(i*math.tau/vertices),radius*math.sin(i*math.tau/vertices),z) for z in [-depth/2,depth/2] for i in range(vertices)]
        faces=[tuple(reversed(range(vertices))),tuple(range(vertices,vertices*2))]+[(i,(i+1)%vertices,(i+1)%vertices+vertices,i+vertices) for i in range(vertices)]
        o=mesh(name,verts,faces,material,True)
        for p in o.data.polygons:p.use_smooth=len(p.vertices)==4
        CYLINDERS[key]=o.data
    o.location=loc
    return o

def tube(name,points,radius,material,cyclic=False,sides=6):
    verts=[]; faces=[]; n=len(points)
    for i,p in enumerate(points):
        p=Vector(p); t=Vector(points[(i+1)%n])-Vector(points[(i-1)%n]) if cyclic or 0<i<n-1 else (Vector(points[1])-p if i==0 else p-Vector(points[i-1]))
        t.normalize(); a=t.cross(Vector((0,0,1)))
        if a.length<.01:a=t.cross(Vector((0,1,0)))
        a.normalize(); b=t.cross(a).normalized()
        for j in range(sides):verts.append(p+radius*(math.cos(j*math.tau/sides)*a+math.sin(j*math.tau/sides)*b))
    for i in range(n if cyclic else n-1):
        for j in range(sides):faces.append((i*sides+j,i*sides+(j+1)%sides,((i+1)%n)*sides+(j+1)%sides,((i+1)%n)*sides+j))
    o=mesh(name,verts,faces,material,True)
    for p in o.data.polygons:p.use_smooth=True
    return o

def rounded_rect(cx,y,cz,w,h,r=.15,steps=10):
    pts=[]
    for x,z,start in [(cx+w/2-r,cz+h/2-r,0),(cx-w/2+r,cz+h/2-r,90),(cx-w/2+r,cz-h/2+r,180),(cx+w/2-r,cz-h/2+r,270)]:
        for i in range(steps+1):
            a=math.radians(start+i*90/steps);pts.append((x+r*math.cos(a),y,z+r*math.sin(a)))
    return pts

def panel(name,cx,y,cz,w,h,material):
    o=mesh(name,[(cx-w/2,y,cz-h/2),(cx+w/2,y,cz-h/2),(cx+w/2,y,cz+h/2),(cx-w/2,y,cz+h/2)],[(0,1,2,3)],material)
    uv=o.data.uv_layers.new(name='UVMap')
    for li,coord in enumerate([(0,0),(1,0),(1,1),(0,1)]):uv.data[li].uv=coord
    return o

def empty(name,loc):
    o=bpy.data.objects.new(name,None);bpy.context.collection.objects.link(o);o.location=loc;o.empty_display_size=.2
    return o

def origin_bottom(o):
    if o.type!='MESH':return
    p=Vector((sum(v.co.x for v in o.data.vertices)/len(o.data.vertices),sum(v.co.y for v in o.data.vertices)/len(o.data.vertices),min(v.co.z for v in o.data.vertices)))
    for v in o.data.vertices:v.co-=p
    o.location+=o.matrix_world.to_3x3()@p

def join_objects(objects,name):
    objects=[o for o in objects if o.type=='MESH']
    if not objects:return
    bpy.ops.object.select_all(action='DESELECT')
    for o in objects:
        # Never merge into a cached primitive or linked instance's shared data.
        o.data=o.data.copy()
        o.select_set(True)
    # Convert the selected evaluated meshes in one call so bevels survive joining.
    bpy.context.view_layer.objects.active=objects[0]
    bpy.ops.object.convert(target='MESH')
    bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();objects[0].name=name;objects[0].data.name=name+'_Mesh'
    return objects[0]

def clone(o,name,offset=(0,0,0)):
    new=o.copy();new.data=o.data;bpy.context.collection.objects.link(new);new.name=name;new.location+=Vector(offset);return new

def star(name,center,radius,material,plane='XZ',points=8):
    raised=list(center)
    if plane=='XZ':raised[1]-=radius*.10
    verts=[raised]
    for i in range(points*2):
        a=i*math.pi/points; r=radius*(1 if i%2==0 else .29)
        v=list(center)
        v[0]+=math.sin(a)*r;v[2 if plane=='XZ' else 1]+=math.cos(a)*r;verts.append(v)
    return mesh(name,verts,[(0,(i+1)%(points*2)+1,i+1) for i in range(points*2)],material,True)

def laurel(cx,y,cz,r,name):
    parts=[]
    for s in [-1,1]:
        pts=[]
        for i in range(17):
            a=math.radians(15+i*7);pts.append((cx+s*r*math.sin(a),y,cz-r*math.cos(a)))
        parts.append(tube(name+'_Stem',pts,.009*r,'Gold',sides=5))
        for i in range(7):
            a=math.radians(27+i*14);xx=cx+s*r*math.sin(a);zz=cz-r*math.cos(a)
            for direction in [-1,1]:
                leaf=sphere(name+'_Leaf',(xx+s*direction*.055*r,y-.012,zz),(.07*r,.018*r,.17*r),'Gold',8,4)
                leaf.rotation_euler.y=s*(a*.65+direction*.6);apply(leaf);parts.append(leaf)
    return join_objects(parts,name)

def collider(name,loc,size):
    o=box('COL_'+name,loc,size,'BlackMetal');o.hide_render=True;o.hide_set(True);o.display_type='WIRE';return o
