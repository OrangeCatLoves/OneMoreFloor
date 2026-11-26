#!/usr/bin/env bash
# Rebuilds the Trivia room on the home PC and brings the results back to this laptop:
#   1. sends the laptop's changes, 2. builds the room in Blender (tools/blender/build_trivia.py),
#   3. copies the model and lighting sheet back, 4. rebuilds and bakes the Unity review scene in the
#   open Editor there (TriviaReviewSceneBuilder.Build), 5. copies the scene, lightmaps and materials back.
# Unity must be open on the home PC (tools/homepc/open-unity.sh) and not in Play mode.
#
# Usage, from the project root in Git Bash:  tools/homepc/build-trivia.sh [build_trivia.py arguments...]
# Example: tools/homepc/build-trivia.sh --iteration 26
set -euo pipefail

cd "$(git rev-parse --show-toplevel)"
source tools/homepc/common.sh
ART=Assets/_OneMoreFloor/Art/Floors/Trivia
UNITY_CMD='unity command --project-path C:/OneMoreFloor --instance "Main Editor" --no-banner --caller plugin --skill unity-cli --result-only'

remote_unity() { ssh -o BatchMode=yes "$HOST" "$UNITY_CMD $*" 2>/dev/null | tr -d '\r'; }

echo "== Blender build on the home PC"
bash tools/homepc/blender.sh tools/blender/build_trivia.py "$@" | grep -vE "Cannot register a valid material index" | tail -6
bash tools/homepc/pull.sh "$ART/SM_Trivia_Theatre.fbx" "$ART/Textures" art-source/trivia

echo "== Unity: compile, then build and bake the review scene"
remote_unity editor.playmode stop --timeout 60 > /dev/null || true
remote_unity script recompile > /dev/null
for _ in $(seq 1 60); do
    status="$(remote_unity script recompile_status | tr -d '\n ')"
    echo "$status" | grep -qE '"(completed|up_to_date|idle)"' && break
    sleep 5
done
if echo "$status" | grep -q '"compilationFailed":true'; then echo "Unity reports compile errors: $status" >&2; exit 1; fi
printf '%s\n' 'var start = System.DateTime.UtcNow;' \
    'OneMoreFloor.ArtReview.TriviaReviewSceneBuilder.Build();' \
    'return "built in " + (System.DateTime.UtcNow - start).TotalSeconds.ToString("F0") + " s";' > "${TMPDIR:-/tmp}/build-trivia-scene.cs"
scp -q -o BatchMode=yes "${TMPDIR:-/tmp}/build-trivia-scene.cs" "$HOST:C:/OneMoreFloorTools/build-trivia-scene.cs"
result="$(remote_unity script eval --file C:/OneMoreFloorTools/build-trivia-scene.cs --timeout 2400)"
echo "$result" | grep -oE '"result": *"[^"]*"|"message": *"[^"]{0,300}' || true
echo "$result" | grep -q '"success": *true' || { echo "The Unity scene build failed." >&2; exit 1; }

echo "== Bringing the review scene, lightmaps and materials back"
bash tools/homepc/pull.sh --mirror "$ART/Review"
bash tools/homepc/pull.sh "$ART/Materials" art-source/trivia
