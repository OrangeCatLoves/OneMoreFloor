"""Render saved geometry without rebuilding; settings are reproducible JSON."""
import bpy,sys,json,argparse
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser();p.add_argument('--iteration',default='01');p.add_argument('--views',default='1,2,3');p.add_argument('--width',type=int,default=1280);p.add_argument('--save',action='store_true')
a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
bpy.ops.wm.open_mainfile(filepath=str(root/'art-source/trivia/Trivia_Round01.blend'))
settings=json.loads((root/'art-source/trivia/review_settings.json').read_text())
scene=bpy.context.scene
for i,cam in enumerate(settings['cameras']):
    o=bpy.data.objects[f'CAM_Trivia_Reference_{i+1:02}'];o.location=cam['position'];o.rotation_euler=(Vector(cam['target'])-o.location).to_track_quat('-Z','Y').to_euler();o.data.lens=cam['lens']
scene.view_settings.exposure=settings['exposure']
for o in bpy.data.objects:
    if o.type=='LIGHT':o.data.energy*=settings.get('light_multiplier',1)
scene.render.resolution_x=a.width;scene.render.resolution_y=round(a.width*941/1672)
scene.eevee.taa_render_samples=settings.get('samples',48)
scene.eevee.shadow_pool_size='512'
if a.save:bpy.ops.wm.save_as_mainfile(filepath=str(root/'art-source/trivia/Trivia_Round01.blend'))
for i in a.views.split(','):
    print('RENDER view '+i,flush=True)
    scene.camera=bpy.data.objects[f'CAM_Trivia_Reference_{int(i):02}'];scene.render.filepath=str(root/f'art-source/renders/trivia/round-01/iteration-{a.iteration}_view-{i}.png')
    bpy.ops.render.render(write_still=True)
    print('DONE view '+i,flush=True)
