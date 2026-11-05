# OneMoreFloor — Project Context

Version 0.10 · 5 Nov 2026 · Owner: Wei Bin (GitHub: OrangeCatLoves)

The single source of truth for what the game is and how it is built. Written for the owner and for AI coding agents (Claude Code, Codex).

- **Status:** pre-production. Build step 1 (project setup) is done. No game rules or gameplay yet.
- **Current focus:** Phase P0, build step 2 (section 9). No backend work yet.
- **Conflicts:** if this file disagrees with the code or with the owner's latest instruction, stop and ask.

---

## 0. Rules for AI agents

These rules are repeated in `AGENTS.md`, which Claude Code and Codex load automatically. Change both together.

- The owner is a strong software engineer (Java, Go, Python, TypeScript, SQL, Docker) but new to game development and Unity.
  - Explain game-dev ideas in plain words. Define any jargon in one line.
  - For anything he must do in the Unity Editor, give exact steps: menu paths, Inspector fields, values.
- Keep replies short and direct.
- **The owner designs the story, the setting and every floor's rules.** Never invent them. If something is missing or unclear, use a neutral placeholder and ask.
- Ask before adding packages, assets, paid services or new top-level folders.
- Never edit vendored code (`Assets/FishNet/`, third-party packages).
- `docs/` belongs to the owner. Read it; edit it only when the owner asks or to record a notable change (see the rule on notable changes below).
- Every room, door, corridor, stair and elevator size comes from `docs/sizes.json` (metres). Don't hard-code these in scripts or models.
- Game rules are plain C# with no `UnityEngine` references, and have unit tests.
- The host decides outcomes. Clients only send requests.
- All randomness comes from the run's seeded random generator. Never `UnityEngine.Random` in game rules.
- Tunable numbers live in data assets (ScriptableObjects), not in code.
- A floor never references another floor. Floors talk to the game only through the Floor contract (section 4).
- Everything must work with a controller and on Steam Deck (section 5).
- All on-screen text comes from the string tables. Never hard-code text (section 5).
- Follow the build order (section 9). Don't start work from a later phase until the current gate passes.
- Only one agent runs at a time. Start every session by reading `HANDOFF.md`; end it by adding an entry there and committing (start the commit message with `WIP:` if the owner hasn't confirmed the work yet). The commit history lists only `OrangeCatLoves`: no co-author or AI attribution lines.
- Work in small steps. After each change: what changed, what you checked and the results. Run every check you can yourself; ask the owner to test only what needs a person (how it feels, looks and plays, or hardware you can't reach).
- When something notable changes (a decision, a tech choice, the project's setup or build status), update the relevant Markdown files in the same session so they stay accurate. Record the owner's decisions; don't make them for him.
- Unclear design rule? Check section 13 and ask. Don't guess.

---

## 1. The game

OneMoreFloor is a 3D challenge game on Steam for PC and Steam Deck, played solo or in co-op, in first-person or third-person view (each player's choice). Players ride an elevator up a building. Every floor is a surprise challenge from a different genre: trivia, escaping a collapsing building, shooting monsters, a spelling bee, parkour, finding keys while a monster hunts you. The team stays on a floor until it clears it, and every mistake costs hearts from one shared pool. When the pool hits zero, the run ends on the spot. Score = floors cleared. Two leaderboards — solo and multiplayer — show the highest floors.

The higher the team climbs, the harder the floors get and the more epic they look: ordinary rooms at the bottom, other worlds further up.

- **Tone:** tense but fun. Thrilling and sometimes scary, but light enough to laugh at a fail. Like a deadly game show.
- **Story and setting:** designed by the owner in `docs/STORY.md`. Until it exists, use a neutral building.
- **Business model:** paid game, one price on Steam. No in-game shop.
- **Between runs:** cosmetic unlocks only (section 2).
- **Language:** English only at launch.
- **First release (MVP):** solo + co-op, 12–15 floors. PvP comes in a later release.

**Design pillars** — judge every feature against these:
1. **Surprise** — players never know what the next floor is.
2. **Instant clarity** — any floor is understood in ~5 seconds (name + one-line goal). Surprise must never mean confusion.
3. **Thrill** — time pressure, shared stakes, close calls.
4. **Climb higher** — short floors, a fast loop, a score worth chasing, and harder, more epic floors the higher you go.

---

## 2. Core rules

*(tunable)* = lives in data. *(default — confirm)* = needs the owner's sign-off (section 13).

### Modes
| Mode | Players | Status |
|---|---|---|
| Solo | 1 | MVP — build first |
| Co-op | 2–4 | MVP — build after solo |
| PvP (solo and team) | TBD | Later release |

### Lobby and joining
- Lobby *(default — confirm)*: players gather, pick a character and colour, see both leaderboards, ready up, and step into the elevator.
- Players join in the lobby only, never mid-run *(default — confirm)*.

### Camera view
- Each player chooses first-person or third-person view.
- Players can change it with the mouse scroll wheel or other custom controls. Exact behaviour and the controller default are still open (section 13, questions 8–9).
- The view is personal: it changes only that player's camera. Teammates still see their full character.
- A floor may switch the view when it needs to (e.g. a trivia screen). A floor's spec can also limit it.

### Hearts
- One shared pool per run.
- Starting hearts = `heartsPerPlayer × players` *(tunable; default 10)* → 1p: 10, 2p: 20, 3p: 30, 4p: 40. Hearts grow in proportion to team size.
- Why 10: every mistake costs hearts, so the pool must cover many mistakes. At about 1 mistake per player per floor, 10 hearts per player last about 10 floors; at ~90 s per floor that is 10 × 90 s = 15 min of play. Tune in playtests.
- Each floor's spec says what costs hearts and how many. One mistake can cost more than 1 heart.
- Hearts can be lost at any moment during a floor, not only at the end.
- Hearts reach 0 → the run ends immediately, usually mid-floor → results screen → score submitted.
- Future: some floors may give hearts back (the owner will design these). All heart changes go through the run manager, so adding this later is a small change.

### Floor loop
1. Elevator ride (~5–10 s, *tunable*). The next floor is chosen and loaded behind closed doors.
2. Doors open → title card: floor name + one-line goal (~5 s) → countdown.
3. Play. Mistakes cost hearts, and the team keeps trying. A floor can't be failed or skipped: the team keeps going until it clears the floor or runs out of hearts.
4. Floor cleared → everyone back in the elevator → floor count +1 → next ride.

### Floor rules (set by the owner, per floor)
- The owner writes every floor's rules in its spec (`docs/floors/<id>.md`, from `docs/floors/_TEMPLATE.md`): what clears it, what costs hearts and how many, how players respawn or the floor resets, and how it gets harder.
- No global default — floors differ. Example: Spelling Bee needs every player to pass at least twice.
- Every spec must answer: what stops a team from stalling forever? Recommended: something keeps costing hearts when the team stops making progress (e.g. a timer that costs a heart, then restarts).
- Target length of a clean run through one floor: 30–120 s *(tunable)*.

### Difficulty (floors get harder as you climb)
- Each floor number has a difficulty level. Default *(tunable)*: level = 1 + ⌊(floor − 1) ÷ 5⌋, so the level goes up by 1 every 5 floors.
  - How it works: floor 7 → 7 − 1 = 6 → 6 ÷ 5 = 1.2 → round down to 1 → 1 + 1 = level 2. Floors 1–5 are level 1, floors 6–10 level 2, floors 11–15 level 3.
- Each floor's spec says what "harder" means at each level (e.g. harder questions, faster collapse, more monsters). The owner decides.
- Past the last level in its spec, a floor stays at its hardest. If playtests show runs that never end, the owner adds harder levels.
- Floors can't be skipped, so a sudden jump in difficulty ends runs. Ramp slowly; watch "hearts lost per floor" in playtests.

### Visual tiers (floors get more epic as you climb)
| Tier | Floors *(tunable)* | Look |
|---|---|---|
| 1 | 1–10 | Ordinary rooms in one building (e.g. office, gym, library) |
| 2 | 11–20 | Bigger, stranger, more dramatic rooms |
| 3 | 21+ | The doors open onto other worlds |

- Why these ranges: an average run lasts about 10 floors (hearts maths above), so tier 2 starts around where an average run ends. Only strong teams reach tier 3.
- A floor's mechanics stay the same in every tier; only its room (look and layout) changes. The same Collapse Escape can be an office block in tier 1 and a crumbling sky temple in tier 3.
- Some floors may only exist in higher tiers.
- Tier 1 is what most players see: build it first and best. Higher tiers are the reward for climbing, and the best trailer shots.

### Score and leaderboards
- Score = floors cleared.
- Two boards: **Solo** and **Multiplayer** (teams of 2–4 together).
- A run's board is set by how many players started it *(default — confirm)*: a duo that drops to one player stays on the multiplayer board.
- Tie-break: lower total time *(default — confirm when the leaderboard is designed)*.
- Full leaderboard design comes later. Technical side: section 8.

### Unlocks (cosmetic only)
- Reaching floor milestones unlocks cosmetics: character colours, outfits, extra characters. Which milestone unlocks what is the owner's call.
- Unlocks never change hearts, difficulty or abilities, so the leaderboards stay fair.

### Leaving
- A player quits mid-run: the run continues for the rest; hearts unchanged.
- The host quits: the run ends and the score is saved (MVP). Host migration is a later improvement.

### Floor selection (the "director")
- Random from the catalog, seeded per run.
- Never the same floor or genre twice in a row.
- A floor only appears from its `minFloor` onward.
- Uses the floor's room for the current visual tier. If the floor has no room for this tier, use its highest tier below.
- Later: special floors every N floors, a daily seed shared by everyone.

---

## 3. Floors

**Launch target: 12–15 floors.** An average run is about 10 floors, so each floor shows up less than once per average run: 10 ÷ 12 ≈ 0.8 times, 10 ÷ 15 ≈ 0.7 times. The 6 starter floors below come first; the owner designs the rest.

Build order (section 9): **Trivia → Collapse Escape → Monster Arena** first. Trivia is the simplest test of the Floor contract. Collapse Escape sets the movement sizes the art needs and is the vertical slice. Monster Arena adds shooting and monster AI. Spelling Bee, Parkour Run and Key Hunt come after.

The owner writes each floor's spec before it is built, using `docs/floors/_TEMPLATE.md`. A spec covers: goal, clear rule, what costs hearts and how many, respawn or reset, anti-stall rule, how it gets harder per level, 1-player vs 4-player behaviour, controls (controller and Steam Deck), camera view (if the floor switches or limits it), rooms per visual tier, assets needed.

Rules marked TBD are placeholders until the owner's spec exists.

| # | Floor | Genre | Clear rule | Costs hearts | Main tech |
|---|---|---|---|---|---|
| 1 | Trivia | Knowledge | TBD (e.g. team gets N answers right) | TBD (e.g. wrong answer) | UI, question bank |
| 2 | Collapse Escape | Panic escape | TBD (e.g. every player gets out) | TBD (e.g. hit by debris, still inside at collapse) | Scripted collapse, timer, checkpoints |
| 3 | Monster Arena | Shooting | TBD (e.g. all monsters dead) | TBD (e.g. hit by a monster) | Aiming, weapons, hit checks by the host, monster AI (NavMesh), animation, effects |
| 4 | Spelling Bee | Listening + typing | **Every player passes at least twice (2 correct words each) — owner's rule** | TBD (e.g. misspelling) | Word audio clips, text input, on-screen keyboard |
| 5 | Parkour Run | Movement | TBD (e.g. every player reaches the exit) | TBD (e.g. falling) | Movement tuning, checkpoints, fall zones |
| 6 | Key Hunt | Stealth + puzzle | TBD (e.g. exit unlocked, everyone out) | TBD (e.g. caught by the monster) | Monster AI (reused from Monster Arena), keys, doors |

- Question banks and word lists are data files (CSV/JSON) in English, with difficulty tags. No repeats within a run.
- Spelling audio is pre-generated (one clip per word), not live text-to-speech.
- Spelling Bee on a controller or Steam Deck: typing uses the on-screen keyboard, which is slower than a real keyboard. The spec's timers should allow for this.
- Monster Arena on a controller or Steam Deck: aiming with sticks is slower than with a mouse, and the leaderboards are shared. The spec should decide on aim assist.
- Both camera views: aiming (Monster Arena) and judging jumps (Parkour Run) play differently in first and third person, so their specs and playtests should cover both.

---

## 4. The Floor contract

How a floor plugs in. This keeps floors independent, so an agent can build one floor without touching the others.

Every floor has four parts:
1. **FloorDefinition** (ScriptableObject — a data asset edited in the Inspector), the fields every floor shares: `id` (stable string, e.g. `collapse_escape`), `displayName`, `goalText` (one line), `genre`, `minPlayers`, `maxPlayers`, `minFloor` (optional: first floor it can appear on), `selectionWeight`, `schemaVersion`, `rooms` (one room prefab per visual tier it supports), `settings` (link to the floor's own settings asset).
2. **Floor settings asset** — one type per floor, holding the numbers from its spec, including a row per difficulty level. Example, Spelling Bee: `passesPerPlayer = 2`, plus per-level values (word difficulty, time per word, heart costs) taken from the owner's spec.
3. **Room prefabs** — one per visual tier: geometry plus marker points (spawns, exit, checkpoints, key spots, monster spawns, objective and hazard anchors). Every room of a floor has the markers its controller needs, so the same code runs in every tier.
4. **Floor controller** (runs on the host). Lifecycle: `Setup(seed, players, room, level)` → `Intro` → `Play` → `Cleared` → `Cleanup`. During `Play` it reports each mistake with `LoseHearts(player, amount, reason)` and calls `Cleared()` when done. It also reports floor stats: time taken, hearts lost per player.

Rules:
- Floors never edit hearts or score directly. They call `LoseHearts`; the run manager applies it at once. (Heart rewards later get a matching `GainHearts`.)
- The run can end mid-floor (hearts hit 0). Floors must stop cleanly at any moment.
- Floors use only the random generator they are given.
- Floors work for 1–4 players or declare their limits.
- Floors work in first-person and third-person view unless their spec says otherwise.
- Floors handle a player leaving mid-floor.
- Floor text comes from the string tables.
- Shared helpers (timers, interaction, kill zones, checkpoints, respawn) live in the Floor SDK and never depend on a specific floor.

---

## 5. Tech stack

| Area | Choice | Why |
|---|---|---|
| Engine | Unity 6 LTS (newest LTS; 6.3 LTS at the time of writing) | Code is plain C# text that AI agents edit well; fast multiplayer testing; free below $200k yearly revenue + funding |
| Rendering | URP | Runs well on mid-range PCs and Steam Deck; suits a stylized look |
| Networking | FishNet | Free and royalty-free under its own licence (not MIT; it allows games you sell); the same code runs as host or dedicated server; client-side prediction built in (needed for PvP). Raycast lag compensation needs paid FishNet Pro — decide before PvP |
| Transport | Tugboat (LAN, dev) → Steam via FishySteamworks (online) | Steam's relay and friend invites cost nothing extra on Steam |
| Steam features | Steamworks.NET | Login, lobbies, leaderboards, Cloud saves, achievements, on-screen keyboard |
| Platform | Windows PC + Steam Deck, on Steam | The Deck runs the Windows build through Proton |
| Camera | First-person and third-person with Cinemachine. Each player chooses and can change view with the mouse scroll wheel or other custom controls; a floor may switch view (e.g. a trivia screen) | Players play the way they prefer; third person shows their character and makes parkour and fights easier |
| Character controller | Starts from Unity's free *First Person + Third Person \| Character Controllers* package, adapted to FishNet | Working first-person and third-person controllers and cameras on day one instead of building from scratch |
| Input | Unity Input System | One set of actions for keyboard/mouse and controller; supports custom controls, such as for changing camera view |
| Text | Unity Localization package (string tables) | English only at launch; more languages later without code changes |
| Voice chat | Later, after launch | Pick the tool then: Steam's voice functions or a voice service |
| Greyboxing | ProBuilder | Build test levels inside Unity |
| Monster AI | AI Navigation package (NavMesh) | Pathfinding |
| Tests | Unity Test Framework (EditMode, PlayMode); Multiplayer Play Mode for 2–4 players in the editor | |
| Agent tooling | Unity CLI (free) | Lets AI agents work with the open Unity Editor: compile, run tests, run builds. The owner doesn't have to copy errors back and forth |
| Version control | Git + Git LFS on GitHub (public repo for now) | Watch LFS storage limits |

### Steam Deck (planned from day one)
Valve's checks for the "Deck Verified" badge, and what they mean here:
- **Controls:** every action works on the Deck's controls with the default layout. No mouse-only menus. On-screen button prompts match the device in use (Valve recommends its Steam Input API for this).
- **Typing:** any text entry (Spelling Bee, names) opens Steam's on-screen keyboard with `ShowFloatingGamepadTextInput` or `ShowGamepadTextInput`.
- **Screen:** run at 1280×800 (Valve's preferred; 1280×720 also allowed). Smallest text at least 9 px tall at that size; aim for 12 px. Layouts must also work on other screen shapes (16:9, ultrawide).
- **Speed:** at least 30 fps at 1280×800 on default settings (Valve's bar). Aim higher.
- **No blockers:** no launcher, no "unsupported device" warnings, no middleware that fails under Proton (e.g. some anti-cheat). Matters when choosing PvP anti-cheat.
- Profile on a real Deck early (see section 13, question 1).

Not chosen:
- **Unreal Engine 5** — better graphics and strong built-in netcode, but its beginner scripting (Blueprints) is saved as binary files AI agents can't edit, C++ is slow to iterate, and the editor is heavy.
- **Netcode for GameObjects** — Unity's official, free option with the most tutorials. FishNet was picked for its built-in client-side prediction, which PvP will need.
- **Photon Fusion** — capable, but cost grows with concurrent users and ties us to one vendor.

---

## 6. Code architecture (inside Unity)

### Layers
Each layer is its own assembly (asmdef), so wrong dependencies fail to compile.

```
Core           pure rules, depends on nothing
  ▲
FloorSDK       floor contract + shared floor helpers
  ▲
Floors/<Name>  one per floor, never reference each other

Game           elevator, lobby, player, HUD, run manager   (uses Core, FloorSDK)
Services       backend clients behind interfaces            (uses Core)
Editor         validators, import tools, menus
```
- **Core**: no `UnityEngine`, no FishNet (asmdef `noEngineReferences: true`). Run state, hearts, scoring, floor sequence, difficulty levels, visual tiers, unlock milestones, seeded RNG, rule data. Fully unit-tested.
- **Services**: each has a fake version so the game runs offline.
- Each module is an asmdef named `OneMoreFloor.<Layer>` with `autoReferenced: false`, so every dependency must be listed in its asmdef. `Core` also blocks engine references and all precompiled DLLs.
- The `Editor` module's namespace is `OneMoreFloor.EditorTools`, because a namespace called `Editor` would clash with Unity's `Editor` class.
- Each floor gets its own asmdef when it is built.

### Who owns state
- The host owns run state: phase, floor number, hearts, seed, players, config version. Clients get read-only copies (SyncVars).
- Clients send requests ("I reached the exit", "my answer is B", "I fired here"). The host checks and decides, including every heart loss and every hit on a monster.
- Run phases: `Lobby → Elevator → FloorIntro → FloorPlay → FloorCleared → Elevator …`. `GameOver` can happen at any moment hearts hit 0, then `Results`.

### Solo = host with one player
Solo starts a local host session (no internet) on the same code path as multiplayer. Never write solo-only gameplay code. Turning a single-player game into multiplayer later usually means rewriting it.

### Movement
Now: each client moves its own character; the host decides outcomes. Fine for co-op. PvP needs host-authoritative movement with FishNet's prediction, so keep movement in one component behind an interface.

### Tuning without code
- `GameRules` asset: hearts per player, elevator ride length, intro and countdown lengths, respawn delay.
- `MovementSettings` asset: run speed, jump height, gravity, air control — tuned by the owner in the Inspector.
- Per floor: `FloorDefinition` + its settings asset (section 4).
- `DirectorSettings`: weights, no-repeat rules, difficulty curve, visual tier ranges.
- `Unlocks` asset: floor milestone → cosmetic.
- Later: export the same data as versioned JSON on a CDN, so balance changes ship without a new build. Every run records the config version it used.

### Player save
- Holds unlocks, best floors and settings. Synced between devices with Steam Cloud (e.g. PC ↔ Steam Deck).
- Once the backend exists (P2), the server's records are the source of truth for best floors.

---

## 7. Assets and art (current priority)

### The key idea
A 3D model is a stage set: shapes and paint, no behaviour. A door from Blender can't open, block anyone or check for a key until Unity code is attached. So:
- Blender (or any AI tool) makes **pieces**: separate, named objects, plus empty **marker** points that say *where* things happen.
- Unity code decides *what* happens.
- Never deliver a whole floor as one welded mesh. Anything that moves or reacts is its own object — including every piece of a building that is meant to collapse.

### Order of work (art track; how it fits with code: section 9)
1. **Check the characters** against the checklist below. Fix what's needed before other art depends on them.
2. **Three style explorations**, built around the owner's characters. For each style: concept images of the lobby, one tier-1 room and one tier-3 world, with the characters in the scene. Use the owner's setting if `docs/STORY.md` exists; otherwise a neutral building. The owner picks one style. This proves the style fits the characters and stretches from an ordinary office to an epic world.
3. **Style guide** (`docs/STYLE_GUIDE.md`, one page) for the chosen style: palette, shapes, lighting mood, UI look, kit grid size, and how the look changes per tier.
4. **2D concept images** in that style: lobby, elevator, each starter floor. Tier 1 first. Check each at Steam Deck size (1280×800): small details and thin text disappear.
5. **Greybox** each floor in Unity with plain boxes. Play it. Fix gap widths, room sizes and paths until it's fun — they depend on how far the character runs and jumps.
6. **Modular kits** that match the greybox: walls, floor tiles, stairs, door frames, railings, props. Tier 1 first.
7. **Swap**: replace grey boxes with art; keep markers and colliders.
8. **Polish**: lighting, effects, sound.
9. **Tier 2–3 rooms** once the tier-1 game works.

### Which tool for what
| Job | Tool |
|---|---|
| Mood and style exploration | 2D image models |
| Single props and creatures | AI 3D generators (e.g. Meshy, Tripo, Hunyuan3D), then clean up in Blender |
| Modular kits, blockouts, batch fixes, export | An agent writing Blender Python scripts (Codex with GPT-6 Astra, or Claude Code). Scripts beat clicking through Blender's screen: re-run with new sizes when the greybox changes |
| Layout, markers, prefabs, game logic | Unity |
| Character rigs | Unity Humanoid rig. Auto-riggers handle clean human shapes; unusual bodies need manual rigging |

### Characters
Decision: use the owner's existing Blender characters. Their look sets the art style, so environments are styled to match them, not the other way round.

Checklist before they go into the game:
- [ ] **Rig:** a human-like skeleton that Unity's Humanoid type can map (hips, spine, head, arms, legs). Best: all characters share one skeleton, so every animation works on every character.
- [ ] **Animations:** teammates and the third-person view show the character all the time, so it needs a full set — idle, walk, run, jump (take-off, in air, land), fall, climb, hit, knocked down, respawn, interact, aim and shoot, celebrate. Start with the animations from Unity's free character controller package (build step 2d); add a Humanoid pack with shooting animations for Monster Arena. Any Humanoid animation plays on any Humanoid rig.
- [ ] **Materials:** Blender's node materials don't carry over to Unity. Bake them into texture images (colour, normal, roughness/metal), then rebuild them as URP materials.
- [ ] **Colour areas:** a mask texture marks which parts take the player's colour, so team colours and colour unlocks need no new art.
- [ ] **Same hitbox for everyone:** one capsule size for all characters, whatever they look like. Fair in co-op, required for PvP.
- [ ] **Easy to tell apart:** distinct shapes plus a player colour, so 4 teammates are clear at a distance and on the Deck's small screen.
- [ ] **Unlockables fit:** extra characters and outfits use the same skeleton and hitbox.
- [ ] **Loose parts** (capes, long hair, tails): need extra bones with physics, or they look stiff and poke through walls.
- [ ] **First-person body:** what a player sees of their own character in first person (section 13, question 10).
- [ ] **Budget:** ≤ 20k triangles at full detail, 3 LODs (4 players + monsters on screen, on a Steam Deck).
- [ ] **Export:** about 1.8 m tall, transforms applied, faces +Z in Unity.
- [ ] **AI log:** if any AI tool helped make them, record it in `art-source/ASSET_LOG.csv` (for Steam's disclosure form).

### Rules for every asset
- Sizes: every room, door, corridor, stair and elevator size comes from `docs/sizes.json` (metres). Game spaces are bigger than real ones, because in third-person view the camera sits a few metres behind the player and needs room. The numbers are starting guesses until build step 5.
- Scale: 1 Blender unit = 1 m = 1 Unity unit. Player ≈ 1.8 m tall.
- Kit pieces snap to one grid (size set in the style guide).
- Origin: bottom centre for props; one fixed corner for kit pieces.
- Apply transforms before export (rotation 0, scale 1). In Unity, models stand upright and face +Z.
- One shared export preset or script for everything. FBX by default.
- Names:
  - `SM_` static mesh, `SK_` rigged mesh, `M_` material, `T_` texture, `A_` animation.
  - `COL_` simple collision mesh → the import script turns it into an invisible collider.
  - `MK_` empty marker → the import script turns it into the matching anchor: `MK_Spawn_01`, `MK_Exit`, `MK_Checkpoint_02`, `MK_Key_A`, `MK_MonsterSpawn_01`, `MK_Objective_01`, `MK_Hazard_01`.
- Budgets *(defaults; profile on a Steam Deck and adjust)*: character ≤ 20k triangles at full detail, 3 LODs; a floor ≤ ~500k visible triangles; textures 1K–2K.
- Share materials and combine static pieces where possible: fewer separate objects = faster, especially on the Deck.
- Get one asset fully working in Unity (scale, names, colliders, markers) before making many.
- New files in `Assets/` need their Unity `.meta` files (the ID Unity uses to find each file) committed with them. Before committing new assets, click into Unity once so it creates them.
- Source files (`.blend`, concept art) live in `art-source/`, outside `Assets/`, in Git LFS.

### AI asset hygiene
- Check each tool's licence for commercial use. Some free tiers are non-commercial; Hunyuan3D's licence excludes the EU, UK and South Korea.
- Log every AI-made asset in `art-source/ASSET_LOG.csv`: file, tool, prompt or source, licence, date.
- Steam requires disclosing AI-generated content that players see or hear; AI coding tools don't need disclosure. The log makes the store form easy.
- Usual fixes for AI meshes: wrong scale, off-centre origin, too many triangles, lighting baked into textures, fused parts that need to move, messy UVs.

---

## 8. Online architecture (built to scale, delivered in phases)

### Why this game scales easily
Each run is a sealed bubble of 1–4 players, and bubbles never interact. More players = more bubbles side by side (horizontal scaling). Only accounts, matchmaking and the leaderboards are shared, and those are small stateless web services that scale by running more copies.

### Target pieces
1. **Game client** — Unity.
2. **Run host** — co-op: one player's PC (listen server) through Steam's relay. PvP and "verified" runs: a headless Unity server built from the same project.
3. **Backend services** — Go, stateless, behind a load balancer:
   - **Auth**: checks the Steam login ticket, issues our own token (JWT).
   - **Runs** (write side): issues a run ticket (run ID, seed, config version, signature); receives results; checks them; saves; updates the player's best floors (which drive unlocks); announces "run finished".
   - **Leaderboards** (read side): solo and multiplayer rankings in Redis; serves top lists and "my rank".
   - **Matchmaking / parties** (later): queues by region, mode and team size; books dedicated servers for PvP.
   - **Config**: versioned balance files on a CDN.
4. **Data**
   - **PostgreSQL** — source of truth: players, runs, per-floor results, unlocks.
   - **Redis (or Valkey)** — leaderboards (sorted sets), cache, rate limits. Always rebuildable from Postgres.
   - **Object storage + CDN** — config files; later replays and content packs. Cloudflare R2 fits (no download fees; the owner already uses Cloudflare).
5. **Event stream** — NATS JetStream first (simple to run); Kafka or Redpanda only when volume demands it. Events: `RunStarted`, `FloorCleared` (time, hearts lost per player), `RunFinished` (floor reached). Listeners: leaderboards, unlocks, cheat checks, analytics, achievements.

### Patterns, in plain words
- **Clean Architecture** — game rules don't know about Unity, networks or databases. Rules sit in the middle; tools plug in around them and can be swapped without touching the rules.
- **CQRS** — saving a score and reading a leaderboard are separate paths. Saving is careful (check, store, announce). Reading uses a fast copy in Redis. Reads far outnumber writes, so each side scales on its own.
- **Event streaming** — the run service announces "run finished" once; each interested service reacts. A new feature = a new listener; nothing else changes.
- **Outbox** — the run record and its announcement are saved in one database transaction; a relay sends the announcement after. No lost or phantom events.
- **Idempotency** — the run ID is the key, so sending the same result twice changes nothing.

### Saving: fast vs. safe
- During a run: state lives in the host's memory. No database writes mid-run.
- At each elevator ride: a tiny checkpoint (floor, hearts, seed, players) so a dropped player can rejoin.
- End of run: one database transaction (always consistent).
- Leaderboards: updated from events within seconds (briefly behind is fine). If Redis is lost, rebuild from Postgres.

### Leaderboard integrity
Solo and co-op runs execute on players' PCs, so results can be faked. Defences, cheapest first:
1. Server-issued run ticket: seed + signature.
2. Per-floor log with timings and heart losses; the server checks it is possible (floor order matches the seed, no impossible times).
3. Outlier detection on scores and times.
4. Review top entries (replays, later).
5. "Verified" boards only for runs on our dedicated servers (PvP, tournaments).

Start with 1–3. Running every run on our own servers stops most cheating but costs money for every session. Ban confirmed cheaters by Steam ID; in a paid game, a new account means buying a new copy.

### Rough scale math
Assumptions, not targets: 1,000,000 players online, average team of 2.
- Live runs: 1,000,000 ÷ 2 = 500,000.
- Average run 15 min = 900 s → runs finishing per second: 500,000 ÷ 900 ≈ 556 → about 556 small database transactions per second. Fine with batching and monthly table partitions.
- One floor event per run per minute: 500,000 ÷ 60 ≈ 8,300 events per second. Fine for NATS or Kafka.
- Conclusion: the database isn't the hard part; game servers are. Co-op on players' PCs costs us almost nothing; dedicated servers cost money for every live match. Hence co-op = player-hosted, PvP = dedicated.

### Dedicated server hosting (decide at PvP)
Edgegap, Gameye, Multiplay (run by Rocket Science Group since Unity ended its own service on 31 Mar 2026), or self-managed Agones on Kubernetes.

### Phases
The step-by-step order, with who does what, is in section 9.

| Phase | Scope | Online pieces |
|---|---|---|
| P0 Pre-production (now) | Project setup, core rules, your character in Unity, movement, game loop, greybox Trivia and Collapse Escape, style guide | None |
| Vertical slice | Collapse Escape, the lobby and the elevator at final quality | None |
| P1 Solo slice | Monster Arena, then Trivia, Collapse Escape and Monster Arena all at final quality; difficulty levels, menus, local best floor, full controller support | None (local host) |
| P2 Online basics | Steam login, solo leaderboard, run tickets + checks, Steam Cloud saves | Steam leaderboards, or one Go service + Postgres + Redis |
| P3 Co-op 2–4 | Steam lobbies and invites, shared hearts, team clear rules, disconnects, multiplayer leaderboard | + Steam relay |
| Launch (MVP) | 12–15 floors, tier 2–3 rooms for some floors, cosmetic unlocks, Steam Deck check | — |
| P4 After launch | In-game voice chat, analytics (which floors drain the most hearts), remote balance, seasons, daily seed | + event stream, CDN config, voice tool |
| P5 PvP | Dedicated servers, host-authoritative movement + prediction, lag compensation, Proton-friendly anti-cheat, matchmaking, skill rating | + server hosting, matchmaker |

The other floors (Spelling Bee, Parkour Run, Key Hunt, then new ones up to 12–15 in total) and tier 2–3 rooms are built alongside P2–P3.

Don't build before P4–P5: Kafka, Kubernetes, microservices, matchmaking.

### Key trade-offs
| Decision | Gain | Cost |
|---|---|---|
| Solo built on host mode | No rewrite for multiplayer | A bit more complex from day one |
| Co-op player-hosted + Steam relay | Near-zero server cost | Host can cheat; host leaving ends the run |
| PvP on dedicated servers | Fair, harder to cheat | Money and ops work |
| FishNet | Free; prediction built in | Smaller community than Unity's official netcode; raycast lag compensation is paid |
| Postgres + Redis | Safe records, fast rankings | Two systems to run |
| NATS before Kafka | Simple start | A migration if volume grows |
| Floors as data + modules | Designers tune safely; agents build floors in isolation | Framework work up front |
| One room per visual tier, same mechanics | Floors get more epic without new code | More art per floor |
| Steam Deck from day one | Bigger audience; no costly fixes later | Tighter art budgets; controller-first UI |
| Cosmetic-only unlocks | Fair leaderboards | Fewer power rewards to chase |
| Unity over Unreal | AI-editable code, fast iteration | Lower graphics ceiling (not needed) |
| Player chooses first or third person | Players play the way they prefer | Two cameras to build; every floor tested in both; fairness to watch on shared leaderboards |

---

## 9. Build order

Three tracks run side by side:
- **You (owner):** floor rules, story, style choice, playtesting, decisions.
- **Claude Code:** all code.
- **Codex + GPT-6 Astra:** art — Blender scripts for building pieces and exports, AI 3D tools for props and monsters.

Folder ownership: Codex works only in `art-source/`, `tools/blender/` and `Assets/_OneMoreFloor/Art/`; Claude Code owns everything else; `HANDOFF.md` is shared. Only one agent runs at a time: each session starts by reading `HANDOFF.md` and ends with a new entry there and a commit.

Each phase ends at its **gate**. Each P0 step ends with a **Done when** check. Don't move on until it passes.

First three floors, in this order: **Trivia → Collapse Escape → Monster Arena** (shooting). Spelling Bee, Parkour Run and Key Hunt come after P1.

### Before step 1 — owner setup (done 4 Nov 2026)
- Install Unity Hub, sign in, and install Unity 6.3 LTS (newest patch). Install Git, Git LFS and Blender.
- Create an empty GitHub repo (no README, no .gitignore). It is public for now (section 13, question 12).
- In Unity Hub: New project → Universal 3D (URP) template → Create new local project, name OneMoreFloor → tick Use Unity CLI.
- Put `AGENTS.md` and `CLAUDE.md` in the project folder, and `PROJECT_CONTEXT.md` in its `docs` folder. Open a terminal there and start Claude Code.
- Install the Unity CLI: `winget install Unity.CLI`, then `unity auth login`. Ticking Use Unity CLI only adds the project's package, not the command itself (done 5 Nov 2026).

### P0 — Foundations (now)

**Step 1 — Project setup** (Claude Code) — done 5 Nov 2026
- Git: Unity `.gitignore`, Git LFS rules for art and audio files, connected to the GitHub repo.
- Packages: Input System, Cinemachine, ProBuilder, AI Navigation, Localization, Test Framework, Multiplayer Play Mode, FishNet.
- The folder layout (section 10) and the asmdef layers (section 6).
- The Unity CLI connection, plus one EditMode test it can run.
- **Done when:** Unity opens with no errors in the Console, the test passes from the command line, and the first commit is on GitHub.

**Step 2 — Five jobs at once.** 2a and 2b don't need step 1; start them any time. The rest start once step 1 is done.
- **2a. You — floor specs.** Write `docs/floors/trivia.md` and `docs/floors/collapse_escape.md` from `docs/floors/_TEMPLATE.md`. Monster Arena's spec can wait until P1.
  - **Done when:** Claude Code can build each floor without asking you a design question.
- **2b. You — 3 style tests.** In an image tool, make 3 sets of concept images, one set per style: the lobby, an ordinary office floor (tier 1) and an epic world (tier 3), with your character in each.
  - **Done when:** the 3 sets sit side by side, ready to compare.
- **2c. Claude Code — core rules** in plain C# with unit tests: hearts pool, run phases, seeded floor picker (no repeats), difficulty levels, visual tiers, and the data assets that hold their numbers.
  - **Done when:** all EditMode tests pass from the command line.
- **2d. Your character in Unity.**
  - **Codex:** a shared Blender export script that applies the asset rules (section 7); run it on one of your characters.
  - **You:** download Unity's free *First Person + Third Person | Character Controllers* package from the Asset Store. Its first-person and third-person controllers are the starting point for step 3; its animations (or any free Humanoid pack) are used for this test.
  - **Claude Code:** import your character as Humanoid and play those animations on it.
  - **Done when:** your character idles, runs and jumps in Unity at the right size (~1.8 m), with nothing stretched or broken.
- **2e. Codex — generator scripts.** Blender scripts in `tools/blender/` that build the elevator and a basic room kit (wall pieces, floor tiles, door frames, stairs) from the numbers in `docs/sizes.json`, following the asset rules (doors as separate objects, `MK_` markers, `COL_` shapes). For now they make plain grey versions; Claude Code may use them in steps 4–5 instead of its own boxes.
  - **Done when:** changing a number in `docs/sizes.json` and re-running the script gives a new elevator that imports into Unity at the right scale, with its doors as separate objects.

**Step 3 — Movement and camera** (Claude Code)
- First-person and third-person controller and cameras, built from the package in 2d and adapted to FishNet: each player moves their own character, and everyone sees everyone.
- Each player chooses their view and can change it with the mouse scroll wheel or other custom controls (section 2).
- The camera doesn't clip through walls in either view. Keyboard/mouse and controller both work.
- Movement numbers (run speed, jump height, gravity) live in the `MovementSettings` asset, which you tune in the Inspector.
- **Done when:** in both views, you can run, jump, look around and change view with a keyboard and mouse and with a controller, and 2 players in the editor see each other move smoothly.

**Step 4 — Game loop** (Claude Code)
- Lobby → ready up → elevator ride (doors close, next floor loads) → title card → play → floor cleared → back in the elevator, floor count +1.
- Hearts on screen; game over at 0 hearts, even mid-floor; results screen; back to the lobby.
- A placeholder test floor: one button clears it, one button costs a heart.
- Runs as a local host. From here on, every test also runs with 2 players in the editor.
- All text from the string tables; all menus work with a controller.
- **Done when:** lobby → 3 test floors → game over → results → lobby works, solo and with 2 players.

**Step 5 — Greybox the first two floors** (Claude Code builds, you playtest)
- **5a. Trivia:** a plain room with a question screen, answering by keyboard or controller, the clear rule and heart costs from your spec, and a sample question file (you supply the real questions).
- **5b. Collapse Escape:** a plain-box building section — corridors, stairs, doors — with the collapse, falling debris, checkpoints and the exit as your spec describes.
- **5c. You:** playtest both and tune movement in `MovementSettings`. Then put the real sizes in `docs/sizes.json`: corridor and door widths, stair step height, ceiling height, elevator size, and the widest gap and highest ledge a player can jump. Codex re-runs the generator scripts with them.
- **Done when:** both floors can be cleared and can drain hearts to game over, solo and with 2 players, and `docs/sizes.json` holds the real numbers.

**Step 6 — Pick the style** (You)
- Pick one of the 3 style tests and write `docs/STYLE_GUIDE.md` (one page): palette, shapes, lighting mood, UI look, kit grid size (from 5c), and how the look changes per tier.
- **Done when:** someone could make a matching asset from the page alone.

**Gate:** Trivia and Collapse Escape playable start to finish in greybox with your character, solo and with 2 players in the editor.

### Vertical slice — one floor at final quality
7. **Claude Code:** import tools (`MK_` markers → anchors, `COL_` meshes → colliders) and a floor validator.
8. **Codex + Astra:** the tier-1 building kit for Collapse Escape (walls, floors, stairs, doors and breakable pieces): the generator scripts from 2e, re-run with the real sizes and given the chosen style. Then its props (AI 3D tools, then cleanup), the lobby and the elevator.
9. **Everyone:** Collapse Escape, lobby and elevator with final art, lighting, sound and collapse effects (dust, debris, camera shake). Fix every pipeline problem here: scale, names, materials, colliders.

**Gate:** one floor looks and plays like the final game. Only now mass-produce art.

### P1 — Solo slice
10. **Monster Arena** (shooting): your spec → greybox → playtest → art. Covers aiming, weapons, hit checks done by the host, monster AI (NavMesh), animations and effects. Then the art pass for Trivia.
11. **Claude Code:** difficulty levels in play, menus, settings (including camera view and its controls), local best floor, full controller support. Test on a Steam Deck.
12. **You:** register on Steamworks and put the Steam page up with final-looking screenshots and a short trailer, so wishlists start building.

**Gate:** Trivia, Collapse Escape and Monster Arena at final quality, fun solo, playable with only a controller.

### P2 — Online basics
13. Steam login, solo leaderboard, Steam Cloud saves. Start with Steam's built-in leaderboards; add the Go service with run checks (section 8) when cheating appears or Steam's version isn't enough.

### P3 — Co-op
14. Steam lobbies and invites (FishySteamworks), every floor with 2–4 players, team clear rules, disconnects, multiplayer leaderboard.

Alongside P2–P3: the other floors — Spelling Bee, Parkour Run, Key Hunt, then new ones up to 12–15 in total (same loop: spec → greybox → playtest → art) — and tier 2–3 rooms.

### Launch
15. Cosmetic unlocks, hearts and difficulty balance, achievements, Steam Deck check, closed playtests, a demo for Steam Next Fest, then release.

### After launch
16. P4: voice chat, analytics, remote balance, daily seed, seasons.
17. Epic Games Store, then consoles, if Steam sales justify it (recommended; owner decides).
18. P5: PvP.

---

## 10. Repo layout (target)

```
OneMoreFloor/
  AGENTS.md                 shared always-on rules for Claude Code and Codex
  CLAUDE.md                 imports AGENTS.md, plus Claude Code notes
  HANDOFF.md                notes between agents (both write to it)
  docs/
    PROJECT_CONTEXT.md      this file
    sizes.json              shared sizes in metres, read by Blender scripts and Unity (owner)
    STORY.md                story and setting, written by the owner
    STYLE_GUIDE.md
    FLOOR_AUTHORING.md      how to build a floor, step by step
    floors/_TEMPLATE.md     blank floor spec
    floors/<floor-id>.md    one-page spec per floor, written by the owner
  art-source/               .blend files, concept art, ASSET_LOG.csv (Git LFS)
  tools/blender/            Blender scripts: export, kit builders (Codex)
  Assets/_OneMoreFloor/
    Core/ FloorSDK/ Floors/<Name>/ Game/ Services/
    Art/Shared/             kits, characters, shared materials
    Data/                   GameRules, MovementSettings, DirectorSettings, Unlocks, FloorDefinitions, floor settings
    Localization/           string tables (English)
    Editor/                 validators, import tools
    Tests/                  EditMode, PlayMode
  Assets/FishNet/           vendored, read-only
  Assets/DefaultPrefabObjects.asset   generated by FishNet
backend/                    later (may become its own repo): Go services, DB migrations, docker-compose
```

Empty folders hold a `.gitkeep` so Git keeps them; delete it when real files arrive. Codex creates its own folders (`art-source/`, `tools/blender/`, `Assets/_OneMoreFloor/Art/`) when it first needs them.

---

## 11. Testing

- Run EditMode tests from the command line with Unity closed: `unity test . --mode EditMode`. Exit code 0 = passed, 8 = a test failed, 6 = the run never finished (e.g. compile errors; the reason is in `%LOCALAPPDATA%\Unity\Editor\Editor.log`).
- Agents can also check an open Unity through the CLI: `unity open .`, wait for `unity status` to say `ready`, then `unity command console status`, `console get --level error`, `package list`, `test run --mode editor`, `editor.playmode start` and `stop`, and `capture game_view --output-path <file>` for a screenshot. Close it with `unity command editor quit`.
- What agents can't judge, and the owner tests: how it feels, looks and plays, and real hardware (controller in hand, Steam Deck).
- Core rules: EditMode unit tests — hearts maths, multi-heart mistakes, game over mid-floor, floor order from a seed, difficulty level and visual tier for a floor number, unlock milestones.
- Every floor: a PlayMode smoke test that clears it, and one that drains hearts to game over — at level 1 and at its hardest level.
- Every floor follows its spec's numbers (e.g. Spelling Bee clears only after 2 passes per player).
- From build step 4 on, every floor is tested with 2 players in Multiplayer Play Mode, even before co-op ships. Networking changes: test with 2–4 players.
- Same seed → same floor order, every time.
- Controller only: play a full run without touching mouse or keyboard.
- Camera views: every floor works in first-person and third-person view, unless its spec limits the view.
- Before closing a milestone: test a standalone build, not just the editor. From P1 on, also on a Steam Deck: at least 30 fps, text readable, on-screen keyboard works.

---

## 12. Decision log

- 3 Nov 2026 — New project from a clean slate.
- 3 Nov 2026 — Modes: solo first, then co-op (max 4). PvP in a later release, not the MVP.
- 3 Nov 2026 — PC on Steam first.
- 3 Nov 2026 — Third-person camera (replaced on 5 Nov: players now choose).
- 3 Nov 2026 — A floor can't be failed or skipped: mistakes cost hearts until the team clears it or the hearts run out.
- 3 Nov 2026 — Hearts can drop mid-floor; one mistake can cost more than 1 heart.
- 3 Nov 2026 — The run ends the moment hearts hit 0. No cash-out mechanic.
- 3 Nov 2026 — Two leaderboards: solo and multiplayer.
- 3 Nov 2026 — Art style: explore 3 styles, then pick one.
- 3 Nov 2026 — The owner sets each floor's rules. No global default. Spelling Bee: every player passes at least twice.
- 3 Nov 2026 — Some floors may give hearts back in a later version.
- 3 Nov 2026 — Floors get more epic as the team climbs; higher floors can open onto other worlds.
- 3 Nov 2026 — Tone: tense but fun.
- 3 Nov 2026 — Characters: use the owner's existing Blender characters.
- 3 Nov 2026 — Paid game, one price. No in-game shop.
- 3 Nov 2026 — Story and setting: designed by the owner.
- 3 Nov 2026 — English only at launch.
- 3 Nov 2026 — In-game voice chat later, not at launch.
- 3 Nov 2026 — 12–15 floors at launch.
- 3 Nov 2026 — Floors get harder as the team climbs; each floor's spec says how.
- 3 Nov 2026 — Steam Deck planned from day one.
- 3 Nov 2026 — Between runs: cosmetic unlocks only. No power upgrades.
- 4 Nov 2026 — Owner setup done: Unity 6.3 LTS project (Universal 3D, local, Unity CLI on).
- 4 Nov 2026 — First three floors: Trivia → Collapse Escape → Monster Arena. Spelling Bee, Parkour Run and Key Hunt come after.
- 4 Nov 2026 — Monster Arena is a shooting floor.
- 4 Nov 2026 — Agents run one at a time and pass information through `HANDOFF.md`.
- 5 Nov 2026 — Camera: each player chooses first-person or third-person view and can change it with the mouse scroll wheel or other custom controls.
- 5 Nov 2026 — Correction: FishNet's licence is its own, not MIT. It is free and royalty-free and allows games you sell.
- 5 Nov 2026 — Agents update the relevant Markdown files whenever something notable changes.
- 5 Nov 2026 — The commit history lists only `OrangeCatLoves`, with no co-author or AI attribution lines.
- 5 Nov 2026 — The GitHub repo stays public for now.
- 5 Nov 2026 — Agents run every check they can; the owner tests only what needs a person.
- 5 Nov 2026 — Build step 1 confirmed by the owner.

---

## 13. Open questions

1. Do you have a Steam Deck to test on? Needed by the end of P1.
2. Voice chat (later): does the whole team hear everyone, or only nearby players?
3. Heart rewards (later): is there a maximum, e.g. the starting amount?
4. Multiplayer tie-break: lower total time (default)? Decide with the leaderboard design.
5. PvP format: race the same floors, 1v1, or team vs team? (Later.)
6. Monster Arena: aim assist for controller and Steam Deck players? Decide in its spec.
7. Defaults to confirm: lobby contents (section 2); no joining mid-run; a run's board is set by its starting team size; difficulty +1 level every 5 floors.
8. Camera view: does the scroll wheel jump between first and third person, or zoom smoothly between them? Decide before build step 3.
9. Camera view: which view do players start in, where do they choose it, and what changes view on a controller or Steam Deck (no scroll wheel)? Decide before build step 3.
10. First-person view: what do players see of their own body, arms only or the full body? Decide before build step 3.
11. Camera view: third person can see around corners, and the leaderboards are shared. Should any floor (e.g. Key Hunt) or PvP limit the view?
12. Repo: when should it go private? It is public for now, and the game will be sold, so anyone can read the code and art and build the game while it stays public.

---

## 14. Glossary

- **Prefab** — a saved, reusable game object (model + scripts + settings). Like a class you instantiate.
- **ScriptableObject** — a data asset edited in Unity's Inspector. Like a typed config file.
- **Greybox / blockout** — a level built from plain boxes to test gameplay before art.
- **Vertical slice** — one floor built to final quality, to prove the whole pipeline before mass production.
- **Gate** — the check that must pass before the next phase starts (section 9).
- **Visual tier** — the look band a floor number falls in (section 2).
- **Difficulty level** — a number worked out from the floor number that tells a floor how hard to be (section 2).
- **Aim assist** — the game gently pulls a controller player's aim toward targets, so sticks can compete with a mouse.
- **Host / listen server** — one player's game also acts as the server.
- **Dedicated server** — a server program with no player, running in the cloud.
- **Relay** — a middleman that passes traffic when players can't connect directly.
- **SyncVar / RPC** — FishNet's synced variable / remote function call.
- **Client-side prediction** — your movement shows instantly; the server corrects it if it disagrees.
- **Lag compensation** — the server rewinds time to judge hits fairly for laggy players.
- **Hitbox** — the invisible shape the game uses for collisions and hits.
- **NavMesh** — a map of walkable areas that monsters use to find paths.
- **LOD** — lower-detail copies of a model used at a distance.
- **Rig / Humanoid** — the skeleton that makes a model animate; Unity's Humanoid type lets characters share animations.
- **Bake** — turn Blender's node materials into plain texture images that Unity can read.
- **Cinemachine** — Unity's camera package; handles the first-person camera and a third-person follow camera that doesn't clip through walls.
- **First-person / third-person view** — seeing through the character's eyes / from a camera behind the character. Also called POV (point of view).
- **String table** — a list of all on-screen text, one entry per line of text, so it can be translated without touching code.
- **Proton** — Valve's layer that runs Windows games on Steam Deck.
- **Steam Cloud** — Steam's save sync between a player's devices.
- **asmdef** — Unity assembly definition; splits code into modules and blocks unwanted dependencies.
- **URP** — Universal Render Pipeline, Unity's general-purpose renderer.

---

## 15. References (checked 3–5 Nov 2026)

- Unity plans and pricing: https://unity.com/products/pricing-updates
- Unity 6 LTS versions and support dates: https://unity.com/ja/releases/unity-6/support
- Unity CLI (replaces the old Unity MCP server): https://docs.unity.com/en-us/unity-cli/replace-mcp-server-unity-cli
- Unity's free character controller package (First Person + Third Person | Character Controllers, URP, works with 6.3): https://assetstore.unity.com/packages/slug/196526
- Unreal Engine licence: https://www.unrealengine.com/license
- FishNet features (prediction; Pro features): https://fish-networking.gitbook.io/docs/overview/readme/features
- FishNet licence: https://github.com/FirstGearGames/FishNet/blob/main/LICENSE.md
- Unity Multiplay handover: https://docs.unity.com/multiplay-hosting/concepts/ecosystem
- Steam Deck compatibility checks: https://partner.steamgames.com/doc/steamdeck/compat
- Steam Deck developer recommendations (on-screen keyboard): https://partner.steamgames.com/doc/steamdeck/recommendations
- Steam AI disclosure (Jan 2026 rewrite): https://videogameschronicle.com/news/valve-has-significantly-rewritten-steams-rules-for-how-developers-much-disclose-ai-use
- Claude Code CLAUDE.md / AGENTS.md: https://code.claude.com/docs/en/memory.md
