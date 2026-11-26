#!/usr/bin/env bash
# Runs a Blender script on the home PC (Blender 5.2.3 LTS, the same version as the laptop), in the
# background with no window. Sends the project first, so the PC builds from the laptop's current files.
# Fetch the results afterwards with tools/homepc/pull.sh.
#
# Usage, from the project root in Git Bash:
#   tools/homepc/blender.sh <script.py relative to the project root> [script arguments...]
# Example: tools/homepc/blender.sh tools/blender/build_trivia.py --iteration 24
# The full Blender log is saved on the PC in C:/OneMoreFloorTools/logs and the last lines are shown here.
set -euo pipefail

cd "$(git rev-parse --show-toplevel)"
source tools/homepc/common.sh
BLENDER='C:/OneMoreFloorTools/blender-5.2.3-windows-x64/blender.exe'

[ $# -gt 0 ] || { echo "Usage: tools/homepc/blender.sh <script.py> [arguments...]" >&2; exit 2; }
script="$1"; shift
log="C:/OneMoreFloorTools/logs/$(basename "$script" .py)-$(date +%Y%m%d-%H%M%S).log"

bash tools/homepc/sync.sh
ssh -o BatchMode=yes "$HOST" "if not exist C:\\OneMoreFloorTools\\logs mkdir C:\\OneMoreFloorTools\\logs" 2>/dev/null
echo "Running $script on the home PC (log: $log)..."
set +e
ssh -o BatchMode=yes "$HOST" "cd /d $REMOTE_DIR && \"$BLENDER\" --background --python-exit-code 1 --python \"$REMOTE_DIR/$script\" -- $* > \"$log\" 2>&1"
status=$?
set -e
ssh -o BatchMode=yes "$HOST" "powershell -NoProfile -Command \"Get-Content -Tail 25 '$log'\"" 2>/dev/null | tr -d '\r' | grep -v -E "$SSH_FILTER" || true
echo "Blender exited with code $status."
exit $status
