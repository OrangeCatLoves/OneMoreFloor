"""Owner's round 3 feedback (19-21 Nov 2026), built by Claude Code.

- The lift moves to the centre of the hall's back wall and opens straight into the hall, facing the stage.
- A gently ramped centre aisle (and two side aisles) lead down from the lift level to the stage.
- Curved steps at both front corners of the stage replace the side landing.
- The galleries stay as audience seating, but players can't reach them: the in-hall stairs go and the
  audience uses doors at the back of the gallery.
- Both front rails go: the guard in front of the first row and the vertical bars along the stage front.
- Thick rope tie-backs, upright screens, a proper back for the players' TV, rows 1.3 m apart,
  chandeliers, a lighter ceiling, and collision that matches what players see.

Coordinates in this module are Unity metres: x across the hall, y up, z from the stage (z < 0) towards
the lift (z = 26). U() converts them to Blender's source axes.
"""
import bpy, bmesh, math
from mathutils import Vector, Matrix
from trivia_geometry import *
import trivia_refinement as detail
from trivia_round_two import U, remove, block, slab

B = 1.2                 # floor height of the rear cross-aisle and the lift
ROWS = 10               # audience rows on each side of the centre aisle
PITCH = 1.3             # metres between rows
ROW0 = 8.3              # first row, seat centre
RISE = B / ROWS         # each row stands 0.12 m above the one in front; the cross-aisle is "row 10"
RAMP_FOOT = ROW0 - PITCH / 2            # 7.65: the aisle ramps start at floor level here
CROSS = ROW0 + PITCH * (ROWS - .5)      # 20.65: the ramps reach the cross-aisle here
WALL = 25.9             # inner face of the rear wall (wall centre z = 26)
SIDE = 14.4             # inner face of the side walls
HEIGHT = 10.5           # ceiling
SEAT_COLS = 10
SEAT_X0, SEAT_DX = 2.15, .86
BLOCK = 10.4            # outer edge of each seating block; the side aisles run from here to the wall
AISLE = 1.5             # half width of the centre aisle (docs/sizes.json stairs width 3.0)
STAGE = .6
APRON = (10.7, 5.0, .6)     # stage front edge: x = a cos t, z = c + b sin t
STEP_X = (3.6, 6.8)         # each corner flight spans this |x| range of the stage front
DOOR_W, DOOR_H = 3.0, 2.5   # lift door, from docs/sizes.json (read in apply_round_three)
CAB = (5.0, 5.0, 3.5)       # lift interior width, depth, height


# ---------------------------------------------------------------- helpers

def ubounds(o):
    """Object bounds in Unity axes: (min, max) vectors."""
    pts = [o.matrix_world @ Vector(c) for c in o.bound_box]
    us = [Vector((-p.x, p.z, -p.y)) for p in pts]
    return (Vector([min(u[i] for u in us) for i in range(3)]),
            Vector([max(u[i] for u in us) for i in range(3)]))

def remove_where(prefixes, test):
    bpy.context.view_layer.update()
    for o in list(bpy.data.objects):
        if o.name.startswith(prefixes) and test(*ubounds(o)):
            bpy.data.objects.remove(o, do_unlink=True)

def solid(name, verts, faces, material, collision=False, smooth_shade=False, render=True):
    """A closed mesh from Unity-space vertices. Faces are turned to wind outward, so colliders
    have positive volume (the Unity validator checks this). render=False makes the collider only."""
    src = [U(*v) for v in verts]
    volume = 0.0
    for f in faces:
        a = src[f[0]]
        for i in range(1, len(f) - 1):
            volume += a.dot(src[f[i]].cross(src[f[i + 1]]))
    if volume < 0:
        faces = [tuple(reversed(f)) for f in faces]
    o = None
    if render:
        o = mesh('SM_Trivia_' + name, src, faces, material, True)
        if smooth_shade:
            for p in o.data.polygons: p.use_smooth = True
    if collision or not render:
        c = mesh('COL_Trivia_' + name, src, faces, None)
        c.hide_render = True; c.hide_set(True)
    return o

def scale_light(LIGHTS, purpose, factor):
    """Brightens (or dims) existing lights by purpose, in the lighting sheet and in Blender."""
    for record in LIGHTS:
        if record['purpose'].startswith(purpose):
            record['watts_blender'] *= factor; record['relative_intensity'] = record['watts_blender'] / 1000
            o = bpy.data.objects.get(record['name'].replace('MK_', ''))
            if o and o.type == 'LIGHT': o.data.energy = record['watts_blender']

def utube(name, pts, radius, material, cyclic=False, sides=6):
    return tube(name, [U(*p) for p in pts], radius, material, cyclic, sides)

def place(objects, matrix):
    """Moves freshly built objects (meshes, markers, lights) by a source-space matrix."""
    bpy.context.view_layer.update()
    for o in objects:
        if o.parent in objects: continue
        if o.type == 'MESH':
            apply(o)
            o.data = o.data.copy()
            world = o.matrix_world.copy()
            o.data.transform(matrix @ world)
            o.matrix_world = Matrix.Identity(4)
        else:
            o.matrix_world = matrix @ o.matrix_world

def built_since(start):
    return [o for o in bpy.data.objects if o not in start]

def remove_lights(LIGHTS, test):
    """Removes lights (marker, light object and lighting-sheet record) whose record passes test."""
    keep = []
    for record in LIGHTS:
        marker = bpy.data.objects.get(record['name'])
        position = marker.matrix_world.translation if marker else Vector(record['position_blender'])
        unity = Vector((-position.x, position.z, -position.y))
        if test(record, unity):
            for n in [record['name'], record['name'].replace('MK_', '')]:
                o = bpy.data.objects.get(n)
                if o: bpy.data.objects.remove(o, do_unlink=True)
        else:
            keep.append(record)
    LIGHTS[:] = keep

def apron_point(t, offset=0.0):
    """Stage front edge at parameter t (0 = stage-right end, pi/2 = centre), pushed out by offset."""
    a, b, c = APRON
    p = Vector((a * math.cos(t), c + b * math.sin(t)))
    n = Vector((b * math.cos(t), a * math.sin(t))).normalized()
    return p + n * offset

def apron_t(x):
    return math.acos(max(-1, min(1, x / APRON[0])))


# ---------------------------------------------------------------- clearing round 2

