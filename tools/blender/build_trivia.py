"""Trivia round 01: reproducible Blender scene, FBX and EEVEE comparisons.

Run Blender in background with --python tools/blender/build_trivia.py -- --pilot
or -- --iteration 01 --render. Source axes: X right, -Y audience, Z up.
No external asset dependencies. Shared dimensions come from docs/sizes.json.
"""
import bpy, math, json, sys, random, argparse
from pathlib import Path
from mathutils import Vector, Quaternion

ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(Path(__file__).parent))
import trivia_geometry as g
import trivia_refinement as detail
from trivia_geometry import *
from trivia_refinement import curtain, laurel
from trivia_materials import build_materials
from export_unity import export_selection
ARGS=argparse.ArgumentParser()
ARGS.add_argument('--pilot',action='store_true');ARGS.add_argument('--render',action='store_true')
ARGS.add_argument('--iteration',default='01');ARGS.add_argument('--width',type=int,default=1280)
ARGS.add_argument('--views',default='1,2,3');ARGS.add_argument('--textures',type=int,default=2048)
ARGS.add_argument('--refresh-textures',action='store_true')
opt=ARGS.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
S=json.loads((ROOT/'docs/sizes.json').read_text())
OUT=ROOT/'Assets/_OneMoreFloor/Art/Floors/Trivia'
SOURCE=ROOT/'art-source/trivia'
RENDERS=ROOT/'art-source/renders/trivia/round-01'
for p in [OUT,SOURCE,RENDERS]:p.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.context.scene.unit_settings.system='METRIC';bpy.context.scene.unit_settings.scale_length=1
g.M,materials=build_materials(OUT/'Textures',opt.textures,opt.refresh_textures)
RNG=random.Random(7139)
LIGHTS=[]
STAGE=S['stairs']['stepHeight']*3

def light(kind,loc,color,power,target=None,size=1,cone=50,purpose=''):
    i=len(LIGHTS)+1;name=f'MK_Light_{kind}_{i:02}'
    o=empty(name,loc)
    if target:o.rotation_euler=(Vector(target)-Vector(loc)).to_track_quat('-Z','Y').to_euler()
    d=bpy.data.lights.new(name.replace('MK_',''),{'Spot':'SPOT','Area':'AREA','Point':'POINT'}[kind]);d.energy=power
    d.use_shadow=kind=='Spot' or (kind=='Area' and ('Broad warm stage key' in purpose or 'Audience fill' in purpose))
    rgb=tuple(int(color[j:j+2],16)/255 for j in (1,3,5));d.color=rgb
    if kind=='Area':d.shape='DISK';d.size=size
    if kind=='Point':d.shadow_soft_size=size
    if kind=='Spot':d.spot_size=math.radians(cone);d.spot_blend=.5;d.shadow_soft_size=.15
    l=bpy.data.objects.new(d.name,d);bpy.context.collection.objects.link(l);l.location=o.location;l.rotation_euler=o.rotation_euler
    LIGHTS.append({'name':name,'type':kind,'position_blender':list(loc),'target_blender':target,'color':color,'watts_blender':power,'relative_intensity':power/1000,'cone_degrees':cone if kind=='Spot' else None,'size_m':size,'casts_shadow_eevee':d.use_shadow,'purpose':purpose})
    return o

def pedestal(x,y,z=0):
    print('BUILD bouquet at',x,y,flush=True)
    start=set(bpy.data.objects)
    box('SM_Trivia_PlinthFoot',(x,y,z+.09),(.85,.85,.18),'Marble',.025)
    box('SM_Trivia_Plinth',(x,y,z+.59),(.65,.65,.9),'Marble',.025)
    for side in [-1,1]:
        box('SM_Trivia_PlinthInset',(x+side*.33,y,z+.60),(.025,.45,.64),'Ivory',.02)
        box('SM_Trivia_PlinthInset',(x,y+side*.33,z+.60),(.45,.025,.64),'Ivory',.02)
    box('SM_Trivia_PlinthCap',(x,y,z+1.09),(.8,.8,.15),'Marble',.02)
    for zz,ww in [(.21,.72),(.97,.69),(1.025,.76)]:
        box('SM_Trivia_PlinthMoulding',(x,y,z+zz),(ww,ww,.045),'Ivory',.012)
    for side in [-1,1]:
        tube('SM_Trivia_PlinthGiltInset',rounded_rect(x,y+side*.344,z+.59,.45,.65,.035,4),.008,'GoldDark',True,sides=5)
    detail.lathe('SM_Trivia_UrnGadroons',(x,y,z),[(.12,1.29),(.18,1.32),(.27,1.37),(.345,1.48),(.35,1.56),(.35,1.60),(.32,1.62),(.30,1.57),(.10,1.32)],'Gold',64,16)
    cylinder('SM_Trivia_UrnFoot',(x,y,z+1.20),.25,.1,'Gold',24)
    cylinder('SM_Trivia_UrnStem',(x,y,z+1.32),.12,.2,'Gold',16)
    tube('SM_Trivia_UrnRim',[(x+.35*math.cos(i*math.tau/40),y+.35*math.sin(i*math.tau/40),z+1.60) for i in range(40)],.028,'Gold',True)
    detail.bouquet(x,y,z,round((x+20)*100+(y+10)*47))
    o=join_objects(list(set(bpy.data.objects)-start),'SM_Trivia_FlowerUrn');origin_bottom(o)
    collider('Trivia_Plinth',(x,y,z+.57),(.85,.85,1.14))

