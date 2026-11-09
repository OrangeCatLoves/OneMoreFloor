# Trivia round 1 — art review and import notes

This is a WIP art review, not owner acceptance or a completed game floor. The
latest three comparisons are iteration 22 in `../renders/trivia/round-01/`.
`review.md` there records the rejected passes and remaining visual differences.
The owner’s “only minor differences” quality gate must not be treated as passed
merely because the technical import checks pass.

The owner deferred playtesting and requested this refinement first. Current
before/after images, close-ups, engine comparisons and the review journal are in
`../renders/trivia/round-04/`. Room layout and interaction positions are frozen
pending the owner's later feedback.

## Review scene (for a later playtest)

In Unity, choose **OneMoreFloor > Art > Open Trivia Review**, then press **Play**
and click the Game view. Walk out of the open lift, turn toward the middle aisle,
and climb the three illuminated stage steps. **WASD** moves, **mouse** looks,
**Space** jumps, **V** switches first/third person, **scroll** adjusts camera
distance, and **Escape** releases the cursor. This uses the existing local-host
player and camera from MovementTest; questions and answer interactions are not
implemented in this art scene.

The scene is `Assets/_OneMoreFloor/Art/Floors/Trivia/Review/TriviaArtReview.unity`.
It contains colliders, four spawn markers, open display-pose doors, 43 light markers, baked area lighting and bounce, two reflection maps and ACES
grading. The scene instantiates 53 lights including derived highlight fills.
Ten area sources and 21 points are baked; 12 stage spots remain realtime,
plus 10 derived spot fills at 65% of the old area stand-in intensity. Four
lights cast realtime shadows. There are 319 lightmapped renderers and 113 light
probes for moving objects. Its temporary URP clone enables 4x MSAA and High SMAA at native
resolution; it restores the previous pipeline/AA settings when disabled.
The project's shared graphics assets are unchanged. Camera smoothing settings
are serialized in the review scene; the pipeline override takes effect in Play.
The separately baked reflection maps are assigned in **Custom** probe mode so
their references survive a scene reload. The stage cubemap also supplies the
indoor fallback outside the probe volumes. Both maps are captured with
reflection feedback disabled, then imported as linear, uncompressed HDR. The
audit rejects empty, nonfinite and implausibly bright maps. Do not change these to a temporary
`bakedTexture` assignment without a complete LightingDataAsset bake.

Use **OneMoreFloor > Art > Build and Open Trivia Review** to regenerate after
exporting a changed room. This also bakes lighting (allow several minutes). It
replaces the art review scene, UV2 mesh copies, maps, probe and grading assets; do not keep manual scene edits there. Runtime integration
still belongs to Claude Code. Review evidence and remaining limitations are in
`../renders/trivia/round-04/review.md`.

## Local deliverables

| File or folder | Contents |
|---|---|
| `art-source/trivia/Trivia_Round01.blend` | Editable metre-scale room, image-backed materials, 3 review cameras and EEVEE lighting |
| `Assets/_OneMoreFloor/Art/Floors/Trivia/SM_Trivia_Theatre.fbx` | Room assembly with separate reactive parts, collision meshes and markers |
| `Assets/_OneMoreFloor/Art/Floors/Trivia/Textures/` | 110 PNG material maps across 25 shared materials |
| `Assets/_OneMoreFloor/Art/Floors/Trivia/Materials/` | 25 generated URP Lit/Complex Lit/Unlit material assets with texture bindings |
| `art-source/renders/trivia/lighting_sheet.md` | Light colours, relative power, cone angles and Unity placement |
| `art-source/trivia/lighting.json` | Machine-readable light positions/directions and EEVEE shadow flags |
| `art-source/trivia/material_manifest.json` | Exact material names, map filenames, resolutions and surface settings |
| `art-source/trivia/unity-markers.json` | Actual imported Unity marker transforms |
| `Assets/_OneMoreFloor/Art/Floors/Trivia/Review/` | Local review scene, UV2 mesh copies, lightmaps, 113 light probes and 2 reflection cubemaps |

The large model, texture and material files and their `.meta` files are local,
excluded from Git pending owner approval as the brief requires. A fresh checkout
must run `tools/blender/README.md`'s build command before importing. The old pilot
FBX/blend proves the initial pipeline; use the full theatre for current geometry.

## Claude Code integration

1. Open the project and let Unity finish importing. The Art helper can rebuild
   material mappings and verify the FBX with:
   `unity run . --timeout 240 -- -executeMethod OneMoreFloor.ArtReview.TriviaArtValidation.Run`
2. Instantiate the full theatre FBX at world origin with identity rotation and
   scale 1. Its source front (-Y in Blender) becomes +Z in Unity. Measured point
   conversion is `(-x, z, -y)`. All mesh rotation/scale is applied.
3. Disable renderers on every `COL_` mesh and attach MeshColliders. Keep static
   colliders non-convex; moving lift-door boxes can be convex. Parent each door
   collider to its matching door with world position preserved. Do not combine
   the two doors, buttons, spotlight hierarchy or marquee chase sets.
4. Build gameplay around `MK_Spawn_01`–`04`, `MK_Objective_01`–`04`, `MK_Host`
   and `MK_Exit`. The four objectives stand on the audience side of the podiums.
5. Assign live question surfaces to `SM_Trivia_QuestionScreen` and
   `SM_Trivia_PodiumScreen_01`–`04`. Each is a flat mesh with clean 0–1 UVs. The
   dark monitor backs are housings inside the static podium meshes, not extra
   live screens. Put all text in the project string tables.
