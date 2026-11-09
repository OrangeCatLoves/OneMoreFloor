"""Verify a refinement did not move interaction pivots or collision geometry."""
import bpy,json,sys,argparse
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser();p.add_argument('--before',default='art-source/trivia/Trivia_BeforeRefinement.blend')
p.add_argument('--unity-before',help='Optional saved pre-edit unity-markers.json')
a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])

def snapshot(path):
    bpy.ops.wm.open_mainfile(filepath=str(path))
    result={}
    for o in bpy.context.scene.objects:
        interaction=o.name.startswith('MK_') or '_Button_' in o.name or o.name.startswith('SM_Trivia_LiftDoor_') or o.name=='SM_Trivia_QuestionScreen' or o.name.startswith('SM_Trivia_PodiumScreen_')
        if interaction:
            result[o.name]=[float(v) for row in o.matrix_world for v in row]
        if o.name.startswith('COL_'):
            corners=sorted(tuple(round(v,5) for v in o.matrix_world@Vector(p)) for p in o.bound_box)
            result[o.name]=[v for corner in corners for v in corner]
    return result

before=snapshot(root/a.before);after=snapshot(root/'art-source/trivia/Trivia_Round01.blend')
missing=sorted(set(before)-set(after));added=sorted(set(after)-set(before))
changed=[n for n in before.keys()&after.keys() if len(before[n])!=len(after[n]) or max(abs(x-y) for x,y in zip(before[n],after[n]))>1e-4]
report={'baseline':a.before,'checked_objects':len(before),'missing':missing,'added':added,'changed_transforms_or_collision_bounds':sorted(changed)}
(root/'art-source/trivia/refinement-stability.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))
if missing or changed:raise RuntimeError('Refinement moved an interaction/collision contract')
if a.unity_before:
    def markers(path):
        return {m['name']:m for m in json.loads(path.read_text())['markers']}
    old=markers(root/a.unity_before);new=markers(root/'art-source/trivia/unity-markers.json')
    differences=[]
    for name in sorted(old.keys()|new.keys()):
        if name not in old or name not in new:
            differences.append(name);continue
        if any(abs(old[name][field][axis]-new[name][field][axis])>1e-4 for field in ('position','forward','up') for axis in ('x','y','z')):
            differences.append(name)
    result={'source':f'Unity imported marker positions and forward/up axes compared with {a.unity_before}','before_count':len(old),'after_count':len(new),'changed_markers':differences}
    (root/'art-source/trivia/marker-stability.json').write_text(json.dumps(result,indent=2)+'\n')
    print(json.dumps(result,indent=2))
    if differences:raise RuntimeError('Refinement changed imported markers')
