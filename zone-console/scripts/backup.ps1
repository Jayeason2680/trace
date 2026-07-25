# ============================================================================
# Zone Console — Nightly backup (Session 3)
# Zips your journal + state + news file to a dated archive and prunes old ones.
# Runs from Windows Task Scheduler once a day (e.g. 03:00).
#
# NOTE (from the ops audit): this writes to the SAME VPS it is backing up, so a
# dead or wiped VPS takes the backups with it. That is fine for the demo phase.
# BEFORE REAL MONEY: also copy the newest zip off-machine (Google Drive Desktop,
# a synced folder, or your MacBook) — see the Session 3 doc.
# ============================================================================

$Root      = Join-Path $env:USERPROFILE "Documents\ZoneConsole"
$BackupDir = Join-Path $Root "backups"
$LogDir    = Join-Path $Root "logs"
$KeepDays  = 30
New-Item -ItemType Directory -Force -Path $BackupDir | Out-Null
New-Item -ItemType Directory -Force -Path $LogDir | Out-Null
$LogFile = Join-Path $LogDir "backup.log"

function Write-Log($msg) {
    Add-Content -Path $LogFile -Value ("{0}  {1}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $msg)
}

$Stage = Join-Path $env:TEMP ("zc-backup-" + (Get-Date -Format "yyyyMMddHHmmss"))
try {
    $stamp   = Get-Date -Format "yyyyMMdd-HHmm"
    $zipPath = Join-Path $BackupDir "ZoneConsole-$stamp.zip"

    # Copy sources to a staging dir FIRST via robocopy, then zip the copy
    # (review fix #4). robocopy reads files even while ZoneExec is appending, so
    # a 03:00 run can't fail or tear on an open journal handle. Compress-Archive
    # on a still-open file could otherwise throw and skip the night's backup.
    New-Item -ItemType Directory -Force -Path $Stage | Out-Null
    $journal = Join-Path $Root "journal"
    if (Test-Path $journal) {
        # /R:2 /W:1 = 2 retries, 1s apart; robocopy exit codes 0-7 are success.
        robocopy $journal (Join-Path $Stage "journal") /E /R:2 /W:1 /NP /NFL /NDL /NJH /NJS | Out-Null
        if ($LASTEXITCODE -ge 8) { Write-Log "WARNING: robocopy journal returned $LASTEXITCODE" }
    }
    $news = Join-Path $Root "news.txt"
    if (Test-Path $news) { Copy-Item $news -Destination $Stage -Force -ErrorAction SilentlyContinue }

    if (-not (Get-ChildItem -Path $Stage -Recurse -File -ErrorAction SilentlyContinue)) {
        Write-Log "nothing to back up yet (no journal/news) - skipping"
        exit 0
    }

    Compress-Archive -Path (Join-Path $Stage "*") -DestinationPath $zipPath -Force
    $sizeKb = [math]::Round((Get-Item $zipPath).Length / 1KB, 1)
    Write-Log "backup ok -> $zipPath ($sizeKb KB)"

    # Prune archives older than KeepDays.
    $cutoff = (Get-Date).AddDays(-$KeepDays)
    Get-ChildItem -Path $BackupDir -Filter "ZoneConsole-*.zip" |
        Where-Object { $_.LastWriteTime -lt $cutoff } |
        ForEach-Object { Remove-Item $_.FullName -Force; Write-Log "pruned old backup $($_.Name)" }

    exit 0
}
catch {
    Write-Log ("EXCEPTION: " + $_.Exception.Message)
    exit 1
}
finally {
    if (Test-Path $Stage) { Remove-Item $Stage -Recurse -Force -ErrorAction SilentlyContinue }
}
