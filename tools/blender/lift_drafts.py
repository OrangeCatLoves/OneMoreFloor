"""Lift design drafts for the owner to choose from (Claude Code, 27 Nov 2026).

Builds five different lifts at the shared sizes in docs/sizes.json (door 3.0 x 2.5 m, cab 5 x 5 x 3.5 m),
each in front of the same neutral wall, and renders three views of each: doors closed, doors open, and
from inside the cab looking out (what players see during the ride). Drafts only: nothing here goes to Unity.

Run on the home PC:  tools/homepc/blender.sh tools/blender/lift_drafts.py [-- --drafts A,B --samples 64]
Renders: art-source/renders/lift/drafts-01/<draft>_<view>.jpg (small JPGs, kept in plain Git: see .gitattributes there)
"""
import argparse
import json
import math
import sys
from pathlib import Path

import bpy
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(Path(__file__).resolve().parent))
import trivia_geometry as g  # noqa: E402
from trivia_geometry import box, cylinder, tube, sphere, rounded_rect, star  # noqa: E402
from trivia_materials import build_materials  # noqa: E402

args = argparse.ArgumentParser()
args.add_argument("--drafts", default="A,B,C,D,E")
args.add_argument("--samples", type=int, default=64)
args.add_argument("--width", type=int, default=1280)
opt = args.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])

SIZES = json.loads((ROOT / "docs/sizes.json").read_text())
DOOR_W = SIZES["elevator"]["door"]["width"]
DOOR_H = SIZES["elevator"]["door"]["height"]
CAB_W = SIZES["elevator"]["interior"]["width"]
CAB_D = SIZES["elevator"]["interior"]["depth"]
CAB_H = SIZES["elevator"]["interior"]["height"]
WALL_T = 0.3                 # hall wall the lift sits in: front face at y = -WALL_T/2, facing -y
CAB_Y0 = WALL_T / 2          # cab starts behind the wall
OUT = ROOT / "art-source/renders/lift/drafts-01"
TEXTURES = Path("C:/OneMoreFloorTools/lift-drafts/textures")


# ----- scene and materials -----

bpy.ops.wm.read_factory_settings(use_empty=True)
g.M, _ = build_materials(TEXTURES, 512, False)


def principled(name, colour, metal=0.0, rough=0.5, emission=None, strength=0.0):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    p = m.node_tree.nodes["Principled BSDF"]
    p.inputs["Base Color"].default_value = (*colour, 1)
    p.inputs["Metallic"].default_value = metal
    p.inputs["Roughness"].default_value = rough
    if emission:
        p.inputs["Emission Color"].default_value = (*emission, 1)
        p.inputs["Emission Strength"].default_value = strength
    g.M[name] = m
    return m


def stripes(name, a, b, frequency=4.0):
    """Diagonal hazard stripes in object space."""
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    nt = m.node_tree
    p = nt.nodes["Principled BSDF"]
    p.inputs["Roughness"].default_value = 0.45
    coord = nt.nodes.new("ShaderNodeTexCoord")
    sep = nt.nodes.new("ShaderNodeSeparateXYZ")
    add = nt.nodes.new("ShaderNodeMath"); add.operation = "ADD"
    mul = nt.nodes.new("ShaderNodeMath"); mul.operation = "MULTIPLY"; mul.inputs[1].default_value = frequency
    frac = nt.nodes.new("ShaderNodeMath"); frac.operation = "FRACT"
    ramp = nt.nodes.new("ShaderNodeValToRGB")
    ramp.color_ramp.interpolation = "CONSTANT"
    ramp.color_ramp.elements[0].position = 0.0
    ramp.color_ramp.elements[0].color = (*a, 1)
    ramp.color_ramp.elements[1].position = 0.5
    ramp.color_ramp.elements[1].color = (*b, 1)
    nt.links.new(coord.outputs["Object"], sep.inputs[0])
    nt.links.new(sep.outputs[0], add.inputs[0])
    nt.links.new(sep.outputs[2], add.inputs[1])
    nt.links.new(add.outputs[0], mul.inputs[0])
    nt.links.new(mul.outputs[0], frac.inputs[0])
    nt.links.new(frac.outputs[0], ramp.inputs["Fac"])
    nt.links.new(ramp.outputs["Color"], p.inputs["Base Color"])
    g.M[name] = m
    return m


principled("Gold", (0.83, 0.58, 0.24), 1.0, 0.24)
principled("GoldDark", (0.46, 0.29, 0.1), 1.0, 0.3)
principled("NeutralWall", (0.32, 0.29, 0.26), 0, 0.7)
principled("NeutralFloor", (0.09, 0.07, 0.06), 0, 0.35)
principled("Mirror", (0.9, 0.9, 0.92), 1.0, 0.04)
principled("Lacquer", (0.012, 0.01, 0.012), 0.0, 0.18)
principled("Steel", (0.5, 0.5, 0.52), 1.0, 0.38)
principled("SteelDark", (0.16, 0.16, 0.17), 1.0, 0.45)
principled("Rubber", (0.02, 0.02, 0.02), 0.0, 0.8)
principled("GlossWhite", (0.82, 0.82, 0.8), 0.0, 0.14)
principled("Glass", (0.02, 0.05, 0.07), 0.2, 0.05)
principled("CyanGlow", (0.0, 0.6, 0.8), 0.0, 0.3, (0.1, 0.85, 1.0), 6.0)
principled("WarmGlow", (1.0, 0.75, 0.45), 0.0, 0.3, (1.0, 0.72, 0.42), 8.0)
principled("RedGlow", (0.9, 0.08, 0.05), 0.0, 0.3, (1.0, 0.12, 0.05), 6.0)
principled("Shaft", (0.05, 0.045, 0.045), 0.0, 0.9)
stripes("Hazard", (0.9, 0.62, 0.02), (0.02, 0.02, 0.02))


