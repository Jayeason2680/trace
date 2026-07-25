# Install Guide — ShadowLogger (Session 1)

ShadowLogger only **watches and records**. It cannot place, modify, or close anything —
there is no order code in it at all. Its jobs: log every rectangle and every touch,
send your nightly Telegram digest, and ping the heartbeat.

## One-time prep (15 min)

1. **Telegram bot**: in Telegram, open **@BotFather** → `/newbot` → name it (e.g. `ZoneConsoleBot`)
   → copy the **token** (`123456:ABC-…`).
2. **Your chat id**: message your new bot once (say "hi"), then open
   `https://api.telegram.org/bot<TOKEN>/getUpdates` in a browser and read `"chat":{"id":…}`.
   That number is your **chat id**.
3. **Heartbeat**: sign up free at **healthchecks.io** → New Check → name `zone-console` →
   set Period **2 min**, Grace **3 min** → copy the ping URL (`https://hc-ping.com/…`).

## Install on the VPS (10 min)

4. RDP into the VPS → open cTrader → **Algo** tab (called "Automate" on older versions)
   → New cBot → name it `ShadowLogger`.
5. Replace the template code with `src/ShadowLogger/ShadowLogger.cs` → **Build** (must be green).
6. Add an **instance on every chart you draw zones on** (the same chart window you
   actually draw on — drawings are per-chart): chart → Algo panel → add `ShadowLogger`.
7. Fill parameters: bot token, chat id, heartbeat URL. Leave the rest at defaults.
8. **Start** the instance. Within a minute you should get:
   `🟢 ShadowLogger started on EURUSD h1 — watching N rectangle(s)` and the healthchecks
   check should turn green.

## Daily use

- Draw and manage rectangles exactly as you always have. Type the grammar line into the
  rectangle Comment (double-click the rectangle → Comment) when you consider it a real zone;
  leave it empty while it's just a sketch — both are logged, configured zones are labelled.
- 22:00 MYT: silent digest — zones watched, touches today, zones expiring soon.
  (Sent late rather than skipped if the platform restarts around that hour; never twice.)
- Journal file on the VPS: `Documents\ZoneConsole\journal\<SYMBOL>-<TIMEFRAME>-shadow.jsonl`
  (one JSON line per event, one file per chart; nightly backup comes in Session 2 — for
  now it's included in your normal weekly copy).

## What "good" looks like after 7 days (Gate G1)

- Touches in the journal match what your eyes saw on the chart.
- Digest arrived every night; heartbeat stayed green except when you rebooted.
- At least a handful of zones carry grammar comments, and none of them show
  `parse: "unparsed"` in the journal.
