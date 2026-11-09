"""Refresh the explicit per-file AI asset register without guessing provenance."""
import csv,argparse
from pathlib import Path

root=Path(__file__).resolve().parents[2]
out=root/'art-source/ASSET_LOG.csv'
parser=argparse.ArgumentParser();parser.add_argument('--date',required=True);parser.add_argument('--round',default='04');parser.add_argument('--iteration',default='22');args=parser.parse_args()
previous={}
if out.exists():
    with out.open(encoding='utf-8',newline='') as f:previous={r['file']:r for r in csv.DictReader(f)}
rows=[]
for p in sorted((root/'art-source/concept/floors/trivia').glob('*.png')):
    rows.append([p.relative_to(root).as_posix(),'ChatGPT (image model unspecified)',
        'Owner supplied; exact prompt in art-source/concept/floors/trivia/provenance.txt; 3 selections from 40 images',
        'Owner-provided ChatGPT output; account terms not independently verified','2026-11-06'])
files=list((root/'Assets/_OneMoreFloor/Art/Floors/Trivia').rglob('*.png'))
files+=list((root/'Assets/_OneMoreFloor/Art/Floors/Trivia').glob('*.fbx'))
files+=list((root/'Assets/_OneMoreFloor/Art/Floors/Trivia/Materials').glob('*.mat'))
files+=[p for p in (root/'art-source/trivia').glob('*.blend') if not p.name.startswith('Trivia_Before')]
files+=list((root/'art-source/renders/trivia').rglob('*.png'))
files+=list((root/'art-source/renders/trivia').rglob('*.jpg'))
for suffix in ('*.unity','*.asset','*.exr'):
    files+=list((root/'Assets/_OneMoreFloor/Art/Floors/Trivia/Review').rglob(suffix))
for p in sorted(set(files)):
    tool='Codex-authored Blender Python; Blender 5.2.3 LTS'
    source='docs/floors/trivia_art_brief.md and owner references; tools/blender/build_trivia.py + trivia_refinement.py + trivia_botanicals.py + trivia_geometry.py + trivia_materials.py; original parametric flowers (AI 3D provider not available)'
    licence='Original generated project artwork; no third-party texture/mesh input; Blender output commercial use permitted: https://www.blender.org/about/license/'
    if p.name=='aa-detail-comparison.png':
        tool='Unity 6000.3.28f1 / URP; PowerShell System.Drawing'
        source='TriviaArtValidation.cs captures identical camera/geometry/lighting at native 720p with AA off/on; review_images.ps1 assembles exact 2x nearest-neighbour crops'
        licence='Original project art and comparison assembly'
    elif p.name=='playtest-both-views.jpg':
        tool='Unity 6000.3.28f1 / URP; PowerShell System.Drawing'
        source='TriviaReviewWalkthroughTests.cs native 720p first/third-person captures; review_images.ps1 or compare_refinement.ps1 labelled comparison'
        licence='Project artwork; existing Starter Assets KyleRobot appears in screenshot only under its Unity asset licence'
    elif p.name.startswith('unity-playtest-'):
        tool='Unity 6000.3.28f1 / URP; TriviaReviewWalkthroughTests.cs'
        source='Native 720p capture of actual art review scene; 4x MSAA + High SMAA; existing local-host KyleRobot player'
        licence='Project artwork; KyleRobot appears in local screenshot only under its existing Unity asset licence'
    elif p.name=='bouquet-work.png' or p.name.startswith('bouquet-study'):
        tool='Codex-authored Blender Python; Blender 5.2.3 LTS'
        source='tools/blender/preview_trivia_bouquet.py and trivia_botanicals.py; unretouched local floral geometry study'
    elif f'round-{args.round}' in p.parts:
        tool='Blender 5.2.3 LTS / Unity 6000.3.28f1 URP; PowerShell System.Drawing'
        source='tools/blender/compare_refinement.ps1; matched previous/current cameras, actual Unity review scene captures with baked area light and bounce or unretouched render_trivia_details.py close-ups, according to filename'
        licence='Original project art; no third-party texture/mesh input'
    elif p.suffix=='.jpg':
        tool='PowerShell System.Drawing comparison assembly'
        source='tools/blender/compare_trivia.ps1; labelled owner ChatGPT reference beside Blender EEVEE render; final supersampled renders resolved by review_images.ps1'
        licence='Composite of owner-provided reference and original project artwork; see provenance.txt'
    elif 'Review' in p.parts:
        tool='Unity 6000.3.28f1 / URP; TriviaReviewSceneBuilder.cs'
        source='Art-only walk-through assembled from generated theatre, 43-light JSON, existing MovementTest local-host player/camera; two custom reflection cubemaps, baked area lights, lightmaps, 113 light probes and ACES volume; TriviaReviewBaking.cs generates UV2 review meshes'
        licence='Original project assembly; existing local player/package references preserved; no third-party model redistributed'
    elif p.name.startswith('unity-') or p.suffix=='.mat':
        tool='Unity 6000.3.28f1 / URP; TriviaArtValidation.cs'
        source='Generated theatre FBX and PBR maps; diagnostic lighting. Scale screenshots also show existing local Starter Assets KyleRobot.'
        licence='Project artwork; KyleRobot only appears in local scale evidence under its existing Unity asset licence, not redistributed as a model'
    file=p.relative_to(root).as_posix()
    old=previous.get(file)
    current_prefixes=tuple(f'{label}-{args.iteration}' for label in ('comparison','iteration','supersampled'))
    archived=('renders' in p.parts and f'round-{args.round}' not in p.parts and not p.name.startswith(current_prefixes)) or 'Pilot' in p.name
    if old and archived:
        rows.append([old[k] for k in ['file','tool','prompt_or_source','licence','date']])
    else:rows.append([file,tool,source,licence,args.date])
with out.open('w',encoding='utf-8',newline='') as f:
    writer=csv.writer(f);writer.writerow(['file','tool','prompt_or_source','licence','date']);writer.writerows(rows)
print(f'Logged {len(rows)} assets to {out}')
