"""Resume only the owner's layout pass from the generator's local checkpoint."""
from pathlib import Path
import bpy,json,sys
path=Path(__file__).with_name('build_trivia.py')
scope={'__file__':str(path),'__name__':'__main__'}
exec(compile(path.read_text(encoding='utf8').rsplit('\nif opt.pilot:',1)[0],str(path),'exec'),scope)
names={k:v.name for k,v in scope['g'].M.items()}
bpy.ops.wm.open_mainfile(filepath=str(scope['SOURCE']/'Trivia_Round2Base.blend'))
scope['g'].M={k:bpy.data.materials[n] for k,n in names.items()}
scope['LIGHTS'][:]=json.loads((scope['SOURCE']/'round2-base-lights.json').read_text(encoding='utf8'))
from trivia_round_two import apply_round_two
apply_round_two(scope['S'],scope['seat_template'],scope['rail'],scope['light'])
scope['finalize']()