def clear_round_two(LIGHTS):
    # Old lift on the stage-right side wall, its landing, steps, threshold and markers.
    remove('SM_Trivia_Lift', 'COL_Trivia_Lift', 'MK_Spawn_', 'MK_Exit',
           'SM_Trivia_StageLanding', 'COL_Trivia_StageLanding', 'SM_Trivia_StageStep', 'COL_Trivia_StageStep',
           'SM_Trivia_StepLight', 'SM_Trivia_FrontCrossCarpet', 'SM_Trivia_FrontCrossBorder',
           'SM_Trivia_HallFloor', 'COL_Trivia_HallFloor', 'SM_Trivia_SideWall', 'COL_Trivia_SideWall',
           'SM_Trivia_RearWall', 'COL_Trivia_RearWall', 'SM_Trivia_RearPanel')
    # Lanterns beside the old lift door, and every rear-wall lantern (rebuilt symmetrically).
    remove_where(('SM_Trivia_WallLantern',), lambda lo, hi: (lo.x > 11.5 and -1.5 < lo.z and hi.z < 5.5 and hi.y < 3.6) or (lo.z > 24.9 and lo.y > 6))
    remove_lights(LIGHTS, lambda r, p: r['purpose'].startswith('Amber wall lantern') and ((p.x > 11.5 and -1.5 < p.z < 5.5 and p.y < 3.6) or (p.z > 24.5 and p.y > 6)))
    remove_lights(LIGHTS, lambda r, p: r['purpose'].startswith(('Lift and side balcony amber key', 'Brushed gold lift door reflection key', 'Rear control working light')))
    # Seating, aisles and the rear cross-aisle are rebuilt with closer rows and ramps.
    remove('SM_Trivia_Seat_R', 'SM_Trivia_SeatTier', 'COL_Trivia_SeatTier', 'COL_Trivia_SeatingTier',
           'SM_Trivia_AisleCarpet', 'SM_Trivia_AisleBorder', 'SM_Trivia_RearCrossAisle', 'COL_Trivia_RearCrossAisle',
           'COL_Trivia_FrontRowGuard')
    # Front-row guard rail (owner: "hideous").
    rail_parts = ('SM_Trivia_Handrail', 'SM_Trivia_HandrailBottom', 'SM_Trivia_Baluster', 'SM_Trivia_BalusterCollar')
    remove_where(rail_parts, lambda lo, hi: hi.y < 1.2 and 7.2 < lo.z and hi.z < 7.8)
    # Gallery stairs: players must not reach the galleries.
    remove('SM_Trivia_GalleryStep', 'COL_Trivia_GalleryStep', 'SM_Trivia_GalleryStairRail',
           'SM_Trivia_GalleryStairPost', 'COL_Trivia_GalleryVoidGuard')
    remove_where(rail_parts, lambda lo, hi: lo.y > 4.6 and min(abs(lo.x), abs(hi.x)) > 10.8 and max(abs(lo.x), abs(hi.x)) < 14.1 and lo.z > 18.8 and hi.z < 24.6)
    remove_where(('SM_Trivia_Shield', 'SM_Trivia_ShieldBorder', 'SM_Trivia_CrestStar', 'SM_Trivia_BannerLaurel'),
                 lambda lo, hi: abs((lo.x + hi.x) / 2) < 1.0 and lo.z > 20)
    # Control position and camera stations move off the back centre.
    remove('SM_Trivia_Control', 'COL_Trivia_Control', 'SM_Trivia_CameraStation', 'COL_Trivia_CameraStation',
           'SM_Trivia_CameraPedestal', 'SM_Trivia_BroadcastCamera', 'SM_Trivia_BroadcastLens')
    # Vertical bars along the stage front (owner: "hideous"); the edge trims are rebuilt whole.
    remove('SM_Trivia_ApronFlute', 'SM_Trivia_StageApronEdge')
    # Tie-backs, the players' TV and the lower lower-wall panels are redesigned below.
    remove('SM_Trivia_GoldTieback', 'SM_Trivia_Tieback', 'SM_Trivia_Tassel', 'SM_Trivia_PlayerTV', 'SM_Trivia_PlayerScreen', 'COL_Trivia_PlayerTV')


# ---------------------------------------------------------------- shell: walls, floor, ceiling

def shell():
    wall = .2
    block('HallFloor', (0, -.12, 10), (29, .24, 32), 'Walnut', True)
    for x in [-14.5, 14.5]:
        block('SideWall', (x, HEIGHT / 2, 10), (wall, HEIGHT, 32), 'BurgundyWood', True)
    # Rear wall with the lift opening at the centre of the cross-aisle.
    half = DOOR_W / 2
    for side in [-1, 1]:
        block('RearWall', (side * (14.5 + half) / 2, HEIGHT / 2, 26), (14.5 - half, HEIGHT, wall), 'BurgundyWood', True)
    block('RearWall', (0, (B + DOOR_H + HEIGHT) / 2, 26), (DOOR_W, HEIGHT - B - DOOR_H, wall), 'BurgundyWood', True)
    block('RearWall', (0, (B - .05) / 2, 26), (DOOR_W, B - .05, wall), 'BurgundyWood', True)
    # Panels between the lift and the corners. Two panels each side of the lift stay clear.
    for x in [-13, -11, -9, -7, -5, 5, 7, 9, 11, 13]:
        block('RearPanel', (x, 5.1, 25.8), (1.7, 9.5, .10), 'BurgundyWood', False, .015)
        utube('SM_Trivia_RearPanelFrame', [(p[0], p[2], -p[1]) for p in rounded_rect(x, -25.72, 5.1, 1.55, 9.2, .05, 3)], .025, 'GoldDark', True)
        for zz, hh in [(2.2, 3.5), (8.1, 3.2)]:
            if zz - hh / 2 < B + .1: zz, hh = (B + .3 + 4.4) / 2 + .15, 4.4 - B - .3
            utube('SM_Trivia_RearPanelInner', [(p[0], p[2], -p[1]) for p in rounded_rect(x, -25.69, zz, 1.33, hh, .09, 5)], .018, 'Gold', True)
    # Rear panels stand off the wall; this keeps players from walking into them on the cross-aisle.
    for side in [-1, 1]:
        collider('Trivia_RearPanelGuard', U(side * 9.2, B + 1.6, 25.72), (10.4, .36, 3.2))
    # A lighter coffered ceiling: ivory field, gilded beams both ways, so the hall reads bright and grand.
    ceiling = bpy.data.objects.get('SM_Trivia_Ceiling')
    if ceiling:
        ceiling.data.materials.clear(); ceiling.data.materials.append(M_('Ivory'))
    for x in [-9.6, -3.6, 3.6, 9.6]:
        block('CeilingBeam', (x, HEIGHT - .1, 10), (.18, .20, 32), 'GoldDark', False, .025)

def M_(name):
    import trivia_geometry as g
    return g.M[name]


# ---------------------------------------------------------------- seating, ramps and carpets

