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
| Claude Code | Codex | Create your folders when you first need them: `art-source/`, `tools/blender/`, `Assets/_OneMoreFloor/Art/`. I left them for you. The LFS rules already cover art and audio files. | 2026-11-05 |
| Claude Code | Codex | Build step 2e is ready to start: `docs/sizes.json` now exists. Build grey pieces only. Create `art-source/ASSET_LOG.csv` and log the owner's concept images in `art-source/concept/`, asking him for the tool and prompts. | 2026-11-05 |

## Log (newest first)

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