def podium(index,x,y,z):
    start=set(bpy.data.objects);tag=f'{index:02}'
    base=box('SM_Trivia_PodiumBase',(x,y,z+.08),(1.36,.92,.16),'Gold',.065)
    box('SM_Trivia_PodiumBody',(x,y,z+.49),(1.20,.79,.78),'Walnut',.055)
    for side in [-1,1]:
        box('SM_Trivia_PodiumFrontPanel',(x,y+side*.405,z+.50),(.91,.026,.64),'Walnut',.015)
        tube('SM_Trivia_PodiumPanelTrim',rounded_rect(x,y+side*.425,z+.5,.96,.67,.04,4),.014,'Gold',True)
        crest=laurel(x,y+side*.445,z+.53,.215,'SM_Trivia_PodiumLaurel')
        if side>0:
            for v in crest.data.vertices:v.co.y=2*(y+side*.445)-v.co.y
            for face in crest.data.polygons:face.flip()
        for xx in [-.61,.61]:
            cylinder('SM_Trivia_PodiumColumn',(x+xx,y+side*.405,z+.51),.065,.71,'Bulb',12)
            for zz in [.18,.52,.84]:cylinder('SM_Trivia_PodiumCollar',(x+xx,y+side*.405,z+zz),.085,.05,'Gold',12)
    box('SM_Trivia_PodiumDeskRim',(x,y,z+.96),(1.46,1.02,.16),'Gold',.07)
    box('SM_Trivia_PodiumDesk',(x,y,z+1.045),(1.33,.91,.045),'Walnut',.035)
    # Monitor is inclined toward the player on the audience (-Y) side.
    box('SM_Trivia_MonitorNeck',(x,y+.22,z+1.16),(.18,.15,.24),'Gold',.02)
    rim=box('SM_Trivia_MonitorFrame',(x,y+.22,z+1.48),(1.12,.10,.69),'Gold',.045)
    rim.rotation_euler.x=math.radians(17);apply(rim)
    screen=panel(f'SM_Trivia_PodiumScreen_{tag}',x,y+.165,z+1.48,1.01,.59,'Monitor')
    back=panel(f'SM_Trivia_MonitorBackHousing_{tag}',x,y+.275,z+1.48,1.01,.59,'Monitor')
    for p in back.data.polygons:p.flip()
    # rotate about monitor center, leaving exact 0..1 UVs intact.
    for part in [screen,back]:
        for v in part.data.vertices:
            yy=v.co.y-(y+.22);zz=v.co.z-(z+1.48)
            v.co.y=y+.22+yy*math.cos(math.radians(17))-zz*math.sin(math.radians(17))
            v.co.z=z+1.48+yy*math.sin(math.radians(17))+zz*math.cos(math.radians(17))
    dynamic=[screen]
    deck_start=set(bpy.data.objects)
    box('SM_Trivia_ButtonDeck',(x,y-.275,z+1.055),(1.02,.70,.045),'Gold',.025)
    for k,(dx,dy,col,symbol) in enumerate([(-.21,-.12,'Red','Circle'),(.21,-.12,'Blue','Square'),(-.21,-.43,'Yellow','Cross'),(.21,-.43,'Green','Triangle')]):
        bx=x+dx;by=y+dy
        box('SM_Trivia_ButtonBezel',(bx,by,z+1.09),(.39,.28,.055),'Gold',.035)
        begin=set(bpy.data.objects)
        button=box('SM_Trivia_Button',(bx,by,z+1.13),(.33,.235,.06),col,.025)
        zz=z+1.164
        if symbol=='Circle':tube('SM_Trivia_Symbol',[(bx+.067*math.sin(i*math.tau/24),by+.067*math.cos(i*math.tau/24),zz) for i in range(24)],.009,'Symbol',True)
        elif symbol=='Square':tube('SM_Trivia_Symbol',[(bx-.066,by-.066,zz),(bx+.066,by-.066,zz),(bx+.066,by+.066,zz),(bx-.066,by+.066,zz)],.009,'Symbol',True)
        elif symbol=='Triangle':tube('SM_Trivia_Symbol',[(bx-.08,by-.06,zz),(bx+.08,by-.06,zz),(bx,by+.07,zz)],.009,'Symbol',True)
        else:
            for s in [-1,1]:tube('SM_Trivia_Symbol',[(bx-.063,by-s*.063,zz),(bx+.063,by+s*.063,zz)],.012,'Symbol')
        btn=join_objects(list(set(bpy.data.objects)-begin),f'SM_Trivia_Podium_{tag}_Button_{symbol}');origin_bottom(btn);dynamic.append(btn)
    # Lift the back of the button deck toward the monitor. Buttons remain
    # separate, with their press direction recorded for the importer.
    bpy.context.view_layer.update()
    angle=math.radians(18)
    for part in set(bpy.data.objects)-deck_start:
        transform=part.matrix_world.copy();part.data=part.data.copy()
        for v in part.data.vertices:
            wx,wy,wz=transform@v.co;dy=wy-(y-.625);dz=wz-(z+1.055)
            v.co=(wx,y-.625+dy*math.cos(angle)-dz*math.sin(angle),z+1.055+dy*math.sin(angle)+dz*math.cos(angle))
        part.location=(0,0,0)
        if '_Button_' in part.name:
            origin_bottom(part);part['press_direction_unity']=[0,-math.cos(angle),-math.sin(angle)]
    mesh('SM_Trivia_ButtonDeckSupport',[(x+dx,y+dy,z+zz) for dx,dy,zz in [(-.49,-.62,1.05),(.49,-.62,1.05),(-.49,.03,1.05),(.49,.03,1.05),(-.49,-.62,1.06),(.49,-.62,1.06),(-.49,.03,1.27),(.49,.03,1.27)]],[(0,2,3,1),(4,5,7,6),(0,1,5,4),(2,6,7,3),(0,4,6,2),(1,3,7,5)],'Walnut',True)
    static=join_objects([o for o in set(bpy.data.objects)-start if o not in dynamic],f'SM_Trivia_Podium_{tag}');origin_bottom(static)
    # Match the reference's slender lectern proportions without changing height.
    for part in [static]+dynamic:
        part.data=part.data.copy()
        for v in part.data.vertices:v.co.x*=.85
        part.location.x=x+(part.location.x-x)*.85
    collider(f'Trivia_Podium_{tag}',(x,y,z+.55),(1.46*.85,1.02,1.10))
    empty(f'MK_Objective_{tag}',(x,y-1.05,z))
    light('Point',(x,y-.68,z+.25),'#FFCF83',2,size=.3,purpose='Subtle podium practical spill on polished wood; bake')
    return static


def shield(x,y,z,w=1.2,starred=False):
    pts=[(x-w/2,y,z+w*.5),(x+w/2,y,z+w*.5),(x+w/2,y,z-w*.3),(x,y,z-w*.65),(x-w/2,y,z-w*.3)]
    mesh('SM_Trivia_Shield',pts,[tuple(reversed(range(5)))],'RoyalPurple',True)
    tube('SM_Trivia_ShieldBorder',[(a,b-.025,c) for a,b,c in pts],.035,'Gold',True)
    if starred:star('SM_Trivia_CrestStar',(x,y-.05,z),w*.36,'Gold')
    else:laurel(x,y-.05,z,w*.28,'SM_Trivia_BannerLaurel')

def scrollwork(x,y,z,r=.3,side=1):
    for direction in [-1,1]:
        pts=[]
        for i in range(42):
            a=i/41*math.pi*2.0;rr=r*(1-i/48)
            pts.append((x+side*(rr*math.cos(a)+r*.6),y,z+direction*rr*math.sin(a)))
        tube('SM_Trivia_GiltScroll',pts,.022,'Gold',sides=6)
    sphere('SM_Trivia_ScrollLeaf',(x,y,z),(.08,.03,.16),'Gold',12,6)

def acanthus(x,y,z,scale=1):
    # Layered, lobed leaves and raised midribs, sculpted as real relief geometry.
    for leaf in range(5):
        angle=math.radians((leaf-2)*28);length=scale*(.55 if leaf==2 else .45)
        vertices=[];rows=14;columns=4
        for j in range(rows+1):
            t=j/rows;width=scale*.12*math.sin(math.pi*t)**.65*(.86+.14*math.cos(t*math.tau*4))
            for k in range(columns+1):
                u=2*k/columns-1;xx=u*width;zz=t*length
                yy=-scale*(.10*math.sin(t*math.pi*.8)+.04*(1-u*u)*math.sin(t*math.pi))
                vertices.append((x+xx*math.cos(angle)+zz*math.sin(angle),y+yy,z-xx*math.sin(angle)+zz*math.cos(angle)))
        faces=[(j*(columns+1)+k,j*(columns+1)+k+1,(j+1)*(columns+1)+k+1,(j+1)*(columns+1)+k) for j in range(rows) for k in range(columns)]
        o=mesh('SM_Trivia_AcanthusRelief',vertices,faces,'Gold',True)
        for p in o.data.polygons:p.use_smooth=True
        tube('SM_Trivia_AcanthusMidrib',[(x+t*length*math.sin(angle),y-scale*(.10*math.sin(t*math.pi*.8)+.04*math.sin(t*math.pi))-.006,z+t*length*math.cos(angle)) for t in [j/14 for j in range(15)]],.008*scale,'GoldDark',sides=5)

