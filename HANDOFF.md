# HANDOFF.md — notes between agents

Claude Code and Codex never run at the same time, and neither remembers the other's sessions. This file is how they pass information. Both agents may edit it.

## Rules
- **Start of a session:** read "Open requests" and the latest log entry before doing anything else.
- **End of a session:** add a log entry at the top of "Log", then commit.
- **Need something from the other agent?** Add a row to "Open requests". When you finish a request, delete its row and mention it in your log entry.
- Design questions go to the owner, not the other agent.
- Keep entries short; details belong in commits and docs. When the log passes 20 entries, move the oldest ones to `HANDOFF_ARCHIVE.md`.

## Entry format
```
### YYYY-MM-DD — Claude Code | Codex — build step N
- Did: what you finished.
- Files: what changed (folders are enough).
- Other agent needs to know: names, sizes used, anything it must follow.
- Owner to check: what to test, and how.
- Next: what should happen next.
```

## Open requests
| From | To | Request | Added |
|---|---|---|---|
| Claude Code | Codex | Continue Trivia's visual quality gate from iteration 21 and `art-source/renders/trivia/round-03/review.md`. The owner deferred playtesting and requested further cloth, ornament, material, lighting and flower refinement; this pass is complete, but reference differences are not all minor. Keep room proportions/interaction positions frozen pending his later feedback, and large binaries local until approval. Concept naming/logging and folder requests are completed. | 2026-11-05 |
| Codex | Claude Code | Owner playtest is deferred; when he is ready, integrate the approved theatre into the game-owned floor. Local review: `OneMoreFloor > Art > Open Trivia Review`; import notes: `art-source/trivia/README.md`. Latest WIP has 1,379,764 imported triangles, 25 materials/106 maps and 43 lights. Preserve the two custom-mode baked cubemap assignments and indoor fallback: direct `bakedTexture` assignments were lost on scene reload. Production indirect lighting, material parity, AA, distance LODs, two-player checks and laptop profiling remain. | 2026-11-06 |

## Log (newest first)

### 2026-11-08 — Codex — Trivia art loop, refinement WIP
- Did: continued the authorized refinement after pulling latest. Added heavier gathered curtains/tassels, cloth swags, carved leaves/rosettes, damask and varied surface roughness, padded seat backs, fabric bunting, detailed lamps, fluted urns and rebuilt bouquets. Reviewed iterations 17–21, rejected angular petals/stems and intersecting urn surfaces, and rerendered all three fixed reference views at 2x resolution. Room proportions remain unchanged as requested.
- Files: `art-source/`, `tools/blender/`, `Assets/_OneMoreFloor/Art/`, `HANDOFF.md`. Current evidence: `art-source/renders/trivia/round-01/comparison-21_view-*.jpg` and `round-03/` before/after, close-up, Blender/Unity and walking captures. Scripts, evidence, provenance and reports are committed; model/maps/materials/review scene/probes and their metas remain local pending approval. Small evidence uses the existing scoped ordinary-Git exception because LFS quota remains exhausted.
- Other agent needs to know: fixed a real review-lighting defect: independently baked reflection maps vanished on scene reload. They now serialize in Custom probe mode with an indoor fallback; both reloaded-scene validation and the walking test check this. All 179 source interaction/collision checks and all 77 imported marker positions/axes are unchanged. 1,379,764 Unity triangles (+46% from iteration 16), 520 render meshes, 25 materials, 106 maps (17 sets at 2K, 8 at 64px); 79 colliders/16 buttons/5 screens/43 lights. Blender/import audits pass; 82 EditMode + 22 PlayMode tests pass, none skipped. Shared graphics/settings assets are unchanged.
- Owner to check: no playtest requested now; he explicitly deferred it. Review the saved comparisons when ready. Remaining gaps are documented candidly: repeated sculpted detail, botanical realism, cloth/URP material response, production indirect light, and unproven laptop performance. The original “only minor differences” gate is not claimed passed.
- Next: continue from the owner's visual feedback; reserve room-proportion changes for his later playtest. Claude Code has the local FBX, maps/materials, stable reactive parts/markers/colliders, lighting sheet and corrected review scene available for integration when appropriate. Do not treat this WIP as art acceptance or a later build-step approval.

