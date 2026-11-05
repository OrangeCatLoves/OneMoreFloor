# Floor spec — Trivia

id: `trivia` · Genre: Q&A · Status: draft

Items marked *(default — confirm)* were filled in by Claude Code so the floor can be built. The owner can change any of them.

## Goal (one line)
Trivia - Time to put your knowledge to a test

## How it plays
The lift door opens onto a game-show stage with a host and a cheering audience. The goal line plays and shows on each player's screen. Players walk to the 4 podiums. A Ready button sits at the center of the screen, and any player can hit it to start. If nobody does, the game starts by itself after 30 seconds *(default — confirm)*.

Each question and its 4 answers show on the big screen and on the podium screens. Each player votes with their podium's buttons: red circle, blue square, yellow X, green triangle. The team's final answer is decided by the majority of the votes.

## The vote
- **Right answer:** the team moves onto the next question.
- **Wrong answer:** the team loses 1 heart and moves onto the next question.
- **Tie:** the same question is asked again.
- **A player doesn't answer before the timer runs out:** their weight is distributed equally among the rest of the team, so only the votes cast count.
- **Nobody answers before the timer runs out:** 1 heart is lost immediately and the team moves onto the next question.
- **Timer:** 15 seconds per question *(default — confirm)*, tuned by playing.

## Cleared when
The team has gone through all the questions for its level.

## Costs hearts
| Mistake | Hearts lost | What happens next |
|---|---|---|
| The team's final answer is wrong | 1, from the team | Next question |
| Nobody answers before the timer runs out | 1, from the team | Next question |

## Stops stalling
The timer on each question. A team that stops answering keeps losing hearts, 1 per question.

## Gets harder per level
| Level | What changes |
|---|---|
| 1 | 5 questions |
| 2 | 8 questions |
| 3 | 12 questions |

Past level 3 it stays at 12 questions.

## Solo vs co-op (1–4 players)
There's no difference between solo and multiplayer. A solo player's vote is the team's answer. All 4 podiums are there even with fewer players.

## Controls
Keyboard and mouse. No typing. At a podium, the camera moves to a fixed view of the podium and the big screen, and a player clicks a button or presses 1–4 *(default — confirm)*.

## Size
A stage big enough for 4 podiums and the host, in a hall with an audience around it. Tuned by playing.

## Rooms
A game-show stage with an audience watching, as if on a live television show (`art-source/concept/floors/trivia/`). It is the only room for now, used at every visual tier, and is meant only for Trivia.

## Assets needed
Drafted by Claude Code from the concept images. Change anything.
- Room: curtains, balconies, stage lights and a gold lift door.
- A big question screen with a lit frame.
- 4 podiums, each with a small screen and 4 buttons.
- A host in a purple jacket with a microphone, and an audience in rows of seats. The owner wants them to look realistic and to move and jump around as if it's real, like the image.
- Effects: confetti and moving spotlights.
- Sounds: applause, right and wrong answer stings, button presses, music.
- On-screen: the Ready button, the title card, the question and its answers, the timer.

## Later variants (not being built now)
- Players type their answer in a text box on their screen.
- The platform below the room transforms into 4 sections with 4 different shapes for the options, something like Kahoot, and players stand on the correct one.
- 4 buttons spring up from the ground and players click one.
- A normal room instead of the game-show stage.

## Open questions
Asked in chat, 5 Nov 2026:
- Does a 2-1-1 vote count as a majority for the option with 2 votes?
- Can a question be asked again forever when the team keeps tying?
- Can players see each other's votes, and change their own, before the question ends?
- Do the questions themselves get harder per level?
- Who writes the questions?
- Does "realistic" for the host and audience mean the style of the images, or photorealistic?
