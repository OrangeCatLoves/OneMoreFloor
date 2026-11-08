# Trivia refinement — session begun 7 November 2026

The owner explicitly deferred playtesting and requested further refinement of
cloth, ornament, materials, lighting and botanical/small props. Room proportions
remain reserved for the owner's later playtest. This pass preserves the layout,
interaction pivots and collision contract; it does not approve the whole floor.

## Changes under review

| Area | Work |
|---|---|
| Cloth | Unequal pleat spacing, diagonal tension into the tiebacks, overlapping secondary folds, relaxed hems, woven bands and fringe. Sculpted tassel skirts with individual threads. Theatre-box swags are now cloth surfaces, rather than thick red tubes. |
| Ornament | Pointed, curved and veined laurel leaves replace the oval pegs. Layered rosettes, pearl/reel borders, pilaster panels and small screen-corner carvings add readable relief. Grilles use a lobed lattice with reeded borders. |
| Materials | Restrained longitudinal floor grain and shallower board gaps. Subtle woven damask, irregular metal patina/roughness and fine marble veins. Padded seat-back inserts and seams. Fabric bunting replaces the metallic flags. All textures remain image-backed PBR maps. |
| Lighting | Reduced broad fill, smaller stage key, narrower stage pools and less dominant purple. Reduced emissive strength retains more colour in highlights. A new Unity capture method uses the real art review scene at the three reference angles, without the diagnostic import-preview light. |
| Flowers/props | Separate roses, small violet/ivory flowers and pointed leaves replace the solid foliage mound. Local seeded variation changes each bouquet without disturbing other geometry. Urns have fluted bowls and plinths layered mouldings. Lamps have separate optics, cooling ribs and pivot bolts. |

## Iterations and corrections

- **17:** first complete stage review. Curtain gathering and leaf-shaped gilding
  improve the silhouette; stage/hall separation is clearer. Rejected the pointed
  rose tips and the floor-lamp faceplates covering the new optics. Also identified
  overly metallic bunting. Saved the reference comparison for this checkpoint.
- **18:** revised petal tips and their curvature, increased the purple/cream/white
  balance, corrected optic clearance, added upholstered backs and richer grilles.
  Batched each lamp's attached details to avoid a separate renderer per tiny lens.
- **19 / close-up studies:** rejected the remaining origami-like rose fans and
  the intersecting urn bowl/fluting. Rebuilt roses as cupped sectors with rolled
  centres, routed small-flower stems around the large blooms and added foliage
  between flower heads. The urn is now one continuous fluted shell. Distant seat
  seams were costing triangles without visible benefit, so detail is concentrated
  on near seats. Smoothed the marquee bulb and frame silhouettes.
- **20:** rebuilt the complete scene with those corrections; rendered all three
  fixed cameras at 3344 × 1882 and resolved them to 1672 × 941. Inspected the
  three concept comparisons and separate bouquet, tieback and screen-carving
  crops. The before/after comparisons retain the original iteration 16 pixels,
  rather than rerendering the old geometry with the new materials.
- **21:** the close-up inspection found gaps between rose sectors and angular
  stem bends. Increased petal overlap and smoothed the stem avoidance paths;
  rebuilt and rerendered the room. The current comparison images use this pass.
- **Unity lighting correction:** actual reference-camera captures revealed a
  cold blue-grey floor. A controlled no-shadow/no-reflection comparison isolated
  the reflection contribution. The individually baked cubemaps had been assigned
  with `bakedTexture`, which did not persist after reopening the scene without
  a full LightingDataAsset bake. The builder now serializes the maps as custom
  probe textures and supplies an indoor default reflection. Reload validation
  and the existing walking test check that the assignments survive. This fixes
  a review-scene defect; it is not a completed indirect-light bake.

## Remaining visible differences, worst first

The full comparison inventory from round 1 still applies where not resolved.
This is the current assessment of all three final views, excluding the
people, text and effects intentionally omitted by the brief.

