# AGENTS.md — OneMoreFloor

Always-on rules for AI agents (Claude Code and Codex). The full project context is in `docs/PROJECT_CONTEXT.md`: read it before starting any build step. These rules repeat its section 0; change both together.

## The project
OneMoreFloor is a 3D co-op challenge game (1–4 players) for Windows PC (desktops and laptops, keyboard and mouse), sold on Steam. Each player chooses first-person or third-person view. Unity 6.3 LTS, URP, C#, FishNet. Players ride an elevator; every floor is a surprise challenge; the team shares one pool of hearts; score = floors cleared. The current phase and step are under "Current focus" at the top of `docs/PROJECT_CONTEXT.md`.

## The owner
- Strong software engineer, new to game development and Unity.
- Explain game-dev ideas in plain words. Define any jargon in one line.
- For anything he must do in the Unity Editor, give exact steps: menu paths, Inspector fields, values.
- Keep replies short and direct.
- Don't ask him to do setup you can do yourself, such as creating folders. Do it and tell him the exact path.
- He has handed technical decisions to the agents. Make them and report them. Design stays his.

## Handoff between agents
- Only one agent runs at a time, and neither remembers the other's sessions. `HANDOFF.md` (project root) is how you pass information.
- **Start of every session:** read `HANDOFF.md` (its open requests and latest entry) before anything else.
- **End of every session:** add your entry to `HANDOFF.md` (the format is inside the file), then commit.
- Need something from the other agent? Add it to "Open requests" in `HANDOFF.md`.

## How we work
- Follow the build order (`docs/PROJECT_CONTEXT.md` section 9). One step at a time. Don't start work from a later phase until the current gate passes.
- Before changing anything, show a short plan and wait for the owner's OK.
- Run every check you can yourself (compile, tests, Console, packages, Play mode, screenshots) so the owner doesn't have to repeat them. Ask him to test only what needs a person: how it feels, looks and plays, or hardware you can't reach.
- After each change, say what changed, what you checked and the results.
- When something notable changes (a decision, a tech choice, the project's setup or build status), update the relevant Markdown files in the same session so they stay accurate. Record the owner's decisions; don't make them for him.
- A build step is done only when the owner confirms it, based on your report and any checks only a person can do.
- End every session with a commit. If the owner hasn't confirmed the work yet, start the commit message with `WIP:`. Small commits, clear messages. The commit history lists only `OrangeCatLoves`: no co-author or AI attribution lines.
- Pull before starting. Push after committing.

## Who owns which folders
- **Codex (art):** `art-source/`, `tools/blender/`, `Assets/_OneMoreFloor/Art/`.
- **Claude Code (code):** everything else.
- **Shared:** `HANDOFF.md`. Both agents write to it.
- `docs/` belongs to the owner. Read it; edit it only when the owner asks or to record a notable change (see "How we work").
- Never edit the other agent's folders. If you need a change there, add a request to `HANDOFF.md`.

## Design rules
- The owner designs the story, the setting and every floor's rules. Never invent them. If something is missing or unclear, use a neutral placeholder and ask.
- Unclear design rule? Check `docs/PROJECT_CONTEXT.md` section 13 (open questions) and ask. Don't guess.

## Code rules
- Game rules (Core) are plain C# with no `UnityEngine` references, and have unit tests.
- The host decides outcomes. Clients only send requests.
- Solo = a local host with one player. Never write solo-only gameplay code.
- All randomness comes from the run's seeded random generator. Never `UnityEngine.Random` in game rules.
- Tunable numbers live in data assets (ScriptableObjects), not in code.
- A floor never references another floor. Floors talk to the game only through the Floor contract (section 4).
- The game is played with keyboard and mouse. Read input through Input System actions, not raw keys, so players can rebind controls and a controller could be added later.
- All on-screen text comes from the string tables. Never hard-code text.
- Ask before adding packages, assets, paid services or new top-level folders. Anything the current build step lists is already approved.
- Never edit vendored code (`Assets/FishNet/`, `Assets/Starter Assets/`, third-party packages).

## Art rules (summary; full list in section 7)
- 1 Blender unit = 1 m = 1 Unity unit. Player ≈ 1.8 m tall. Transforms applied. Models face +Z in Unity. FBX by default.
- Names: `SM_` static mesh, `SK_` rigged mesh, `M_` material, `T_` texture, `A_` animation, `COL_` collision mesh, `MK_` marker.
- Anything that moves or reacts is its own object. Never deliver a whole floor as one welded mesh.
- Get one asset fully working in Unity before making many.
- New files in `Assets/` need their Unity `.meta` files committed with them. Before committing new assets, make Unity import them so it creates those files: running `unity test` does this, or ask the owner to click into Unity once.
- Log every AI-made asset in `art-source/ASSET_LOG.csv`: file, tool, prompt or source, licence, date.
- Concept images go in `art-source/concept/` (`style-tests/<style>/` or `floors/<floor-id>/`), never in `Assets/`. They are reference only.

## Sizes
- Doors, corridors, stairs, ceilings and the elevator get their sizes from `docs/sizes.json` (metres). Don't hard-code these in scripts or models; read them from the file.
- These spaces are bigger than real ones: in third-person view the camera sits a few metres behind the player and needs room.
- The numbers are starting guesses until build step 5. When they change, re-run the generator scripts.
- Each floor has its own overall size: its spec describes it in plain words, Claude Code turns that into metres, and the owner tunes it by playing.

## Testing
- Core rules: EditMode unit tests. Every floor: a PlayMode test that clears it, and one that drains hearts to game over.
- From build step 4 on, test every floor with 2 players in the editor (Multiplayer Play Mode).
- Test every floor in both first-person and third-person view, unless its spec limits the view.
- PlayMode tests: `unity test . --mode PlayMode`. For 2 players, see `docs/PROJECT_CONTEXT.md` section 11.
- Use the Unity CLI to compile and run tests yourself before asking the owner to check in the Editor. With Unity closed: `unity test . --mode EditMode`. Exit code 0 = passed, 8 = a test failed, 6 = the run never finished (the reason is in `%LOCALAPPDATA%\Unity\Editor\Editor.log`).
- With the project open (`unity open .`, then wait for `unity status` to say `ready`), use `unity command`: `console status`, `console get --level error`, `package list`, `test run --mode editor`, `editor.playmode start` and `stop`, `capture game_view --output-path <file>`. Close Unity afterwards with `unity command editor quit`. Don't use `editor save_all` while the open scene has never been saved: it opens a Save dialog that blocks Unity until someone closes it.