def lantern(x,y,z):
    start=set(bpy.data.objects)
    sphere('SM_Trivia_SconceBack',(x,y+.12,z-.16),(.16,.08,.26),'Gold',12,8)
    tube('SM_Trivia_SconceArm',[(x,y,z-.25),(x,y-.28,z-.32),(x,y-.4,z-.18)],.028,'Gold')
    cylinder('SM_Trivia_LanternBase',(x,y-.4,z-.12),.12,.065,'Gold',6)
    sphere('SM_Trivia_LanternGlass',(x,y-.4,z+.1),(.13,.13,.25),'Bulb',6,4)
    for k in range(6):
        a=k*math.tau/6;tube('SM_Trivia_LanternFrame',[(x+.09*math.cos(a),y-.4+.09*math.sin(a),z-.1),(x+.16*math.cos(a),y-.4+.16*math.sin(a),z+.29)],.013,'Gold',sides=4)
    sphere('SM_Trivia_LanternCap',(x,y-.4,z+.31),(.19,.19,.065),'Gold',6,4)
    sphere('SM_Trivia_LanternFinial',(x,y-.4,z+.43),(.055,.055,.1),'Gold',8,4)
    join_objects(list(set(bpy.data.objects)-start),'SM_Trivia_WallLantern')
    light('Point',(x,y-.65,z+.12),'#FFB85A',65,size=.22,purpose='Amber wall lantern; bake indirect light')

def moving_light(x,y,z,index,floor=False):
    root=empty(f'MK_Trivia_Spotlight_{index:02}_Pan',(x,y,z))
    base=cylinder(f'SM_Trivia_Spotlight_{index:02}_Base',(x,y,z),.22,.12,'BlackMetal',16)
    root['pan_axis']='local Z in Blender / Y in Unity'
    h=.36 if floor else -.36
    for side in [-1,1]:
        yoke=box(f'SM_Trivia_Spotlight_{index:02}_Yoke',(side*.23,0,h/2),(.08,.16,abs(h)+.1),'BlackMetal',.025)
        yoke.parent=root
    tilt=empty(f'MK_Trivia_Spotlight_{index:02}_Tilt',(0,0,h));tilt.parent=root
    head=cylinder(f'SM_Trivia_Spotlight_{index:02}_Head',(0,0,0),.21,.42,'BlackMetal',20)
    head.rotation_euler.x=math.radians(30 if floor else -22);apply(head)
    head.parent=tilt
    lens=cylinder(f'SM_Trivia_Spotlight_{index:02}_Lens',(0,-(.11 if floor else -.09),(.19 if floor else -.19)),.17,.025,'BlackMetal',20)
    lens.rotation_euler.x=math.radians(30 if floor else -22);apply(lens);lens.parent=tilt
    detail.fixture_detail(head,index,floor)
    target=(x*.65,1,1) if not floor else (x*.5,4,5)
    light('Spot',(x,y,z+h),'#B78DFF' if index%3==0 else '#FFE0AD',650 if not floor else 260,target,cone=44,purpose='Purple stage wash' if index%3==0 else 'Warm stage pool')

