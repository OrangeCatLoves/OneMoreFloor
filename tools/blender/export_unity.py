"""Shared OneMoreFloor FBX preset: metres, named meshes/empties, Unity +Z front."""
import bpy
import math
from mathutils import Matrix

def export_selection(path):
    """Caller selects export meshes/markers and applies mesh transforms first."""
    # Blender's automatic space bake double-converts grandchildren below a
    # rotated empty. Rebase local coordinates explicitly, preserving every
    # world-space vertex and pivot, then export with automatic baking disabled.
    objects=[o for o in bpy.context.selected_objects if o.type in ('MESH','EMPTY')]
    bpy.context.view_layer.update()
    poses={o:o.matrix_world.copy() for o in objects};original={};copies={}
    basis=Matrix.Rotation(-math.pi/2,4,'X');inverse=basis.inverted()
    def depth(o):return 0 if not o.parent else 1+depth(o.parent)
    levels=sorted({depth(o) for o in objects})
    try:
        for o in objects:
            if o.type!='MESH':continue
            original[o]=o.data
            if o.data not in copies:
                data=o.data.copy();data.transform(basis);copies[o.data]=data
            o.data=copies[o.data]
        for level in levels:
            for o in objects:
                if depth(o)==level:o.matrix_world=poses[o]@inverse
            bpy.context.view_layer.update()
        bpy.ops.export_scene.fbx(
            filepath=str(path),use_selection=True,object_types={'MESH','EMPTY'},
            global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',
            axis_forward='-Z',axis_up='Y',bake_space_transform=False,
            use_mesh_modifiers=True,add_leaf_bones=False,bake_anim=False,
            use_custom_props=True,path_mode='RELATIVE')
    finally:
        for o,data in original.items():o.data=data
        for level in levels:
            for o in objects:
                if depth(o)==level:o.matrix_world=poses[o]
            bpy.context.view_layer.update()
        for data in copies.values():bpy.data.meshes.remove(data)
