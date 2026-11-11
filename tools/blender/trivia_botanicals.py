"""Original rose/camellia, phlox and foliage meshes for the Trivia urns.

Petals use explicit base-to-tip UVs, so surface colour follows the botanical
form rather than a world-space box projection. No external mesh or texture.
"""
import math, random
from mathutils import Vector, Quaternion
from trivia_geometry import mesh, tube, sphere


def surface(name, vertices, faces, coords, material):
    obj=mesh(name,vertices,faces,material)
    uv=obj.data.uv_layers.new(name='UVMap')
    for face in obj.data.polygons:
        face.use_smooth=True
        for loop in face.loop_indices:
            uv.data[loop].uv=coords[obj.data.loops[loop].vertex_index]
    return obj


def flower(center, normal, radius, material, rng, full=True):
    """Overlapping convex whorls; taller inner petals form a closed centre."""
    orient=normal.to_track_quat('Z','Y')
    vertices=[];faces=[];uv=[]
    layers=[(8,1.0,.30),(7,.79,.48),(6,.56,.64),(5,.35,.76),(3,.18,.82)]
    if not full:layers=[(7,1.0,.28),(6,.66,.46),(4,.32,.60)]
    for layer,(count,spread,height) in enumerate(layers):
        phase=rng.random()*math.tau
        for k in range(count):
            angle=k*math.tau/count+phase+rng.uniform(-.12,.12)
            q=orient@Quaternion((0,0,1),angle)
            size=radius*rng.uniform(.91,1.08)
            twist=rng.uniform(-.09,.09);curl=rng.uniform(.09,.16)
            rows=6;cols=5;offset=len(vertices)
            for j in range(rows+1):
                t=j/rows
                for i in range(cols+1):
                    u=i/cols*2-1
                    # Rounded, unequal rims; sides reach a little less far
                    # than the centre, without a pointed daisy-like petal tip.
                    v=t*(.80+.20*math.sqrt(max(0,1-u*u)))
                    theta=u*(math.pi/count*1.58)*(.22+.78*math.sin(v*math.pi/2))
                    radial=size*spread*(.10+.90*math.sin(v*math.pi/2)+curl*v**7)
                    rise=size*(height*math.sin(v*math.pi/2)+.14*math.sin(v*math.pi))
                    rise-=size*spread*.19*v**7*(1-u*u)
                    rise+=size*(twist*u*v+.03*math.sin(u*9+angle)*v**6)
                    vertices.append(center+q@Vector((radial*math.sin(theta),radial*math.cos(theta),rise)))
                    uv.append((.05+.90*i/cols,.05+.9*t))
            for j in range(rows):
                for i in range(cols):
                    a=offset+j*(cols+1)+i
                    faces.append((a,a+1,a+cols+2,a+cols+1))
    petals=surface('SM_Trivia_RoseWhorls',vertices,faces,uv,material)
    subdiv=petals.modifiers.new('Soft petal rims','SUBSURF');subdiv.levels=1
    reduce=petals.modifiers.new('Petal silhouette budget','DECIMATE');reduce.ratio=.30
    reduce.use_collapse_triangulate=True
    # A small bud closes the basal holes without a conspicuous spiral decal.
    sphere('SM_Trivia_RoseBud',center+normal*radius*(.73 if full else .52),
           (radius*.095,)*3,material,12,6)


def phlox(center, normal, radius, material, phase):
    q=normal.to_track_quat('Z','Y');vertices=[];faces=[];uv=[]
    for k in range(5):
        rot=q@Quaternion((0,0,1),phase+k*math.tau/5)
        offset=len(vertices);rows=4;cols=4
        for j in range(rows+1):
            t=j/rows
            for i in range(cols+1):
                u=i/cols*2-1
                width=radius*.60*math.sin(math.pi*t)**.65
                p=Vector((width*u,radius*t,radius*(.18*math.sin(math.pi*t)+.12*u*u*t)))
                vertices.append(center+rot@p);uv.append((i/cols,t))
        for j in range(rows):
            for i in range(cols):
                a=offset+j*(cols+1)+i;faces.append((a,a+1,a+cols+2,a+cols+1))
    surface('SM_Trivia_Phlox',vertices,faces,uv,material)
    sphere('SM_Trivia_PhloxEye',center+normal*.008,(radius*.16,)*3,'PetalCream',8,4)


