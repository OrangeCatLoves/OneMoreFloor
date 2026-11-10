# Art brief — Trivia, game-show stage

For Codex. This is the standing brief for the art loop (`docs/PROJECT_CONTEXT.md` section 9). Read it with `AGENTS.md` and `docs/floors/trivia.md`.

The owner's playtest feedback is in `trivia_art_feedback.md`. Where the two files differ, the feedback wins. The lines below that it changed are already updated.

## The quality bar
The owner's 3 concept images are the target, not loose inspiration: `art-source/concept/floors/trivia/` (`trivia-1.png` to `trivia-3.png`, or their new names once logged). Someone seeing your work next to them should say it is the same place. The lighting, the richness of detail and the mood matter as much as the shapes.

This room is the game's first impression and its best trailer shot. Treat it as a hero environment for a shipped game. A first pass is never the finished work: see "Check your own work" below.

## Art direction
**The concept:** a prime-time televised game show staged in a grand old-world theatre. Opulent, celebratory and glamorous, with the buzz of a live broadcast. The look is stylized realism with the polish of a feature-animated film: believable materials and lighting, slightly exaggerated shapes, nothing flat or plasticky.

**Palette:**
- Dominant: crimson and burgundy velvet, royal purple, antique gold and brass.
- Supporting: warm walnut wood, cream and ivory plaster and marble.
- Accents, only where the images use them: saturated red, blue, yellow and green on the answer symbols.

**Lighting mood:** warm, theatrical and high contrast. Pools of amber key light against rich, darker surroundings, with purple-gelled stage light as the secondary colour. Dozens of small practical lights (bulbs, sconces, lit edges) make the room sparkle. Highlights glow softly. The polished stage floor reflects all of it. Light should visibly fall off into shadow, giving depth from the bright stage to the darker hall.

**Detail language:** ornate and layered, never bare. Every large surface has moulding, trim, fabric folds, inlay or lights. Detail is richest around the stage and simplifies with distance from it.

## What to build
Where the images disagree, `trivia-1.png` wins.

**Stage**
- A curved stage front with gold trim and a lit edge, raised above the hall.
- Polished warm wood planks, glossy enough to show clear reflections.
- A gold star-compass inlay in a gold ring at centre stage, where the host stands.
- Steps with lit risers on the side of the stage nearest the lift, on the direct route from the lift doors.
- One low TV for the players at the centre of the stage front, facing them and tilted up, showing the same content as the big screen.
- Compact floor-standing stage lights at the corners of the stage, each tilted up at a specific target.

**Proscenium and backdrop**
- A grand gilded proscenium arch with carved mouldings, topped by a purple-and-gold shield crest with a gold star.
- Deep crimson velvet drapes with heavy, sculpted folds and gold fringe. Thick gold rope tiebacks with tassels wrap around the gathered curtains and tie off to the wall.
- Gold and purple triangular bunting strung across the top.
- An exposed lighting truss with moving-head spotlights.

**Big question screen**
- A large ornate frame behind the host: gilded, with a running marquee of round bulbs all the way around.
- A cream panel that fills the whole inside of the frame, with no side panels, left blank (see "Rules for the game to work").

**4 podiums, spaced across the stage**
- Turned so the players face the audience: the players stand on the big-screen side, and the emblem faces the hall.
- Warm wood lecterns with brass trim and a gold laurel emblem on the front.
- Glowing vertical light columns at the front corners.
- An angled, gold-framed dark monitor on top, left blank.
- A deck of 4 chunky illuminated arcade buttons: red circle, blue square, yellow X, green triangle.

**Hall**
- Tiered rows of burgundy velvet seats with gold trim, empty for now.
- A red carpet aisle with gold stars, leading to the stage front.
- Gold handrails along the front of the seating sections.
- A curved upper balcony with a cream fascia lined with small bulbs, and purple banners and gold-star shields along it.
- Theatre boxes on the side walls, with arches, drapes and gold lanterns.
- Burgundy panelled walls with gold lattice grilles and glowing wall sconces.
- Cream marble plinths with gold urns of purple, white and cream flowers.

**Lift**
- Brushed gold double doors in an arched gold surround, with an up and down indicator above, beside the stage as in `trivia-1.png`.
- Flanked by sconces and flower plinths.

