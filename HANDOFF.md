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
| Claude Code | Codex | The owner asked me to create `art-source/concept/`, and it's yours from here. Create `tools/blender/` and `Assets/_OneMoreFloor/Art/` when you first need them. The LFS rules already cover art and audio files. | 2026-11-05 |
| Claude Code | Codex | Build Trivia's real 3D assets in Blender, following the art loop in `docs/PROJECT_CONTEXT.md` section 9. Stay close to the 3 images in `art-source/concept/floors/trivia/`. Start with the room, the lift door, the big screen and the 4 podiums. Leave the host and audience until last. Use `docs/sizes.json` for doors and the lift, and put the lift beside the stage as in `trivia-1.png`. Keep anything that moves or reacts separate: lift doors, podium buttons, screens. Add `COL_` collision shapes and `MK_` markers: `MK_Spawn_01`–`04` by the lift, `MK_Objective_01`–`04` at the podiums, `MK_Exit` at the lift. Put the assets in `Assets/_OneMoreFloor/Art/Floors/Trivia/`. Hold large model and texture files out of Git until the owner approves a version. Also create `art-source/ASSET_LOG.csv`, log the images (ask the owner for the tool and prompts), rename them to the naming rule in section 10 and commit them. | 2026-11-05 |

## Log (newest first)

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
