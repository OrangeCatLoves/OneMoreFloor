# Trivia art pipeline

Run from the project root with Blender 5.2 LTS installed. No downloaded mesh,
texture pack, Python package or paid service is required.

```powershell
& 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe' --background --python-exit-code 1 --python tools/blender/build_trivia.py -- --refresh-textures --iteration 22 --width 3344
& 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe' --background --python-exit-code 1 --python tools/blender/render_trivia.py -- --iteration 22 --width 3344 --save
powershell -NoProfile -ExecutionPolicy Bypass -File tools/blender/review_images.ps1 -Iteration 22 -Downsample
& 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe' --background --python-exit-code 1 --python tools/blender/render_trivia_details.py -- --tag final
& 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe' --background --python-exit-code 1 --python tools/blender/audit_trivia.py
unity run . --timeout 900 -- -executeMethod OneMoreFloor.ArtReview.TriviaReviewSceneBuilder.BuildAndCapture
& 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe' --background --python-exit-code 1 --python tools/blender/audit_trivia.py
unity test . --mode EditMode
unity test . --mode PlayMode
powershell -NoProfile -ExecutionPolicy Bypass -File tools/blender/compare_trivia.ps1 -Iteration 22
powershell -NoProfile -ExecutionPolicy Bypass -File tools/blender/compare_refinement.ps1 -Iteration 22
& 'C:/Program Files/Blender Foundation/Blender 5.2/5.2/python/bin/python.exe' tools/blender/log_trivia_assets.py --date 2026-11-09 --round 04 --iteration 22
```

- `build_trivia.py`: geometry, scene setup, markers, collision, FBX and EEVEE
  renders. `--pilot` builds one podium. `--refresh-textures` overwrites generated
  PNG contents while preserving Unity `.meta` IDs. Use it after material changes.
- `trivia_geometry.py`: reusable geometry; repeated seats and small primitives
  share mesh data. Static detail is joined by semantic part; reactive parts stay
  separate. Closed solid meshes have outward-facing normals.
- `trivia_refinement.py`: gathered cloth, curved gilt leaves, rosettes, floral
  upholstered backs and fixture details. `trivia_botanicals.py` supplies overlapping
  rose whorls, phlox clusters and veined leaves with explicit base-to-tip UVs. Local RNG seeds keep
  botanical edits independent of the room layout and stage planks.
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
  EXRs exist, it also checks their HDR pixels to reject empty, nonfinite or implausibly bright
  reflection bakes. The room-specific HDR peak limit is 500.
- `compare_trivia.ps1`: places the unaltered reference beside its EEVEE render.
  Windows System.Drawing only resizes the images and adds comparison labels.
- `review_images.ps1`: resolves 2x supersampled Blender renders with bicubic
  filtering while retaining the raw PNGs locally. It also assembles native
  Unity AA-off/on crops enlarged with nearest-neighbour filtering, and the
  actual first/third-person playtest captures.
- `log_trivia_assets.py`: explicit per-file asset provenance register.
- `render_trivia_details.py`: unretouched bouquet, curtain and carving close-ups.
  `preview_trivia_bouquet.py` replaces only the bouquet in memory for quick
  floral experiments; it never saves or exports the deliverable scene.
- `compare_refinement.ps1`: previous/current and Blender/Unity comparisons;
  defaults to `-Iteration 22 -Previous 21 -Round 04`. The previous raw images
  must exist locally for before/after. Historical pixels are never rerendered.
- `audit_trivia_refinement.py`: checks the 179 frozen interaction/collision
  contracts against a local pre-edit `.blend` snapshot (`--before <path>`).
  Keep that snapshot before changing source; it is not required on a fresh clone.
  Optional `--unity-before <saved-marker-json>` also compares the actual imported
  marker positions and axes, writing `marker-stability.json`.

Generated deliverables:

- `art-source/trivia/Trivia_Round01.blend`
- `Assets/_OneMoreFloor/Art/Floors/Trivia/SM_Trivia_Theatre.fbx`
- `Assets/_OneMoreFloor/Art/Floors/Trivia/Textures/`
- `Assets/_OneMoreFloor/Art/Floors/Trivia/Materials/` after the Unity validator
- `art-source/renders/trivia/round-01/`
- `art-source/renders/trivia/lighting_sheet.md`
- `Assets/_OneMoreFloor/Art/Floors/Trivia/Review/TriviaArtReview.unity`
- `art-source/renders/trivia/round-02/`
- `art-source/renders/trivia/round-03/` (historical)
- `art-source/renders/trivia/round-04/` (current)

The Blender and FBX/texture/material binaries stay local until the owner approves
a version. Their `.meta` files stay with them locally. Scripts, reference images,
comparison evidence, provenance and reports are committed. Do not use `git add -f`
to bypass these review exclusions.

Unity art helpers live under the owned Art folder. The scene builder copies the
existing MovementTest setup into a separate art scene, assembles collision and
review lighting, lightmaps and 113 light probes, then bakes two reflection maps. It does not replace the
movement scene, create Trivia rules or register a floor definition. The test
reuses the existing player/controller and local host. Generated review scenes,
probe textures, UV2 mesh copies, lightmaps and grading assets stay local with
the unapproved model. `TriviaReviewBaking.cs` uses Progressive CPU at 10 texels/m,
32 direct / 128 indirect samples and 3 bounces. The first bake took about 95 s;
import, UV preparation and cubemap captures add time. Allow up to 900 s.
The builder uses Custom-mode assignments for the separately baked reflection
maps, validates them after reopening, and supplies an indoor fallback cubemap.
`CaptureLightingStudy` disables realtime shadows/reflections for diagnostic
frames; it cannot remove already baked shadows or light. It does not save those
temporary scene changes. Rebuild the scene to change baked lights.

Reflection bakes disable existing reflection contributions while capturing both
first-bounce maps, then restore the two Custom probes and indoor fallback.
Their EXR imports are explicitly linear and uncompressed. Use
`TriviaReviewSceneBuilder.RebakeReflectionsAndCapture` for a probe-only repair.