| Views | Difference | Assessment / next work |
|---|---|---|
| 3, then 1–2 | Hall width/depth, ceiling exposure, balcony height and projected podium/screen sizes differ. The room feels less intimate. | Preserved deliberately in this pass: the owner reserved proportions for a later playtest. Reference 3 also contradicts image 1's lift and monitor arrangement. |
| All | The reference has richer, more locally varied carved relief and edge wear. Large mouldings and the lift surround still look cleaner and mechanically repeated. | New leaf shapes, rosettes and pearl borders help, but do not equal individually sculpted ornament with baked recess detail. |
| 1–2 | Backdrop pleats are still orderly; tied curtain volume, fringe density and fold compression do not reproduce the reference exactly. | New unequal folds, tension and tassels make the fabric heavier. A dedicated cloth sculpt would improve the remaining regularity. |
| All | Lighting/reflections have less broken amber sparkle and the reverse hall is darker. Gold and the compass can turn pale in bright reflections. | Reduced broad fill improves hierarchy, but reference highlights include people/effects and finer surface variation. Further local roughness and engine indirect-light work remain. |
| 2, then 1 | Bouquets have more visible stems, simpler flower species and more regular petal layering. Colour distribution is not exact. | The solid foliage mound and pointed petal fans are gone. Cupped petals and rolled centres are still a procedural approximation, not a sculpted botanical asset. |
| 1–2 | The screen's inner corners, frame profile and crest are simpler; the blank panel dominates the view. | The live screen's dimensions/pivot are frozen. Corner carvings improve the frame, but its silhouette is not an exact reconstruction. |
| All | Podium proportions, luminous-column segmentation and wood/metal trim profiles differ. | Interaction positions are unchanged. Laurels now have pointed curved leaves; local wear and individual corner mouldings remain less rich. |
| 1, 3 | Balcony fascia/rails are thin, banners repeat, and the boxes are less deeply layered. Seating has less variation and fine stitching. | Added fabric swags, grilles and padded backs improve detail; architecture remains fixed and there is no high-to-low upholstery bake. |
| 1–2 | Lift metal is less directionally brushed; indicator and neighbouring box/panel profiles differ. | Existing metre-scale opening is retained. The material has roughness variation, but needs a dedicated brushed finish. |
| 1–2 | Fixtures now have optics/ribs/bolts, but housing and yoke profiles remain simpler; bunting spacing and drape differ. | Fabric flags and seven-lens faces improve the prop read. These are still repeated fixtures rather than bespoke reference replicas. |
| 2–3 | Stage board pattern, star/ring proportions and reflection shapes differ. | Narrower seams and restrained grain reduce distracting lines. Inlay shape and platform proportions are preserved. |

These differences are not all minor. This pass is a substantial refinement
checkpoint, not a claim that the original visual quality gate has passed.

## Evidence and limits

The three reference cameras remain fixed. `before-after-view-*.jpg` compares the
previous iteration 16 with this pass, and the `engine-comparison-view-*.jpg`
images compare Blender with the actual Unity review scene. Detail images show
actual geometry/materials rendered in EEVEE; no generated paint-over is used.

The audit compares all 179 existing marker/interaction transforms and collider
bounds against the source saved before refinement. It also independently checks
solid collision winding, screens, material bindings, actual imported light
positions and nonempty reflection-probe pixels. Final counts and test results
are recorded in the reports under `art-source/trivia/` and the handoff.

The reference's room proportions remain unresolved by design, pending the owner.
The host, audience, text, confetti and light shafts remain outside this art round.
URP/Lit still approximates Blender's velvet sheen and clearcoat; the local review
scene has baked reflection probes but no production indirect-light bake.
No laptop frame-time result or owner acceptance is claimed.

## Final technical results — 8 November 2026

- 1,379,764 imported Unity triangles; 1,420,860 source triangles. This is
  +434,304 imported triangles (+46%) from iteration 16. The brief prioritizes
  the look and requires this cost to be stated; the laptop budget is unproven.
- 520 render objects, 269 unique source meshes, 25 shared materials and 106
  maps. Seventeen material sets are 2048 × 2048; eight simple sets are 64 × 64.
- No Blender/import contract problems. All 179 frozen source checks and all
  77 imported marker positions/axes are unchanged. There are still 79 collision
  meshes, 16 separate buttons, five live screens and 43 light markers.
- Both baked reflection maps contain varied HDR pixels, and both assignments
  plus the indoor fallback survive reopening the Unity scene.
- 82 EditMode and 22 PlayMode tests passed, zero failures or skips. The walking
  test uses the actual local-host player, exits the lift, climbs the stage and
  captures first/third person with 4x MSAA + High SMAA.
- The actual Unity comparisons are warmer after the reflection correction,
  but remain darker and more contrasty than EEVEE. Velvet has less soft sheen,
  emissive trim reads more yellow, and fine highlights show more sparkle at
  native 720p. Production light baking/material tuning and moving-camera
  assessment remain; a still image cannot establish freedom from shimmer.
- Nonfatal Blender thumbnail/exit-cleanup and linked-seat FBX warnings remain
  in the logs. The blend reopens and the imported URP material bindings pass.
  No claim of warning-free output is made.
