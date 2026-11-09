"""Unretouched EEVEE close-ups for inspecting actual deliverable geometry."""
import bpy,sys,argparse
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser();p.add_argument('--tag',default='final');p.add_argument('--width',type=int,default=1672);p.add_argument('--views',default='bouquet,curtain,carving');p.add_argument('--round',default='04')
a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
bpy.ops.wm.open_mainfile(filepath=str(root/'art-source/trivia/Trivia_Round01.blend'))
scene=bpy.context.scene;scene.render.resolution_x=a.width;scene.render.resolution_y=round(a.width*941/1672)
scene.eevee.taa_render_samples=192
data=bpy.data.cameras.new('CAM_Trivia_Detail');cam=bpy.data.objects.new(data.name,data);scene.collection.objects.link(cam);scene.camera=cam
views=[('bouquet',(-10.5,-3.8,3.5),(-8.8,-.1,2.65),50),
       ('curtain',(-5,-5,4.0),(-9,1.8,3.65),50),
       ('carving',(-6,-2,6.1),(-5.2,4.2,6.3),55)]
for name,pos,target,lens in views:
    if name not in a.views.split(','):continue
    cam.location=pos;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();data.lens=lens
    scene.render.filepath=str(root/f'art-source/renders/trivia/round-{a.round}/{a.tag}_{name}.png')
    bpy.ops.render.render(write_still=True)
