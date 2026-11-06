"""Refresh the explicit per-file AI asset register without guessing provenance."""
import csv
from pathlib import Path

root=Path(__file__).resolve().parents[2]
out=root/'art-source/ASSET_LOG.csv'
rows=[]
for p in sorted((root/'art-source/concept/floors/trivia').glob('*.png')):
    rows.append([p.relative_to(root).as_posix(),'ChatGPT (image model unspecified)',
        'Owner supplied; exact prompt in art-source/concept/floors/trivia/provenance.txt; 3 selections from 40 images',
        'Owner-provided ChatGPT output; account terms not independently verified','2026-11-06'])
files=list((root/'Assets/_OneMoreFloor/Art/Floors/Trivia').rglob('*.png'))
files+=list((root/'Assets/_OneMoreFloor/Art/Floors/Trivia').glob('*.fbx'))
files+=list((root/'Assets/_OneMoreFloor/Art/Floors/Trivia/Materials').glob('*.mat'))
files+=list((root/'art-source/trivia').glob('*.blend'))
files+=list((root/'art-source/renders/trivia').rglob('*.png'))
files+=list((root/'art-source/renders/trivia').rglob('*.jpg'))
for suffix in ('*.unity','*.asset','*.exr'):
    files+=list((root/'Assets/_OneMoreFloor/Art/Floors/Trivia/Review').glob(suffix))
for p in sorted(files):
    tool='Codex-authored Blender Python; Blender 5.2.3 LTS'
    source='docs/floors/trivia_art_brief.md and owner references; tools/blender/build_trivia.py + trivia_geometry.py + trivia_materials.py; procedural flowers (AI 3D provider not available)'
    licence='Original generated project artwork; no third-party texture/mesh input; Blender output commercial use permitted: https://www.blender.org/about/license/'
    if p.name=='aa-detail-comparison.png':
        tool='Unity 6000.3.28f1 / URP; PowerShell System.Drawing'
        source='TriviaArtValidation.cs captures identical camera/geometry/lighting at native 720p with AA off/on; review_images.ps1 assembles exact 2x nearest-neighbour crops'
        licence='Original project art and comparison assembly'
    elif p.name=='playtest-both-views.jpg':
        tool='Unity 6000.3.28f1 / URP; PowerShell System.Drawing'
        source='TriviaReviewWalkthroughTests.cs native 720p first/third-person captures; review_images.ps1 labelled comparison'
        licence='Project artwork; existing Starter Assets KyleRobot appears in screenshot only under its Unity asset licence'
    elif p.suffix=='.jpg':
        tool='PowerShell System.Drawing comparison assembly'
        source='tools/blender/compare_trivia.ps1; labelled owner ChatGPT reference beside Blender EEVEE render; iteration 16 uses 2x supersampling resolved by review_images.ps1'
        licence='Composite of owner-provided reference and original project artwork; see provenance.txt'
    elif 'Review' in p.parts:
        tool='Unity 6000.3.28f1 / URP; TriviaReviewSceneBuilder.cs'
        source='Art-only walk-through assembled from generated theatre, 43-light JSON, existing MovementTest local-host player/camera; two baked reflection probes and ACES volume'
        licence='Original project assembly; existing local player/package references preserved; no third-party model redistributed'
    elif p.name.startswith('unity-playtest-'):
        tool='Unity 6000.3.28f1 / URP; TriviaReviewWalkthroughTests.cs'
        source='Native 720p capture of actual art review scene; 4x MSAA + High SMAA; existing local-host KyleRobot player'
        licence='Project artwork; KyleRobot appears in local screenshot only under its existing Unity asset licence'
    elif p.name.startswith('unity-') or p.suffix=='.mat':
        tool='Unity 6000.3.28f1 / URP; TriviaArtValidation.cs'
        source='Generated theatre FBX and PBR maps; diagnostic lighting. Scale screenshots also show existing local Starter Assets KyleRobot.'
        licence='Project artwork; KyleRobot only appears in local scale evidence under its existing Unity asset licence, not redistributed as a model'
    rows.append([p.relative_to(root).as_posix(),tool,source,licence,
        '2026-11-06'])
with out.open('w',encoding='utf-8',newline='') as f:
    writer=csv.writer(f);writer.writerow(['file','tool','prompt_or_source','licence','date']);writer.writerows(rows)
print(f'Logged {len(rows)} assets to {out}')
