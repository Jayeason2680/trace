# ============================================================================
# Zone Console — Watchdog (Session 3, review-hardened)
# Restarts the cTrader desktop app if its process has died, and logs actions.
# Runs from Windows Task Scheduler every few minutes, in the LOGGED-ON
# interactive session (see the Session 3 doc — a non-interactive task cannot
# put cTrader's window on your desktop).
#
# WHAT IT DOES / DOES NOT DO
#   - Restarts the cTrader *process* if it is not running.
#   - It CANNOT start a cBot inside cTrader; enable cTrader's "auto-run"
#     setting so ZoneExec/ShadowLogger resume on launch (see the doc).
#   - The healthchecks.io heartbeat is the backstop for a frozen-but-alive
#     cTrader (process up, cBot not pinging) — this script won't catch that.
# ============================================================================

# --- EDIT THIS ONE LINE ---
# Full path to the cTrader executable. Find it: right-click the desktop shortcut
# -> Open file location -> right-click the app -> Properties -> copy "Target".
$CTraderExe = "$env:LOCALAPPDATA\Spotware\ctrader\ctrader.exe"
# --------------------------

# Process name is DERIVED from the exe (no separate hand-typed value to get
# wrong — a mismatch would relaunch forever). Review fix #3.
$ProcessName = [System.IO.Path]::GetFileNameWithoutExtension($CTraderExe)

$LogDir = Join-Path $env:USERPROFILE "Documents\ZoneConsole\logs"
New-Item -ItemType Directory -Force -Path $LogDir | Out-Null
$LogFile = Join-Path $LogDir "watchdog.log"
function Write-Log($msg) {
    Add-Content -Path $LogFile -Value ("{0}  {1}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $msg)
}

# Single-instance guard: if a previous watchdog run is still launching cTrader,
# a new scheduled tick must NOT fire a second Start-Process (review fix #2 —
# double-launch could stack two ZoneExec instances). Global mutex, non-blocking.
$mutex = New-Object System.Threading.Mutex($false, "Global\ZoneConsoleWatchdog")
if (-not $mutex.WaitOne(0)) { exit 0 }

try {
    if (Get-Process -Name $ProcessName -ErrorAction SilentlyContinue) { exit 0 } # running: quiet

    if (-not (Test-Path $CTraderExe)) {
        Write-Log "ERROR: cTrader exe not found at '$CTraderExe' - fix the path in watchdog.ps1"
        exit 1
    }

    Write-Log "cTrader not running -> starting it"
    Start-Process -FilePath $CTraderExe

    # Give it time to appear, re-checking; a slow cold start must NOT be treated
    # as failure (that would trigger a scheduler retry and a second launch).
    for ($i = 0; $i -lt 12; $i++) {
        Start-Sleep -Seconds 5
        $p = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue
        if ($p) { Write-Log "restarted: $ProcessName up (pid $($p.Id)). Confirm cBots auto-started; watch the heartbeat."; exit 0 }
    }
    # Still not visible after ~60s: log once and exit 0 (NOT 1) so the next
    # 5-min tick re-evaluates cleanly instead of a rapid retry storm.
    Write-Log "NOTE: launched but $ProcessName not visible after 60s; next tick will re-check"
    exit 0
}
catch {
    Write-Log ("EXCEPTION: " + $_.Exception.Message)
    exit 0
}
finally {
    $mutex.ReleaseMutex(); $mutex.Dispose()
}
