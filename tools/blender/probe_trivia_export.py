"""Temporary FBX axis diagnostic; no production geometry edits."""
from pathlib import Path
import bpy
import sys
sys.path.insert(0,str(Path(__file__).parent))
from export_unity import export_selection
root=Path(__file__).resolve().parents[2]
bpy.ops.wm.open_mainfile(filepath=str(root/'art-source/trivia/Trivia_Round01.blend'))
bpy.ops.object.select_all(action='DESELECT')
for o in bpy.context.scene.objects:
    if o.type in ('MESH','EMPTY'):o.hide_set(False);o.select_set(True)
export_selection(root/'Assets/_OneMoreFloor/Art/Floors/Trivia/SM_Trivia_AxisProbe.fbx')
