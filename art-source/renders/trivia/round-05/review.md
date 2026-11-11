# Trivia round 2 - owner feedback review, 12 November 2026

This round implements the nine items in `docs/floors/trivia_art_feedback.md`.
The hall's outer dimensions are unchanged. Work stops here for the owner's
next playtest; this is not final art acceptance or a finished gameplay floor.

Every comparison below uses the same saved Unity camera before and after.
Each half is an actual 1280 x 720 URP capture, with 4x MSAA and High SMAA.
The JPEG assembly only places the captures beside one another and adds labels.
Original PNGs remain local. These are engine captures, not concept paintovers.

## The nine items

| Item | Change and check | Unity before / after |
|---|---|---|
| 1. Curtain tie-backs | Replaced the disconnected band with two complete ropes fitted around the gathered cloth, an arch-mounted boss, a knot and suspended tassel. The source intersection audit finds no rope, anchor or tassel triangles passing through the tied curtain. | [Tie-back](01-tieback.jpg) |
| 2. Lift floor flashes | Split the hall floor at the cab opening and added a 24 mm threshold. The old 7.5 square metres of coincident hall/cab top faces are gone. Whole-room overlap scan and actual player crossing pass. | [Threshold](02-lift-threshold.jpg) |
| 3. Floor lamps | All four heads aim upward at recorded curtain targets. Corrected an FBX export defect that displaced/rotated meshes below nested lamp pivots. All twelve imported lamp lenses now agree with their optical markers, light directions and targets. Floor bases sit on the stage; overhead units have brackets and truss suspension. The after camera sees the rear of the correctly aimed foreground lamp. | [Floor lamps](03-floor-lamps.jpg), [overhead fixtures](07-full-screen.jpg) |
| 4. Podiums face the audience | Turned the four assemblies 180 degrees; objectives face +Z at y=0.6, z=1.45. Kept 4 m behind them clear. Monitors move outboard and angle toward players; answer decks now slope 12 degrees to preserve the TV sightline. Host moves to (-6.4, 0.6, 3.1), downstage and opposite the lift, visible to players and audience without blocking the TV. | [Player facing](04-player-facing.jpg) |
| 5. Shared player TV | Added one low 1.4 m-wide live screen, tilted up 25 degrees, with housing, hinge, stand, foot, cable and stage entry gland. Its aspect and 0-1 UV orientation match the big screen. All twenty rays from the four actual player eye heights to the TV's corners and centre clear the podiums and TV housing in Unity. | [Player TV](05-player-tv.jpg) |
| 6. Lift-side stage steps | Removed centre steps. A 3 m-wide corner landing and 0.3 m-deep / 0.2 m-high treads lead from the lift landing to the stage. Values come from `docs/sizes.json`. Moved urns out of this route and joined the red centre aisle to a front cross-aisle leading to the steps. The motor walks this route without jumping. | [Side steps](06-lift-side-steps.jpg) |
| 7. Full-width big screen | Removed the purple panels and their laurel/carving; the live panel fills the bulb frame. The live dimensions are 12.15 x 4.10 m, aspect **2.9634146:1**. The outer frame, bulbs and separate chase meshes remain. | [Big screen](07-full-screen.jpg) |
| 8. Rear and side functions | Added a rear sound/lighting control position with supported equipment and cable tray, two side broadcast camera positions, regular aisle sconces and a continuous accessible gallery. Seven ground seating rows retain a clear central aisle and rear cross-aisle. No invented host, audience or screen text. | [Rear](08-rear-function.jpg), [sides](08-side-function.jpg) |
| 9. Rest-of-room audit | Corrected unusable row spacing, disconnected galleries, obstructing side boxes, floating rail/fixture details and several exposed construction overlaps. Walked the routes using the existing Unity player motor. Details below. | [Circulation](09-circulation.jpg), [gallery](09-gallery.jpg), [row clearance](09-row-clearance.jpg) |

## Additional problems found and fixed

