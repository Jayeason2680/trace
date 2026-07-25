# Install Guide — ZoneExec v1 (Session 2) — DEMO ONLY

ZoneExec is the trading cBot: it turns configured rectangles into resting orders with
broker-side SL/TP and broker-side expiry, under FTMO Normal law. **Run it only on the
demo account until every roadmap gate is passed.**

## Prep (5 min)

1. In `Documents\ZoneConsole\` create **`news.txt`** — one UTC timestamp per line for
   red-flag events you want paused (±15 min). Refresh it each Sunday from the FTMO/
   ForexFactory calendar. Example:
   ```
   # NFP
   2026-08-07 12:30
   # FOMC
   2026-08-19 18:00
   ```
2. Know the kill switch: creating a file named **`KILL.txt`** in `Documents\ZoneConsole\`
   (Notepad → Save As) instantly closes every ZC position, cancels every ZC order, and
   latches arming OFF until you delete the file. This is your no-Telegram emergency stop —
   it's on the panic card.

## Install (10 min)

3. cTrader → **Algo** tab → New cBot → name it `ZoneExec` → replace the template with
   `src/ZoneExec/ZoneExec.cs` → **Build** (green).
4. Add ONE instance per symbol, on the chart you draw that symbol's zones on
   (e.g. your GER40.cash M15). Do not run two ZoneExec instances on the same symbol.
5. Parameters:
   - **Heartbeat ping URL** — create a SECOND check at healthchecks.io named
     `zone-exec` (period 2 min, grace 3 min) and paste its URL. Keep ShadowLogger's
     check separate.
   - **FTMO initial balance** — enter your account's starting balance (e.g. 10000)
     **explicitly**. (0 snapshots the current balance — only correct on a fresh account.)
   - **Daily flatten UTC** — on index charts (GER40, US100…) set `20:40`: FTMO Normal
     forbids holding through the index's ~3-hour nightly break, so those positions
     close before it every day. Leave empty on FX charts.
   - Everything else: defaults are the audited values.
6. **Start.** The journal `Documents\ZoneConsole\journal\<SYMBOL>-<TF>-exec.jsonl`
   gets an `exec_start` line; on a demo account it records `account_is_live: "demo"`.

## How you use it (nothing new to learn)

- Draw a rectangle → double-click → Comment → e.g. `RED BUY` or `YEL SELL R0.3` → OK.
- ZoneExec validates it: **green/red border = armed** (T1 resting order placed, SL/TP
  and expiry at the broker), **amber = T2 waiting for touch + M15 confirmation**,
  **gray = rejected** (reason in the journal).
- The cBot writes an `ID:xxxx` token back into your comment — leave it there, it's the
  zone's identity.
- Editing an armed zone's rectangle or comment **disarms it first**, then re-validates —
  the drawing is the instruction.

## What it will never do (v1.1, post-audit)

Place a marketable limit (price must be on the approach side, outside the zone) ·
re-enter immediately after a close (30-min cooldown + approach-side rule) · arm during
a news window, after Friday's cut, on weekends, or through an index's nightly break ·
exceed 0.5%/1.0% per trade, 3 positions+orders, 3% total, ±2% per factor · keep
anything open after −3% today (Prague reset) or −8% from initial balance — **both now
flatten, not just freeze** (protects FTMO's −5%/−10% floors) · trade while `KILL.txt`
exists · leave a resting order alive when the bot is dead (broker-side expiry, capped
at the next session cut).

Notes on v1.1 behavior: after you delete `KILL.txt`, zones stay disarmed until you
re-save each comment (deliberate). After news windows, cut-out lapses and Mondays,
eligible zones re-arm automatically within ~5 seconds. If the bot was down over
midnight Prague, it enters a safe mode (no new arming that day) because the FTMO
day-anchor can't be trusted — the journal says so.

Two expected quiet periods (not hangs): (1) on an index like GER40, a zone that
becomes eligible after the daily flatten time waits for the next session to open before
arming — that's the >2h-break rule, not a fault; (2) the bot never places an order into
a closed market, so overnight you'll see `market_closed` / `past daily flatten` in the
journal rather than new orders. Both resume automatically at session open.

## First-week checklist (feeds Gate G2)

- [ ] Draw one small T1 zone near price on demo → see `zone_armed` + the resting order
      with SL/TP + expiry in cTrader's Orders tab.
- [ ] Let one fill → `zone_filled`, then TP/SL → `position_closed`.
- [ ] Put a fake timestamp 10 min ahead in news.txt → watch `news_pause_start` cancel
      the resting order, then `news_pause_end` re-arm it.
- [ ] Create KILL.txt → everything flattens; delete it → arming unlatches (zones stay
      disarmed until you re-save a comment).
- [ ] 20+ journal events total that match your intent → Gate G2 passed.
