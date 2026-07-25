# Demo Soak Tracker (M3 → M4 → M5)

The soak exits by **checklist, not calendar** — we go live only when the system has
cleanly survived the events that break trading bots. Tick these as they happen.
Target window: ~28 Jul – mid-Aug. If an item hasn't occurred by then, we wait for it.

## Gate G2 — first-week checklist (M3, "does it do what I meant?")

- [ ] `exec_start` shows `account: "demo"`, zone-exec heartbeat green
- [ ] One T1 zone: `zone_armed` → resting order visible in cTrader with SL/TP + expiry
- [ ] One `zone_filled` → `position_closed` (TP or SL), correct in the journal
- [ ] News drill: fake near-future line in `news.txt` → `news_pause_start` then `news_pause_end`
- [ ] Kill drill: create `KILL.txt` → `flatten_all`; delete + re-save a comment → re-arms
- [ ] ~20 total events that match what you intended
- [ ] **G2 passed →** begin M4 soak

## Gate G4 — demo soak checklist (M4, "does it survive the real world?")

- [ ] **≥20 zone events** across the soak (arms, fills, closes)
- [ ] **2 weekend rollovers** survived — `session_flatten` each Friday, clean Monday re-arm
- [ ] **1 red-news week** — NFP/FOMC/CPI window paused correctly (`news_pause_*`)
- [ ] **1 forced restart** — stop/start cTrader mid-session; bot adopts orders, no double-place
- [ ] **1 Windows/cTrader update cycle** survived (watchdog behaviour — Session 3 item)
- [ ] **1 index nightly-break** handled — `session_flatten` at 20:40 UTC on GER40, no overnight hold
- [ ] **Zero unhandled errors** the whole time (no `timer_error`, no `MANUAL_INTERVENTION`, no `close_failed`)
- [ ] Journal spot-checks: every fill's realized R and costs look sane
- [ ] **G4 passed →** M5 go-live at minimum size

## Gate G5 — evidence gate (blocks sizing up, not going live)

- [ ] **≥100 journaled zones** (ShadowLogger + ZoneExec combined)
- [ ] Positive expectancy **net of spread, commission and swap**
- [ ] **G5 passed →** raise risk gradually

## Running notes
_(jot dates/observations here as you go — this becomes the story we review each Sunday)_

- 24 Jul — ShadowLogger live on GER40.cash M15; first zone `RED BUY` logged.
- …
