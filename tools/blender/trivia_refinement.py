"""Hand-shaped parametric cloth, botanical forms and carved theatre details.

All meshes are original project artwork. Local RNGs keep unrelated geometry,
interaction pivots and the existing stage plank layout deterministic.
"""
import math
import bpy
from mathutils import Vector, Quaternion
from trivia_geometry import mesh, tube, sphere, cylinder, box, apply, join_objects, rounded_rect
from trivia_botanicals import bouquet


def smooth(o):
    for face in o.data.polygons: face.use_smooth = True
    return o


def lathe(name, center, profile, material, segments=32, flutes=0):
    x,y,z=center
    verts=[]
    for radius,height in profile:
        for i in range(segments):
            a=i*math.tau/segments
            r=radius*(1+.045*math.cos(a*flutes)) if flutes else radius
            verts.append((x+r*math.cos(a),y+r*math.sin(a),z+height))
    faces=[(j*segments+i,j*segments+(i+1)%segments,(j+1)*segments+(i+1)%segments,(j+1)*segments+i)
           for j in range(len(profile)-1) for i in range(segments)]
    return smooth(mesh(name,verts,faces,material,True))


def curtain(name,x0,x1,y,bottom,top,pull=0):
    width=x1-x0; folds=max(5,round(width/.48)); nx=folds*10; nz=36
    verts=[]
    side=1 if x0+x1>0 else -1
    for j in range(nz+1):
        v=j/nz
        gather=math.exp(-((v-.30)/.19)**2) if pull else 0
        for i in range(nx+1):
            u=i/nx
            # Unequal pleat spacing, diagonal tension at the tie, and a relaxed
            # lower hem. Broad folds carry the cloth's weight; small folds nest.
            phase=(u+.013*math.sin(u*19)+.007*math.sin(u*47))*folds*math.tau
            phase+=.35*math.sin(v*4.3+u*8)+.12*math.sin(v*11+u*21)
            xx=x0+width*u
            if pull:
                anchor=(x0+x1)/2+side*width*.26
                xx=xx*(1-.76*gather)+anchor*.76*gather
                xx+=side*.08*math.sin(math.pi*v)*math.sin(math.pi*u)
            amp=(.18+.065*math.sin(u*13+.8)**2)*(1-.62*gather)
            yy=y+amp*(math.cos(phase)+.23*math.cos(2*phase+.4))
            yy-=.24*math.sin(math.pi*v)**2*(1-gather) if pull else .055*math.sin(v*6+u*7)
            zz=bottom+(top-bottom)*v
            zz+=(.055*math.sin(u*15+.7)+.023*math.cos(phase))*(1-v)**5
            zz-=.07*math.sin(math.pi*u)**2*math.sin(math.pi*v)
            verts.append((xx,yy,zz))
    faces=[(j*(nx+1)+i,j*(nx+1)+i+1,(j+1)*(nx+1)+i+1,(j+1)*(nx+1)+i) for j in range(nz) for i in range(nx)]
    o=smooth(mesh(name,verts,faces,'VelvetRed'))
    uv=o.data.uv_layers.new(name='UVMap')
    for face in o.data.polygons:
        for li in face.loop_indices:
            vi=o.data.loops[li].vertex_index
            uv.data[li].uv=((vi%(nx+1))/nx*width*1.35,(vi//(nx+1))/nz*(top-bottom)*1.35)
    hem=verts[:nx+1]
    # A real woven band instead of three floating round wires.
    band=[(a,b-.018,c+dz) for dz in [.035,.14] for a,b,c in hem]
    smooth(mesh(name+'_Fringe',band,[(i,i+1,nx+i+2,nx+i+1) for i in range(nx)],'GoldDark',True))
    for dz in [.035,.14]:tube(name+'_HemBraid',[(a,b-.024,c+dz) for a,b,c in hem],.012,'Gold',sides=5)
    for i in range(0,nx,3):
        a,b,c=hem[i]
        tube(name+'_FringeThread',[(a,b-.028,c+.04),(a+.007,b-.03,c+.005)],.005,'Gold',sides=4)
    return o


def swag(x,y,z,width,drop):
    nx=48;nv=12;verts=[]
    for j in range(nv+1):
        v=j/nv
        for i in range(nx+1):
            u=i/nx;s=math.sin(math.pi*u)
            verts.append((x+(u-.5)*width,y-.06*s-.045*math.sin(v*math.pi*6)*s,z-drop*s-(.20+.20*s)*v))
    smooth(mesh('SM_Trivia_BoxSwag',verts,[(j*(nx+1)+i,j*(nx+1)+i+1,(j+1)*(nx+1)+i+1,(j+1)*(nx+1)+i) for j in range(nv) for i in range(nx)],'VelvetRed',True))
    tube('SM_Trivia_BoxSwagBraid',[(a,b-.02,c) for a,b,c in verts[-nx-1:]],.017,'GoldDark',sides=6)


def tassel(x,y,z,size=1):
    lathe('SM_Trivia_TasselKnot',(x,y,z),[(.025,.03),(.095,0),(.11,-.09),(.08,-.16),(.09,-.19)],'Gold',32,12)
    profile=[(.07,-.18),(.09,-.23),(.10,-.36),(.135,-.65),(.16,-.71),(.15,-.74)]
    lathe('SM_Trivia_TasselSkirt',(x,y,z),[(r*size,h*size) for r,h in profile],'GoldDark',40,20)
    for j in range(28):
        a=j*math.tau/28
        tube('SM_Trivia_TasselThread',[(x+(r+.008)*size*math.cos(a),y+(r+.008)*size*math.sin(a),z+h*size) for r,h in profile[1:]],.008*size,'Gold',sides=5)
    for h,r in [(-.17,.09),(-.22,.094)]:
        tube('SM_Trivia_TasselCollar',[(x+r*math.cos(a*math.tau/32),y+r*math.sin(a*math.tau/32),z+h) for a in range(32)],.015,'Gold',True)


def carved_leaf(name,base,tip,width,material='Gold',depth=.025):
    base=Vector(base);axis=Vector(tip)-base
    across=Vector((axis.z,0,-axis.x)).normalized()
    verts=[];rows=12;cols=6
    for j in range(rows+1):
        t=j/rows
        w=width*math.sin(math.pi*t)**.8*(1+.09*math.sin(t*math.pi*8))
        for k in range(cols+1):
            u=k/cols*2-1
            p=base+axis*t+across*u*w
            p.y-=depth*math.sin(math.pi*t)*(1-.75*abs(u))
            # Fold the lobes and curl the tip, instead of repeating a flat
            # almond cross-section. The shallow side veins are actual relief.
            p.y+=depth*(.55*t**5+.14*abs(u)*math.sin(t*math.pi*6))
            p+=across*(width*.10*math.sin(t*math.pi)*math.sin(base.x*3+base.z))
            verts.append(p)
    return smooth(mesh(name,verts,[(j*(cols+1)+k,j*(cols+1)+k+1,(j+1)*(cols+1)+k+1,(j+1)*(cols+1)+k) for j in range(rows) for k in range(cols)],material,True))


def laurel(cx,y,cz,r,name):
    start=set(bpy.data.objects)
    for side in [-1,1]:
        pts=[]
        for i in range(25):
            a=math.radians(12+i*5.1)
            pts.append((cx+side*r*math.sin(a),y,cz-r*math.cos(a)))
        tube(name+'_Stem',pts,.011*r,'GoldDark',sides=5)
        for i in range(9):
            a=math.radians(22+i*13)
            base=Vector((cx+side*r*math.sin(a),y-.012,cz-r*math.cos(a)))
            tangent=Vector((side*math.cos(a),0,math.sin(a)))
            radial=Vector((side*math.sin(a),0,-math.cos(a)))
            for s in [-1,1]:
                tip=base+(tangent*.23+radial*s*.18)*r
                carved_leaf(name+'_Leaf',base,tip,.072*r,depth=.035*r)
                tube(name+'_Vein',[base,base.lerp(tip,.8)+Vector((0,-.02*r,0))],.004*r,'GoldDark',sides=4)
    return join_objects(list(set(bpy.data.objects)-start),name)


def rosette(x,y,z,r=.16):
    phase=math.sin(x*13+z*7)*.13
    for k in range(8):
        a=k*math.tau/8+phase
        reach=r*(1+.07*math.sin(k*3.7+x*2))
        base=(x+math.sin(a)*r*.12,y,z+math.cos(a)*r*.12)
        tip=(x+math.sin(a)*reach,y,z+math.cos(a)*reach)
        carved_leaf('SM_Trivia_CarvedRosette',base,tip,r*.24,depth=r*.30)
    sphere('SM_Trivia_RosetteBoss',(x,y-r*.16,z),(r*.22,r*.16,r*.22),'GoldDark',10,6)


def fixture_detail(head,index,floor):
    """Lens cluster, cooling ribs and yoke bolts follow the existing tilt pivot."""
    # Mesh coordinates inherit the exact housing tilt, with unchanged pivots.
    start=set(bpy.data.objects)
    angle=math.radians(30 if floor else -22)
    transform=Quaternion((1,0,0),angle)
    for z in [-.12,-.06,0,.06]:
        pts=[transform@Vector((.214*math.cos(i*math.tau/32),.214*math.sin(i*math.tau/32),z)) for i in range(32)]
        rib=tube(f'SM_Trivia_Spotlight_{index:02}_Cooling',pts,.012,'GoldDark',True,sides=5);rib.parent=head.parent
    face_z=.252 if floor else -.252
    for k in range(7):
        a=k*math.tau/6;rad=.105 if k<6 else 0
        p=transform@Vector((rad*math.cos(a),rad*math.sin(a),face_z))
        lens=cylinder(f'SM_Trivia_Spotlight_{index:02}_Optic',p,.045,.012,'Bulb',12)
        lens.rotation_euler.x=angle;apply(lens);lens.parent=head.parent
    for side in [-1,1]:
        bolt=sphere(f'SM_Trivia_Spotlight_{index:02}_Bolt',(side*.25,0,0),(.025,.055,.055),'GoldDark',10,6);bolt.parent=head.parent
    join_objects(sorted(set(bpy.data.objects)-start,key=lambda o:o.name),f'SM_Trivia_Spotlight_{index:02}_Details')


def upholstered_back(distant=False):
    n=4 if distant else 10
    for side in [-1,1]:
        verts=[]
        for j in range(n+1):
            v=j/n*2-1
            for i in range(n+1):
                u=i/n*2-1
                bulge=.052*max(0,(1-u*u)*(1-v*v))**.7
                for button in [-.55,0,.55]:
                    bulge-=.018*math.exp(-((u-button)/.13)**2-((v-.20)/.19)**2)
                verts.append((u*.285,-.26+side*(.078+bulge),.88+v*.32))
        faces=[(j*(n+1)+i,j*(n+1)+i+1,(j+1)*(n+1)+i+1,(j+1)*(n+1)+i) for j in range(n) for i in range(n)]
        if side>0:faces=[tuple(reversed(f)) for f in faces]
        smooth(mesh('SM_Trivia_SeatPaddedInsert',verts,faces,'VelvetSeat',True))
        if not distant and side<0:
            tube('SM_Trivia_SeatSeam',rounded_rect(0,-.26+side*.083,.88,.58,.65,.10,5),.004,'GoldDark',True,sides=4)