| Problem | Fix and evidence |
|---|---|
| Ground seat rows had too little clear passage; broad seat-tier colliders blocked row access. | Rebuilt seven rows on 2 m pitch with individual seat collision and continuous tier floors. All seven passages are walked across both seating blocks. [Row comparison](09-row-clearance.jpg). |
| Balcony segments and seats did not form a supported, reachable circulation route. | Continuous horseshoe deck, two rear stair flights, usable bottom/top landings, physical guards, rear seat collision and clear side promenades. [Gallery comparison](09-gallery.jpg). |
| Side theatre boxes projected across the gallery route. | Removed only the obstructing side pair; stage wing boxes remain. Raised retained lanterns above head height and added wall backing blocks. [Side comparison](08-side-function.jpg). |
| Rail posts, fascia and shields did not consistently meet their deck; duplicate posts/bulbs overlapped at joins. | Rebased rails/fascia to the gallery, attached shields, removed duplicate posts/bulbs and offset the exposed fascia/deck overlap. [Gallery comparison](09-gallery.jpg). |
| Front audience guard extended into the side entry path. | Shortened it to leave side access and added physical collision. [Step comparison](06-lift-side-steps.jpg). |
| TV was initially partly hidden by button decks/monitors; its rounded housing clipped corner rays. | Outboard monitors, shallower button decks and a slightly wider housing opening. Verified all twenty actual imported-mesh sightlines. [Player TV](05-player-tv.jpg). |
| Stage corner conversion exposed jagged plank ends and apron trim at the new landing. | Landing covers the cut ends; removed only the apron section inside the join. [Step comparison](06-lift-side-steps.jpg). |
| Fixture heads and emitted light diverged after FBX import, despite correct Blender transforms. | Rebased mesh/object coordinates explicitly during export, retaining the original scene afterwards. Unity checks actual lens geometry and position, not just marker direction. [Lamp comparison](03-floor-lamps.jpg). |
| Lamp bases floated above the stage; overhead lamps lacked a visible mounting connection. | Bases lowered to stage y=0.6; added mounting brackets, truss clamps and suspension. [Lamps](03-floor-lamps.jpg), [rig](07-full-screen.jpg). |
| Rear control desk needed a working-height base; monitor supports/cable tray overlapped exposed desk faces. | Two floor-supported equipment racks carry the desk; stands and cable tray meet it from above/below. [Rear comparison](08-rear-function.jpg). |

The shallow backing blocks also close the retained gallery lanterns' wall gap;
new aisle sconces connect through their backplates. These are support repairs,
not additional decorative filler.

## Measured space and import contract

Clear geometry measured from the imported meshes (`clearances.json`):

| Route | Clear width / depth |
|---|---:|
| Between ground seating rows | 1.291 m |
| Narrowest ground side route, beside gallery stair | 1.463 m |
| Main centre aisle | 3.000 m |
| Rear cross-aisle | 2.100 m |
| Side gallery, guard to pilaster | 1.665 m |
| Rear gallery passage behind seats | 2.156 m |
| Upper seats to stair guard | 1.813 m |
| Bottom gallery stair landing | 1.400 m |
| Behind the four player objectives | 4.000 m |

These dimensions and the motor test establish traversability. They do not
establish how comfortable four humans and their cameras feel together.

All changed imported names, pivots, axes, bounds and triangle counts are listed
in [name-changes.md](name-changes.md), with full data in `name-changes.json`.
The matching inventories include the review's open lift-door display pose.
No blanket claim that every old pivot or mesh stayed unchanged is made.

The six live surfaces are the big question panel, the shared TV, and four
podium status monitors. Bind one question image to both question surfaces.
Button press direction at the identity room pose is
**(0, -0.978148, +0.207912)**. Import, lamp axes and rebuild instructions:
[current art README](../../../trivia/README.md).

## Whole-room coplanar audit