def ramp(name, x0, x1, material='Walnut'):
    verts = [(x0, 0, RAMP_FOOT), (x0, 0, CROSS), (x0, B, CROSS), (x1, 0, RAMP_FOOT), (x1, 0, CROSS), (x1, B, CROSS)]
    faces = [(0, 1, 2), (3, 5, 4), (0, 3, 4, 1), (1, 4, 5, 2), (0, 2, 5, 3)]
    return solid(name, verts, faces, material, True)

def on_ramp(z):
    return B * min(1, max(0, (z - RAMP_FOOT) / (CROSS - RAMP_FOOT)))

def sloped_strip(name, x0, x1, z0, z1, lift, thickness, material):
    """A thin strip lying on the ramp (and the flat floors either side of it)."""
    zs = [z0] + [z for z in (RAMP_FOOT, CROSS) if z0 < z < z1] + [z1]
    verts = []
    for z in zs:
        h = on_ramp(z) + lift
        verts += [(x0, h, z), (x1, h, z), (x0, h + thickness, z), (x1, h + thickness, z)]
    n = len(zs); faces = []
    for i in range(n - 1):
        a, b = 4 * i, 4 * (i + 1)
        faces += [(a + 2, b + 2, b + 3, a + 3), (a, a + 1, b + 1, b), (a, b, b + 2, a + 2), (a + 1, a + 3, b + 3, b + 1)]
    faces += [(0, 2, 3, 1), (4 * (n - 1), 4 * (n - 1) + 1, 4 * (n - 1) + 3, 4 * (n - 1) + 2)]
    return solid(name, verts, faces, material)

def seating(seat_template):
    far = seat_template(distant=True)
    for side in [-1, 1]:
        for r in range(ROWS):
            z = ROW0 + r * PITCH; top = r * RISE
            if r > 0:
                block('SeatTier', (side * (AISLE + BLOCK) / 2, top / 2, z), (BLOCK - AISLE, top, PITCH), 'Walnut', True)
                # Gilded nosing on each row edge, so the rise reads clearly from the aisle.
                utube('SM_Trivia_TierNosing', [(side * AISLE, top + .008, z - PITCH / 2 + .01), (side * BLOCK, top + .008, z - PITCH / 2 + .01)], .012, 'GoldDark', sides=5)
            for col in range(SEAT_COLS):
                clone(far, f'SM_Trivia_Seat_R{r:02}_{-side}_{col:02}', U(side * (SEAT_X0 + col * SEAT_DX), top, z))
            # One collider per row block: players can't climb into the rows.
            x0 = SEAT_X0 - .43; x1 = SEAT_X0 + (SEAT_COLS - 1) * SEAT_DX + .43
            collider('Trivia_SeatingTier', U(side * (x0 + x1) / 2, top + .63, z + .02), (x1 - x0, .72, 1.26))
    bpy.data.objects.remove(far, do_unlink=True)
    # Ramps: centre aisle and both side aisles rise 1.2 m over 13 m (about 1 in 11).
    ramp('AisleRamp', -AISLE, AISLE)
    for side in [-1, 1]:
        ramp('SideAisleRamp', side * BLOCK, side * SIDE)
    # Cross-aisle at lift level, from the top of the ramps to the rear wall.
    block('CrossAisle', (0, B / 2, (CROSS + WALL) / 2), (2 * SIDE, B, WALL - CROSS), 'Walnut', True)
    # Red carpet with gold stars runs from the lift, down the centre aisle, to the stage steps.
    sloped_strip('AisleCarpet', -1.38, 1.38, RAMP_FOOT, WALL, .002, .010, 'VelvetRed')
    for x in [-1.39, 1.39]:
        sloped_strip('AisleBorder', x - .0125, x + .0125, RAMP_FOOT, WALL, .012, .004, 'Gold')
    slope = math.atan(B / (CROSS - RAMP_FOOT))
    z = RAMP_FOOT + .8
    while z < WALL - .6:
        o = star('SM_Trivia_AisleStar', (0, 0, 0), .16, 'Gold', plane='XY')
        if RAMP_FOOT < z < CROSS:
            o.rotation_euler.x = -slope; apply(o)
        o.location = U(0, on_ramp(z) + .014, z)
        z += PITCH
    # Front carpet: the foot of the aisle out to both corner flights.
    pts = []
    t0, t1 = apron_t(7.6), apron_t(-7.6)
    for i in range(41):
        p = apron_point(t0 + (t1 - t0) * i / 40, .6)
        pts.append((p.x, p.y))
    verts = []
    for x, z in pts:
        verts += [U(x, .012, z), U(x, .012, RAMP_FOOT)]
    faces = [(2 * i, 2 * i + 2, 2 * i + 3, 2 * i + 1) for i in range(len(pts) - 1)]
    carpet = mesh('SM_Trivia_FrontCarpet', verts, faces, 'VelvetRed', True)
    if carpet.data.polygons[0].normal.z < 0:
        for f in carpet.data.polygons: f.flip()
    utube('SM_Trivia_FrontCarpetBorder', [(x, .014, z) for x, z in pts], .012, 'Gold', sides=5)


# ---------------------------------------------------------------- stage front and corner steps

