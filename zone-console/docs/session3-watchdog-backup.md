# Session 3 — Watchdog & Backup setup

Two small guardians for the demo soak. Do this once, after ZoneExec is running.
Both are PowerShell scripts in `zone-console/scripts/`, driven by Windows Task Scheduler.

## Part 1 — Make cBots resume after a restart (the important bit)

A watchdog can restart *cTrader*, but only cTrader can restart a *cBot*. So first:

1. In cTrader, open your running ZoneExec (and ShadowLogger) instance.
2. Enable the **"automatically run"** / start-on-launch option so the instance
   re-starts when cTrader launches. (Exact label varies by build; look on the
   Algo instance's settings/overflow menu.)
3. If your cTrader build offers **"start in external process"** for cBots
   (desktop 4.8+), prefer it — the bot then survives a platform restart.
4. Reality check (this is why the heartbeat exists): some builds still bring
   cBots back **stopped**. The healthchecks.io email is your backstop — if the
   heartbeat goes silent for 5 min you get pinged and RDP in.

## Part 2 — Install the watchdog (restarts cTrader if it dies)

1. Edit `scripts\watchdog.ps1`: set `$CTraderExe` (the exe path — right-click the
   desktop shortcut → Open file location → Properties → Target) and `$ProcessName`
   (Task Manager → Details, the name without `.exe`).
2. Task Scheduler → **Create Task** (not Basic):
   - General: name `ZoneConsole Watchdog`; **Run whether user is logged on or not**;
     **Run with highest privileges**.
   - Triggers: **At startup**, and **Repeat every 5 minutes** indefinitely.
   - Actions: Start a program →
     Program: `powershell.exe`
     Arguments: `-NoProfile -ExecutionPolicy Bypass -File "%USERPROFILE%\Documents\ZoneConsole\scripts\watchdog.ps1"`
     *(copy the scripts folder onto the VPS under Documents\ZoneConsole\, or point to wherever you cloned the repo)*
   - Settings: allow "run on demand"; if it fails, restart every 1 min up to 3×.
3. Test: end the cTrader process in Task Manager → within ~5 min the watchdog
   relaunches it → check `Documents\ZoneConsole\logs\watchdog.log`.
   **This is your "forced restart" soak-gate item — watch the ZoneExec journal show
   `zone_adopted` (not a second order) afterwards.**

## Part 3 — Install the nightly backup

1. Task Scheduler → Create Task → name `ZoneConsole Backup`;
   Trigger: **Daily 03:00**; Action: `powershell.exe`
   `-NoProfile -ExecutionPolicy Bypass -File "%USERPROFILE%\Documents\ZoneConsole\scripts\backup.ps1"`.
2. Test: right-click the task → **Run** → check `Documents\ZoneConsole\backups\`
   for a dated `.zip` and `logs\backup.log`.
3. **Restore drill (soak-gate item):** copy a backup zip to a temp folder, unzip,
   confirm the journal + `.state` files are intact. You should be able to rebuild
   in under 30 minutes.

## Before real money (not needed for demo)
- Copy the newest backup **off the VPS** automatically (Google Drive Desktop into
  a synced folder, or a scheduled copy to your MacBook). The ops audit flagged that
  same-VPS backups die with the VPS.
- Revisit an object-locked / versioned cloud target so a wipe can't erase history.