**Not in this round:** the host and the audience. Leave the seats empty. Confetti and light beams are effects that Claude Code adds in Unity.

## Materials
- Physically based materials baked to textures: base colour, normal, roughness/metallic, and emissive where things glow. Blender node materials don't carry over to Unity.
- Nothing is uniform. Vary the roughness and colour: slightly worn gold in the recesses, tarnish in the mouldings, wood grain and fine scratches on the stage.
- Velvet needs its soft sheen and dark folds. Gold must read as metal, not yellow paint.
- Use normal maps for fine carving and stitching, and real geometry for silhouettes and large forms.

## Lighting hand-off
Blender lights don't transfer to the game, so the room's lighting is rebuilt in Unity by Claude Code from what you provide:
- An `MK_Light_<type>_<nn>` empty for every light, aimed where the light should point.
- `art-source/renders/trivia/lighting_sheet.md`: each light's type, colour as a hex value, relative intensity, cone angle and purpose, plus notes on the overall mood.
- Glowing things (bulbs, sconces, button faces, light columns, screen frame) use emissive maps.

## Rules for the game to work
**Scale.** 1 Blender unit = 1 m. The player is 1.8 m tall. Take door and lift sizes from `docs/sizes.json`. Leave clear walking room around the podiums and steps: a third-person camera sits about 4 m behind the player.

**Separate objects.** Anything that moves or reacts is its own object with a sensible pivot:
- Each lift door.
- Each of the 16 podium buttons, able to press in.
- Each moving-head spotlight, able to pan and tilt.
- Marquee bulbs grouped into at least 3 repeating sets, so they can chase.

**Blank screens.** The big screen's panel, the players' TV and each podium monitor are separate flat meshes with clean 0–1 UVs and no text, so the game can show live questions. Don't bake any words into textures.

**Collision.** Add simple `COL_` meshes for floors, stage, steps, walls, seating, podiums and plinths.

**Markers.**
- `MK_Spawn_01` to `04` by the lift.
- `MK_Objective_01` to `04` where each player stands at a podium.
- `MK_Host` at the star inlay.
- `MK_Exit` at the lift.

**Names and export.**
- Follow the prefixes in `AGENTS.md`, for example `SM_Trivia_Podium`, `SM_Trivia_ProsceniumArch`, `M_Trivia_VelvetRed`, `T_Trivia_StageWood_BaseColor`.
- FBX, transforms applied, facing +Z in Unity.
- Assets go in `Assets/_OneMoreFloor/Art/Floors/Trivia/`, and the `.blend` files in `art-source/`.

**Performance.** It has to run on a mid-range laptop:
- Aim for about 500k visible triangles and 2K textures, using 4K only for the hero pieces that need it.
- Make repeated things cheap: one seat, bulb, moulding and baluster model reused many times, with shared materials and trim sheets.
- Spend the detail near the stage.
- The look comes first. If matching the images needs more than this budget, keep the look and state what it costs in your report, so the owner can decide. Don't cut detail silently.

**Tools.**
- Use Blender Python for the architecture and repeated pieces, so a change of size or layout means re-running a script.
- Use AI 3D tools for organic props such as flowers, then clean them up in Blender. Check that each tool's licence allows commercial use.
- Log every AI-made asset in `art-source/ASSET_LOG.csv`.

## Check your own work
Don't hand off after the first build. Repeat this until the differences are minor:
1. Recreate the camera angle of each of the 3 concept images in Blender.
2. Render each angle with EEVEE, which is the closest to what a real-time game can show. Cycles will flatter the result. Add a Cycles render only as an extra.
3. Put each render side by side with its concept image, and save it in `art-source/renders/trivia/round-01/`.
4. List every visible difference in shapes, proportions, detail, materials, colour and lighting, worst first.
5. Fix them, starting with what most affects the first impression.

Before handing off, also confirm that everything imports at the right scale and that a 1.8 m figure fits the doors, steps and podiums.

## When you finish
Add a `HANDOFF.md` entry and give the owner:
- The final side-by-side comparisons.
- What matches the images, and what still falls short and why.
- The triangle count and texture sizes.
- What you would improve next.

Be candid, so the owner's feedback goes where it counts. Hold large model and texture files out of Git until he approves a version; commit scripts, the log and the lighting sheet.

**Later rounds:** change what the owner asks for, and keep names, pivots and markers stable so each import keeps working.
