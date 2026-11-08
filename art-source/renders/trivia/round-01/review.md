# Trivia round 1 — comparison journal

The references are owner-selected ChatGPT images. Image 1 controls geometry where
they disagree. All comparisons use the same actual 3D room and EEVEE, with no
paint-over, generated replacement image, or composited reference imagery.

## Required differences from the concepts

- People are absent: the brief excludes the host, players used as illustration,
  and audience models from this round. Seating is empty.
- The question panel and four monitors are blank live-screen meshes. No question
  text or answer text is baked into a texture.
- Confetti and visible light shafts are reserved for Unity effects.
- Reference 3 moves the lift to the rear wall and reverses the monitors relative
  to references 1/2. The consistent room retains the lift beside the stage and
  monitors facing players approaching from the aisle, following reference 1.

## Iteration 01 — rejected

Comparisons: `comparison-01_view-1.jpg` through `comparison-01_view-3.jpg`.

Worst differences across the three views:

1. Fabric reads grey instead of crimson/burgundy/purple. White velvet sheen and
   excessive normal strength overwhelm its base colour.
2. Wide camera faces the wrong side of the hall: it hides the lift and left
   balcony composition. Reverse camera cuts off two podiums.
3. Host compass is below the upper surface of the planks. Only its ring appears.
4. Wood grain is far too coarse, repetitive and raised; it resembles corrugated
   material. Metal and stone also carry too much fine surface disturbance.
5. Podium luminous corner columns are buried in the body. Moving spotlight heads
   are displaced by their parent transforms. Both lose important practical light
   details.
6. Backdrop folds are too shallow and regular. Side curtains need fuller hanging
   volume, stronger gathered silhouettes and thicker braided tiebacks.
7. Gold carving is sparse around the proscenium and screen corners. Existing
   leaves shade badly because the procedural sphere normals point inward.
8. Balcony sides lack banners and upper seats; rear balcony is too dark.
9. Flowers are too flat and evenly spaced. Urns and plinths need more ornament.
10. Theatre boxes are narrow, sparsely framed and too dark inside. Wall panels
    and side grilles need a stronger layered facade.
11. Room is too empty between the stage and screen, with small podiums in the
    wide composition. Close and reverse camera framing need closer alignment.
12. Gold is excessively uniform. Stage planks need colour/length variation and
    a smoother glossy finish, with reflection detail separated from grain.
13. Marquee bulbs do not yet follow the rounded corners at even intervals.
14. A flower plinth obstructs the lift approach; move it beyond the door opening.

These are substantial differences. Iteration 01 does not meet the brief's
"minor differences" threshold.

## Technical checks before visual iteration

- Original pilot podium imported into Unity at 1.46 × 1.812 × 1.08 m, upright;
  four independent button meshes, one collider, one 0–1 UV screen; no errors.
- Fixed a shared primitive mutation bug before the first valid room comparison.
  Reduced static objects by batching repeated decorative elements.
- Initial room: 833,872 total render triangles, including all seating and hidden
  room surfaces (not a camera visibility count). Final counts are in the final
  metrics and Unity validation reports.

## Iteration 02 — rejected

Comparisons: `comparison-02_view-1.jpg` through `comparison-02_view-3.jpg`.

Improved: restored the reference palette, showed the lift in the wide shot,
added upper side seating and banners, added gold scrollwork, corrected inward
normals on organic forms, and reduced noisy normal maps.

Remaining visible differences, worst first:

1. Backdrop looks like a red sheet with broad ripples rather than dense velvet
   folds; wall red is also too saturated. Make the backdrop deeper and tighter,
   darken the velvet and give the walls burgundy wood panels.
2. Podium columns remain hidden inside their bodies and spotlight heads are
   missing from their yokes. Correct geometry and hierarchy before evaluating
   the lighting against the reference.
3. Reverse view still clips outer podiums and gives too much space to the
   ceiling. Tighten podium spacing and move the camera back within the stage.
4. Rear balcony shields are behind its curved fascia. Position them on the
   actual curve, with faces pointing into the room.
5. Compass gold remains hard to read. Its surface needs a more diffuse gilded
   finish and a guaranteed position above the plank tops.