def stage_front():
    arc = [(10.7 * math.cos(i * math.pi / 120), -.6 - 5 * math.sin(i * math.pi / 120), STAGE) for i in range(121)]
    for dz, r, material in [(.02, .045, 'Gold'), (-.06, .022, 'Bulb'), (-.49, .035, 'Gold')]:
        tube('SM_Trivia_StageApronEdge', [(x, y, z + dz) for x, y, z in arc], r, material, sides=8)
    flights = sorted(sorted([apron_t(side * STEP_X[0]), apron_t(side * STEP_X[1])]) for side in [1, -1])
    gaps = [(0.0, flights[0][0]), (flights[0][1], flights[1][0]), (flights[1][1], math.pi)]
    for i, (t0, t1) in enumerate(gaps):
        n = max(2, int((t1 - t0) / .04)); verts = []
        for k in range(n + 1):
            t = t0 + (t1 - t0) * k / n
            pin, pout = apron_point(t, -.03), apron_point(t, .07)
            verts += [(pin.x, 0, pin.y), (pout.x, 0, pout.y), (pin.x, STAGE, pin.y), (pout.x, STAGE, pout.y)]
        faces = []
        for k in range(n):
            a, b = 4 * k, 4 * (k + 1)
            faces += [(a + 2, a + 3, b + 3, b + 2), (a + 1, b + 1, b + 3, a + 3), (a, a + 2, b + 2, b), (a, b, b + 1, a + 1)]
        faces += [(0, 1, 3, 2), (4 * n, 4 * n + 2, 4 * n + 3, 4 * n + 1)]
        solid(f'ApronEdgeGuard_{i}', verts, faces, None, render=False)
    # Curved flights at both front corners, concentric with the stage edge.
    rise = STAGE / 3; going = .3
    for side, name in [(1, 'R'), (-1, 'L')]:
        ta, tb = sorted([apron_t(side * STEP_X[0]), apron_t(side * STEP_X[1])])
        n = 24
        for k in range(2):           # k = 0: upper tread (0.4 m), k = 1: lower tread (0.2 m)
            top = STAGE - rise * (k + 1); inner = going * k; outer = going * (k + 1)
            verts = []
            for i in range(n + 1):
                t = ta + (tb - ta) * i / n
                pi_, po = apron_point(t, inner - (.02 if k == 0 else 0)), apron_point(t, outer)
                verts += [(pi_.x, 0, pi_.y), (po.x, 0, po.y), (pi_.x, top, pi_.y), (po.x, top, po.y)]
            faces = []
            for i in range(n):
                a, b = 4 * i, 4 * (i + 1)
                faces += [(a + 2, a + 3, b + 3, b + 2), (a + 1, b + 1, b + 3, a + 3), (a, a + 2, b + 2, b), (a, b, b + 1, a + 1)]
            faces += [(0, 1, 3, 2), (4 * n, 4 * n + 2, 4 * n + 3, 4 * n + 1)]
            solid(f'CornerStep_{name}_{k + 1}', verts, faces, 'Walnut', True)
            # Gilded nosing and a lit strip under it on every riser.
            edge = [apron_point(ta + (tb - ta) * i / n, outer) for i in range(n + 1)]
            utube('SM_Trivia_CornerStepNosing', [(p.x, top + .005, p.y + 0) for p in edge], .018, 'Gold', sides=6)
            utube('SM_Trivia_CornerStepLight', [(p.x, top - .045, p.y) for p in [apron_point(ta + (tb - ta) * i / n, outer + .006) for i in range(n + 1)]], .011, 'Bulb', sides=5)
        # Gilded cheeks close both ends of the flight.
        for t in [ta, tb]:
            a, b = apron_point(t, -.02), apron_point(t, 2 * going)
            utube('SM_Trivia_CornerStepCheek', [(a.x, STAGE - .02, a.y), (b.x, .02, b.y)], .02, 'GoldDark', sides=6)


# ---------------------------------------------------------------- the lift, at the centre of the rear wall

def lift(S, light, lantern):
    w, h = DOOR_W, DOOR_H
    start = set(bpy.data.objects)
    # Built in the facade's own frame (x across, -y towards the hall, z up from the lift floor),
    # then turned to face the stage. Proportions follow the round 1 portal, with a lower arch so the
    # whole portal fits under the rear gallery (deck underside 4.6 m, lift floor 1.2 m).
    for side in [-1, 1]:
        box('SM_Trivia_LiftJamb', (side * (w / 2 + .13), 0, h / 2), (.23, .32, h), 'Gold', .04)
    arch = [((w / 2 + .1) * math.cos(i * math.pi / 40), -.05, h + .40 * math.sin(i * math.pi / 40)) for i in range(41)]
    tube('SM_Trivia_LiftArch', arch, .12, 'Gold', sides=10)
    for side in [-1, 1]:
        box('SM_Trivia_LiftPilaster', (side * (w / 2 + .36), .06, (h + .55) / 2), (.35, .35, h + .55), 'Ivory', .03)
        for zz in [.10, .28, h + .37, h + .55]:
            box('SM_Trivia_LiftCapital', (side * (w / 2 + .36), -.01, zz), (.52, .48, .10), 'Gold', .02)
        tube('SM_Trivia_LiftPilasterInset', rounded_rect(side * (w / 2 + .36), -.13, 1.45, .22, 2.0, .035, 4), .018, 'Gold', True)
    for dz in [.17, .30]:
        tube('SM_Trivia_LiftArchLayer', [(xx, -.08, zz + dz) for xx, _, zz in arch], .05, 'GoldDark' if dz == .17 else 'Gold', sides=8)
    box('SM_Trivia_LiftIndicator', (0, -.09, h + .14), (.85, .13, .24), 'BlackMetal', .045)
    for side in [-1, 1]:
        x = side * .18; coords = [(x - .09, -.17, h + .08), (x + .09, -.17, h + .08), (x, -.17, h + .21)]
        if side > 0: coords = [(xx, yy, 2 * (h + .145) - zz) for xx, yy, zz in coords]
        mesh('SM_Trivia_LiftArrow', coords, [(0, 2, 1) if side > 0 else (0, 1, 2)], 'Bulb', True)
    for x in [-2.1, 2.1]:
        lantern(x, 0, 2.5)
    # Facade frame -> world: front (-y) faces the stage, back plane sits on the rear wall face.
    to_world = Matrix.Translation(U(0, B, WALL - .24)) @ Matrix.Rotation(math.pi, 4, 'Z')
    place(built_since(start), to_world)
    # Door leaves stand just behind the wall, so they slide open behind it, into the cab.
    for side, tag in [(-1, 'L'), (1, 'R')]:
        leaf_start = set(bpy.data.objects)
        box('SM_Trivia_LiftDoorPanel', U(side * (w / 4 + .006), B + h / 2, 26.16), (w / 2 - .012, .10, h), 'Gold', .015)
        for dx in [-w / 4 + .12, w / 4 - .12]:
            box('SM_Trivia_LiftDoorInset', U(side * w / 4 + dx, B + h / 2, 26.10), (.012, .02, h - .25), 'GoldDark')
        door = join_objects(built_since(leaf_start), f'SM_Trivia_LiftDoor_{tag}'); origin_bottom(door)
        collider(f'Trivia_LiftDoor_{tag}', U(side * (w / 4 + .006), B + h / 2, 26.16), (w / 2 - .012, .1, h))
    # Reveal lining the wall opening, and a flush threshold (no lip between hall and cab).
    for side in [-1, 1]:
        block('LiftReveal', (side * (w / 2 + .02), B + h / 2, 26), (.04, h, .2), 'Gold', False)
    block('LiftReveal', (0, B + h - .02, 26), (w + .08, .04, .2), 'Gold', False)
    block('LiftThreshold', (0, B - .025, 26), (w, .05, .2), 'GoldDark', True)
    # Cab behind the wall: floor flush with the cross-aisle, interior from docs/sizes.json.
    cw, cd, ch = CAB; cz = 26.1 + cd / 2
    block('LiftFloor', (0, B - .1, cz), (cw, .2, cd), 'Marble', True)
    for side in [-1, 1]:
        block('LiftCabWall', (side * (cw / 2 + .075), B + ch / 2, cz), (.15, ch, cd), 'Walnut', True)
    block('LiftCabBack', (0, B + ch / 2, 26.1 + cd + .075), (cw, ch, .15), 'Walnut', True)
    block('LiftCeiling', (0, B + ch + .075, cz), (cw + .3, .15, cd + .15), 'Ivory', True)
    block('LiftCeilingLight', (0, B + ch - .01, cz), (2.4, .02, 2.4), 'Bulb', False)
    for side in [-1, 1]:
        utube('SM_Trivia_LiftHandrail', [(side * (cw / 2 - .05), B + .95, 26.6), (side * (cw / 2 - .05), B + .95, 26.1 + cd - .5)], .025, 'Gold', sides=8)
    # Four players start inside the cab, facing the doors and the stage beyond.
    for i, (x, z) in enumerate([(-1.0, cz - .7), (1.0, cz - .7), (-1.0, cz + .7), (1.0, cz + .7)]):
        empty(f'MK_Spawn_{i + 1:02}', U(x, B, z))
    empty('MK_Exit', U(0, B, WALL - .6))
    light('Area', U(0, B + ch - .05, cz), '#FFE6C4', 520, U(0, B, cz), size=2.4, purpose='Lift cab ceiling panel; bake')
    light('Area', U(0, 4.4, 23.3), '#FFD9AA', 380, U(0, B + 1.4, WALL), size=2.6, purpose='Lift portal key under the rear gallery; bake')

