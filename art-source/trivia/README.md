# Trivia round 2 - art review and import notes

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