def collection(name):
    c = bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(c)
    return c


def into(c):
    """Makes new objects go into collection c."""
    layer = bpy.context.view_layer.layer_collection.children[c.name]
    bpy.context.view_layer.active_layer_collection = layer


def area_light(name, loc, target, size, power, colour=(1, 0.93, 0.85), size_y=None):
    d = bpy.data.lights.new(name, "AREA")
    d.energy = power
    d.color = colour
    d.shape = "RECTANGLE"
    d.size = size
    d.size_y = size_y or size
    o = bpy.data.objects.new(name, d)
    bpy.context.collection.objects.link(o)
    o.location = loc
    o.rotation_euler = (Vector(target) - Vector(loc)).to_track_quat("-Z", "Y").to_euler()
    return o


def point_light(name, loc, power, colour=(1, 0.8, 0.55), radius=0.05):
    d = bpy.data.lights.new(name, "POINT")
    d.energy = power
    d.color = colour
    d.shadow_soft_size = radius
    o = bpy.data.objects.new(name, d)
    bpy.context.collection.objects.link(o)
    o.location = loc
    return o


def camera(name, loc, target, lens=26):
    d = bpy.data.cameras.new(name)
    d.lens = lens
    d.clip_start = 0.05
    o = bpy.data.objects.new(name, d)
    bpy.context.scene.collection.objects.link(o)
    o.location = loc
    o.rotation_euler = (Vector(target) - Vector(loc)).to_track_quat("-Z", "Y").to_euler()
    return o


def segment_digit(prefix, digit, x, y, z, height, material, thickness=0.05, depth=0.03, bulbs=False, flip_about=None):
    """A seven-segment digit standing in the XZ plane (facing -y), bottom-left at (x, z).
    flip_about mirrors it across that x, so it reads correctly from behind (from inside the cab)."""
    w = height * 0.55
    h = height / 2
    segments = {
        "a": ((x + w / 2, z + 2 * h), (w, thickness)), "b": ((x + w, z + 1.5 * h), (thickness, h)),
        "c": ((x + w, z + 0.5 * h), (thickness, h)), "d": ((x + w / 2, z), (w, thickness)),
        "e": ((x, z + 0.5 * h), (thickness, h)), "f": ((x, z + 1.5 * h), (thickness, h)),
        "g": ((x + w / 2, z + h), (w, thickness)),
    }
    lit = {"0": "abcdef", "1": "bc", "2": "abged", "3": "abgcd", "4": "fgbc", "5": "afgcd",
           "6": "afgedc", "7": "abc", "8": "abcdefg", "9": "abcfgd"}[digit]
    for key in lit:
        (cx, cz), (sw, sh) = segments[key]
        if flip_about is not None:
            cx = 2 * flip_about - cx
        if bulbs:
            count = max(2, int(max(sw, sh) / 0.09))
            for i in range(count):
                t = (i + 0.5) / count - 0.5
                px, pz = (cx + t * sw, cz) if sw > sh else (cx, cz + t * sh)
                sphere(f"{prefix}_Bulb", (px, y, pz), (0.035, 0.035, 0.035), material, 8, 6)
        else:
            box(f"{prefix}_Segment", (cx, y, cz), (sw, depth, sh), material)


def segment_text(prefix, text, centre_x, y, z, height, material, inside=False, gap=0.06, **style):
    """Digits centred on centre_x. inside=True lays them out to read from inside the cab (looking at -y)."""
    width = height * 0.55
    start = centre_x - (len(text) * width + (len(text) - 1) * gap) / 2
    for i, digit in enumerate(text):
        segment_digit(f"{prefix}{i}", digit, start + i * (width + gap), y, z, height, material,
                      flip_about=centre_x if inside else None, **style)


# ----- shared surroundings: the hall wall with the lift opening, and the cab shell -----

def hall(prefix):
    """Floor, and a 10 m wall with the door opening, its front face at y = -WALL_T/2."""
    box(f"{prefix}_HallFloor", (0, -4, -0.05), (14, 12, 0.1), "NeutralFloor")
    side = (10 - DOOR_W) / 2
    for sx in (-1, 1):
        box(f"{prefix}_Wall", (sx * (DOOR_W / 2 + side / 2), 0, 2.75), (side, WALL_T, 5.5), "NeutralWall")
    box(f"{prefix}_WallHeader", (0, 0, DOOR_H + (5.5 - DOOR_H) / 2), (DOOR_W, WALL_T, 5.5 - DOOR_H), "NeutralWall")


