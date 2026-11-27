#!/usr/bin/env bash
# Sends the project from this laptop to the home PC, where the owner playtests on the RTX 3060 and art is built.
# One-way: the laptop is the source of truth. Only files that changed are sent, and files deleted
# here are deleted there. Unity's Library folder on the home PC is left alone.
# Exception: a .meta file that Unity on the home PC created for an asset this laptop also has is copied
# back here instead of deleted, so the asset keeps its ID on both machines.
# Anything else built on the home PC must be fetched with tools/homepc/pull.sh before the next sync.
# As a safety net, files made on the home PC since the last sync are kept (and listed), not deleted;
# --discard-pc-changes deletes them too.
#
# Usage, from the project root in Git Bash:  tools/homepc/sync.sh [--discard-pc-changes]
# Needs the "homepc" host in ~/.ssh/config (see docs/PROJECT_CONTEXT.md, section 11).
set -euo pipefail

cd "$(git rev-parse --show-toplevel)"
source tools/homepc/common.sh
SYNC_DIRS=(Assets Packages ProjectSettings docs tools art-source)
SYNC_MARKER='C:\OneMoreFloorTools\last-sync'
discard=0
if [ "${1:-}" = "--discard-pc-changes" ]; then discard=1; fi

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

# When the last sync finished, by the home PC's clock (0 if it has never synced with this script).
last_sync="$(echo "\$f = '$SYNC_MARKER'; if (Test-Path \$f) { [math]::Floor(((Get-Item \$f).LastWriteTimeUtc - [datetime]'1970-01-01').TotalSeconds) } else { 0 }" | run_remote_powershell | tr -dc '0-9')"
last_sync="${last_sync:-0}"

# Unity-made .meta files for assets that exist here come back. Other files only there are deleted, unless
# they were made on the PC since the last sync (Unity or Blender output not pulled back yet): those are kept
# and listed, so they aren't lost. Pull them back with tools/homepc/pull.sh, or delete them with
# --discard-pc-changes.
: > "$work/fetch.txt"; : > "$work/delete.txt"; : > "$work/kept.txt"
while IFS= read -r path; do
    if [[ "$path" == *.meta ]] && grep -qxF "${path%.meta}" "$work/local-paths.txt"; then
        echo "$path" >> "$work/fetch.txt"
        continue
    fi
    modified="$(awk -F'\t' -v p="$path" '$1 == p { print $3; exit }' "$work/remote.txt")"
    if [ "$discard" -eq 0 ] && [ "$last_sync" -gt 0 ] && [ "${modified:-0}" -gt "$last_sync" ]; then
        echo "$path" >> "$work/kept.txt"
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

echo "Set-Content -LiteralPath '$SYNC_MARKER' -Value (Get-Date -Format o)" | run_remote_powershell > /dev/null

echo "Sent $send_count file(s), removed $delete_count file(s) on $HOST:$REMOTE_DIR; copied back $fetch_count .meta file(s) Unity made there."
kept_count=$(wc -l < "$work/kept.txt")
if [ "$kept_count" -gt 0 ]; then
    echo "Kept $kept_count file(s) made on the home PC since the last sync, which this laptop doesn't have yet:"
    sed 's/^/  /' "$work/kept.txt" | head -20
    [ "$kept_count" -gt 20 ] && echo "  ... and $((kept_count - 20)) more"
    echo "Bring them here with tools/homepc/pull.sh <paths>, or delete them there with tools/homepc/sync.sh --discard-pc-changes."
fi
