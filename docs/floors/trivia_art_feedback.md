# Owner feedback — Trivia art

For Codex. The owner's playtest feedback on the Trivia room, newest round first. Where this file and `trivia_art_brief.md` differ, this file wins.

## Round 2 — 11 November 2026

The owner walked iteration 22 on his home PC. **The room's overall size is fine: keep the hall's outer dimensions.** The design inside it needs the changes below.

### The owner's instruction, in his words
> Act as an expert interior designer. Redesign the 3D room using strict spatial logic, functional human ergonomics, and a cohesive material palette. Every object placement, scale, and lighting choice must be highly intentional, realistic, and serve a clear purpose—strictly avoid nonsensical layouts, blocked walkways, or random clutter. While it is true that it is following a design I have given, it still has to strictly follow great design principles!

Apply this to everything in the room, not only the items listed. The concept images set the look. They don't excuse an object that has no purpose, floats, or blocks a route.

### Defects to fix
Positions are in Unity metres, measured in the review scene.

1. **Curtain tie-back ropes float.** The owner: "not even connected to the curtains, just standing by itself". `SM_Trivia_GoldTieback` is a single band 19.8 m wide at a height of 3 m.
   - Each tie-back wraps fully around its gathered curtain and ties off to a hook or boss fixed to the wall or arch.
   - The curtain visibly pinches where it is tied, and the tassel hangs from the knot.
   - Nothing floats and nothing passes through the cloth.
2. **The floor flashes in front of the lift (z-fighting).** `SM_Trivia_HallFloor` and `SM_Trivia_LiftFloor` both have their top face at y = 0.000 and overlap from x = 13.0 to 14.5, z = −0.5 to 4.5.
   - Leave one surface there: end one floor at the door line and add a threshold strip.
   - Add a check for overlapping coplanar faces to the audit, and run it on the whole room.
3. **The 4 lamps on the stage floor point at the floor.** The heads of `SM_Trivia_Spotlight_09` to `12` face straight down, while their lights (`MK_Light_Spot_14` to `17`) shine upward. The owner: "this makes no sense at all".
   - Keep the 4 lamps, as in the concept images.
   - Tilt each head up at a specific target, such as the backdrop curtain or the players, and record the target in the lighting sheet.
   - Each lamp's lens must point the same way as the light it casts. Apply this check to every fixture in the room.

### Layout changes
4. **Turn all 4 podiums 180° so the players face the audience.**
   - The laurel front faces the audience (+Z). The buttons and the podium monitor face the player, who now stands on the big-screen side.
   - Move `MK_Objective_01` to `04` to the new standing spots, facing +Z.
   - Keep clear walking room behind the players: a third-person camera sits about 4 m behind them.
   - The host must not end up behind the players' backs. Place `MK_Host` where the players and the audience can both see the host, and say where and why.
   - Update the button press direction in your import notes.
5. **Add one TV for the players.** The players now have their backs to the big screen.
   - One screen, "not too big", centred on the stage's front edge (x = 0, near z = 5.6), facing the players and tilted up at them.
   - Keep it low, so the audience still sees the players over it.
   - It shows the same content as the big screen. Make its live surface `SM_Trivia_PlayerScreen`: a separate flat mesh with clean 0–1 UVs, no text, and **the same aspect ratio as the big screen's live surface**, so one image fits both.
   - Design it as a believable piece of stage equipment, with a stand or housing and a cable route.
6. **Move the stage steps to the lift side.** The TV takes the centre of the stage front, where `SM_Trivia_StageStep` is now (x = ±1.5, z = 5.6 to 6.5).
   - Remove the centre steps. Build the steps on the side of the stage nearest the lift (+X), on the direct route from the lift doors (`MK_Exit` at x = 12.4, z = 2).
   - Sizes come from `docs/sizes.json`.
   - Keep a clear landing in front of the lift doors for 4 players. The stage edge at x = 10.7 is only 2.3 m from the lift doors, so wrap the steps around the stage corner if a straight flight leaves too little room.
   - Resolve what the red carpet aisle now leads to, so it doesn't end at nothing.
7. **Big screen: remove the side panels.** The owner: "remove the gold design on the left and right, just let the white screen take up the entire frame".
   - Remove the purple side panels, their gold laurels and any carving that sat on them.
   - `SM_Trivia_QuestionScreen` fills the whole inside of the bulb frame. Keep the outer frame and the marquee bulbs.
   - Report the new aspect ratio.

### Fill the empty space
8. **The back and sides of the audience area.** The owner: "a lot of blank space and it's really awkward and ugly".
   - Fill it with things a real televised theatre has there, each with a purpose. Ideas, not orders: more seating blocks with proper aisles, a rear cross-aisle, exit doors with lit signs, a sound and lighting control desk at the rear centre, broadcast camera positions, wall pilasters and sconces at a steady rhythm.
   - No filler props. Every walkway stays at least 1.2 m clear.
9. **Audit the rest of the room.** The owner: "analyse other parts of the room and check for any space that makes no sense, redundant, ugly and fix them".
   - Walk every route a player or an audience member would take.
   - List each problem you find with its fix, and show before and after.

### Limits for this round
- **Performance:** the room is at 1,549,521 triangles against a target of about 500,000 visible. This round must not raise the total. Pay for new pieces by making the flowers cheaper (434,223 triangles for 4 urns) and by reusing the far-seat mesh.
- **Names and markers:** keep existing object names, pivots and markers stable, apart from the changes above. List everything renamed, moved, added or removed.
- **Not in this round:** the host and the audience.
- **Evidence:** judge the result in Unity, not only in Blender. For each numbered item, save a before and after capture from the review scene in `art-source/renders/trivia/round-05/`.
- **Stop after this list and the audit.** Don't start further refinement passes: the next step is the owner's playtest.
- **Hand-off:** rebuild and rebake the review scene, add a `HANDOFF.md` entry, and tell the owner candidly what is done and what still falls short. Claude Code then sends the room to the owner's home PC.
