#!/usr/bin/env bash
# Runs a C# snippet in the Unity Editor that is open on the home PC and prints the result.
# The snippet is a method body: statements ending in `return <value>;`. Use full type names.
# Unity must be open there (tools/homepc/open-unity.sh). Doesn't sync: run tools/homepc/sync.sh first
# if the snippet needs the laptop's latest code, then recompile (tools/homepc/unity-eval.sh --recompile).
#
# Usage, from the project root in Git Bash:
#   tools/homepc/unity-eval.sh <snippet.cs> [timeout seconds, default 600]
#   tools/homepc/unity-eval.sh --recompile          (compiles and waits; fails on compile errors)
#   UNITY_INSTANCE="Player 2" tools/homepc/unity-eval.sh <snippet.cs>   (a virtual player in a Play Mode scenario)
set -euo pipefail

cd "$(git rev-parse --show-toplevel)"
source tools/homepc/common.sh
# UNITY_INSTANCE picks the Editor: "Main Editor" (default) or a virtual player such as "Player 2".
UNITY_CMD="unity command --project-path C:/OneMoreFloor --instance \"${UNITY_INSTANCE:-Main Editor}\" --no-banner --caller plugin --skill unity-cli --result-only"
remote_unity() { ssh -o BatchMode=yes "$HOST" "$UNITY_CMD $*" 2>/dev/null | tr -d '\r' || true; }

if [[ "${1:-}" == "--recompile" ]]; then
    remote_unity script recompile > /dev/null
    for _ in $(seq 1 90); do
        status="$(remote_unity script recompile_status | tr -d '\n ')"
        echo "$status" | grep -qE '"(completed|up_to_date|idle)"' && break
        sleep 4
    done
    echo "$status" | head -c 4000; echo
    if echo "$status" | grep -q '"compilationFailed":true'; then exit 1; fi
    exit 0
fi

snippet="$1"
timeout="${2:-600}"
remote="C:/OneMoreFloorTools/eval-$(basename "$snippet")"
scp -q -o BatchMode=yes "$snippet" "$HOST:$remote"
remote_unity script eval --file "$remote" --timeout "$timeout"