def cab_shell(prefix, walls, floor, ceiling, back=None):
    y1 = CAB_Y0 + CAB_D
    box(f"{prefix}_CabFloor", (0, CAB_Y0 + CAB_D / 2, -0.05), (CAB_W, CAB_D, 0.1), floor)
    box(f"{prefix}_CabCeiling", (0, CAB_Y0 + CAB_D / 2, CAB_H + 0.05), (CAB_W, CAB_D, 0.1), ceiling)
    for sx in (-1, 1):
        box(f"{prefix}_CabSide", (sx * (CAB_W / 2 + 0.05), CAB_Y0 + CAB_D / 2, CAB_H / 2), (0.1, CAB_D, CAB_H), walls)
    box(f"{prefix}_CabBack", (0, y1 + 0.05, CAB_H / 2), (CAB_W, 0.1, CAB_H), back or walls)
    # The cab's front wall around the door, seen from inside.
    side = (CAB_W - DOOR_W) / 2
    for sx in (-1, 1):
        box(f"{prefix}_CabFront", (sx * (DOOR_W / 2 + side / 2), CAB_Y0 + 0.05, CAB_H / 2), (side, 0.1, CAB_H), walls)
    box(f"{prefix}_CabFrontHeader", (0, CAB_Y0 + 0.05, DOOR_H + (CAB_H - DOOR_H) / 2), (DOOR_W, 0.1, CAB_H - DOOR_H), walls)


def handrails(prefix, material, height=0.95, inset=0.12, radius=0.025):
    """A rail round the sides and back of the cab, on short brackets."""
    y1 = CAB_Y0 + CAB_D
    x = CAB_W / 2 - inset
    tube(f"{prefix}_Handrail", [(-x, CAB_Y0 + 0.6, height), (-x, y1 - inset, height), (x, y1 - inset, height), (x, CAB_Y0 + 0.6, height)], radius, material, sides=10)
    for py in (CAB_Y0 + 0.8, CAB_Y0 + CAB_D / 2, y1 - 0.6):
        for sx in (-1, 1):
            box(f"{prefix}_HandrailPost", (sx * (CAB_W / 2 - inset / 2), py, height), (inset, 0.03, 0.03), material)
    for px in (-1.2, 0.0, 1.2):
        box(f"{prefix}_HandrailPost", (px, y1 - inset / 2, height), (0.03, inset, 0.03), material)


def sliding_leaves(prefix, build_leaf):
    """Two door leaves in the wall opening. build_leaf(name, centre_x, side) makes one; returns (objects, open offsets)."""
    leaves, moves = [], []
    for side in (-1, 1):
        before = set(bpy.data.objects)
        build_leaf(f"{prefix}_Door{'L' if side < 0 else 'R'}", side * DOOR_W / 4, side)
        parts = [o for o in bpy.data.objects if o not in before]
        leaves += parts
        moves += [Vector((side * DOOR_W / 2 * 0.98, 0, 0))] * len(parts)
    return leaves, moves


# ----- the five drafts -----

def draft_a(p):
    """Grand Hotel Art Deco: black lacquer and gold, stepped frame, sunburst doors, dial indicator."""
    hall(p)
    w, h = DOOR_W, DOOR_H
    for i, (pad, depth, mat) in enumerate([(0.55, 0.06, "Lacquer"), (0.38, 0.1, "Gold"), (0.24, 0.14, "Lacquer"), (0.12, 0.18, "GoldDark")]):
        y = -WALL_T / 2 - depth / 2
        for sx in (-1, 1):
            box(f"{p}_Frame{i}", (sx * (w / 2 + pad / 2), y, (h + pad) / 2), (pad, depth, h + pad), mat, 0.01)
        box(f"{p}_FrameTop{i}", (0, y, h + pad / 2), (w + 2 * pad, depth, pad), mat, 0.01)
    # Stepped crown with a semicircular floor dial and its needle.
    tiers = [(w + 1.1, 0.18), (w + 0.6, 0.16), (w * 0.62, 0.78)]
    for i, (width, height) in enumerate(tiers):
        z = h + 0.55 + sum(hh for _, hh in tiers[:i]) + height / 2
        box(f"{p}_Crown{i}", (0, -WALL_T / 2 - 0.05, z), (width, 0.1, height), "Gold" if i < 2 else "Lacquer", 0.01)
    dial_z = h + 0.55 + 0.34 + 0.06
    ring = [(0.62 * math.cos(math.pi * t / 30), -WALL_T / 2 - 0.12, dial_z + 0.62 * math.sin(math.pi * t / 30)) for t in range(31)]
    tube(f"{p}_DialRim", ring, 0.025, "Gold", sides=8)
    for t in range(9):
        a = math.pi * t / 8
        box(f"{p}_DialTick", (0.5 * math.cos(a), -WALL_T / 2 - 0.12, dial_z + 0.5 * math.sin(a)), (0.03, 0.02, 0.12), "Gold").rotation_euler = (0, -a + math.pi / 2, 0)
    needle = box(f"{p}_DialNeedle", (0, -WALL_T / 2 - 0.13, dial_z + 0.22), (0.025, 0.015, 0.45), "RedGlow")
    needle.rotation_euler = (0, math.radians(-25), 0)
    needle.location = (0.45 * 0.5 * math.sin(math.radians(25)), -WALL_T / 2 - 0.13, dial_z + 0.22 * math.cos(math.radians(25)))
    for sx in (-1, 1):
        cylinder(f"{p}_Sconce", (sx * (w / 2 + 1.05), -WALL_T / 2 - 0.08, 2.0), 0.09, 0.45, "WarmGlow", 16)
        box(f"{p}_SconceBack", (sx * (w / 2 + 1.05), -WALL_T / 2 - 0.02, 2.0), (0.3, 0.04, 0.7), "Gold", 0.01)

    def leaf(name, cx, side):
        box(name, (cx, 0, h / 2), (w / 2 - 0.01, 0.06, h), "Gold", 0.005)
        # Sunburst rays fanning up from the bottom of the meeting edge.
        origin_x = cx - side * (w / 4 - 0.02)
        for i in range(9):
            a = math.radians(8 + i * 9)
            length = min(h * 0.92, (w / 2 - 0.08) / max(0.2, math.sin(a)))
            ray = box(f"{name}_Ray", (origin_x + side * math.sin(a) * length / 2, -0.035, 0.1 + math.cos(a) * length / 2), (0.022, 0.012, length), "GoldDark")
            ray.rotation_euler = (0, side * a, 0)
        box(f"{name}_Kick", (cx, -0.035, 0.12), (w / 2 - 0.05, 0.012, 0.2), "Lacquer")

    doors = sliding_leaves(p, leaf)
    cab_shell(p, "Walnut", "Marble", "Ivory", back="Mirror")
    y1 = CAB_Y0 + CAB_D
    for sx in (-1, 1):
        for z in (0.12, 1.0, 2.4, 3.3):
            box(f"{p}_WallTrim", (sx * (CAB_W / 2 - 0.01), CAB_Y0 + CAB_D / 2, z), (0.02, CAB_D, 0.04), "Gold")
    box(f"{p}_FloorInlay", (0, CAB_Y0 + CAB_D / 2, 0.003), (1.6, 1.6, 0.006), "Lacquer").rotation_euler = (0, 0, math.radians(45))
    fan = [(1.1 * math.cos(math.pi * t / 24), y1 - 0.06, 2.55 + 1.1 * math.sin(math.pi * t / 24)) for t in range(25)]
    g.mesh(f"{p}_FanLight", [(0, y1 - 0.06, 2.55)] + fan, [(0, i + 1, i + 2) for i in range(24)], "WarmGlow")
    handrails(p, "Gold")
    # Inside: a small dial over the door.
    tube(f"{p}_InnerDial", [(0.35 * math.cos(math.pi * t / 20), CAB_Y0 + 0.11, h + 0.25 + 0.35 * math.sin(math.pi * t / 20)) for t in range(21)], 0.02, "Gold", sides=8)
    return doors