def leaf(base, direction, length, width, roll):
    axis=direction.normalized();side=axis.cross(Vector((0,0,1))).normalized()
    if side.length<.01:side=Vector((1,0,0))
    up=side.cross(axis).normalized();vertices=[];uv=[];rows=10;cols=4
    for j in range(rows+1):
        t=j/rows
        for i in range(cols+1):
            u=i/cols*2-1
            w=width*math.sin(math.pi*t)**.8*(1+.035*math.sin(t*55))
            p=base+axis*length*t+side*w*u
            p+=up*(length*.12*math.sin(math.pi*t)*(1-abs(u))-.1*length*t*t+roll*u*t)
            vertices.append(p);uv.append((i/cols,t))
    faces=[(j*(cols+1)+i,j*(cols+1)+i+1,(j+1)*(cols+1)+i+1,(j+1)*(cols+1)+i) for j in range(rows) for i in range(cols)]
    obj=surface('SM_Trivia_BotanicalLeaf',vertices,faces,uv,'Leaves')
    shell=obj.modifiers.new('Leaf underside','SOLIDIFY');shell.thickness=.0008


def bouquet(x,y,z,seed):
    rng=random.Random(seed);origin=Vector((x,y,z+1.59))
    # A domed arrangement, anchored at the existing urn. Lower flowers face
    # outward, upper flowers tilt upward. Smaller heads fill the larger gaps.
    positions=[(-.31,-.16,.11),(.01,-.30,.23),(.29,-.16,.14),
               (-.18,.03,.36),(.16,.07,.34),(-.30,.20,.21),(.31,.20,.19),
               (.0,.28,.16),(-.41,.04,-.01),(.41,.01,.0),
               (-.16,-.29,-.01),(.19,-.27,-.01),(-.01,-.03,.40),
               (-.05,.37,-.03),(.01,.02,-.02)]
    heads=[]
    for j,(dx,dy,dz) in enumerate(positions):
        c=Vector((x+dx,y+dy,z+1.90+dz))
        n=Vector((dx*2.7,dy*2.7-.12,.35 if dz<.2 else .60)).normalized()
        rad=rng.uniform(.135,.175) if j<8 else rng.uniform(.105,.13)
        color=['PetalCream','PetalPurple','PetalPurple','PetalCream','PetalPurple','PetalWhite','PetalPurple','PetalWhite'][j%8]
        flower(c,n,rad,color,rng,full=j<8)
        tube('SM_Trivia_FlowerStem',[origin,origin.lerp(c,.5)+Vector((dx*.08,dy*.08,0)),c],.006,'Leaves',sides=5)
        heads.append((c,rad))
    # Small clustered phlox sits among the roses rather than on long stalks.
    for j in range(15):
        a=j*2.399+rng.uniform(-.18,.18);r=rng.uniform(.30,.49)
        base=Vector((x+math.cos(a)*r,y+math.sin(a)*r,z+1.91+rng.uniform(.02,.35)))
        for c,rad in heads:
            delta=base-c
            if .001<delta.length<rad*1.12:base+=delta.normalized()*(rad*1.12-delta.length)
        n=Vector((math.cos(a),math.sin(a),.8)).normalized()
        path=[origin.lerp(base,k/8)+Vector((0,0,.055*math.sin(k/8*math.pi))) for k in range(9)]
        tube('SM_Trivia_FlowerSpray',path,.003,'Leaves',sides=4)
        q=n.to_track_quat('Z','Y')
        for k in range(3):
            angle=a+k*math.tau/3
            c=base+q@Vector((.04*math.cos(angle),.04*math.sin(angle),rng.uniform(0,.025)))
            phlox(c,n,rng.uniform(.035,.055),'PetalPurple' if j%3 else 'PetalWhite',rng.random()*math.tau)
    for j in range(62):
        a=j*2.399+rng.uniform(-.2,.2)
        h=rng.uniform(.04,.17) if j<40 else rng.uniform(.23,.42)
        base=origin+Vector((.16*math.cos(a),.16*math.sin(a),h))
        direction=Vector((math.cos(a),math.sin(a),rng.uniform(-.3,.65)))
        leaf(base,direction,rng.uniform(.17,.28),rng.uniform(.035,.055),rng.uniform(-.02,.02))