6. Material patterns repeat too conspicuously; randomize plank UV offsets,
   reduce coarse colour modulation and use finer, restrained textile detail.
7. Flowers resemble flat discs, and the boxes resemble closed rectangular
   niches. Add layered tilted petals, leaves, real recesses and separate upper
   side drapes.
8. Large walls and box pilasters need more moulding, and the bare shiny ceiling
   needs a darker finish with simple structural beams.
9. Marquee corner bulbs are uneven. Resample the full rounded perimeter.
10. Gold is too orange, cream stone too brown, and the floor too saturated.
    Adjust material colour rather than applying a global colour grade.

No visual quality acceptance is claimed for this iteration.

## Iteration 03 — geometry correction

Made the main backdrop denser, exposed the luminous podium columns, corrected
spotlight parenting, brought the podiums into the reverse composition and moved
the balcony shields onto its actual curve. The cloth now reads as red velvet.

Still different: excessively even pleats, shallow side drapes, flat floral discs,
closed-looking boxes, sparse facade trim, noisy floor grain and broad unshaped
light. The reference has much richer carved silhouettes and warmer shadows.

## Iteration 04 — architectural detail

Added recessed boxes, layered cornices, upper side drapes, tilted multi-layer
flowers and foreground rails. Reduced the wood texture's coarse grain.

Still different: strong circular practical-light patches on the stage, overly
orange gold, pale stone, regular flowers and weakly separated depth between the
proscenium and backdrop. Some solid faces were incorrectly wound; that also
affected reliable collision and shadow inspection.

## Iteration 05 — depth and surface correctness

Corrected solid mesh winding and the remaining spotlight transforms; moved the
proscenium, tied drapes, truss and bunting forward from the question backdrop.
Reduced podium light spill. Stage, drapes and screen now form separate layers.

Still different: decorative leaves lack the reference's carved lobes, the
reverse angle is too centered, the lecterns are broad, and balcony/rear walls
are too plain. The polished floor has a broader white reflection than the
reference's warm, broken reflections. Live-screen backs differ from the
concept's physically inconsistent two-sided monitor arrangement.

## Iteration 06 — export and scale correction

Corrected stage plank/compass height, solid stage wedges, lift-door pivots and
colliders. Measured the actual FBX-to-Unity coordinates and corrected the
lighting data to `(-x, z, -y)`. Kept one set of live screens facing the players.

Visual differences from 05 remained: simple carved relief, smooth uniform
gold, narrow tassels, repeated curtain forms and an overly clean stage.
No amount of export validation counts as resolving those visual differences.

## Iteration 07 — reverse view

Added dark rear monitor housings and moved the reverse camera off axis. Raised
render resolution to the references' 1672 × 941.

Still different: the reverse view's diagonal was wrong, its foreground compass
was too large, and an EEVEE shadow buffer warning made the high-resolution
lighting result unreliable. Rear walls and box facades still lacked detail.

## Iteration 08 — camera and shadow investigation

Changed the reverse camera diagonal and increased the EEVEE shadow pool.
The camera now gives the closest right-hand podium greater prominence.

Still different: increasing the pool alone did not remove the shadow resource
warning. The overhead frame intruded into the reverse view. Gold and cream were
too pale and lectern columns too bright. These renders are intermediate evidence,
not the final lighting result.

## Iteration 09 — carved relief

Added lobed acanthus relief and midribs around the screen, proscenium and box
pilasters, plus sidewall lattice grilles. Disabled shadows on small practical
point lights, retaining their illumination.

Still different: EEVEE still reported shadow resource exhaustion. Surfaces
were cleaner and more mechanically repeated than the concept, and the main
backdrop folds remained too regular.

## Iteration 10 — stable EEVEE lighting

Kept shadows on the stage spots and the main stage/audience area keys; the
remaining lights provide fill. All three high-resolution renders completed
without the shadow buffer warning.

Remaining differences, worst first:

