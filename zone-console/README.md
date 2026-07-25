# Zone Console

Turning hand-drawn Support & Resistance rectangles in cTrader into reliable, journaled,
risk-capped execution — with the human keeping all the judgment.

Based on the "Executing Discretion" architecture report (23 Jul 2026) plus a four-auditor
independent review (24 Jul 2026, twelve adopted fixes). Full trader's guide and roadmap:
see the PDFs shared in the project chat.

## Status — expedited plan (v2)

| Milestone | Target | Status |
|---|---|---|
| **M0 · Session 1** — docs, journal template, ShadowLogger, ZoneExec core | 24 Jul | ✅ done |
| **M1 · ShadowLogger LIVE** — installed on FTMO cTrader (GER40.cash M15), journal writing, heartbeat green, first zone configured (`RED BUY`) | 24 Jul — 3 days early | ✅ done (hardening checklist still owed) |
| **M2 · Session 2** — ZoneExec MVP for **demo**: T1/T2 entries, broker-side SL/TP + order expiry, caps, FTMO Normal laws (weekend + index-nightly flatten, news pause via news.txt, −3%/−8% Prague-reset breakers), KILL.txt switch | 24 Jul (ahead of ~28 Jul) | ✅ built & audited — 4-agent audit (27 fixes) + verification (7 regressions fixed) + final sign-off (7/7 confirmed, demo-safe). Ready to install. |
| M3 · Session 3 — fixes from first live events + drills (kill-test, restore) | ~1–2 Aug | ⬜ |
| M4 · Demo soak, event-based checklist (≥20 events, 2 weekends, 1 red-news week, 1 forced restart, 1 update cycle, zero unhandled errors) | 28 Jul – ~15 Aug | ⬜ |
| **M5 · GO-LIVE at minimum size** | **~17 Aug** (prudent fallback: 31 Aug) | ⬜ |
| M6 · Size-up gate: ≥100 journaled zones, positive expectancy net of costs | ~Nov (zone-rate dependent) | ⬜ |
| M7 · Stage 2 — Zone Manager panel (click-form, kill switch UI) | after M5 earned | ⬜ |

## Layout

```
docs/     grammar-spec-v1.md · install-shadowlogger.md · vps-hardening-checklist.md · panic-card.md
journal/  zone-journal-template.csv       ← start filling this from day 1
src/ShadowLogger/ShadowLogger.cs          ← Session 1: watch-only cBot (no order code)
src/ZoneExec/ZoneRecord.cs                ← comment-grammar parser (pure C#)
src/ZoneExec/ZoneValidator.cs             ← ATR sanity checks (pure C#)
src/ZoneExec/RiskEngine.cs                ← sizing + caps + breakers (pure C#)
```

The ZoneExec core is deliberately platform-free (no cAlgo references) so its logic is
testable outside cTrader; Session 2 wraps it in the trading cBot and adds: order
placement with broker-side expiry, touch counting, news-window pause, Telegram
commands (/status, /killswitch, /ack), watchdog + nightly backup.

## Safety posture (v1)

- ShadowLogger contains no order code — it physically cannot trade.
- Empty rectangle comment = silent draft; nothing arms without an explicit instruction.
- Risk caps: RED ≤1.0% / YEL ≤0.5% per trade · max 3 positions · ≤3% total open risk ·
  ≤2% per currency/factor (both legs counted) · −3% daily cut-out (freeze) ·
  −10% account breaker (disarm + written review) · WKD:HOLD sized for 2× stop (gaps).
- Demo account until every gate on the roadmap is passed. Live starts at minimum size.
