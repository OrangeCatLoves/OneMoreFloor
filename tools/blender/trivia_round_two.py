"""Owner's 2026-11-11 feedback. Geometry changes are limited to its nine items.

Coordinates in this module use Unity metres; U converts to the source basis.
"""
import bpy, math
from mathutils import Vector, Matrix
from trivia_geometry import *
LIVE_WIDTH=12.15
LIVE_HEIGHT=4.10

def U(x,y,z): return Vector((-x,-z,y))

def remove(*prefixes):
    for o in list(bpy.data.objects):
        if any(o.name.startswith(p) for p in prefixes): bpy.data.objects.remove(o,do_unlink=True)

def block(name, center, size, material='Walnut', collision=False, bevel=0):
    o=box('SM_Trivia_'+name,U(*center),(size[0],size[2],size[1]),material,bevel)
    if collision: collider('Trivia_'+name,U(*center),(size[0],size[2],size[1]))
    return o

def shift(o,delta):
    o.location+=U(*delta)

def turn(o,center,angle):
    bpy.context.view_layer.update()
    m=Matrix.Translation(center)@Matrix.Rotation(angle,4,'Z')@Matrix.Translation(-Vector(center))
    o.matrix_world=m@o.matrix_world

def slab(name,outline,top,depth=.2,material='Walnut',collision=False):
    # outline in Unity XZ, counter-clockwise viewed from above in source XY.
    pts=[(-x,-z) for x,z in outline]
    area=sum(pts[i][0]*pts[(i+1)%len(pts)][1]-pts[(i+1)%len(pts)][0]*pts[i][1] for i in range(len(pts)))
    if area<0:pts.reverse()
    n=len(pts);v=[(x,y,h) for h in [top-depth,top] for x,y in pts]
    f=[tuple(reversed(range(n))),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    o=mesh('SM_Trivia_'+name,v,f,material,True)
    if collision:
        c=mesh('COL_Trivia_'+name,v,f,None);c.hide_render=True;c.hide_set(True)
    return o

def rounded_screen(name,y,w,h):
    pts=rounded_rect(0,y,5.4,w,h,.65,16)
    o=mesh(name,pts,[tuple(range(len(pts)))],'Screen')
    uv=o.data.uv_layers.new(name='UVMap')
    for loop in o.data.loops:
        v=o.data.vertices[loop.vertex_index].co
        uv.data[loop.index].uv=(v.x/w+.5,(v.z-5.4)/h+.5)
    return o

def ties():
    import trivia_refinement as detail
    from mathutils.geometry import convex_hull_2d
    remove('SM_Trivia_GoldTieback','SM_Trivia_Tassel')
    for side in [-1,1]:
        name='L' if side<0 else 'R';cx=side*9.51;cy=1.8;zz=3.12
        cloth=next(o for o in bpy.data.objects if o.name.startswith('SM_Trivia_TiedDrape') and 'Fringe' not in o.name and 'Hem' not in o.name and (sum(v.co.x for v in o.data.vertices)/len(o.data.vertices))*side>0)
        bpy.context.view_layer.update();cross=[]
        for edge in cloth.data.edges:
            a,b=[cloth.matrix_world@cloth.data.vertices[i].co for i in edge.vertices]
            for plane in [zz-.035,zz,zz+.035]:
                if (a.z-plane)*(b.z-plane)<0:
                    p=a.lerp(b,(plane-a.z)/(b.z-a.z));p.z=zz;cross.append(p)
        hull=[cross[i] for i in convex_hull_2d([Vector((v.x,v.y)) for v in cross])]
        center=sum(hull,Vector())/len(hull)
        if sum(p.x*hull[(i+1)%len(hull)].y-p.y*hull[(i+1)%len(hull)].x for i,p in enumerate(hull))<0:hull.reverse()
        inflated=[]
        for i,p in enumerate(hull):
            a=(p-hull[i-1]).normalized();b=(hull[(i+1)%len(hull)]-p).normalized()
            na=Vector((a.y,-a.x,0));nb=Vector((b.y,-b.x,0));bisect=(na+nb).normalized()
            inflated.append(p+bisect*(.026/max(.1,bisect.dot(na))))
        hull=inflated
        # Closed braided loop embraces the actual pinched cloth cross section.
        for strand in range(3):
            pts=[]
            for edge,p in enumerate(hull):
                for i in range(8):
                    t=i/8;v=p.lerp(hull[(edge+1)%len(hull)],t);phase=(edge+t)*math.tau*2+strand*math.tau/3
                    radial=(v-center).normalized();v+=radial*.006*math.cos(phase);v.z+=.006*math.sin(phase);pts.append(v)
            tube('SM_Trivia_GoldTieback_'+name,pts,.012,'Gold',True,sides=6)
        # Fixed boss on the proscenium, with two cord ends tied at the front.
        sphere('SM_Trivia_TiebackBoss_'+name,(side*10.25,1.73,zz),(.12,.08,.12),'GoldDark',16,8)
        outer=max(hull,key=lambda p:p.x*side)
        tube('SM_Trivia_TiebackAnchor_'+name,[outer,(side*10.03,1.64,zz),(side*10.25,1.67,zz)],.024,'Gold')
        front=min(hull,key=lambda p:p.y);cx=front.x;cy=front.y+.175
        knot=(cx,cy-.22,zz)
        sphere('SM_Trivia_TiebackKnot_'+name,knot,(.068,.043,.050),'Gold',16,8)
        tube('SM_Trivia_TasselCord_'+name,[knot,(cx,cy-.36,zz-.16),(cx,cy-.42,zz-.22)],.022,'Gold')
        detail.tassel(cx,cy-.42,zz-.22)

def stage_layout(S):
    # Rotate each complete podium assembly about its existing body centre.
    for i,x in enumerate([-4.8,-1.6,1.6,4.8],1):
        prefixes=(f'SM_Trivia_Podium_{i:02}',f'SM_Trivia_PodiumScreen_{i:02}',f'COL_Trivia_Podium_{i:02}')
        for o in list(bpy.data.objects):
            if o.name.startswith(prefixes):
                turn(o,(x,-2.5,.6),math.pi)
                if '_Button_' in o.name:
                    d=list(o['press_direction_unity']);o['press_direction_unity']=[-d[0],d[1],-d[2]]
        marker=bpy.data.objects[f'MK_Objective_{i:02}'];marker.location=(x,-1.45,.6)
        # FBX empty local +Y is Unity +Z (exporter basis conversion).
        marker.rotation_euler.z=0
        for name in [f'MK_Light_Point_{i:02}',f'Light_Point_{i:02}']:
            if name in bpy.data.objects:turn(bpy.data.objects[name],(x,-2.5,.6),math.pi)
    for name in ['SM_Trivia_HostCompass','SM_Trivia_HostCompassRing','MK_Host']:
        shift(bpy.data.objects[name],(-6.4,0,3.6))
    bpy.data.objects['MK_Host'].rotation_euler.z=math.radians(-65)
    remove('SM_Trivia_StageStep','SM_Trivia_StepLight','COL_Trivia_Step')
    sw=S['stairs']['width'];sd=S['stairs']['stepDepth'];sh=S['stairs']['stepHeight']
    # A corner landing joins only the area outside the existing curved stage.
    x0=7.4;x1=x0+sw;front=4.6
    curve=[(x, .6+5*math.sqrt(max(0,1-(x/10.7)**2))) for x in [x0+(x1-x0)*i/40 for i in range(41)]]
    # Cover the old plank ends with a flush curved junction, hiding their coarse
    # row clipping. A 2 mm height separation prevents coplanar flicker.
    slab('StageLanding',[(x,z-.35) for x,z in curve]+[(x1,front),(x0,front)],.602,.602,'StageWood')
    # Convex collider strips follow the concave union seam, without blocking the lift landing.
    for i in range(20):
        xa=x0+(x1-x0)*i/20;xb=x0+(x1-x0)*(i+1)/20
        za=.6+5*math.sqrt(1-(xa/10.7)**2);zb=.6+5*math.sqrt(1-(xb/10.7)**2)
        pts=[(xa,za),(xb,zb),(xb,front),(xa,front)]
        o=slab('LandingCollision',pts,.6,.6);o.name=f'COL_Trivia_StageLanding_{i:02}';o.hide_render=True;o.hide_set(True)
    for i in range(3):
        z=front+(3-i-.5)*sd;top=sh*(i+1)
        block('StageStep',((x0+x1)/2,top/2,z),(sw,top,sd),'Walnut',True,.012)
        tube('SM_Trivia_StepLight',[U(x0+.08,top-.045,z+sd/2+.006),U(x1-.08,top-.045,z+sd/2+.006)],.012,'Bulb')
    # The main red aisle now joins a front cross-aisle leading to the side flight.
    block('FrontCrossCarpet',(4.45,.008,6.5),(11.9,.012,1.6),'VelvetRed')
    for z in [5.72,7.28]:block('FrontCrossBorder',(4.45,.017,z),(11.9,.006,.024),'GoldDark')
    # The old curved fascia must stop at the new landing, not run across it.
    import bmesh
    for o in list(bpy.data.objects):
        if not o.name.startswith(('SM_Trivia_StageApronEdge','SM_Trivia_ApronFlute')):continue
        bm=bmesh.new();bm.from_mesh(o.data)
        faces=[f for f in bm.faces if -10.43<(o.matrix_world@f.calc_center_median()).x<-7.37]
        bmesh.ops.delete(bm,geom=faces,context='FACES');bm.to_mesh(o.data);bm.free()
    # Existing lift urns move against the wing walls, clear of the gathering area.
    for o in bpy.data.objects:
        if o.name.startswith(('SM_Trivia_FlowerUrn','COL_Trivia_Plinth')) and o.location.x<-11:
            shift(o,(-.1,0,-1.2 if o.location.y>0 else 2.1))

def television():
    # Housing local source front is -Y, then rotated to face the players (+Y).
    center=U(0,1.345,5.32)
    rotation=Matrix.Rotation(math.pi,4,'Z')@Matrix.Rotation(math.radians(-25),4,'X')
    w=1.4;h=w/(LIVE_WIDTH/LIVE_HEIGHT)
    begin=set(bpy.data.objects)
    box('SM_Trivia_PlayerTVHousing',(0,0,0),(w+.12,.16,h+.12),'BlackMetal',.04)
    live=panel('SM_Trivia_PlayerScreen',0,-.088,0,w,h,'Screen')
    tube('SM_Trivia_PlayerTVBezel',rounded_rect(0,-.094,0,w+.085,h+.085,.05,8),.012,'GoldDark',True)
    for o in set(bpy.data.objects)-begin:
        # Preserve UV up/right while the front normal faces backstage and up.
        o.matrix_world=Matrix.Translation(center)@rotation@o.matrix_world
    block('PlayerTVFoot',(0,.65,5.32),(1.0,.10,.40),'BlackMetal',True,.025)
    block('PlayerTVStand',(0,.9725,5.42),(.12,.645,.12),'BlackMetal',True,.015)
    hinge=cylinder('SM_Trivia_PlayerTVTiltJoint',U(0,1.285,5.395),.055,.20,'GoldDark',16);hinge.rotation_euler.y=math.pi/2
    block('PlayerTVGuard',(0,1.345,5.32),(1.52,.62,.42),'BlackMetal',False)
    o=bpy.data.objects['SM_Trivia_PlayerTVGuard'];o.name='COL_Trivia_PlayerTV';o.hide_render=True;o.hide_set(True)
    tube('SM_Trivia_PlayerTVCable',[U(.3,1.175,5.33),U(.2,.8,5.45),U(.1,.68,5.4),U(.1,.61,5.4)],.012,'BlackMetal')
    cylinder('SM_Trivia_PlayerTVCableGland',U(.1,.609,5.4),.035,.018,'BlackMetal',12)

def floor():
    remove('SM_Trivia_HallFloor','COL_Trivia_HallFloor')
    # Three disjoint rectangles end at the lift sill. Their top faces never overlap.
    block('HallFloor',(-.75,-.12,10),(27.5,.24,32),'Walnut',True)
    block('HallFloor',(13.75,-.12,-3.25),(1.5,.24,5.5),'Walnut',True)
    block('HallFloor',(13.75,-.12,15.25),(1.5,.24,21.5),'Walnut',True)
    block('LiftThreshold',(13,.012,2),(.18,.024,3.0),'GoldDark',True,.004)

def seating(S,seat_template,rail):
    remove('SM_Trivia_Seat_R','SM_Trivia_UpperSeat','SM_Trivia_SeatTier','COL_Trivia_SeatingTier','SM_Trivia_Aisle','COL_Trivia_Aisle')
    # Seven 2 m-pitch rows, with independent row passages and three access aisles.
    # Far mesh is shared by every seat; visible silhouette and materials retained.
    far=seat_template(distant=True)
    for row in range(7):
        z=8.3+row*2.0;top=row*S['stairs']['stepHeight']
        if row>0:block('SeatTier',(0,top/2,z),(28.4,top,2.0),'Walnut',True)
        block('AisleCarpet',(0,top+.008,z),(S['stairs']['width'],.012,2.0),'VelvetRed')
        for x in [-1.39,1.39]:block('AisleBorder',(x,top+.017,z),(.025,.006,2.0),'Gold')
        for side in [-1,1]:
            for col in range(9):
                x=side*(2.15+col*.86)
                clone(far,f'SM_Trivia_Seat_R{row:02}_{-side}_{col:02}',U(x,top,z))
            block('SeatingTier',(side*5.59,top+.63,z),(7.73,1.26,.72),'Walnut',False)
            o=bpy.data.objects.get('SM_Trivia_SeatingTier')
            # Rename immediately: collision only, no invisible wall in row passage.
            o.name='COL_Trivia_SeatingTier';o.hide_render=True;o.hide_set(True)
    block('RearCrossAisle',(0,.6,23.55),(28.4,1.2,4.5),'Walnut',True)
    for side in [-1,1]:
        for col in range(9):clone(far,f'SM_Trivia_UpperSeat_{-side}_0_{col}',U(side*(1.8+col*.86),4.8,23.1))
        collider('Trivia_UpperSeating',U(side*5.24,5.43,23.1),(7.73,.72,1.26))
    bpy.data.objects.remove(far,do_unlink=True)
    # End the front-row divider at the seating block so both side aisles open.
    bpy.context.view_layer.update()
    for o in list(bpy.data.objects):
        if not o.name.startswith(('SM_Trivia_Handrail','SM_Trivia_Baluster')):continue
        p=[o.matrix_world@Vector(v) for v in o.bound_box]
        if max(v.z for v in p)<1.1 and abs(sum(v.y for v in p)/8+7.5)<.1:bpy.data.objects.remove(o,do_unlink=True)
    for side in [-1,1]:
        rail([(side*(2+i*7.4/20),-7.5) for i in range(21)],1,0)
        collider('Trivia_FrontRowGuard',U(side*5.7,.5,7.5),(7.4,.07,1))
    # Repair the balcony: lowered fascia and guards, continuous structural floor,
    # no unsupported second row or obstructing side chairs.
    bpy.context.view_layer.update()
    for o in bpy.data.objects:
        if o.name.startswith(('SM_Trivia_BalconyFascia','SM_Trivia_BalconyMoulding','SM_Trivia_BalconyBulb')):o.location.z-=.85
        if o.name.startswith('SM_Trivia_BalconyFascia'):
            o.data=o.data.copy()
            for v in o.data.vertices:
                if abs(v.co.x)>12.49 and v.co.y>=-10:v.co.x*=12.488/12.5
        if o.name.startswith('SM_Trivia_SidePanelVertical'):o.location.x-=math.copysign(.006,o.location.x)
        if o.name.startswith(('SM_Trivia_Handrail','SM_Trivia_Baluster')):
            if min((o.matrix_world@Vector(v)).z for v in o.bound_box)>4.7:
                o.location.z-=.6
                if o.name.startswith('SM_Trivia_Baluster') and 'Collar' not in o.name:
                    height=max(v.co.z for v in o.data.vertices)-min(v.co.z for v in o.data.vertices)
                    o.scale.z=1.1/height;o.location.z=5.35
        if o.name.startswith(('SM_Trivia_Shield','SM_Trivia_BannerLaurel','SM_Trivia_CrestStar')):
            p=sum((o.matrix_world@Vector(v) for v in o.bound_box),Vector())/8
            if p.y<-3 and p.z>4.9:o.location.z-=1.3
        if o.name.startswith('SM_Trivia_Baluster') and 'Collar' not in o.name and o.location.z<1:
            height=max(v.co.z for v in o.data.vertices)-min(v.co.z for v in o.data.vertices)
            o.scale.z=1/height;o.location.z=.5
    remove('SM_Trivia_BalconyDeck','COL_Trivia_BalconyDeck','SM_Trivia_BackBalconyDeck','COL_Trivia_BackBalconyDeck')
    # Polygon ring, tiled into convex strips and split around both stair openings.
    for side in [-1,1]:
        levels=sorted(set([-1+j*.24 for j in range(112)]+[19,22.5,24.4]))
        for a,b in zip(levels,levels[1:]):
            def inner(z):return 12.5 if z<=10 else math.sqrt(max(0,12.5**2-(z-10)**2))
            ia,ib=inner(a),inner(b)
            # Slot is x=10.95..13.95, z=19..24.4. Tile limits land exactly there.
            for lo,hi in ([(0,10.95),(13.95,14.4)] if a<24.4 and b>19 else [(0,14.4)]):
                xa=max(lo,ia);xb=max(lo,ib)
                if max(xa,xb)>=hi:continue
                slab('BalconyDeck',[(side*xa,a),(side*hi,a),(side*hi,b),(side*xb,b)],4.8,.2,'Walnut',True)
        # 18 data-sized treads rise from rear cross-aisle to gallery.
        count=round((4.8-1.2)/S['stairs']['stepHeight'])
        for i in range(count):
            top=1.2+(i+1)*S['stairs']['stepHeight'];z=24.4-(i+.5)*S['stairs']['stepDepth']
            block('GalleryStep',(side*12.45,(1.2+top)/2,z),(S['stairs']['width'],top-1.2,S['stairs']['stepDepth']),'Walnut',True)
            tube('SM_Trivia_GalleryStepNosing',[U(side*10.99,top+.004,z+.14),U(side*13.91,top+.004,z+.14)],.008,'GoldDark')
        for x in [10.95,13.95]:
            tube('SM_Trivia_GalleryStairRail',[U(side*x,2.3,24.4),U(side*x,5.9,19)],.035,'Gold')
            for i in range(7):
                z=24.4-i*.9;h=1.2+i*.6
                tube('SM_Trivia_GalleryStairPost',[U(side*x,h,z),U(side*x,h+1.1,z)],.025,'GoldDark')
            # Guard at upper-floor edge of the void; handrail alone is not a barrier.
            rail([(-side*x,-(19+i*5.4/30)) for i in range(31)],5.9,4.8)
            collider('Trivia_GalleryVoidGuard',U(side*x,5.35,21.7),(.06,5.4,1.1))
        # Stair landing clear of side wall fixtures, upper gallery continues to stage.
    # Side boxes projected through the narrow gallery. Remove that pair only;
    # wing boxes remain. Their existing lanterns stay as practical wall lights.
    bpy.context.view_layer.update()
    for o in list(bpy.data.objects):
        if o.name.startswith('COL_Trivia_SideTheatreBox'):bpy.data.objects.remove(o,do_unlink=True);continue
        if o.name.startswith(('SM_Trivia_WallLantern','MK_Light_Point','Light_Point')):
            pos=o.matrix_world.translation
            if o.type=='MESH':pos=sum((o.matrix_world@Vector(v) for v in o.bound_box),Vector())/8
            if abs(pos.x)>12.5 and -8.1<pos.y<-4.7 and 5<pos.z<7:
                o.location.x+=math.copysign(.1,pos.x);o.location.z+=1.4
        if o.type=='MESH' and o.name.startswith(('SM_Trivia_Box','SM_Trivia_Shield','SM_Trivia_Banner','SM_Trivia_Swag','SM_Trivia_Handrail','SM_Trivia_Baluster','SM_Trivia_Acanthus')):
            p=[o.matrix_world@Vector(v) for v in o.bound_box]
            if min(abs(v.x) for v in p)>12.55 and -8.1<sum(v.y for v in p)/8<-4.7:
                bpy.data.objects.remove(o,do_unlink=True)
    # Removing the projecting boxes leaves their lantern bosses short of the wall.
    # Shallow backing blocks bridge that gap without entering the gallery route.
    for side in [-1,1]:
        for z in [5.05,7.75]:
            block('GalleryLanternMount',(side*14.28,7.14,z),(.30,.55,.36),'Ivory',False,.01)
    # Walkable galleries need physical edge protection, not just decorative rails.
    for side in [-1,1]:collider('Trivia_GalleryFrontGuard',U(side*12.5,5.35,4.5),(.07,11,1.1))
    for side in [-1,1]:
        rail([(-side*(12.5+i*1.9/8),1) for i in range(9)],5.9,4.8)
        collider('Trivia_GalleryEndGuard',U(side*13.45,5.35,-1),(1.9,.07,1.1))
    for i in range(48):
        a=math.pi+i*math.pi/48;b=math.pi+(i+1)*math.pi/48
        p=Vector((12.5*math.cos(a),-10+12.5*math.sin(a),5.35));q=Vector((12.5*math.cos(b),-10+12.5*math.sin(b),5.35))
        o=collider('Trivia_GalleryCurvedGuard',(p+q)/2,((q-p).length+.01,.07,1.1));o.rotation_euler.z=math.atan2(q.y-p.y,q.x-p.x)

def functions(light):
    # Rear control position, seated equipment at waist height, working aisle behind.
    block('ControlDesk',(0,2.10,23.8),(4.2,.14,.8),'Walnut',True,.035)
    for x in [-1.7,1.7]:block('ControlRack',(x,1.62,23.8),(.65,.84,.66),'BlackMetal',True,.025)
    for x in [-1.2,0,1.2]:
        block('ControlMonitorStand',(x,2.3,23.95),(.06,.35,.06),'BlackMetal')
        block('ControlMonitor',(x,2.56,23.95),(.8,.48,.08),'BlackMetal',False,.025)
        # Neutral equipment display, no invented text or game UI.
        block('ControlDisplay',(x,2.56,24.0),(.73,.40,.008),'Monitor')
    for i in range(16):
        block('ControlFaderTrack',(-.85+i*.115,2.179,23.59),(.012,.008,.18),'BlackMetal')
        block('ControlFader',(-.85+i*.115,2.19,23.59+(i%4-1.5)*.025),(.05,.015,.025),'GoldDark',False,.003)
    block('ControlCableDuct',(0,1.97,24.15),(3.8,.06,.08),'BlackMetal')
    # Fixed camera stations outside the 1.5 m-wide side audience aisles.
    for side in [-1,1]:
        x=side*12.8;z=12.3;h=.4
        block('CameraStationFoot',(x,h+.06,z),(.8,.12,.8),'BlackMetal',True,.05)
        cylinder('SM_Trivia_CameraPedestal',U(x,h+.75,z),.065,1.4,'BlackMetal',16)
        block('BroadcastCamera',(x,h+1.62,z),(.32,.34,.65),'BlackMetal',False,.04)
        lens=cylinder('SM_Trivia_BroadcastLens',U(x,h+1.62,z-.4),.115,.22,'BlackMetal',20);lens.rotation_euler.x=math.pi/2
        block('CameraStationGuard',(x,h+.8,z),(.9,1.6,1.1),'BlackMetal')
        o=bpy.data.objects['SM_Trivia_CameraStationGuard'];o.name='COL_Trivia_CameraStation';o.hide_render=True;o.hide_set(True)
        # Side wall illumination has a regular rhythm and lights circulation.
        for z in [10,15,20]:
            block('AisleSconceBack',(side*14.33,3.4,z),(.18,.65,.28),'GoldDark',False,.025)
            block('AisleSconceDiffuser',(side*14.20,3.4,z),(.12,.46,.22),'Bulb',False,.02)
            light('Point',U(side*13.98,3.4,z),'#FFD3A0',45,size=.25,purpose=f'Side aisle wall sconce, Unity x={side*14.2}, z={z}; diffuse practical, bake')
    light('Area',U(0,4.35,23.8),'#FFD9B3',180,U(0,1.4,23.3),size=3,purpose='Rear control working light; bake, below balcony')

def apply_round_two(S,seat_template,rail,light):
    remove('SM_Trivia_QuestionScreen')
    rounded_screen('SM_Trivia_QuestionScreen',4.245,LIVE_WIDTH,LIVE_HEIGHT)
    ties();stage_layout(S);television();floor();seating(S,seat_template,rail);functions(light)
    for i in range(1,13):
        base=bpy.data.objects[f'SM_Trivia_Spotlight_{i:02}_Base']
        if i>=9:
            base.location.z=.66
            collider(f'Trivia_Spotlight_{i:02}_Base',(base.location.x,base.location.y,1.01),(.62,.62,.82))
        else:
            x=base.location.x
            tube('SM_Trivia_LampMount',[(x,1,8.2),(x,1.8,8.2),(x,1.8,8.45)],.035,'BlackMetal')
            clamp=cylinder('SM_Trivia_TrussClamp',(x,1.8,8.45),.055,.08,'BlackMetal',12);clamp.rotation_euler.y=math.pi/2
    for x in [-8.5,8.5]:tube('SM_Trivia_TrussHanger',[(x,1.8,8.45),(x,1.8,9.18)],.03,'BlackMetal')
    bpy.context.view_layer.update();seen=set()
    for o in list(bpy.data.objects):
        if not o.name.startswith(('SM_Trivia_Baluster','SM_Trivia_BalconyBulb')):continue
        base=o.name.split('.')[0]
        points=[o.matrix_world@Vector(v) for v in o.bound_box]
        key=(base,tuple(round(f(p[i] for p in points),5) for f in [min,max] for i in range(3)))
        if key in seen:bpy.data.objects.remove(o,do_unlink=True)
        else:seen.add(key)
    # Static non-convex collision supports holes and stair treads. Merge new
    # structural strips by role to avoid hundreds of redundant physics actors.
    for prefix in ['COL_Trivia_BalconyDeck','COL_Trivia_StageLanding','COL_Trivia_GalleryStep','COL_Trivia_GalleryCurvedGuard','COL_Trivia_GalleryVoidGuard','COL_Trivia_GalleryFrontGuard','COL_Trivia_GalleryEndGuard']:
        parts=[o for o in bpy.data.objects if o.name.startswith(prefix)]
        for o in parts:o.hide_set(False)
        o=join_objects(parts,prefix)
        if o:o.hide_render=True;o.hide_set(True)