6. Each `SM_Trivia_Podium_nn_Button_<Circle|Square|Cross|Triangle>` has its own
   mesh and pivot. The deck slopes 18 degrees. A short press moves in Unity
   world/local direction `(0, -0.951057, -0.309017)` at the identity room pose;
   return to its initial position after release. The FBX custom property records
   this direction. Lift leaves slide along Unity Z. Spot pan is local Y, tilt
   local X, using the supplied pan/tilt parent hierarchy.
7. `SM_Trivia_MarqueeChase_01`–`03` contain every third marquee bulb. They share
   the bulb material initially: use per-renderer emission overrides for chasing
   so other room bulbs do not change. Instantiate actual Unity lights at the
   supplied markers, using the JSON direction with `Quaternion.LookRotation`.
8. Rebuild/bake the theatrical lighting, then compare
   against EEVEE. Area sources are bake targets. Use only selected stage spots
   as realtime shadow lights. The diagnostic Unity preview is deliberately not
   a production lighting setup; its extra unshadowed directional fill reveals
   textures through the ceiling. Do not copy that fill into the game scene.

The helper's `Preview` method still creates an unsaved diagnostic import-check
scene. `TriviaReviewSceneBuilder.Build` creates the separate playable art scene
described above. It now includes baked indirect lighting and rectangular area
sources. It is still an art review, not an integrated production-lighting setup.
The 79 generated UV2 meshes and LightingDataAsset remain with the local scene;
rebake after changing geometry, materials or static light placement.

## Materials and performance

- BaseColor/Emission are sRGB; Normal, Roughness, SpecularSmoothness and MetallicSmoothness are
  linear data. Unity uses MetallicSmoothness R for metal and A for smoothness,
  with the material smoothness multiplier set to 1. Separate Roughness maps
  are supplied for Blender/rebaking, not plugged directly into URP/Lit. Four
  cloth materials use low-F0 specular maps (RGB 0.008, A smoothness) to reduce
  broad plastic-like highlights. Wood uses Complex Lit with clearcoat weight
  0.24 and smoothness 0.76; cloth still lacks an exact velvet-sheen counterpart.
- The cream live-screen placeholder uses URP Unlit, so stage illumination does
  not tint its display colour. Gameplay will supply the live question texture.
- Petal materials use Render Face = Both (`_Cull = 0`) because the curled petals
  are thin surfaces. All other materials use backface culling.
- Seventeen material sets use 2048 × 2048 maps. Eight simple monitor/screen,
  emissive and button sets use 64 × 64 maps. No 4K textures are used.
- Texture imports use mipmaps, trilinear filtering and anisotropic level 8 to
  reduce shimmer on surfaces viewed at an angle.
- Image maps are deterministic procedural bakes. They are not high-to-low
  sculpt bakes or downloaded materials. Fine carved normals, recess wear and
  stitching remain weaker than the concepts. Repeated pieces share materials;
  a consolidated production trim atlas is not yet supplied.
- Near seats share one mesh; far rows/balconies share a second with fewer bevel
  subdivisions. Features and trim remain. These are fixed distance groups,
  not runtime LODGroups. Static details are batched by type; reactive pieces
  stay independent. GPU instancing is enabled on the generated materials.
- `unity-validation.json` is the actual imported triangle/object count;
  `blender-validation.json` includes category costs and conservative camera
  frustum counts that also include occluded geometry. Neither is a measured
  GPU frame-time benchmark. The 500k visible-triangle target still needs Unity
  profiling with occlusion, lights and shadows enabled.
- Latest totals are 1,549,521 imported triangles, 520 render meshes, 79 collision
  meshes, 16 buttons, 5 screens and 43 light markers. The source has 1,605,549
  triangles before Unity's removal of degenerate faces and triangulation.
  This is 169,757 more imported triangles (+12.3%) than iteration 21. Flowers,
  rounded rose whorls and finer carved leaves account for this pass's increase;
  the heavier cloth/upholstery work remains from iteration 21. Flowers and seats need production distance LODs before the
  laptop budget can be accepted; no frame-time result is claimed.
- Baked area lighting, bounce and wood clearcoat narrow the engine gap.
  Blender and URP still differ in tone mapping, velvet sheen and reflections;
  a custom cloth shader and production reflection setup may be needed.

## Verification and limits

The pilot was imported before the room was generated. The full model is checked
for the five live screens, sixteen buttons, required markers, correct door
height, outward solid collision winding and URP material assignment. Blender
also checks closed collision meshes, applied transforms, marquee sets and all
twelve spot pan/tilt hierarchies. Actual imported light positions are compared
to the lighting JSON.

Shared sizes come from `docs/sizes.json`: player 1.8 m, lift opening 3 × 2.5 m,
cab 5 × 5 × 3.5 m, steps 0.2 m high × 0.3 m deep × 3 m wide. The final podium
width is 1.241 m, leaving 1.959 m between neighboring lecterns. Local Unity
screenshots put the existing KyleRobot at measured 1.8 m beside a podium and
the lift. The new PlayMode test also walks from the lift to the stage using the
existing player motor and captures both views with AA enabled. Camera comfort
and several players moving together still require human assessment.

All 82 EditMode and 22 PlayMode tests passed during this iteration. The extra
PlayMode test exercises the art review route; it skips if the local binary scene
has not been generated on a fresh clone. These tests do not cover Trivia rules,
which have not been implemented yet. Summaries and validation reports are here.

The brief requested an AI 3D provider for flowers. No suitable connected provider
was available in the plugin search. These flowers were constructed in Blender
as a documented substitute; there is no invented tool or licence claim. The
refinement uses overlapping convex rose whorls with closed centres, clustered
phlox, veined leaves and local seeded variation. Botanical realism still falls short of a
sculpted reference-quality asset. The owner's
exact ChatGPT concept prompt is preserved in `../concept/floors/trivia/provenance.txt`.
