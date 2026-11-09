"""Fast local floral iteration in the saved theatre lighting; never exports.
The full generator remains the deliverable source of truth.
"""
import bpy,ast,sys,math,argparse
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(Path(__file__).parent))
import trivia_geometry as g
import trivia_refinement as detail
bpy.ops.wm.open_mainfile(filepath=str(root/'art-source/trivia/Trivia_Round01.blend'))
p=argparse.ArgumentParser();p.add_argument('--refresh-textures',action='store_true');p.add_argument('--output',default='art-source/renders/trivia/round-04/bouquet-study.png')
a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
if a.refresh_textures:
    from trivia_materials import build_materials
    g.M,_=build_materials(root/'Assets/_OneMoreFloor/Art/Floors/Trivia/Textures',2048,True)
else:
    g.M={m.name.removeprefix('M_Trivia_'):m for m in bpy.data.materials if m.name.startswith('M_Trivia_')}
for o in list(bpy.data.objects):
    if o.name.startswith('SM_Trivia_FlowerUrn'):o.hide_render=True
node=next(n for n in ast.parse((root/'tools/blender/build_trivia.py').read_text()).body if isinstance(n,ast.FunctionDef) and n.name=='pedestal')
ns=dict(vars(g));ns.update({'bpy':bpy,'detail':detail,'math':math})
exec(compile(ast.Module(body=[node],type_ignores=[]),'pedestal-preview','exec'),ns)
ns['pedestal'](-8.8,-.1,.6)
scene=bpy.context.scene;scene.render.resolution_x=1280;scene.render.resolution_y=720
scene.eevee.taa_render_samples=96
cam=scene.camera;cam.location=(-10.5,-3.8,3.5);cam.rotation_euler=(Vector((-8.8,-.1,2.65))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.lens=50
scene.render.filepath=str(root/a.output)
bpy.ops.render.render(write_still=True)