1. Reverse camera looks past a large nearby screen-frame edge across the top.
2. Broad, regular curtain pleats feel manufactured rather than heavy cloth.
3. The lecterns look short and wide; button decks are too horizontal.
4. Wide and close composition differ in screen size, eye height and flower crop.
5. Upper/side walls have large plain areas; boxes only face the main audience.
6. Gold/ivory and luminous columns are too bright and pale.
7. Stage reflections are broad white patches rather than warm broken highlights.
8. Petals and leaves are repeated and less full than the reference bouquets.
9. Carvings and wreaths remain simpler; fascia, pilasters and railings are more
   regular. Fine stitching, tarnished crevices and carved normal detail are weak.
10. The hall feels less intimate, especially when viewed from the stage.

## Iteration 11 — material and architecture refinement

Varied pleat phase, amplitude and hem height; warmed gold and cream, reduced
practical emission, added inward-facing side boxes, layered rear-wall frames,
and corrected the rear sconces to face the hall. Moved the reverse camera in
front of the screen so the intrusive frame disappeared. The close camera now
includes the left flower plinth and a less elevated view of the podiums.

Rejected changes: lowering the wide camera placed a balcony rail across the
stage. The reverse camera clipped the nearest lectern. The flat button decks,
wide lecterns and broad floor reflection still needed correction. Flowers and
fine carved texture remained less convincing than the concept.

## Iteration 12 — lecterns and framing

Restored wide-camera rail clearance. Narrowed the podiums to 1.241 m without
changing their height, tilted the button decks by 18 degrees, preserved all
16 separate buttons and their press directions, and adjusted the reverse view
to include the nearest lectern. Increased floor roughness and softened its
clearcoat to reduce the broad hot reflection.

## Iteration 13 — supported decks and distant seats

Raised each angled button deck from its front edge and added a solid wood wedge
under it. Added collision for the two side-facing theatre boxes. Kept all seat
features but reduced bevel subdivisions in distant rows and balconies. This
removed 232,704 source triangles; the imported Unity room was 860,100 triangles.

The three compositions remain coherent views of one room. The closer view now
shows four slender lecterns, left/right flowers, deeper side drapes and the full
question frame. The reverse shot includes all four lecterns without the screen
frame occlusion. Large differences remain in the rear hall's intimacy, the
concept's full bouquets, detailed mouldings and softer material response.

Unity static-fit screenshots at 1.8 m exposed overly visible bouquet stems and
an indicator triangle whose backface was hidden in Unity. Those are corrected
in the next pass, not excused as lighting differences.

## Iteration 14 — flower silhouettes and Unity backface fix

Replaced spherical petal discs with curled surface petals in three nested
layers, increased the bloom count, added dense foliage behind the flowers and
shortened exposed leaves. Added more purple blooms. Corrected the down-arrow
winding so both lift indicators face the room in Unity.
The final Unity close-up also required two-sided petal materials: backface
culling otherwise opened holes where the undersides face the camera.

## Round 1 assessment — WIP, visual gate remains open

The subsequent anti-aliasing/polish review is in `../round-02/review.md`.
The latest refinement is iteration 21: all three concept comparisons are here,
and `../round-03/review.md` records its before/after images, close-ups, actual
Unity comparisons and current differences. Counts and test results below
record the earlier iteration 14 checkpoint.

The deliverable contains the requested room elements and is technically
importable. Fourteen render passes are not themselves evidence that the
reference quality bar is met. I cannot honestly call all remaining differences
minor. Keep this as a review checkpoint, not an approved final-art handoff.

Visible differences in iteration 14, excluding the required omissions above:

