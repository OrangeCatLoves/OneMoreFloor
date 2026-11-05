# Trivia art pipeline

Run from the project root with Blender 5.2 LTS installed. No downloaded mesh,
texture pack, Python package or paid service is required.

```powershell
& 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe' --background --python-exit-code 1 --python tools/blender/build_trivia.py -- --render --iteration 14 --width 1672
& 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe' --background --python-exit-code 1 --python tools/blender/audit_trivia.py
unity run . --timeout 240 -- -executeMethod OneMoreFloor.ArtReview.TriviaArtValidation.Preview
powershell -NoProfile -ExecutionPolicy Bypass -File tools/blender/compare_trivia.ps1 -Iteration 14
& 'C:/Program Files/Blender Foundation/Blender 5.2/5.2/python/bin/python.exe' tools/blender/log_trivia_assets.py
```

- `build_trivia.py`: geometry, scene setup, markers, collision, FBX and EEVEE
  renders. `--pilot` builds one podium. `--refresh-textures` overwrites generated
  PNG contents while preserving Unity `.meta` IDs. Use it after material changes.
- `trivia_geometry.py`: reusable geometry; repeated seats and small primitives
  share mesh data. Static detail is joined by semantic part; reactive parts stay
  separate. Closed solid meshes have outward-facing normals.
- `trivia_materials.py`: deterministic surface functions evaluated into image
  maps. The same images are used by Blender and Unity. These are procedural
  texture bakes, not photographs or a high-to-low sculpt bake.
- `export_unity.py`: the shared metre-scale FBX preset. Blender source front is
  -Y; exported room front is Unity +Z. Rotation and scale are applied to meshes.
  Measured Unity coordinate conversion for this preset is `(-x, z, -y)`;
  the lighting sheet is checked against imported marker positions.
- `render_trivia.py`: rerender a saved `.blend` using `review_settings.json`,
  without rebuilding geometry. `--views 2` selects one view. Light multipliers
  are applied to the loaded saved lights, not cumulatively across normal runs.
- `audit_trivia.py`: validates geometry contracts, inventories triangle counts,
  checks clear dimensions and writes the lighting sheet.
- `compare_trivia.ps1`: places the unaltered reference beside its EEVEE render.
  Windows System.Drawing only resizes the images and adds comparison labels.
- `log_trivia_assets.py`: explicit per-file asset provenance register.

Generated deliverables:

- `art-source/trivia/Trivia_Round01.blend`
- `Assets/_OneMoreFloor/Art/Floors/Trivia/SM_Trivia_Theatre.fbx`
- `Assets/_OneMoreFloor/Art/Floors/Trivia/Textures/`
- `Assets/_OneMoreFloor/Art/Floors/Trivia/Materials/` after the Unity validator
- `art-source/renders/trivia/round-01/`
- `art-source/renders/trivia/lighting_sheet.md`

The Blender and FBX/texture/material binaries stay local until the owner approves
a version. Their `.meta` files stay with them locally. Scripts, reference images,
comparison evidence, provenance and reports are committed. Do not use `git add -f`
to bypass these review exclusions.

The Unity helper lives under the owned Art/Editor folder. It verifies and maps
art only; it does not create floor gameplay, register a floor definition, replace
the movement test scene, or implement Claude Code's runtime marker importer.
