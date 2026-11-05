"""Shared OneMoreFloor FBX preset: metres, named meshes/empties, Unity +Z front."""
import bpy

def export_selection(path):
    """Caller selects export meshes/markers and applies mesh transforms first."""
    bpy.ops.export_scene.fbx(
        filepath=str(path),use_selection=True,object_types={'MESH','EMPTY'},
        global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',
        axis_forward='-Z',axis_up='Y',bake_space_transform=True,
        use_mesh_modifiers=True,add_leaf_bones=False,bake_anim=False,
        use_custom_props=True,path_mode='RELATIVE')
