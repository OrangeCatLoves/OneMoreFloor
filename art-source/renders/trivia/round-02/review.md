# Trivia polish and anti-aliasing review — 2026-11-06

The owner requested another improvement pass before playtesting. This is a
playtest checkpoint, not acceptance of the original reference quality gate.
The three current reference comparisons are `../round-01/comparison-16_view-1.jpg`
through `view-3.jpg`. The earlier journal preserves the complete difference list.

## Findings and changes

| Finding | Correction and evidence |
|---|---|
| Native Unity captures showed staircase pixels on gold arches, rails, board seams and small symbols. | Review rendering uses 4x MSAA plus High SMAA at native 1280x720. `aa-detail-comparison.png` shows identical cameras, geometry and lighting with AA off/on, enlarged 200% using nearest-neighbour sampling. No compositor blur conceals the difference. |
| Fine textures shimmered at oblique angles. | All generated texture imports use mipmaps, trilinear filtering and anisotropic level 8. This reduces undersampling; a moving human playtest is still needed to judge residual sparkle. |
| Flower silhouettes were polygonal, and leaves formed a repeated tangential ring. | Petals increased from 4x3 to 8x4 surface subdivisions; leaf meshes increased to 12x6 with radial orientation and variation. These are actual geometry changes. Bouquet shapes remain too regular. |
| Upper side seats intersected the balcony fascia, producing conspicuous red triangles. | Shifted the two upper side rows inward and tightened their spacing. All three rerenders were inspected; the protrusions are gone. |
| The last stair tread stopped 0.15 m short of the stage. | Corrected tread centres to meet the stage edge. The real player motor now walks up the three steps in the PlayMode check. |
| Stage reflections washed out the wood/inlay. | Slightly increased wood roughness and reduced clearcoat; warmed the compass gold, lowered its metallic response and increased ring segments. Highlights are calmer, but the compass still reads too pale in the wide reverse shot. |
| The previous engine preview lacked room reflections and a walking route. | Added a separate local art review scene with the existing local-host player/camera, 79 collision meshes, open lift display pose, four spawn markers, 43 review lights, four shadow sources, two baked reflection probes and restrained ACES grading. |

The shared project pipeline/quality assets are unchanged. Review code clones the
current pipeline and restores it when the review ends. This deliberately avoids
choosing production graphics settings on Claude Code's behalf. The built scene
is in `Assets/_OneMoreFloor/Art/Floors/Trivia/Review/TriviaArtReview.unity`.

## Render iterations

- Iteration 15: 192 samples at twice the output dimensions exposed EEVEE's
  512 MB shadow-pool exhaustion. Rejected; only the first view was completed.
- Iteration 16: all three views rerendered at 3344x1882, 192 samples, 1024 MB
  shadow pool and 0.75 shadow resolution scale. No shadow-pool warning in the
  completed render log. Resolved to 1672x941 with high-quality bicubic filtering;
  raw supersampled files remain local. Source cameras match the previous pass,
  so geometry and shading changes can be compared without moving the goalposts.
- Unity evidence: native 1280x720, 4x MSAA + High SMAA. The first gameplay capture
  briefly showed pink variants while Unity compiled offscreen shaders. The
  capture helper now waits for synchronous compilation and renders twice. Final
  first/third-person evidence has the correct materials. This is an editor
  capture correction, not a recolouring of the image.
- Reflection check: the first two probe files contained only a flat background.
  The imported scenery needed `ReflectionProbeStatic` flags in the art scene.
  Corrected, rebaked, and checked the actual HDR pixels in Blender: both probes
  now contain substantial image variation. This pixel check is part of the art
  audit so a successful but empty bake cannot pass silently again.

AA choices follow the [Unity 6.3 URP anti-aliasing documentation](https://docs.unity.com/en-us/engine/6000.3/manual/render-pipelines/universal-render-pipeline/anti-aliasing).
Multisampling treats geometry edges; SMAA smooths remaining image edges. Neither
adds missing modelling detail or guarantees shimmer-free movement. TAA was not
used; this pass keeps sharp symbols without temporal accumulation.

## Remaining differences, worst first

| Priority | Views | Remaining gap |
|---|---|---|
| 1 | All | The carving, cloth and upholstery still look mechanically repeated and lack the references' fine relief, stitching and local wear. This requires richer hero modelling and baked normal detail. |
| 2 | 3 | The hall is wider and less intimate, with more empty ceiling/wall. Image 1 still governs the conflicting lift position and podium orientation. |
| 3 | All | EEVEE has broad stage illumination; Unity has harder pools and more saturated highlights. The playable scene has reflection probes but no baked indirect light, so it does not match the Blender lighting yet. |
| 4 | 1, 2 | Drapes are evenly pleated, tied panels slim and tassels simple. Flowers are smoother but remain small and regular, with visibly layered foliage and plain plinths. |
| 5 | 1, 2 | Question-frame ornament and shaped corners are simpler; screen and lectern proportions/spacing still project differently from the references. |
| 6 | 1, 3 | Balcony mouldings, rails and shields are thinner and more regular; the rear elevation needs richer depth and a closer proportion match. |
| 7 | 2, 3 | Compass and stage boards remain uniform, with pale highlights. In-engine plank grain is too pronounced relative to the concept. |
| 8 | All | Lift carving, brushed finish, light fixtures, bunting, grilles and seat cushions lack hero close-up detail. Some fine metal highlights can still sparkle at low resolution. |

Host, audience, text, confetti and light shafts remain intentionally absent in
this round. The full earlier difference inventory remains open except for the
specific defects corrected above; these gaps are not all minor.

## Checks and cost

- Blender contract audit: no problems; 480 render objects, 229 unique mesh data
  blocks, 24 materials, 1,004,028 source triangles.
- Unity import audit: no problems; 945,460 triangles, 79 collision meshes,
  16 independent buttons, 5 live-screen surfaces, 43 light markers. Names,
  marker positions and axes remain stable; see `marker-stability.json`.
- Previous Unity count: 846,661. Increase: 98,799 (+11.7%), mostly petal curves.
  Textures remain 102 maps, sixteen material sets at 2K and eight at 64 pixels.
- Conservative source camera-frustum counts: 803,708 / 672,414 / 925,454,
  including occluded geometry. The 500k visible target is still unproven;
  these are not GPU measurements. Production flower LODs, occlusion and lighting
  profiling are the next performance work. 4x MSAA also adds render cost.
- 82 EditMode and 22 PlayMode tests passed, none skipped. The new test starts
  the real local host, walks out of the lift using input, follows a collision
  route to climb the stage, checks AA settings and captures both camera views.
  This does not evaluate human comfort, two-player interaction or laptop FPS.
- Blender's saved source reopens and passes an independent audit. Its logs
  retain harmless thumbnail-path/deprecation/cleanup messages; the export is
  not described as warning-free. Unity shader compilation completed without
  a test failure. No shared graphics-settings diff remains.

For the owner: **OneMoreFloor > Art > Open Trivia Review**, **Play**, click Game
view. WASD/mouse, Space jump, V view switch, scroll camera distance, Escape cursor.
Please assess the room's look, close-up edges, moving shimmer, stage navigation
and camera comfort. This is an art walk-through; answer gameplay is not present.