### 2026-11-06 — Codex — Trivia art loop, polish pass WIP
- Did: pulled latest; inspected native renders for zigzags and geometry defects. Added 4x MSAA + High SMAA to art review rendering, mipmaps/trilinear/aniso filtering, smoother petals/leaves, corrected balcony seat intersections and closed the top-step gap. Rerendered all three reference angles at 2x resolution/192 samples; iteration 16 comparisons and controlled AA-off/on crops are saved.
- Files: `art-source/`, `tools/blender/`, `Assets/_OneMoreFloor/Art/`, `HANDOFF.md`. Added an art-owned review scene builder, temporary quality override and walk-through test; existing gameplay/controller files and shared graphics assets are unchanged. Generated scene/probes, blend/FBX/maps/materials and their metas remain local pending approval.
- Repository note: GitHub rejected the first push because its LFS budget is exhausted. This iteration's five small comparison images use a scoped ordinary-Git exception in `art-source/renders/trivia/.gitattributes`; large art remains excluded. Existing LFS history is unchanged and may remain unavailable to fresh clones until the account's quota is restored.
- Other agent needs to know: the review scene copies the existing MovementTest local-host player/camera and adds the theatre, 79 colliders, open door display pose, 43 lights, four shadow sources and two baked reflection probes. It has no Trivia rules or baked indirect light. All 77 imported marker positions/axes are unchanged. 945,460 imported triangles (+98,799, mainly petal curves), 102 maps, 24 materials; performance target remains unproven. Blender/import audits pass; 82 EditMode + 22 PlayMode tests pass. New test walks out of the lift, climbs the stage and captures both views with AA enabled.
- Owner to check: Unity menu **OneMoreFloor > Art > Open Trivia Review**, then **Play** and click Game view. WASD/mouse, Space jump, V first/third person, scroll camera distance, Escape cursor. Assess close-up edges/moving shimmer, stage navigation, camera comfort and mood. Evidence and remaining gaps: `art-source/renders/trivia/round-02/review.md`.
- Next: owner playtest and feedback. Richer cloth/flowers/ornament, closer reverse-hall proportions, production lighting and distance LODs still need work; the original reference gate is not claimed passed. Keep interaction pivots and marker names stable.

### 2026-11-06 — Codex — Trivia art loop, round 1 WIP
- Did: pulled latest; built the theatre in Blender after validating one podium in Unity. Rendered all three reference views through 14 passes; saved labelled comparisons and a candid difference journal. Renamed/logged the 3 concepts and preserved the owner's exact ChatGPT prompt. Created the requested art folders, asset log, export scripts and lighting sheet. Visual quality gate remains open; this is not an approved final-art handoff.
- Files: `art-source/`, `tools/blender/`, `Assets/_OneMoreFloor/Art/`, `HANDOFF.md`. Large blend/FBX/maps/materials and their metas remain local as the brief requires; scripts regenerate them after a fresh clone.
- Other agent needs to know: ready locally: `SM_Trivia_Theatre.fbx`, 102 texture maps, 24 URP materials, 79 collision meshes, 16 buttons, 5 live screens, 43 light markers, 3 marquee chase sets and 12 pan/tilt hierarchies. Import notes: `art-source/trivia/README.md`. 846,661 Unity triangles (901,052 source); roughly 500k visible target not yet demonstrated. Geometry/import audits passed, a measured 1.8 m KyleRobot fits, and all existing tests passed (82 EditMode, 21 PlayMode). Flowers are a documented procedural substitute because no suitable connected AI 3D provider was available.
- Owner to check: `art-source/renders/trivia/round-01/comparison-14_view-1.jpg` through `view-3.jpg`. Ornament, cloth/flower variation, reverse-view proportions and material/lighting polish still fall short of the references. No art acceptance or playable-camera comfort is claimed.
- Next: continue art refinement from the journal; use the importable WIP for an integration review after the owner reviews it. Claude Code owns the walkable scene and engine lighting. Preserve object names, pivots and markers, and commit the large assets only after the owner approves a version.

### 2026-11-05 — Claude Code — art loop
- Did: raised run speed to 5.8 after the owner's playtest. Switched Trivia to the owner's art loop: Codex builds real 3D assets from the concept images, Claude Code imports them, the owner playtests, and it repeats until he is satisfied.
- Files: `Assets/_OneMoreFloor/Data/`, `HANDOFF.md`, `docs/`.
- Other agent needs to know: see your updated request above. Don't build the grey kit from 2e for Trivia. Add a log entry listing what's ready so Claude Code can import it.
- Owner to check: run Codex with the message from chat.
- Next: Codex builds the first round. Claude Code imports it into a scene the owner can walk around, then does steps 4 and 5 between rounds.

### 2026-11-05 — Claude Code — build step 3
- Did: built movement, the camera with scroll zoom and the `V` key, and 2-player networking, using the package's robot. Recorded Trivia's team voting rules in `docs/floors/trivia.md`. All 103 tests pass (82 EditMode, 21 PlayMode). In a real 2-player run, each player saw the other, and the host saw Player 2's movement and run animation.
- Files: `Assets/_OneMoreFloor/Game/`, `Editor/`, `Tests/`, `Data/`, `Assets/Settings/PlayMode/`, `ProjectSettings/`, `AGENTS.md`, `HANDOFF.md`, `docs/`.
- Other agent needs to know: Unity's character package is in `Assets/Starter Assets/`, local only and not in Git. The player layer is `Player`. The test arena reads `docs/sizes.json`, so re-run its builder after those numbers change.
- Owner to check: press Play and try the controls for feel. Answer the open questions in `docs/floors/trivia.md`.
- Next: the owner tries step 3, then Claude Code does step 4 (game loop). Codex can do 2e while Claude Code isn't working.

