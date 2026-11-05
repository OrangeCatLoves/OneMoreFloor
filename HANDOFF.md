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
| — | — | none yet | — |

## Log (newest first)

### 2026-11-05 — Claude Code — before build step 1
- Did: installed and signed in the Unity CLI. Updated the docs for the owner's decision: each player chooses first-person or third-person view and can change it with the mouse scroll wheel or other custom controls. Corrected FishNet's licence in the docs (its own licence, not MIT). Not committed yet: the Git repo gets created in step 1.
- Files: `AGENTS.md`, `HANDOFF.md`, `docs/PROJECT_CONTEXT.md`, `docs/floors/_TEMPLATE.md`.
- Other agent needs to know: rooms stay sized for the third-person camera. Characters keep the full animation set, because teammates and third-person view show them. `docs/sizes.json` is listed in the setup entry below but isn't in the project; the owner has been asked to add it. From now on, update the relevant Markdown files whenever something notable changes (see `AGENTS.md`).
- Owner to check: nothing to test yet. Answer `docs/PROJECT_CONTEXT.md` section 13, questions 8–11 (8–10 are needed before step 3).
- Next: Claude Code does build step 1 once the owner OKs the plan.

### 2026-11-04 — Owner — setup
- Did: created the Unity 6.3 LTS project (Universal 3D template, local project, Unity CLI on). Added `AGENTS.md`, `CLAUDE.md`, `HANDOFF.md`, and in `docs/`: `PROJECT_CONTEXT.md`, `sizes.json`, `floors/_TEMPLATE.md`.
- Next: Claude Code does build step 1. Codex starts step 2e after that.
