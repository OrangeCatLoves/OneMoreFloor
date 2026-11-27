#!/usr/bin/env bash
# Shared helpers for the home PC scripts. Source it; don't run it.
# The home PC is reached over SSH as the host "homepc" in ~/.ssh/config (docs/PROJECT_CONTEXT.md, section 11).

HOST="${HOMEPC_HOST:-homepc}"
REMOTE_DIR="${HOMEPC_DIR:-C:/OneMoreFloor}"
PC_NAME="${HOMEPC_NAME:-WeiBin}"
SSH_FILTER='store now|openssh.com/pq|post-quantum|Permanently added'

run_remote_powershell() {
    ssh -o BatchMode=yes "$HOST" "powershell -NoProfile -ExecutionPolicy Bypass -Command -" 2>/dev/null | tr -d '\r'
}

# The router sometimes gives the PC a new address. If the configured one stops answering, look the PC up
# by name (Windows name lookup works from PowerShell even when SSH's own lookup doesn't), update
# ~/.ssh/config and try again.
ensure_homepc_reachable() {
    if ssh -o BatchMode=yes -o ConnectTimeout=8 "$HOST" "exit 0" 2>/dev/null; then
        return 0
    fi
    local found
    found="$(powershell.exe -NoProfile -Command "try { (Resolve-DnsName '$PC_NAME' -ErrorAction Stop | Where-Object { \$_.IPAddress -like '*.*' } | Select-Object -First 1).IPAddress } catch { '' }" 2>/dev/null | tr -d '\r ')"
    if [ -n "$found" ] && [ -f "$HOME/.ssh/config" ]; then
        # Replace the HostName line inside the "Host homepc" block only.
        sed -i "/^Host $HOST\$/,/^Host /{s/^\([[:space:]]*HostName[[:space:]]\+\).*/\1$found/}" "$HOME/.ssh/config"
        if ssh -o BatchMode=yes -o ConnectTimeout=8 "$HOST" "exit 0" 2>/dev/null; then
            echo "The home PC moved to $found; updated ~/.ssh/config." >&2
            return 0
        fi
    fi
    echo "Can't reach the home PC ($HOST). It must be switched on, and the laptop must be on the home network." >&2
    return 1
}

# One line per file under the given folders on the home PC: path <tab> size <tab> modified time in whole seconds.
# PowerShell reads its script from standard input one line at a time, so each statement stays on one line.
# Paths may contain spaces (e.g. "Assets/TextMesh Pro"): they are passed to PowerShell separated by '|'.
list_remote_files() {
    local dirs
    dirs="$(printf '%s|' "$@")"
    dirs="${dirs%|}"
    dirs="${dirs//\'/\'\'}"
    run_remote_powershell <<EOF
\$root = '$REMOTE_DIR'
\$epoch = [datetime]'1970-01-01'
'$dirs'.Split('|') | Where-Object { Test-Path (Join-Path \$root \$_) } | ForEach-Object { \$p = Join-Path \$root \$_; if (Test-Path \$p -PathType Leaf) { Get-Item -LiteralPath \$p -Force } else { Get-ChildItem -LiteralPath \$p -Recurse -File -Force } } | ForEach-Object { \$_.FullName.Substring(\$root.Length + 1).Replace('\\', '/') + "\`t" + \$_.Length + "\`t" + [math]::Floor((\$_.LastWriteTimeUtc - \$epoch).TotalSeconds) }
EOF
}

# The same listing for this laptop.
list_local_files() {
    local existing=()
    for d in "$@"; do [ -e "$d" ] && existing+=("$d"); done
    [ ${#existing[@]} -eq 0 ] && return 0
    find "${existing[@]}" -type f -printf '%p\t%s\t%T@\n' | sed -E 's/\.[0-9]+$//'
}

# Copies the listed files (one path per line, relative to the project root) from the home PC to this laptop.
fetch_from_homepc() {
    local list="$1"
    [ -s "$list" ] || return 0
    scp -q -o BatchMode=yes "$list" "$HOST:C:/OneMoreFloorTools/fetch-list.txt" 2>/dev/null
    ssh -o BatchMode=yes -o Compression=no "$HOST" "tar -cf - -C $REMOTE_DIR -T C:/OneMoreFloorTools/fetch-list.txt" 2>/dev/null | tar -xf -
}