def stage():
    # Rear is straight; the stage apron follows a wide ellipse.
    outline=[(-10.7,5.3),(10.7,5.3)]
    outline +=[(10.7*math.cos(i*math.pi/80),-.6-5.0*math.sin(i*math.pi/80)) for i in range(81)]
    outline.reverse()
    n=len(outline);verts=[(x,y,z) for z in [0,STAGE-.025] for x,y in outline]
    faces=[tuple(range(n,2*n)),tuple(reversed(range(n)))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    mesh('SM_Trivia_Stage',verts,faces,'Walnut',True)
    # Individual planks clipped against the curved edge.
    for row in range(39):
        yy=-5.5+row*.28
        if yy>5.25:break
        half=10.65 if yy>=-.6 else 10.65*math.sqrt(max(0,1-((yy+.6)/5.0)**2))
        xx=-half
        while xx<half-.02:
            length=min(RNG.uniform(1.1,2.5),half-xx)
            if length>.04:
                plank=box('SM_Trivia_StagePlank',(xx+length/2,yy+.13,STAGE-.0125),(length-.003,.276,.025),'StageWood',.0017)
                du,dv=RNG.random(),RNG.random()
                for loop in plank.data.uv_layers.active.data:loop.uv.x+=du;loop.uv.y+=dv
            xx+=length
    arc=[(10.7*math.cos(i*math.pi/120),-.6-5*math.sin(i*math.pi/120),STAGE) for i in range(121)]
    for dz,r,material in [(.02,.045,'Gold'),(-.06,.022,'Bulb'),(-.49,.035,'Gold')]:tube('SM_Trivia_StageApronEdge',[(x,y,z+dz) for x,y,z in arc],r,material,sides=8)
    for i in range(100):
        a=i*math.pi/99;x=10.72*math.cos(a);y=-.6-5.02*math.sin(a)
        tube('SM_Trivia_ApronFlute',[(x,y,.10),(x,y,STAGE-.11)],.016,'GoldDark',sides=4)
    collider('Trivia_Stage',(0,2.35,STAGE/2),(21.4,5.9,STAGE))
    # Convex front wedges are separate simple colliders.
    for i in range(20):
        a=i*math.pi/20;b=(i+1)*math.pi/20
        p=[(0,-.6),(10.7*math.cos(a),-.6-5*math.sin(a)),(10.7*math.cos(b),-.6-5*math.sin(b))]
        p.reverse()
        o=mesh(f'COL_Trivia_StageApron_{i:02}',[(x,y,z) for z in [0,STAGE] for x,y in p],[(0,2,1),(3,4,5),(0,1,4,3),(1,2,5,4),(2,0,3,5)],None);o.hide_render=True;o.hide_set(True)
    sw=S['stairs']['width'];sd=S['stairs']['stepDepth'];sh=S['stairs']['stepHeight']
    for i in range(3):
        yy=-5.6-(3-i-.5)*sd
        box('SM_Trivia_StageStep',(0,yy,sh*(i+1)/2),(sw,sd+.02,sh*(i+1)),'Walnut',.045)
        tube('SM_Trivia_StepLight',[(-sw/2+.08,yy-sd/2-.01,sh*(i+1)-.05),(sw/2-.08,yy-sd/2-.01,sh*(i+1)-.05)],.02,'Bulb')
        collider('Trivia_Step',(0,yy,sh*(i+1)/2),(sw,sd,sh*(i+1)))
    star('SM_Trivia_HostCompass',(0,.5,STAGE+.002),1.05,'InlayGold',plane='XY')
    tube('SM_Trivia_HostCompassRing',[(1.3*math.sin(i*math.tau/128),.5+1.3*math.cos(i*math.tau/128),STAGE+.003) for i in range(128)],.018,'InlayGold',True)
    empty('MK_Host',(0,.5,STAGE))
    for i,x in enumerate([-4.8,-1.6,1.6,4.8]):podium(i+1,x,-2.5,STAGE)

def screen_and_arch():
    curtain('SM_Trivia_BackCurtain',-10,10,5.05,STAGE,9.5)
    # Bulb marquee with a purple surround and live cream center.
    cx=0;y=4.25;cz=5.4;w=12.3;h=4.25
    border=rounded_rect(cx,y,cz,w,h,.65,14)
    mesh('SM_Trivia_QuestionSurround',border,[tuple(range(len(border)))],'RoyalPurple',True)
    for d,r in [(0,.075),(-.17,.025),(.17,.055)]:tube('SM_Trivia_QuestionFrame',rounded_rect(cx,y-.055,cz,w+d,h+d,.65,14),r,'Gold',True,sides=16)
    panel('SM_Trivia_QuestionScreen',0,y-.10,cz,9.5,3.55,'Screen')
    tube('SM_Trivia_InnerQuestionFrame',rounded_rect(0,y-.13,cz,9.6,3.65,.25,8),.035,'Gold',True)
    for sx in [-1,1]:
        for sz in [-1,1]:
            # Small carved corner flourishes sit outside the live-screen inset.
            x=sx*4.71;z=cz+sz*1.79
            for k in range(3):
                a=k*.35+.20
                detail.carved_leaf('SM_Trivia_ScreenCornerCarving',(x,y-.18,z),(x+sx*.24*math.sin(a),y-.18,z+sz*.24*math.cos(a)),.045,depth=.025)
    for x in [-5.35,5.35]:laurel(x,y-.13,cz,.49,'SM_Trivia_ScreenLaurel')
    # Pearl-and-reel moulding gives the broad surround a carved inner layer.
    for side in [-1,1]:
        for i in range(47):
            xx=-4.65+i*.202
            sphere('SM_Trivia_FramePearl',(xx,y-.16,cz+side*1.94),(.035,.022,.026),'GoldDark',8,4)
        for sx in [-1,1]:
            detail.rosette(sx*5.58,y-.17,cz+side*1.43,.145)
            scrollwork(sx*5.22,y-.16,cz+side*1.48,.22,sx)
    for sx in [-1,1]:
        for sz in [-1,1]:
            scrollwork(sx*5.65,y-.15,cz+sz*1.6,.18,sx)
            acanthus(sx*5.65,y-.15,cz+sz*1.6-.12,.5)
    groups=[[],[],[]]
    perimeter=rounded_rect(cx,y-.13,cz,w,h,.65,30)
    lengths=[(Vector(perimeter[(i+1)%len(perimeter)])-Vector(p)).length for i,p in enumerate(perimeter)]
    total=sum(lengths);pts=[]
    for j in range(96):
        distance=j*total/96
        for i,segment in enumerate(lengths):
            if distance<=segment:
                pts.append(Vector(perimeter[i]).lerp(Vector(perimeter[(i+1)%len(perimeter)]),distance/segment));break
            distance-=segment
    for i,p in enumerate(pts):groups[i%3].append(sphere('SM_Trivia_MarqueeBulb',p,(.055,.055,.055),'Bulb',20,10))
    for i,grp in enumerate(groups):join_objects(grp,f'SM_Trivia_MarqueeChase_{i+1:02}')
    light('Area',(0,3.95,5.6),'#FFE1AD',180,(0,-3,2.4),size=8,purpose='Aggregate marquee bounce; bulbs use three emissive chase sets')
    # Layered columns and a shallow arched cornice, carved with leaves and beads.
    arch_start=set(bpy.data.objects)
    for side in [-1,1]:
        x=side*10.25
        box('SM_Trivia_ProsceniumPillar',(x,4.2,4.7),(1.05,.9,8.2),'Ivory',.07)
        for z in [.8,1.05,8.3,8.55,8.8]:box('SM_Trivia_ProsceniumCapital',(x,4.1,z),(1.35,1.08,.16),'Gold',.035)
        for dx in [-.38,.38]:box('SM_Trivia_ProsceniumFlute',(x+dx,3.72,4.65),(.045,.04,7.0),'Gold',.01)
        curtain('SM_Trivia_TiedDrape',side*7.8 if side>0 else -10.05,10.05 if side>0 else -7.8,3.8,STAGE,9.0,pull=1.25)
        for strand in range(3):
            pts=[]
            for i in range(121):
                t=i/120;phase=t*math.tau*12+strand*math.tau/3
                pts.append((side*(9.0+t*.88),3.53+math.cos(phase)*.035,2.9+.15*math.sin(t*math.pi)+math.sin(phase)*.035))
            tube('SM_Trivia_GoldTieback',pts,.022,'Gold',sides=6)
        detail.tassel(side*9.0,3.45,2.89)
        for zz in [1.4,7.65]:
            detail.rosette(x,3.69,zz,.22)
        for zz in [2.4,5.75]:
            tube('SM_Trivia_PilasterRaisedPanel',rounded_rect(x,3.70,zz,.65,2.3,.08,5),.022,'GoldDark',True)
            acanthus(x,3.65,zz+.50,.65)
    arch=[(-10.5+21*i/100,4.0,8.85+.75*math.sin(i*math.pi/100)) for i in range(101)]
    for dz,r,ma in [(0,.19,'Gold'),(.23,.17,'Ivory'),(.44,.12,'Gold'),(-.2,.045,'Bulb')]:tube('SM_Trivia_ProsceniumArch',[(x,y,z+dz) for x,y,z in arch],r,ma,sides=10)
    for i in range(43):
        a=i/42;x=-10.2+a*20.4;z=8.85+.75*math.sin(a*math.pi)
        if i%3==0:detail.rosette(x,3.77,z,.12)
        else:sphere('SM_Trivia_ArchRosette',(x,3.77,z),(.045,.025,.05),'GoldDark',8,4)
    for i in range(18):
        x=-9.35+i*1.1;acanthus(x,3.74,8.85+.75*math.sin((x+10.5)/21*math.pi)-.03,.6)
    shield(0,3.62,9.03,1.8,True)
    for sx in [-1,1]:
        for i in range(8):scrollwork(sx*(1.6+i*.95),3.77,9.05,.16,sx)
    # Truss triangulation and scalloped bunting.
    for z in [8.05,8.45]:tube('SM_Trivia_Truss',[(-9,3.8,z),(9,3.8,z)],.035,'BlackMetal')
    for i in range(36):
        x=-9+i*.5;tube('SM_Trivia_TrussBrace',[(x,3.8,8.05),(x+.25,3.8,8.45),(x+.5,3.8,8.05)],.02,'GoldDark',sides=5)
    rope=[(-9+18*i/100,3.6,8.5-.7*math.sin(i*math.pi/100)**2) for i in range(101)]
    tube('SM_Trivia_BuntingCord',rope,.018,'Gold')
    for i in range(22):
        u=(i+.5)/22;x=-9+18*u;z=8.5-.7*math.sin(u*math.pi)**2
        verts=[];rows=5;cols=4
        for j in range(rows+1):
            t=j/rows
            for k in range(cols+1):
                v=k/cols*2-1
                verts.append((x+v*.32*(1-t)+.10*t,3.58-.065*math.sin(math.pi*t)*math.cos(v*2+i),z-.55*t))
        flag=detail.smooth(mesh('SM_Trivia_Bunting',verts,[(j*(cols+1)+k,j*(cols+1)+k+1,(j+1)*(cols+1)+k+1,(j+1)*(cols+1)+k) for j in range(rows) for k in range(cols)],'RoyalPurple' if i%3==0 else 'BuntingGold',True))
        for face in flag.data.polygons:face.flip()
        solid=flag.modifiers.new('Fabric thickness','SOLIDIFY');solid.thickness=.003
    for o in set(bpy.data.objects)-arch_start:o.location.y-=2.0
    for i,x in enumerate([-8,-5.7,-3.4,-1.1,1.1,3.4,5.7,8]):moving_light(x,1.0,8.2,i+1)
    for i,(x,y) in enumerate([(-9,-2),(9,-2),(-7,3),(7,3)]):moving_light(x,y,STAGE+.14,i+9,True)

def seat_template(distant=False):
    start=set(bpy.data.objects)
    box('SM_Trivia_SeatCushion',(0,0,.47),(.66,.61,.18),'VelvetSeat',.075)
    back=box('SM_Trivia_SeatBack',(0,-.26,.87),(.68,.15,.77),'VelvetSeat',.09)
    detail.upholstered_back(distant)
    tube('SM_Trivia_SeatPiping',rounded_rect(0,-.345,.89,.62,.69,.10,5),.012,'Gold',True)
    for s in [-1,1]:
        box('SM_Trivia_SeatArm',(s*.37,0,.65),(.105,.65,.1),'Gold',.035)
        box('SM_Trivia_SeatArmPad',(s*.37,-.02,.71),(.10,.50,.05),'VelvetSeat',.02)
        box('SM_Trivia_SeatLeg',(s*.31,.03,.25),(.065,.07,.5),'BlackMetal',.012)
    for x in [-.18,0,.18]:sphere('SM_Trivia_SeatButton',(x,-.345,.95),(.018,.009,.018),'GoldDark',8,4)
    parts=list(set(bpy.data.objects)-start)
    if distant:
        # Far rows retain the same silhouette, trim and upholstery. Fewer bevel
        # subdivisions remove subpixel curved-edge tessellation, not features.
        for part in parts:
            for modifier in part.modifiers:
                if modifier.type=='BEVEL':modifier.segments=1
    o=join_objects(parts,'SM_Trivia_Seat_PrototypeFar' if distant else 'SM_Trivia_Seat_Prototype');origin_bottom(o);return o

def rail(points,z):
    tube('SM_Trivia_Handrail',[(x,y,z) for x,y in points],.055,'Gold',sides=8)
    tube('SM_Trivia_HandrailBottom',[(x,y,z-.72) for x,y in points],.025,'Gold',sides=6)
    for x,y in points[::max(1,len(points)//18)]:
        cylinder('SM_Trivia_Baluster',(x,y,z-.43),.025,.85,'Gold',8)
        sphere('SM_Trivia_BalusterCollar',(x,y,z-.30),(.045,.045,.07),'Gold',8,4)

def grille(x,y,z,w=1.0,h=1.35):
    box('SM_Trivia_GrilleShadow',(x,y,z),(w,.10,h),'BlackMetal',.025)
    tube('SM_Trivia_GrilleFrame',rounded_rect(x,y-.07,z,w,h,.035,3),.045,'Gold',True)
    for i in range(9):
        for j in range(10):
            xx=x-w*.43+i*w*.105;zz=z-h*.43+j*h*.095
            motif=[]
            for k in range(12):
                a=k*math.tau/12;r=.044*(1+.23*math.cos(4*a))
                motif.append((xx+r*math.cos(a),y-.08,zz+r*1.16*math.sin(a)))
            tube('SM_Trivia_GrilleMotif',motif,.007,'GoldDark',True,sides=4)
    for d,r in [(.06,.014),(-.07,.012)]:
        tube('SM_Trivia_GrilleReededBorder',rounded_rect(x,y-.11,z,w+d,h+d,.06,4),r,'Gold',True,sides=6)

def theatre_box(x,y):
    # Facade planes face the stage and main audience; an actual recess behind.
    for z in [1.0,4.0,7.4]:box('SM_Trivia_BoxCornice',(x,y,z),(2.8,1.0,.18),'Gold',.04)
    for dx in [-1.18,1.18]:
        box('SM_Trivia_BoxPilaster',(x+dx,y,4.2),(.30,.7,6.2),'Ivory',.025)
        for xx in [-.10,.10]:box('SM_Trivia_BoxPilasterTrim',(x+dx+xx,y-.36,4.2),(.025,.03,5.9),'Gold')
    curtain('SM_Trivia_BoxCurtain',x-1.05,x+1.05,y+.14,1.1,3.9)
    box('SM_Trivia_BoxRecess',(x,y+.22,5.7),(2.1,.12,2.7),'BlackMetal')
    for side in [-1,1]:
        curtain('SM_Trivia_BoxUpperDrape',x+side*.86-.22,x+side*.86+.22,y-.08,4.55,7.2,pull=.16)
    for zz in [1.15,3.80,4.0,7.35,7.5,7.65]:
        box('SM_Trivia_BoxLayeredCornice',(x,y-.15,zz),(2.85+(zz%1)*.12,1,.09),'Ivory' if zz==7.5 else 'Gold',.02)
    arch=[(x+1.07*math.cos(i*math.pi/32),y-.42,6.15+1.15*math.sin(i*math.pi/32)) for i in range(33)]
    for dz,r in [(0,.08),(.13,.035)]:tube('SM_Trivia_BoxArch',[(a,b,c+dz) for a,b,c in arch],r,'Gold',sides=8)
    for i in range(15):
        a=i*math.pi/14;sphere('SM_Trivia_BoxArchBead',(x+1.19*math.cos(a),y-.44,6.15+1.28*math.sin(a)),(.035,.035,.035),'Gold',8,4)
    for dx in [-1.15,1.15]:
        for zz in [2.4,5.7]:tube('SM_Trivia_BoxPilasterInset',rounded_rect(x+dx,y-.37,zz,.21,2.1,.035,3),.012,'Gold',True)
        acanthus(x+dx,y-.4,6.65,.6)
    # Side swags above the upper box, layered fabric loops.
    for side in [-1,1]:detail.swag(x+side*.51,y-.35,7.03,1.04,.35)
    box('SM_Trivia_BoxFascia',(x,y-.2,4.3),(2.45,.65,.56),'Ivory',.08)
    rail([(x-1.12+i*2.24/20,y-.58) for i in range(21)],5.18)
    shield(x,y-.64,4.35,.85)
    sphere('SM_Trivia_BoxKeystone',(x,y-.43,7.27),(.17,.12,.27),'Gold',16,8)
    for dx in [-1.35,1.35]:lantern(x+dx,y-.24,5.9)

def hall():
    wall=S['wall']['thickness'];height=S['ceiling']['height']*3
    box('SM_Trivia_HallFloor',(0,-10,-.12),(29,32,.24),'Walnut')
    collider('Trivia_HallFloor',(0,-10,-.12),(29,32,.24))
    for x in [-14.5,14.5]:
        if x<0:
            # Open passage through the left wall into the lift cab.
            for yy,depth in [(-14.75,22.5),(2.75,6.5)]:
                box('SM_Trivia_SideWall',(x,yy,height/2),(wall,depth,height),'BurgundyWood')
                collider('Trivia_SideWall',(x,yy,height/2),(wall,depth,height))
            hh=S['elevator']['door']['height'];ww=S['elevator']['door']['width']
            box('SM_Trivia_LiftWallHeader',(x,-2,(height+hh)/2),(wall,ww,height-hh),'VelvetRed')
            collider('Trivia_LiftWallHeader',(x,-2,(height+hh)/2),(wall,ww,height-hh))
        else:
            box('SM_Trivia_SideWall',(x,-10,height/2),(wall,32,height),'BurgundyWood')
            collider('Trivia_SideWall',(x,-10,height/2),(wall,32,height))
    box('SM_Trivia_RearWall',(0,-26,height/2),(29,wall,height),'BurgundyWood')
    collider('Trivia_RearWall',(0,-26,height/2),(29,wall,height))
    box('SM_Trivia_BackWall',(0,5.5,height/2),(29,wall,height),'VelvetRed')
    collider('Trivia_BackWall',(0,5.5,height/2),(29,wall,height))
    box('SM_Trivia_Ceiling',(0,-10,height+.15),(29,32,.3),'VelvetSeat')
    collider('Trivia_Ceiling',(0,-10,height+.15),(29,32,.3))
    for yy in [-22,-14,-6,2]:
        box('SM_Trivia_CeilingBeam',(0,yy,height-.1),(29,.18,.20),'GoldDark',.025)
    # Downstage side wing fronts, carrying lift and theatre boxes.
    for x in [-12.4,12.4]:
        box('SM_Trivia_WingWall',(x,3.2,4.7),(4.1,.35,9.4),'Walnut')
        for dx in [-1.8,-1.4,-1,-.6,-.2,.2,.6,1,1.4,1.8]:box('SM_Trivia_WingWallBead',(x+dx,3.00,4.7),(.018,.035,9.2),'GoldDark')
        theatre_box(x,2.8)
    # Balcony horseshoe: back arc continues as side balconies.
    balcony=[]
    for i in range(81):
        a=math.pi+i*math.pi/80;balcony.append((12.5*math.cos(a),-10+12.5*math.sin(a)))
    for side in [-1,1]:
        line=[(side*12.5,-10+i*11/40) for i in range(41)]
        balcony.extend(line)
    # Separate continuous runs to avoid connecting across the hall.
    paths=[[(12.5*math.cos(math.pi+i*math.pi/80),-10+12.5*math.sin(math.pi+i*math.pi/80)) for i in range(81)]]
    paths += [[(s*12.5,-10+i*11/40) for i in range(41)] for s in [-1,1]]
    for path in paths:
        n=len(path);v=[(x,y,z) for z in [4.8,5.65] for x,y in path]
        mesh('SM_Trivia_BalconyFascia',v,[(i,i+1,n+i+1,n+i) for i in range(n-1)],'Ivory',True)
        for z,r,ma in [(4.8,.06,'Gold'),(4.95,.025,'Bulb'),(5.65,.06,'Gold')]:tube('SM_Trivia_BalconyMoulding',[(x,y,z) for x,y in path],r,ma,sides=8)
        rail(path,6.5)
        for i in range(0,n,3):
            x,y=path[i];sphere('SM_Trivia_BalconyBulb',(x*.995,y+.03,4.98),(.055,.055,.055),'Bulb',8,6)
    # Balcony decks and side seating, respecting the central view.
    for s in [-1,1]:
        box('SM_Trivia_BalconyDeck',(s*13.5,-5,4.72),(2.0,14,.20),'Walnut')
        collider('Trivia_BalconyDeck',(s*13.5,-5,4.72),(2.0,14,.20))
    box('SM_Trivia_BackBalconyDeck',(0,-24,4.72),(25,3,.20),'Walnut')
    collider('Trivia_BackBalconyDeck',(0,-24,4.72),(25,3,.20))
    for x in [-8,-4,0,4,8]:
        yy=-10-math.sqrt(12.5**2-x*x)+.10
        before=set(bpy.data.objects);shield(0,0,5.5,1.2,x==0)
        bpy.context.view_layer.update()
        angle=math.atan2(-x,-math.sqrt(12.5**2-x*x))
        for o in set(bpy.data.objects)-before:
            transform=o.matrix_world.copy();o.data=o.data.copy()
            for v in o.data.vertices:
                vx,vy,vz=transform@v.co;v.co=(x+vx*math.cos(angle)-vy*math.sin(angle),yy+vx*math.sin(angle)+vy*math.cos(angle),vz)
            o.location=(0,0,0)
    for side in [-1,1]:
        for yy in [-4,-9,-14]:
            before=set(bpy.data.objects);shield(0,0,5.45,1.2,yy==-9)
            for o in set(bpy.data.objects)-before:
                if o.type!='MESH':continue
                transform=o.matrix_world.copy();o.data=o.data.copy()
                for v in o.data.vertices:
                    vx,vy,vz=transform@v.co;v.co=(side*(12.46+vy),yy-side*vx,vz)
                o.location=(0,0,0)
    for side in [-1,1]:
        # Additional side-wall theatre boxes face into the hall. They enrich the
        # reverse stage view while preserving the image-1 wing boxes and lift.
        before=set(bpy.data.objects);theatre_box(0,0)
        bpy.context.view_layer.update()
        for o in set(bpy.data.objects)-before:
            if o.type=='MESH':
                transform=o.matrix_world.copy();o.data=o.data.copy()
                for v in o.data.vertices:
                    vx,vy,vz=transform@v.co;v.co=(side*(14.1+vy),-6.4-side*vx,vz)
                o.location=(0,0,0)
            else:
                old=o.location.copy();o.location=(side*(14.1+old.y),-6.4-side*old.x,old.z)
                o.rotation_euler.z+=side*math.pi/2
        collider('Trivia_SideTheatreBox',(side*13.9,-6.4,3.85),(1.2,2.9,7.7))
        for yy in [-19,-14,-9,-4]:
            # Burgundy panels and gilt pilasters on the side walls.
            x=side*14.3
            box('SM_Trivia_WallPilaster',(x,yy,5.1),(.18,.32,10),'Ivory',.03)
            box('SM_Trivia_WallPilasterGilt',(x-side*.1,yy,5.1),(.035,.05,9.7),'Gold')
        for yy in [-22,-18,-14,-10,-6,-2,2]:
            for zz in [2.3,7.6]:
                for edge in [-1,1]:
                    box('SM_Trivia_SidePanelVertical',(side*14.32,yy+edge*1.55,zz),(.055,.035,3.6),'GoldDark')
                    box('SM_Trivia_SidePanelHorizontal',(side*14.32,yy,zz+edge*1.8),(.055,3.1,.035),'Gold')
        for yy in [-8,1.2]:
            for zz in [2.0,7.8]:
                before=set(bpy.data.objects);grille(0,0,zz,1.15,1.65)
                bpy.context.view_layer.update()
                for o in set(bpy.data.objects)-before:
                    transform=o.matrix_world.copy();o.data=o.data.copy()
                    for v in o.data.vertices:
                        vx,vy,vz=transform@v.co;v.co=(side*(14.28+vy),yy-side*vx,vz)
                    o.location=(0,0,0)
    # Rear panels visible from the stage.
    for x in range(-13,14,2):
        box('SM_Trivia_RearPanel',(x,-25.8,5.1),(1.7,.10,9.5),'BurgundyWood',.015)
        tube('SM_Trivia_RearPanelFrame',rounded_rect(x,-25.72,5.1,1.55,9.2,.05,3),.025,'GoldDark',True)
        for zz,hh in [(2.2,3.5),(8.1,3.2)]:
            tube('SM_Trivia_RearPanelInner',rounded_rect(x,-25.69,zz,1.33,hh,.09,5),.018,'Gold',True)
        if x%4==1:
            before=set(bpy.data.objects);lantern(x,0,7.3)
            bpy.context.view_layer.update()
            for o in set(bpy.data.objects)-before:
                if o.type=='MESH':
                    transform=o.matrix_world.copy();o.data=o.data.copy()
                    for v in o.data.vertices:
                        vx,vy,vz=transform@v.co;v.co=(2*x-vx,-25.45-vy,vz)
                    o.location=(0,0,0)
                else:
                    old=o.location.copy();o.location=(2*x-old.x,-25.45-old.y,old.z);o.rotation_euler.z+=math.pi
    prototype=seat_template();far_prototype=seat_template(distant=True)
    for row in range(9):
        yy=-8.3-row*1.43;zz=row*S['stairs']['stepHeight']
        for side in [-1,1]:
            box('SM_Trivia_SeatTier',(side*7.0,yy,zz/2-.01),(10.2,1.43,max(.05,zz)),'Walnut')
            collider('Trivia_SeatingTier',(side*7,yy,zz+.55),(10.2,1.10,1.1))
            for col in range(11):
                xx=side*(2.15+col*.86);clone(prototype if row<4 else far_prototype,f'SM_Trivia_Seat_R{row:02}_{side}_{col:02}',(xx,yy,zz))
        # Continuous stair aisle, made from data-sized steps on the seating rise.
        box('SM_Trivia_AisleCarpet',(0,yy,zz+.012),(S['stairs']['width'],1.43,.025),'VelvetRed')
        if row>0:
            box('SM_Trivia_AisleTerrace',(0,yy,zz/2),(S['stairs']['width'],1.43,zz),'Walnut')
            panel('SM_Trivia_AisleRiser',0,yy+.716,zz-S['stairs']['stepHeight']/2,S['stairs']['width'],S['stairs']['stepHeight'],'VelvetRed')
        for xx in [-1.39,1.39]:box('SM_Trivia_AisleBorder',(xx,yy,zz+.029),(.025,1.43,.008),'Gold')
        star('SM_Trivia_AisleStar',(0,yy,zz+.03),.16,'Gold',plane='XY')
        collider('Trivia_Aisle',(0,yy,zz/2-.01),(3,1.43,max(.05,zz)))
    for side in [-1,1]:
        rail([(side*(2.0+i*.45),-7.5) for i in range(23)],1.0)
        for row in range(2):
            for col in range(11):clone(far_prototype,f'SM_Trivia_UpperSeat_{side}_{row}_{col}',(side*(2.3+col*.85),-24-row*1.0,4.85+row*.2))
        for row in range(2):
            for col in range(12):
                seat=clone(far_prototype,f'SM_Trivia_UpperSeat_Side_{side}_{row}_{col}',(side*(13.25+row*.65),-10+col*.95,4.85+row*.2))
                seat.rotation_euler.z=math.atan2(side*(13.25+row*.65),4-(-10+col*.95))
    bpy.data.objects.remove(prototype,do_unlink=True)
    bpy.data.objects.remove(far_prototype,do_unlink=True)
    # Gold grille pair and lantern-lit lift to stage-left.
    for z in [5.7,7.6]:grille(-10.8,1.4,z,1.05,1.45)
    lift()
    for x,y,z in [(-8.8,-.1,STAGE),(8.8,-.1,STAGE),(-12.5,-4.5,0),(-12.5,.5,0)]:pedestal(x,y,z)

def lift():
    # Side wall entrance faces +X into the hall. Build in a local front plane,
    # then rotate the assembly; clear dimensions come directly from sizes.json.
    start=set(bpy.data.objects);w=S['elevator']['door']['width'];h=S['elevator']['door']['height']
    for side in [-1,1]:
        leaf_start=set(bpy.data.objects)
        door=box('SM_Trivia_LiftDoorPanel',(side*(w/4+.006),0,h/2),(w/2-.012,.10,h),'Gold',.015)
        for dx in [-w/4+.12,w/4-.12]:box('SM_Trivia_LiftDoorInset',(side*w/4+dx,-.06,h/2),(.012,.02,h-.25),'GoldDark')
        door=join_objects(list(set(bpy.data.objects)-leaf_start),f'SM_Trivia_LiftDoor_{"L" if side<0 else "R"}');origin_bottom(door)
        collider(f'Trivia_LiftDoor_{"L" if side<0 else "R"}',(side*(w/4+.006),0,h/2),(w/2-.012,.1,h))
    for side in [-1,1]:box('SM_Trivia_LiftJamb',(side*(w/2+.13),0,h/2),(.23,.32,h),'Gold',.04)
    arch=[((w/2+.1)*math.cos(i*math.pi/40),-.05,h+.5*math.sin(i*math.pi/40)) for i in range(41)]
    tube('SM_Trivia_LiftArch',arch,.13,'Gold',sides=10)
    for side in [-1,1]:
        box('SM_Trivia_LiftPilaster',(side*(w/2+.36),.06,(h+.65)/2),(.35,.35,h+.65),'Ivory',.03)
        for zz in [.10,.28,h+.45,h+.65]:box('SM_Trivia_LiftCapital',(side*(w/2+.36),-.01,zz),(.52,.48,.10),'Gold',.02)
        tube('SM_Trivia_LiftPilasterInset',rounded_rect(side*(w/2+.36),-.13,1.45,.22,2.0,.035,4),.018,'Gold',True)
    for dz in [.2,.37]:tube('SM_Trivia_LiftArchLayer',[(xx,-.08,zz+dz) for xx,_,zz in arch],.05,'GoldDark' if dz==.2 else 'Gold',sides=8)
    box('SM_Trivia_LiftIndicator',(0,-.09,h+.26),(.85,.13,.27),'BlackMetal',.045)
    for side in [-1,1]:
        x=side*.18;coords=[(x-.09,-.17,h+.20),(x+.09,-.17,h+.20),(x,-.17,h+.34)]
        if side>0:coords=[(xx,yy,2*(h+.27)-zz) for xx,yy,zz in coords]
        mesh('SM_Trivia_LiftArrow',coords,[(0,2,1) if side>0 else (0,1,2)],'Bulb',True)
    for x in [-2.1,2.1]:lantern(x,0,2.5)
    # Rotate facade to left side. Origin of every moving leaf stays at its lower center.
    objs=set(bpy.data.objects)-start
    bpy.context.view_layer.update()
    for o in objs:
        # x local -> -Y world, front (-Y local) -> +X world.
        if o.type=='MESH':
            apply(o)
            for v in o.data.vertices:
                wx,wy,wz=o.matrix_world@v.co;v.co=(-wy,wx,wz)
            o.location=(-13,-2.0,0)
        else:
            old=o.location.copy();o.location=(-13-old.y,-2+old.x,old.z);o.rotation_euler.z+=math.pi/2
    for o in objs:
        if o.name.startswith('SM_Trivia_LiftDoor_'):origin_bottom(o)
    # Spawn markers inside the adjoining lift volume.
    ei=S['elevator']['interior'];cx=-13-ei['depth']/2
    box('SM_Trivia_LiftFloor',(cx,-2,-.10),(ei['depth'],ei['width'],.20),'Marble')
    collider('Trivia_LiftFloor',(cx,-2,-.10),(ei['depth'],ei['width'],.20))
    for side in [-1,1]:
        box('SM_Trivia_LiftCabWall',(cx,-2+side*ei['width']/2,ei['height']/2),(ei['depth'],.15,ei['height']),'GoldDark')
        collider('Trivia_LiftCabWall',(cx,-2+side*ei['width']/2,ei['height']/2),(ei['depth'],.15,ei['height']))
    box('SM_Trivia_LiftCabBack',(cx-ei['depth']/2,-2,ei['height']/2),(.15,ei['width'],ei['height']),'GoldDark')
    collider('Trivia_LiftCabBack',(cx-ei['depth']/2,-2,ei['height']/2),(.15,ei['width'],ei['height']))
    box('SM_Trivia_LiftCeiling',(cx,-2,ei['height']),(ei['depth'],ei['width'],.15),'Ivory')
    collider('Trivia_LiftCeiling',(cx,-2,ei['height']),(ei['depth'],ei['width'],.15))
    for i,(dx,dy) in enumerate([(-1,-1),(-1,1),(1,-1),(1,1)]):empty(f'MK_Spawn_{i+1:02}',(cx+dx,-2+dy,0))
    empty('MK_Exit',(-12.4,-2,0))

def cameras():
    settings=[((9,-22,8),(-2.5,-.7,3.5),31.5),((-11.5,-15,3.8),(-1,1,3.7),35),((-2,3.65,3.1),(0,-6,1.8),19.5)]
    for i,(loc,target,lens) in enumerate(settings):
        data=bpy.data.cameras.new(f'CAM_Trivia_Reference_{i+1:02}');o=bpy.data.objects.new(data.name,data);bpy.context.collection.objects.link(o);o.location=loc;o.rotation_euler=(Vector(target)-Vector(loc)).to_track_quat('-Z','Y').to_euler();data.lens=lens;data.clip_end=200
    return settings

def finalize():
    print('FINALIZE',len(bpy.data.objects),'objects',flush=True)
    # Apply mesh rotations and scales. Curves are already meshes.
    for o in list(bpy.context.scene.objects):
        if o.type=='MESH':apply(o)
    # Batch repeated static detail by name. Seats retain linked mesh instances;
    # anything interactive, parented, or used as a marker remains independent.
    import re
    groups={}
    for o in list(bpy.context.scene.objects):
        if o.type!='MESH' or o.parent or o.name.startswith('COL_') or any(t in o.name for t in ['Seat_R','UpperSeat','LiftDoor','Spotlight','Screen','Button_']):continue
        key=re.sub(r'\.\d{3,}$','',o.name)
        groups.setdefault(key,[]).append(o)
    for key,group in groups.items():
        if len(group)>1:join_objects(group,key)
    print('BATCHED static details',flush=True)
    bpy.context.view_layer.update()
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH' and not o.name.startswith('COL_')]
    deps=bpy.context.evaluated_depsgraph_get()
    triangles=sum(sum(len(p.vertices)-2 for p in o.evaluated_get(deps).data.polygons) for o in meshes)
    (SOURCE/'material_manifest.json').write_text(json.dumps(materials,indent=2))
    for record in LIGHTS:
        marker=bpy.data.objects[record['name']]
        record['position_blender']=list(marker.matrix_world.translation)
        direction=marker.matrix_world.to_3x3()@Vector((0,0,-1))
        record['direction_blender']=list(direction)
        px,py,pz=record['position_blender'];dx,dy,dz=direction
        record['position_unity']=[-px,pz,-py];record['direction_unity']=[-dx,dz,-dy]
    (SOURCE/'lighting.json').write_text(json.dumps(LIGHTS,indent=2))
    (SOURCE/'metrics.json').write_text(json.dumps({'triangles_total_render_meshes':triangles,'mesh_objects':len(meshes),'unique_meshes':len({o.data.name for o in meshes}),'materials':len(g.M),'textures':len(list((OUT/'Textures').glob('*.png'))),'shared_sizes':S,'pilot':opt.pilot},indent=2))
    scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE'
    scene.render.resolution_x=opt.width;scene.render.resolution_y=round(opt.width*941/1672);scene.render.resolution_percentage=100
    scene.render.image_settings.file_format='PNG';scene.render.film_transparent=False
    scene.world.color=(.035,.035,.035)
    scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.09,.065,.12,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.25
    scene.view_settings.view_transform='AgX';scene.view_settings.look='AgX - Medium High Contrast';scene.view_settings.exposure=.6
    if hasattr(scene,'eevee'):
        scene.eevee.taa_render_samples=192
        scene.eevee.shadow_pool_size='1024'
        scene.eevee.shadow_resolution_scale=.75
        if hasattr(scene.eevee,'use_raytracing'):scene.eevee.use_raytracing=True
    # Compositor soft highlight glow, retained in all reference comparisons.
    scene.use_nodes=True
    tree=scene.compositing_node_group if hasattr(scene,'compositing_node_group') else scene.node_tree
    if tree is None:
        tree=bpy.data.node_groups.new('TriviaCompositor','CompositorNodeTree');scene.compositing_node_group=tree
        tree.interface.new_socket(name='Image',in_out='OUTPUT',socket_type='NodeSocketColor')
        output=tree.nodes.new('NodeGroupOutput')
    else:
        tree.nodes.clear();output=tree.nodes.new('CompositorNodeComposite')
    render=tree.nodes.new('CompositorNodeRLayers');glow=tree.nodes.new('CompositorNodeGlare')
    glow.inputs['Type'].default_value='Fog Glow';glow.inputs['Quality'].default_value='High';glow.inputs['Strength'].default_value=.22
    tree.links.new(render.outputs['Image'],glow.inputs['Image']);tree.links.new(glow.outputs['Image'],output.inputs['Image'])
    bpy.ops.object.select_all(action='DESELECT')
    for o in bpy.context.scene.objects:
        if o.type in ('MESH','EMPTY'):
            o.hide_set(False);o.select_set(True)
    fbx=OUT/('SM_Trivia_Podium_Pilot.fbx' if opt.pilot else 'SM_Trivia_Theatre.fbx')
    export_selection(fbx)
    for o in bpy.context.scene.objects:
        if o.name.startswith('COL_'):o.hide_set(True)
    scene.camera=bpy.data.objects.get('CAM_Trivia_Reference_01')
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/('Trivia_Podium_Pilot.blend' if opt.pilot else 'Trivia_Round01.blend')))
    print('TRIVIA_METRICS',triangles,len(meshes),flush=True)
    if opt.render:
        for view in opt.views.split(','):
            scene.camera=bpy.data.objects[f'CAM_Trivia_Reference_{int(view):02}'];scene.render.filepath=str(RENDERS/f'iteration-{opt.iteration}_view-{view}.png');bpy.ops.render.render(write_still=True)

if opt.pilot:
    podium(1,0,0,0)
    light('Area',(0,-3,4),'#FFE2AD',500,(0,0,.7),size=3)
    cameras()
else:
    print('BUILD stage and podiums',flush=True);stage()
    print('BUILD screen, curtains and proscenium',flush=True);screen_and_arch()
    print('BUILD hall, seating and lift',flush=True);hall();cameras()
    light('Area',(0,-3,7.5),'#FFE0B0',1800,(0,1,0),size=5,purpose='Broad warm stage key, bake indirect')
    light('Area',(-7,-6,5),'#FFD6AF',650,(0,1,2),size=4,purpose='Warm camera-left fill')
    light('Area',(6,2,6.8),'#AD83FF',450,(0,0,.6),size=3.5,purpose='Purple rim and wood reflections')
    light('Area',(0,-15,8),'#FFD6AD',1050,(0,-13,0),size=8,purpose='Audience fill, preserve stage contrast')
    light('Area',(0,-23,8),'#DFA778',450,(0,-20,2),size=6,purpose='Upper balcony soft fill')
    light('Area',(-10,-8,7.5),'#FFC78A',1600,(-13,-2,2.5),size=5,purpose='Lift and side balcony amber key')
    light('Area',(-9,-5,4),'#FFDDAA',500,(-13,-2,1.5),size=3,purpose='Brushed gold lift door reflection key')
    light('Area',(7,-18,7),'#FFC78A',950,(0,-23,4.5),size=6,purpose='Rear balcony and banner key')
    light('Area',(-7,-18,7),'#FFC78A',950,(0,-23,4.5),size=6,purpose='Rear balcony balancing fill')
finalize()
