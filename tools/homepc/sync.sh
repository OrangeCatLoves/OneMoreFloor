#!/usr/bin/env bash
# Sends the project from this laptop to the home PC, where the owner playtests on the RTX 3060 and art is built.
# One-way: the laptop is the source of truth. Only files that changed are sent, and files deleted
# here are deleted there. Unity's Library folder on the home PC is left alone.
# Exception: a .meta file that Unity on the home PC created for an asset this laptop also has is copied
# back here instead of deleted, so the asset keeps its ID on both machines.
# Anything else built on the home PC must be fetched with tools/homepc/pull.sh before the next sync.
#
# Usage, from the project root in Git Bash:  tools/homepc/sync.sh
# Needs the "homepc" host in ~/.ssh/config (see docs/PROJECT_CONTEXT.md, section 11).
set -euo pipefail

cd "$(git rev-parse --show-toplevel)"
source tools/homepc/common.sh
SYNC_DIRS=(Assets Packages ProjectSettings docs tools art-source)

work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT

# Without this check, an unreachable home PC would look like "every file is new".
ensure_homepc_reachable

list_local_files "${SYNC_DIRS[@]}" | LC_ALL=C sort > "$work/local.txt"
list_remote_files "${SYNC_DIRS[@]}" | LC_ALL=C sort > "$work/remote.txt"

# Files that are new or different here, and files that exist only on the home PC.
LC_ALL=C comm -23 "$work/local.txt" "$work/remote.txt" | cut -f1 > "$work/send.txt"
cut -f1 "$work/local.txt" | LC_ALL=C sort > "$work/local-paths.txt"
cut -f1 "$work/remote.txt" | LC_ALL=C sort > "$work/remote-paths.txt"
LC_ALL=C comm -13 "$work/local-paths.txt" "$work/remote-paths.txt" > "$work/only-there.txt"

# Unity-made .meta files for assets that exist here come back; everything else only there is deleted.
: > "$work/fetch.txt"; : > "$work/delete.txt"
while IFS= read -r path; do
    if [[ "$path" == *.meta ]] && grep -qxF "${path%.meta}" "$work/local-paths.txt"; then
        echo "$path" >> "$work/fetch.txt"
    else
        echo "$path" >> "$work/delete.txt"
    fi
done < "$work/only-there.txt"

send_count=$(wc -l < "$work/send.txt")
delete_count=$(wc -l < "$work/delete.txt")
fetch_count=$(wc -l < "$work/fetch.txt")

fetch_from_homepc "$work/fetch.txt"

if [ "$send_count" -gt 0 ]; then
    tar -cf - --files-from="$work/send.txt" | ssh -o BatchMode=yes -o Compression=no "$HOST" "tar -xf - -C $REMOTE_DIR" 2>/dev/null
fi

if [ "$delete_count" -gt 0 ]; then
    {
        echo "\$root = '$REMOTE_DIR'"
        while IFS= read -r path; do
            echo "Remove-Item -LiteralPath (Join-Path \$root '${path//\'/\'\'}') -Force -ErrorAction SilentlyContinue"
        done < "$work/delete.txt"
    } | run_remote_powershell > /dev/null
fi

echo "Sent $send_count file(s), removed $delete_count file(s) on $HOST:$REMOTE_DIR; copied back $fetch_count .meta file(s) Unity made there."