def flank_lift_with_urns():
    """The two floor urns that stood by the old lift now flank the new lift, as in the reverse concept."""
    bpy.context.view_layer.update()
    urns = sorted([o for o in bpy.data.objects if o.name.startswith('SM_Trivia_FlowerUrn') and ubounds(o)[0].x > 11],
                  key=lambda o: ubounds(o)[0].z)
    plinths = sorted([o for o in bpy.data.objects if o.name.startswith('COL_Trivia_Plinth') and ubounds(o)[0].x > 11],
                     key=lambda o: ubounds(o)[0].z)
    for o, x in zip(urns, [-3.25, 3.25]):
        o.location = U(x, B, WALL - .55)
    for o, x in zip(plinths, [-3.25, 3.25]):
        o.location = U(x, B + .57, WALL - .55)


# ---------------------------------------------------------------- galleries: kept, out of reach

def galleries(seat_template):
    # Fill the deck where the stairs came up.
    for side in [-1, 1]:
        slab('BalconyDeck', [(side * 10.95, 19), (side * 13.95, 19), (side * 13.95, 24.4), (side * 10.95, 24.4)], 4.8, .2, 'Walnut', True)
    # The audience reaches the galleries through two doors at their back, above the lift.
    for x in [-3.0, 3.0]:
        door_start = set(bpy.data.objects)
        box('SM_Trivia_GalleryDoorLeaf', (0, .03, 1.1), (1.2, .06, 2.2), 'BurgundyWood', .02)
        for dx in [-.3, .3]:
            tube('SM_Trivia_GalleryDoorInset', rounded_rect(dx, -.005, 1.25, .42, 1.6, .05, 4), .012, 'Gold', True)
            sphere('SM_Trivia_GalleryDoorHandle', (dx * .15, -.03, 1.05), (.025, .03, .025), 'Gold', 10, 6)
        for side in [-1, 1]:
            box('SM_Trivia_GalleryDoorArchitrave', (side * .7, .0, 1.18), (.2, .1, 2.36), 'Ivory', .015)
        box('SM_Trivia_GalleryDoorArchitrave', (0, .0, 2.42), (1.6, .12, .2), 'Ivory', .015)
        box('SM_Trivia_GalleryDoorPediment', (0, -.02, 2.6), (1.8, .16, .1), 'Gold', .02)
        sphere('SM_Trivia_GalleryDoorLamp', (0, -.1, 2.85), (.09, .09, .12), 'Bulb', 12, 8)
        place(built_since(door_start), Matrix.Translation(U(x, 4.8, WALL - .06)) @ Matrix.Rotation(math.pi, 4, 'Z'))
    # One row of gallery seats along each side, turned towards the stage.
    far = seat_template(distant=True)
    for side in [-1, 1]:
        for i in range(10):
            z = .2 + i * .95; x = side * 13.35
            seat = clone(far, f'SM_Trivia_UpperSeat_Side_{-side}_{i}', U(x, 4.8, z))
            # Face the centre of the stage front (Unity (0, z=1.5)).
            seat.rotation_euler.z = math.atan2(-x, z - 1.5)
            apply(seat)
        collider('Trivia_UpperSideSeating', U(side * 13.35, 4.8 + .63, 4.5), (.75, 1.26, 9.6))
    bpy.data.objects.remove(far, do_unlink=True)


# ---------------------------------------------------------------- tie-backs, TV, screens

def rope(name, pts, r=.022, pitch=.085, cyclic=False):
    """Two-ply twisted gold rope along a path (source coordinates)."""
    pts = [Vector(p) for p in pts]
    s = [0.0]
    for i in range(1, len(pts)): s.append(s[-1] + (pts[i] - pts[i - 1]).length)
    for strand in range(2):
        out = []
        n = len(pts)
        for i, p in enumerate(pts):
            if cyclic: t = pts[(i + 1) % n] - pts[i - 1]
            else: t = (pts[min(i + 1, n - 1)] - pts[max(i - 1, 0)])
            t.normalize()
            a = t.cross(Vector((0, 0, 1)))
            if a.length < .2: a = t.cross(Vector((0, 1, 0)))
            a.normalize(); b = t.cross(a).normalized()
            phase = s[i] / pitch * math.tau + strand * math.pi
            out.append(p + (a * math.cos(phase) + b * math.sin(phase)) * r * .9)
        tube(name, out, r, 'Gold', cyclic, sides=8)

def resample(pts, step, cyclic=False):
    pts = [Vector(p) for p in pts]
    if cyclic: pts = pts + [pts[0]]
    out = [pts[0]]
    for a, b in zip(pts, pts[1:]):
        n = max(1, int((b - a).length / step))
        for i in range(1, n + 1): out.append(a.lerp(b, i / n))
    return out[:-1] if cyclic else out

