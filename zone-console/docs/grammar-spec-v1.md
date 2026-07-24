# Zone Comment Grammar — v1

One short line typed into a rectangle's **Comment** field turns a drawing into an executable zone.
The parser ignores unknown tokens (forward compatibility) and is case-insensitive.

## Format

```
<CLASS> <DIRECTION> [R<risk%>] [EXP:yyyy-mm-dd] [T<1|2>] [TCH:<n>] [E:MID|EDGE] [WKD:FLAT|HOLD] [ID:<tag>]
```

An **empty comment = draft**: the cBot silently ignores the rectangle (safe default while you think).

## Tokens

| Token | Meaning | Values | Default (RED) | Default (YEL) |
|---|---|---|---|---|
| CLASS | conviction class | `RED` / `YEL` (alias `YELLOW`) | — required | — required |
| DIRECTION | trade side | `BUY` / `SELL` | — required | — required |
| `R` | risk % of equity | `R0.1` … class cap · decimal **point** only | `R0.8` (cap 1.0) | `R0.4` (cap 0.5) |
| `EXP:` | zone expiry (UTC date, end of day) | `EXP:2026-08-30` | +42 days | +14 days |
| `T` | entry style | `T1` resting limit at entry price / `T2` touch + M15 confirmation | `T1` | `T2` |
| `TCH:` | touch budget before the zone retires | `TCH:1..3` | `TCH:2` | `TCH:1` |
| `E:` | entry price inside the zone | `E:MID` middle / `E:EDGE` near edge | `E:MID` | `E:EDGE` |
| `WKD:` | weekend policy | `WKD:FLAT` close before weekend / `WKD:HOLD` keep (sized for gaps: stop assumed 2×) | `WKD:FLAT` | `WKD:FLAT` |
| `ID:` | your own tag, no spaces (else assigned at first arm and written back — never derived from geometry, so resizing keeps identity) | `ID:NFPFADE` | auto | auto |

Reward:risk targets are class defaults: **RED → TP at 2.5R**, **YEL → TP at 2.0R** (v1 fixed; configurable later).

## Examples

```
RED SELL                      → Red sell, all defaults (0.8%, T1 at MID, 2 touches, +42d, flat weekends)
RED SELL R0.8 EXP:2026-08-30 T2
YEL BUY R0.3 TCH:1 E:EDGE
RED BUY WKD:HOLD ID:WKLYDEMAND
```

## Validation rules (checked before a zone can arm)

1. CLASS and DIRECTION present and valid.
2. Zone height between **0.15× and 3×** the daily ATR(14).
3. Nearest zone edge within **15× daily ATR** of current price.
4. Expiry in the future, at most **84 days** out.
5. Risk between 0.1% and the class cap (RED 1.0%, YEL 0.5%). A malformed or over-cap
   risk token **rejects the zone** — it is never clamped and never falls back to a
   default (a typo must not become a maximum-size bet).
6. No overlap with an existing same-symbol, same-direction zone (zones stacked
   edge-to-edge are allowed).
7. Unknown tokens produce a **warning**, never a rejection — but anything that *looks*
   like a risk token (starts with `R` + digits) must parse, or rule 5 applies.

## Touch definition (audit-fixed)

A touch begins when the spread band [Bid, Ask] first overlaps the zone, and ends only after mid-price
leaves the zone by **25% of the zone height** (hysteresis, so one wobble ≠ two touches). Touches during
cBot downtime are reconciled from bar history on restart (Session 2).
