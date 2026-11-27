#!/usr/bin/env bash
# Sends commands to a game window in a Multiplayer Play Mode scenario on the home PC, through the
# developer-only FloorTestRemote (Assets/_OneMoreFloor/Dev/FloorTestRemote.cs), and prints its replies.
# Virtual players have no Unity command server, so this is how agents drive Player 2 in 2-player tests.
#
# Usage, from the project root in Git Bash:
#   tools/homepc/remote-player.sh <player> "<command>" ["<command>" ...]
#   <player> is "main" or a virtual player number (2, 3 or 4).
# Commands: place x y z yaw | call Component Method [args...] | call Component get_Property | shot Temp/name.png
# Example:  tools/homepc/remote-player.sh 2 "call TriviaFloor RequestVote 1" "call TriviaFloor get_Stage"
set -euo pipefail

cd "$(git rev-parse --show-toplevel)"
source tools/homepc/common.sh

player="$1"; shift
if [ "$player" = "main" ]; then
    project="$REMOTE_DIR"
else
    # Virtual players are numbered in the order MPPM made their folders (Library/VP/<id>).
    project="$(echo "\$vp = @(Get-ChildItem '$REMOTE_DIR/Library/VP' -Directory | Sort-Object CreationTime); if (\$vp.Count -ge $((player - 1))) { \$vp[$((player - 2))].FullName.Replace('\\', '/') }" | run_remote_powershell)"
    [ -n "$project" ] || { echo "No virtual player $player on $HOST." >&2; exit 1; }
fi

commands="$(printf '%s\n' "$@")"
before="$(echo "if (Test-Path '$project/Temp/FloorTestRemote.out.txt') { (Get-Content '$project/Temp/FloorTestRemote.out.txt').Count } else { 0 }" | run_remote_powershell | tr -dc '0-9')"
printf '%s' "$commands" | ssh -o BatchMode=yes "$HOST" "powershell -NoProfile -Command \"\$input | Set-Content -LiteralPath '$project/Temp/FloorTestRemote.txt'\"" 2>/dev/null

# Wait for the replies (one line per command).
expected=$(( ${before:-0} + $# ))
for _ in $(seq 1 50); do
    sleep 0.3
    count="$(echo "if (Test-Path '$project/Temp/FloorTestRemote.out.txt') { (Get-Content '$project/Temp/FloorTestRemote.out.txt').Count } else { 0 }" | run_remote_powershell | tr -dc '0-9')"
    if [ "${count:-0}" -ge "$expected" ]; then
        echo "Get-Content '$project/Temp/FloorTestRemote.out.txt' | Select-Object -Last $#" | run_remote_powershell
        exit 0
    fi
done
echo "No reply from player $player: is it in Play mode?" >&2
exit 1