def tiebacks():
    """Thick twisted gold rope wrapped round each gathered drape, hooked with some slack onto a gilded
    holdback on the proscenium pillar, with a heavy tassel hanging from the knot at the front."""
    from mathutils.geometry import convex_hull_2d
    zz = 3.12
    for side in [-1, 1]:
        name = 'L' if side < 0 else 'R'
        cloth = next(o for o in bpy.data.objects if o.name.startswith('SM_Trivia_TiedDrape') and 'Fringe' not in o.name and 'Hem' not in o.name
                     and (sum(v.co.x for v in o.data.vertices) / len(o.data.vertices)) * side > 0)
        bpy.context.view_layer.update(); cross = []
        for edge in cloth.data.edges:
            a, b = [cloth.matrix_world @ cloth.data.vertices[i].co for i in edge.vertices]
            for plane in [zz - .05, zz, zz + .05]:
                if (a.z - plane) * (b.z - plane) < 0:
                    p = a.lerp(b, (plane - a.z) / (b.z - a.z)); p.z = zz; cross.append(p)
        hull = [cross[i] for i in convex_hull_2d([Vector((v.x, v.y)) for v in cross])]
        center = sum(hull, Vector()) / len(hull)
        loop = [center + (p - center).normalized() * ((p - center).length + .07) for p in hull]
        for p in loop: p.z = zz
        rope('SM_Trivia_TiebackRope_' + name, resample(loop, .012, True), r=.026, pitch=.10, cyclic=True)
        # Slack run from the outer side of the loop down and up onto the holdback.
        outer = max(loop, key=lambda p: p.x * side)
        hook = Vector((side * 9.70, center.y, zz + .05))
        mid = outer.lerp(hook, .5) + Vector((0, 0, -.16))
        rope('SM_Trivia_TiebackRope_' + name, resample([outer, outer.lerp(mid, .6), mid, mid.lerp(hook, .5) + Vector((0, 0, -.03)), hook], .012), r=.026, pitch=.10)
        hold = detail.lathe('SM_Trivia_TiebackHoldback_' + name, (0, 0, 0), [(.0, .0), (.17, 0), (.19, .03), (.16, .08), (.10, .12), (.05, .15), (.0, .16)], 'Gold', 28, 8)
        hold.rotation_euler.y = -side * math.pi / 2; apply(hold); hold.location = (side * 9.725, center.y, zz + .05)
        # Knot at the front of the drape with a long, heavy tassel.
        front = min(loop, key=lambda p: p.y)
        knot = front + Vector((0, -.07, -.02))
        sphere('SM_Trivia_TiebackKnot_' + name, knot, (.10, .075, .09), 'Gold', 16, 10)
        rope('SM_Trivia_TiebackRope_' + name, resample([knot, knot + Vector((0, -.02, -.2))], .012), r=.018, pitch=.08)
        detail.tassel(knot.x, knot.y - .02, knot.z - .2, 1.35)

def television():
    """Upright players' TV on the stage front: faces the players, with a dressed back for the audience."""
    LIVE_W, LIVE_H = 12.15, 4.10
    w = 1.4; h = w / (LIVE_W / LIVE_H)
    center = U(0, 1.35, 5.32)
    begin = set(bpy.data.objects)
    box('SM_Trivia_PlayerTVHousing', (0, 0, 0), (w + .12, .14, h + .12), 'BlackMetal', .035)
    panel('SM_Trivia_PlayerScreen', 0, -.076, 0, w, h, 'Screen')
    tube('SM_Trivia_PlayerTVBezel', rounded_rect(0, -.082, 0, w + .085, h + .085, .05, 8), .012, 'GoldDark', True)
    # Back, seen by the audience: walnut panel, gold frame and the show's laurel.
    box('SM_Trivia_PlayerTVBack', (0, .085, 0), (w + .16, .03, h + .16), 'Walnut', .012)
    tube('SM_Trivia_PlayerTVBackFrame', rounded_rect(0, .103, 0, w + .06, h + .06, .04, 6), .014, 'Gold', True)
    crest = laurel(0, .11, -.02, .17, 'SM_Trivia_PlayerTVBackLaurel')
    if crest:
        for v in crest.data.vertices: v.co.y = 2 * .11 - v.co.y
        for f in crest.data.polygons: f.flip()
    # The housing's front is -y in this frame; turn it to face the players (towards the stage, Unity -z).
    for o in built_since(begin):
        apply(o)
        o.data = o.data.copy()
        o.data.transform(Matrix.Translation(center) @ Matrix.Rotation(math.pi, 4, 'Z') @ o.matrix_world)
        o.matrix_world = Matrix.Identity(4)
    block('PlayerTVFoot', (0, .65, 5.32), (1.0, .10, .40), 'BlackMetal', True, .025)
    block('PlayerTVStand', (0, .935, 5.38), (.12, .57, .10), 'BlackMetal', True, .015)
    utube('SM_Trivia_PlayerTVCable', [(.1, 1.1, 5.44), (.1, .8, 5.45), (.1, .68, 5.42), (.1, .61, 5.40)], .012, 'BlackMetal')
    collider('Trivia_PlayerTV', U(0, 1.35, 5.32), (1.56, .2, h + .2))


# ---------------------------------------------------------------- control position and cameras

