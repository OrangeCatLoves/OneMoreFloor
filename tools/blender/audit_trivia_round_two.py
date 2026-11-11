"""Physical source checks complement the imported Unity geometry/route audit."""
from pathlib import Path
import bpy,json,math,re
from mathutils import Vector
from mathutils.bvhtree import BVHTree
root=Path(__file__).resolve().parents[2]
bpy.ops.wm.open_mainfile(filepath=str(root/'art-source/trivia/Trivia_Round01.blend'))
deps=bpy.context.evaluated_depsgraph_get();problems=[]
def tree(o):
    m=o.evaluated_get(deps).to_mesh()
    result=BVHTree.FromPolygons([o.matrix_world@v.co for v in m.vertices],[p.vertices[:] for p in m.polygons],epsilon=.00001)
    o.evaluated_get(deps).to_mesh_clear();return result
cloth=tree(bpy.data.objects['SM_Trivia_TiedDrape']);ties=[]
for o in bpy.data.objects:
    if o.type=='MESH' and o.name.startswith(('SM_Trivia_GoldTieback_','SM_Trivia_TiebackAnchor_','SM_Trivia_TiebackKnot_','SM_Trivia_Tassel')):
        count=len(tree(o).overlap(cloth));ties.append({'mesh':o.name,'cloth_triangle_intersections':count})
        if count:problems.append(o.name+': intersects tied cloth')
lights=json.loads((root/'art-source/trivia/lighting.json').read_text(encoding='utf8'));axes=[]
for l in lights:
    if 'fixture_axis' not in l:continue
    axis=bpy.data.objects[l['fixture_axis']];d=axis.matrix_world.to_3x3()@Vector((0,0,-1))
    target=Vector(l['target_blender'])-axis.matrix_world.translation
    error=math.degrees(d.angle(target));axes.append({'fixture':axis.name,'target_error_degrees':error})
    if error>.05:problems.append(axis.name+': does not aim at recorded target')
prefab=(root/'Assets/_OneMoreFloor/Game/Player/Player.prefab').read_text(encoding='utf8')
eye_height=float(re.search(r'm_LocalPosition: \{x: [^,]+, y: ([^,]+),',prefab[prefab.index('m_Name: CameraTarget'):]).group(1))
display=bpy.data.objects['SM_Trivia_PlayerScreen']
targets=[display.matrix_world@v.co for v in display.data.vertices]
targets.append(sum(targets,Vector())/len(targets))
obstacles=[(o.name,tree(o)) for o in bpy.data.objects if o.type=='MESH' and o.name.startswith(('SM_Trivia_Podium','SM_Trivia_PlayerTV'))]
sightlines=[]
for i in range(1,5):
    objective=bpy.data.objects[f'MK_Objective_{i:02}'];eye=objective.matrix_world.translation+Vector((0,0,eye_height))
    for target in targets:
        direction=(target-eye).normalized();length=(target-eye).length;blocker=None
        for name,bvh in obstacles:
            location,normal,index,distance=bvh.ray_cast(eye,direction,length-.025)
            if location is not None:blocker=name;break
        sightlines.append({'player':objective.name,'target_blender':list(target),'blocker':blocker})
        if blocker:problems.append(objective.name+': TV blocked by '+blocker)
feet=[]
for i in range(9,13):
    base=bpy.data.objects[f'SM_Trivia_Spotlight_{i:02}_Base']
    bottom=min((base.matrix_world@Vector(v)).z for v in base.bound_box)
    feet.append({'fixture':i,'bottom_m':bottom})
    if abs(bottom-.6)>.001:problems.append(base.name+': base does not sit on stage')
report={'problems':problems,'cloth_attachment_intersections':ties,'fixture_targets':axes,'tv_sightlines':sightlines,'floor_fixture_support':feet}
(root/'art-source/renders/trivia/round-05/source-audit.json').write_text(json.dumps(report,indent=2),encoding='utf8')
print(json.dumps(report,indent=2))
if problems:raise RuntimeError('; '.join(problems))