`before-coplanar.json` and `after-coplanar.json` test all imported render meshes,
including pairs inside merged meshes. The scan compares same-facing planes
within 0.1 mm and reports overlaps above 0.00001 square metres. Opposite faces,
edge-only contacts and smaller triangles are excluded. Pair areas sum triangle
intersections; they are diagnostic areas, not necessarily unique surface area.

The hall/lift top-face defect is removed. The exposed gallery fascia/deck join,
duplicate balcony pieces and control-desk contacts found during iteration were
also corrected. The final diagnostic still reports 31 construction/decorative
pairs. They have not been silently filtered out:

- Stage/landing and stage/wing-wall: coincident underside construction faces;
  the walkable landing is above the stage base, so these are not duplicate
  exposed walking surfaces.
- Wall corners, ceiling/side-wall, balcony/pilaster and cab-wall/ceiling joins:
  embedded structural edges. Open lift leaves also intersect their display
  pockets/jambs; their sliding poses remain for Claude Code's integration.
- Box cornice/pilaster, podium self-pairs, handrail and truss junctions:
  joined trim/assembly contacts, including same-material surfaces. No visible
  flashing was observed in the saved Unity views; these are retained contacts,
  not a claim of a mathematically intersection-free solid.
- Four cross-button self-pairs: the two white strokes of each cross overlap.
- TV cable gland/foot: shared hidden bottom contact at the stage surface.
- Tiny tier and balcony-bulb contacts: residual junction slivers, recorded in
  the raw report. They are below 0.0001 square metres per listed pair.

Moving-camera shimmer and any remaining exposed contact should still be
reported during the owner's playtest. The audit is evidence, not a substitute
for that check.

## Validation and limits

The final rebuild, bake, captures and checks completed successfully after the
wall-support repair. **82 EditMode and 22 PlayMode tests passed, none skipped.**
Source geometry, imported geometry/material bindings, reflection HDR and round-2
audits report no failures. Unity reports:

| Measure | Final |
|---|---:|
| Imported triangles | 997,614 (35.6% below iteration 22) |
| Render objects / colliders | 397 / 96 |
| Separate buttons / live screens | 16 / 6 |
| Source light markers | 50 |
| Materials / texture maps | 25 / 110 |
| Baked receivers / light probes | 185 / 113 |
| Lightmaps | 3: 1024 square, 256 square, 256 square |
| Source flower triangles | 224,382, down from 434,223 |

The source total is 1,039,074 triangles; Unity's imported total above is the
performance ceiling measurement. All 648 changed/added/removed object records
are in the name-change report. Source cloth intersections, twelve imported
lamp alignments and twenty imported TV sightlines pass. The raw reports are
beside this journal and under `../../../trivia/`.

The walkthrough uses the existing local-host player and motor, exits the lift,
climbs the corner steps, reaches all four objectives, crosses every ground row,
walks both side aisles and the control position, climbs/descends both gallery
stairs and traverses both galleries and the rear gallery passage. It saves
[first- and third-person captures](playtest-both-views.jpg). It does not use
teleporting to pass routes.

Flower tessellation and seating cost were reduced to pay for the new pieces.
All seats use the existing far-seat mesh. The whole-room total is below the
1,549,521-triangle limit; the separate target of roughly 500,000 **visible**
triangles is still unproven without visibility/LOD work and profiling.

Remaining visual gaps are candidly visible in these captures: repeated
ornament and flower forms, regular curtain folds, and a simpler velvet response
than the concepts. The hall remains deliberately darker than the stage, and
the control/camera equipment is visually plain. Screen text readability, moving
edge shimmer, four-player comfort, laptop performance and two-player networking
in this room are not signed off. Host/audience characters and Trivia rules
are outside this round.

Claude Code's next action is to sync the local art and review scene with
`tools/homepc/sync.sh`, then open the room on the home PC for the owner's
playtest. Large art binaries and generated Unity metas remain local pending
approval; the source, reports and small JPEG comparisons are committed.
