# Floor spec — Trivia

id: `trivia` · Genre: Q&A · Status: draft

Copy this file to `docs/floors/<floor_id>.md` and fill it in. Write rules, not code. Anything you haven't decided goes under "Open questions" — agents will ask instead of guessing.

## Goal (one line)
Trivia - Time to put your knowledge to a test

## How it plays
The lift door opens and the Goal line plays and shows on the screens for each of them. There will be a ready button placed at the center of the screen and any of the players must hit it for the game session to start. There could be different variants of this game. The first question will flash on the screen inside the room and the users might have to input their answer in a text box on their screen. Or, (for other variants), the platform below the room will transform and there will be 4 sections representing 4 different shapes that corresponds to the options, something like Kahoot and they need to stand on the correct option. It could also be 4 buttons spring up from the ground that the players have to click to indicate their answer. If they answer wrongly, stand on the wrong answer platform or click the wrong button that indicates the wrong answer, the player will lose 1 heart.

## Cleared when
All players have to clear all questions.

## Costs hearts
When a player answers a question wrongly, they lose a heart and it's collectively from a team. If 2 players answer the same question wrong but 2 players answer ir right, the team will lose 2 hearts all together.

## Stops stalling
Regardless of whether the players are losing hearts continuously or not, the game continues as long as they have enough hearts to continue.

## Gets harder per level
| Level | What changes |
|---|---|
| 1 | 5 questions |
| 2 | 8 questions |
| 3 | 12 questions |

## Solo vs co-op (1–4 players)
What changes with more players, if anything. Who must finish? What do players who finish early do?
There's no difference between solo and multiplayer

## Controls
Keyboard and mouse. Any typing? Any aiming? Does this floor switch or limit the camera view (first or third person)?
There is typing potentially.

## Size
How big should it feel? Use plain words, not metres: how long a clean run takes, how many players must fit, what it should feel like. Claude Code turns this into metres, and you tune it by playing.
It could be different settings like, a normal room, or even an event show setting with a huge audience watching you while you answer questions. (Different designs of room for the same genre of Q&A)

## Rooms
- A normal room
- A huge stage with audience watching you as if you're on show starred in a live television show

## Assets needed
Drafted by Claude Code from the concept images in `art-source/concept/floors/trivia/`. Change anything.
- Room: a game-show stage with curtains, balconies, stage lights and a gold lift door.
- A big question screen with a lit frame.
- 4 podiums, each with a small screen and 4 buttons: red circle, blue square, yellow X, green triangle.
- A host in a purple jacket with a microphone.
- An audience in rows of seats, low detail so a laptop can run it.
- Effects: confetti and moving spotlights.
- Sounds: applause, right and wrong answer stings, button presses, music.
- On-screen: the Ready button, the title card, the question and its answers.

## Open questions
Anything else I have yet to consider
