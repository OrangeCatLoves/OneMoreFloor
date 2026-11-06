# Trivia art pipeline

Run from the project root with Blender 5.2 LTS installed. No downloaded mesh,
texture pack, Python package or paid service is required.

```powershell
& 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe' --background --python-exit-code 1 --python tools/blender/build_trivia.py -- --refresh-textures --iteration 16 --width 1672
& 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe' --background --python-exit-code 1 --python tools/blender/render_trivia.py -- --iteration 16 --width 3344 --save
powershell -NoProfile -ExecutionPolicy Bypass -File tools/blender/review_images.ps1 -Iteration 16 -Downsample
& 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe' --background --python-exit-code 1 --python tools/blender/audit_trivia.py
unity run . --timeout 360 -- -executeMethod OneMoreFloor.ArtReview.TriviaReviewSceneBuilder.BuildAndPreview
& 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe' --background --python-exit-code 1 --python tools/blender/audit_trivia.py
unity test . --mode PlayMode
powershell -NoProfile -ExecutionPolicy Bypass -File tools/blender/review_images.ps1 -Evidence
powershell -NoProfile -ExecutionPolicy Bypass -File tools/blender/compare_trivia.ps1 -Iteration 16
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
  checks clear dimensions and writes the lighting sheet. If Unity review probe
  EXRs exist, it also checks their HDR pixels to reject empty reflection bakes.
- `compare_trivia.ps1`: places the unaltered reference beside its EEVEE render.
  Windows System.Drawing only resizes the images and adds comparison labels.
- `review_images.ps1`: resolves 2x supersampled Blender renders with bicubic
  filtering while retaining the raw PNGs locally. It also assembles native
  Unity AA-off/on crops enlarged with nearest-neighbour filtering, and the
  actual first/third-person playtest captures.
- `log_trivia_assets.py`: explicit per-file asset provenance register.

Generated deliverables:

- `art-source/trivia/Trivia_Round01.blend`
- `Assets/_OneMoreFloor/Art/Floors/Trivia/SM_Trivia_Theatre.fbx`
- `Assets/_OneMoreFloor/Art/Floors/Trivia/Textures/`
- `Assets/_OneMoreFloor/Art/Floors/Trivia/Materials/` after the Unity validator
- `art-source/renders/trivia/round-01/`
- `art-source/renders/trivia/lighting_sheet.md`
- `Assets/_OneMoreFloor/Art/Floors/Trivia/Review/TriviaArtReview.unity`
- `art-source/renders/trivia/round-02/`

The Blender and FBX/texture/material binaries stay local until the owner approves
a version. Their `.meta` files stay with them locally. Scripts, reference images,
comparison evidence, provenance and reports are committed. Do not use `git add -f`
to bypass these review exclusions.

Unity art helpers live under the owned Art folder. The scene builder copies the
existing MovementTest setup into a separate art scene, assembles collision and
review lighting, and bakes two reflection probes. It does not replace the
movement scene, create Trivia rules or register a floor definition. The test
reuses the existing player/controller and local host. Generated review scenes,
probe textures and grading assets stay local with the unapproved model.