def draft_b(p):
    """Showtime Marquee: red velvet and gold, a frame of bulbs, a lit floor number, star doors."""
    hall(p)
    w, h = DOOR_W, DOOR_H
    pad = 0.55
    y = -WALL_T / 2 - 0.08
    for sx in (-1, 1):
        box(f"{p}_Frame", (sx * (w / 2 + pad / 2), y, (h + pad) / 2), (pad, 0.16, h + pad), "VelvetRed", 0.02)
    box(f"{p}_FrameTop", (0, y, h + pad / 2), (w + 2 * pad, 0.16, pad), "VelvetRed", 0.02)
    trim = rounded_rect(0, y - 0.09, (h + pad) / 2, w + 2 * pad - 0.1, h + pad - 0.1, 0.06, 4)
    tube(f"{p}_FrameTrim", [(px, py, pz) for px, py, pz in trim], 0.03, "Gold", True)
    tube(f"{p}_InnerTrim", rounded_rect(0, y - 0.09, h / 2 + 0.02, w + 0.06, h + 0.04, 0.04, 4), 0.025, "Gold", True)
    # Bulbs every 22 cm round the frame.
    path = [(-w / 2 - pad / 2, z) for z in [0.25 + i * 0.22 for i in range(int((h + pad / 2 - 0.25) / 0.22))]]
    path += [(x, h + pad / 2) for x in [-w / 2 - pad / 2 + i * 0.22 for i in range(int((w + pad) / 0.22) + 1)]]
    path += [(w / 2 + pad / 2, z) for z in [0.25 + i * 0.22 for i in range(int((h + pad / 2 - 0.25) / 0.22))]]
    for x, z in path:
        sphere(f"{p}_Bulb", (x, y - 0.1, z), (0.045, 0.045, 0.045), "Bulb", 10, 8)
    # Floor sign with a big bulb digit and stars.
    sign_z = h + pad + 0.55
    box(f"{p}_Sign", (0, y, sign_z), (1.5, 0.2, 1.0), "BurgundyWood", 0.03)
    tube(f"{p}_SignTrim", rounded_rect(0, y - 0.11, sign_z, 1.4, 0.9, 0.08, 4), 0.025, "Gold", True)
    segment_digit(p, "7", -0.2, y - 0.12, sign_z - 0.33, 0.66, "Bulb", bulbs=True)
    for sx in (-1, 1):
        star(f"{p}_SignStar", (sx * 0.95, y - 0.05, sign_z), 0.22, "Gold")
        sphere(f"{p}_Globe", (sx * (w / 2 + pad + 0.5), y - 0.12, 2.1), (0.16, 0.16, 0.16), "WarmGlow", 16, 10)
        cylinder(f"{p}_GlobeStem", (sx * (w / 2 + pad + 0.5), y - 0.06, 1.8), 0.03, 0.5, "Gold", 10)

    def leaf(name, cx, side):
        box(name, (cx, 0, h / 2), (w / 2 - 0.01, 0.06, h), "Gold", 0.005)
        star(f"{name}_Star", (cx, -0.04, h * 0.58), 0.42, "GoldDark")
        tube(f"{name}_Panel", rounded_rect(cx, -0.035, h / 2, w / 2 - 0.25, h - 0.3, 0.05, 4), 0.012, "GoldDark", True)

    doors = sliding_leaves(p, leaf)
    cab_shell(p, "VelvetRed", "Carpet", "BurgundyWood")
    y1 = CAB_Y0 + CAB_D
    for sx in (-1, 1):
        for py in (CAB_Y0 + 1.0, CAB_Y0 + 2.5, CAB_Y0 + 4.0):
            box(f"{p}_Pilaster", (sx * (CAB_W / 2 - 0.04), py, CAB_H / 2), (0.08, 0.16, CAB_H), "Gold", 0.01)
    for py in [CAB_Y0 + 0.5 + i * 0.3 for i in range(15)]:
        for sx in (-1, 1):
            sphere(f"{p}_CoveBulb", (sx * (CAB_W / 2 - 0.25), py, CAB_H - 0.08), (0.04, 0.04, 0.04), "Bulb", 8, 6)
    for px in [-CAB_W / 2 + 0.6 + i * 0.4 for i in range(10)]:
        sphere(f"{p}_BackBulb", (px, y1 - 0.08, CAB_H - 0.25), (0.04, 0.04, 0.04), "Bulb", 8, 6)
    star(f"{p}_BackStar", (0, y1 - 0.02, 2.0), 0.6, "Gold")
    handrails(p, "Gold")
    segment_text(f"{p}_Inner", "7", 0, CAB_Y0 + 0.12, h + 0.12, 0.4, "Bulb", inside=True, bulbs=True)
    return doors


