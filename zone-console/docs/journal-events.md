# Journal Events — plain-English reference

Your bots write one JSON line per event to `Documents\ZoneConsole\journal\`. This is
every event they can write, what it means, and whether you need to do anything.
🟢 = normal/informational · 🟡 = worth a glance · 🔴 = act now.

Read a line like: `{"ts":"…","event":"zone_armed","zone":"Rectangle 1","id":"a1b2c3d4","status":"Armed",…}`
The `event` field is the key; everything else is detail.

## ShadowLogger (the watch-only logger)

| Event | 🟢🟡🔴 | Meaning |
|---|:--:|---|
| `logger_start` | 🟢 | Logger started on a chart; `watching` = rectangles seen. |
| `logger_stop` | 🟢 | Logger stopped (you stopped it, or cTrader closed). |
| `zone_configured` | 🟢 | You typed a valid comment on a rectangle (`parse` shows what it read). |
| `zone_modified` | 🟢 | You moved/resized a rectangle. |
| `zone_removed` | 🟢 | A rectangle was deleted. |
| `touch_start` / `touch_end` | 🟢 | Price entered / left a zone. This is your raw evidence. |
| `daily_digest` | 🟢 | The nightly summary line. |
| `heartbeat_failing` | 🟡 | 5 pings in a row failed — check the VPS internet if it persists. |

## ZoneExec (the trading bot) — normal life of a trade

| Event | 🟢🟡🔴 | Meaning |
|---|:--:|---|
| `exec_start` | 🟢 | Bot started. **Check `account: "demo"` here.** |
| `zone_armed` | 🟢 | A zone passed all checks; order placed (T1) or waiting to confirm (T2). Shows entry/sl/tp. |
| `zone_adopted` | 🟢 | After a restart, the bot re-attached to an order/position it already had. |
| `touch` | 🟢 | Price reached a zone; `n` = touch number. |
| `zone_filled` | 🟢 | The order filled — you now have a position (protected by broker SL/TP). |
| `position_closed` | 🟢 | The position closed (TP or SL). `next` shows if the zone re-arms or retires. |
| `zone_retired` | 🟢 | Zone used up its touch budget — done, won't trade again. |
| `zone_expired` | 🟢 | Zone passed its expiry date — removed. |
| `daily_digest` | 🟢 | Nightly summary: armed/filled counts, equity, breaker states. |
| `day_anchor` | 🟢 | Records the day's starting equity at Prague midnight (for the −3% rule). |

## ZoneExec — "I chose not to arm" (all normal; `event:"arm_blocked"`, `kind` tells you why)

| `kind` | 🟢🟡🔴 | Meaning |
|---|:--:|---|
| `waiting` | 🟢 | Price isn't on the approach side of the zone yet. Will arm when it is. |
| `market_closed` | 🟢 | Market is closed (e.g. GER40 overnight). Resumes at session open. |
| `cooldown` | 🟢 | A trade on this zone just closed; waiting out the 30-min cooldown. |
| `caps` | 🟡 | A risk cap would be exceeded (too many positions, too much in one currency). Working as designed. |
| `anomaly` | 🟡 | Spread too wide right now — standing aside. |
| `blocked` | 🟡 | A safety state is active (news window, cut-out, kill, ATR missing). |
| `order_rejected` | 🟡 | The broker refused the order. Occasional = fine; repeated = tell me. |
| `validation` / `sizing` | 🟡 | The zone failed a sanity check (too thin/tall/far, or too small to size). Fix the drawing. |

## ZoneExec — safety events (these are the important ones)

| Event | 🟢🟡🔴 | Meaning |
|---|:--:|---|
| `news_pause_start` / `news_pause_end` | 🟢 | A news window from `news.txt` began / ended. Orders paused then re-armed. |
| `session_flatten` | 🟢 | Friday-evening or index-nightly close — everything flattened per FTMO rules. |
| `flatten_all` / `orders_cancelled_all` | 🟢 | A bulk cancel/close happened (reason is in the line). |
| `killswitch` / `killswitch_cleared` | 🟡 | You created / deleted `KILL.txt`. Everything flattened / latch released. |
| `daily_cutout` | 🔴 | Down 3% today — all flat, no new trades until tomorrow. Stop, review, sleep. |
| `account_breaker` | 🔴 | Down 8% from start — all flat, stays off until you reset it in writing. Big review. |
| `news_file_stale` | 🟡 | `news.txt` is missing or old — refresh it (Sunday routine). |
| `anchor_late` | 🟡 | Bot was down over midnight; it's playing safe (no new trades today). |
| `orphan_position_alert` | 🔴 | A position with no matching zone — review it manually. |
| `MANUAL_INTERVENTION` | 🔴 | The bot couldn't cancel a stale order — cancel it yourself in cTrader. |
| `close_failed` / `cancel_failed` | 🔴 | A close/cancel didn't go through — check cTrader and do it by hand. |
| `timer_error` / `tz_error` / `param_warning` | 🟡 | An internal hiccup or a misconfigured parameter — send me the line. |

## The 4 you should react to fast
`daily_cutout`, `account_breaker`, `MANUAL_INTERVENTION`, `close_failed` — everything
else is either normal or a "glance later." When in doubt, copy the line to me.