def functions(light):
    # Front-of-house desk on the cross-aisle, stage-left of the lift: walnut console, gold trim, faders.
    x, z = -9.6, 23.4
    block('ControlDesk', (x, B + .5, z), (3.4, 1.0, .9), 'Walnut', True, .03)
    utube('SM_Trivia_ControlTrim', [(x - 1.7, B + 1.0, z - .45), (x + 1.7, B + 1.0, z - .45)], .018, 'Gold', sides=6)
    surface = solid('ControlSurface', [(x - 1.6, B + 1.0, z - .4), (x + 1.6, B + 1.0, z - .4), (x - 1.6, B + 1.14, z + .35), (x + 1.6, B + 1.14, z + .35),
                                       (x - 1.6, B + .98, z - .4), (x + 1.6, B + .98, z - .4), (x - 1.6, B + .98, z + .35), (x + 1.6, B + .98, z + .35)],
                    [(0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1), (2, 3, 7, 6), (0, 2, 6, 4), (1, 5, 7, 3)], 'BlackMetal')
    for i in range(20):
        fx = x - 1.35 + i * .142; t = .3 + .4 * ((i * 7) % 5) / 5
        block('ControlFader', (fx, B + 1.0 + .14 * t + .02, z - .4 + .75 * t), (.04, .02, .05), 'GoldDark', False, .004)
    for dx in [-1.05, 0, 1.05]:
        block('ControlMonitorStand', (x + dx, B + 1.3, z + .32), (.06, .32, .06), 'BlackMetal')
        block('ControlMonitor', (x + dx, B + 1.62, z + .32), (.92, .55, .06), 'BlackMetal', False, .02)
        tube('SM_Trivia_ControlMonitorFrame', [U(*p) for p in [(px, py, z + .285) for px, _, py in [(q[0], 0, q[2]) for q in rounded_rect(x + dx, 0, B + 1.62, .94, .57, .03, 3)]]], .01, 'Gold', True)
        block('ControlDisplay', (x + dx, B + 1.62, z + .286), (.84, .47, .006), 'Monitor')
    front = z - .45
    for dx in [-1.1, 0, 1.1]:
        utube('SM_Trivia_ControlPanelFrame', [(x + dx + q[0], B + .5 + q[2], front - .012) for q in rounded_rect(0, 0, 0, .9, .62, .05, 4)], .014, 'Gold', True)
    crest_start = set(bpy.data.objects)
    laurel(0, 0, 0, .16, 'SM_Trivia_ControlLaurel')
    place(built_since(crest_start), Matrix.Translation(U(x, B + .5, front - .02)) @ Matrix.Rotation(math.pi, 4, 'Z'))
    for dx in [-1.72, 1.72]:
        for dz in [-.45, .45]:
            block('ControlPost', (x + dx, B + .52, z + dz), (.12, 1.04, .12), 'Ivory', False, .015)
            block('ControlPostCap', (x + dx, B + 1.06, z + dz), (.16, .05, .16), 'Gold', False, .01)
    utube('SM_Trivia_ControlTrim', [(x - 1.7, B + .06, front - .01), (x + 1.7, B + .06, front - .01)], .016, 'Gold', sides=6)
    # Operator's chair behind the desk.
    cylinder('SM_Trivia_ControlChairBase', U(x, B + .03, z + .95), .28, .06, 'BlackMetal', 16)
    cylinder('SM_Trivia_ControlChairColumn', U(x, B + .25, z + .95), .035, .4, 'GoldDark', 10)
    block('ControlChairSeat', (x, B + .5, z + .95), (.5, .09, .48), 'VelvetSeat', False, .04)
    block('ControlChairBack', (x, B + .82, z + 1.17), (.48, .55, .08), 'VelvetSeat', False, .04)
    light('Area', U(x, 4.3, z - .8), '#FFE2BE', 160, U(x, B + 1.0, z), size=2.2, purpose='Front-of-house desk working light; bake')
    # Broadcast cameras: one wide camera on the cross-aisle (stage-right of the lift) and one on each
    # side of the stage, covering the podiums.
    for cx, cy, cz, aim in [(9.6, B, 23.2, (0, 1.8, 2.5)), (-12.2, 0, 5.2, (-2.5, 1.6, 2.2)), (12.2, 0, 5.2, (2.5, 1.6, 2.2))]:
        cam_start = set(bpy.data.objects)
        block('CameraStationFoot', (0, .06, 0), (.8, .12, .8), 'BlackMetal', False, .05)
        for a in range(3):
            ang = a * math.tau / 3
            block('CameraStationLeg', (math.cos(ang) * .25, .3, math.sin(ang) * .25), (.07, .5, .07), 'BlackMetal')
        cylinder('SM_Trivia_CameraPedestal', U(0, .9, 0), .07, 1.2, 'BlackMetal', 16)
        block('BroadcastCamera', (0, 1.66, -.05), (.32, .34, .65), 'BlackMetal', False, .04)
        lens = cylinder('SM_Trivia_BroadcastLens', U(0, 1.66, -.48), .115, .24, 'BlackMetal', 20); lens.rotation_euler.x = math.pi / 2; apply(lens)
        utube('SM_Trivia_CameraHandle', [(-.35, 1.5, .45), (-.2, 1.55, .25)], .02, 'GoldDark')
        utube('SM_Trivia_CameraHandle', [(.35, 1.5, .45), (.2, 1.55, .25)], .02, 'GoldDark')
        built = built_since(cam_start)
        yaw = math.atan2(-(aim[0] - cx), -(aim[2] - cz))   # Unity yaw so the lens (-z local) faces the aim point
        place(built, Matrix.Translation(U(cx, cy, cz)) @ Matrix.Rotation(-yaw, 4, 'Z'))


# ---------------------------------------------------------------- house lights

def chandelier(light, z, y_bottom=7.2):
    start = set(bpy.data.objects)
    top = HEIGHT
    detail.lathe('SM_Trivia_ChandelierRose', (0, 0, top - .05), [(.0, 0), (.55, 0), (.5, -.06), (.3, -.12), (.12, -.16), (.0, -.17)], 'Gold', 40)
    tube('SM_Trivia_ChandelierStem', [(0, 0, top - .15), (0, 0, y_bottom + 1.2)], .035, 'GoldDark', sides=10)
    detail.lathe('SM_Trivia_ChandelierBody', (0, 0, y_bottom), [(.0, -.1), (.14, -.05), (.24, .15), (.30, .45), (.18, .75), (.10, 1.1), (.06, 1.2)], 'Gold', 32)
    for ring_r, ring_z, count in [(1.35, y_bottom + .45, 16), (.85, y_bottom + .95, 10)]:
        tube('SM_Trivia_ChandelierRing', [(ring_r * math.cos(i * math.tau / 64), ring_r * math.sin(i * math.tau / 64), ring_z) for i in range(64)], .03, 'Gold', True, sides=8)
        for i in range(count):
            a = i * math.tau / count
            px, py = ring_r * math.cos(a), ring_r * math.sin(a)
            arm = []
            for k in range(9):
                t = k / 8; rr = .22 + (ring_r - .22) * t
                arm.append((rr * math.cos(a), rr * math.sin(a), ring_z - .30 - .16 * math.sin(math.pi * t) + .30 * t * t))
            tube('SM_Trivia_ChandelierArm', arm, .018, 'Gold', sides=6)
            # Crystal swag to the next candle.
            b = (i + 1) * math.tau / count
            qx, qy = ring_r * math.cos(b), ring_r * math.sin(b)
            for k in range(1, 6):
                t = k / 6
                sphere('SM_Trivia_ChandelierCrystal', (px + (qx - px) * t, py + (qy - py) * t, ring_z - .03 - .16 * math.sin(math.pi * t)), (.02, .02, .03), 'PetalWhite', 8, 5)
            cylinder('SM_Trivia_ChandelierCup', (px, py, ring_z + .04), .045, .06, 'Gold', 10)
            cylinder('SM_Trivia_ChandelierCandle', (px, py, ring_z + .14), .022, .14, 'Ivory', 8)
            sphere('SM_Trivia_ChandelierBulb', (px, py, ring_z + .25), (.035, .035, .055), 'Bulb', 10, 6)
            sphere('SM_Trivia_ChandelierDrop', (px * 1.02, py * 1.02, ring_z - .14), (.022, .022, .05), 'Bulb', 8, 5)
    detail.lathe('SM_Trivia_ChandelierFinial', (0, 0, y_bottom - .1), [(.0, -.35), (.05, -.3), (.09, -.18), (.06, -.05), (.1, 0), (.0, .02)], 'Gold', 24)
    place(built_since(start), Matrix.Translation(U(0, 0, z)))
    light('Point', U(0, y_bottom + .6, z), '#FFD9A6', 2600, size=1.2, purpose=f'Chandelier over the audience at z={z}; house light, bake')
    return light


