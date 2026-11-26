#!/usr/bin/env bash
# Opens the project in Unity on the home PC's desktop (RTX 3060), from this laptop, then waits until
# the Editor is ready for `unity command`.
#
# Why a scheduled task: anything started over SSH runs in a hidden background session, so no window
# appears on the desktop. A task set to "run only when the user is logged on" runs in the owner's own
# desktop session instead. The script creates that task, starts it once and deletes it straight away,
# so nothing is left behind on the PC.
#
# Usage, from the project root in Git Bash:  bash tools/homepc/open-unity.sh [--no-wait]
# Needs: the "homepc" host in ~/.ssh/config, and the owner signed in on the PC (locked is fine).
# See docs/PROJECT_CONTEXT.md, section 11.
set -euo pipefail

WAIT=1
[[ "${1:-}" == "--no-wait" ]] && WAIT=0

cd "$(git rev-parse --show-toplevel)"
source tools/homepc/common.sh
REMOTE_DIR='C:\OneMoreFloor'
version="$(sed -n 's/^m_EditorVersion: *//p' ProjectSettings/ProjectVersion.txt | tr -d '\r')"
UNITY_EXE="${HOMEPC_UNITY:-C:\\Program Files\\Unity\\Hub\\Editor\\$version\\Editor\\Unity.exe}"

ssh_quiet() { ssh -o BatchMode=yes -o ConnectTimeout=8 "$HOST" "$@" 2>/dev/null | tr -d '\r'; }

ensure_homepc_reachable

# The PowerShell part runs on the PC. Sent base64-encoded because multi-line scripts piped over
# stdin break in PowerShell.
read -r -d '' ps <<EOF || true
\$ErrorActionPreference = 'Stop'
\$ProgressPreference = 'SilentlyContinue'
\$exe = '$UNITY_EXE'
\$project = '$REMOTE_DIR'
if (-not (Test-Path -LiteralPath \$exe)) { 'NO_EDITOR ' + \$exe; exit 3 }
\$desktop = Get-CimInstance Win32_Process -Filter "Name='explorer.exe'" | Where-Object { \$_.SessionId -ne 0 }
if (-not \$desktop) { 'NOT_SIGNED_IN'; exit 4 }
\$open = Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | Where-Object { \$_.CommandLine -like ('*' + \$project + '*') }
if (\$open) { 'ALREADY_OPEN'; exit 0 }
\$name = 'OneMoreFloor-OpenUnity-Once'
\$action = New-ScheduledTaskAction -Execute \$exe -Argument ('-projectPath "' + \$project + '"')
\$principal = New-ScheduledTaskPrincipal -UserId ([Security.Principal.WindowsIdentity]::GetCurrent().Name) -LogonType Interactive
try {
    Register-ScheduledTask -TaskName \$name -Action \$action -Principal \$principal -Force | Out-Null
    Start-ScheduledTask -TaskName \$name
    Start-Sleep -Seconds 3
} finally {
    Unregister-ScheduledTask -TaskName \$name -Confirm:\$false -ErrorAction SilentlyContinue
}
'STARTED'
EOF
encoded="$(printf '%s' "$ps" | iconv -f UTF-8 -t UTF-16LE | base64 -w0)"
result="$(ssh_quiet "powershell -NoProfile -ExecutionPolicy Bypass -EncodedCommand $encoded" || true)"

case "$result" in
    *ALREADY_OPEN*) echo "Unity is already open on the home PC." ;;
    *STARTED*)      echo "Unity is starting on the home PC's desktop." ;;
    *NOT_SIGNED_IN*) echo "Nobody is signed in on the home PC. Sign in there once (Parsec or at the PC), then run this again." >&2; exit 1 ;;
    *NO_EDITOR*)    echo "Unity $version isn't installed on the home PC: ${result#NO_EDITOR }" >&2; exit 1 ;;
    *)              echo "Couldn't start Unity on the home PC. Output: $result" >&2; exit 1 ;;
esac

[[ $WAIT -eq 0 ]] && exit 0

# Wait until the Editor answers `unity command` (up to 10 minutes: a big re-import can take a while).
for _ in $(seq 1 60); do
    if ssh_quiet "cd /d $REMOTE_DIR && unity status --no-banner --format json" | grep -q '"ready"'; then
        echo "Unity is ready on the home PC."
        exit 0
    fi
    sleep 10
done
echo "Unity was started but wasn't ready after 10 minutes. It may still be importing; check with: ssh homepc \"cd /d $REMOTE_DIR && unity status\"" >&2
exit 1
