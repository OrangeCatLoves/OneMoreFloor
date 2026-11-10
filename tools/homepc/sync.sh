#!/usr/bin/env bash
# Sends the Unity project from this laptop to the home PC, where the owner playtests on the RTX 3060.
# One-way: the laptop is the source of truth. Only files that changed are sent, and files deleted
# here are deleted there. Unity's Library folder on the home PC is left alone.
#
# Usage, from the project root in Git Bash:  tools/homepc/sync.sh
# Needs the "homepc" host in ~/.ssh/config (see docs/PROJECT_CONTEXT.md, section 11).
set -euo pipefail

HOST="${HOMEPC_HOST:-homepc}"
REMOTE_DIR="${HOMEPC_DIR:-C:/OneMoreFloor}"
SYNC_DIRS=(Assets Packages ProjectSettings docs)

cd "$(git rev-parse --show-toplevel)"
work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT

run_remote_powershell() {
    ssh -o BatchMode=yes "$HOST" "powershell -NoProfile -ExecutionPolicy Bypass -Command -" 2>/dev/null | tr -d '\r'
}

# One line per file: path <tab> size <tab> modified time in whole seconds.
find "${SYNC_DIRS[@]}" -type f -printf '%p\t%s\t%T@\n' | sed -E 's/\.[0-9]+$//' | LC_ALL=C sort > "$work/local.txt"

# PowerShell reads this from standard input one line at a time, so each statement stays on one line.
run_remote_powershell > "$work/remote.txt.unsorted" <<EOF
\$root = '$REMOTE_DIR'
New-Item -ItemType Directory -Force \$root | Out-Null
\$epoch = [datetime]'1970-01-01'
'${SYNC_DIRS[*]}'.Split(' ') | Where-Object { Test-Path (Join-Path \$root \$_) } | ForEach-Object { Get-ChildItem -LiteralPath (Join-Path \$root \$_) -Recurse -File -Force } | ForEach-Object { \$_.FullName.Substring(\$root.Length + 1).Replace('\\', '/') + "\`t" + \$_.Length + "\`t" + [math]::Floor((\$_.LastWriteTimeUtc - \$epoch).TotalSeconds) }
EOF
LC_ALL=C sort "$work/remote.txt.unsorted" > "$work/remote.txt"

# Files that are new or different here, and files that exist only on the home PC.
LC_ALL=C comm -23 "$work/local.txt" "$work/remote.txt" | cut -f1 > "$work/send.txt"
cut -f1 "$work/local.txt" | LC_ALL=C sort > "$work/local-paths.txt"
cut -f1 "$work/remote.txt" | LC_ALL=C sort > "$work/remote-paths.txt"
LC_ALL=C comm -13 "$work/local-paths.txt" "$work/remote-paths.txt" > "$work/delete.txt"

send_count=$(wc -l < "$work/send.txt")
delete_count=$(wc -l < "$work/delete.txt")

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

echo "Sent $send_count file(s), removed $delete_count file(s) on $HOST:$REMOTE_DIR"