def draft_c(p):
    """Victorian Iron Cage: an open wrought-iron car behind a folding lattice gate, under a dial arch."""
    hall(p)
    w, h = DOOR_W, DOOR_H
    y = -WALL_T / 2 - 0.1
    for sx in (-1, 1):
        x = sx * (w / 2 + 0.18)
        cylinder(f"{p}_Column", (x, y, h / 2 + 0.2), 0.11, h + 0.4, "BlackMetal", 16)
        for z in (0.12, 0.9, h + 0.3):
            cylinder(f"{p}_Collar", (x, y, z), 0.15, 0.08, "Gold", 16)
    arch = [((w / 2 + 0.18) * math.cos(math.pi * t / 32), y, h + 0.4 + 0.9 * math.sin(math.pi * t / 32)) for t in range(33)]
    tube(f"{p}_Arch", arch, 0.07, "BlackMetal", sides=10)
    inner = [((w / 2) * math.cos(math.pi * t / 32), y, h + 0.4 + 0.7 * math.sin(math.pi * t / 32)) for t in range(33)]
    tube(f"{p}_ArchInner", inner, 0.035, "BlackMetal", sides=8)
    for t in range(1, 12):
        a = math.pi * t / 12
        tube(f"{p}_ArchSpoke", [(0.35 * math.cos(a), y, h + 0.45 + 0.35 * math.sin(a)), ((w / 2) * math.cos(a), y, h + 0.4 + 0.7 * math.sin(a))], 0.018, "BlackMetal", sides=6)
    # The dial: a clock-like floor indicator at the arch's crown.
    dial = cylinder(f"{p}_Dial", (0, y - 0.05, h + 0.95), 0.38, 0.06, "Ivory", 32)
    dial.rotation_euler = (math.pi / 2, 0, 0)
    ring = cylinder(f"{p}_DialRing", (0, y - 0.04, h + 0.95), 0.43, 0.05, "Gold", 32)
    ring.rotation_euler = (math.pi / 2, 0, 0)
    for t in range(12):
        a = 2 * math.pi * t / 12
        box(f"{p}_DialMark", (0.3 * math.sin(a), y - 0.09, h + 0.95 + 0.3 * math.cos(a)), (0.025, 0.01, 0.07), "BlackMetal").rotation_euler = (0, a, 0)
    hand = box(f"{p}_DialHand", (0.08, y - 0.1, h + 1.05), (0.02, 0.01, 0.26), "BlackMetal")
    hand.rotation_euler = (0, math.radians(35), 0)

    def gate(name, cx, side):
        # Folding lattice: crossing diagonal bars between upright posts.
        half = w / 2 - 0.02
        posts = 6
        for i in range(posts + 1):
            box(f"{name}_Post", (cx - half / 2 + i * half / posts, -0.02, h / 2), (0.025, 0.025, h - 0.04), "BlackMetal")
        cells = 7
        for i in range(posts):
            x0 = cx - half / 2 + i * half / posts
            for j in range(cells):
                z0 = 0.05 + j * (h - 0.1) / cells
                z1 = z0 + (h - 0.1) / cells
                length = math.hypot(half / posts, z1 - z0)
                a = math.atan2(half / posts, z1 - z0)
                for flip in (1, -1):
                    bar = box(f"{name}_Bar", (x0 + half / posts / 2, -0.03, (z0 + z1) / 2), (0.014, 0.012, length), "BlackMetal")
                    bar.rotation_euler = (0, flip * a, 0)
        for z in (0.05, h - 0.05):
            box(f"{name}_Rail", (cx, -0.02, z), (half, 0.03, 0.03), "Gold")

    # Opening folds the gate away to each side (shown as sliding it into the side pockets).
    leaves, moves = sliding_leaves(p, gate)
    # The car: wooden floor, iron bars all round, a lamp, and the dark shaft beyond.
    box(f"{p}_CabFloor", (0, CAB_Y0 + CAB_D / 2, -0.05), (CAB_W, CAB_D, 0.1), "StageWood")
    box(f"{p}_CabRoof", (0, CAB_Y0 + CAB_D / 2, CAB_H + 0.05), (CAB_W, CAB_D, 0.1), "BlackMetal")
    y1 = CAB_Y0 + CAB_D
    for sx in (-1, 1):
        for py in [CAB_Y0 + 0.2 + i * 0.32 for i in range(int(CAB_D / 0.32))]:
            cylinder(f"{p}_CageBar", (sx * CAB_W / 2, py, CAB_H / 2), 0.016, CAB_H, "BlackMetal", 8)
        for z in (0.0, 1.1, 2.2, CAB_H):
            box(f"{p}_CageRail", (sx * CAB_W / 2, CAB_Y0 + CAB_D / 2, z), (0.05, CAB_D, 0.05), "Gold" if z == 1.1 else "BlackMetal")
    for px in [-CAB_W / 2 + 0.2 + i * 0.32 for i in range(int(CAB_W / 0.32) + 1)]:
        cylinder(f"{p}_CageBar", (px, y1, CAB_H / 2), 0.016, CAB_H, "BlackMetal", 8)
    for z in (0.0, 1.1, 2.2, CAB_H):
        box(f"{p}_CageRail", (0, y1, z), (CAB_W, 0.05, 0.05), "Gold" if z == 1.1 else "BlackMetal")
    for sx in (-1, 1):
        box(f"{p}_FrontPost", (sx * (DOOR_W / 2 + 0.05), CAB_Y0 + 0.02, CAB_H / 2), (0.08, 0.08, CAB_H), "BlackMetal")
    box(f"{p}_ShaftBack", (0, y1 + 1.2, 4), (12, 0.2, 12), "Shaft")
    for sx in (-1, 1):
        box(f"{p}_ShaftSide", (sx * (CAB_W / 2 + 1.0), (CAB_Y0 + y1 + 1.2) / 2, 4), (0.2, y1 + 1.2 - CAB_Y0, 12), "Shaft")
    cylinder(f"{p}_LampCord", (0, CAB_Y0 + CAB_D / 2, CAB_H - 0.35), 0.01, 0.7, "BlackMetal", 6)
    sphere(f"{p}_Lamp", (0, CAB_Y0 + CAB_D / 2, CAB_H - 0.8), (0.14, 0.14, 0.14), "WarmGlow", 16, 10)
    tube(f"{p}_LampShade", [(0.22 * math.cos(2 * math.pi * t / 16), CAB_Y0 + CAB_D / 2 + 0.22 * math.sin(2 * math.pi * t / 16), CAB_H - 0.72) for t in range(17)], 0.02, "Gold", sides=6)
    tube(f"{p}_CabHandrail", [(-CAB_W / 2 + 0.1, CAB_Y0 + 0.5, 0.95), (-CAB_W / 2 + 0.1, y1 - 0.1, 0.95), (CAB_W / 2 - 0.1, y1 - 0.1, 0.95), (CAB_W / 2 - 0.1, CAB_Y0 + 0.5, 0.95)], 0.025, "Gold", sides=10)
    return leaves, moves


