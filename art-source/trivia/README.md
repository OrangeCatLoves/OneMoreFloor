# Trivia round 3 - art review and import notes

Round 3 follows `docs/floors/trivia_art_feedback.md` ("Round 3", 19-21 November 2026). Claude Code
built it (owner, 26 November): `tools/blender/trivia_round_three.py` runs after the round 2 pass and
changes only what round 3 asks for. The hall's outer dimensions stay unchanged. This is an art review
awaiting the owner's playtest, not a completed Trivia game floor.

## What changed (Unity metres)

- **Lift** at the centre of the rear wall (door plane z = 26, floor y = 1.2), facing the stage. The
  doors open straight onto the rear cross-aisle. The leaves stand just behind the wall and slide apart
  behind it, into the cab (the review builder opens them by half the door width each). Flower urns
  flank the portal; two gallery doors sit above it. Cab interior from `docs/sizes.json`, floor flush
  with the cross-aisle, gold threshold flush with both.
- **Lift: Showtime Marquee (owner's pick, 27 Nov):** red velvet frame (0.42 m bands, 0.5 m over the doorway,
  topping out at 4.2 m under the rear gallery) lined with bulbs, gold trims open at the floor (no lip), two
  stars, globe lamps on gold stems (they replace the wall lanterns; the urns stay), gold doors with a star and a
  panel line. Cab: velvet walls, gold pilasters and rails, carpet, burgundy ceiling with a smaller light panel and
  rows of bulbs, a gold star on the back wall. Two bulb floor signs, outside in the frame's top band and inside
  over the door (mirrored to read from the cab): one object per segment, `SM_LiftSign_<Out|In>_<Tens|Ones>_<a-g>`,
  so the game lights the floor number. `COL_Trivia_LiftFrameGuard_L/R` keep players and the camera off the
  frame's sides (PlayerBlocker layer); globes and pilasters get decor proxies.
- **Screen markers (made in Unity, 27 Nov):** the game can't read mesh shapes while playing (the model
  isn't Read/Write), so `TriviaReviewSceneBuilder.AddScreenMarkers` adds `MK_Screen_Question`,
  `MK_Screen_Players` and `MK_Screen_Podium_01`-`04` when it builds the scene: each sits at its screen's
  centre, its forward (+Z) points out of the screen and its scale is the screen's width and height in
  metres. Trivia draws its question boards on them. The builder also adds the game's run starter
  (`Assets/_OneMoreFloor/Game/Run/RunBootstrap.prefab`), so Play in this scene plays the Trivia floor.
- **Markers:** `MK_Spawn_01`-`04` in the cab at x = +/-1, z = 27.9 and 29.3; `MK_Exit` at (0, 1.2, 25.3).
  `MK_PlayArea_01` (hall, aisles, cross-aisle and stage, under the galleries) and `MK_PlayArea_02` (lift
  cab) are empties whose position is a box centre and whose scale is its size in metres.
- **Seating:** 10 rows each side at 1.3 m pitch from z = 8.3, each 0.12 m above the one in front; the
  cross-aisle (y = 1.2) is "row 10". Rows are audience-only: the passages are narrower than a player.
- **Aisles:** the centre aisle (x +/-1.5) and both side aisles (|x| 10.4-14.4) are ramps from y = 0 at
  z = 7.65 to y = 1.2 at z = 20.65 (about 1 in 11). Red carpet with gold stars from the lift to the
  front carpet, which runs round the stage front to both flights.
- **Stage:** curved flights at both front corners (|x| 3.6-6.8 along the stage edge), two treads of
  0.2 m and 0.3 m going, lit risers, gilded nosings and cheeks. The vertical bars on the stage front and
  the rail in front of the first row are gone.
- **Galleries** stay as audience seating (one row added along each side gallery) but can't be reached:
  the in-hall stairs are gone and the deck is continuous.
- **Tie-backs:** two-ply twisted gold rope round each gathered drape, a slack run onto a fluted holdback
  on the proscenium pillar, a knot and a heavy tassel.
- **Screens:** the players' TV (1.4 m live surface, same aspect as the big screen) and the four podium
  monitors stand upright. The TV's back is walnut with a gold frame and laurel.