| Priority | View | Remaining difference | Why it remains / next correction |
|---|---|---|---|
| 1 | All | Surfaces and ornament look cleaner, simpler and more mechanically repeated than the feature-animation reference. | Procedural trim and analytic texture maps do not yet supply enough sculpted recess detail, local wear or varied edge shapes. Build richer hero trim, bake its fine relief and compare close crops. |
| 2 | 3 | The hall feels wider and less intimate; more ceiling and plain wall remain visible. Rear balcony scale and camera perspective do not match exactly. | One physically consistent room follows image 1, but the reverse composition still needs a tighter architectural/camera match. The reference's rear lift and reversible podium fronts conflict with image 1 and are deliberately not copied. |
| 3 | 1, 2 | Drapes remain too evenly pleated and red; tied panels are slimmer and tassels simpler. | Fold phase/hem variation is improved, but the shapes still come from repeated analytic folds rather than a sculpted or simulated heavy cloth shape. |
| 4 | All | Stage illumination is broader and smoother; reflections sometimes wash pale. Gold lacks some of the reference's warm, broken specular detail. | The current EEVEE arrangement uses broad fills. Refine the pools and material roughness locally; Unity needs its own bake/probes before an engine match can be judged. |
| 5 | 1, 2 | Question-frame ornament and inner panel silhouette are simpler; the screen occupies a different proportion of the view. | Rounded-rectangle framing and repeated acanthus approximate the motif rather than reproducing the shaped inner corners and individual carvings. |
| 6 | 2 | Flowers are smaller and less lush; foliage still forms a regular mound. Plinths are plain, with simpler marble detail. | Curved petals improved the silhouette and reduced exposed stems, but the procedural substitute does not yet reproduce the reference's varied rose/daisy bouquet. No suitable connected AI 3D provider was available. |
| 7 | 1, 2 | Podium spacing, lectern width/depth and monitor sizes do not project exactly like the references. Light columns are more uniformly white; brass is less richly layered. | Physical player scale is verified, but camera matching and local shape refinement remain. Keep all interaction pivots stable during refinement. |
| 8 | 1, 3 | Balcony fascia, handrails and shields are thinner and more regular; upper seating is less visible. | Missing people are intentional, but the architecture also needs more substantial mouldings and a closer height/depth relationship. |
| 9 | 2, 3 | Compass reads pale in highlights and its star/ring proportions differ. Stage boards are too uniform in spacing and finish. | Material contrast and inlay shape need another local pass; avoid solving this with a global exposure change. |
| 10 | 1, 2 | Floor lights, truss heads, lanterns, grilles and bunting are simpler, with different spacing and larger unbroken surfaces. | All requested objects are present and reactive lights have pivots, but their hero close-up modelling is still less detailed. |
| 11 | 1 | Lift placement follows image 1, but its surround, indicator, door finish and neighboring box proportions are not exact. | The opening and cab follow shared sizes. More carved layering and a directional brushed-metal finish are needed. |
| 12 | All | Upholstery is smoother and has less stitching/cushion deformation. | Shared near/far meshes preserve silhouettes and trim, but stitched normal detail is not yet present. |

The missing host, crowd, text, confetti and light shafts are required by this
round's scope, not unfinished mesh work. They explain some lost celebration and
visual density, but do not excuse the material and modelling gaps listed above.

## Final technical evidence

- Blender audit: no contract problems. 901,052 source render triangles; 480
  render objects; 229 unique mesh datablocks; 24 materials.
- Unity import: no validation problems. 846,661 imported triangles; 79 solid
  collision meshes; 16 buttons; 5 live-screen meshes; 43 light markers. Unity
  removes degenerate pole triangles and triangulates differently from the
  conservative source count.
- Camera-frustum bounds: 700,732 / 569,438 / 822,478 source triangles. These
  deliberately include occluded geometry and are not GPU visibility timings.
  The roughly 500k target has not been demonstrated. The biggest costs are
  architecture/detail (362,164), seats (342,872), flowers (83,824), curtains
  (76,128) and podiums (36,064).
- Texture maps: 102 PNGs, 16 material sets at 2K and 8 at 64 pixels; no 4K.
- Static Unity scale check uses the existing KyleRobot at measured 1.8 m.
  Door head clearance is 0.7 m; clear podium gaps are 1.959 m. Walking and
  camera comfort remain an integration playtest, not a passed check here.
- Existing project regressions: 82 EditMode + 21 PlayMode tests passed.
- EEVEE final renders have no shadow buffer exhaustion. Blender reports an
  unavailable thumbnail-cache path during save and a cleanup warning on exit;
  the saved blend reopens, all three render files exist and the independent
  audit passes. Linked-seat FBX material-index warnings were also inspected;
  imported renderers use the expected URP materials. Do not describe the export
  logs as warning-free.
- The Unity preview is an import/scale check with diagnostic fill. It has no
  baked indirect light or reflection probes and is visibly below the EEVEE
  presentation. Production engine lighting is not claimed complete.