def draft_d(p):
    """Retro-Future Capsule: glossy white rounded portal, cyan light ring, porthole doors, digital counter."""
    hall(p)
    w, h = DOOR_W, DOOR_H
    y = -WALL_T / 2 - 0.12
    tube(f"{p}_Portal", rounded_rect(0, y, h / 2 + 0.05, w + 0.55, h + 0.6, 0.55, 10), 0.2, "GlossWhite", True, sides=16)
    tube(f"{p}_GlowRing", rounded_rect(0, y - 0.16, h / 2 + 0.05, w + 0.2, h + 0.25, 0.42, 10), 0.035, "CyanGlow", True, sides=10)
    for sx in (-1, 1):
        box(f"{p}_PortalBack", (sx * (w / 2 + 0.225), -WALL_T / 2 - 0.02, (h + 0.5) / 2), (0.45, 0.04, h + 0.5), "SteelDark", 0.01)
    box(f"{p}_PortalBackTop", (0, -WALL_T / 2 - 0.02, h + 0.25), (w, 0.04, 0.5), "SteelDark", 0.01)
    counter_z = h + 0.75
    box(f"{p}_Counter", (0, y, counter_z), (1.1, 0.16, 0.42), "Lacquer", 0.06)
    tube(f"{p}_CounterTrim", rounded_rect(0, y - 0.09, counter_z, 1.1, 0.42, 0.18, 6), 0.018, "GlossWhite", True)
    for i, digit in enumerate("07"):
        segment_digit(f"{p}_Count{i}", digit, -0.28 + i * 0.32, y - 0.1, counter_z - 0.14, 0.28, "CyanGlow", thickness=0.035)
    for sx in (-1, 1):
        box(f"{p}_Strip", (sx * (w / 2 + 1.1), -WALL_T / 2 - 0.03, 1.6), (0.06, 0.05, 2.6), "CyanGlow")

    def leaf(name, cx, side):
        box(name, (cx, 0, h / 2), (w / 2 - 0.01, 0.07, h), "GlossWhite", 0.01)
        port = cylinder(f"{name}_Porthole", (cx, -0.04, 1.65), 0.24, 0.03, "Glass", 32)
        port.rotation_euler = (math.pi / 2, 0, 0)
        rim = cylinder(f"{name}_PortRim", (cx, -0.035, 1.65), 0.29, 0.025, "SteelDark", 32)
        rim.rotation_euler = (math.pi / 2, 0, 0)
        box(f"{name}_Edge", (cx - side * (w / 4 - 0.03), -0.04, h / 2), (0.03, 0.02, h - 0.1), "CyanGlow")

    doors = sliding_leaves(p, leaf)
    cab_shell(p, "GlossWhite", "Lacquer", "GlossWhite")
    y1 = CAB_Y0 + CAB_D
    for sx in (-1, 1):
        for z in (1.0, CAB_H - 0.15):
            box(f"{p}_CabStrip", (sx * (CAB_W / 2 - 0.01), CAB_Y0 + CAB_D / 2, z), (0.02, CAB_D - 0.2, 0.04), "CyanGlow")
    for z in (1.0, CAB_H - 0.15):
        box(f"{p}_CabStripBack", (0, y1 - 0.01, z), (CAB_W - 0.2, 0.02, 0.04), "CyanGlow")
    ring = [(1.0 * math.cos(2 * math.pi * t / 48), CAB_Y0 + CAB_D / 2 + 1.0 * math.sin(2 * math.pi * t / 48), 0.006) for t in range(49)]
    tube(f"{p}_FloorRing", ring, 0.02, "CyanGlow", sides=6)
    light = cylinder(f"{p}_CeilingDisc", (0, CAB_Y0 + CAB_D / 2, CAB_H - 0.02), 1.1, 0.03, "WarmGlow", 48)
    for sx in (-1, 1):
        box(f"{p}_Bench", (sx * (CAB_W / 2 - 0.25), CAB_Y0 + CAB_D / 2 + 0.6, 0.25), (0.45, 2.2, 0.5), "GlossWhite", 0.08)
    segment_text(f"{p}_InnerCount", "07", 0, CAB_Y0 + 0.12, h + 0.18, 0.18, "CyanGlow", inside=True, thickness=0.025)
    return doors


