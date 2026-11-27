#!/usr/bin/env bash
# Copies what the home PC built (art from Blender, scenes and lightmaps from Unity) back to this laptop,
# the project's master copy. Copies files that are new on the PC, or differ and are newer there.
# Run it after building on the PC and before the next tools/homepc/sync.sh, which would otherwise delete them.
#
# Usage, from the project root in Git Bash:
#   tools/homepc/pull.sh [--mirror] <path>...      paths relative to the project root, files or folders
#   --mirror  also deletes files under those paths here that no longer exist on the PC (for fully generated folders)
set -euo pipefail

cd "$(git rev-parse --show-toplevel)"
source tools/homepc/common.sh

mirror=0
if [ "${1:-}" = "--mirror" ]; then mirror=1; shift; fi
[ $# -gt 0 ] || { echo "Usage: tools/homepc/pull.sh [--mirror] <path>..." >&2; exit 2; }

work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT
ensure_homepc_reachable

list_remote_files "$@" | LC_ALL=C sort > "$work/remote.txt"
list_local_files "$@" | LC_ALL=C sort > "$work/local.txt"

# New on the PC, or a different size, or modified later there.
# (FILENAME, not NR==FNR, tells the two lists apart: NR==FNR misreads the PC's list when nothing exists here yet.)
awk -F'\t' 'FILENAME == ARGV[1] { size[$1]=$2; time[$1]=$3; next }
            !($1 in size) || size[$1]!=$2 || $3>time[$1] { print $1 }' "$work/local.txt" "$work/remote.txt" > "$work/fetch.txt"
fetch_from_homepc "$work/fetch.txt"
fetched=$(wc -l < "$work/fetch.txt")

removed=0
if [ $mirror -eq 1 ]; then
    cut -f1 "$work/remote.txt" | LC_ALL=C sort > "$work/remote-paths.txt"
    cut -f1 "$work/local.txt" | LC_ALL=C sort > "$work/local-paths.txt"
    while IFS= read -r path; do
        rm -f -- "$path"; removed=$((removed + 1))
    done < <(LC_ALL=C comm -23 "$work/local-paths.txt" "$work/remote-paths.txt")
fi

echo "Copied $fetched file(s) from $HOST:$REMOTE_DIR; removed $removed file(s) here that the PC no longer has."
