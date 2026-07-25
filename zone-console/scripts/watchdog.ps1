# ============================================================================
# Zone Console — Watchdog (Session 3)
# Restarts the cTrader desktop app if its process has died, and logs every
# action. Runs from Windows Task Scheduler every few minutes.
#
# WHAT IT DOES / DOES NOT DO
#   - It restarts the cTrader *process* if it is not running.
#   - It CANNOT start a cBot inside cTrader. For automations to resume after a
#     restart you MUST enable cTrader's "automatically run" / start-on-launch
#     setting for the ZoneExec + ShadowLogger instances (see the Session 3 doc).
#     The healthchecks.io heartbeat is the backstop: if the cBot is not pinging,
#     you get an email even though the process is up.
#
# SETUP: edit the two variables below for your machine, then schedule it.
# ============================================================================

# --- EDIT THESE ---
# Full path to the cTrader executable. Find it: right-click the desktop shortcut
# -> Open file location -> right-click the app -> Properties -> copy "Target".
# FTMO's build is usually under %LOCALAPPDATA%\Spotware\... or Program Files.
$CTraderExe  = "$env:LOCALAPPDATA\Spotware\ctrader\ctrader.exe"
# The process name WITHOUT .exe. Verify in Task Manager -> Details (e.g. "ctrader").
$ProcessName = "ctrader"
# ------------------

$LogDir = Join-Path $env:USERPROFILE "Documents\ZoneConsole\logs"
New-Item -ItemType Directory -Force -Path $LogDir | Out-Null
$LogFile = Join-Path $LogDir "watchdog.log"

function Write-Log($msg) {
    $line = "{0}  {1}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $msg
    Add-Content -Path $LogFile -Value $line
}

try {
    $proc = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue
    if ($proc) {
        # Running. Keep the log quiet; uncomment for a heartbeat line.
        # Write-Log "ok: $ProcessName running (pid $($proc.Id))"
        exit 0
    }

    if (-not (Test-Path $CTraderExe)) {
        Write-Log "ERROR: cTrader exe not found at '$CTraderExe' - fix the path in watchdog.ps1"
        exit 1
    }

    Write-Log "cTrader not running -> starting it"
    Start-Process -FilePath $CTraderExe
    Start-Sleep -Seconds 20

    $proc = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue
    if ($proc) {
        Write-Log "restarted: $ProcessName up (pid $($proc.Id)). Confirm cBots auto-started; watch the heartbeat."
        exit 0
    } else {
        Write-Log "WARNING: launch attempted but $ProcessName still not visible after 20s"
        exit 1
    }
}
catch {
    Write-Log ("EXCEPTION: " + $_.Exception.Message)
    exit 1
}