def draft_e(p):
    """Industrial Freight: steel beams, hazard stripes, vertical doors that part up and down, caged lamps."""
    hall(p)
    w, h = DOOR_W, DOOR_H
    y = -WALL_T / 2 - 0.12
    for sx in (-1, 1):
        x = sx * (w / 2 + 0.2)
        box(f"{p}_Beam", (x, y, (h + 0.6) / 2), (0.3, 0.24, h + 0.6), "Steel", 0.01)
        box(f"{p}_BeamFlange", (x, y - 0.12, (h + 0.6) / 2), (0.42, 0.04, h + 0.6), "SteelDark", 0.005)
        for z in [0.3 + i * 0.4 for i in range(int(h / 0.4) + 1)]:
            for dx in (-0.15, 0.15):
                cylinder(f"{p}_Rivet", (x + dx, y - 0.15, z), 0.022, 0.03, "Steel", 8).rotation_euler = (math.pi / 2, 0, 0)
    box(f"{p}_Header", (0, y, h + 0.45), (w + 0.82, 0.24, 0.5), "Steel", 0.01)
    box(f"{p}_HeaderStripes", (0, y - 0.13, h + 0.45), (w + 0.6, 0.02, 0.32), "Hazard")
    box(f"{p}_Threshold", (0, -WALL_T / 2 - 0.25, 0.005), (w + 0.6, 0.5, 0.01), "Hazard")
    for sx in (-1, 1):
        x = sx * (w / 2 + 1.0)
        sphere(f"{p}_WorkLamp", (x, y - 0.1, 2.5), (0.12, 0.12, 0.12), "WarmGlow", 16, 10)
        for t in range(4):
            a = math.pi * t / 4
            tube(f"{p}_LampCage", [(x + 0.17 * math.cos(a) * math.sin(math.pi * k / 8), y - 0.1 + 0.17 * math.sin(a) * math.sin(math.pi * k / 8), 2.5 + 0.17 * math.cos(math.pi * k / 8)) for k in range(9)], 0.008, "SteelDark", sides=5)
        box(f"{p}_LampBracket", (x, y - 0.02, 2.75), (0.08, 0.2, 0.06), "SteelDark")
    box(f"{p}_ControlBox", (w / 2 + 0.75, y + 0.02, 1.3), (0.36, 0.16, 0.5), "SteelDark", 0.02)
    sphere(f"{p}_Button", (w / 2 + 0.75, y - 0.07, 1.4), (0.05, 0.03, 0.05), "RedGlow", 12, 8)
    cylinder(f"{p}_Lever", (w / 2 + 0.75, y - 0.1, 1.2), 0.015, 0.25, "Steel", 8).rotation_euler = (math.radians(30), 0, 0)
    segment_digit(f"{p}_FloorNo", "7", w / 2 + 0.62, y - 0.09, h + 0.33, 0.22, "RedGlow", thickness=0.03)

    leaves, moves = [], []
    for part, z0, z1 in [("Upper", h / 2, h), ("Lower", 0, h / 2)]:
        before = set(bpy.data.objects)
        cz = (z0 + z1) / 2
        box(f"{p}_Door{part}", (0, 0, cz), (w - 0.02, 0.08, z1 - z0 - 0.01), "Steel", 0.005)
        for i in range(5):
            box(f"{p}_Door{part}Rib", (0, -0.05, z0 + (i + 0.5) * (z1 - z0) / 5), (w - 0.1, 0.03, 0.05), "SteelDark")
        box(f"{p}_Door{part}Edge", (0, -0.03, z0 if part == "Upper" else z1), (w - 0.02, 0.1, 0.04), "Rubber")
        if part == "Lower":
            box(f"{p}_DoorLowerStripe", (0, -0.045, 0.15), (w - 0.1, 0.012, 0.2), "Hazard")
        parts = [o for o in bpy.data.objects if o not in before]
        leaves += parts
        moves += [Vector((0, 0, (h / 2 + 0.05) if part == "Upper" else -(h / 2 + 0.05)))] * len(parts)
    # The cab: ribbed steel walls, steel floor, caged lamps.
    cab_shell(p, "SteelDark", "Steel", "SteelDark")
    y1 = CAB_Y0 + CAB_D
    for sx in (-1, 1):
        for py in [CAB_Y0 + 0.3 + i * 0.25 for i in range(int(CAB_D / 0.25) - 1)]:
            box(f"{p}_WallRib", (sx * (CAB_W / 2 - 0.02), py, CAB_H / 2), (0.04, 0.06, CAB_H - 0.2), "Steel")
        box(f"{p}_Bumper", (sx * (CAB_W / 2 - 0.06), CAB_Y0 + CAB_D / 2, 0.6), (0.12, CAB_D - 0.2, 0.25), "Hazard")
    for px in [-CAB_W / 2 + 0.3 + i * 0.25 for i in range(int(CAB_W / 0.25) - 1)]:
        box(f"{p}_BackRib", (px, y1 - 0.02, CAB_H / 2), (0.06, 0.04, CAB_H - 0.2), "Steel")
    for py in (CAB_Y0 + 1.5, CAB_Y0 + 3.5):
        sphere(f"{p}_CabLamp", (0, py, CAB_H - 0.2), (0.12, 0.12, 0.12), "WarmGlow", 16, 10)
        box(f"{p}_CabLampGuard", (0, py, CAB_H - 0.1), (0.4, 0.4, 0.04), "SteelDark")
    segment_text(f"{p}_InnerNo", "7", 0, CAB_Y0 + 0.12, h + 0.25, 0.3, "RedGlow", inside=True, thickness=0.03)
    return leaves, moves


