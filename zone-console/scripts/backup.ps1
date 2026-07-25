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

try {
    $stamp   = Get-Date -Format "yyyyMMdd-HHmm"
    $zipPath = Join-Path $BackupDir "ZoneConsole-$stamp.zip"

    # Back up the journal folder (events + .state + .digestdate) and news.txt.
    # Deliberately exclude KILL.txt and the backups folder itself.
    $toBackup = @()
    $journal = Join-Path $Root "journal"
    if (Test-Path $journal)               { $toBackup += $journal }
    $news = Join-Path $Root "news.txt"
    if (Test-Path $news)                  { $toBackup += $news }

    if ($toBackup.Count -eq 0) {
        Write-Log "nothing to back up yet (no journal/news) - skipping"
        exit 0
    }

    Compress-Archive -Path $toBackup -DestinationPath $zipPath -Force
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