# ---------------------------------------------------------------- collision that matches what players see

def proxy_colliders():
    """Box colliders around the visible parts players could otherwise walk into: curtains, pillars,
    theatre-box fronts, wing walls and wall pilasters. One box per connected piece, kept to the
    height a player occupies above the floor it stands on."""
    bpy.context.view_layer.update()
    prefixes = ('SM_Trivia_BackCurtain', 'SM_Trivia_TiedDrape', 'SM_Trivia_ProsceniumPillar', 'SM_Trivia_ProsceniumCapital',
                'SM_Trivia_ProsceniumFlute', 'SM_Trivia_Box', 'SM_Trivia_WingWall', 'SM_Trivia_WallPilaster',
                'SM_Trivia_PilasterRaisedPanel', 'SM_Trivia_CarvedRosette', 'SM_Trivia_RosetteBoss', 'SM_Trivia_Acanthus',
                'SM_Trivia_Tassel', 'SM_Trivia_TiebackKnot', 'SM_Trivia_TiebackHoldback', 'SM_Trivia_TiebackRope', 'SM_Trivia_AisleSconce',
                'SM_Trivia_WingWallBead', 'SM_Trivia_Grille', 'SM_Trivia_LiftHandrail', 'SM_Trivia_LiftJamb', 'SM_Trivia_LiftPilaster',
                'SM_Trivia_LiftCapital', 'SM_Trivia_PodiumScreen', 'SM_Trivia_MonitorBackHousing', 'SM_Trivia_Podium_0',
                'SM_Trivia_BroadcastCamera', 'SM_Trivia_BroadcastLens', 'SM_Trivia_CameraPedestal', 'SM_Trivia_CameraStation',
                'SM_Trivia_CameraHandle', 'SM_Trivia_ControlMonitor', 'SM_Trivia_ControlPost', 'SM_Trivia_ControlChair')
    boxes = []
    for o in bpy.data.objects:
        if o.type != 'MESH' or not o.name.startswith(prefixes): continue
        bm = bmesh.new(); bm.from_mesh(o.data); bm.verts.ensure_lookup_table()
        seen = set()
        for v in bm.verts:
            if v.index in seen: continue
            stack = [v]; part = []
            seen.add(v.index)
            while stack:
                cur = stack.pop(); part.append(cur)
                for e in cur.link_edges:
                    w = e.other_vert(cur)
                    if w.index not in seen: seen.add(w.index); stack.append(w)
            us = [o.matrix_world @ p.co for p in part]
            us = [Vector((-p.x, p.z, -p.y)) for p in us]
            lo = Vector([min(u[i] for u in us) for i in range(3)]); hi = Vector([max(u[i] for u in us) for i in range(3)])
            if max((hi - lo).x, (hi - lo).y, (hi - lo).z) < .15: continue
            on_stage = lo.z < 5.6 and abs((lo.x + hi.x) / 2) < 10.7 and lo.z > -5.4 and lo.y >= STAGE - .05
            floor = STAGE if on_stage else on_ramp((lo.z + hi.z) / 2)
            top = floor + 2.3
            if lo.y > top or hi.y < floor - .1: continue
            lo.y = max(lo.y, floor - .02); hi.y = min(hi.y, top)
            if (hi - lo).x < .02 or (hi - lo).z < .02: continue
            boxes.append((lo, hi))
        bm.free()
    verts = []; faces = []
    for lo, hi in boxes:
        base = len(verts)
        for x in (lo.x, hi.x):
            for y in (lo.y, hi.y):
                for z in (lo.z, hi.z):
                    verts.append(U(x, y, z))
        # Index = 4*ix + 2*iy + iz.
        for f in [(0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1), (2, 3, 7, 6), (0, 2, 6, 4), (1, 5, 7, 3)]:
            faces.append(tuple(base + i for i in f))
    if not verts: return
    o = mesh('COL_Trivia_DecorProxy', verts, faces, None)
    # Same outward-winding rule as solid(): check one box's volume sign.
    a, b = Vector(verts[0]), Vector(verts[7])
    test = 0.0
    for f in faces[:6]:
        p = [Vector(verts[i]) for i in f]
        for i in range(1, 3): test += p[0].dot(p[i].cross(p[i + 1]))
    if test < 0:
        for f in o.data.polygons: f.flip()
    o.hide_render = True; o.hide_set(True)


def play_areas():
    """Where players may go. The room checks fail if a player can reach anywhere outside these boxes."""
    for name, center, size in [
        ('MK_PlayArea_01', (0, 1.9, 10.25), (28.8, 4.8, 31.3)),      # hall floor, aisles, cross-aisle and stage, under the galleries
        ('MK_PlayArea_02', (0, B + 1.75, 28.7), (5.4, 3.6, 5.6)),    # lift cab and threshold
    ]:
        o = empty(name, U(*center))
        o.scale = (size[0], size[2], size[1])     # source axes: x, depth, height
        o.empty_display_type = 'CUBE'; o.empty_display_size = .5


# ---------------------------------------------------------------- entry point

def apply_round_three(S, seat_template, rail, light, LIGHTS, lantern):
    global DOOR_W, DOOR_H, CAB
    DOOR_W = S['elevator']['door']['width']; DOOR_H = S['elevator']['door']['height']
    interior = S['elevator']['interior']; CAB = (interior['width'], interior['depth'], interior['height'])
    print('ROUND3 clearing', flush=True); clear_round_two(LIGHTS)
    print('ROUND3 shell', flush=True); shell()
    print('ROUND3 seating', flush=True); seating(seat_template)
    print('ROUND3 stage front', flush=True); stage_front()
    print('ROUND3 lift', flush=True); lift(S, light, lantern); flank_lift_with_urns()
    print('ROUND3 galleries', flush=True); galleries(seat_template)
    print('ROUND3 tiebacks and TV', flush=True); tiebacks(); television()
    print('ROUND3 functions', flush=True); functions(light)
    print('ROUND3 house lights', flush=True)
    for z in [10.0, 18.0]:
        chandelier(light, z); LIGHTS[-1]['range_m'] = 24
    # Rear-wall lanterns, symmetric about the lift, above the rear gallery.
    for x in [-13, -9, -5, 5, 9, 13]:
        start = set(bpy.data.objects); lantern(0, 0, 7.3)
        place(built_since(start), Matrix.Translation(U(x, 0, 25.55)) @ Matrix.Rotation(math.pi, 4, 'Z'))
    scale_light(LIGHTS, 'Audience fill', 1.4)
    print('ROUND3 collision and play areas', flush=True); proxy_colliders(); play_areas()