DRAFTS = {
    "A": ("Grand Hotel Art Deco", draft_a),
    "B": ("Showtime Marquee", draft_b),
    "C": ("Victorian Iron Cage", draft_c),
    "D": ("Retro-Future Capsule", draft_d),
    "E": ("Industrial Freight", draft_e),
}


# ----- lighting, cameras and rendering -----

scene = bpy.context.scene
scene.render.engine = "BLENDER_EEVEE"
scene.render.resolution_x = opt.width
scene.render.resolution_y = round(opt.width * 9 / 16)
scene.render.image_settings.file_format = "JPEG"
scene.render.image_settings.quality = 90
scene.view_settings.view_transform = "AgX"
scene.view_settings.look = "AgX - Medium High Contrast"
scene.view_settings.exposure = 0.3
if hasattr(scene, "eevee"):
    scene.eevee.taa_render_samples = opt.samples
    if hasattr(scene.eevee, "use_raytracing"):
        scene.eevee.use_raytracing = True
world = bpy.data.worlds.new("World")
world.use_nodes = True
world.node_tree.nodes["Background"].inputs[0].default_value = (0.05, 0.045, 0.05, 1)
world.node_tree.nodes["Background"].inputs[1].default_value = 0.3
scene.world = world

shared = collection("Shared")
into(shared)
area_light("Key", (-3.5, -6.5, 4.5), (0, 0, 1.6), 4.0, 900)
area_light("Fill", (4.5, -5.0, 2.2), (0, 0, 1.4), 3.0, 350, (0.85, 0.9, 1.0))
area_light("CabLight", (0, CAB_Y0 + CAB_D / 2, CAB_H - 0.1), (0, CAB_Y0 + CAB_D / 2, 0), 3.0, 450)
point_light("CabFill", (0, CAB_Y0 + 1.0, 2.4), 120)
cameras = {
    "closed": camera("CAM_Closed", (2.4, -7.6, 1.75), (0, 0, 1.75), 28),
    "open": camera("CAM_Open", (2.4, -7.6, 1.75), (0, 0.5, 1.6), 28),
    "inside": camera("CAM_Inside", (0.6, CAB_Y0 + CAB_D - 0.5, 2.05), (0, 0, 1.55), 20),
}

OUT.mkdir(parents=True, exist_ok=True)
built = {}
for key in opt.drafts.split(","):
    title, build = DRAFTS[key]
    c = collection(f"Draft_{key}")
    into(c)
    leaves, moves = build(f"SM_Lift{key}")
    built[key] = (c, leaves, moves, title)

for key, (c, leaves, moves, title) in built.items():
    for other, (oc, *_rest) in built.items():
        oc.hide_render = other != key
    rest = [o.location.copy() for o in leaves]
    for view in ("closed", "open", "inside"):
        for o, start, move in zip(leaves, rest, moves):
            o.location = start + (move if view == "open" else Vector((0, 0, 0)))
        scene.camera = cameras[view]
        scene.render.filepath = str(OUT / f"{key}_{view}.jpg")
        print(f"RENDER {key} {title} {view}", flush=True)
        bpy.ops.render.render(write_still=True)
    for o, start in zip(leaves, rest):
        o.location = start

(OUT / "drafts.json").write_text(json.dumps({k: v[3] for k, v in built.items()}, indent=2))
bpy.ops.wm.save_as_mainfile(filepath=str(Path("C:/OneMoreFloorTools/lift-drafts/lift_drafts.blend")))
print("LIFT_DRAFTS_DONE", flush=True)
