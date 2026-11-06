# Trivia round 1 — art review and import notes

This is a WIP art review, not owner acceptance or a completed game floor. The
latest three comparisons are iteration 16 in `../renders/trivia/round-01/`.
`review.md` there records the rejected passes and remaining visual differences.
The owner’s “only minor differences” quality gate must not be treated as passed
merely because the technical import checks pass.

## Playtest this iteration

In Unity, choose **OneMoreFloor > Art > Open Trivia Review**, then press **Play**
and click the Game view. Walk out of the open lift, turn toward the middle aisle,
and climb the three illuminated stage steps. **WASD** moves, **mouse** looks,
**Space** jumps, **V** switches first/third person, **scroll** adjusts camera
distance, and **Escape** releases the cursor. This uses the existing local-host
player and camera from MovementTest; questions and answer interactions are not
implemented in this art scene.

The scene is `Assets/_OneMoreFloor/Art/Floors/Trivia/Review/TriviaArtReview.unity`.
It contains colliders, four spawn markers, open display-pose doors, 43 review
lights, two baked reflection probes and ACES grading. Only four lights cast
realtime shadows. Its temporary URP clone enables 4x MSAA and High SMAA at native
resolution; it restores the previous pipeline/AA settings when disabled.
The project's shared graphics assets are unchanged. Camera smoothing settings
are serialized in the review scene; the pipeline override takes effect in Play.

Use **OneMoreFloor > Art > Build and Open Trivia Review** to regenerate after
exporting a changed room. This replaces the art review scene and its local probe
and grading assets; do not keep manual scene edits there. Runtime integration
still belongs to Claude Code. Review evidence and remaining limitations are in
`../renders/trivia/round-02/review.md`.

## Local deliverables

| File or folder | Contents |
|---|---|
| `art-source/trivia/Trivia_Round01.blend` | Editable metre-scale room, image-backed materials, 3 review cameras and EEVEE lighting |
| `Assets/_OneMoreFloor/Art/Floors/Trivia/SM_Trivia_Theatre.fbx` | Room assembly with separate reactive parts, collision meshes and markers |
| `Assets/_OneMoreFloor/Art/Floors/Trivia/Textures/` | 102 PNG maps across 24 shared materials |
| `Assets/_OneMoreFloor/Art/Floors/Trivia/Materials/` | 24 generated URP/Lit material assets with texture bindings |
| `art-source/renders/trivia/lighting_sheet.md` | Light colours, relative power, cone angles and Unity placement |
| `art-source/trivia/lighting.json` | Machine-readable light positions/directions and EEVEE shadow flags |
| `art-source/trivia/material_manifest.json` | Exact material names, map filenames, resolutions and surface settings |
| `art-source/trivia/unity-markers.json` | Actual imported Unity marker transforms |

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
described above. Its two reflection probes improve metal response, but it has
no baked indirect illumination. Do not treat either preview as final lighting.

## Materials and performance

- BaseColor/Emission are sRGB; Normal, Roughness and MetallicSmoothness are
  linear data. Unity uses MetallicSmoothness R for metal and A for smoothness,
  with the material smoothness multiplier set to 1. Separate Roughness maps
  are supplied for Blender/rebaking, not plugged directly into URP/Lit.
- Petal materials use Render Face = Both (`_Cull = 0`) because the curled petals
  are thin surfaces. All other materials use backface culling.
- Sixteen material sets use 2048 × 2048 maps. Eight simple monitor/screen,
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
- Latest totals are 945,460 imported triangles, 480 render meshes, 79 collision
  meshes, 16 buttons, 5 screens and 43 light markers. The source has 1,004,028
  triangles before Unity's removal of degenerate faces and triangulation.
  Smoother petals account for most of the increase; flowers need production
  distance LODs before the laptop budget can be accepted.
- URP/Lit does not reproduce Blender's velvet sheen or wood clearcoat exactly.
  Baked lighting and reflection probes are necessary before judging the engine
  match; shader work may still be needed after that comparison.

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
as a documented substitute; there is no invented tool or licence claim. Their
repetition and limited petal shapes are a remaining quality issue. The owner's
exact ChatGPT concept prompt is preserved in `../concept/floors/trivia/provenance.txt`.
