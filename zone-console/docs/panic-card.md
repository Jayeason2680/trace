# PANIC CARD — print this, keep it in your wallet / phone notes

**When something is wrong and you're not at a desk. Work top to bottom; stop when safe.**

## A. Flatten NOW (any phone, ~1 minute)
1. Open **cTrader mobile** → Positions.
2. **Close every open position.**
3. Orders tab → **cancel every pending order.**
   *(You are now flat at the broker. The cBot may still re-arm zones — continue.)*

## B. Stop the machine (Session-2 feature, ~1 minute)
4. Open Telegram → your bot chat → send **`/killswitch`** → confirm with **`/confirm`**.
5. Wait for the bot's reply: *"all orders withdrawn, positions closed, nothing will re-arm."*
   No reply in 2 minutes → the bot may be dead (that's also safe: nothing can arm) → step C.

## C. Take the console (5–15 minutes)
6. iPhone/MacBook → **Tailscale ON** → **Windows App (RDP)** → connect to the VPS `100.x.x.x`.
7. In cTrader: **stop the ZoneExec cBot** on every chart (Automate → Stop).
8. Still trading somehow? Close cTrader entirely. Positions/orders remain manageable
   from cTrader mobile; SL/TP stay live at the broker regardless.

## D. If even RDP is dead
9. Positions are protected by broker-side SL/TP; you already flattened in step A.
10. forexvps.net portal → restart the VPS. Do **not** re-enable anything tonight.
11. Tomorrow, with coffee: read the journal, find the cause, write it down before re-arming.

**Numbers that matter:** worst per-trade loss ≈ 1R (0.5–1.0% — more only on gaps);
daily cut-out −3%; account breaker −10%.
**Rule:** never un-freeze a tripped cut-out from your phone. Sleep first.