### 2026-11-05 — Claude Code — before build step 3
- Did: the owner wrote the Trivia spec and added 3 concept images. Renamed the spec to `docs/floors/trivia.md`, filled in its id and drafted its asset list from the images. Playtests now use a real character instead of a capsule: the owner is downloading Unity's character package.
- Files: `HANDOFF.md`, `docs/`.
- Other agent needs to know: the concept images are in `art-source/concept/floors/trivia/` and are not committed yet: rename them, log them and commit them (see Open requests). They show one design, a game-show stage. Final art for it waits for the greybox playtests and the style choice.
- Owner to check: answer the open points on the Trivia spec. Download the package.
- Next: Claude Code imports the package, checks its licence, then does steps 3 and 4.

### 2026-11-05 — Claude Code — after build step 2c
- Did: the owner confirmed 2c. Removed the controller and Steam Deck requirements: the game is keyboard and mouse only for now. Made Trivia the vertical slice, so it is finished to production quality before the next floor's design starts. Dropped the Asset Store package: I write movement and camera myself, with a capsule standing in until the owner's character arrives.
- Files: `AGENTS.md`, `HANDOFF.md`, `docs/`.
- Other agent needs to know: art budgets now target a mid-range laptop instead of a Steam Deck, and concept images are checked at 1280×720. Your final art work (step 8) is Trivia's room, the lobby and the elevator, not Collapse Escape.
- Owner to check: write `docs/floors/trivia.md`. Put concept images in `art-source/concept/floors/trivia/`.
- Next: Codex does 2e. Claude Code does step 3.

### 2026-11-05 — Claude Code — build step 2c
- Did: built the core rules in `Core`: hearts pool, run phases, seeded random generator, floor picker, difficulty levels and visual tiers. Added the `GameRules` and `DirectorSettings` data assets. All 64 EditMode tests pass from the command line. P0 now focuses on Trivia, seen through to completion before Collapse Escape starts.
- Files: `Assets/_OneMoreFloor/Core/`, `Game/`, `Data/`, `Tests/EditMode/`, `art-source/concept/`, `AGENTS.md`, `HANDOFF.md`, `docs/`.
- Other agent needs to know: nothing changes for 2e. Put concept images in `art-source/concept/floors/<floor-id>/` or `style-tests/<style>/`.
- Owner to check: nothing to test; there is nothing to play yet. Write `docs/floors/trivia.md`. Do step 2d. Decide section 13, questions 8, 11 and 12.
- Next: Codex does 2e. Step 3 needs the character package from 2d and an answer to question 11.

### 2026-11-05 — Claude Code — before build step 2
- Did: recorded the owner's camera decisions and the new approach to sizes. Wrote `docs/sizes.json` with starting guesses for what every floor shares. Each floor now gets its own size from its spec and from playtests.
- Files: `AGENTS.md`, `HANDOFF.md`, `docs/`.
- Other agent needs to know: in 2e, build grey pieces only. Final rooms wait for the greybox playtests and the style choice (section 9, steps 5, 6 and 8). Concept images in `art-source/concept/` are reference only.
- Owner to check: write `docs/floors/trivia.md` from the template. Do step 2d. Answer section 13, question 8 before step 3.
- Next: Codex does 2e. Claude Code does 2c once the owner OKs the plan.

### 2026-11-05 — Claude Code — build step 1
- Did: installed and signed in the Unity CLI. Set up Git and LFS. Added Cinemachine 3.1.7, ProBuilder 6.1.4, Localization 1.5.13, Multiplayer Play Mode 2.0.2, Newtonsoft JSON 3.2.2 and FishNet 4.7.4. Added the folder layout, the code modules and one EditMode test, which passes from the command line. Updated the docs for the owner's decision: each player chooses first-person or third-person view and can change it with the mouse scroll wheel or other custom controls. Corrected FishNet's licence in the docs (its own licence, not MIT).
- Files: `.gitignore`, `.gitattributes`, `Packages/`, `ProjectSettings/`, `Assets/FishNet/`, `Assets/_OneMoreFloor/`, `AGENTS.md`, `HANDOFF.md`, `docs/`.
- Other agent needs to know: rooms stay sized for the third-person camera. Characters keep the full animation set, because teammates and third-person view show them. `docs/sizes.json` is listed in the setup entry below but isn't in the project; the owner has been asked to add it. Update the relevant Markdown files whenever something notable changes. The commit history lists only `OrangeCatLoves` (see `AGENTS.md` for both rules).
- Owner to check: nothing for step 1; the owner confirmed it. The repo stays public, and the work is pushed. Agents now run every check they can and ask the owner only for what needs a person (see `AGENTS.md`).
- Next: the owner decides how `docs/sizes.json` should work, which Codex needs for 2e. Claude Code does 2c after showing its plan.

### 2026-11-04 — Owner — setup
- Did: created the Unity 6.3 LTS project (Universal 3D template, local project, Unity CLI on). Added `AGENTS.md`, `CLAUDE.md`, `HANDOFF.md`, and in `docs/`: `PROJECT_CONTEXT.md`, `sizes.json`, `floors/_TEMPLATE.md`.
- Next: Claude Code does build step 1. Codex starts step 2e after that.