- **Functions:** front-of-house desk at the back-left of the cross-aisle (dressed walnut console, ivory
  posts, operator's chair); broadcast cameras at the back-right and on both sides of the stage.
- **Light:** two chandeliers over the audience (z = 10 and 18), an ivory ceiling with gilded beams both
  ways, more audience fill, a lift portal key and a cab light. The review camera exposure is 2.3.
- **Collision:** `COL_Trivia_DecorProxy` boxes cover curtains, pillars, wall pilasters, grilles, podium
  monitors, cameras, the lift portal and the cab handrails; `COL_Trivia_ApronEdgeGuard_*` keeps players
  off the stage-edge trims.

## Checks

- Room checks (Claude Code): `unity test . --mode EditMode --filter TriviaRoomChecks`, report in
  `Temp/OneMoreFloor/TriviaRoomChecks.txt`. Round 3 passes all 7: play area, no walking into visible
  meshes, routes, no small floor edges, upright screens, brightness 0.36 (target 0.35).
- Walkthrough (PlayMode): `TriviaReviewWalkthroughTests` walks out of the lift, down the ramp, up a
  corner flight, to all four podiums, then down the other flight and round both side aisles.
  Captures in `../renders/trivia/round-06/`.
- RTX 3060, Editor Game view 1718x806: about 175 fps uncapped. 1,167,199 render triangles.

## Rebuild

From the laptop, with Unity open on the home PC: `tools/homepc/build-trivia.sh --iteration <n>`.
It builds in Blender on the PC, rebuilds and bakes the review scene there, and copies everything back.

---

# Round 2 notes (historical)

Round 2 follows `docs/floors/trivia_art_feedback.md` (11 November 2026).
The hall's outer dimensions stay unchanged. This is an art review awaiting the
owner's next playtest, not a completed Trivia game floor. Stop further refinement
until that feedback arrives.

Numbered before/after Unity captures, the spatial audit and the object-name
change list are in `../renders/trivia/round-05/`. Earlier rounds remain archived.

## Open the room

Unity menu **OneMoreFloor > Art > Open Trivia Review**, then **Play** and click
Game view. Walk out of the lift, turn left around the stage corner, and climb the
illuminated side steps. **WASD** moves, **mouse** looks, **Space** jumps,
**V** switches first/third person, **scroll** adjusts camera distance, and
**Escape** releases the cursor. This uses the existing local-host player from
MovementTest. It has no Trivia question/answer gameplay.

Claude Code should run `tools/homepc/sync.sh` and open this scene on the owner's
home PC for the next playtest. Do not edit that copy. The large local art is
still excluded from Git pending owner approval; ordinary source commits alone
will not transfer it.

## Local deliverables

| Path | Contents |
|---|---|
| `art-source/trivia/Trivia_Round01.blend` | Current editable round-2 room; legacy filename retained |
| `Assets/_OneMoreFloor/Art/Floors/Trivia/SM_Trivia_Theatre.fbx` | Separate static/reactive parts, collision meshes and markers |
| `Assets/_OneMoreFloor/Art/Floors/Trivia/Textures/` | 110 PNG maps across 25 shared materials |
| `Assets/_OneMoreFloor/Art/Floors/Trivia/Materials/` | URP Lit, Complex Lit wood, and Unlit question-screen materials |
| `Assets/_OneMoreFloor/Art/Floors/Trivia/Review/TriviaArtReview.unity` | Walkable scene with baked lighting and saved reflection assignments |
| `art-source/trivia/lighting.json` | Exact light positions, directions, targets and purposes |
| `art-source/renders/trivia/lighting_sheet.md` | Human-readable lighting sheet |
| `art-source/renders/trivia/round-05/name-changes.md` | Every added, removed or changed imported object |

## Updated layout and interaction contract

- Four podiums face the audience (+Z). `MK_Objective_01` to `04` stand at
  Unity z=1.45, y=0.6, facing +Z; their x values remain 4.8, 1.6, -1.6, -4.8.
  The four-metre strip behind those positions stays clear for the camera.
- Status monitors sit outboard on their desk-mounted stems and turn toward
  their players. This leaves the shared TV visible. The answer decks slope
  **12 degrees**; all 16 buttons remain separate, with their original names.
  Press direction at the identity room pose is **(0, -0.978148, +0.207912)**.
  Use the FBX custom property or this vector, then restore the initial pose.
- `MK_Host` is **(-6.4, 0.6, 3.1)**: downstage of the players, opposite the lift,
  with a clear audience view and no obstruction across the shared TV.
- `SM_Trivia_PlayerScreen` is the low shared question display near the centre
  of the stage front. Its live surface is **1.4 m wide**, tilted **25 degrees
  up**, in a supported black housing with a routed cable entering the stage.
- The big live panel `SM_Trivia_QuestionScreen` is **12.15 x 4.10 m**;
  both question surfaces have aspect **2.9634146:1**, clean 0-1 UVs and no text.
  Bind the same question image to both. The four `SM_Trivia_PodiumScreen_nn`
  surfaces remain separate status displays; assign textures only to the named
  live surfaces, not the static monitor backs or control-desk equipment.
- Stage steps wrap the lift-side corner: 3 m width, 0.3 m treads, 0.2 m rises,
  read from `docs/sizes.json`. The red aisle joins the front cross-aisle and
  leads to these steps. The lift landing remains clear for four players.
- Seven ground seating rows use 2 m pitch and share the far-seat mesh.
  The rear cross-aisle serves the control desk and both gallery staircases.
  The side galleries are clear promenades; rear gallery seating has its own
  passage. Two side broadcast positions and wall sconces serve the hall.

## Import and lighting

Instantiate the FBX at world origin, rotation identity, scale 1. One Blender
unit equals one metre. Measured conversion is Blender `(x,y,z)` to Unity
`(-x,z,-y)`. Source mesh rotation and scale are applied. `export_unity.py` now
rebases object and mesh coordinates explicitly before FBX export: Blender's
automatic space bake introduced a rotation and offset below nested lamp pivots.
The fixed exporter preserves geometry, all marker axes and parent-child poses.

Disable every `COL_` renderer and add a non-convex MeshCollider. Keep static
collision assemblies separate by role; preserve button, lift-door and lamp
hierarchies. Parent door colliders to the matching moving leaves while keeping
world poses. Lift leaves slide along Unity Z. Spotlight pan remains local Y in
Unity and tilt local X. New `MK_Trivia_Spotlight_nn_OpticalAxis` children identify
the lens; its **negative local up** is the light direction. The Unity audit also
checks the actual lens face normal and lens position, not only the marker.

The review has **50 source light markers**: **11 areas + 27 points baked**, and
**12 realtime spots**. Each area supplies one restrained realtime highlight
spot; four sources cast realtime shadows. Positions and aims come from
`lighting.json`; use `Quaternion.LookRotation(direction_unity)` for each lamp.
All twelve stage lamps have specific targets in the lighting sheet. Point
sconces are diffuse emitters; the broad area sources are review fill.

Use **OneMoreFloor > Art > Build and Open Trivia Review** to regenerate and bake
after an export. This overwrites the art-owned review scene and its generated
lighting assets. Current measured receiver/map counts are in
`baked-lighting-validation.json`; there are 113 light probes for moving objects.

Keep both reflection probes in **Custom** mode with `customBakedTexture`, and
retain the indoor fallback reflection. Bake with existing reflection feedback
disabled; import EXRs as linear uncompressed HDR. Audits reject empty, nonfinite
or excessive HDR values. The temporary review pipeline enables **4x MSAA + High
SMAA**, restores previous settings when disabled and does not change shared
project graphics assets. Texture maps retain mipmaps, trilinear filtering and
anisotropic level 8.

## Budget, evidence and limitations

`unity-validation.json` is the authoritative imported triangle/object count.
Round 2 must remain below 1,549,521 total triangles. Flower tessellation was
reduced and all seating now shares the far-seat mesh to pay for the additions.
The separate approximately 500,000-visible-triangle goal still needs production
visibility/LOD work and profiling; a lower total is not proof of frame rate.

`round-05/unity-audit.json` checks screen aspect/UV orientation, all 12 actual
lamp lenses and 20 player-to-TV sightlines. `source-audit.json` checks cloth
intersections and floor-fixture support. The whole-room coplanar audit records
same-facing overlaps above 0.00001 square metres, including within merged
meshes. Its remaining candidates need the classification in the review journal;
hidden construction contacts are not an assertion of exposed z-fighting.

The automated Unity walkthrough exercises the existing player motor, the lift,
stage, all seating passages, control position, both stairs and both galleries.
It captures first/third person. Crowd comfort and final appearance require the
owner's playtest. Host/audience characters and Trivia gameplay are outside this
round. Cloth response, repeated ornament and flower close-ups remain less
natural than the concept images. This is not final art acceptance.
