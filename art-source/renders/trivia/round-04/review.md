# Trivia refinement - 9 November 2026

Current geometry: **iteration 22**. This continues the owner's authorized
refinement. Playtesting is deferred. Room proportions, interaction positions
and collision geometry remain reserved for his later feedback.

## Changes and rejected studies

- Rebuilt bouquets around overlapping, convex rose whorls with closed centres,
  short phlox clusters and veined leaves. Base-to-tip UVs let colour follow each
  petal. Rejected the first study's angular rims; subdivision followed by
  silhouette-conscious reduction gives the final rounded study. The crown is
  fuller and long exposed stems are less prominent. These remain original
  procedural meshes, not outputs from an unavailable AI 3D provider.
- Added small asymmetries, tip curl and finer vein relief to carved leaves and
  rosettes. The room uses the existing heavier curtain shapes from iteration 21.
- Gave cloth low-reflectance specular maps in Unity and wood a Complex Lit
  clearcoat layer. The cloth is less glossy; this is not a true velvet shader.
  Bulbs now use warm-white emission instead of the previous saturated yellow.
- Added actual Unity baked rectangular lighting and bounced light. The first
  bake was rejected for overexposure. A reduced-energy bake recovered colour
  but flattened metal/wood highlights. The next pass combines baked areas with
  restrained real-time spot counterparts for view-dependent highlights. The
  cream live display now uses an unlit material so stage lighting cannot turn
  it yellow. This preserves a blank, colour-stable surface for the game UI.
- A later Unity render exposed saturated reflection maps (HDR values reached
  65504). Disabling reflections isolated the failure. Rebaking with reflection
  feedback disabled and explicit linear, uncompressed HDR imports restored
  the room. The audit now rejects nonfinite or implausibly bright probe pixels,
  in addition to empty bakes. The exact internal engine cause is not claimed.
- Rebuilt and rendered all three fixed Blender cameras at 3344 x 1882 / 192
  EEVEE samples, then resolved to 1672 x 941. Unity evidence uses native 720p,
  4x MSAA and High SMAA. No paint-over or generated replacement is used.

## Remaining visible differences, worst first

The concepts exclude several delivery constraints: the room intentionally has
no people, question text, confetti or light shafts. Those omissions are not
mistakes in this environment round. Reference 1 controls conflicting geometry.

| Views | Difference | Current assessment |
|---|---|---|
| 3, then 1-2 | Hall depth/width, balcony elevation, ceiling exposure, screen/podium projection and intimate framing differ. | Frozen for the owner's later proportion feedback. This prevents an exact reference match in this pass. |
| All | Carved relief, layered mouldings and irregular recess wear are still simpler and more repetitive. | Curled leaves and finer rosettes improve close-ups; hand-sculpted relief and a production trim atlas remain stronger approaches. |
| 1-2 | Backdrop pleats stay orderly; side-curtain compression, bottom pooling, fringe density and tassel mass differ. | Existing gathered cloth reads heavier, but a dedicated sculpt would be needed to reproduce the reference's irregular weight. |
| All | Amber sparkle, velvet response and reflection shapes differ between the concept, EEVEE and URP. | The reverse hall remains darker than the concept. The new bake addresses absent bounce; diffuse lightmaps alone were insufficient. Neither real-time cubemaps nor the current cloth shader reproduce the reference exactly. |
| 1-2 | Flowers still have regular whorls and pale, opaque petals; species variety and organic irregularity are limited. | Denser heads, rounded rims and shorter sprays improve the bouquet silhouette. Translucency and bespoke sculpted flowers remain absent. |
| 1-2 | Screen corner outline, layered frame profile, crest relief and laurel design are simpler. | Fine leaf relief improved; the screen's live surface, size and pivot are intentionally preserved. |
| All | Podium body proportions, column segmentation, wood joinery and brass profiles differ. | Clearcoat helps material character; the frozen geometry/interaction contract remains unchanged. |
| 1, 3 | Balcony fascia/rails, box depth, banner variety and seat upholstery are less rich. | Existing padded backs and fabric swags remain; repeated modules are visible and stitching is shallow. |
| 1-2 | Lift brushing and surrounding arch/indicator details are less specific. | The correct opening and camera clearance are preserved. A dedicated brushed-metal finish would improve the close-up. |
| 1-2 | Fixture silhouettes and bunting spacing/drape differ. | Optics, ribs and bolts are present; housings and flags remain repeated procedural props. |
| 2-3 | Floor board pattern, compass proportions, local wear and reflections differ. | The polished stage has stronger surface layering in Unity; no layout/inlay edits were made. |

These differences are **not all minor**. Technical success does not close the
owner's visual gate. This is a reviewable WIP, not art acceptance.

## Evidence

- `../round-01/comparison-22_view-1.jpg` through `view-3.jpg`: concept/current.
- `before-after-view-*.jpg`: exact iteration 21 pixels beside iteration 22.
- `detail-before-after-*.jpg`: fixed bouquet, curtain and carving close-ups.
- `engine-comparison-view-*.jpg`: actual Blender and Unity room captures.
- `unity-lighting-before-after.jpg`: previous direct-only review versus this pass.
- `playtest-both-views.jpg`: automated local-host walking capture, not an owner playtest.

Large source/export/maps/review-scene files remain local pending owner approval.
The small JPEG evidence uses the existing scoped ordinary-Git exception because
GitHub LFS quota is exhausted; no large binary policy has been bypassed.

## Technical verification

- 1,605,549 source triangles; 1,549,521 imported triangles (+169,757 / +12.3%
  from iteration 21). Flowers cost 434,223 source triangles across four urns.
  The laptop budget is not proven; the 500k visible target needs profiling and LODs.
- 520 render objects / 269 unique source meshes; 25 shared materials / 110 PNG
  material maps. Seventeen sets are 2K, eight simple sets are 64px. No 4K maps.
- All 179 source interaction/collision checks and all 77 imported marker
  positions/axes are unchanged. 79 colliders, 16 buttons, 5 screens, 43 light markers.
- Geometry/import audits pass. HDR reflection maps are finite and below the
  room-specific ceiling after the correction. Shared-seat FBX exporter material
  index warnings remain; imported material bindings pass validation.
- The review bake produces two lightmaps (1024 and 256 square), 319 lightmapped
  renderers, 79 UV2 mesh copies and 113 light probes. The last full bake took
  about 99 s, excluding import/unwrap/cubemap work. Ten baked areas + 21 baked
  points; 12 realtime stage spots + 10 derived highlight spots, four realtime
  shadow sources. Two separately captured 128px Custom cubemaps persist on reload.
- 82 EditMode + 22 PlayMode tests passed, none skipped. The walking test loads
  the saved scene, checks lightmap/probe persistence and captures first/third person.
  It is not a human camera-comfort assessment or a two-player/laptop benchmark.
- Unity created the required asset metadata. Shared graphics/quality files are
  unchanged in the final commit. No gameplay rules or later build gate are approved.

The marker comparison baseline was the iteration-21 import in commit `635f869`,
saved temporarily as `round4-baseline-markers.json`. Source geometry was compared
with the local `Trivia_BeforeRound04.blend` snapshot.
