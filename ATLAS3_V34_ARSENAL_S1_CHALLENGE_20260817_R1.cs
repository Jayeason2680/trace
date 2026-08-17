// =================================================================================================
// ATLAS3 SENTINEL V34.ARSENAL FULL RE-ENGINEERED ARSENAL R1 — release dated 2026-08-17. Profile: S1 CHALLENGE.
//
// V34.ARSENAL is a genuine release-identity successor to V33.SPARK CORE-2, not a relabel. The cBot
// class, BUILD, Version, ReceiptBuild, journal filenames and manifest prefixes are all bumped, so
// cTrader will display V34.ARSENAL and a fresh CONFIRM & ARM enrolment is required. Deploy FLAT.
//
// WHY THIS RELEASE EXISTS
//   V33.SPARK CORE-2 pruned the portfolio to two setups (DAX A1 + NAS C1) by muting the other five
//   to $0 risk. That pruning removed drag but also removed capacity. V34.ARSENAL does not simply
//   un-mute the five: each muted setup is RE-ENGINEERED against the microstructure fault that made
//   it unprofitable, and a sixth, non-correlated commodity lane (XAUUSD H1 Break-of-Structure) is
//   added. The two retained core setups are unchanged.
//
// V34.ARSENAL CHANGE SET — five re-engineered setups, one new lane, and the frontier
//   1. DAX B (A2) BECOMES THE FRANKFURT 08:00 CASH-OPEN VELOCITY BURST.
//      Old fault: the 08:15-08:30 London opening range was unbounded (RangeMax 100000) and entries
//      ran to 11:00 London, so the setup traded inside Frankfurt opening order-matching chop and
//      then collided with DAX A1. New mechanics: signal range is the FIRST cash candle
//      08:00-08:15 London (09:00-09:15 Frankfurt); range must be 20.0-55.0 pts; LONG ONLY and only
//      when the 08:15 close is above the previous session's 16:30 London cash close (gap-aligned
//      momentum); SL 20.0 pts / TP 50.0 pts (2.5R); entries stop at 09:15 London and any open A2
//      position is flattened at market at 09:15 so the book is flat before DAX A1 opens at 09:30.
//   2. DAX C (A3) BECOMES THE PRE-LONDON SQUEEZE BREAKOUT.
//      Old fault: a profitable coil whose entry window ran to 11:00 London, bleeding late entries
//      into European lunch chop. New mechanics: the 08:00-08:30 Frankfurt opening range and the
//      inside-bar compression test are unchanged, but entries now stop at 09:15 London
//      (last eligible bar opens 09:00), and A3 gains a +1.5R break-even ratchet that locks the stop
//      2.0 points beyond entry.
//   3. UK B1 BECOMES THE 08:00 LONDON CASH-OPEN MOMENTUM SWEEP.
//      Old fault: a limit-at-mid entry with a 2.5R target on an index whose average daily range
//      cannot support it - the FTSE 100 is weighted to oil, mining and banking, so pullback limits
//      filled into dead rotations that never expanded to target. New mechanics: immediate MARKET
//      entry on the M15 close that breaks the 08:00-08:30 London opening range; fixed SL 11.0 pts
//      and TP 20.0 pts (1.82R) sized to FTSE ADR rather than basis points of the day open; the
//      inside-bar compression precondition is dropped; an optional commodity-sector confluence
//      filter requires Brent or Copper to be green on the day before a BUY sweep is taken; and the
//      whole UK book is flattened at 11:30 London instead of 16:30.
//   4. NAS C2 BECOMES THE 10:00 ET FAILED-DRIVE LIQUIDITY TRAP FADE.
//      Old fault: an unbounded re-entry with no take-profit that could fire at any time up to
//      14:00 ET, on a stop distance equal to the full opening drive range. New mechanics: the
//      re-entry is armed only by a C1 stop-out at or before 10:20 ET and may only fire inside the
//      10:00-10:20 ET window; it must be confirmed by an M5 candle that sweeps THROUGH the 09:30 ET
//      session open against the failed drive and closes with a rejection body; geometry becomes
//      SL 0.60x drive range and TP 3.00x drive range. C2 now always carries a broker take-profit,
//      so its historical exemption from the take-profit protection audit is removed.
//   5. NEW LANE - XAUUSD H1 STRUCTURAL BREAK-OF-STRUCTURE (setup D1).
//      A closed-H1 fractal Break-of-Structure engine on gold: a 3-candle fractal high/low is
//      confirmed from closed bars only, a closed bar through that fractal is the entry, the stop
//      sits beyond the opposing structural swing pivot subject to a minimum and maximum stop
//      distance, and the target is a configurable R multiple (1.5R-2.0R research band). Gold is a
//      multi-day swing lane, so it is deliberately exempt from the European cross-day flatten and
//      from every intraday session backstop; a configurable maximum hold time bounds it instead.
//
//   6. FRONTIER RISK ARCHITECTURE. Risk is no longer a static per-setup table. One mode selects the
//      DAILY RISK BUDGET as a fraction of this profile's OWN official daily-loss limit, and that
//      budget is split across the two frontier-allocated core engines:
//
//        MODE    BUDGET       S1 (3.0% limit)                     S2 (5.0% limit)
//        GREEN   50% of it    $1,500  DAX 825    NAS 675/450      $2,500  DAX 1,375  NAS 1,125/750
//        ORANGE  75% of it    $2,250  DAX 1,238  NAS 1,013/675    $3,750  DAX 2,063  NAS 1,688/1,125
//        RED     90% of it    $2,700  DAX 1,485  NAS 1,215/810    $4,500  DAX 2,475  NAS 2,025/1,350
//
//      DAX A1 takes 55% of the budget. The NAS C1 DAY budget takes 60%, which the inherited aligned
//      and misaligned multipliers (75% / 50%) resolve to exactly the 45% / 30% of daily budget shown
//      above - so the frontier and the existing NAS sizing compose rather than fight. NAS C2 draws
//      only the residual NAS budget. GOLD D1 is allocated INDEPENDENTLY at 0.25% of the locked
//      initial balance, because a non-correlated multi-day swing is not part of the same-day
//      DAX+NAS stopout constraint the daily budget is derived from.
//
//      GREEN is the shipped default: it is the only tier the source report assigns a 0.00%
//      floor-breach probability. ORANGE and RED are one parameter change away and each needs its
//      own native soak before it carries real capital.
//
// RISK POSTURE
//   The frontier raises the REQUEST; it never raises the approval. Every proposal still passes the
//   unchanged atomic account gate: equity floors, Prague day anchor, open and pending stop risk,
//   execution reserve, correlation throttle, same-direction cluster cap and capacity clipping. The
//   frontier budget also becomes the internal daily worst-case cap, but the buffered official daily
//   floor and the max-loss floor are untouched and the gate still takes the MAXIMUM of all three. In
//   RED the buffered official floor is normally the stricter of the two and the gate clips to it -
//   that is the intended fail-closed ordering, not a misconfiguration.
//
// DEPLOYMENT PER THE SETUP SCORE HIERARCHY
//   DEPLOY (CORE)  DAX A1 94/100 and NAS C1 91/100 - the two frontier-allocated engines.
//   ADD (SWING)    GOLD D1 86/100 - independent 0.25% allocation, outside the daily budget.
//   EXPERIMENT     NAS C2 82/100 trap fade - residual NAS budget only.
//   DROP / PRUNE   UK B1 35/100, plus DAX B and DAX C, which carry no frontier allocation.
//
//   The re-engineered DAX B, DAX C and UK B1 mechanics below are fully implemented and ship
//   DISABLED. That prune verdict was measured on their ORIGINAL mechanics, so they are unproven
//   rather than disproven: backtest each on its own before enabling it, and allocate it budget if
//   you do - enabling one as-is spends capacity the frontier has not reserved.
//
// DELIBERATELY FROZEN IN V34.ARSENAL — these are NOT release identity and must never track a version:
//   - The "V310" durable hard-halt latch payload/parser marker.
//   - The ";v=311;" trade-comment format and its authorization parser. The 100-character ASCII
//     comment budget is unchanged, and setup codes remain the two-character A1/A2/A3/B1/B2/C1/C2
//     tokens plus the new D1, because LabelSetup() round-trips them through the ownership ledger.
//   - The LocalStorage state keys "Atlas3 S1 V311 C " (receipt) and "Atlas3 S1 V311 X " (execution
//     failsafe latch). These look like release identity but are STATE keys. Renaming the failsafe
//     key would orphan an existing latch and force a typed manual fallback. Re-enrolment is instead
//     driven by the bumped ReceiptBuild, which reports an explicit build mismatch.
//   - The historical V2x/V30/V31/V33 lineage comments below, which document provenance.
//
// The atomic account gate, the checksummed ownership ledger, monotonic execution latency tracking,
// receipt-bound enrolment, JSONL journaling, the one-click safe rearm and every durable fail-closed
// state machine are unchanged from V33.SPARK CORE-2.
// =================================================================================================
// INHERITED LINEAGE — V33.SPARK CORE-2 and earlier. Retained for traceability; NOT deployment
// instructions. The V33.SPARK change set below remains ACTIVE except where V34.ARSENAL supersedes
// it: DAX A1's 11:15 entry truncation, the DAX A1 break-even ratchet, the NAS C1 drive-range
// ceiling and the multi-tier NAS laggard cut are all still in force.
// =================================================================================================
// =================================================================================================
// ATLAS3 SENTINEL V33.SPARK CORE-2 OPTIMIZED R1 — release dated 2026-08-17. Profile: S1 CHALLENGE.
//
// V33.SPARK is a genuine release-identity successor to V31.1 RIGHTSIDE HUD, not a relabel. The
// cBot class, BUILD, Version, ReceiptBuild, journal filenames and manifest prefixes are all bumped,
// so cTrader will display V33.SPARK and a fresh CONFIRM & ARM enrolment is required.
//
// V33.SPARK CHANGE SET — five microstructure optimizations
//   1. CORE-2 SETUP ISOLATION. Module defaults are pruned to the retained core: DAX A1 RefBreak and
//      NAS C1 Bell Core ACTIVE; DAX B, DAX C, UK A Coil30, UK B and NAS C2 re-entry DISABLED.
//      Only UkAEnabled actually changed value (true -> false); the other six already held the
//      requested default in V31.1 and are unchanged.
//   2. DAX A1 ENTRY WINDOW TRUNCATION. _daxA.EntryEnd 780 (13:00 London) -> 675 (11:15), cutting
//      midday European lunch false breakouts. RunDaxWindow already gates on tod >= EntryEnd.
//   3. DAX A1 BREAK-EVEN RATCHET. At +1.5R (37.5 pts) the A1 stop moves to entry + 2.0 pts.
//      Gated on CanManagePosition, tighten-only, and latched per position ID for 1 Hz idempotence.
//   4. NAS C1 DRIVE-RANGE CEILING. New NasMaximumDriveRangePts (default 120.0) rejects exhaustion
//      drives, symmetric with the inherited V25 FIX-3 minimum floor.
//   5. MULTI-TIER NAS LAGGARD CUT. New Tier-1 sweep at 11:30 ET on unrealised R < -0.50 alongside
//      the inherited 12:30 ET sweep, whose threshold (-0.30R) is unchanged and whose journal reason
//      is renamed nas_laggard_cut -> nas_laggard_cut_1230.
//
// DELIBERATELY FROZEN IN V33.SPARK — these are NOT release identity and must never track a version:
//   - The "V310" durable hard-halt latch payload/parser marker. A renamed latch token can make the
//     bot fail to recognise a genuine execution-quality halt.
//   - The ";v=311;" trade-comment format and its expectedAuthorization parser. Write side and parse
//     side must agree; the 100-character ASCII comment budget is unchanged.
//   - The LocalStorage state keys "Atlas3 S1 V311 C " (receipt) and "Atlas3 S1 V311 X " (execution
//     failsafe latch). These look like release identity but are STATE keys. Renaming the failsafe
//     key would orphan an existing latch: the device-scoped latch would then be present while the
//     type-scoped latch was absent in CURRENT (4-field) format, which LoadExecutionFailsafeLatch
//     treats as a storage-integrity blocker, refusing one-click rearm and forcing a typed manual
//     fallback. Re-enrolment is instead driven by the bumped ReceiptBuild, which reports an explicit
//     build mismatch rather than silently presenting as a first run.
//   - Roughly 70-75 historical V2x/V30/V31 lineage comments below, which document provenance.
//
// Strategy signals, sizing, session windows, SL/TP geometry, risk limits, the atomic account gate,
// the checksummed ownership ledger, monotonic execution latency tracking, receipt-bound enrolment,
// JSONL journaling and every durable fail-closed state machine are otherwise unchanged from V31.1.
// =================================================================================================
// INHERITED LINEAGE — V31.1 and earlier. Retained for traceability; NOT deployment instructions.
// =================================================================================================
// ATLAS3 SENTINEL S1 CHALLENGE V31.1 RIGHTSIDE HUD R1 — candidate dated 2026-08-14.
// V31.1 derives only from the certified V31.0 ONECLICK SAFE REARM parent. Strategy signals,
// sizing, sessions, risk limits, parameters, execution safety and SL/TP geometry are unchanged.
//
// V31.1 RIGHTSIDE HUD CHANGE SET
//   1. The Strategy Radar static panel moves from Bottom/Left to Bottom/Right for clearer chart use.
//   2. The main safety HUD remains Top/Left, Execution Book remains Top/Right, and the contextual
//      action button remains Bottom/Center.
//   3. V31.0 one-click rearm, durable state, generic device failsafe continuity, V310 latch-format
//      marker, schema 2, ownership, broker-comment and execution-ordering safeguards are preserved.
//   4. S1 strategy, risk, session, sizing, proposal and order behavior are unchanged.
//
// S1 V27 PARENT HISTORY (retained below for traceability; NOT current deployment instructions)
//
// V27 PURPOSE
//   This is the sole new S1 deployment identity derived from the audited V26 safety-working source
//   (parent SHA-256 205af212453a55a1c77979267dd6cf17e50c1d82f55f3d2cdd9320456096ec48).
//   It is deliberately a clean rebrand and state-identity boundary, not a strategy retune.
//
// V27 AMENDMENT HISTORY — 2026-08-09
//   1. New file, cBot class, BUILD value and locked Version string use the V27 deployment identity.
//   2. New orders write v=27 metadata; the configuration fingerprint starts with V27-DEPLOYMENT;
//      runtime JSONL journals use an Atlas3_v27_deployment filename.
//   3. Restart reconstruction accepts only V27 metadata with matching configuration, owner and
//      stage epoch. Positions from V26 or older are intentionally rejected and fail closed.
//   4. Operator-facing HUD and startup text are rebranded V27. Production strategy labels remain
//      version-independent so the exclusive-account lease still prevents parallel Atlas3 masters.
//   5. The account/profile execution-quality halt file remains version-independent intentionally:
//      changing a version must never bypass a genuine execution-quality stop. S1 peak-EOD state
//      also remains account + stage-epoch scoped so its risk floor cannot be reset by rebranding.
//
// V27 PARAMETER LOCK
//   - All 111 parameter declarations are present. The BUILD text is the only changed default.
//   - The other 110 parameter names, types, groups, bounds, steps and defaults are unchanged from
//     V26, including Challenge stage, USD 100,000 initial balance, US100.cash M5 host convention,
//     strategy switches, session windows, risk amounts, SL/TP geometry and every safety threshold.
//   - MasterArm remains FALSE. ExpectedAccountNumber, DeploymentOwnerId, StageEpochId and
//     EvaluationStartText remain blank because inventing FTMO account identity would be unsafe.
//
// V27 HISTORICAL DEPLOYMENT CONTRACT — DO NOT FOLLOW FOR V30.1 STORAGEKEY SAFE
//   - Import/attach in FTMO cTrader to US100.cash M5 and deploy FLAT at the start of a fresh stage.
//   - Use one Atlas3 master only. Do not run V20-V26, a standalone module, or S2 on this account.
//   - Before arming: confirm USD 100,000 initial capital; set the exact account number; set a unique
//     4-8 character owner ID and NEW stage epoch ID; set the evaluation start date; verify symbols,
//     time frame, costs and news coverage; then explicitly set MasterArm=true.
//   - The embedded restricted-event list currently ends 2026-12-09 and the bot fails closed when
//     coverage is not verified at least seven days ahead.
//
// PERFORMANCE / EXECUTION IMPACT
//   - C# comments are discarded by the compiler. Class/build/version strings, journal filenames
//     and stricter restart identity checks do not change signal calculations, sizing, sessions,
//     SL/TP, order style or the normal market-data decision path. Their speed impact is negligible.
//   - Historical V27 behavior differed. V30.1 preserves unrecognised exposure, blocks entries and
//     requires durable-ledger authority before any automatic mutation. Introduce V30.1 flat.
//   - V26 native FTMO-cTrader reference only: Jan +$2,482.77 / +5.93007R / 3.06% max equity DD;
//     May +$5,699.02 / +13.89174R / 2.39% DD; July -$4,302.84 / -8.76156R / 6.81% DD.
//     Those results are lineage evidence, not a promise of future results or native V27 proof.
//
// V26 SOURCE HISTORY — entry signals, session windows, SL/TP geometry and risk defaults unchanged:
//
//   1. Capacity rejection is now an explicit runtime state with transition audit records.
//   2. DAX/UK positions from an earlier London trading date are flattened immediately.
//   3. Managed-close attempt counts are included in exit audit records.
//
// V25 CHANGE SET — four defects that could lose a funded account. NO strategy logic was touched:
//   every signal window, SL/TP geometry, session time and risk DEFAULT is byte-identical to V24.
//   Only guard/validation code changed. Diff V25 against V24 to confirm.
//
//   FIX-1 (CRITICAL, S1-only) — EOD-trailing floor no longer resets on restart.
//     V24: `PeakEodBalanceOverride > 0` short-circuited ReconstructPeakEodBalance(), AND
//     _peakEodBalance was never persisted. Every restart snapped the ratcheted peak back to the
//     operator's static override, dropping MaxLossLimit() by the whole accumulated profit while the
//     HUD stayed green (peak 107k -> restart -> bot authorises trading down to 90k, a 7k breach).
//     The same condition also switched OFF the history-reconciliation halt.
//     V25: always reconstruct; adopt max(initial, reconstructed, persisted, override); the override
//     is a floor-RAISER only. Peak is persisted atomically on every ratchet advance, scoped by
//     account + stage epoch + initial balance. Reconciliation halt now always runs.
//
//   FIX-2 (HIGH) — news guard now covers CHALLENGE, not just FUNDED.
//     V24: NewsBlocked(), EnforceRestrictedNewsFlatBook() and ValidateRestrictedEventCoverage() all
//     early-returned on !IsFunded(). An evaluation account had no news protection and no
//     calendar-expiry halt — while NAS C1 opens 09:45 ET and holds to 15:30 ET, straight through a
//     14:00 ET FOMC. V25 gates on Stage != Unconfigured instead. NOTE: the shipped FOMC list ends
//     2026-12-09; the coverage halt will now correctly refuse to arm once it lapses.
//
//   FIX-3 (HIGH) — NAS drive-range floor + absolute notional ceiling.
//     V24 only rejected range <= 0. Since units = risk / (0.90 * range), a compressed opening range
//     drove size toward VolumeInUnitsMax: dollar risk looked right, dollar-per-tick did not.
//     V25 adds NasMinimumDriveRangePts (default 15.0) and MaxNotionalPerPositionUsd (default
//     1,500,000) enforced inside VolumeForRisk for EVERY setup, clamping DOWN and re-checking the
//     broker minimum so a clamp can never round up.
//
//   FIX-4 (CRITICAL) — dynamic ladder can no longer exceed the stage-locked risk ceiling.
//     V24: stage scales are capped (Funded 0.50) but every ladder scale is capped at 1.00 and
//     nothing compared them; enabling the ladder raised FUNDED risk 0.40 -> 0.70 (+75%) with the
//     stage lock still showing ON. V25 halts at startup if any enabled ladder scale exceeds the
//     stage ceiling — the ladder may only ever REDUCE risk.
//   FIX-4b (HIGH) — refuses to start with EITHER the Challenge or the Funded profit-tier ratchet
//     disabled while the ladder is on, because that configuration walks risk UP after losses.
//
//   FIX-2a (CRITICAL, regression found by post-patch audit) — "News calendar verified through UTC"
//     ships BLANK, which V24 never noticed because the coverage halt was Funded-only. De-gating it
//     in FIX-2 turned that blank default into an unconditional StartupHalt on an evaluation account
//     (checked every bar, _startupHalt never clears) — the bot would have placed ZERO trades.
//     Implicit coverage is now derived from the last event in the calendar when the operator has
//     not attested; an explicit attestation still wins if later. Fails closed once the list lapses.
//   FIX-1a (MEDIUM) — the persisted peak now rejects any stored value above 1.5x initial balance,
//     so a mistyped PeakEodBalanceOverride cannot latch permanently with no way to clear it.
//     HISTORICAL NOTE SUPERSEDED BY V29: never delete peak-EOD or execution safety state to bypass
//     a halt. V29 treats lost existing state as an integrity failure requiring review.
//
// REMAINING LIMITATIONS (not changed by this safety working copy):
//   - Risk defaults are designed for a USD 100,000 account. Startup asserts the configured initial
//     balance against the backtest/account balance within InitialCapitalToleranceUsd.
//   (Struck: an earlier note here claimed "S2 lacks S1's history-reconciliation halt". That was
//    WRONG — S2's InitialBalance-vs-(Balance - realised - cashAdj) test is the same equation
//    rearranged, with the same $100 default tolerance. No guard was needed and none was added.)
//   - 20-minute stale-signal window is too wide for market orders.
//   - IsDaxHoliday() contains US holidays.
//   - Correlation estimator is inert (stress floor 0.70 > trigger 0.60) — conservative, but the
//     displayed correlation is a constant, not a measurement.
//
// BACKTEST USE: select the configured US100 host on M5, use a USD 100,000 starting balance, and
// keep the dynamic ladder OFF unless its stage-specific scales have been deliberately retuned.
// Historical V27 note: V29 replaces the public MasterArm workflow with receipt-bound enrollment.
// =================================================================================================
// ATLAS3 SENTINEL S1 V24 — dynamic-risk predecessor dated 2026-08-02.
// FTMO 1-Step, USD 100k, three-market / seven-setup portfolio master for cTrader.
//
// HOST: attach ONE instance to US100.cash M5. The host is an operator convention and a hard guard.
// DATA: GER40.cash M15; UK100.cash M15 + H4; US100.cash M5 are loaded explicitly.
//
// ARCHITECTURE CONTRACT
//  1. DAX, UK and NAS strategy engines may create TradeProposal objects only.
//  2. Only SubmitProposal() may place an order.
//  3. The account gate checks current equity, Prague-day anchor, all open-position remaining
//     stop risk, pending reservations, proposed risk and an execution/gap allowance atomically.
//  4. Profile rules are compile-locked: 3% daily loss, 10% EOD-trailing max loss, 10% target,
//     50% Best Day rule. Internal daily worst-case cap defaults to $2,400.
//  5. Production labels are compile-locked and version-independent.
//  6. Existing v20 standalone sources are not modified and must never run beside this Master.
//
// V24 MIGRATION CONTRACT
//  - Deploy flat when moving from V23. V24 intentionally rejects V23 position metadata.
//  - Dynamic risk is opt-in at the master switch; static V23 risk behaviour remains the default.
//  - Dynamic tiers use the reconstructed start-of-Prague-day BALANCE, never intraday equity.
//
// VALIDATION STATUS
//  - Strategy rules are ported from the v20.GPT standalones into explicit market contexts.
//  - Per-bot account-floor/target controls are intentionally replaced by the single master gate.
//  - Source compilation is checked; strategy parity and backtest evidence remain separate tasks.
// =================================================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.FullAccess, TimeZone = TimeZones.UTC)]
    public class ATLAS3_S1_CHALLENGE_V34_ARSENAL_20260817_R1 : Robot
    {
        public enum StageMode
        {
            Unconfigured,
            Challenge,
            Funded
        }

        public enum HealthGuardMode
        {
            ShadowOnly,
            LegacyPermanent
        }

        // V34.ARSENAL FRONTIER: the FTMO frontier risk architecture. GREEN preserves capital,
        // ORANGE is balanced-aggressive, RED is maximum frontier velocity. OFF restores the
        // static per-setup risk table exactly as it behaved before the frontier existed.
        public enum FrontierRiskMode
        {
            Off,
            Green,
            Orange,
            Red
        }

        private enum EntryStyle
        {
            MarketWithPips,
            LimitAtMid,
            MarketAbsoluteProtection
        }

        private enum DeploymentState
        {
            SetupRequired,
            Confirming,
            Activating,
            Running,
            Blocked
        }

        private enum ExecutionGuardState
        {
            Armed,
            LatencyCooldown,
            HardHalt
        }

        private enum ProposalOutcome
        {
            Accepted,
            TransientReject,
            TerminalReject
        }

        // -----------------------------------------------------------------------------------------
        // 1. IDENTITY AND ARM
        // -----------------------------------------------------------------------------------------
        [Parameter("BUILD", DefaultValue = "ATLAS3 S1 CHALLENGE V34.ARSENAL | 7-SETUP ARSENAL | 20260817-R1", Group = "0. Build")]
        public string ReleaseNotice { get; set; }

        private StageMode Stage { get { return StageMode.Challenge; } }
        private bool MasterArm { get { return RunningMode != RunningMode.RealTime || _deploymentArmed; } }

        [Parameter("REQUIRED HOST", DefaultValue = "US100.cash M5 ONLY", Group = "1. Identity & Arm")]
        public string RequiredHostNotice { get; set; }

        private double InitialBalance { get { return 100000.0; } }
        private double VerifiedCashAdjustmentsUsd { get { return 0.0; } }
        private double InitialCapitalToleranceUsd { get { return 100.0; } }

        private string ExpectedAccountNumber { get { return _effectiveAccountNumber; } }

        private string ExpectedAccountCurrency { get { return "USD"; } }

        [Parameter("Peak EOD balance override $ (0=auto)", DefaultValue = 0, MinValue = 0, Group = "1. Identity & Arm")]
        public double PeakEodBalanceOverride { get; set; }

        private string DeploymentOwnerId { get { return _effectiveOwnerId; } }
        private string StageEpochId { get { return _effectiveEpochId; } }
        private string EvaluationStartText { get { return _effectiveEvaluationStartText; } }

        private double DayStartBalanceOverride { get { return 0.0; } }
        private string DayStartOverrideDateText { get { return ""; } }

        // -----------------------------------------------------------------------------------------
        // 2. SYMBOLS AND DATA
        // -----------------------------------------------------------------------------------------
        [Parameter("DAX symbol", DefaultValue = "GER40.cash", Group = "2. Symbols & Data")]
        public string DaxSymbolName { get; set; }

        [Parameter("UK symbol", DefaultValue = "UK100.cash", Group = "2. Symbols & Data")]
        public string UkSymbolName { get; set; }

        [Parameter("NAS symbol / host", DefaultValue = "US100.cash", Group = "2. Symbols & Data")]
        public string NasSymbolName { get; set; }

        // V34.ARSENAL: the XAUUSD H1 break-of-structure lane. This symbol is only required
        // when GoldEnabled is true; with the lane off the bot never resolves or loads it.
        [Parameter("GOLD symbol", DefaultValue = "XAUUSD", Group = "2. Symbols & Data")]
        public string GoldSymbolName { get; set; }

        [Parameter("Strict data checks (host fixed)", DefaultValue = true, Group = "2. Symbols & Data")]
        public bool StrictDataChecks { get; set; }

        // -----------------------------------------------------------------------------------------
        // 3. MODULE SWITCHES
        // -----------------------------------------------------------------------------------------
        [Parameter("DAX A RefBreak", DefaultValue = true, Group = "3. Modules")]
        public bool DaxAEnabled { get; set; }

        // V34.ARSENAL: A2 is re-engineered into the Frankfurt 08:00 cash-open velocity burst, but
        // it ships DISABLED. The frontier budget allocates 55% to A1 and 60% to the NAS C1 day
        // budget and nothing to A2, so enabling A2 would spend budget the frontier has not
        // reserved. The V33.SPARK 5-year tick audit also lists DAX B inside its pruned drag. The
        // re-engineered mechanics are fully implemented and one switch away once independently
        // backtested; do that before turning this on.
        [Parameter("DAX B Frankfurt open burst (A2)", DefaultValue = false, Group = "3. Modules")]
        public bool DaxBEnabled { get; set; }

        // V34.ARSENAL: A3 is re-engineered into the pre-London squeeze breakout (entries stop at
        // 09:15 London) but ships DISABLED for the same reason as A2 — it carries no frontier
        // budget allocation, and the audit prunes it by default.
        [Parameter("DAX C Pre-London squeeze (A3, Challenge only)", DefaultValue = false, Group = "3. Modules")]
        public bool DaxCEnabled { get; set; }

        // V34.ARSENAL replaces the Coil30 limit-at-mid mechanics with the 08:00 London cash-open
        // momentum sweep, but ships it DISABLED. The V33.SPARK setup-score hierarchy scores UK B1
        // 35/100 with an explicit DROP/PRUNE directive and attributes -$22k of 5-year drag to it,
        // and the frontier allocates it no budget. NOTE: that verdict was measured on the Coil30
        // mechanics, NOT on the re-engineered sweep implemented below — so this is "unproven",
        // not "disproven". Backtest the sweep on its own before enabling it.
        [Parameter("UK B1 London open sweep", DefaultValue = false, Group = "3. Modules")]
        public bool UkAEnabled { get; set; }

        [Parameter("UK B RefBreakX test (forced OFF)", DefaultValue = false, Group = "3. Modules")]
        public bool UkBRequested { get; set; }

        [Parameter("NAS C1 Bell core", DefaultValue = true, Group = "3. Modules")]
        public bool NasCoreEnabled { get; set; }

        // V34.ARSENAL: C2 is re-engineered into the 10:00 ET failed-drive liquidity trap fade,
        // which the setup-score hierarchy scores 82/100 and designates EXPERIMENT with a 1:3R
        // target. It ships ENABLED and is bounded to the RESIDUAL NAS day budget left after C1,
        // so it can never spend allocation the frontier has already committed to the core.
        [Parameter("NAS C2 trap fade (Challenge only)", DefaultValue = true, Group = "3. Modules")]
        public bool NasReentryEnabled { get; set; }

        // V34.ARSENAL: the new non-correlated commodity lane, scored 86/100 with an ADD (SWING)
        // directive and an independent 0.25% swing allocation outside the daily budget. Turning
        // this OFF removes every gold symbol/data requirement, so the bot still starts on an
        // account without XAUUSD.
        [Parameter("GOLD D1 H1 break-of-structure", DefaultValue = true, Group = "3. Modules")]
        public bool GoldEnabled { get; set; }

        // -----------------------------------------------------------------------------------------
        // 4. RISK — A2 is deliberately zero-risk by default; MaxValue preserves research opt-in.
        // -----------------------------------------------------------------------------------------
        [Parameter("Portfolio risk scale", DefaultValue = 0.40, MinValue = 0.25, MaxValue = 1.00, Step = 0.05, Group = "4. Setup Risk")]
        public double PortfolioRiskScale { get; set; }

        [Parameter("Use stage-locked risk", DefaultValue = true, Group = "4. Setup Risk")]
        public bool UseStageRiskScale { get; set; }

        [Parameter("Challenge risk scale", DefaultValue = 0.60, MinValue = 0.25, MaxValue = 0.70, Step = 0.05, Group = "4. Setup Risk")]
        public double ChallengeRiskScale { get; set; }

        [Parameter("Funded risk scale", DefaultValue = 0.40, MinValue = 0.25, MaxValue = 0.50, Step = 0.05, Group = "4. Setup Risk")]
        public double FundedRiskScale { get; set; }

        [Parameter("DAX A risk $", DefaultValue = 860, MinValue = 0, MaxValue = 926, Step = 1, Group = "4. Setup Risk")]
        public double DaxARiskUsd { get; set; }

        [Parameter("DAX B risk $", DefaultValue = 347, MinValue = 0, MaxValue = 347, Step = 1, Group = "4. Setup Risk")]
        public double DaxBRiskUsd { get; set; }

        [Parameter("DAX C risk $", DefaultValue = 172, MinValue = 0, MaxValue = 172, Step = 1, Group = "4. Setup Risk")]
        public double DaxCRiskUsd { get; set; }

        [Parameter("UK A risk $", DefaultValue = 701, MinValue = 0, MaxValue = 755, Step = 1, Group = "4. Setup Risk")]
        public double UkARiskUsd { get; set; }

        [Parameter("UK B test risk $", DefaultValue = 0, MinValue = 0, MaxValue = 100, Step = 1, Group = "4. Setup Risk")]
        public double UkBRiskUsd { get; set; }

        [Parameter("NAS day budget $", DefaultValue = 858, MinValue = 0, MaxValue = 924, Step = 1, Group = "4. Setup Risk")]
        public double NasDayBudgetUsd { get; set; }

        [Parameter("NAS C2 trap-fade risk $", DefaultValue = 105, MinValue = 0, MaxValue = 105, Step = 1, Group = "4. Setup Risk")]
        public double NasReentryRiskUsd { get; set; }

        [Parameter("NAS funded adoption risk %", DefaultValue = 90, MinValue = 50, MaxValue = 100, Step = 5, Group = "4. Setup Risk")]
        public double NasFundedAdoptionRiskPct { get; set; }

        // -----------------------------------------------------------------------------------------
        // 4F. V34.ARSENAL — FRONTIER RISK ARCHITECTURE
        // -----------------------------------------------------------------------------------------
        // GREEN 50% / ORANGE 75% / RED 90% of the official daily-loss limit becomes the daily risk
        // budget; DAX A1 takes 55% of it and the NAS C1 day budget 60% (which the inherited
        // aligned/misaligned split resolves to 45% / 30%). OFF restores the static risk table.
        //
        // GREEN is the deliberate default. It is the only tier the source report assigns a 0.00%
        // floor-breach probability, and it is still a material step up from the static table.
        // Moving to ORANGE or RED is a single parameter change and needs its own native soak.
        [Parameter("Frontier risk mode", DefaultValue = FrontierRiskMode.Green, Group = "4F. Frontier Risk")]
        public FrontierRiskMode FrontierMode { get; set; }

        // The gold lane is allocated INDEPENDENTLY of the daily budget, as a flat percentage of
        // the locked initial balance, because it is a non-correlated multi-day swing rather than
        // part of the same-day DAX+NAS stopout constraint the daily budget is derived from.
        [Parameter("Frontier GOLD swing allocation %", DefaultValue = 0.25, MinValue = 0.0, MaxValue = 2.0, Step = 0.05, Group = "4F. Frontier Risk")]
        public double GoldSwingAllocationPct { get; set; }

        // -----------------------------------------------------------------------------------------
        // 4G. V34.ARSENAL — GOLD D1 GEOMETRY AND THE UK B1 COMMODITY CONFLUENCE FILTER
        // -----------------------------------------------------------------------------------------
        [Parameter("GOLD risk $", DefaultValue = 700, MinValue = 0, MaxValue = 950, Step = 1, Group = "4. Setup Risk")]
        public double GoldRiskUsd { get; set; }

        // Research band for the H1 break-of-structure target is 1.5R-2.0R; 1.75 is its midpoint.
        [Parameter("GOLD target R multiple", DefaultValue = 1.75, MinValue = 1.00, MaxValue = 3.00, Step = 0.05, Group = "4. Setup Risk")]
        public double GoldTargetRMultiple { get; set; }

        // A structural pivot can sit only cents away from the break. The minimum stop keeps the
        // implied size sane; VolumeForRisk still applies the absolute notional ceiling on top.
        [Parameter("GOLD minimum stop $", DefaultValue = 3.00, MinValue = 0.50, MaxValue = 25.00, Step = 0.25, Group = "4. Setup Risk")]
        public double GoldMinimumStopUsd { get; set; }

        // A pivot far below the break is a broken structure, not a swing entry: reject it.
        [Parameter("GOLD maximum stop $ (0=off)", DefaultValue = 30.00, MinValue = 0.0, MaxValue = 200.0, Step = 0.50, Group = "4. Setup Risk")]
        public double GoldMaximumStopUsd { get; set; }

        [Parameter("GOLD swing-pivot stop buffer $", DefaultValue = 0.30, MinValue = 0.0, MaxValue = 5.0, Step = 0.05, Group = "4. Setup Risk")]
        public double GoldStopBufferUsd { get; set; }

        // Gold is the only multi-day lane, so it is exempt from every session backstop. This is
        // the bound that replaces them; 0 disables it and lets the trade run to SL/TP only.
        [Parameter("GOLD max hold hours (0=off)", DefaultValue = 72, MinValue = 0, MaxValue = 336, Step = 1, Group = "4. Setup Risk")]
        public int GoldMaxHoldHours { get; set; }

        // UK B1 buy sweeps require oil or copper to be green on the day. VERIFY THESE SYMBOL
        // NAMES AGAINST THE BROKER before arming: if the filter is ON and no listed symbol can
        // be resolved with usable daily data, every BUY sweep is refused fail-closed (SELL
        // sweeps are unaffected, matching the researched asymmetry). Blank/false disables it.
        [Parameter("UK B1 commodity confluence", DefaultValue = true, Group = "4. Setup Risk")]
        public bool UkConfluenceFilterOn { get; set; }

        [Parameter("UK B1 confluence symbols CSV", DefaultValue = "XBRUSD,XCUUSD", Group = "4. Setup Risk")]
        public string UkConfluenceSymbolsCsv { get; set; }

        // -----------------------------------------------------------------------------------------
        // 4B. DYNAMIC RISK LADDER — absolute portfolio scales selected once per Prague day.
        // The master switch is deliberately OFF by default so V24 preserves V23 sizing unless the
        // operator explicitly opts into the researched ladder.
        // -----------------------------------------------------------------------------------------
        // V29 Challenge production lock: dynamic sizing is unavailable until its own native suite.
        private bool UseDynamicRiskLadder { get { return false; } }

        [Parameter("Challenge ladder enabled", DefaultValue = true, Group = "4B. Dynamic Risk")]
        public bool ChallengeDynamicRiskEnabled { get; set; }

        [Parameter("Challenge profit ladder enabled", DefaultValue = true, Group = "4B. Dynamic Risk")]
        public bool ChallengeProfitLadderEnabled { get; set; }

        [Parameter("Challenge profit-tier ratchet", DefaultValue = true, Group = "4B. Dynamic Risk")]
        public bool ChallengeProfitTierRatchetEnabled { get; set; }

        [Parameter("Challenge downside de-risk enabled", DefaultValue = true, Group = "4B. Dynamic Risk")]
        public bool ChallengeDownsideDeriskEnabled { get; set; }

        [Parameter("Challenge downside hysteresis", DefaultValue = true, Group = "4B. Dynamic Risk")]
        public bool ChallengeDownsideHysteresisEnabled { get; set; }

        [Parameter("Challenge downside trigger %", DefaultValue = -2.0, MinValue = -9.0, MaxValue = 0.0, Step = 0.25, Group = "4B. Dynamic Risk")]
        public double ChallengeDownsideTriggerPct { get; set; }

        [Parameter("Challenge downside recovery %", DefaultValue = -1.0, MinValue = -8.75, MaxValue = 5.0, Step = 0.25, Group = "4B. Dynamic Risk")]
        public double ChallengeDownsideRecoveryPct { get; set; }

        [Parameter("Challenge downside scale", DefaultValue = 0.40, MinValue = 0.10, MaxValue = 1.00, Step = 0.05, Group = "4B. Dynamic Risk")]
        public double ChallengeDownsideScale { get; set; }

        [Parameter("Challenge accelerator scale", DefaultValue = 0.80, MinValue = 0.10, MaxValue = 1.00, Step = 0.05, Group = "4B. Dynamic Risk")]
        public double ChallengeAcceleratorScale { get; set; }

        [Parameter("Challenge first downshift %", DefaultValue = 6.0, MinValue = 0.0, MaxValue = 9.0, Step = 0.25, Group = "4B. Dynamic Risk")]
        public double ChallengeFirstDownshiftPct { get; set; }

        [Parameter("Challenge mid scale", DefaultValue = 0.70, MinValue = 0.10, MaxValue = 1.00, Step = 0.05, Group = "4B. Dynamic Risk")]
        public double ChallengeMidScale { get; set; }

        [Parameter("Challenge second downshift %", DefaultValue = 8.0, MinValue = 0.25, MaxValue = 9.75, Step = 0.25, Group = "4B. Dynamic Risk")]
        public double ChallengeSecondDownshiftPct { get; set; }

        [Parameter("Challenge late scale", DefaultValue = 0.60, MinValue = 0.10, MaxValue = 1.00, Step = 0.05, Group = "4B. Dynamic Risk")]
        public double ChallengeLateScale { get; set; }

        [Parameter("Funded ladder enabled", DefaultValue = true, Group = "4B. Dynamic Risk")]
        public bool FundedDynamicRiskEnabled { get; set; }

        [Parameter("Funded profit ladder enabled", DefaultValue = true, Group = "4B. Dynamic Risk")]
        public bool FundedProfitLadderEnabled { get; set; }

        [Parameter("Funded profit-tier ratchet", DefaultValue = true, Group = "4B. Dynamic Risk")]
        public bool FundedProfitTierRatchetEnabled { get; set; }

        [Parameter("Funded downside de-risk enabled", DefaultValue = true, Group = "4B. Dynamic Risk")]
        public bool FundedDownsideDeriskEnabled { get; set; }

        [Parameter("Funded downside hysteresis", DefaultValue = true, Group = "4B. Dynamic Risk")]
        public bool FundedDownsideHysteresisEnabled { get; set; }

        [Parameter("Funded downside trigger %", DefaultValue = -2.0, MinValue = -9.0, MaxValue = 0.0, Step = 0.25, Group = "4B. Dynamic Risk")]
        public double FundedDownsideTriggerPct { get; set; }

        [Parameter("Funded downside recovery %", DefaultValue = -1.0, MinValue = -8.75, MaxValue = 1.75, Step = 0.25, Group = "4B. Dynamic Risk")]
        public double FundedDownsideRecoveryPct { get; set; }

        [Parameter("Funded downside scale", DefaultValue = 0.35, MinValue = 0.10, MaxValue = 1.00, Step = 0.05, Group = "4B. Dynamic Risk")]
        public double FundedDownsideScale { get; set; }

        [Parameter("Funded accelerator scale", DefaultValue = 0.70, MinValue = 0.10, MaxValue = 1.00, Step = 0.05, Group = "4B. Dynamic Risk")]
        public double FundedAcceleratorScale { get; set; }

        [Parameter("Funded downshift %", DefaultValue = 2.0, MinValue = 0.0, MaxValue = 3.75, Step = 0.25, Group = "4B. Dynamic Risk")]
        public double FundedDownshiftPct { get; set; }

        [Parameter("Funded payout-approach scale", DefaultValue = 0.50, MinValue = 0.10, MaxValue = 1.00, Step = 0.05, Group = "4B. Dynamic Risk")]
        public double FundedPayoutApproachScale { get; set; }

        [Parameter("Funded payout lock", DefaultValue = false, Group = "4B. Dynamic Risk")]
        public bool FundedPayoutLockEnabled { get; set; }

        [Parameter("Funded payout lock target %", DefaultValue = 4.0, MinValue = 2.25, MaxValue = 10.0, Step = 0.25, Group = "4B. Dynamic Risk")]
        public double FundedPayoutLockTargetPct { get; set; }

        // -----------------------------------------------------------------------------------------
        // 5. ATOMIC ACCOUNT GATE
        // -----------------------------------------------------------------------------------------
        [Parameter("Daily worst-case cap $", DefaultValue = 2400, MinValue = 500, MaxValue = 2900, Step = 50, Group = "5. Atomic Account Gate")]
        public double DailyWorstCaseCapUsd { get; set; }

        [Parameter("Max-loss safety buffer $", DefaultValue = 2000, MinValue = 250, MaxValue = 5000, Step = 250, Group = "5. Atomic Account Gate")]
        public double MaxLossSafetyBufferUsd { get; set; }

        [Parameter("Near-floor de-risk room $", DefaultValue = 4000, MinValue = 1000, MaxValue = 8000, Step = 250, Group = "5. Atomic Account Gate")]
        public double NearFloorDeriskRoomUsd { get; set; }

        [Parameter("Execution/gap reserve %", DefaultValue = 15, MinValue = 0, MaxValue = 100, Step = 1, Group = "5. Atomic Account Gate")]
        public double ExecutionReservePct { get; set; }

        [Parameter("Emergency flatten buffer $", DefaultValue = 500, MinValue = 0, MaxValue = 2000, Step = 100, Group = "5. Atomic Account Gate")]
        public double EmergencyBufferUsd { get; set; }

        [Parameter("Completion risk multiplier", DefaultValue = 0.25, MinValue = 0.10, MaxValue = 1.00, Step = 0.05, Group = "5. Atomic Account Gate")]
        public double CompletionRiskMultiplier { get; set; }

        [Parameter("Exclusive account ownership", DefaultValue = true, Group = "5. Atomic Account Gate")]
        public bool ExclusiveAccount { get; set; }

        [Parameter("Clip risk to gate capacity", DefaultValue = true, Group = "5. Atomic Account Gate")]
        public bool ClipRiskToCapacity { get; set; }

        [Parameter("Minimum clipped risk %", DefaultValue = 25, MinValue = 10, MaxValue = 100, Step = 5, Group = "5. Atomic Account Gate")]
        public double MinimumClippedRiskPct { get; set; }

        // -----------------------------------------------------------------------------------------
        // 6. EXECUTION SAFETY
        // -----------------------------------------------------------------------------------------
        [Parameter("DAX max spread (points)", DefaultValue = 5.0, MinValue = 0, Group = "6. Execution")]
        public double DaxMaxSpreadPts { get; set; }

        [Parameter("UK max spread (points)", DefaultValue = 3.0, MinValue = 0, Group = "6. Execution")]
        public double UkMaxSpreadPts { get; set; }

        [Parameter("NAS max spread (points)", DefaultValue = 5.0, MinValue = 0, Group = "6. Execution")]
        public double NasMaxSpreadPts { get; set; }

        [Parameter("GOLD max spread $", DefaultValue = 0.60, MinValue = 0, Group = "6. Execution")]
        public double GoldMaxSpreadUsd { get; set; }

        // V34.ARSENAL: the re-engineered B1 sweep enters at MARKET, so this now defaults OFF.
        // It is retained as a deliberate escape hatch: turning it on routes B1 back through the
        // limit-at-mid path with the NEW 11.0/20.0 pt geometry, which also keeps that router
        // branch reachable rather than leaving it dead code.
        [Parameter("UK B1 limit-at-mid (legacy, OFF)", DefaultValue = false, Group = "6. Execution")]
        public bool UkLimitEntry { get; set; }

        [Parameter("UK limit expiry M15 bars", DefaultValue = 2, MinValue = 1, MaxValue = 8, Group = "6. Execution")]
        public int UkLimitExpiryBars { get; set; }

        [Parameter("UK H4 EMA50 gate (test; OFF)", DefaultValue = false, Group = "6. Execution")]
        public bool UkH4Gate { get; set; }

        [Parameter("NAS C1 SL × drive", DefaultValue = 0.90, MinValue = 0.75, MaxValue = 1.25, Step = 0.05, Group = "6. Execution")]
        public double NasCoreStopDriveMultiple { get; set; }

        [Parameter("NAS C1 TP × drive", DefaultValue = 2.50, MinValue = 1.50, MaxValue = 3.50, Step = 0.10, Group = "6. Execution")]
        public double NasCoreTargetDriveMultiple { get; set; }

        [Parameter("NAS decision grace seconds", DefaultValue = 30, MinValue = 5, MaxValue = 120, Step = 5, Group = "6. Execution")]
        public int NasDecisionGraceSeconds { get; set; }

        [Parameter("Maximum quote age seconds", DefaultValue = 5, MinValue = 1, MaxValue = 30, Step = 1, Group = "6. Execution")]
        public int MaximumQuoteAgeSeconds { get; set; }

        // -----------------------------------------------------------------------------------------
        // 6B. PORTFOLIO ROBUSTNESS GUARDS
        // -----------------------------------------------------------------------------------------
        [Parameter("Correlation risk throttle", DefaultValue = true, Group = "6B. RobustGuard")]
        public bool CorrelationThrottleOn { get; set; }

        [Parameter("Correlation trigger", DefaultValue = 0.60, MinValue = 0, MaxValue = 1, Step = 0.05, Group = "6B. RobustGuard")]
        public double CorrelationTrigger { get; set; }

        [Parameter("Correlation stress floor", DefaultValue = 0.70, MinValue = 0, MaxValue = 1, Step = 0.05, Group = "6B. RobustGuard")]
        public double CorrelationStressFloor { get; set; }

        [Parameter("Correlation lookback days", DefaultValue = 60, MinValue = 20, MaxValue = 180, Step = 5, Group = "6B. RobustGuard")]
        public int CorrelationLookbackDays { get; set; }

        [Parameter("2nd correlated risk multiplier", DefaultValue = 0.50, MinValue = 0, MaxValue = 1, Step = 0.05, Group = "6B. RobustGuard")]
        public double CorrelatedSecondRiskMultiplier { get; set; }

        [Parameter("Same-direction index risk cap $", DefaultValue = 1600, MinValue = 250, MaxValue = 3000, Step = 50, Group = "6B. RobustGuard")]
        public double SameDirectionClusterCapUsd { get; set; }

        [Parameter("Execution-quality kill switch", DefaultValue = true, Group = "6B. RobustGuard")]
        public bool ExecutionQualityKillOn { get; set; }

        [Parameter("Execution sample window", DefaultValue = 20, MinValue = 5, MaxValue = 100, Step = 1, Group = "6B. RobustGuard")]
        public int ExecutionSampleWindow { get; set; }

        [Parameter("Mean adverse slip halt (R)", DefaultValue = 0.06, MinValue = 0.01, MaxValue = 0.50, Step = 0.01, Group = "6B. RobustGuard")]
        public double MeanAdverseSlippageHaltR { get; set; }

        [Parameter("Single adverse slip incident (R)", DefaultValue = 0.20, MinValue = 0.01, MaxValue = 1.00, Step = 0.01, Group = "6B. RobustGuard")]
        public double SingleAdverseSlippageIncidentR { get; set; }

        [Parameter("Market-order latency incident ms", DefaultValue = 1500, MinValue = 100, MaxValue = 10000, Step = 100, Group = "6B. RobustGuard")]
        public int MarketLatencyIncidentMs { get; set; }

        [Parameter("Close latency incident ms", DefaultValue = 2000, MinValue = 100, MaxValue = 15000, Step = 100, Group = "6B. RobustGuard")]
        public int CloseLatencyIncidentMs { get; set; }

        [Parameter("Consecutive latency incidents to cooldown", DefaultValue = 2, MinValue = 1, MaxValue = 10, Step = 1, Group = "6B. RobustGuard")]
        public int ConsecutiveExecutionIncidents { get; set; }

        [Parameter("Latency cooldown minutes", DefaultValue = 60, MinValue = 5, MaxValue = 1440, Step = 5, Group = "6B. RobustGuard")]
        public int LatencyCooldownMinutes { get; set; }

        [Parameter("Latency episodes to hard halt", DefaultValue = 3, MinValue = 1, MaxValue = 10, Step = 1, Group = "6B. RobustGuard")]
        public int LatencyEpisodesToHardHalt { get; set; }

        [Parameter("Latency escalation window hours", DefaultValue = 24, MinValue = 1, MaxValue = 168, Step = 1, Group = "6B. RobustGuard")]
        public int LatencyEscalationWindowHours { get; set; }

        [Parameter("Manual rearm execution halt", DefaultValue = false, Group = "6B. RobustGuard")]
        public bool RearmExecutionHalt { get; set; }

        [Parameter("Execution rearm nonce", DefaultValue = "", Group = "6B. RobustGuard")]
        public string ExecutionRearmNonce { get; set; }

        [Parameter("Module health action", DefaultValue = HealthGuardMode.ShadowOnly, Group = "6B. RobustGuard")]
        public HealthGuardMode ModuleHealthAction { get; set; }

        // -----------------------------------------------------------------------------------------
        // 7. GOVERNANCE / AUDIT
        // -----------------------------------------------------------------------------------------
        [Parameter("Show master HUD", DefaultValue = true, Group = "7. Audit & HUD")]
        public bool ShowHud { get; set; }

        [Parameter("Show execution cockpit", DefaultValue = true, Group = "7. Audit & HUD")]
        public bool ShowExecutionCockpit { get; set; }

        [Parameter("Draw host trade levels", DefaultValue = true, Group = "7. Audit & HUD")]
        public bool DrawHostTradeLevels { get; set; }

        [Parameter("Daily JSONL journal", DefaultValue = true, Group = "7. Audit & HUD")]
        public bool JournalOn { get; set; }

        [Parameter("Extra US half-days yyyy-MM-dd,csv", DefaultValue = "", Group = "7. Audit & HUD")]
        public string ExtraHalfDaysCsv { get; set; }

        // V25 FIX-2: this guard now applies to CHALLENGE as well as FUNDED. In V24 both NewsBlocked()
        // and ValidateRestrictedEventCoverage() early-returned on !IsFunded(), so an evaluation
        // account had NO news protection and NO calendar-expiry halt whatsoever — while NAS C1 opens
        // 09:45 ET and holds to 15:30 ET, i.e. straight through a 14:00 ET FOMC.
        // Property name deliberately unchanged so existing .cbotset files keep binding.
        [Parameter("NAS news guard (all armed stages)", DefaultValue = true, Group = "7. Audit & HUD")]
        public bool FundedNewsGuard { get; set; }

        // V25 FIX-3: hard floor on the NAS 09:30-09:45 drive range. Below this the implied position
        // size (risk / 0.90*range) becomes dangerous in notional terms even though the dollar risk
        // looks correct. 15 index points on US100 is a normal-quiet opening; genuine coils are rarer
        // than the size they would produce is dangerous.
        [Parameter("NAS minimum drive range (pts)", DefaultValue = 15.0, MinValue = 0.0, MaxValue = 200.0, Step = 0.5, Group = "4. Setup Risk")]
        public double NasMinimumDriveRangePts { get; set; }

        // V33.SPARK CORE-2: upper ceiling on the NAS 09:30-09:45 drive range. Above this the opening
        // drive is an exhaustion move rather than a trend initiation, and the 09:45 entry routinely
        // faces a sharp reversal. Kept in the same group as the floor so the pair reads together;
        // 0 disables the ceiling. Symmetric with the V25 FIX-3 floor immediately above.
        [Parameter("NAS maximum drive range (pts, 0=off)", DefaultValue = 120.0, MinValue = 0.0, MaxValue = 1000.0, Step = 0.5, Group = "4. Setup Risk")]
        public double NasMaximumDriveRangePts { get; set; }

        // V25 FIX-3: absolute notional ceiling per position, applied inside VolumeForRisk for every
        // setup. Belt-and-braces against any future geometry that produces a tiny stop distance.
        [Parameter("Max notional per position $ (0=off)", DefaultValue = 1500000, MinValue = 0, Step = 50000, Group = "5. Atomic Account Gate")]
        public double MaxNotionalPerPositionUsd { get; set; }

        [Parameter("Restricted events UTC ; separated", DefaultValue = "2026-01-28 19:00;2026-03-18 18:00;2026-04-29 18:00;2026-06-17 18:00;2026-07-29 18:00;2026-09-16 18:00;2026-10-28 18:00;2026-12-09 19:00", Group = "7. Audit & HUD")]
        public string RestrictedEventsCsv { get; set; }

        [Parameter("News calendar verified through UTC", DefaultValue = "", Group = "7. Audit & HUD")]
        public string RestrictedEventsVerifiedThroughText { get; set; }

        [Parameter("News pre-flat lead minutes", DefaultValue = 5, MinValue = 3, MaxValue = 30, Step = 1, Group = "7. Audit & HUD")]
        public int NewsPreFlatLeadMinutes { get; set; }

        // -----------------------------------------------------------------------------------------
        // LOCKED IDENTITY
        // -----------------------------------------------------------------------------------------
        private const string ReleaseToken = "20260817-R1";
        private const string Version = "ATLAS3_SENTINEL_S1.CHALLENGE.V34.ARSENAL.2026-08-17.R1";
        private const string ProfileCode = "S1";
        private const string ReceiptSchema = "2";
        private const string ReceiptBuild = "V34ARSENAL-S1C-20260817-R1";
        private const int MaxTradeCommentLength = 100;
        private const int MaxLocalStorageKeyLength = 50;
        private const string OwnershipLedgerSchema = "1";
        private const string ExecutionStateSchema = "2";
        private const double OneClickRearmReviewSeconds = 5.0;
        private const string RequiredHostSymbol = "US100.cash";

        private const string DaxALabel = "AT3-S1-A-A1";
        private const string DaxBLabel = "AT3-S1-A-A2.esp";
        private const string DaxCLabel = "AT3-S1-A-A3.coil";
        private const string UkALabel = "AT3-S1-B-B1.coil";
        private const string UkBLabel = "AT3-S1-B-B2.test";
        private const string NasCoreLabel = "AT3-S1-C-C1.bell";
        private const string NasReLabel = "AT3-S1-C-C2.RE";
        // V34.ARSENAL: the new XAUUSD H1 break-of-structure lane.
        private const string GoldLabel = "AT3-S1-D-D1.bos";

        private static readonly string[] MasterLabels =
        {
            DaxALabel, DaxBLabel, DaxCLabel, UkALabel, UkBLabel, NasCoreLabel, NasReLabel,
            GoldLabel
        };

        private static readonly string[] LegacyLabels =
        {
            "ASetup", "BSetup", "CSetup", "Coil30", "RefBreakX", "Bell Core", "Bell-2RE"
        };

        // FTMO 1-Step locked rule set, rechecked against the official objectives on 2026-08-12.
        private const double FtmoDailyLossPct = 3.0;
        private const double FtmoMaxLossPct = 10.0;
        private const double ProfitTargetPct = 10.0;
        private const double BestDayMaxShare = 0.50;

        // Strategy constants frozen from v20.
        private const double DaxASlPts = 25.0;
        private const double DaxATpPts = 62.5;
        // V33.SPARK CORE-2: DAX A1 break-even ratchet. Both values are PRICE distances, the same
        // unit as DaxASlPts — this file consistently treats "points" as raw price and divides by
        // PipSize only when a broker call needs pips (see VolumeForRisk and the slPips conversions).
        // Trigger is +1.5R against the 25.0 pt A1 stop; the stop is then locked 2.0 points BEYOND
        // entry so the ratchet covers spread and commission rather than landing exactly at cost.
        private const double DaxABreakEvenTriggerPts = 37.5;
        private const double DaxABreakEvenOffsetPts = 2.0;
        // V34.ARSENAL: DAX B geometry is retuned for the Frankfurt cash-open velocity burst.
        // 20.0 pt stop / 50.0 pt target is 2.5R, replacing the inherited 55.0 / 110.0 (2.0R).
        private const double DaxBSlPts = 20.0;
        private const double DaxBTpPts = 50.0;
        private const double DaxCSlBp = 10.0;
        private const double DaxCTpMultiple = 3.0;
        private const double UkSlBp = 13.0;
        private const double UkTpMultiple = 2.5;
        private const double NasBodyFractionMin = 0.60;
        private const double NasAlignedPct = 75.0;
        private const double NasMisalignedPct = 50.0;

        // ===== V34.ARSENAL RE-ENGINEERED MUTED-SETUP CONSTANTS =====
        // All "points" below are raw price distances, the same unit as DaxASlPts. London
        // minute-of-day values match the existing ToUk() convention used by the DAX engine.
        //
        // DAX B (A2) — Frankfurt 08:00 cash-open velocity burst.
        private const int DaxBSignalStartTod = 480;        // 08:00 London = 09:00 Frankfurt open
        private const int DaxBSignalEndTod = 495;          // 08:15 London — one cash candle
        private const int DaxBEntryEndTod = 555;           // 09:15 London — hard entry cutoff
        private const int DaxBHardFlattenTod = 555;        // flat at market before A1 at 09:30
        private const double DaxBRangeMinPts = 20.0;
        private const double DaxBRangeMaxPts = 55.0;
        private const double DaxBMinClosePosition = 0.20;
        // Opening minute of the M15 bar that CLOSES the 16:30 London cash session. That close
        // is the previous-session reference DAX B must gap above before a long is allowed.
        private const int DaxBPriorCashCloseTod = 975;     // 16:15 London bar, closes 16:30
        //
        // DAX C (A3) — pre-London squeeze. The 08:00-08:30 opening range and the inside-bar
        // compression test are inherited unchanged; only the entry cutoff and the ratchet are new.
        private const int DaxCEntryEndTod = 555;           // 09:15 London (was 11:00)
        private const double DaxCBreakEvenTriggerRMultiple = 1.5;
        private const double DaxCBreakEvenOffsetPts = 2.0;
        //
        // UK B1 — London cash-open momentum sweep. Absolute points, sized to FTSE average daily
        // range, replacing the inherited basis-point stop and 2.5R multiple.
        private const double UkSweepSlPts = 11.0;
        private const double UkSweepTpPts = 20.0;          // 1.82R
        private const int UkSessionFlattenTod = 690;       // 11:30 London (was 16:30)
        //
        // NAS C2 — 10:00 ET failed-drive liquidity trap fade.
        private const double NasTrapStopDriveMultiple = 0.60;
        private const double NasTrapTargetDriveMultiple = 3.00;
        private const double NasTrapBodyFractionMin = 0.50;
        private static readonly TimeSpan NasTrapWindowStart = new TimeSpan(10, 0, 0);
        private static readonly TimeSpan NasTrapWindowEnd = new TimeSpan(10, 20, 0);
        //
        // FRONTIER RISK ARCHITECTURE — shares of the daily risk budget, which is itself
        // (locked initial balance x official daily-loss % x mode fraction).
        private const double FrontierRedFraction = 0.90;
        private const double FrontierOrangeFraction = 0.75;
        private const double FrontierGreenFraction = 0.50;
        // DAX A1 takes 55% of the budget outright.
        private const double FrontierDaxShare = 0.55;
        // The NAS C1 DAY BUDGET is 60% of the frontier budget. The inherited aligned/misaligned
        // multipliers (NasAlignedPct 75% / NasMisalignedPct 50%) then resolve it to exactly the
        // 45% / 30% of daily budget the frontier specifies, so the two mechanisms compose rather
        // than overriding one another.
        private const double FrontierNasBudgetShare = 0.60;
        // The C2 trap fade is an EXPERIMENT tier: it draws only the residual NAS budget left
        // after an aligned C1, and the Math.Min against the remaining day budget still binds it.
        private const double FrontierNasReentryShare = 0.15;
        //
        // GOLD D1 — XAUUSD H1 break-of-structure.
        private const int GoldMinimumBars = 60;
        private const double GoldRollingKillR = -20.0;
        private const int GoldRollingKillWindow = 60;

        // -----------------------------------------------------------------------------------------
        // MARKET CONTEXT
        // -----------------------------------------------------------------------------------------
        private Symbol _daxSymbol;
        private Symbol _ukSymbol;
        private Symbol _nasSymbol;
        private Bars _daxM15;
        private Bars _ukM15;
        private Bars _ukH4;
        private Bars _nasM5;
        private ExponentialMovingAverage _ukH4Ema;

        private TimeZoneInfo _eastern;
        private TimeZoneInfo _prague;
        private DateTime _evaluationStartUtc = DateTime.MinValue;

        // One-click deployment state. These values are never typed into cTrader parameters.
        private DeploymentState _deploymentState = DeploymentState.SetupRequired;
        private bool _deploymentArmed;
        private bool _managementAuthority;
        private bool _runtimeActivated;
        private bool _runtimeEventsSubscribed;
        private bool _bindingValid;
        private string _bindingStatus = "not checked";
        private string _effectiveAccountNumber = "";
        private string _effectiveOwnerId = "";
        private string _effectiveEpochId = "";
        private string _effectiveEvaluationStartText = "";
        private string _proposedEvaluationStartText = "";
        private string _receiptKey = "";
        private string _receiptPendingKey = "";
        private string _receiptConfirmedUtc = "";
        private string _boundAccountIdentityHash = "";
        private string _boundRuntimeIdentityFingerprint = "";
        private Button _confirmArmButton;
        private bool _confirmClickBusy;
        private DateTime _enrollmentReviewAvailableUtc = DateTime.MinValue;
        private long _oneClickRearmReviewStartedTicks;
        private string _oneClickRearmReviewSignature = "";
        private long _oneClickRearmSafetyContextSequence;
        private bool _oneClickRearmSafetyObserversSubscribed;
        private bool _executionRearmTransactionBusy;
        private readonly List<string> _preflightBlockers = new List<string>();
        private readonly List<string> _startupReasons = new List<string>();
        private DateTime _dataWarmupSinceUtc = DateTime.MinValue;
        private DateTime _lastDataWarmupLogUtc = DateTime.MinValue;
        private bool _dataBlockedLogged;
        private bool _journalProbeChecked;
        private bool _journalProbeOk;
        private string _journalProbeError = "";
        private string _lastPreflightSignature = "";
        private string _lastReadySignature = "";
        private string _cachedStrategyManifest = "";
        private string _cachedStrategyFingerprint = "";
        private string _cachedStrategyReceiptHash = "";
        private string _cachedDeploymentKey = "";
        private string _cachedDeploymentManifest = "";
        private string _cachedDeploymentFingerprint = "";

        private bool _startupHalt;
        private bool _dataReady;
        private string _haltReason = "";
        private bool _gateBusy;
        private string _lastProposalRejectCode = "";
        private long _brokerMutationSequence;
        private bool _stateRebuildMode;
        private bool _journalDead;
        private string _journalDir;
        private string _executionStatePath;
        private string _ownershipLedgerPath;
        private bool _ownershipLedgerLoaded;
        private bool _ownershipLedgerInvalid;
        private string _ownershipLedgerStatus = "not loaded";
        private string _peakEodStatePath;   // V25 FIX-1: durable EOD-trailing peak
        private bool _peakEodInitialised;
        private bool _peakEodSafetyFailed;
        private string _executionFailsafeKey = "";
        private string _executionFailsafeDeviceKey = "";
        private ExecutionGuardState _executionGuardState = ExecutionGuardState.Armed;
        private string _executionGuardReason = "none";
        private DateTime _executionStateSinceUtc = DateTime.MinValue;
        private DateTime _latencyCooldownUntilUtc = DateTime.MinValue;
        private DateTime _latencyWindowStartUtc = DateTime.MinValue;
        private DateTime _lastLatencyIncidentUtc = DateTime.MinValue;
        private int _latencyIncidentStreak;
        private int _latencyEpisodeCount;
        private bool _executionAutoResumePending;
        private DateTime _executionFlatBookObservedUtc = DateTime.MinValue;
        private bool _executionStateCreatedFreshAtStartup;
        private bool _executionFailsafeLatchPreexistedAtStartup;
        private bool _executionStorageIntegrityBlocker;
        private string _executionFailsafeLatchedReason = "";
        private bool _legacyDeviceOnlyExecutionLatch;
        private bool _executionFailsafeSnapshotReadFailed;
        private bool _deferExecutionFailsafeWritesUntilReceiptContext;
        private string _startupExecutionTypeLatch = "";
        private string _startupExecutionDeviceLatch = "";
        private string _lastExecutionRearmNonceHash = "-";
        private readonly Queue<double> _adverseSlippageR = new Queue<double>();
        private double _lastCorrelationUsed;
        private double _lastCorrelationMultiplier = 1.0;

        // Account-wide risk/objective state.
        private string _pragueDay = "";
        private double _dayAnchorBalance;
        private double _peakEodBalance;
        private double _transientReservedRisk;
        private bool _dayHalt;
        private bool _objectiveLocked;
        private bool _objectiveClosing;
        private bool _fundedPayoutLocked;
        private bool _fundedPayoutClosing;
        private bool _targetReached;
        private int _challengeProfitTierRatchet;
        private int _fundedProfitTierRatchet;
        private bool _challengeDownsideLatched;
        private bool _fundedDownsideLatched;
        private double _dynamicDayProgressPct;
        private double _currentDynamicRiskScale;
        private string _currentDynamicRiskTier = "STATIC";
        private int _objectiveTradeDays;
        private double _bestDayProfit;
        private double _positiveDaysProfit;
        private string _lastGateReason = "startup";
        private bool _capacityFrozen;
        private string _capacityFreezeDetail = "";
        private DateTime _capacityFreezeSinceUtc = DateTime.MinValue;
        private DateTime _lastObjectiveRefresh = DateTime.MinValue;
        private double _historyReconstructedBalance;
        private DateTime _lastDaxPulseUtc = DateTime.MinValue;
        private DateTime _lastUkPulseUtc = DateTime.MinValue;
        private DateTime _lastNasPulseUtc = DateTime.MinValue;
        private DateTime _lastEngineCheckUtc = DateTime.MinValue;
        private readonly HashSet<string> _designLevelObjects = new HashSet<string>();

        private static readonly Color DesignHealthy = Color.FromArgb(255, 46, 204, 113);
        private static readonly Color DesignWarning = Color.FromArgb(255, 241, 196, 15);
        private static readonly Color DesignDanger = Color.FromArgb(255, 231, 76, 60);
        private static readonly Color DesignInfo = Color.FromArgb(255, 52, 152, 219);
        private static readonly Color DesignText = Color.FromArgb(255, 225, 232, 238);
        private static readonly Color DesignMuted = Color.FromArgb(255, 149, 165, 166);
        private static readonly Color DesignEntry = Color.FromArgb(210, 52, 152, 219);
        private static readonly Color DesignTarget = Color.FromArgb(210, 46, 204, 113);
        private static readonly Color DesignStop = Color.FromArgb(210, 231, 76, 60);
        private static readonly Color DesignPending = Color.FromArgb(210, 241, 196, 15);

        private sealed class TradeProposal
        {
            public string Engine;
            public string Setup;
            public string Label;
            public Symbol Symbol;
            public TradeType Side;
            public double RequestedRisk;
            public double SlDistancePts;
            public double TpDistancePts;
            public EntryStyle Style;
            public DateTime DecisionTimeUtc;
            public int ExpiryBars;
            public double AbsoluteSl;
            public double AbsoluteTp;
            public double ReferenceDistancePts;
            public double TriggerPrice;
            public int TriggerDirection;
            public string SignalId;
        }

        private sealed class SignalRetry
        {
            public TradeProposal Proposal;
            public DateTime DeadlineUtc;
            public DateTime NextAttemptUtc;
            public int Attempts;
            public string LastReason;
        }

        private sealed class PendingInfo
        {
            public TradeProposal Proposal;
            public double ApprovedRisk;
            public double Units;
            public double DecisionPrice;
            public double Spread;
            public string ExpectedComment;
            public long RegisteredMonotonicTicks;
        }

        // Durable recovery authority for comments that a live broker blanked or truncated. Every
        // record is account/config/owner/epoch bound by the checksummed file header and must match
        // the exact live broker ID plus immutable trade fields before it grants any authority.
        private sealed class OwnershipLedgerRecord
        {
            public string ExposureType;
            public long BrokerId;
            public string Label;
            public string SymbolName;
            public TradeType Side;
            public double VolumeInUnits;
            public double BrokerPrice;
            public DateTime BrokerTimeUtc;
            public string SignalId;
            public double ApprovedRisk;
            public double RequestedRisk;
            public double ReferenceDistancePts;
            public double StopDistancePts;
            public double TargetDistancePts;
            public double DecisionPrice;
            public string ExpectedCommentHash;
        }

        // One broker entry mutation is possible at a time because SubmitProposal owns _gateBusy.
        // This context exists before the broker call so a synchronous PositionOpened/PendingFilled
        // callback can be reconciled without trusting a missing or delayed broker comment. It is
        // deliberately transient and is never used for restart adoption.
        private sealed class ActiveEntryMutation
        {
            public TradeProposal Proposal;
            public double ApprovedRisk;
            public double Units;
            public double DecisionPrice;
            public double Spread;
            public string ExpectedComment;
            public string ExpectedCommentHash;
            public DateTime StartedUtc;
            public long StartedMonotonicTimestamp;
            public int? PendingOrderId;
            public long? PositionId;
            public bool PendingFilledSeen;
            public bool PendingFillCallbackSeen;
            public bool PendingCancelledSeen;
            public bool PendingFillHandledSuccessfully;
            public bool EntryJournalWritten;
        }

        private sealed class DirectLimitTerminalExpectation
        {
            public long PositionId;
            public string Label;
            public string SymbolName;
            public TradeType Side;
            public double Units;
            public double TargetPrice;
            public DateTime StartedUtc;
            public DateTime ResultUtc;
            public string ExpectedComment;
            public long ExpiresMonotonicTicks;
            public bool ClosedSeen;
        }

        private sealed class PositionMeta
        {
            public string Engine;
            public string Setup;
            public string Label;
            public string SignalId;
            public double InitialRisk;
            public double ApprovedRisk;
            public double StopRiskDistancePts;
            public double ReferenceDistancePts;
            public int Direction;
            public double DecisionPrice;
            public double MfePrice;
            public double MaePrice;
            public double LastStopPrice;
            public double LastTakeProfitPrice;
            public string Kind;
        }

        private sealed class CloseRequestMeta
        {
            public string Reason;
            public DateTime RequestedWallTimeUtc;
            public DateTime BrokerCallReturnedWallTimeUtc;
            public long RequestMonotonicTimestamp;
            public long BrokerCallReturnedMonotonicTimestamp;
            public double BrokerCallMs;
            public bool BrokerCallCompleted;
            public double DecisionPrice;
            public int Attempts;
        }

        private readonly Dictionary<int, PendingInfo> _pendingInfo = new Dictionary<int, PendingInfo>();
        private readonly Dictionary<string, OwnershipLedgerRecord> _ownershipLedger =
            new Dictionary<string, OwnershipLedgerRecord>(StringComparer.Ordinal);
        private readonly Dictionary<string, SignalRetry> _signalRetries = new Dictionary<string, SignalRetry>();
        private readonly Dictionary<long, PositionMeta> _positionMeta = new Dictionary<long, PositionMeta>();
        private readonly Dictionary<long, CloseRequestMeta> _closeRequestMeta = new Dictionary<long, CloseRequestMeta>();
        private readonly HashSet<long> _currentRunCreatedPositionIds = new HashSet<long>();
        private readonly HashSet<int> _currentRunCreatedPendingIds = new HashSet<int>();
        private readonly Dictionary<long, string> _expectedPositionComments = new Dictionary<long, string>();
        private readonly Dictionary<int, string> _expectedPendingComments = new Dictionary<int, string>();
        private readonly HashSet<long> _positionCommentDiagnosticsWritten = new HashSet<long>();
        private readonly HashSet<int> _pendingCommentDiagnosticsWritten = new HashSet<int>();
        private readonly HashSet<long> _processedPositionOpenedIds = new HashSet<long>();
        private readonly HashSet<int> _processedPendingFillIds = new HashSet<int>();
        private readonly HashSet<int> _processedPendingCancelIds = new HashSet<int>();
        private readonly HashSet<long> _requestAuthorizedImmediateFillPositionIds = new HashSet<long>();
        private readonly Dictionary<long, DirectLimitTerminalExpectation> _directLimitTerminalExpectations =
            new Dictionary<long, DirectLimitTerminalExpectation>();
        private ActiveEntryMutation _activeEntryMutation;
        private readonly HashSet<long> _closeRetry = new HashSet<long>();
        private readonly HashSet<int> _cancelRetry = new HashSet<int>();
        private readonly HashSet<int> _cancelRequested = new HashSet<int>();
        private readonly Dictionary<long, long> _closeRetryNotBeforeTicks = new Dictionary<long, long>();
        private readonly Dictionary<int, long> _cancelRetryNotBeforeTicks = new Dictionary<int, long>();
        private readonly Dictionary<int, long> _cancelRequestMonotonicTicks = new Dictionary<int, long>();
        private long _nextCloseRetryOperationTicks;
        private long _nextCancelOperationTicks;
        private const double BrokerRetryBackoffSeconds = 5.0;
        private const double BrokerRetryAggregateSpacingSeconds = 1.0;
        private const double BrokerMutationEventTimeoutSeconds = 30.0;
        private const double ActiveEntryMutationMaxSeconds = 30.0;
        private readonly string _runId = Guid.NewGuid().ToString("N").Substring(0, 12);
        private System.IO.FileStream _accountLease;
        private bool _accountLeaseOwned;
        private DateTime _dayStartOverrideDate = DateTime.MinValue;
        private int _journalFailures;
        private DateTime _journalRetryAt = DateTime.MinValue;

        // -----------------------------------------------------------------------------------------
        // DAX STATE
        // -----------------------------------------------------------------------------------------
        private sealed class DaxWindow
        {
            public string Label;
            public bool Enabled;
            public bool TwoSided;
            public int SignalStart;
            public int SignalEnd;
            public int EntryEnd;
            public double SlPts;
            public double TpPts;
            public double RiskUsd;
            public double RangeMin;
            public double RangeMax;
            public double MinClosePosition;
            public int RequiredBars;
            // V34.ARSENAL: DAX B only. The signal candle must close above the previous
            // session cash close before a long is considered (gap-aligned momentum).
            public bool RequirePriorCashCloseAlignment;
            public double High;
            public double Low;
            public double Close;
            public int SeenBars;
            public bool Done;
            public bool Valid;
            public bool Traded;
            public bool ExpiredLogged;

            public void Reset()
            {
                High = double.MinValue;
                Low = double.MaxValue;
                Close = 0;
                SeenBars = 0;
                Done = false;
                Valid = false;
                Traded = false;
                ExpiredLogged = false;
            }
        }

        private DaxWindow _daxA;
        private DaxWindow _daxB;
        private int _daxLastProcessed = -1;
        private DateTime _daxDay = DateTime.MinValue;
        private bool _daxNoTradeDay;
        private bool _daxHasPrev;
        private double _daxPrevHigh;
        private double _daxPrevLow;
        private bool _daxCOrReady;
        private bool _daxCInsideSeen;
        private bool _daxCTraded;
        // V33.SPARK CORE-2: position IDs whose A1 stop has already been ratcheted to break-even.
        // RunDaxTimeBackstop runs at 1 Hz, so without this the bot would re-issue a broker modify
        // every second for any position whose stop the venue has not yet moved. Cleared per DAX day.
        private readonly HashSet<long> _daxBreakEvenRatcheted = new HashSet<long>();
        // Backoff for a REJECTED ratchet, mirroring _closeRetryNotBeforeTicks. Without it a broker
        // that persistently refuses the modify would be re-called, and journalled, once per second
        // for the remainder of the session.
        private readonly Dictionary<long, long> _daxBreakEvenRetryNotBeforeTicks =
            new Dictionary<long, long>();
        private int _daxCOrBars;
        private double _daxCOrHigh;
        private double _daxCOrLow;
        private double _daxCDayOpen;
        // V34.ARSENAL: previous session 16:30 London cash close — the DAX B trend-alignment
        // reference. Recomputed once per DAX day from closed M15 history. NaN blocks DAX B.
        private double _daxPriorCashClose = double.NaN;
        private int _daxConsecutiveLosses;
        private bool _daxDayBlocked;
        private bool _daxDailyProfitLocked;
        private readonly Queue<bool> _daxWinQueue = new Queue<bool>();
        private int _daxMonitorTotal;
        private int _daxFirst20Wins;
        private bool _daxRegimeHalt;
        private bool _daxRegimeAlert;

        // -----------------------------------------------------------------------------------------
        // UK STATE
        // -----------------------------------------------------------------------------------------
        private int _ukLastProcessed = -1;
        private DateTime _ukDay = DateTime.MinValue;
        private double _ukOrHigh;
        private double _ukOrLow;
        private double _ukDayOpen;
        private bool _ukOrReady;
        private bool _ukCompressionSeen;
        private bool _ukATraded;
        // V34.ARSENAL: lazily resolved daily Bars for the UK B1 commodity confluence filter.
        private readonly List<Bars> _ukConfluenceDaily = new List<Bars>();
        private bool _ukConfluenceInitialised;
        private bool _ukConfluenceWarned;
        private readonly Queue<double> _ukRollingR = new Queue<double>();
        private bool _ukKillHalt;
        private bool _ukKillAlert;

        // -----------------------------------------------------------------------------------------
        // NAS STATE
        // -----------------------------------------------------------------------------------------
        private readonly HashSet<string> _nasHalfDays = new HashSet<string>();
        private readonly HashSet<string> _nasFullHolidays = new HashSet<string>();
        private readonly List<DateTime> _restrictedEvents = new List<DateTime>();
        private readonly HashSet<string> _newsPreFlatDone = new HashSet<string>();
        private DateTime _restrictedEventsVerifiedThroughUtc = DateTime.MinValue;
        private bool _restrictedEventsParseValid = true;
        private string _nasEtDay = "";
        private bool _nasSignalDone;
        private bool _nasCoreDone;
        // V33.SPARK CORE-2: Tier-1 (11:30 ET) laggard sweep. One-shot per ET day, exactly like
        // _nasLaggardDone, which remains the Tier-2 (12:30 ET) sweep. Both are reset in
        // NasRolloverIfNeeded, the single ET-day rollover path.
        private bool _nasEarlyLaggardDone;
        private bool _nasLaggardDone;
        private bool _nasFlattened;
        private bool _nasReDone;
        private bool _nasPendingReentry;
        private int _nasPendingReDirection;
        private double _nasDayRiskUsed;
        private double _nasDriveRange;
        private int _nasDriveDirection;
        // V34.ARSENAL: the 09:30 ET session open. The C2 trap fade requires price to sweep
        // THROUGH this level against the failed drive before any reversal may be proposed.
        private double _nasSessionOpen = double.NaN;
        private double _nasTodayLastRthClose = double.NaN;
        private double _nasPreviousRthClose = double.NaN;
        private int _nasCalendarYear;
        private readonly Queue<double> _nasKillQueue = new Queue<double>();
        private int _nasLiveCloses;
        private bool _nasKillHalt;
        private bool _nasKillAlert;

        // -----------------------------------------------------------------------------------------
        // GOLD STATE — V34.ARSENAL XAUUSD H1 break-of-structure lane
        // -----------------------------------------------------------------------------------------
        private Symbol _goldSymbol;
        private Bars _goldH1;
        private int _goldLastProcessed = -1;
        // Most recent CONFIRMED 3-candle fractal pivots. Both are confirmed from closed bars
        // strictly older than the bar being evaluated, so a break is never self-confirming.
        private double _goldSwingHigh = double.NaN;
        private double _goldSwingLow = double.NaN;
        // One entry per distinct pivot: a pivot that has already produced a break is consumed.
        private double _goldLastBrokenHigh = double.NaN;
        private double _goldLastBrokenLow = double.NaN;
        private DateTime _lastGoldPulseUtc = DateTime.MinValue;
        private readonly Queue<double> _goldRollingR = new Queue<double>();
        private bool _goldKillHalt;
        private bool _goldKillAlert;

        // =========================================================================================
        // LIFECYCLE
        // =========================================================================================
        protected override void OnStart()
        {
            _journalDir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "TradingJournal",
                "Atlas3");

            SubscribeOneClickRearmSafetyObservers();
            AcquireAccountLease();
            InitialiseExecutionQualityState();
            ResolveTimeZones();
            ParseDayStartOverrideDate();
            ParseRestrictedEvents();
            SeedNasCalendars();

            _effectiveAccountNumber = Account.Number.ToString(CultureInfo.InvariantCulture);
            _proposedEvaluationStartText = ProposedEvaluationStartText();

            _daxSymbol = Symbols.GetSymbol(DaxSymbolName);
            _ukSymbol = Symbols.GetSymbol(UkSymbolName);
            _nasSymbol = Symbols.GetSymbol(NasSymbolName);
            // V34.ARSENAL: gold is only a hard dependency while its lane is enabled, so an
            // account without XAUUSD can still run the six index setups with GoldEnabled off.
            _goldSymbol = GoldEnabled ? Symbols.GetSymbol(GoldSymbolName) : null;

            if (_daxSymbol == null || _ukSymbol == null || _nasSymbol == null)
                HaltStartup("one or more configured symbols are unavailable on this account");
            if (GoldEnabled && _goldSymbol == null)
                HaltStartup("GOLD lane is enabled but " + GoldSymbolName +
                    " is unavailable on this account");

            if (!string.Equals(NasSymbolName, RequiredHostSymbol, StringComparison.Ordinal) ||
                !string.Equals(SymbolName, RequiredHostSymbol, StringComparison.Ordinal) ||
                Bars.TimeFrame != TimeFrame.Minute5)
                HaltStartup("host and NAS symbol parameter must be " + RequiredHostSymbol + " M5");

            if (Account.AccountType != AccountType.Hedged)
                HaltStartup("account type must be Hedged; Netted accounts are unsupported");

            ValidateDynamicRiskConfiguration();
            if (string.IsNullOrWhiteSpace(ExpectedAccountCurrency) ||
                !string.Equals(ExpectedAccountCurrency.Trim(),
                    Account.Asset == null ? "" : Account.Asset.Name, StringComparison.OrdinalIgnoreCase))
                HaltStartup("account-currency lock mismatch");

            if (InitialBalance <= 0)
                HaltStartup("initial account balance must be positive");
            else if (RunningMode != RunningMode.RealTime && Math.Abs(Account.Balance - InitialBalance) > InitialCapitalToleranceUsd)
                HaltStartup(string.Format(CultureInfo.InvariantCulture,
                    "backtest starting balance {0:F2} must match configured initial balance {1:F2} within {2:F2}",
                    Account.Balance, InitialBalance, InitialCapitalToleranceUsd));

            // Market contexts are also useful for management-only recovery. A startup entry halt
            // must not prevent an exact, receipt-owned position from being inspected/managed.
            if (_daxSymbol != null && _ukSymbol != null && _nasSymbol != null)
            {
                try
                {
                    _daxM15 = MarketData.GetBars(TimeFrame.Minute15, DaxSymbolName);
                    _ukM15 = MarketData.GetBars(TimeFrame.Minute15, UkSymbolName);
                    _ukH4 = MarketData.GetBars(TimeFrame.Hour4, UkSymbolName);
                    _nasM5 = MarketData.GetBars(TimeFrame.Minute5, NasSymbolName);
                    if (GoldEnabled && _goldSymbol != null)
                        _goldH1 = MarketData.GetBars(TimeFrame.Hour1, GoldSymbolName);
                    LoadMinimumHistory(_daxM15, 4);
                    LoadMinimumHistory(_ukM15, 4);
                    if (UkH4Gate) LoadMinimumHistory(_ukH4, 52);
                    LoadMinimumHistory(_nasM5, 80);
                    if (GoldEnabled) LoadMinimumHistory(_goldH1, GoldMinimumBars);
                    _ukH4Ema = Indicators.ExponentialMovingAverage(_ukH4.ClosePrices, 50);
                    _dataReady = ValidateDataContexts();
                    if (!_dataReady) _dataWarmupSinceUtc = Server.TimeInUtc;
                }
                catch (Exception ex)
                {
                    _dataReady = false;
                    if (_dataWarmupSinceUtc == DateTime.MinValue) _dataWarmupSinceUtc = Server.TimeInUtc;
                    HaltStartup("market-data context initialisation failed: " + ex.Message);
                }
            }

            InitialiseDax();
            InitialiseUk();
            InitialiseNas();
            InitialiseGold();

            if (RunningMode == RunningMode.RealTime)
            {
                LoadDeploymentReceipt();
                ResolveLegacyDeviceOnlyExecutionLatchContext();
                if (!_bindingValid)
                {
                    TryManualExecutionRearm();
                    TryBootstrapFreshExecutionStateForEnrollment();
                }
                if (_bindingValid && _executionStateCreatedFreshAtStartup)
                    EnterExecutionHardHalt("execution_state_missing_for_existing_binding", 0, 0);
                if (_bindingValid) LoadOwnershipLedger();
                Journal(_bindingValid ? "BINDING_LOADED" : "BINDING_REQUIRED",
                    "\"valid\":" + Bool(_bindingValid) + ",\"status\":\"" + Js(_bindingStatus) +
                    "\",\"account_mask\":\"" + Js(MaskedAccountNumber()) + "\"");
                if (_bindingValid)
                {
                    ActivateConfirmedDeployment(false);
                    TryManualExecutionRearm();
                }
                else
                {
                    _deploymentState = _startupHalt ? DeploymentState.Blocked : DeploymentState.SetupRequired;
                    RefreshPreflightBlockers(true);
                    UpdateEnrollmentControl();
                }
            }
            else
            {
                _effectiveOwnerId = "BT311S1";
                _effectiveEpochId = "BT311RUN";
                _effectiveEvaluationStartText = "";
                _bindingValid = true;
                _boundRuntimeIdentityFingerprint = FingerprintText(AccountIdentityText());
                _boundAccountIdentityHash = ReceiptChecksum(AccountIdentityText());
                ActivateConfirmedDeployment(false);
            }

            Timer.Start(TimeSpan.FromSeconds(1));
            Print("{0} | profile={1} stage={2} | host={3} {4} | STATE={5} | ARM={6} | data={7} | halt={8}",
                Version, ProfileCode, Stage, SymbolName, Bars.TimeFrame, _deploymentState, MasterArm, _dataReady,
                _startupHalt ? string.Join(" || ", _startupReasons) : "no");
            Print("LABELS | {0}", string.Join(" | ", MasterLabels));
            Print("RISK | DAX {0}/{1}/{2} | UK {3} | NAS budget {4} trap {5} | GOLD {6} | scale {7:F2} | tier {8}",
                DaxARiskUsd, DaxBRiskUsd, DaxCRiskUsd, UkARiskUsd, EffectiveNasC1DayBudget(), NasReentryRiskUsd,
                GoldEnabled ? GoldRiskUsd : 0, EffectivePortfolioRiskScale(), _currentDynamicRiskTier);
            Print("V34.ARSENAL | S1 Challenge and USD100K locked | binding={0} | proposed Prague start={1}",
                _bindingStatus, _proposedEvaluationStartText);
            Print("CONFIG | id={0} | profile={1} stage=Challenge (deployment identity is not printed)",
                ConfigurationFingerprint(), ProfileCode);
            Print("GATE | daily worst-case ${0:F0} | max-loss safety ${1:F0} | reserve {2:F0}% | max-loss type EOD trailing",
                EffectiveDailyWorstCaseCapUsd(), MaxLossSafetyBufferUsd, ExecutionReservePct);
            Print("FRONTIER | mode {0} | daily budget ${1:F0} = {2:F0}% of the {3:F1}% daily limit | DAX A1 ${4:F0} | NAS C1 ${5:F0} aligned / ${6:F0} misaligned | GOLD ${7:F0} independent",
                FrontierModeText(), FrontierDailyRiskBudgetUsd(), FrontierModeFraction() * 100.0,
                FtmoDailyLossPct, FrontierDailyRiskBudgetUsd() * FrontierDaxShare,
                FrontierDailyRiskBudgetUsd() * FrontierNasBudgetShare * NasAlignedPct / 100.0,
                FrontierDailyRiskBudgetUsd() * FrontierNasBudgetShare * NasMisalignedPct / 100.0,
                FrontierActive() ? InitialBalance * GoldSwingAllocationPct / 100.0 : GoldRiskUsd);

            Journal("BOT_START",
                "\"run\":\"" + _runId + "\",\"stage\":\"" + Stage + "\",\"host\":\"" + Js(SymbolName) + "\",\"host_tf\":\"" + Bars.TimeFrame +
                "\",\"armed\":" + Bool(MasterArm) + ",\"data_ready\":" + Bool(_dataReady) +
                ",\"lease_owned\":" + Bool(_accountLeaseOwned) +
                ",\"deployment_state\":\"" + _deploymentState + "\"" +
                ",\"binding_valid\":" + Bool(_bindingValid) +
                ",\"journal_dead\":" + Bool(_journalDead) +
                ",\"execution_halt\":" + Bool(ExecutionGuardBlocked()) +
                ",\"execution_guard_state\":\"" + ExecutionGuardStateText() + "\"" +
                ",\"execution_guard_reason\":\"" + Js(_executionGuardReason) + "\"" +
                ",\"execution_resume_utc\":\"" + Js(StateDateText(_latencyCooldownUntilUtc)) + "\"" +
                ",\"config_id\":\"" + ConfigurationFingerprint() + "\"" +
                ",\"dynamic_tier\":\"" + Js(_currentDynamicRiskTier) + "\"" +
                ",\"dynamic_scale\":" + Jn(_currentDynamicRiskScale) +
                ",\"dynamic_day_progress_pct\":" + Jn(_dynamicDayProgressPct) +
                ",\"frontier_mode\":\"" + FrontierModeText() + "\"" +
                ",\"frontier_daily_budget\":" + Jn(FrontierDailyRiskBudgetUsd()) +
                ",\"frontier_dax_a1\":" + Jn(FrontierDailyRiskBudgetUsd() * FrontierDaxShare) +
                ",\"frontier_nas_aligned\":" +
                    Jn(FrontierDailyRiskBudgetUsd() * FrontierNasBudgetShare * NasAlignedPct / 100.0) +
                ",\"frontier_nas_misaligned\":" +
                    Jn(FrontierDailyRiskBudgetUsd() * FrontierNasBudgetShare * NasMisalignedPct / 100.0) +
                ",\"frontier_gold\":" + Jn(FrontierActive()
                    ? InitialBalance * GoldSwingAllocationPct / 100.0 : GoldRiskUsd) +
                ",\"internal_daily_cap\":" + Jn(EffectiveDailyWorstCaseCapUsd()) +
                ",\"startup_halt\":" + Bool(_startupHalt) + ",\"halt_reason\":\"" + Js(_haltReason) + "\"");

            UpdateEnrollmentControl();
            DrawMasterHud();
        }

        protected override void OnTimer()
        {
            PruneDirectLimitTerminalExpectations();
            EnforceReadyStateTransition();
            if (!_accountLeaseOwned)
            {
                UpdateEnrollmentControl();
                DrawMasterHud();
                return;
            }

            // Identity must be revalidated before even the management-only branch. This prevents
            // a switched account or recycled broker ID from inheriting close/cancel authority.
            if (RunningMode == RunningMode.RealTime && _bindingValid && !RuntimeIdentityMatchesReceipt())
            {
                _deploymentArmed = false;
                _managementAuthority = false;
                HaltStartup("runtime account identity changed; V34.ARSENAL authorization revoked");
                UpdateEnrollmentControl();
                DrawMasterHud();
                return;
            }

            if (RunningMode == RunningMode.RealTime && !_deploymentArmed)
            {
                if (_managementAuthority)
                {
                    RebuildPragueDay(false);
                    TrackPositionExtremes();
                    AuditOwnedPositionProtection();
                    CheckAccountEmergency();
                    EnforceRestrictedNewsFlatBook();
                    ReconcileBrokerMutationCallbacks();
                    RetryEmergencyCloses();
                    RetryPendingCancels();
                    EnforceEuropeanCrossDayFlatBook();
                    RunDaxTimeBackstop();
                    RunUkTimeBackstop();
                    RunNasTimeChecks();
                    RunGoldTimeBackstop();
                }
                TryAutoRecoverExecutionLatencyCooldown();
                if (!_dataReady && !_startupHalt) _dataReady = ValidateDataContexts();
                RefreshPreflightBlockers(!_bindingValid);
                if (_bindingValid && !_runtimeActivated && !_startupHalt && _preflightBlockers.Count == 0)
                    ActivateConfirmedDeployment(false);
                else if (_executionAutoResumePending && _bindingValid && _runtimeActivated && !_startupHalt &&
                         _executionGuardState == ExecutionGuardState.Armed && _preflightBlockers.Count == 0 &&
                         _deploymentState != DeploymentState.Running)
                {
                    _deploymentState = DeploymentState.Running;
                    _deploymentArmed = true;
                    _executionAutoResumePending = false;
                    Journal("DEPLOYMENT_AUTO_RESUMED",
                        "\"reason\":\"execution guard durably recovered or rearmed\"" +
                        ",\"guard_reason\":\"" + Js(_executionGuardReason) + "\"" +
                        ",\"account_mask\":\"" +
                        Js(MaskedAccountNumber()) + "\"");
                }
                UpdateEnrollmentControl();
                _lastEngineCheckUtc = Server.TimeInUtc;
                DrawMasterHud();
                return;
            }

            CheckExclusiveAccountIntegrity();
            TryAutoRecoverExecutionLatencyCooldown();
            ValidateRestrictedEventCoverage();
            RebuildPragueDay(false);
            if (!_dataReady && !_startupHalt) _dataReady = ValidateDataContexts();
            AuditOwnedPositionProtection();
            CheckAccountEmergency();
            EnforceRestrictedNewsFlatBook();
            ReconcileBrokerMutationCallbacks();
            RetryEmergencyCloses();
            RetryPendingCancels();
            EnforceEuropeanCrossDayFlatBook();
            RunDaxTimeBackstop();
            RunUkTimeBackstop();
            RunNasTimeChecks();
            RunGoldTimeBackstop();
            ProcessProposalRetries();
            RefreshObjectiveStats(false);
            CheckFundedPayoutCompletion();
            CheckObjectiveCompletion();
            _lastEngineCheckUtc = Server.TimeInUtc;
            DrawMasterHud();
        }

        protected override void OnStop()
        {
            UnsubscribeOneClickRearmSafetyObservers();
            RemoveEnrollmentControl();
            ClearDesignObjects();
            Journal("BOT_STOP", "\"balance\":" + Jn(Account.Balance) + ",\"equity\":" + Jn(Account.Equity));
            if (_accountLease != null)
            {
                try { _accountLease.Dispose(); }
                catch { }
                _accountLease = null;
                _accountLeaseOwned = false;
            }
            Print("{0} stopped. Open positions and pending orders are NOT automatically closed by OnStop.", Version);
        }

        protected override void OnException(Exception exception)
        {
            string detail = exception == null ? "unknown" : exception.GetType().Name + ": " + exception.Message;
            if (RunningMode == RunningMode.RealTime)
            {
                try
                {
                    RecordExecutionIncident("unhandled_runtime_exception_" +
                        (exception == null ? "unknown" : exception.GetType().Name), 0, 0);
                }
                catch
                {
                    // The durable hard-halt transition is attempted before its diagnostic journal.
                    // This fallback keeps the in-memory gate closed if even exception reporting fails.
                    _executionGuardState = ExecutionGuardState.HardHalt;
                    _executionGuardReason = "unhandled_runtime_exception_persistence_path_failed";
                    _executionStateSinceUtc = Server.TimeInUtc;
                    _latencyCooldownUntilUtc = DateTime.MinValue;
                    _executionAutoResumePending = false;
                    SuspendDeploymentForExecutionGuard();
                    TryPersistExecutionQualityState();
                    TrySetExecutionFailsafeLatch();
                }
            }
            HaltStartup("unhandled runtime exception: " + detail);
            try { Journal("RUNTIME_EXCEPTION", "\"detail\":\"" + Js(detail) + "\""); }
            catch { }
            Print("*** ATLAS V34.ARSENAL EXCEPTION FAIL-CLOSED: {0} ***", detail);
        }

        private void InitialiseS1TrailingFloor(bool freshEnrollment)
        {
            if (_peakEodInitialised) return;
            _peakEodInitialised = true;
            if (_evaluationStartUtc == DateTime.MinValue && RunningMode == RunningMode.RealTime)
            {
                HaltStartup("S1 trailing floor requires a valid evaluation start date");
                return;
            }
            if (PeakEodBalanceOverride > InitialBalance * 1.5)
            {
                HaltStartup("Peak EOD balance override exceeds the S1 plausibility ceiling of 1.5x initial balance");
                return;
            }

            // S1 V25 invariant: the EOD-trailing maximum-loss floor may move up, never down.
            InitialisePeakEodStatePath();
            if (_startupHalt) return;
            double persistedPeak = LoadPersistedPeakEodBalance(freshEnrollment);
            if (_startupHalt) return;
            double reconstructedPeak = ReconstructPeakEodBalance();
            _peakEodBalance = Math.Max(
                Math.Max(InitialBalance, reconstructedPeak),
                Math.Max(persistedPeak, PeakEodBalanceOverride > 0 ? PeakEodBalanceOverride : 0));
            Print("S1 peak-EOD resolve: reconstructed ${0:F2} | persisted ${1:F2} | override ${2:F2} | ADOPTED ${3:F2} (max-loss floor ${4:F2})",
                reconstructedPeak, persistedPeak, PeakEodBalanceOverride, _peakEodBalance,
                _peakEodBalance - InitialBalance * FtmoMaxLossPct / 100.0);
            if (RunningMode == RunningMode.RealTime &&
                Math.Abs(_historyReconstructedBalance - Account.Balance) > Math.Max(10, InitialBalance * 0.001))
                HaltStartup("broker history does not reconcile to current balance; review the proposed evaluation start and Peak EOD override");
            if (!_startupHalt) PersistPeakEodBalance();
        }

        private void ActivateConfirmedDeployment(bool freshEnrollment)
        {
            if (_runtimeActivated || !_bindingValid) return;
            _deploymentState = DeploymentState.Activating;
            _deploymentArmed = false;
            ParseEvaluationStart();
            ValidateRestrictedEventCoverage();

            if (_evaluationStartUtc != DateTime.MinValue)
            {
                double reconstructedInitial = ReconstructedStageInitialBalance();
                if (Math.Abs(InitialBalance - reconstructedInitial) > InitialCapitalToleranceUsd)
                    HaltStartup(string.Format(CultureInfo.InvariantCulture,
                        "stage initial capital mismatch: locked {0:F2}, reconstructed {1:F2}; receipt/date/history review required",
                        InitialBalance, reconstructedInitial));
            }

            bool identityAuthorized = RunningMode != RunningMode.RealTime ||
                (_accountLeaseOwned && RuntimeIdentityMatchesReceipt());
            if (!identityAuthorized)
            {
                _deploymentState = DeploymentState.Blocked;
                if (freshEnrollment) UpdateEnrollmentControl();
                return;
            }

            InitialiseS1TrailingFloor(freshEnrollment);
            RefreshPreflightBlockers(freshEnrollment);
            bool entryPreflightPassed = !_startupHalt && _preflightBlockers.Count == 0;

            // Fresh enrollment remains broker-read-only until every S1 safety check passes.
            if (freshEnrollment && !entryPreflightPassed)
            {
                _deploymentState = DeploymentState.Blocked;
                UpdateEnrollmentControl();
                return;
            }

            // An existing exact receipt may establish management authority even while entries are
            // blocked, so exact-owned exposure can still be reconciled and protected fail-closed.
            if (!_managementAuthority)
            {
                if (!_ownershipLedgerLoaded && !_ownershipLedgerInvalid)
                    LoadOwnershipLedger();
                if (_ownershipLedgerInvalid)
                {
                    HaltStartup("ownership ledger invalid: " + _ownershipLedgerStatus +
                        "; all exposure is preserved for manual review");
                    _deploymentState = DeploymentState.Blocked;
                    UpdateEnrollmentControl();
                    return;
                }
                _managementAuthority = true;
                SubscribeRuntimeEvents();
                RebuildPragueDay(true);
                ReconcileStartupExposure();
                if (_peakEodSafetyFailed)
                {
                    CancelAllMasterPending();
                    CloseAllMasterPositions("s1_peak_eod_state_invalid");
                }
            }

            RefreshPreflightBlockers(false);
            if (_startupHalt || _preflightBlockers.Count > 0)
            {
                _deploymentState = DeploymentState.Blocked;
                UpdateEnrollmentControl();
                return;
            }

            RebuildPragueDay(true);
            SeedModuleMonitors();
            RefreshObjectiveStats(true);
            _stateRebuildMode = true;
            ProcessDaxClosedBars();
            ProcessUkClosedBars();
            ProcessGoldClosedBars();
            _stateRebuildMode = false;
            RebuildDaxCurrentDayGuards();
            MarkNasDecisionMissedBeforeArm();

            _runtimeActivated = true;
            _deploymentArmed = true;
            _deploymentState = DeploymentState.Running;
            _executionAutoResumePending = false;
            RemoveEnrollmentControl();
            CheckAccountEmergency();
            CheckFundedPayoutCompletion();
            CheckObjectiveCompletion();
            List<string> postActivationBlockers = GetGlobalEntryBlockers();
            if (postActivationBlockers.Count > 0)
            {
                Journal("DEPLOYMENT_BOUND_ENTRY_BLOCKED",
                    "\"fresh_enrollment\":" + Bool(freshEnrollment) +
                    ",\"account_mask\":\"" + Js(MaskedAccountNumber()) + "\"" +
                    ",\"blockers\":\"" + Js(string.Join(" || ", postActivationBlockers)) + "\"");
                Print("*** ATLAS V34.ARSENAL S1 BOUND; NEW ENTRIES BLOCKED | {0} ***",
                    string.Join(" || ", postActivationBlockers));
                return;
            }
            Journal("DEPLOYMENT_READY",
                "\"fresh_enrollment\":" + Bool(freshEnrollment) +
                ",\"account_mask\":\"" + Js(MaskedAccountNumber()) + "\"" +
                ",\"evaluation_start\":\"" + Js(EvaluationStartText) + "\"" +
                ",\"owner\":\"" + Js(DeploymentOwnerId) + "\",\"epoch\":\"" + Js(StageEpochId) + "\"");
            Print("*** ATLAS V34.ARSENAL GLOBAL SAFETY READY | {0} | S1 CHALLENGE | USD100K | start {1} | proposal quote/spread/capacity checks remain active ***",
                MaskedAccountNumber(), EvaluationStartText);
        }

        private void SubscribeRuntimeEvents()
        {
            if (_runtimeEventsSubscribed) return;
            Positions.Opened += OnPositionOpened;
            Positions.Closed += OnPositionClosed;
            PendingOrders.Filled += OnPendingFilled;
            PendingOrders.Cancelled += OnPendingCancelled;
            if (_daxM15 != null)
            {
                _daxM15.BarOpened += _ => { if (_deploymentArmed) ProcessDaxClosedBars(); };
                _daxM15.Tick += _ => OnMarketPulse("DAX");
            }
            if (_ukM15 != null)
            {
                _ukM15.BarOpened += _ => { if (_deploymentArmed) ProcessUkClosedBars(); };
                _ukM15.Tick += _ => OnMarketPulse("UK");
            }
            if (_nasM5 != null)
            {
                _nasM5.BarOpened += _ => OnNasM5BarOpened();
                _nasM5.Tick += _ => OnMarketPulse("NAS");
            }
            if (_goldH1 != null)
            {
                _goldH1.BarOpened += _ => { if (_deploymentArmed) ProcessGoldClosedBars(); };
                _goldH1.Tick += _ => OnMarketPulse("GOLD");
            }
            _runtimeEventsSubscribed = true;
        }

        private void SubscribeOneClickRearmSafetyObservers()
        {
            if (RunningMode != RunningMode.RealTime || _oneClickRearmSafetyObserversSubscribed) return;
            Positions.Opened += OnRearmSafetyPositionOpened;
            Positions.Closed += OnRearmSafetyPositionClosed;
            PendingOrders.Created += OnRearmSafetyPendingCreated;
            PendingOrders.Cancelled += OnRearmSafetyPendingCancelled;
            PendingOrders.Filled += OnRearmSafetyPendingFilled;
            _oneClickRearmSafetyObserversSubscribed = true;
        }

        private void UnsubscribeOneClickRearmSafetyObservers()
        {
            if (!_oneClickRearmSafetyObserversSubscribed) return;
            try { Positions.Opened -= OnRearmSafetyPositionOpened; }
            catch { }
            try { Positions.Closed -= OnRearmSafetyPositionClosed; }
            catch { }
            try { PendingOrders.Created -= OnRearmSafetyPendingCreated; }
            catch { }
            try { PendingOrders.Cancelled -= OnRearmSafetyPendingCancelled; }
            catch { }
            try { PendingOrders.Filled -= OnRearmSafetyPendingFilled; }
            catch { }
            _oneClickRearmSafetyObserversSubscribed = false;
        }

        private void OnRearmSafetyPositionOpened(PositionOpenedEventArgs args)
        {
            AdvanceOneClickRearmSafetyContext();
        }

        private void OnRearmSafetyPositionClosed(PositionClosedEventArgs args)
        {
            AdvanceOneClickRearmSafetyContext();
        }

        private void OnRearmSafetyPendingCreated(PendingOrderCreatedEventArgs args)
        {
            AdvanceOneClickRearmSafetyContext();
        }

        private void OnRearmSafetyPendingCancelled(PendingOrderCancelledEventArgs args)
        {
            AdvanceOneClickRearmSafetyContext();
        }

        private void OnRearmSafetyPendingFilled(PendingOrderFilledEventArgs args)
        {
            AdvanceOneClickRearmSafetyContext();
        }

        private void AdvanceOneClickRearmSafetyContext()
        {
            unchecked { _oneClickRearmSafetyContextSequence++; }
            InvalidateOneClickExecutionRearmReview();
        }

        private string ProposedEvaluationStartText()
        {
            if (_prague == null) return "";
            DateTime createdUtc = NormalizedAccountCreationUtc();
            if (createdUtc == DateTime.MinValue || createdUtc > Server.TimeInUtc.AddMinutes(5)) return "";
            Print("V34.ARSENAL DATE PROPOSAL | raw={0:o} kind={1} | normalized_utc={2:o} | Prague={3:yyyy-MM-dd}",
                Account.CreationTime, Account.CreationTime.Kind, createdUtc, PragueDate(createdUtc));
            return PragueDate(createdUtc).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private DateTime NormalizedAccountCreationUtc()
        {
            DateTime created = Account.CreationTime;
            if (created == DateTime.MinValue) return DateTime.MinValue;
            if (created.Kind == DateTimeKind.Utc) return created;
            if (created.Kind == DateTimeKind.Local) return created.ToUniversalTime();
            return DateTime.SpecifyKind(created, DateTimeKind.Utc);
        }

        private string AccountIdentityText()
        {
            DateTime createdUtc = NormalizedAccountCreationUtc();
            return Account.Number.ToString(CultureInfo.InvariantCulture) + "|" +
                Account.UserId.ToString(CultureInfo.InvariantCulture) + "|" +
                (Account.BrokerName ?? "").Trim() + "|" + Bool(Account.IsLive) + "|" +
                Account.AccountType + "|" + (Account.Asset == null ? "" : Account.Asset.Name) + "|" +
                createdUtc.ToString("o", CultureInfo.InvariantCulture);
        }

        private bool RuntimeIdentityFingerprintMatches()
        {
            if (RunningMode != RunningMode.RealTime) return true;
            return _bindingValid && !string.IsNullOrEmpty(_boundRuntimeIdentityFingerprint) &&
                string.Equals(_boundRuntimeIdentityFingerprint, FingerprintText(AccountIdentityText()), StringComparison.Ordinal);
        }

        private bool RuntimeIdentityMatchesReceipt()
        {
            if (RunningMode != RunningMode.RealTime) return true;
            if (!RuntimeIdentityFingerprintMatches() || string.IsNullOrEmpty(_boundAccountIdentityHash)) return false;
            return string.Equals(_boundAccountIdentityHash, ReceiptChecksum(AccountIdentityText()), StringComparison.Ordinal);
        }

        private bool BindingAuthorityCurrent()
        {
            return _managementAuthority && _bindingValid && _accountLeaseOwned && RuntimeIdentityMatchesReceipt();
        }

        private string ReceiptPayload(string owner, string epoch, string evaluationStart, string confirmedUtc)
        {
            return "schema=" + ReceiptSchema +
                "|build=" + ReceiptBuild +
                "|profile=" + ProfileCode +
                "|stage=Challenge" +
                "|account=" + Account.Number.ToString(CultureInfo.InvariantCulture) +
                "|identity=" + ReceiptChecksum(AccountIdentityText()) +
                "|currency=" + (Account.Asset == null ? "" : Account.Asset.Name) +
                "|initial=" + InitialBalance.ToString("F2", CultureInfo.InvariantCulture) +
                "|eval=" + evaluationStart +
                "|owner=" + owner +
                "|epoch=" + epoch +
                "|strategy=" + StrategyConfigurationReceiptHash() +
                "|confirmed=" + confirmedUtc;
        }

        private string SignedReceipt(string owner, string epoch, string evaluationStart, string confirmedUtc)
        {
            string payload = ReceiptPayload(owner, epoch, evaluationStart, confirmedUtc);
            return payload + "|checksum=" + ReceiptChecksum(payload);
        }

        private void LoadDeploymentReceipt()
        {
            _bindingValid = false;
            _effectiveAccountNumber = Account.Number.ToString(CultureInfo.InvariantCulture);
            _effectiveOwnerId = "";
            _effectiveEpochId = "";
            _effectiveEvaluationStartText = _proposedEvaluationStartText;
            string identityHash = ReceiptChecksum(AccountIdentityText()).Substring(0, 16);
            _receiptKey = "Atlas3 S1 V311 C " + identityHash;
            _receiptPendingKey = _receiptKey + " Pending";

            if (!IsValidLocalStorageKey(_receiptKey) ||
                !IsValidLocalStorageKey(_receiptPendingKey))
            {
                _bindingStatus = "invalid deployment receipt local-storage key";
                HaltStartup(_bindingStatus);
                return;
            }

            string receipt;
            try
            {
                receipt = GetLocalStorageStringChecked(_receiptKey, LocalStorageScope.Type) ?? "";
            }
            catch (Exception ex)
            {
                _bindingStatus = "storage unavailable: " + ex.Message;
                return;
            }

            if (string.IsNullOrWhiteSpace(receipt))
            {
                string pending = "";
                try { pending = GetLocalStorageStringChecked(_receiptPendingKey, LocalStorageScope.Type) ?? ""; }
                catch { }
                _bindingStatus = string.IsNullOrWhiteSpace(pending)
                    ? "first-run confirmation required"
                    : "interrupted prior confirmation detected; committed receipt is absent";
                return;
            }

            Dictionary<string, string> values;
            string receiptError;
            if (!TryParseReceipt(receipt, out values, out receiptError))
            {
                _bindingStatus = "invalid receipt: " + receiptError;
                return;
            }

            var mismatches = new List<string>();
            ReceiptMatch(values, "schema", ReceiptSchema, mismatches);
            ReceiptMatch(values, "build", ReceiptBuild, mismatches);
            ReceiptMatch(values, "profile", ProfileCode, mismatches);
            ReceiptMatch(values, "stage", "Challenge", mismatches);
            ReceiptMatch(values, "account", Account.Number.ToString(CultureInfo.InvariantCulture), mismatches);
            ReceiptMatch(values, "identity", ReceiptChecksum(AccountIdentityText()), mismatches);
            ReceiptMatch(values, "currency", Account.Asset == null ? "" : Account.Asset.Name, mismatches);
            ReceiptMatch(values, "initial", InitialBalance.ToString("F2", CultureInfo.InvariantCulture), mismatches);
            ReceiptMatch(values, "strategy", StrategyConfigurationReceiptHash(), mismatches);

            string owner = ReceiptValue(values, "owner");
            string epoch = ReceiptValue(values, "epoch");
            string evaluationStart = ReceiptValue(values, "eval");
            if (!ValidIdentityToken(owner)) mismatches.Add("owner token");
            if (!ValidIdentityToken(epoch)) mismatches.Add("epoch token");
            DateTime parsed;
            if (!DateTime.TryParseExact(evaluationStart, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out parsed)) mismatches.Add("evaluation start");

            if (mismatches.Count > 0)
            {
                _bindingStatus = "receipt mismatch: " + string.Join(", ", mismatches);
                return;
            }

            _effectiveOwnerId = owner;
            _effectiveEpochId = epoch;
            _effectiveEvaluationStartText = evaluationStart;
            _receiptConfirmedUtc = ReceiptValue(values, "confirmed");
            _boundAccountIdentityHash = ReceiptValue(values, "identity");
            _boundRuntimeIdentityFingerprint = FingerprintText(AccountIdentityText());
            _bindingValid = true;
            _bindingStatus = "exact account/config receipt";
        }

        private bool TryParseReceipt(string receipt, out Dictionary<string, string> values, out string error)
        {
            values = new Dictionary<string, string>(StringComparer.Ordinal);
            error = "";
            string marker = "|checksum=";
            int checksumIndex = receipt.LastIndexOf(marker, StringComparison.Ordinal);
            if (checksumIndex <= 0)
            {
                error = "checksum missing";
                return false;
            }
            string payload = receipt.Substring(0, checksumIndex);
            string checksum = receipt.Substring(checksumIndex + marker.Length);
            if (!string.Equals(checksum, ReceiptChecksum(payload), StringComparison.Ordinal))
            {
                error = "checksum failed";
                return false;
            }
            foreach (string item in payload.Split('|'))
            {
                int split = item.IndexOf('=');
                if (split <= 0 || split == item.Length - 1)
                {
                    error = "malformed field";
                    return false;
                }
                string key = item.Substring(0, split);
                if (values.ContainsKey(key))
                {
                    error = "duplicate field " + key;
                    return false;
                }
                values[key] = item.Substring(split + 1);
            }
            return true;
        }

        private static string ReceiptValue(Dictionary<string, string> values, string key)
        {
            string value;
            return values.TryGetValue(key, out value) ? value : "";
        }

        private static void ReceiptMatch(Dictionary<string, string> values, string key, string expected, List<string> mismatches)
        {
            if (!string.Equals(ReceiptValue(values, key), expected, StringComparison.Ordinal))
                mismatches.Add(key);
        }

        private void RefreshPreflightBlockers(bool requireFlat)
        {
            _preflightBlockers.Clear();
            foreach (string reason in _startupReasons) AddPreflightBlocker(reason);
            if (!_accountLeaseOwned) AddPreflightBlocker("another local Atlas master owns this account");
            if (_daxSymbol == null || _ukSymbol == null || _nasSymbol == null)
                AddPreflightBlocker("required GER40/UK100/US100 symbols are unavailable");
            if (!string.Equals(NasSymbolName, RequiredHostSymbol, StringComparison.Ordinal) ||
                !string.Equals(SymbolName, RequiredHostSymbol, StringComparison.Ordinal) ||
                Bars.TimeFrame != TimeFrame.Minute5)
                AddPreflightBlocker("attach to " + RequiredHostSymbol + " M5 and restore the NAS symbol parameter");
            if (Account.AccountType != AccountType.Hedged)
                AddPreflightBlocker("account type must be Hedged; Netted accounts are unsupported");
            if (!string.Equals(Account.Asset == null ? "" : Account.Asset.Name, "USD", StringComparison.OrdinalIgnoreCase))
                AddPreflightBlocker("account currency must be USD");
            if (!_dataReady) AddPreflightBlocker(DataWarmupDetail());
            if (RunningMode == RunningMode.RealTime)
            {
                if (!_restrictedEventsParseValid) AddPreflightBlocker("restricted-news calendar has a malformed timestamp");
                if (FundedNewsGuard && _restrictedEvents.Count == 0)
                    AddPreflightBlocker("restricted-news guard requires at least one event");
                if (FundedNewsGuard && _restrictedEventsVerifiedThroughUtc < Server.TimeInUtc.Date.AddDays(8))
                    AddPreflightBlocker("news calendar coverage is less than seven days ahead");
                if (NewsBlocked()) AddPreflightBlocker("currently inside a restricted-news window");
                if (_executionGuardState == ExecutionGuardState.LatencyCooldown)
                    AddPreflightBlocker("execution latency cooldown until " +
                        StateDateText(_latencyCooldownUntilUtc) + " (automatic recovery requires a flat account)");
                else if (_executionGuardState == ExecutionGuardState.HardHalt)
                    AddPreflightBlocker("execution-quality HARD HALT requires deliberate rearm: " + _executionGuardReason);
                if (JournalOn && _journalDead) AddPreflightBlocker("journal HALT");
                if (JournalOn && !ProbeJournalWritable()) AddPreflightBlocker("journal path is not writable: " + _journalProbeError);
                if (requireFlat && (Positions.Count > 0 || PendingOrders.Count > 0))
                    AddPreflightBlocker("first enrollment/rebinding requires zero positions and zero pending orders");

                string candidate = _bindingValid ? _effectiveEvaluationStartText : _proposedEvaluationStartText;
                DateTime parsed;
                if (!DateTime.TryParseExact(candidate, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out parsed) || _prague == null)
                {
                    AddPreflightBlocker("account creation date could not produce a Prague stage-start proposal");
                }
                else
                {
                    try
                    {
                        DateTime pragueMidnight = DateTime.SpecifyKind(parsed.Date, DateTimeKind.Unspecified);
                        DateTime candidateUtc = TimeZoneInfo.ConvertTimeToUtc(pragueMidnight, _prague);
                        double realised = History.Where(x => x.ClosingTime >= candidateUtc).Sum(x => x.NetProfit);
                        double reconstructedInitial = Account.Balance - realised - VerifiedCashAdjustmentsUsd;
                        if (Math.Abs(InitialBalance - reconstructedInitial) > InitialCapitalToleranceUsd)
                            AddPreflightBlocker(string.Format(CultureInfo.InvariantCulture,
                                "proposed start {0} reconstructs ${1:F2}, not locked ${2:F2}",
                                candidate, reconstructedInitial, InitialBalance));
                    }
                    catch { AddPreflightBlocker("Prague stage-start conversion failed"); }
                }
            }

            string signature = _preflightBlockers.Count + "|" + string.Join("|", _preflightBlockers);
            if (!string.Equals(signature, _lastPreflightSignature, StringComparison.Ordinal))
            {
                _lastPreflightSignature = signature;
                Print("ATLAS PREFLIGHT | blockers={0} | {1}", _preflightBlockers.Count,
                    _preflightBlockers.Count == 0 ? "PASS" : string.Join(" || ", _preflightBlockers));
                Journal("PREFLIGHT_SUMMARY", "\"pass\":" + Bool(_preflightBlockers.Count == 0) +
                    ",\"blocker_count\":" + _preflightBlockers.Count +
                    ",\"blockers\":\"" + Js(string.Join(" || ", _preflightBlockers)) + "\"");
            }
        }

        private void AddPreflightBlocker(string reason)
        {
            if (!string.IsNullOrWhiteSpace(reason) && !_preflightBlockers.Contains(reason))
                _preflightBlockers.Add(reason);
        }

        private bool ProbeJournalWritable()
        {
            if (_journalProbeChecked) return _journalProbeOk;
            _journalProbeChecked = true;
            try
            {
                System.IO.Directory.CreateDirectory(_journalDir);
                string path = System.IO.Path.Combine(_journalDir, "Atlas3_v34_arsenal_write_probe_" + _runId + ".tmp");
                WriteTextAtomically(path, "probe");
                System.IO.File.Delete(path);
                _journalProbeOk = true;
            }
            catch (Exception ex)
            {
                _journalProbeOk = false;
                _journalProbeError = ex.Message;
            }
            return _journalProbeOk;
        }

        private string DataWarmupDetail()
        {
            if (_daxM15 == null || _ukM15 == null || _ukH4 == null || _nasM5 == null)
                return "market-data contexts unavailable";
            return string.Format(CultureInfo.InvariantCulture,
                "data warming: DAX M15={0}, UK M15={1}, UK H4={2}, NAS M5={3}, GOLD H1={4}",
                _daxM15.Count, _ukM15.Count, _ukH4.Count, _nasM5.Count,
                _goldH1 == null ? 0 : _goldH1.Count);
        }

        private bool CanOfferOneClickExecutionRearm(out string reason)
        {
            reason = "";
            if (RunningMode != RunningMode.RealTime)
            {
                reason = "real-time mode is required";
                return false;
            }
            if (_executionGuardState != ExecutionGuardState.HardHalt)
            {
                reason = "execution guard is not in HARD HALT";
                return false;
            }
            if (!_accountLeaseOwned)
            {
                reason = "this instance does not own the account lease";
                return false;
            }
            if (_startupHalt)
            {
                reason = "another startup safety failure must be resolved first";
                return false;
            }
            if (!ExecutionRearmJournalHealthy())
            {
                reason = "the daily safety journal must be enabled, writable and clear of retry failures";
                return false;
            }
            if (_ownershipLedgerInvalid)
            {
                reason = "the ownership ledger is invalid";
                return false;
            }
            if (string.IsNullOrWhiteSpace(_executionStatePath) ||
                string.IsNullOrWhiteSpace(_executionFailsafeKey) ||
                string.IsNullOrWhiteSpace(_executionFailsafeDeviceKey) ||
                !IsValidLocalStorageKey(_executionFailsafeKey) ||
                !IsValidLocalStorageKey(_executionFailsafeDeviceKey))
            {
                reason = "execution-state storage is not ready";
                return false;
            }
            if (_bindingValid)
            {
                if (!RuntimeIdentityMatchesReceipt())
                {
                    reason = "the deployment receipt does not match this account";
                    return false;
                }
                if (!_managementAuthority || !_ownershipLedgerLoaded || _ownershipLedger.Count != 0 ||
                    !string.Equals(_ownershipLedgerStatus, "valid", StringComparison.Ordinal))
                {
                    reason = "the bound ownership ledger is not reconciled, valid and empty";
                    return false;
                }
            }
            else if (!string.Equals(_bindingStatus, "first-run confirmation required", StringComparison.Ordinal))
            {
                reason = "the deployment receipt needs review before rearm";
                return false;
            }
            if (Positions.Any() || PendingOrders.Any())
            {
                reason = "zero positions and zero pending orders are required";
                return false;
            }
            if (!ExecutionGuardBookkeepingDrained())
            {
                reason = "execution bookkeeping has not completely drained";
                return false;
            }
            if (OneClickRearmHasStorageIntegrityBlocker())
            {
                reason = "execution-state storage integrity needs log review";
                return false;
            }
            return true;
        }

        private bool ExecutionRearmJournalHealthy()
        {
            return JournalOn && !_journalDead && _journalProbeChecked && _journalProbeOk &&
                _journalFailures == 0 && _journalRetryAt <= Server.TimeInUtc;
        }

        private bool OneClickRearmHasStorageIntegrityBlocker()
        {
            return _executionStorageIntegrityBlocker ||
                IsExecutionStorageIntegrityReason(_executionGuardReason);
        }

        private static bool IsExecutionStorageIntegrityReason(string value)
        {
            string reason = value ?? "";
            return reason.StartsWith("execution_failsafe_storage_", StringComparison.Ordinal) ||
                reason.StartsWith("execution_state_persistence_failed_", StringComparison.Ordinal) ||
                reason.StartsWith("execution_state_read_failed_", StringComparison.Ordinal) ||
                reason.StartsWith("execution_state_invalid_", StringComparison.Ordinal) ||
                reason.StartsWith("execution_state_excess_samples", StringComparison.Ordinal) ||
                reason.StartsWith("execution_state_clock_or_resume_invalid", StringComparison.Ordinal) ||
                reason.StartsWith("execution_rearm_nonce_read_failed", StringComparison.Ordinal) ||
                reason.StartsWith("execution_rearm_nonce_persistence_failed", StringComparison.Ordinal) ||
                reason.StartsWith("execution_rearm_commit_failed", StringComparison.Ordinal) ||
                reason.StartsWith("execution_rearm_journal_commit_failed", StringComparison.Ordinal) ||
                reason.StartsWith("fresh_install_execution_bootstrap_failed", StringComparison.Ordinal) ||
                reason.StartsWith("execution_guard_clock_regression", StringComparison.Ordinal) ||
                reason.StartsWith("execution_state_missing_for_existing_binding", StringComparison.Ordinal);
        }

        private string OneClickExecutionRearmReviewSignature()
        {
            return ExecutionGuardStateText() + "|" + SanitizeExecutionReason(_executionGuardReason) +
                "|" + StateDateText(_executionStateSinceUtc) +
                "|lease=" + Bool(_accountLeaseOwned) +
                "|startup=" + Bool(_startupHalt) +
                "|binding=" + Bool(_bindingValid) +
                "|binding_status=" + (_bindingStatus ?? "") +
                "|management_authority=" + Bool(_managementAuthority) +
                "|runtime_identity=" + FingerprintText(AccountIdentityText()) +
                "|ledger_loaded=" + Bool(_ownershipLedgerLoaded) +
                "|ledger_invalid=" + Bool(_ownershipLedgerInvalid) +
                "|ledger_status=" + (_ownershipLedgerStatus ?? "") +
                "|ledger_count=" + _ownershipLedger.Count.ToString(CultureInfo.InvariantCulture) +
                "|positions=" + Positions.Count.ToString(CultureInfo.InvariantCulture) +
                "|orders=" + PendingOrders.Count.ToString(CultureInfo.InvariantCulture) +
                "|bookkeeping=" + Bool(ExecutionGuardBookkeepingDrained()) +
                "|journal_on=" + Bool(JournalOn) +
                "|journal_dead=" + Bool(_journalDead) +
                "|journal_probe=" + Bool(_journalProbeChecked && _journalProbeOk) +
                "|journal_failures=" + _journalFailures.ToString(CultureInfo.InvariantCulture) +
                "|journal_retry_at=" + StateDateText(_journalRetryAt) +
                "|journal_retry_active=" + Bool(_journalRetryAt > Server.TimeInUtc) +
                "|storage_integrity=" + Bool(!_executionStorageIntegrityBlocker) +
                "|legacy_device_bridge=" + Bool(_legacyDeviceOnlyExecutionLatch) +
                "|book_event_seq=" + _oneClickRearmSafetyContextSequence.ToString(CultureInfo.InvariantCulture);
        }

        private void InvalidateOneClickExecutionRearmReview()
        {
            _oneClickRearmReviewStartedTicks = 0;
            _oneClickRearmReviewSignature = "";
        }

        private void OnOneClickExecutionRearmClicked()
        {
            if (_confirmClickBusy || _executionRearmTransactionBusy) return;

            string refusal;
            bool eligible = CanOfferOneClickExecutionRearm(out refusal);
            string currentSignature = eligible ? OneClickExecutionRearmReviewSignature() : "";
            bool contextUnchanged = eligible &&
                !string.IsNullOrEmpty(_oneClickRearmReviewSignature) &&
                string.Equals(currentSignature, _oneClickRearmReviewSignature, StringComparison.Ordinal);
            double elapsedSeconds = _oneClickRearmReviewStartedTicks <= 0
                ? 0
                : MonotonicElapsedMilliseconds(_oneClickRearmReviewStartedTicks) / 1000.0;
            if (!eligible || !contextUnchanged || _oneClickRearmReviewStartedTicks <= 0 ||
                elapsedSeconds < OneClickRearmReviewSeconds)
            {
                _oneClickRearmReviewStartedTicks = 0;
                _oneClickRearmReviewSignature = "";
                Print("ATLAS ONE-CLICK SAFE REARM REFUSED: {0}", !eligible
                    ? refusal
                    : (!contextUnchanged
                        ? "the safety context changed; a new five-second review is required"
                        : "the uninterrupted five-second safety review is incomplete"));
                UpdateEnrollmentControl();
                DrawMasterHud();
                return;
            }

            _confirmClickBusy = true;
            _oneClickRearmReviewStartedTicks = 0;
            _oneClickRearmReviewSignature = "";
            try
            {
                long clickTicks = Stopwatch.GetTimestamp();
                string nonce = BuildOneClickRearmNonce(
                    ProfileCode, _runId, clickTicks, Guid.NewGuid().ToString("N"));
                bool rearmed = TryExecutionRearmTransaction(nonce, "hud_one_click");
                RefreshPreflightBlockers(!_bindingValid);
                if (rearmed)
                    Print("ATLAS ONE-CLICK SAFE REARM COMMITTED. Enrollment, if required, remains a separate action.");
            }
            finally
            {
                _confirmClickBusy = false;
                UpdateEnrollmentControl();
                DrawMasterHud();
            }
        }

        private static string BuildOneClickRearmNonce(
            string profile, string runId, long monotonicTicks, string entropy)
        {
            string material = (profile ?? "") + "|" + (runId ?? "") + "|" +
                monotonicTicks.ToString(CultureInfo.InvariantCulture) + "|" + (entropy ?? "");
            return "HUD-" + ReceiptChecksum(material).Substring(0, 48);
        }

        private void UpdateEnrollmentControl()
        {
            if (RunningMode != RunningMode.RealTime || Chart == null || _deploymentArmed)
            {
                RemoveEnrollmentControl();
                return;
            }

            // The existing deployment control has exactly one contextual action. A hard halt is
            // never cleared by rendering, a timer, startup or enrollment confirmation. Rendering
            // only maintains the monotonic review window; the button click is the sole mutation.
            if (_executionGuardState == ExecutionGuardState.HardHalt)
            {
                _enrollmentReviewAvailableUtc = DateTime.MinValue;
                string refusal;
                bool eligible = CanOfferOneClickExecutionRearm(out refusal) &&
                    !_confirmClickBusy && !_executionRearmTransactionBusy;
                if (!eligible)
                {
                    _oneClickRearmReviewStartedTicks = 0;
                    _oneClickRearmReviewSignature = "";
                }
                else
                {
                    string currentSignature = OneClickExecutionRearmReviewSignature();
                    if (_oneClickRearmReviewStartedTicks <= 0 ||
                        !string.Equals(currentSignature, _oneClickRearmReviewSignature, StringComparison.Ordinal))
                    {
                        _oneClickRearmReviewSignature = currentSignature;
                        _oneClickRearmReviewStartedTicks = Stopwatch.GetTimestamp();
                    }
                }

                double elapsedSeconds = _oneClickRearmReviewStartedTicks <= 0
                    ? 0
                    : MonotonicElapsedMilliseconds(_oneClickRearmReviewStartedTicks) / 1000.0;
                int rearmReviewSeconds = eligible
                    ? Math.Max(0, (int)Math.Ceiling(OneClickRearmReviewSeconds - elapsedSeconds))
                    : 0;
                bool rearmable = eligible && elapsedSeconds >= OneClickRearmReviewSeconds;
                string rearmAccountReview = "S1 CHALLENGE / " + (Account.IsLive ? "LIVE" : "DEMO") +
                    " / " + MaskedAccountNumber();
                string rearmText = rearmable
                    ? "[AMBER] REVIEWED & FLAT — CLICK SAFE REARM\n" +
                      rearmAccountReview + " | Rearm only; enrollment stays separate"
                    : (eligible
                        ? "[BLUE] SAFETY REVIEW — " + rearmAccountReview + " (" + rearmReviewSeconds + "s)\n" +
                          "Confirm zero positions/orders; then click SAFE REARM"
                        : "[RED] SAFE REARM BLOCKED — " + DesignTrim(refusal, 82) +
                          "\nSave the log and resolve this blocker; do not bypass");
                RenderEnrollmentControl(rearmText, rearmable,
                    rearmable ? DesignWarning : (eligible ? DesignInfo : DesignDanger));
                return;
            }

            _oneClickRearmReviewStartedTicks = 0;
            _oneClickRearmReviewSignature = "";
            if (_bindingValid)
            {
                RemoveEnrollmentControl();
                return;
            }
            bool preflightGreen = !_startupHalt && _accountLeaseOwned && _preflightBlockers.Count == 0 && !_confirmClickBusy;
            if (!preflightGreen)
                _enrollmentReviewAvailableUtc = DateTime.MinValue;
            else if (_enrollmentReviewAvailableUtc == DateTime.MinValue)
                _enrollmentReviewAvailableUtc = Server.TimeInUtc.AddSeconds(5);
            int reviewSeconds = _enrollmentReviewAvailableUtc == DateTime.MinValue
                ? 0
                : Math.Max(0, (int)Math.Ceiling((_enrollmentReviewAvailableUtc - Server.TimeInUtc).TotalSeconds));
            bool confirmable = preflightGreen && reviewSeconds == 0;
            string accountReview = "S1 CHALLENGE / " + (Account.IsLive ? "LIVE" : "DEMO") +
                " / " + MaskedAccountNumber();
            string text = confirmable
                ? "[GREEN] CONFIRM & ARM — " + accountReview +
                  "\nUS100.cash M5 | USD100K | START " + _proposedEvaluationStartText
                : (preflightGreen
                    ? "[BLUE] REVIEW — " + accountReview + " (" + reviewSeconds + "s)" +
                      "\nCheck profile, account and start date"
                    : "[RED] SETUP BLOCKED — " +
                      DesignBlockerText(DesignPriorityBlocker(_preflightBlockers)) +
                      "\nSee NEXT ACTION on the ATLAS HUD");
            RenderEnrollmentControl(text, confirmable,
                confirmable ? DesignHealthy : (preflightGreen ? DesignInfo : DesignDanger));
        }

        private void RenderEnrollmentControl(string text, bool enabled, Color color)
        {
            if (_confirmArmButton == null)
            {
                _confirmArmButton = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = 20,
                    Padding = 12
                };
                _confirmArmButton.Click += OnConfirmArmClicked;
                Chart.AddControl(_confirmArmButton);
            }
            _confirmArmButton.Text = text;
            _confirmArmButton.IsEnabled = enabled;
            _confirmArmButton.ForegroundColor = DesignText;
            _confirmArmButton.BackgroundColor = color;
            _confirmArmButton.BorderColor = color;
            _confirmArmButton.BorderThickness = 2;
        }

        private void RemoveEnrollmentControl()
        {
            _oneClickRearmReviewStartedTicks = 0;
            _oneClickRearmReviewSignature = "";
            if (_confirmArmButton == null || Chart == null) return;
            try { _confirmArmButton.Click -= OnConfirmArmClicked; }
            catch { }
            try { Chart.RemoveControl(_confirmArmButton); }
            catch { }
            _confirmArmButton = null;
        }

        private void OnConfirmArmClicked(ButtonClickEventArgs args)
        {
            if (_executionGuardState == ExecutionGuardState.HardHalt)
            {
                OnOneClickExecutionRearmClicked();
                return;
            }
            if (_confirmClickBusy || _deploymentArmed || _bindingValid ||
                RunningMode != RunningMode.RealTime || _enrollmentReviewAvailableUtc == DateTime.MinValue ||
                Server.TimeInUtc < _enrollmentReviewAvailableUtc) return;
            _confirmClickBusy = true;
            _deploymentState = DeploymentState.Confirming;
            try
            {
                _effectiveEvaluationStartText = _proposedEvaluationStartText;
                ParseEvaluationStart();
                if (!_dataReady && !_startupHalt) _dataReady = ValidateDataContexts();
                RefreshPreflightBlockers(true);
                if (_startupHalt || _preflightBlockers.Count > 0)
                {
                    _deploymentState = DeploymentState.SetupRequired;
                    Print("ATLAS V34.ARSENAL confirmation refused: {0}", string.Join(" | ", _preflightBlockers));
                    return;
                }

                string owner = NewIdentityToken();
                string epoch = NewIdentityToken();
                string confirmed = Server.TimeInUtc.ToString("o", CultureInfo.InvariantCulture);
                string receipt = SignedReceipt(owner, epoch, _proposedEvaluationStartText, confirmed);

                SetLocalStorageStringChecked(_receiptPendingKey, receipt, LocalStorageScope.Type);
                LocalStorage.Flush(LocalStorageScope.Type);
                string pendingReadback = GetLocalStorageStringChecked(_receiptPendingKey, LocalStorageScope.Type) ?? "";
                if (!string.Equals(pendingReadback, receipt, StringComparison.Ordinal))
                    throw new InvalidOperationException("receipt pending write/readback mismatch");

                SetLocalStorageStringChecked(_receiptKey, receipt, LocalStorageScope.Type);
                LocalStorage.Flush(LocalStorageScope.Type);
                string committedReadback = GetLocalStorageStringChecked(_receiptKey, LocalStorageScope.Type) ?? "";
                if (!string.Equals(committedReadback, receipt, StringComparison.Ordinal))
                    throw new InvalidOperationException("receipt commit write/readback mismatch");

                LoadDeploymentReceipt();
                if (!_bindingValid) throw new InvalidOperationException(_bindingStatus);
                ResetOwnershipLedgerForFreshEnrollment();
                Journal("BINDING_CONFIRMED",
                    "\"account_mask\":\"" + Js(MaskedAccountNumber()) + "\",\"stage\":\"Challenge\",\"evaluation_start\":\"" +
                    Js(_effectiveEvaluationStartText) + "\",\"strategy_id\":\"" + StrategyConfigurationFingerprint() + "\"");
                ActivateConfirmedDeployment(true);
            }
            catch (Exception ex)
            {
                _deploymentArmed = false;
                _deploymentState = DeploymentState.Blocked;
                _bindingStatus = "enrollment failed: " + ex.Message;
                // A committed receipt may already exist if activation failed after the atomic
                // write. Reload it so this run never offers to overwrite a valid deployment.
                try { LoadDeploymentReceipt(); }
                catch { _bindingValid = false; }
                if (!_bindingValid) AddPreflightBlocker(_bindingStatus);
                Print("*** ATLAS V34.ARSENAL ENROLLMENT FAILED: {0}. NO TRADING AUTHORIZED. ***", ex.Message);
            }
            finally
            {
                _confirmClickBusy = false;
                UpdateEnrollmentControl();
                DrawMasterHud();
            }
        }

        private static string NewIdentityToken()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
        }

        private string MaskedAccountNumber()
        {
            string value = Account.Number.ToString(CultureInfo.InvariantCulture);
            return "****" + (value.Length <= 4 ? value : value.Substring(value.Length - 4));
        }

        private static string FingerprintText(string value)
        {
            unchecked
            {
                uint hash = 2166136261;
                foreach (char ch in value ?? "")
                {
                    hash ^= ch;
                    hash *= 16777619;
                }
                return hash.ToString("X8", CultureInfo.InvariantCulture);
            }
        }

        private static string ReceiptChecksum(string value)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? ""));
                var textValue = new StringBuilder(bytes.Length * 2);
                foreach (byte item in bytes) textValue.Append(item.ToString("x2", CultureInfo.InvariantCulture));
                return textValue.ToString();
            }
        }

        // cTrader LocalStorage keys are deliberately kept inside the broker-safe common subset:
        // 1..50 ASCII characters, alphanumeric words, and single internal spaces only.
        private static bool IsValidLocalStorageKey(string key)
        {
            if (string.IsNullOrEmpty(key) || key.Length > MaxLocalStorageKeyLength) return false;
            if (key[0] == ' ' || key[key.Length - 1] == ' ') return false;
            for (int i = 0; i < key.Length; i++)
            {
                char ch = key[i];
                bool asciiAlphaNumeric = (ch >= 'A' && ch <= 'Z') ||
                    (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9');
                if (asciiAlphaNumeric) continue;
                if (ch != ' ' || (i > 0 && key[i - 1] == ' ')) return false;
            }
            return true;
        }

        private string GetLocalStorageStringChecked(string key, LocalStorageScope scope)
        {
            if (!IsValidLocalStorageKey(key))
                throw new InvalidOperationException("invalid local-storage key format");
            return LocalStorage.GetString(key, scope);
        }

        private void SetLocalStorageStringChecked(string key, string value, LocalStorageScope scope)
        {
            if (!IsValidLocalStorageKey(key))
                throw new InvalidOperationException("invalid local-storage key format");
            LocalStorage.SetString(key, value, scope);
        }

        private void OnMarketPulse(string source)
        {
            if (source == "DAX") _lastDaxPulseUtc = Server.TimeInUtc;
            else if (source == "UK") _lastUkPulseUtc = Server.TimeInUtc;
            else if (source == "NAS") _lastNasPulseUtc = Server.TimeInUtc;
            else if (source == "GOLD") _lastGoldPulseUtc = Server.TimeInUtc;
            if (!_accountLeaseOwned) return;
            // The full SHA receipt check runs once per timer and immediately before every broker
            // mutation. The hot tick path uses the cached FNV fingerprint to avoid SHA allocations.
            if (RunningMode == RunningMode.RealTime && _bindingValid && !RuntimeIdentityFingerprintMatches())
            {
                _deploymentArmed = false;
                _managementAuthority = false;
                HaltStartup("runtime account identity changed; market processing stopped");
                return;
            }
            if (RunningMode == RunningMode.RealTime && !_deploymentArmed)
            {
                if (_managementAuthority)
                {
                    RebuildPragueDay(false);
                    TrackPositionExtremes();
                    CheckAccountEmergency();
                    EnforceRestrictedNewsFlatBook();
                }
                return;
            }
            CheckExclusiveAccountIntegrity();
            RebuildPragueDay(false);
            TrackPositionExtremes();
            CheckAccountEmergency();
            CheckFundedPayoutCompletion();
            EnforceRestrictedNewsFlatBook();
            CheckDaxLocalProfitLock();
            if (source == "NAS")
            {
                NasRolloverIfNeeded();
                RunNasTimeChecks();
                EvaluateNasSignalIfDue();
            }
            if (source == "GOLD") RunGoldTimeBackstop();
            ProcessProposalRetries();
            RefreshObjectiveStats(false);
            CheckObjectiveCompletion();
        }

        // =========================================================================================
        // CENTRAL ATOMIC GATE AND ORDER ROUTER
        // =========================================================================================
        private bool SubmitProposal(TradeProposal p)
        {
            if (p == null || p.Symbol == null) return false;
            if (string.IsNullOrEmpty(p.SignalId))
                p.SignalId = FingerprintText(p.Label + "|" + p.DecisionTimeUtc.ToString("o", CultureInfo.InvariantCulture) +
                    "|" + p.Side + "|" + Jn(p.TriggerPrice));
            if (_gateBusy)
            {
                GateReject(p, "gate_busy");
                return false;
            }

            _gateBusy = true;
            double approvedRisk = 0;
            try
            {
                string reason;
                if (!TryApproveProposal(p, out approvedRisk, out reason))
                {
                    GateReject(p, reason);
                    return false;
                }

                _transientReservedRisk += approvedRisk;
                double units = VolumeForRisk(p.Symbol, approvedRisk, p.SlDistancePts);
                if (units <= 0)
                {
                    GateReject(p, "below_broker_minimum");
                    return false;
                }

                double decide = p.Side == TradeType.Buy ? p.Symbol.Ask : p.Symbol.Bid;
                double spread = p.Symbol.Ask - p.Symbol.Bid;
                // The comment, active mutation and durable ledger must share one final decision
                // price. LimitAtMid previously built dc= from the side quote and then changed only
                // the mutation to mid, making its own restart-ledger hash impossible to validate.
                double finalDecisionPrice = p.Style == EntryStyle.LimitAtMid
                    ? Math.Round((p.Symbol.Ask + p.Symbol.Bid) / 2.0, p.Symbol.Digits)
                    : decide;
                string comment = BuildTradeComment(p, approvedRisk, finalDecisionPrice);
                if (comment.Length > MaxTradeCommentLength || !IsAscii(comment) ||
                    !IsCurrentTradeComment(comment) ||
                    ParseCompleteCommentToken(comment, "c=") != ConfigurationFingerprint() ||
                    ParseCompleteCommentToken(comment, "o=") != (DeploymentOwnerId ?? "").Trim() ||
                    ParseCompleteCommentToken(comment, "e=") != (StageEpochId ?? "").Trim())
                {
                    GateReject(p, "trade_metadata_invalid_or_too_long");
                    return false;
                }

                // Approval itself may journal a throttle/capacity transition. If that write is the
                // fifth IO failure, journal HALT becomes true after TryApproveProposal's first
                // readiness snapshot. Re-authorize side-effect-free at the broker boundary.
                if (!FinalEntryAuthorization()) return false;

                _activeEntryMutation = new ActiveEntryMutation
                {
                    Proposal = p,
                    ApprovedRisk = approvedRisk,
                    Units = units,
                    DecisionPrice = finalDecisionPrice,
                    Spread = spread,
                    ExpectedComment = comment,
                    ExpectedCommentHash = ReceiptChecksum(comment),
                    StartedUtc = Server.TimeInUtc,
                    StartedMonotonicTimestamp = Stopwatch.GetTimestamp()
                };

                if (p.Style == EntryStyle.LimitAtMid)
                {
                    double mid = finalDecisionPrice;
                    double slPips = p.SlDistancePts / p.Symbol.PipSize;
                    double tpPips = p.TpDistancePts / p.Symbol.PipSize;
                    DateTime expiry = Server.TimeInUtc.AddMinutes(15 * Math.Max(1, p.ExpiryBars));
#pragma warning disable CS0618
                    var limitResult = PlaceLimitOrder(p.Side, p.Symbol.Name, units, mid, p.Label,
                        slPips, tpPips, ProtectionType.Relative, expiry, comment);
#pragma warning restore CS0618
                    // Some cTrader/broker combinations synchronously fill the limit and return no
                    // PendingOrder. An exact callback-captured/result Position ID is success, not a
                    // placement failure.
                    Position immediateLimitPosition = limitResult == null ? null : limitResult.Position;
                    if (immediateLimitPosition != null &&
                        !RegisterSuccessfulMarketResult(immediateLimitPosition))
                    {
                        GateReject(p, "limit_fill_result_identity_mismatch");
                        return false;
                    }
                    if (_activeEntryMutation.PositionId.HasValue)
                    {
                        Position filledPosition = Positions.FirstOrDefault(
                            x => x.Id == _activeEntryMutation.PositionId.Value);
                        bool successfulResult = limitResult != null && limitResult.IsSuccessful;
                        bool positionConsistent = filledPosition != null &&
                            IsPersistentOrCurrentPosition(filledPosition) &&
                            (limitResult.Position == null ||
                             (limitResult.Position.Id == filledPosition.Id &&
                              PositionCoreMatchesMutation(limitResult.Position, _activeEntryMutation)));
                        bool pendingConsistent = limitResult != null &&
                            (limitResult.PendingOrder == null ||
                             (_activeEntryMutation.PendingOrderId.HasValue &&
                              limitResult.PendingOrder.Id == _activeEntryMutation.PendingOrderId.Value &&
                              PendingCoreMatchesMutation(limitResult.PendingOrder, _activeEntryMutation)));
                        if (!successfulResult || !positionConsistent || !pendingConsistent ||
                            !TryCompleteImmediateLimitFill(p, approvedRisk, units, mid))
                        {
                            GuardFailedBrokerResultWithEarlyExposure(
                                "limit_result_inconsistent_after_fill_callback");
                            GateReject(p, "limit_result_inconsistent_after_fill_callback");
                            return false;
                        }
                        if (!_activeEntryMutation.PendingFillCallbackSeen)
                            RememberDirectLimitTerminalExpectation(
                                _activeEntryMutation, filledPosition);
                        _brokerMutationSequence++;
                        Print("ATLAS GATE APPROVED | {0} {1} IMMEDIATE LIMIT FILL risk ${2:F0}",
                            p.Label, p.Side, approvedRisk);
                        return true;
                    }
                    if (_activeEntryMutation.PendingCancelledSeen)
                    {
                        _brokerMutationSequence++;
                        GateReject(p, "limit_cancelled_synchronously");
                        return false;
                    }
                    if (limitResult == null || !limitResult.IsSuccessful || limitResult.PendingOrder == null)
                    {
                        GuardFailedBrokerResultWithEarlyExposure("limit_order_result_inconsistent");
                        string limitError = limitResult == null ? "null_result" : limitResult.Error.ToString();
                        RecordExecutionIncident("limit_order_failed_" + limitError, 0, 0);
                        GateReject(p, "limit_order_failed_" + limitError);
                        return false;
                    }

                    if (!RegisterSuccessfulPendingResult(limitResult.PendingOrder))
                    {
                        GateReject(p, "limit_order_callback_result_mismatch");
                        return false;
                    }
                    _brokerMutationSequence++;
                    Journal("ORDER",
                        "\"label\":\"" + Js(p.Label) + "\",\"engine\":\"" + p.Engine + "\",\"setup\":\"" + p.Setup +
                        "\",\"order_id\":" + limitResult.PendingOrder.Id +
                        ",\"type\":\"limit\",\"side\":\"" + p.Side + "\",\"price\":" + Jn(mid) +
                        ",\"risk\":" + Jn(approvedRisk) + ",\"dynamic_tier\":\"" + Js(_currentDynamicRiskTier) + "\"" +
                        ",\"dynamic_scale\":" + Jn(_currentDynamicRiskScale) +
                        ",\"units\":" + Jn(units) + ",\"expiry\":\"" + expiry.ToString("o") + "\"");
                    Print("ATLAS GATE APPROVED | {0} {1} LIMIT @ {2:F2} risk ${3:F0}", p.Label, p.Side, mid, approvedRisk);
                    return true;
                }

                TradeResult marketResult;
                long requestMonotonicTimestamp = Stopwatch.GetTimestamp();
                if (p.Style == EntryStyle.MarketAbsoluteProtection)
                {
                    double emergencySlPips = p.SlDistancePts / p.Symbol.PipSize;
                    double? emergencyTpPips = p.TpDistancePts > 0
                        ? (double?)(p.TpDistancePts / p.Symbol.PipSize)
                        : null;
                    marketResult = ExecuteMarketOrder(p.Side, p.Symbol.Name, units, p.Label,
                        emergencySlPips, emergencyTpPips, comment);
                }
                else
                {
                    double? slPips = p.SlDistancePts > 0 ? (double?)(p.SlDistancePts / p.Symbol.PipSize) : null;
                    double? tpPips = p.TpDistancePts > 0 ? (double?)(p.TpDistancePts / p.Symbol.PipSize) : null;
                    marketResult = ExecuteMarketOrder(p.Side, p.Symbol.Name, units, p.Label, slPips, tpPips, comment);
                }

                if (marketResult == null || !marketResult.IsSuccessful || marketResult.Position == null)
                {
                    GuardFailedBrokerResultWithEarlyExposure("market_order_result_inconsistent");
                    string marketError = marketResult == null ? "null_result" : marketResult.Error.ToString();
                    RecordExecutionIncident("market_order_failed_" + marketError, 0, 0);
                    GateReject(p, "market_order_failed_" + marketError);
                    return false;
                }

                var pos = marketResult.Position;
                double brokerCallMs = MonotonicElapsedMilliseconds(requestMonotonicTimestamp);
                if (!RegisterSuccessfulMarketResult(pos))
                {
                    GateReject(p, "market_order_callback_result_mismatch");
                    return false;
                }
                _brokerMutationSequence++;
                if (pos.VolumeInUnits + p.Symbol.VolumeInUnitsStep * 0.5 < units)
                    RecordExecutionIncident("partial_fill", 0, 0);
                PositionMeta meta = _positionMeta[pos.Id];
                ExecutionGuardState guardBeforeEntryQuality = _executionGuardState;
                RecordExecutionQuality(p, decide, pos.EntryPrice,
                    brokerCallMs, true);
                if (guardBeforeEntryQuality != ExecutionGuardState.LatencyCooldown &&
                    _executionGuardState == ExecutionGuardState.LatencyCooldown)
                    _requestAuthorizedImmediateFillPositionIds.Add(pos.Id);

                string initialProtectionReason;
                if (!ValidPositionProtection(pos, out initialProtectionReason))
                {
                    _lastProposalRejectCode = "initial_fill_protection_invalid_" + initialProtectionReason;
                    RecordExecutionIncident("initial_fill_protection_invalid_" + initialProtectionReason, 0, brokerCallMs);
                    Journal("GUARD", "\"kind\":\"initial_fill_protection_invalid\",\"position_id\":" + pos.Id +
                        ",\"reason\":\"" + Js(initialProtectionReason) + "\"");
                    EmergencyClose(pos, "initial_fill_" + initialProtectionReason);
                    return false;
                }

                // Authority can change while a synchronous market call is in flight. Apply this
                // post-fill boundary to every market style, including positions whose relative
                // protection was already installed server-side.
                if (!CanManagePosition(pos))
                {
                    _lastProposalRejectCode = "post_fill_management_authority_missing";
                    RecordExecutionIncident("post_fill_management_authority_missing", 0, brokerCallMs);
                    HaltStartup("management authority changed after market fill; protected position preserved for manual review");
                    GateReject(p, "post_fill_management_authority_missing");
                    return false;
                }

                // A blocker may appear while the synchronous broker request is in flight.
                // Preserve only the protected fill that itself caused a latency cooldown;
                // every unrelated blocker still closes this newly admitted position.
                if (EntryHaltRequiresClosingOpenedPosition(pos))
                {
                    _lastProposalRejectCode = "post_fill_global_halt";
                    Journal("GUARD",
                        "\"kind\":\"post_fill_global_halt\",\"position_id\":" + pos.Id +
                        ",\"blockers\":\"" + Js(string.Join(" || ", GetGlobalEntryBlockers())) + "\"");
                    EmergencyClose(pos, "post_fill_global_halt");
                    GateReject(p, "post_fill_global_halt");
                    return false;
                }

                if (p.Style == EntryStyle.MarketAbsoluteProtection)
                {
                    double actualDistance = p.Side == TradeType.Buy
                        ? pos.EntryPrice - p.AbsoluteSl
                        : p.AbsoluteSl - pos.EntryPrice;
                    double actualRisk = actualDistance > 0
                        ? actualDistance / p.Symbol.PipSize * p.Symbol.PipValue * pos.VolumeInUnits
                        : double.PositiveInfinity;
                    if (double.IsInfinity(actualRisk) || double.IsNaN(actualRisk) ||
                        actualRisk > approvedRisk * (1.0 + ExecutionReservePct / 100.0))
                    {
                        _lastProposalRejectCode = "post_fill_risk_exceeded";
                        RecordExecutionIncident("post_fill_risk_exceeded", 0, 0);
                        Journal("GUARD",
                            "\"kind\":\"post_fill_risk_exceeded\",\"position_id\":" + pos.Id +
                            ",\"approved_risk\":" + Jn(approvedRisk) + ",\"actual_stop_risk\":" + Jn(actualRisk));
                        EmergencyClose(pos, "post_fill_risk_exceeded");
                        return false;
                    }

                    var modify = ModifyPosition(pos, p.AbsoluteSl, p.AbsoluteTp > 0 ? (double?)p.AbsoluteTp : null, ProtectionType.Absolute);
                    if (modify == null || !modify.IsSuccessful)
                        modify = ModifyPosition(pos, p.AbsoluteSl, p.AbsoluteTp > 0 ? (double?)p.AbsoluteTp : null, ProtectionType.Absolute);
                    if (modify == null || !modify.IsSuccessful)
                    {
                        _lastProposalRejectCode = "absolute_protection_failed";
                        RecordExecutionIncident("absolute_protection_failed", 0, 0);
                        Journal("GUARD", "\"kind\":\"protect_failsafe\",\"position_id\":" + pos.Id + ",\"label\":\"" + Js(p.Label) + "\"");
                        EmergencyClose(pos, "absolute_protection_failed");
                        return false;
                    }
                    double protectionTolerance = Math.Max(TickSizeFor(p.Symbol) * 0.51, 1e-9);
                    bool stopInstalled = pos.StopLoss.HasValue &&
                        Math.Abs(pos.StopLoss.Value - p.AbsoluteSl) <= protectionTolerance;
                    bool targetInstalled = p.AbsoluteTp <= 0 ||
                        (pos.TakeProfit.HasValue && Math.Abs(pos.TakeProfit.Value - p.AbsoluteTp) <= protectionTolerance);
                    if (!stopInstalled || !targetInstalled)
                    {
                        _lastProposalRejectCode = "absolute_protection_mismatch";
                        RecordExecutionIncident("absolute_protection_mismatch", 0, 0);
                        Journal("GUARD",
                            "\"kind\":\"absolute_protection_mismatch\",\"position_id\":" + pos.Id +
                            ",\"requested_sl\":" + Jn(p.AbsoluteSl) + ",\"installed_sl\":" + Jn(pos.StopLoss ?? 0) +
                            ",\"requested_tp\":" + Jn(p.AbsoluteTp) + ",\"installed_tp\":" + Jn(pos.TakeProfit ?? 0));
                        EmergencyClose(pos, "absolute_protection_mismatch");
                        return false;
                    }
                    meta.InitialRisk = CalculateInitialRiskAtFill(pos, p.Symbol, approvedRisk);
                    meta.StopRiskDistancePts = Math.Abs(pos.EntryPrice - pos.StopLoss.Value);
                    meta.LastStopPrice = pos.StopLoss.Value;
                    meta.LastTakeProfitPrice = pos.TakeProfit ?? 0;
                }

                Journal("ENTRY",
                    "\"label\":\"" + Js(p.Label) + "\",\"engine\":\"" + p.Engine + "\",\"setup\":\"" + p.Setup +
                    "\",\"position_id\":" + pos.Id + ",\"side\":\"" + p.Side +
                    "\",\"decide\":" + Jn(decide) + ",\"fill\":" + Jn(pos.EntryPrice) +
                    ",\"spread\":" + Jn(spread) + ",\"approved_risk\":" + Jn(approvedRisk) +
                    ",\"dynamic_tier\":\"" + Js(_currentDynamicRiskTier) + "\"" +
                    ",\"dynamic_scale\":" + Jn(_currentDynamicRiskScale) +
                    ",\"initial_risk\":" + Jn(meta.InitialRisk) + ",\"units\":" + Jn(units) +
                    ",\"sl_dist\":" + Jn(p.SlDistancePts) + ",\"tp_dist\":" + Jn(p.TpDistancePts) +
                    ",\"absolute_sl\":" + Jn(p.AbsoluteSl) + ",\"absolute_tp\":" + Jn(p.AbsoluteTp) +
                    ",\"config_id\":\"" + ConfigurationFingerprint() + "\"");
                Print("ATLAS GATE APPROVED | {0} {1} fill {2:F2} risk ${3:F0}", p.Label, p.Side, pos.EntryPrice, approvedRisk);
                return true;
            }
            catch (Exception ex)
            {
                RecordExecutionIncident("router_exception_" + ex.GetType().Name, 0, 0);
                GateReject(p, "router_exception_" + ex.Message);
                return false;
            }
            finally
            {
                _activeEntryMutation = null;
                _transientReservedRisk = Math.Max(0, _transientReservedRisk - approvedRisk);
                _gateBusy = false;
            }
        }

        private bool FinalEntryAuthorization()
        {
            if (!BindingAuthorityCurrent())
            {
                _lastProposalRejectCode = "final_binding_authority_failed";
                _lastGateReason = "BLOCKED: final binding authority failed";
                return false;
            }
            List<string> blockers = GetGlobalEntryBlockers();
            if (blockers.Count == 0) return true;
            _lastProposalRejectCode = "final_authorization_" + blockers[0];
            _lastGateReason = "BLOCKED: " + blockers[0];
            return false;
        }

        private ProposalOutcome SubmitProposalOutcome(TradeProposal proposal)
        {
            if (proposal == null) return ProposalOutcome.TerminalReject;
            if (string.IsNullOrEmpty(proposal.SignalId))
                proposal.SignalId = ReceiptChecksum(proposal.Label + "|" +
                    proposal.DecisionTimeUtc.ToString("o", CultureInfo.InvariantCulture) + "|" +
                    proposal.Side + "|" + Jn(proposal.TriggerPrice)).Substring(0, 16).ToUpperInvariant();
            if (HasSignalExposure(proposal)) return ProposalOutcome.Accepted;

            long brokerSequenceBefore = _brokerMutationSequence;
            _lastProposalRejectCode = "";
            bool accepted = SubmitProposal(proposal);
            if (accepted || HasSignalExposure(proposal)) return ProposalOutcome.Accepted;
            if (_brokerMutationSequence != brokerSequenceBefore) return ProposalOutcome.TerminalReject;
            return RetryableProposalReject(_lastProposalRejectCode)
                ? ProposalOutcome.TransientReject
                : ProposalOutcome.TerminalReject;
        }

        private bool RetryableProposalReject(string reason)
        {
            if (string.IsNullOrEmpty(reason)) return false;
            return reason == "gate_busy" || reason == "data_not_ready" ||
                   reason == "invalid_or_stale_quote" || reason == "stale_quote_tick_age" ||
                   reason.StartsWith("spread ", StringComparison.Ordinal);
        }

        private bool HasSignalExposure(TradeProposal proposal)
        {
            if (proposal == null || string.IsNullOrEmpty(proposal.SignalId) || string.IsNullOrEmpty(proposal.Label))
                return false;
            return Positions.Any(p => p.Label == proposal.Label && IsPersistentOrCurrentPosition(p) &&
                                      SignalIdForPosition(p) == proposal.SignalId) ||
                   PendingOrders.Any(o => o.Label == proposal.Label && IsPersistentOrCurrentPending(o) &&
                                          SignalIdForPending(o) == proposal.SignalId);
        }

        private ProposalOutcome SubmitSignalWithBoundedRetry(TradeProposal proposal, DateTime deadlineUtc)
        {
            if (proposal == null) return ProposalOutcome.TerminalReject;
            if (_stateRebuildMode)
            {
                ConsumeSignalProposal(proposal, false, "missed_before_arm_or_restart");
                return ProposalOutcome.TerminalReject;
            }
            ProposalOutcome outcome = SubmitProposalOutcome(proposal);
            if (outcome == ProposalOutcome.Accepted)
            {
                ConsumeSignalProposal(proposal, true, "accepted");
                return outcome;
            }
            if (outcome == ProposalOutcome.TerminalReject || deadlineUtc <= Server.TimeInUtc)
            {
                ConsumeSignalProposal(proposal, false, _lastProposalRejectCode);
                return ProposalOutcome.TerminalReject;
            }

            _signalRetries[proposal.Label] = new SignalRetry
            {
                Proposal = proposal,
                DeadlineUtc = deadlineUtc,
                NextAttemptUtc = Server.TimeInUtc.AddSeconds(1),
                Attempts = 1,
                LastReason = _lastProposalRejectCode
            };
            Journal("SIGNAL_DEFERRED",
                "\"label\":\"" + Js(proposal.Label) + "\",\"signal_id\":\"" + proposal.SignalId +
                "\",\"reason\":\"" + Js(_lastProposalRejectCode) + "\",\"expiry_utc\":\"" + deadlineUtc.ToString("o") + "\"");
            _lastGateReason = "RETRYING " + proposal.Label + ": " + _lastProposalRejectCode;
            return ProposalOutcome.TransientReject;
        }

        private void ProcessProposalRetries()
        {
            if (_signalRetries.Count == 0 || !_deploymentArmed || _stateRebuildMode) return;
            DateTime now = Server.TimeInUtc;
            foreach (var pair in _signalRetries.ToArray())
            {
                SignalRetry retry = pair.Value;
                if (retry == null || retry.Proposal == null)
                {
                    _signalRetries.Remove(pair.Key);
                    continue;
                }
                if (now > retry.DeadlineUtc || retry.Attempts >= 10)
                {
                    _signalRetries.Remove(pair.Key);
                    ConsumeSignalProposal(retry.Proposal, false, "retry_expired");
                    Journal("SIGNAL_EXPIRED", "\"label\":\"" + Js(retry.Proposal.Label) +
                        "\",\"signal_id\":\"" + retry.Proposal.SignalId + "\",\"attempts\":" + retry.Attempts + "");
                    continue;
                }
                if (now < retry.NextAttemptUtc) continue;
                if (HasSignalExposure(retry.Proposal))
                {
                    _signalRetries.Remove(pair.Key);
                    ConsumeSignalProposal(retry.Proposal, true, "accepted_recovered");
                    continue;
                }
                if (!RetryTriggerStillValid(retry.Proposal))
                {
                    _signalRetries.Remove(pair.Key);
                    ConsumeSignalProposal(retry.Proposal, false, "trigger_invalidated");
                    continue;
                }

                retry.Attempts++;
                retry.NextAttemptUtc = now.AddSeconds(1);
                ProposalOutcome outcome = SubmitProposalOutcome(retry.Proposal);
                if (outcome == ProposalOutcome.TransientReject)
                {
                    retry.LastReason = _lastProposalRejectCode;
                    Journal("SIGNAL_RETRY", "\"label\":\"" + Js(retry.Proposal.Label) +
                        "\",\"signal_id\":\"" + retry.Proposal.SignalId + "\",\"attempt\":" + retry.Attempts +
                        ",\"reason\":\"" + Js(_lastProposalRejectCode) + "\"");
                    continue;
                }
                _signalRetries.Remove(pair.Key);
                ConsumeSignalProposal(retry.Proposal, outcome == ProposalOutcome.Accepted,
                    outcome == ProposalOutcome.Accepted ? "accepted_retry" : _lastProposalRejectCode);
            }
        }

        private bool RetryTriggerStillValid(TradeProposal proposal)
        {
            if (proposal == null || proposal.Symbol == null) return false;
            if (Positions.Any(p => p.Label == proposal.Label) || PendingOrders.Any(o => o.Label == proposal.Label)) return false;
            if (proposal.TriggerPrice <= 0 || proposal.TriggerDirection == 0) return true;
            double executable = proposal.TriggerDirection > 0 ? proposal.Symbol.Ask : proposal.Symbol.Bid;
            return proposal.TriggerDirection > 0
                ? executable >= proposal.TriggerPrice
                : executable <= proposal.TriggerPrice;
        }

        private void ConsumeSignalProposal(TradeProposal proposal, bool accepted, string reason)
        {
            if (proposal == null) return;
            _signalRetries.Remove(proposal.Label);
            if (proposal.Label == DaxALabel && _daxA != null) _daxA.Traded = true;
            else if (proposal.Label == DaxBLabel && _daxB != null) _daxB.Traded = true;
            else if (proposal.Label == DaxCLabel) _daxCTraded = true;
            else if (proposal.Label == NasCoreLabel)
            {
                _nasSignalDone = true;
                if (accepted)
                {
                    _nasCoreDone = true;
                    _nasDriveRange = proposal.ReferenceDistancePts;
                    _nasDriveDirection = proposal.Side == TradeType.Buy ? 1 : -1;
                    _nasDayRiskUsed += proposal.RequestedRisk;
                }
            }
            else if (proposal.Label == NasReLabel)
            {
                _nasPendingReentry = false;
                _nasReDone = true;
                if (accepted) _nasDayRiskUsed += proposal.RequestedRisk;
            }
            Journal(accepted ? "SIGNAL_ACCEPTED" : "SIGNAL_TERMINAL",
                "\"label\":\"" + Js(proposal.Label) + "\",\"signal_id\":\"" + (proposal.SignalId ?? "") +
                "\",\"reason\":\"" + Js(reason) + "\"");
        }

        private bool TryApproveProposal(TradeProposal p, out double approvedRisk, out string reason)
        {
            approvedRisk = 0;
            reason = "";

            List<string> blockers = GetGlobalEntryBlockers();
            if (blockers.Count > 0) { reason = blockers[0]; return false; }
            if (!IsFunded() &&
                Account.Equity >= InitialBalance * (1.0 + ProfitTargetPct / 100.0) &&
                BestDaySatisfied())
            {
                _objectiveClosing = true;
                reason = "objective_close_in_progress";
                return false;
            }
            if (FundedPayoutCloseCondition())
            {
                _fundedPayoutClosing = true;
                reason = "funded_payout_close_in_progress";
                return false;
            }
            if (Server.TimeInUtc - p.DecisionTimeUtc > TimeSpan.FromMinutes(20)) { reason = "stale_signal"; return false; }
            if (p.RequestedRisk <= 0 || p.SlDistancePts <= 0) { reason = "invalid_risk_or_stop"; return false; }

            if (double.IsNaN(p.Symbol.Bid) || double.IsNaN(p.Symbol.Ask) ||
                double.IsInfinity(p.Symbol.Bid) || double.IsInfinity(p.Symbol.Ask) ||
                p.Symbol.Ask <= p.Symbol.Bid)
            {
                reason = "invalid_or_stale_quote";
                return false;
            }
            DateTime lastPulse = p.Engine == "DAX" ? _lastDaxPulseUtc :
                                 p.Engine == "UK" ? _lastUkPulseUtc :
                                 p.Engine == "GOLD" ? _lastGoldPulseUtc : _lastNasPulseUtc;
            if (RunningMode == RunningMode.RealTime &&
                (lastPulse == DateTime.MinValue || Server.TimeInUtc - lastPulse > TimeSpan.FromSeconds(MaximumQuoteAgeSeconds)))
            {
                reason = "stale_quote_tick_age";
                return false;
            }

            double spread = p.Symbol.Ask - p.Symbol.Bid;
            double spreadLimit = SpreadLimitFor(p);
            if (spreadLimit > 0 && spread > spreadLimit)
            {
                reason = string.Format(CultureInfo.InvariantCulture,
                    "spread {0:F2} > {1:F2}", spread, spreadLimit);
                return false;
            }

            if (ExclusiveAccount &&
                (Positions.Any(x => !IsRuntimeRecognizedPosition(x)) ||
                 PendingOrders.Any(x => !IsRuntimeRecognizedPending(x))))
            {
                reason = "external_account_exposure";
                return false;
            }

            bool unbounded;
            double openRemainingRisk = CalculateAllOpenRemainingRisk(out unbounded);
            if (unbounded)
            {
                reason = "open_position_without_valid_stop";
                return false;
            }

            bool pendingUnbounded;
            double pendingRisk = CalculateAllPendingRisk(out pendingUnbounded);
            if (pendingUnbounded)
            {
                reason = "pending_order_without_modelled_stop";
                return false;
            }
            double riskMultiplier = EffectivePortfolioRiskScale();
            double roomToMaxLoss = Account.Equity - MaxLossLimit();
            if (roomToMaxLoss < NearFloorDeriskRoomUsd) riskMultiplier *= 0.5;
            if (_targetReached || FundedPayoutCompletionRiskActive()) riskMultiplier *= CompletionRiskMultiplier;
            approvedRisk = p.RequestedRisk * riskMultiplier;
            if (!ApplyCorrelationThrottle(p, ref approvedRisk, out reason)) return false;
            if (approvedRisk <= 0) { reason = "effective_risk_zero"; return false; }

            double internalDailyFloor = _dayAnchorBalance - EffectiveDailyWorstCaseCapUsd();
            double bufferedOfficialDailyFloor = OfficialDailyLimit() + EmergencyBufferUsd;
            double bufferedMaxLossFloor = MaxLossLimit() + MaxLossSafetyBufferUsd;
            double requiredFloor = Math.Max(Math.Max(internalDailyFloor, bufferedOfficialDailyFloor), bufferedMaxLossFloor);

            double reserveRate = ExecutionReservePct / 100.0;
            double aggregate = openRemainingRisk + pendingRisk + _transientReservedRisk + approvedRisk;
            double reserve = aggregate * reserveRate;
            double projectedWorstEquity = Account.Equity - aggregate - reserve;

            if (projectedWorstEquity <= requiredFloor)
            {
                double capacity = (Account.Equity - requiredFloor - 1.0) / (1.0 + reserveRate) -
                                  openRemainingRisk - pendingRisk - _transientReservedRisk;
                double minimumUsefulRisk = Math.Max(
                    MinimumBrokerRisk(p.Symbol, p.SlDistancePts),
                    approvedRisk * MinimumClippedRiskPct / 100.0);
                if (capacity < minimumUsefulRisk)
                {
                    reason = string.Format(CultureInfo.InvariantCulture,
                        "projected_floor projected={0:F0} required>{1:F0} capacity={2:F0} minimum={3:F0}",
                        projectedWorstEquity, requiredFloor, capacity, minimumUsefulRisk);
                    MarkCapacityFrozen(p, capacity, minimumUsefulRisk, requiredFloor, projectedWorstEquity);
                    return false;
                }
                if (!ClipRiskToCapacity)
                {
                    reason = string.Format(CultureInfo.InvariantCulture,
                        "projected_floor projected={0:F0} required>{1:F0} capacity={2:F0}; risk clipping disabled",
                        projectedWorstEquity, requiredFloor, capacity);
                    return false;
                }

                double originalApprovedRisk = approvedRisk;
                approvedRisk = Math.Min(approvedRisk, capacity);
                aggregate = openRemainingRisk + pendingRisk + _transientReservedRisk + approvedRisk;
                reserve = aggregate * reserveRate;
                projectedWorstEquity = Account.Equity - aggregate - reserve;
                Journal("RISK_CLIP",
                    "\"label\":\"" + Js(p.Label) + "\",\"from\":" + Jn(originalApprovedRisk) +
                    ",\"to\":" + Jn(approvedRisk) + ",\"capacity\":" + Jn(capacity) +
                    ",\"required_floor\":" + Jn(requiredFloor));
            }

            ClearCapacityFrozen("proposal_capacity_available");
            _lastGateReason = string.Format(CultureInfo.InvariantCulture,
                "APPROVED {0} ${1:F0}; {2} x{3:F2}; projected ${4:F0} > floor ${5:F0}",
                p.Label, approvedRisk, _currentDynamicRiskTier, _currentDynamicRiskScale, projectedWorstEquity, requiredFloor);
            return true;
        }

        private void GateReject(TradeProposal p, string reason)
        {
            _lastProposalRejectCode = reason ?? "unknown";
            _lastGateReason = "REJECT " + (p != null ? p.Label : "?") + ": " + reason;
            Print("ATLAS GATE REJECT | {0} | {1}", p != null ? p.Label : "?", reason);
            Journal("SKIP",
                "\"label\":\"" + Js(p != null ? p.Label : "?") + "\",\"engine\":\"" + Js(p != null ? p.Engine : "?") +
                "\",\"setup\":\"" + Js(p != null ? p.Setup : "?") + "\",\"reason\":\"" + Js(reason) + "\"");
        }

        private void MarkCapacityFrozen(TradeProposal p, double capacity, double minimumUsefulRisk,
            double requiredFloor, double projectedWorstEquity)
        {
            string detail = string.Format(CultureInfo.InvariantCulture,
                "{0} capacity ${1:F0} < minimum ${2:F0}; projected ${3:F0}, floor ${4:F0}",
                p != null ? p.Label : "?", capacity, minimumUsefulRisk, projectedWorstEquity, requiredFloor);
            bool transition = !_capacityFrozen;
            _capacityFrozen = true;
            _capacityFreezeDetail = detail;
            if (_capacityFreezeSinceUtc == DateTime.MinValue)
                _capacityFreezeSinceUtc = Server.TimeInUtc;
            if (transition)
                Journal("GUARD",
                    "\"kind\":\"capacity_frozen\",\"label\":\"" + Js(p != null ? p.Label : "?") +
                    "\",\"capacity\":" + Jn(capacity) + ",\"minimum_useful_risk\":" + Jn(minimumUsefulRisk) +
                    ",\"projected_worst_equity\":" + Jn(projectedWorstEquity) +
                    ",\"required_floor\":" + Jn(requiredFloor));
        }

        private void ClearCapacityFrozen(string reason)
        {
            if (!_capacityFrozen) return;
            double durationSeconds = _capacityFreezeSinceUtc == DateTime.MinValue
                ? 0
                : Math.Max(0, (Server.TimeInUtc - _capacityFreezeSinceUtc).TotalSeconds);
            Journal("GUARD",
                "\"kind\":\"capacity_recovered\",\"reason\":\"" + Js(reason) +
                "\",\"duration_seconds\":" + Jn(durationSeconds) +
                ",\"prior_detail\":\"" + Js(_capacityFreezeDetail) + "\"");
            _capacityFrozen = false;
            _capacityFreezeDetail = "";
            _capacityFreezeSinceUtc = DateTime.MinValue;
        }

        private double CalculateAllOpenRemainingRisk(out bool unbounded)
        {
            unbounded = false;
            double total = 0;
            foreach (var p in Positions)
            {
                var symbol = Symbols.GetSymbol(p.SymbolName);
                if (symbol == null || !p.StopLoss.HasValue || symbol.PipSize <= 0 || symbol.PipValue <= 0)
                {
                    unbounded = true;
                    continue;
                }
                double currentExit = p.TradeType == TradeType.Buy ? symbol.Bid : symbol.Ask;
                bool crossedStop = p.TradeType == TradeType.Buy
                    ? p.StopLoss.Value >= currentExit
                    : p.StopLoss.Value <= currentExit;
                if (crossedStop)
                {
                    unbounded = true;
                    continue;
                }
                double remainingDistance = p.TradeType == TradeType.Buy
                    ? currentExit - p.StopLoss.Value
                    : p.StopLoss.Value - currentExit;
                total += remainingDistance / symbol.PipSize * symbol.PipValue * p.VolumeInUnits;
            }
            return total;
        }

        private double CalculateAllPendingRisk(out bool unbounded)
        {
            unbounded = false;
            double total = 0;
            foreach (var order in PendingOrders)
            {
                Symbol symbol = Symbols.GetSymbol(order.SymbolName);
                if (symbol == null || symbol.PipValue <= 0 || !order.StopLossPips.HasValue ||
                    order.StopLossPips.Value <= 0)
                {
                    unbounded = true;
                    continue;
                }

                double liveRisk = order.StopLossPips.Value * symbol.PipValue * order.VolumeInUnits;
                PendingInfo info;
                if (_pendingInfo.TryGetValue(order.Id, out info))
                    liveRisk = Math.Max(liveRisk, info.ApprovedRisk);
                total += liveRisk;
            }
            return total;
        }

        private double SpreadLimitFor(TradeProposal proposal)
        {
            if (proposal == null) return 0;
            if (proposal.Engine == "DAX") return DaxMaxSpreadPts;
            if (proposal.Engine == "UK") return UkMaxSpreadPts;
            if (proposal.Engine == "GOLD") return GoldMaxSpreadUsd;
            return NasMaxSpreadPts;
        }

        private bool SpreadTooWide(Symbol symbol, double maxSpreadPts)
        {
            if (symbol == null || double.IsNaN(symbol.Bid) || double.IsNaN(symbol.Ask) ||
                double.IsInfinity(symbol.Bid) || double.IsInfinity(symbol.Ask) || symbol.Ask <= symbol.Bid)
                return true;
            return maxSpreadPts > 0 && symbol.Ask - symbol.Bid > maxSpreadPts;
        }

        private bool ApplyCorrelationThrottle(TradeProposal proposal, ref double approvedRisk, out string reason)
        {
            reason = "";
            _lastCorrelationUsed = 0;
            _lastCorrelationMultiplier = 1.0;
            if (!CorrelationThrottleOn || proposal == null) return true;

            int direction = proposal.Side == TradeType.Buy ? 1 : -1;
            double existingDirectionalRisk = DirectionalClusterRisk(direction);
            if (double.IsInfinity(existingDirectionalRisk))
            {
                reason = "correlation_cluster_risk_unbounded";
                return false;
            }

            bool crossMarket;
            double correlation = MaxCorrelationAgainstExposure(proposal.Engine, direction, out crossMarket);
            _lastCorrelationUsed = correlation;
            if (crossMarket && correlation >= CorrelationTrigger)
            {
                approvedRisk *= CorrelatedSecondRiskMultiplier;
                _lastCorrelationMultiplier = CorrelatedSecondRiskMultiplier;
            }

            double room = SameDirectionClusterCapUsd - existingDirectionalRisk;
            approvedRisk = Math.Min(approvedRisk, Math.Max(0, room));
            if (approvedRisk <= 0)
            {
                reason = "same_direction_cluster_cap";
                return false;
            }

            Journal("RISK_THROTTLE",
                "\"engine\":\"" + Js(proposal.Engine) + "\",\"label\":\"" + Js(proposal.Label) +
                "\",\"correlation\":" + Jn(correlation) + ",\"cross_market\":" + Bool(crossMarket) +
                ",\"multiplier\":" + Jn(_lastCorrelationMultiplier) +
                ",\"existing_directional_risk\":" + Jn(existingDirectionalRisk) +
                ",\"approved_risk\":" + Jn(approvedRisk) + ",\"cluster_cap\":" + Jn(SameDirectionClusterCapUsd));
            return true;
        }

        private double DirectionalClusterRisk(int direction)
        {
            double total = 0;
            foreach (var position in Positions.Where(x => IsMasterLabel(x.Label)))
            {
                int positionDirection = position.TradeType == TradeType.Buy ? 1 : -1;
                if (positionDirection != direction) continue;
                Symbol symbol = Symbols.GetSymbol(position.SymbolName);
                if (symbol == null || !position.StopLoss.HasValue || symbol.PipSize <= 0 || symbol.PipValue <= 0)
                    return double.PositiveInfinity;
                double currentExit = position.TradeType == TradeType.Buy ? symbol.Bid : symbol.Ask;
                double distance = position.TradeType == TradeType.Buy
                    ? currentExit - position.StopLoss.Value
                    : position.StopLoss.Value - currentExit;
                if (distance <= 0) return double.PositiveInfinity;
                total += distance / symbol.PipSize * symbol.PipValue * position.VolumeInUnits;
            }
            foreach (var order in PendingOrders.Where(x => IsMasterLabel(x.Label)))
            {
                int orderDirection = order.TradeType == TradeType.Buy ? 1 : -1;
                if (orderDirection != direction) continue;
                Symbol symbol = Symbols.GetSymbol(order.SymbolName);
                if (symbol == null || !order.StopLossPips.HasValue || order.StopLossPips.Value <= 0 || symbol.PipValue <= 0)
                    return double.PositiveInfinity;
                total += order.StopLossPips.Value * symbol.PipValue * order.VolumeInUnits;
            }
            return total;
        }

        private double MaxCorrelationAgainstExposure(string proposalEngine, int direction, out bool crossMarket)
        {
            crossMarket = false;
            double maxCorrelation = 0;
            foreach (var position in Positions.Where(x => IsMasterLabel(x.Label)))
            {
                int positionDirection = position.TradeType == TradeType.Buy ? 1 : -1;
                string engine = LabelEngine(position.Label);
                if (positionDirection != direction || engine == proposalEngine) continue;
                crossMarket = true;
                maxCorrelation = Math.Max(maxCorrelation, EstimateRollingCorrelation(proposalEngine, engine));
            }
            foreach (var order in PendingOrders.Where(x => IsMasterLabel(x.Label)))
            {
                int orderDirection = order.TradeType == TradeType.Buy ? 1 : -1;
                string engine = LabelEngine(order.Label);
                if (orderDirection != direction || engine == proposalEngine) continue;
                crossMarket = true;
                maxCorrelation = Math.Max(maxCorrelation, EstimateRollingCorrelation(proposalEngine, engine));
            }
            return crossMarket ? Math.Max(maxCorrelation, CorrelationStressFloor) : 0;
        }

        private double EstimateRollingCorrelation(string firstEngine, string secondEngine)
        {
            Bars first = BarsForEngine(firstEngine);
            Bars second = BarsForEngine(secondEngine);
            if (first == null || second == null) return CorrelationStressFloor;
            Dictionary<DateTime, double> a = DailyCloses(first);
            Dictionary<DateTime, double> b = DailyCloses(second);
            var common = a.Keys.Where(b.ContainsKey).OrderBy(x => x).ToList();
            if (common.Count < Math.Max(12, CorrelationLookbackDays / 2)) return CorrelationStressFloor;
            int start = Math.Max(1, common.Count - CorrelationLookbackDays - 1);
            var ar = new List<double>();
            var br = new List<double>();
            for (int i = start; i < common.Count; i++)
            {
                DateTime previous = common[i - 1];
                DateTime current = common[i];
                if (a[previous] <= 0 || b[previous] <= 0) continue;
                ar.Add(a[current] / a[previous] - 1.0);
                br.Add(b[current] / b[previous] - 1.0);
            }
            if (ar.Count < 10) return CorrelationStressFloor;
            double meanA = ar.Average();
            double meanB = br.Average();
            double covariance = 0;
            double varianceA = 0;
            double varianceB = 0;
            for (int i = 0; i < ar.Count; i++)
            {
                double da = ar[i] - meanA;
                double db = br[i] - meanB;
                covariance += da * db;
                varianceA += da * da;
                varianceB += db * db;
            }
            if (varianceA <= 0 || varianceB <= 0) return CorrelationStressFloor;
            return Math.Max(-1, Math.Min(1, covariance / Math.Sqrt(varianceA * varianceB)));
        }

        private Bars BarsForEngine(string engine)
        {
            if (engine == "DAX") return _daxM15;
            if (engine == "UK") return _ukM15;
            if (engine == "GOLD") return _goldH1;
            return _nasM5;
        }

        private Dictionary<DateTime, double> DailyCloses(Bars bars)
        {
            var closes = new Dictionary<DateTime, double>();
            if (bars == null) return closes;
            DateTime today = Server.TimeInUtc.Date;
            int desired = Math.Max(30, CorrelationLookbackDays + 10);
            for (int i = bars.Count - 2; i >= 0; i--)
            {
                DateTime day = bars.OpenTimes[i].Date;
                if (day >= today) continue;
                if (!closes.ContainsKey(day)) closes[day] = bars.ClosePrices[i];
                if (closes.Count >= desired) break;
            }
            return closes;
        }

        private double CalculateInitialRiskAtFill(Position position, Symbol symbol, double fallback)
        {
            if (position == null || symbol == null || !position.StopLoss.HasValue ||
                symbol.PipSize <= 0 || symbol.PipValue <= 0) return fallback;
            double distance = Math.Abs(position.EntryPrice - position.StopLoss.Value);
            double risk = distance / symbol.PipSize * symbol.PipValue * position.VolumeInUnits;
            return double.IsNaN(risk) || double.IsInfinity(risk) || risk <= 0 ? fallback : risk;
        }

        private void RecordExecutionQuality(TradeProposal proposal, double decisionPrice, double fillPrice,
            double brokerCallMs, bool evaluateLatency)
        {
            if (!ExecutionQualityKillOn || proposal == null || proposal.SlDistancePts <= 0) return;
            int direction = proposal.Side == TradeType.Buy ? 1 : -1;
            double adverseR = Math.Max(0, direction * (fillPrice - decisionPrice) / proposal.SlDistancePts);
            RecordExecutionSample(proposal.Label, evaluateLatency ? "entry_market" : "entry_limit",
                adverseR, brokerCallMs, 0, brokerCallMs, evaluateLatency, decisionPrice, fillPrice);
        }

        private void RecordExitExecutionQuality(Position position, PositionCloseReason closeReason, PositionMeta meta)
        {
            if (!ExecutionQualityKillOn || position == null || meta == null || meta.StopRiskDistancePts <= 0) return;
            Symbol symbol = Symbols.GetSymbol(position.SymbolName);
            if (symbol == null || symbol.PipSize <= 0) return;

            double expectedPrice = 0;
            double brokerCallMs = 0;
            double eventConfirmationMs = 0;
            double totalRequestMs = 0;
            bool evaluateLatency = false;
            string channel = "exit_unclassified";
            CloseRequestMeta closeRequest;
            if (_closeRequestMeta.TryGetValue(position.Id, out closeRequest))
            {
                expectedPrice = closeRequest.DecisionPrice;
                totalRequestMs = Math.Max(0, (DateTime.UtcNow - closeRequest.RequestedWallTimeUtc).TotalMilliseconds);
                if (closeRequest.BrokerCallCompleted)
                {
                    brokerCallMs = Math.Max(0, closeRequest.BrokerCallMs);
                    eventConfirmationMs = Math.Max(0,
                        (DateTime.UtcNow - closeRequest.BrokerCallReturnedWallTimeUtc).TotalMilliseconds);
                    evaluateLatency = true;
                }
                else if (closeRequest.RequestMonotonicTimestamp > 0)
                {
                    // Some cTrader builds may raise Positions.Closed synchronously inside
                    // ClosePosition(). In that ordering the elapsed monotonic time is still broker-
                    // call time; there is no post-return event-confirmation interval to include.
                    brokerCallMs = MonotonicElapsedMilliseconds(closeRequest.RequestMonotonicTimestamp);
                    totalRequestMs = brokerCallMs;
                    evaluateLatency = true;
                }
                channel = "managed_exit";
            }
            else if (closeReason == PositionCloseReason.StopLoss)
            {
                expectedPrice = meta.LastStopPrice > 0 ? meta.LastStopPrice : (position.StopLoss ?? 0);
                channel = "stop_exit";
            }
            else if (closeReason == PositionCloseReason.TakeProfit)
            {
                expectedPrice = meta.LastTakeProfitPrice > 0 ? meta.LastTakeProfitPrice : (position.TakeProfit ?? 0);
                channel = "target_exit";
            }

            if (expectedPrice <= 0 || double.IsNaN(expectedPrice) || double.IsInfinity(expectedPrice)) return;
            double actualExitPrice = position.TradeType == TradeType.Buy
                ? position.EntryPrice + position.Pips * symbol.PipSize
                : position.EntryPrice - position.Pips * symbol.PipSize;
            if (actualExitPrice <= 0 || double.IsNaN(actualExitPrice) || double.IsInfinity(actualExitPrice)) return;
            int direction = position.TradeType == TradeType.Buy ? 1 : -1;
            double adverseR = Math.Max(0, direction * (expectedPrice - actualExitPrice) / meta.StopRiskDistancePts);
            RecordExecutionSample(position.Label, channel, adverseR, brokerCallMs, eventConfirmationMs,
                totalRequestMs, evaluateLatency, expectedPrice, actualExitPrice);
        }

        private void RecordExecutionSample(string label, string channel, double adverseR, double brokerCallMs,
            double eventConfirmationMs, double totalRequestMs, bool evaluateLatency,
            double expectedPrice, double actualPrice)
        {
            if (!ExecutionQualityKillOn) return;
            _adverseSlippageR.Enqueue(adverseR);
            while (_adverseSlippageR.Count > Math.Max(5, ExecutionSampleWindow)) _adverseSlippageR.Dequeue();
            double mean = _adverseSlippageR.Count > 0 ? _adverseSlippageR.Average() : 0;
            bool hardSlippageIncident = adverseR >= SingleAdverseSlippageIncidentR ||
                (_adverseSlippageR.Count >= Math.Min(10, Math.Max(5, ExecutionSampleWindow)) &&
                 mean >= MeanAdverseSlippageHaltR);
            double latencyThresholdMs = channel == "managed_exit" ? CloseLatencyIncidentMs : MarketLatencyIncidentMs;
            bool latencyIncident = RunningMode == RunningMode.RealTime && evaluateLatency &&
                brokerCallMs >= latencyThresholdMs;
            string incidentClass = hardSlippageIncident ? "hard_slippage" :
                (latencyIncident ? "latency_only" : "none");
            Journal("EXECUTION_QUALITY",
                "\"label\":\"" + Js(label) + "\",\"channel\":\"" + Js(channel) +
                "\",\"expected_price\":" + Jn(expectedPrice) + ",\"actual_price\":" + Jn(actualPrice) +
                ",\"adverse_slippage_r\":" + Jn(adverseR) +
                ",\"rolling_mean_r\":" + Jn(mean) + ",\"samples\":" + _adverseSlippageR.Count +
                ",\"latency_ms\":" + Jn(brokerCallMs) +
                ",\"broker_call_ms\":" + Jn(brokerCallMs) +
                ",\"event_confirmation_ms\":" + Jn(eventConfirmationMs) +
                ",\"total_request_ms\":" + Jn(totalRequestMs) +
                ",\"latency_evaluated\":" + Bool(evaluateLatency) +
                ",\"incident\":" + Bool(hardSlippageIncident || latencyIncident) +
                ",\"incident_class\":\"" + incidentClass + "\"");
            if (hardSlippageIncident)
                RecordExecutionIncident(channel + "_adverse_slippage", adverseR, brokerCallMs);
            else if (latencyIncident)
                RecordLatencyExecutionIncident(channel + "_latency", adverseR, brokerCallMs);
            else
            {
                // Only an actually measured, healthy broker call breaks a latency streak. Limit
                // fills and server-side SL/TP exits have no comparable synchronous call duration.
                if (RunningMode == RunningMode.RealTime && evaluateLatency) _latencyIncidentStreak = 0;
                if (!TryPersistExecutionQualityState() && BindingAuthorityCurrent())
                    CancelAllMasterPending();
            }
        }

        private void RecordExecutionIncident(string kind, double adverseR, double latencyMs)
        {
            // Operational and protection faults are invariant safety failures. The optional
            // execution-quality switch controls latency/slippage sampling, never these hard faults.
            EnterExecutionHardHalt(kind, adverseR, latencyMs);
            Journal("EXECUTION_INCIDENT",
                "\"kind\":\"" + Js(kind) + "\",\"class\":\"hard\"" +
                ",\"adverse_slippage_r\":" + Jn(adverseR) + ",\"broker_call_ms\":" + Jn(latencyMs));
        }

        private void RecordLatencyExecutionIncident(string kind, double adverseR, double brokerCallMs)
        {
            if (!ExecutionQualityKillOn || _executionGuardState == ExecutionGuardState.HardHalt) return;
            DateTime now = Server.TimeInUtc;
            if ((_lastLatencyIncidentUtc != DateTime.MinValue && now < _lastLatencyIncidentUtc.AddMinutes(-1)) ||
                (_latencyWindowStartUtc != DateTime.MinValue && now < _latencyWindowStartUtc.AddMinutes(-1)))
            {
                EnterExecutionHardHalt("execution_guard_clock_regression", adverseR, brokerCallMs);
                return;
            }
            if (_latencyWindowStartUtc == DateTime.MinValue ||
                now - _latencyWindowStartUtc >= TimeSpan.FromHours(Math.Max(1, LatencyEscalationWindowHours)))
            {
                _latencyWindowStartUtc = now;
                _latencyEpisodeCount = 0;
            }
            _lastLatencyIncidentUtc = now;
            _latencyIncidentStreak++;
            Journal("EXECUTION_INCIDENT",
                "\"kind\":\"" + Js(kind) + "\",\"class\":\"latency_only\"" +
                ",\"streak\":" + _latencyIncidentStreak +
                ",\"adverse_slippage_r\":" + Jn(adverseR) +
                ",\"broker_call_ms\":" + Jn(brokerCallMs));

            if (_latencyIncidentStreak < Math.Max(1, ConsecutiveExecutionIncidents))
            {
                if (!TryPersistExecutionQualityState() && BindingAuthorityCurrent())
                    CancelAllMasterPending();
                return;
            }

            _latencyIncidentStreak = 0;
            _latencyEpisodeCount++;
            if (_latencyEpisodeCount >= Math.Max(1, LatencyEpisodesToHardHalt))
            {
                EnterExecutionHardHalt("repeated_latency_episodes", adverseR, brokerCallMs);
                return;
            }

            bool newlyBlocked = _executionGuardState == ExecutionGuardState.Armed;
            _executionGuardState = ExecutionGuardState.LatencyCooldown;
            _executionGuardReason = "latency_threshold";
            _executionStateSinceUtc = now;
            _latencyCooldownUntilUtc = now.AddMinutes(Math.Max(5, LatencyCooldownMinutes));
            _executionAutoResumePending = false;
            _executionFlatBookObservedUtc = DateTime.MinValue;
            SuspendDeploymentForExecutionGuard();
            bool persisted = TryPersistExecutionQualityState();
            if (newlyBlocked && BindingAuthorityCurrent()) CancelAllMasterPending();
            Journal("GUARD",
                "\"kind\":\"execution_latency_cooldown\",\"auto_recovery\":true" +
                ",\"persisted\":" + Bool(persisted) +
                ",\"resume_utc\":\"" + StateDateText(_latencyCooldownUntilUtc) + "\"" +
                ",\"episode\":" + _latencyEpisodeCount +
                ",\"hard_halt_at_episode\":" + Math.Max(1, LatencyEpisodesToHardHalt));
        }

        private void EnterExecutionHardHalt(string reason, double adverseR, double brokerCallMs)
        {
            bool newlyHalted = _executionGuardState != ExecutionGuardState.HardHalt;
            _executionGuardState = ExecutionGuardState.HardHalt;
            _executionGuardReason = SanitizeExecutionReason(reason);
            if (IsExecutionStorageIntegrityReason(_executionGuardReason))
                _executionStorageIntegrityBlocker = true;
            _executionStateSinceUtc = Server.TimeInUtc;
            _latencyCooldownUntilUtc = DateTime.MinValue;
            _latencyIncidentStreak = 0;
            _executionAutoResumePending = false;
            _executionFlatBookObservedUtc = DateTime.MinValue;
            SuspendDeploymentForExecutionGuard();
            bool latchPersisted = TrySetExecutionFailsafeLatch();
            bool persisted = TryPersistExecutionQualityState() && latchPersisted;
            if (newlyHalted && BindingAuthorityCurrent()) CancelAllMasterPending();
            Journal("GUARD",
                "\"kind\":\"execution_quality_hard_halt\",\"manual_rearm_required\":true" +
                ",\"reason\":\"" + Js(_executionGuardReason) + "\"" +
                ",\"adverse_slippage_r\":" + Jn(adverseR) +
                ",\"broker_call_ms\":" + Jn(brokerCallMs) +
                ",\"persisted\":" + Bool(persisted));
        }

        private void SuspendDeploymentForExecutionGuard()
        {
            if (RunningMode != RunningMode.RealTime) return;
            _deploymentArmed = false;
            if (_bindingValid) _deploymentState = DeploymentState.Blocked;
        }

        private void TryAutoRecoverExecutionLatencyCooldown()
        {
            if (RunningMode != RunningMode.RealTime ||
                _executionGuardState != ExecutionGuardState.LatencyCooldown) return;
            DateTime now = Server.TimeInUtc;
            if (_executionStateSinceUtc == DateTime.MinValue || _latencyCooldownUntilUtc == DateTime.MinValue ||
                now < _executionStateSinceUtc.AddMinutes(-1) ||
                (_lastLatencyIncidentUtc != DateTime.MinValue && now < _lastLatencyIncidentUtc.AddMinutes(-1)))
            {
                EnterExecutionHardHalt("execution_guard_clock_regression", 0, 0);
                return;
            }
            if (now < _latencyCooldownUntilUtc) return;
            if (Positions.Any() || PendingOrders.Any())
            {
                _executionFlatBookObservedUtc = DateTime.MinValue;
                return;
            }
            if (_executionFlatBookObservedUtc == DateTime.MinValue)
                _executionFlatBookObservedUtc = now;
            if (!ExecutionGuardBookkeepingDrained())
            {
                // A late Closed/Cancelled event may lag the broker response briefly. It must not
                // block forever: after a bounded grace, missing event reconciliation is a hard
                // operational fault rather than a latency-only condition.
                if (now < _executionFlatBookObservedUtc.AddSeconds(30)) return;
                EnterExecutionHardHalt("execution_event_bookkeeping_not_drained", 0, 0);
                return;
            }
            if (!BindingAuthorityCurrent()) return;

            _executionGuardState = ExecutionGuardState.Armed;
            _executionGuardReason = "latency_cooldown_completed";
            _executionStateSinceUtc = now;
            _latencyCooldownUntilUtc = DateTime.MinValue;
            _latencyIncidentStreak = 0;
            _executionFlatBookObservedUtc = DateTime.MinValue;
            if (!TryPersistExecutionQualityState()) return;
            _executionAutoResumePending = true;
            Journal("GUARD",
                "\"kind\":\"execution_latency_auto_recovered\",\"flat\":true" +
                ",\"episode_count\":" + _latencyEpisodeCount +
                ",\"window_start_utc\":\"" + StateDateText(_latencyWindowStartUtc) + "\"");
        }

        private bool ExecutionGuardBookkeepingDrained()
        {
            return !Positions.Any() && !PendingOrders.Any() && !_gateBusy &&
                _transientReservedRisk <= 0.01 && _closeRequestMeta.Count == 0 &&
                _closeRetry.Count == 0 && _cancelRequested.Count == 0 &&
                _cancelRetry.Count == 0 && _closeRetryNotBeforeTicks.Count == 0 &&
                _cancelRetryNotBeforeTicks.Count == 0 && _cancelRequestMonotonicTicks.Count == 0 &&
                _pendingInfo.Count == 0 &&
                _positionMeta.Count == 0 && _currentRunCreatedPositionIds.Count == 0 &&
                _currentRunCreatedPendingIds.Count == 0 &&
                _directLimitTerminalExpectations.Count == 0 &&
                (RunningMode != RunningMode.RealTime || _ownershipLedger.Count == 0);
        }

        private void InitialiseExecutionQualityState()
        {
            _deferExecutionFailsafeWritesUntilReceiptContext = true;
            _executionStatePath = System.IO.Path.Combine(_journalDir,
                "Atlas3_" + ProfileCode + "_acc" + Account.Number + "_execution_quality.state");
            _executionFailsafeKey = "Atlas3 S1 V311 X " +
                ExecutionStateAccountHash().Substring(0, 16);
            // Device scope survives cBot class/name changes. The account/profile hash keeps
            // independent Atlas profiles and trading accounts isolated from one another.
            _executionFailsafeDeviceKey = "Atlas3 Execution Failsafe " +
                ExecutionStateAccountHash().Substring(0, 16);
            if (!IsValidLocalStorageKey(_executionFailsafeKey) ||
                !IsValidLocalStorageKey(_executionFailsafeDeviceKey))
            {
                _executionStorageIntegrityBlocker = true;
                _executionGuardState = ExecutionGuardState.HardHalt;
                _executionGuardReason = "execution_failsafe_storage_key_invalid";
                _executionStateSinceUtc = Server.TimeInUtc;
                HaltStartup("execution failsafe local-storage key is invalid");
                return;
            }
            if (RunningMode != RunningMode.RealTime)
            {
                _deferExecutionFailsafeWritesUntilReceiptContext = false;
                _executionGuardState = ExecutionGuardState.Armed;
                _executionGuardReason = "backtest";
                _executionStateSinceUtc = Server.TimeInUtc;
                return;
            }

            try
            {
                _startupExecutionTypeLatch = GetLocalStorageStringChecked(
                    _executionFailsafeKey, LocalStorageScope.Type) ?? "";
                _startupExecutionDeviceLatch = GetLocalStorageStringChecked(
                    _executionFailsafeDeviceKey, LocalStorageScope.Device) ?? "";
                _executionFailsafeLatchPreexistedAtStartup =
                    !string.IsNullOrWhiteSpace(_startupExecutionTypeLatch) ||
                    !string.IsNullOrWhiteSpace(_startupExecutionDeviceLatch);
            }
            catch
            {
                // Storage uncertainty must never be mistaken for a clean first installation.
                _executionStorageIntegrityBlocker = true;
                _executionFailsafeSnapshotReadFailed = true;
                _executionFailsafeLatchPreexistedAtStartup = true;
            }

            string raw = "";
            bool readFailed = false;
            try
            {
                raw = System.IO.File.Exists(_executionStatePath)
                    ? System.IO.File.ReadAllText(_executionStatePath).Trim()
                    : "";
            }
            catch (Exception ex)
            {
                readFailed = true;
                SetExecutionHardHaltFromLoad("execution_state_read_failed_" + ex.GetType().Name, "");
            }
            if (!readFailed && string.IsNullOrWhiteSpace(raw))
            {
                _executionStateCreatedFreshAtStartup = true;
                // A missing state file cannot prove that an earlier persistent halt never
                // existed. Require one deliberate, flat-book, fresh-nonce rearm.
                SetExecutionHardHaltFromLoad("execution_state_missing_manual_rearm_required", "");
            }
            else if (!readFailed)
                LoadExecutionQualityState(raw);

            LoadExecutionFailsafeLatch();
        }

        private void TryBootstrapFreshExecutionStateForEnrollment()
        {
            if (RunningMode != RunningMode.RealTime || !_executionStateCreatedFreshAtStartup ||
                _executionFailsafeLatchPreexistedAtStartup || _bindingValid ||
                !string.Equals(_bindingStatus, "first-run confirmation required", StringComparison.Ordinal) ||
                _startupHalt || !_accountLeaseOwned || !ExecutionGuardBookkeepingDrained())
                return;

            _executionGuardState = ExecutionGuardState.Armed;
            _executionGuardReason = "fresh_install_pending_enrollment";
            _executionStateSinceUtc = Server.TimeInUtc;
            _latencyCooldownUntilUtc = DateTime.MinValue;
            _latencyIncidentStreak = 0;
            _latencyEpisodeCount = 0;
            _latencyWindowStartUtc = DateTime.MinValue;
            _lastLatencyIncidentUtc = DateTime.MinValue;
            _executionFlatBookObservedUtc = DateTime.MinValue;
            _adverseSlippageR.Clear();
            if (!TryPersistExecutionQualityState() || !TryClearExecutionFailsafeLatch())
            {
                _executionStorageIntegrityBlocker = true;
                _executionGuardState = ExecutionGuardState.HardHalt;
                _executionGuardReason = "fresh_install_execution_bootstrap_failed";
                TryPersistExecutionQualityState();
                TrySetExecutionFailsafeLatch();
                return;
            }
            _executionStorageIntegrityBlocker = false;
            _executionFailsafeLatchedReason = "";
            _legacyDeviceOnlyExecutionLatch = false;
            _executionStateCreatedFreshAtStartup = false;
            Journal("GUARD", "\"kind\":\"fresh_install_execution_bootstrap\",\"flat\":true");
        }

        private void LoadExecutionQualityState(string raw)
        {
            if (string.Equals(raw, "ARMED", StringComparison.Ordinal) ||
                string.Equals(raw, "HALTED", StringComparison.Ordinal))
            {
                ArchiveLegacyExecutionState(raw);
                _executionGuardState = raw == "ARMED"
                    ? ExecutionGuardState.Armed
                    : ExecutionGuardState.HardHalt;
                _executionGuardReason = raw == "ARMED" ? "legacy_v1_armed_migrated" : "legacy_v1_untyped_halt";
                _executionStateSinceUtc = Server.TimeInUtc;
                _latencyCooldownUntilUtc = DateTime.MinValue;
                _latencyIncidentStreak = 0;
                _latencyEpisodeCount = 0;
                _adverseSlippageR.Clear();
                TryPersistExecutionQualityState();
                if (_executionGuardState == ExecutionGuardState.HardHalt)
                    TrySetExecutionFailsafeLatch();
                return;
            }

            Dictionary<string, string> values;
            string error;
            if (!TryParseExecutionQualityState(raw, out values, out error))
            {
                SetExecutionHardHaltFromLoad("execution_state_invalid_" + error, raw);
                return;
            }

            ExecutionGuardState parsedState = ReceiptValue(values, "state") == "ARMED"
                ? ExecutionGuardState.Armed
                : (ReceiptValue(values, "state") == "LATENCY_COOLDOWN"
                    ? ExecutionGuardState.LatencyCooldown
                    : ExecutionGuardState.HardHalt);
            DateTime stateSince;
            DateTime resume;
            DateTime windowStart;
            DateTime lastLatency;
            if (!TryParseStateDate(ReceiptValue(values, "state_since_utc"), false, out stateSince) ||
                !TryParseStateDate(ReceiptValue(values, "resume_utc"), true, out resume) ||
                !TryParseStateDate(ReceiptValue(values, "window_start_utc"), true, out windowStart) ||
                !TryParseStateDate(ReceiptValue(values, "last_latency_utc"), true, out lastLatency))
            {
                SetExecutionHardHaltFromLoad("execution_state_invalid_timestamp", raw);
                return;
            }
            int streak;
            int episodes;
            if (!int.TryParse(ReceiptValue(values, "latency_streak"), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out streak) || streak < 0 || streak > 100000 ||
                !int.TryParse(ReceiptValue(values, "latency_episodes"), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out episodes) || episodes < 0 || episodes > 100000)
            {
                SetExecutionHardHaltFromLoad("execution_state_invalid_counter", raw);
                return;
            }
            var samples = new List<double>();
            string sampleText = ReceiptValue(values, "slip_samples");
            if (sampleText != "-")
            {
                foreach (string item in sampleText.Split(','))
                {
                    double sample;
                    if (!double.TryParse(item, NumberStyles.Float, CultureInfo.InvariantCulture, out sample) ||
                        double.IsNaN(sample) || double.IsInfinity(sample) || sample < 0 || sample > 1000)
                    {
                        SetExecutionHardHaltFromLoad("execution_state_invalid_sample", raw);
                        return;
                    }
                    samples.Add(sample);
                }
            }
            if (samples.Count > Math.Max(100, ExecutionSampleWindow))
            {
                SetExecutionHardHaltFromLoad("execution_state_excess_samples", raw);
                return;
            }
            DateTime now = Server.TimeInUtc;
            if (stateSince > now.AddMinutes(5) || windowStart > now.AddMinutes(5) ||
                lastLatency > now.AddMinutes(5) ||
                (parsedState == ExecutionGuardState.LatencyCooldown && resume == DateTime.MinValue))
            {
                SetExecutionHardHaltFromLoad("execution_state_clock_or_resume_invalid", raw);
                return;
            }

            _executionGuardState = parsedState;
            _executionGuardReason = SanitizeExecutionReason(ReceiptValue(values, "reason"));
            if (_executionGuardState == ExecutionGuardState.HardHalt &&
                IsExecutionStorageIntegrityReason(_executionGuardReason))
                _executionStorageIntegrityBlocker = true;
            _executionStateSinceUtc = stateSince;
            _latencyCooldownUntilUtc = resume;
            _latencyWindowStartUtc = windowStart;
            _lastLatencyIncidentUtc = lastLatency;
            _latencyIncidentStreak = streak;
            _latencyEpisodeCount = episodes;
            _lastExecutionRearmNonceHash = ReceiptValue(values, "last_rearm_nonce_hash");
            _adverseSlippageR.Clear();
            foreach (double sample in samples) _adverseSlippageR.Enqueue(sample);
            if (_executionGuardState != ExecutionGuardState.Armed) SuspendDeploymentForExecutionGuard();
            if (_executionGuardState == ExecutionGuardState.HardHalt)
                TrySetExecutionFailsafeLatch();
        }

        private bool TryParseExecutionQualityState(string raw, out Dictionary<string, string> values, out string error)
        {
            if (!TryParseReceipt(raw, out values, out error)) return false;
            string[] required =
            {
                "schema", "profile", "account_hash", "state", "reason", "state_since_utc",
                "resume_utc", "latency_streak", "latency_episodes", "window_start_utc",
                "last_latency_utc", "slip_samples", "last_rearm_nonce_hash"
            };
            bool fieldSetValid = values.Count == required.Length;
            foreach (string key in required)
                if (!values.ContainsKey(key)) fieldSetValid = false;
            if (!fieldSetValid)
            {
                error = "field_set";
                return false;
            }
            if (ReceiptValue(values, "schema") != ExecutionStateSchema ||
                ReceiptValue(values, "profile") != ProfileCode ||
                ReceiptValue(values, "account_hash") != ExecutionStateAccountHash())
            {
                error = "identity";
                return false;
            }
            string state = ReceiptValue(values, "state");
            if (state != "ARMED" && state != "LATENCY_COOLDOWN" && state != "HARD_HALT")
            {
                error = "state";
                return false;
            }
            string nonceHash = ReceiptValue(values, "last_rearm_nonce_hash");
            if (nonceHash != "-" && (nonceHash.Length != 64 || !nonceHash.All(IsLowerHex)))
            {
                error = "rearm_hash";
                return false;
            }
            return true;
        }

        private void SetExecutionHardHaltFromLoad(string reason, string raw)
        {
            if (!string.IsNullOrWhiteSpace(raw)) ArchiveLegacyExecutionState(raw);
            _executionGuardState = ExecutionGuardState.HardHalt;
            _executionGuardReason = SanitizeExecutionReason(reason);
            if (IsExecutionStorageIntegrityReason(_executionGuardReason))
                _executionStorageIntegrityBlocker = true;
            _executionStateSinceUtc = Server.TimeInUtc;
            _latencyCooldownUntilUtc = DateTime.MinValue;
            _latencyIncidentStreak = 0;
            _executionAutoResumePending = false;
            _executionFlatBookObservedUtc = DateTime.MinValue;
            _adverseSlippageR.Clear();
            SuspendDeploymentForExecutionGuard();
            TryPersistExecutionQualityState();
            TrySetExecutionFailsafeLatch();
        }

        private void ArchiveLegacyExecutionState(string raw)
        {
            try
            {
                string backup = _executionStatePath + ".pre-r2.bak";
                if (!System.IO.File.Exists(backup)) WriteTextAtomically(backup, raw);
            }
            catch { }
        }

        private void LoadExecutionFailsafeLatch()
        {
            try
            {
                if (_executionFailsafeSnapshotReadFailed)
                    throw new InvalidOperationException("startup execution latch snapshot unavailable");
                string typeLatch = _startupExecutionTypeLatch ?? "";
                string deviceLatch = _startupExecutionDeviceLatch ?? "";
                if (string.IsNullOrWhiteSpace(typeLatch) && string.IsNullOrWhiteSpace(deviceLatch)) return;
                bool typePresent = !string.IsNullOrWhiteSpace(typeLatch);
                bool devicePresent = !string.IsNullOrWhiteSpace(deviceLatch);
                bool typeLegacy = false;
                bool deviceLegacy = false;
                bool typeValid = !typePresent || InspectExecutionFailsafeLatch(typeLatch, out typeLegacy);
                bool deviceValid = !devicePresent || InspectExecutionFailsafeLatch(deviceLatch, out deviceLegacy);

                if (!typeValid || !deviceValid || (typePresent && typeLegacy) ||
                    (typePresent && devicePresent &&
                     !string.Equals(typeLatch, deviceLatch, StringComparison.Ordinal)) ||
                    (typePresent && !devicePresent))
                    _executionStorageIntegrityBlocker = true;
                else if (!typePresent && devicePresent)
                {
                    if (deviceLegacy && deviceValid)
                        _legacyDeviceOnlyExecutionLatch = true;
                    else
                        _executionStorageIntegrityBlocker = true;
                }
                _executionGuardState = ExecutionGuardState.HardHalt;
                _executionGuardReason = "execution_state_failsafe_latch";
                _executionStateSinceUtc = Server.TimeInUtc;
                _latencyCooldownUntilUtc = DateTime.MinValue;
                SuspendDeploymentForExecutionGuard();
                TryPersistExecutionQualityState();
            }
            catch
            {
                _executionStorageIntegrityBlocker = true;
                _executionGuardState = ExecutionGuardState.HardHalt;
                _executionGuardReason = "execution_failsafe_storage_unavailable";
                _executionStateSinceUtc = Server.TimeInUtc;
                _latencyCooldownUntilUtc = DateTime.MinValue;
                _executionAutoResumePending = false;
                SuspendDeploymentForExecutionGuard();
                TryPersistExecutionQualityState();
            }
        }

        private bool InspectExecutionFailsafeLatch(string payload, out bool legacy)
        {
            legacy = false;
            if (string.IsNullOrWhiteSpace(payload)) return false;
            string[] parts = payload.Split('|');
            legacy = parts.Length == 3;
            bool current = parts.Length == 4 && parts[3] == "V310";
            DateTime writtenUtc;
            if ((!legacy && !current) || parts[0] != "HARD_HALT" ||
                string.IsNullOrWhiteSpace(parts[1]) ||
                !TryParseStateDate(parts[2], false, out writtenUtc) ||
                writtenUtc > Server.TimeInUtc.AddMinutes(5))
            {
                _executionStorageIntegrityBlocker = true;
                return false;
            }
            string latchedReason = SanitizeExecutionReason(parts[1]);
            if (string.IsNullOrEmpty(_executionFailsafeLatchedReason))
                _executionFailsafeLatchedReason = latchedReason;
            if (IsExecutionStorageIntegrityReason(latchedReason))
                _executionStorageIntegrityBlocker = true;
            return true;
        }

        private void ResolveLegacyDeviceOnlyExecutionLatchContext()
        {
            bool exactMigrationContext = !_bindingValid &&
                string.Equals(_bindingStatus, "first-run confirmation required", StringComparison.Ordinal);
            bool allowedLegacyBridge = _legacyDeviceOnlyExecutionLatch &&
                exactMigrationContext && !_executionStorageIntegrityBlocker;

            if (_legacyDeviceOnlyExecutionLatch && !exactMigrationContext)
            {
                _executionStorageIntegrityBlocker = true;
                _executionFailsafeLatchedReason = "execution_failsafe_storage_partial_write";
            }
            if (_executionGuardState == ExecutionGuardState.HardHalt &&
                !_executionFailsafeLatchPreexistedAtStartup &&
                !(_executionStateCreatedFreshAtStartup && exactMigrationContext))
            {
                _executionStorageIntegrityBlocker = true;
                _executionFailsafeLatchedReason = "execution_failsafe_storage_missing_latch";
            }

            _deferExecutionFailsafeWritesUntilReceiptContext = false;
            bool freshBootstrapCandidate = _executionStateCreatedFreshAtStartup &&
                !_executionFailsafeLatchPreexistedAtStartup && exactMigrationContext;
            if (_executionGuardState != ExecutionGuardState.HardHalt ||
                freshBootstrapCandidate || allowedLegacyBridge)
                return;

            TryPersistExecutionQualityState();
            TrySetExecutionFailsafeLatch();
        }

        private void TryManualExecutionRearm()
        {
            if (_executionGuardState != ExecutionGuardState.HardHalt || !RearmExecutionHalt ||
                !ExecutionGuardBookkeepingDrained()) return;
            string nonce = (ExecutionRearmNonce ?? "").Trim();
            TryExecutionRearmTransaction(nonce, "manual_parameter");
        }

        private bool TryExecutionRearmTransaction(string nonce, string origin)
        {
            if (_executionRearmTransactionBusy) return false;
            _executionRearmTransactionBusy = true;
            try
            {
                if (RunningMode != RunningMode.RealTime)
                {
                    Print("ATLAS EXECUTION REARM REFUSED: real-time mode is required.");
                    return false;
                }
                if (_executionGuardState != ExecutionGuardState.HardHalt)
                {
                    Print("ATLAS EXECUTION REARM REFUSED: execution guard is not in HARD HALT.");
                    return false;
                }
                if (!_accountLeaseOwned)
                {
                    Print("ATLAS EXECUTION REARM REFUSED: this instance does not own the account lease.");
                    return false;
                }
                if (!ExecutionRearmJournalHealthy())
                {
                    Print("ATLAS EXECUTION REARM REFUSED: the daily safety journal is not fully healthy or is retrying.");
                    return false;
                }
                if (_bindingValid)
                {
                    if (!RuntimeIdentityMatchesReceipt() || !_managementAuthority ||
                        !_ownershipLedgerLoaded || _ownershipLedgerInvalid ||
                        !string.Equals(_ownershipLedgerStatus, "valid", StringComparison.Ordinal))
                    {
                        Print("ATLAS EXECUTION REARM REFUSED: the exact receipt and reconciled ownership ledger are required.");
                        return false;
                    }
                }
                else if (!string.Equals(
                    _bindingStatus, "first-run confirmation required", StringComparison.Ordinal))
                {
                    Print("ATLAS EXECUTION REARM REFUSED: the deployment receipt requires manual review.");
                    return false;
                }
                if (Positions.Any() || PendingOrders.Any() || !ExecutionGuardBookkeepingDrained())
                {
                    Print("ATLAS EXECUTION REARM REFUSED: the broker book and internal bookkeeping must be fully flat.");
                    return false;
                }
                nonce = (nonce ?? "").Trim();
                if (nonce.Length < 4 || nonce.Length > 64 || !IsAscii(nonce))
                {
                    Print("ATLAS EXECUTION REARM REFUSED: provide a fresh 4-64 character ASCII nonce while flat.");
                    return false;
                }

                string previousReason = string.Equals(
                        _executionGuardReason, "execution_state_failsafe_latch", StringComparison.Ordinal) &&
                    !string.IsNullOrWhiteSpace(_executionFailsafeLatchedReason)
                        ? _executionFailsafeLatchedReason
                        : _executionGuardReason;
                string nonceHash = ReceiptChecksum(nonce);
                string consumedNoncePath = _executionStatePath + ".rearm";
                string consumedHash = "";
                try
                {
                    if (System.IO.File.Exists(consumedNoncePath))
                    {
                        string stored = System.IO.File.ReadAllText(consumedNoncePath).Trim();
                        consumedHash = stored.StartsWith("sha256:", StringComparison.Ordinal)
                            ? stored.Substring("sha256:".Length)
                            : ReceiptChecksum(stored);
                    }
                }
                catch
                {
                    EnterExecutionHardHalt("execution_rearm_nonce_read_failed", 0, 0);
                    return false;
                }
                if (string.Equals(nonceHash, consumedHash, StringComparison.Ordinal) ||
                    string.Equals(nonceHash, _lastExecutionRearmNonceHash, StringComparison.Ordinal))
                {
                    Print("ATLAS EXECUTION REARM REFUSED: nonce was already consumed.");
                    return false;
                }
                try
                {
                    WriteTextAtomically(consumedNoncePath, "sha256:" + nonceHash);
                    string readback = System.IO.File.ReadAllText(consumedNoncePath).Trim();
                    if (readback != "sha256:" + nonceHash)
                        throw new InvalidOperationException("nonce write/readback mismatch");
                }
                catch
                {
                    EnterExecutionHardHalt("execution_rearm_nonce_persistence_failed", 0, 0);
                    return false;
                }

                _executionStorageIntegrityBlocker = false;
                _executionGuardState = ExecutionGuardState.Armed;
                _executionGuardReason = string.Equals(origin, "hud_one_click", StringComparison.Ordinal)
                    ? "one_click_rearm"
                    : "manual_rearm";
                _executionStateSinceUtc = Server.TimeInUtc;
                _latencyCooldownUntilUtc = DateTime.MinValue;
                _latencyIncidentStreak = 0;
                _latencyEpisodeCount = 0;
                _latencyWindowStartUtc = DateTime.MinValue;
                _lastLatencyIncidentUtc = DateTime.MinValue;
                _lastExecutionRearmNonceHash = nonceHash;
                _executionFlatBookObservedUtc = DateTime.MinValue;
                _adverseSlippageR.Clear();
                if (!TryPersistExecutionQualityState() || !TryClearExecutionFailsafeLatch())
                {
                    _executionStorageIntegrityBlocker = true;
                    _executionGuardState = ExecutionGuardState.HardHalt;
                    _executionGuardReason = "execution_rearm_commit_failed";
                    TryPersistExecutionQualityState();
                    TrySetExecutionFailsafeLatch();
                    return false;
                }
                _executionStateCreatedFreshAtStartup = false;
                _executionFailsafeLatchedReason = "";
                _legacyDeviceOnlyExecutionLatch = false;
                _executionAutoResumePending = _bindingValid && _runtimeActivated;
                string eventKind = string.Equals(origin, "manual_parameter", StringComparison.Ordinal)
                    ? "execution_quality_manual_rearm"
                    : "execution_quality_one_click_rearm";
                Journal("GUARD",
                    "\"kind\":\"" + eventKind + "\",\"origin\":\"" + Js(origin) +
                    "\",\"previous_reason\":\"" + Js(previousReason) +
                    "\",\"flat\":true,\"nonce_hash_prefix\":\"" +
                    nonceHash.Substring(0, 12) + "\"");
                if (!ExecutionRearmJournalHealthy())
                {
                    _executionAutoResumePending = false;
                    EnterExecutionHardHalt("execution_rearm_journal_commit_failed", 0, 0);
                    return false;
                }
                return true;
            }
            finally
            {
                _executionRearmTransactionBusy = false;
            }
        }

        private bool TryPersistExecutionQualityState()
        {
            if (RunningMode != RunningMode.RealTime || string.IsNullOrWhiteSpace(_executionStatePath)) return true;
            try
            {
                System.IO.Directory.CreateDirectory(_journalDir);
                string payload = ExecutionQualityStatePayload();
                string signed = payload + "|checksum=" + ReceiptChecksum(payload);
                WriteTextAtomically(_executionStatePath, signed);
                string readback = System.IO.File.ReadAllText(_executionStatePath).Trim();
                if (!string.Equals(readback, signed, StringComparison.Ordinal))
                    throw new InvalidOperationException("execution state write/readback mismatch");
                return true;
            }
            catch (Exception ex)
            {
                _executionStorageIntegrityBlocker = true;
                _executionGuardState = ExecutionGuardState.HardHalt;
                _executionGuardReason = "execution_state_persistence_failed_" + ex.GetType().Name;
                _executionStateSinceUtc = Server.TimeInUtc;
                _latencyCooldownUntilUtc = DateTime.MinValue;
                SuspendDeploymentForExecutionGuard();
                TrySetExecutionFailsafeLatch();
                Print("*** ATLAS EXECUTION STATE PERSISTENCE FAILED: HARD HALT LATCHED IN MEMORY. ***");
                return false;
            }
        }

        private string ExecutionQualityStatePayload()
        {
            string samples = _adverseSlippageR.Count == 0
                ? "-"
                : string.Join(",", _adverseSlippageR.Select(x => x.ToString("G17", CultureInfo.InvariantCulture)));
            return "schema=" + ExecutionStateSchema +
                "|profile=" + ProfileCode +
                "|account_hash=" + ExecutionStateAccountHash() +
                "|state=" + ExecutionGuardStateText() +
                "|reason=" + ExecutionGuardReasonForPersistence() +
                "|state_since_utc=" + StateDateText(_executionStateSinceUtc) +
                "|resume_utc=" + StateDateText(_latencyCooldownUntilUtc) +
                "|latency_streak=" + _latencyIncidentStreak.ToString(CultureInfo.InvariantCulture) +
                "|latency_episodes=" + _latencyEpisodeCount.ToString(CultureInfo.InvariantCulture) +
                "|window_start_utc=" + StateDateText(_latencyWindowStartUtc) +
                "|last_latency_utc=" + StateDateText(_lastLatencyIncidentUtc) +
                "|slip_samples=" + samples +
                "|last_rearm_nonce_hash=" +
                    (string.IsNullOrWhiteSpace(_lastExecutionRearmNonceHash) ? "-" : _lastExecutionRearmNonceHash);
        }

        private string ExecutionGuardReasonForPersistence()
        {
            string current = SanitizeExecutionReason(_executionGuardReason);
            if (string.Equals(current, "execution_state_failsafe_latch", StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(_executionFailsafeLatchedReason))
                current = SanitizeExecutionReason(_executionFailsafeLatchedReason);
            if (!_executionStorageIntegrityBlocker || IsExecutionStorageIntegrityReason(current))
                return current;
            return IsExecutionStorageIntegrityReason(_executionFailsafeLatchedReason)
                ? _executionFailsafeLatchedReason
                : "execution_failsafe_storage_manual_review_required";
        }

        private string ExecutionStateAccountHash()
        {
            return ReceiptChecksum(ProfileCode + "|" + AccountIdentityText());
        }

        private bool TrySetExecutionFailsafeLatch()
        {
            if (RunningMode != RunningMode.RealTime) return true;
            if (_deferExecutionFailsafeWritesUntilReceiptContext) return true;
            try
            {
                string latchReason = ExecutionGuardReasonForPersistence();
                string payload = "HARD_HALT|" + latchReason + "|" +
                    StateDateText(Server.TimeInUtc) + "|V310";
                SetLocalStorageStringChecked(_executionFailsafeKey, payload, LocalStorageScope.Type);
                LocalStorage.Flush(LocalStorageScope.Type);
                SetLocalStorageStringChecked(_executionFailsafeDeviceKey, payload, LocalStorageScope.Device);
                LocalStorage.Flush(LocalStorageScope.Device);
                string typeReadback = GetLocalStorageStringChecked(
                    _executionFailsafeKey, LocalStorageScope.Type) ?? "";
                string deviceReadback = GetLocalStorageStringChecked(
                    _executionFailsafeDeviceKey, LocalStorageScope.Device) ?? "";
                bool persisted = !string.IsNullOrWhiteSpace(typeReadback) &&
                    !string.IsNullOrWhiteSpace(deviceReadback) &&
                    string.Equals(typeReadback, payload, StringComparison.Ordinal) &&
                    string.Equals(deviceReadback, payload, StringComparison.Ordinal);
                if (!persisted) _executionStorageIntegrityBlocker = true;
                return persisted;
            }
            catch
            {
                _executionStorageIntegrityBlocker = true;
                return false;
            }
        }

        private bool TryClearExecutionFailsafeLatch()
        {
            if (RunningMode != RunningMode.RealTime) return true;
            try
            {
                SetLocalStorageStringChecked(_executionFailsafeKey, "", LocalStorageScope.Type);
                LocalStorage.Flush(LocalStorageScope.Type);
                SetLocalStorageStringChecked(_executionFailsafeDeviceKey, "", LocalStorageScope.Device);
                LocalStorage.Flush(LocalStorageScope.Device);
                string typeReadback = GetLocalStorageStringChecked(
                    _executionFailsafeKey, LocalStorageScope.Type) ?? "";
                string deviceReadback = GetLocalStorageStringChecked(
                    _executionFailsafeDeviceKey, LocalStorageScope.Device) ?? "";
                bool cleared = string.IsNullOrWhiteSpace(typeReadback) &&
                    string.IsNullOrWhiteSpace(deviceReadback);
                if (!cleared) _executionStorageIntegrityBlocker = true;
                return cleared;
            }
            catch
            {
                _executionStorageIntegrityBlocker = true;
                return false;
            }
        }

        private bool ExecutionGuardBlocked()
        {
            return _executionGuardState != ExecutionGuardState.Armed;
        }

        private string ExecutionGuardStateText()
        {
            if (_executionGuardState == ExecutionGuardState.LatencyCooldown) return "LATENCY_COOLDOWN";
            if (_executionGuardState == ExecutionGuardState.HardHalt) return "HARD_HALT";
            return "ARMED";
        }

        private static string SanitizeExecutionReason(string reason)
        {
            string value = string.IsNullOrWhiteSpace(reason) ? "none" : reason.Trim();
            value = value.Replace('|', '/').Replace('=', ':').Replace('\r', ' ').Replace('\n', ' ');
            return value.Length <= 120 ? value : value.Substring(0, 120);
        }

        private static string StateDateText(DateTime value)
        {
            if (value == DateTime.MinValue) return "-";
            DateTime utc = value.Kind == DateTimeKind.Utc
                ? value
                : (value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : DateTime.SpecifyKind(value, DateTimeKind.Utc));
            return utc.ToString("o", CultureInfo.InvariantCulture);
        }

        private static bool TryParseStateDate(string text, bool allowMissing, out DateTime value)
        {
            value = DateTime.MinValue;
            if (text == "-") return allowMissing;
            DateTime parsed;
            if (!DateTime.TryParseExact(text, "o", CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out parsed)) return false;
            value = parsed.Kind == DateTimeKind.Utc ? parsed : parsed.ToUniversalTime();
            return true;
        }

        private static bool IsLowerHex(char value)
        {
            return (value >= '0' && value <= '9') || (value >= 'a' && value <= 'f');
        }

        private static double MonotonicElapsedMilliseconds(long started)
        {
            if (started <= 0) return 0;
            long elapsed = Stopwatch.GetTimestamp() - started;
            return elapsed <= 0 ? 0 : elapsed * 1000.0 / Stopwatch.Frequency;
        }

        private static long MonotonicDeadlineAfterSeconds(double seconds)
        {
            double safeSeconds = Math.Max(0, seconds);
            return Stopwatch.GetTimestamp() +
                (long)Math.Ceiling(safeSeconds * Stopwatch.Frequency);
        }

        private void WriteTextAtomically(string path, string value)
        {
            string tempPath = path + ".tmp." + Guid.NewGuid().ToString("N");
            System.IO.File.WriteAllText(tempPath, value);
            if (System.IO.File.Exists(path))
                System.IO.File.Replace(tempPath, path, null);
            else
                System.IO.File.Move(tempPath, path);
        }

        private static string OwnershipLedgerKey(string exposureType, long brokerId)
        {
            return exposureType + ":" + brokerId.ToString(CultureInfo.InvariantCulture);
        }

        private string OwnershipLedgerHeader()
        {
            return "schema=" + OwnershipLedgerSchema +
                "|build=" + ReceiptBuild +
                "|profile=" + ProfileCode +
                "|account_hash=" + ReceiptChecksum(AccountIdentityText()) +
                "|receipt_hash=" + CurrentBindingLedgerHash() +
                "|strategy_hash=" + StrategyConfigurationReceiptHash() +
                "|config=" + ConfigurationFingerprint() +
                "|owner=" + (DeploymentOwnerId ?? "").Trim() +
                "|epoch=" + (StageEpochId ?? "").Trim();
        }

        private string CurrentBindingLedgerHash()
        {
            if (!_bindingValid) return "";
            return ReceiptChecksum(ReceiptPayload((DeploymentOwnerId ?? "").Trim(),
                (StageEpochId ?? "").Trim(), (EvaluationStartText ?? "").Trim(),
                (_receiptConfirmedUtc ?? "").Trim()));
        }

        private string OwnershipLedgerPayload()
        {
            var lines = new List<string> { OwnershipLedgerHeader() };
            foreach (OwnershipLedgerRecord record in _ownershipLedger.Values
                .OrderBy(x => x.ExposureType, StringComparer.Ordinal).ThenBy(x => x.BrokerId))
            {
                lines.Add("type=" + record.ExposureType +
                    ";id=" + record.BrokerId.ToString(CultureInfo.InvariantCulture) +
                    ";label=" + record.Label +
                    ";symbol=" + record.SymbolName +
                    ";side=" + record.Side +
                    ";volume=" + record.VolumeInUnits.ToString("R", CultureInfo.InvariantCulture) +
                    ";price=" + record.BrokerPrice.ToString("R", CultureInfo.InvariantCulture) +
                    ";time=" + AsUtc(record.BrokerTimeUtc).ToString("o", CultureInfo.InvariantCulture) +
                    ";signal=" + record.SignalId +
                    ";risk=" + record.ApprovedRisk.ToString("R", CultureInfo.InvariantCulture) +
                    ";requested=" + record.RequestedRisk.ToString("R", CultureInfo.InvariantCulture) +
                    ";reference=" + record.ReferenceDistancePts.ToString("R", CultureInfo.InvariantCulture) +
                    ";stop=" + record.StopDistancePts.ToString("R", CultureInfo.InvariantCulture) +
                    ";target=" + record.TargetDistancePts.ToString("R", CultureInfo.InvariantCulture) +
                    ";decision=" + record.DecisionPrice.ToString("R", CultureInfo.InvariantCulture) +
                    ";comment_hash=" + record.ExpectedCommentHash);
            }
            return string.Join("\n", lines);
        }

        private bool TryPersistOwnershipLedger()
        {
            if (RunningMode != RunningMode.RealTime) return true;
            if (string.IsNullOrWhiteSpace(_ownershipLedgerPath)) return false;
            try
            {
                System.IO.Directory.CreateDirectory(_journalDir);
                string payload = OwnershipLedgerPayload();
                string signed = payload + "\nchecksum=" + ReceiptChecksum(payload);
                WriteTextAtomically(_ownershipLedgerPath, signed);
                string readback = System.IO.File.ReadAllText(_ownershipLedgerPath);
                if (!string.Equals(readback, signed, StringComparison.Ordinal))
                    throw new InvalidOperationException("ownership ledger write/readback mismatch");
                _ownershipLedgerStatus = "valid";
                return true;
            }
            catch (Exception ex)
            {
                _ownershipLedgerStatus = "persistence failed " + ex.GetType().Name;
                Print("*** ATLAS OWNERSHIP LEDGER PERSISTENCE FAILED: {0}. ***", ex.GetType().Name);
                return false;
            }
        }

        private void InitialiseOwnershipLedgerPath()
        {
            _ownershipLedgerPath = System.IO.Path.Combine(_journalDir,
                "Atlas3_" + ProfileCode + "_" +
                ReceiptChecksum(AccountIdentityText()).Substring(0, 16) + "_ownership_ledger.state");
        }

        private void ResetOwnershipLedgerForFreshEnrollment()
        {
            if (RunningMode != RunningMode.RealTime) return;
            if (Positions.Any() || PendingOrders.Any())
                throw new InvalidOperationException("ownership ledger can only be reset during reviewed flat enrollment");
            InitialiseOwnershipLedgerPath();
            if (System.IO.File.Exists(_ownershipLedgerPath))
            {
                string backup = _ownershipLedgerPath + ".superseded." +
                    DateTime.UtcNow.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture) + ".bak";
                System.IO.File.Copy(_ownershipLedgerPath, backup, false);
            }
            _ownershipLedger.Clear();
            _ownershipLedgerInvalid = false;
            _ownershipLedgerLoaded = true;
            _ownershipLedgerStatus = "fresh enrollment";
            if (!TryPersistOwnershipLedger())
                throw new InvalidOperationException("ownership ledger initial write/readback failed");
        }

        private void LoadOwnershipLedger()
        {
            _ownershipLedger.Clear();
            _ownershipLedgerLoaded = false;
            _ownershipLedgerInvalid = false;
            InitialiseOwnershipLedgerPath();
            if (RunningMode != RunningMode.RealTime)
            {
                _ownershipLedgerLoaded = true;
                _ownershipLedgerStatus = "backtest memory-only";
                return;
            }

            string raw;
            try
            {
                raw = System.IO.File.Exists(_ownershipLedgerPath)
                    ? System.IO.File.ReadAllText(_ownershipLedgerPath)
                    : "";
            }
            catch (Exception ex)
            {
                InvalidateOwnershipLedger("read failed " + ex.GetType().Name);
                return;
            }

            if (string.IsNullOrWhiteSpace(raw))
            {
                _ownershipLedgerLoaded = true;
                if (Positions.Any() || PendingOrders.Any())
                {
                    InvalidateOwnershipLedger("missing ledger while exposure exists");
                    return;
                }
                _ownershipLedgerStatus = "fresh empty ledger";
                if (!TryPersistOwnershipLedger())
                    InvalidateOwnershipLedger(_ownershipLedgerStatus);
                return;
            }

            string marker = "\nchecksum=";
            int checksumIndex = raw.LastIndexOf(marker, StringComparison.Ordinal);
            if (checksumIndex <= 0)
            {
                InvalidateOwnershipLedger("checksum missing");
                return;
            }
            string payload = raw.Substring(0, checksumIndex);
            string checksum = raw.Substring(checksumIndex + marker.Length).Trim();
            if (!string.Equals(checksum, ReceiptChecksum(payload), StringComparison.Ordinal))
            {
                InvalidateOwnershipLedger("checksum failed");
                return;
            }

            string[] lines = payload.Split(new[] { '\n' }, StringSplitOptions.None);
            Dictionary<string, string> header;
            if (lines.Length == 0 || !TryParseLedgerFields(lines[0], '|', out header) ||
                header.Count != 9 || ReceiptValue(header, "schema") != OwnershipLedgerSchema ||
                ReceiptValue(header, "build") != ReceiptBuild ||
                ReceiptValue(header, "profile") != ProfileCode ||
                ReceiptValue(header, "account_hash") != ReceiptChecksum(AccountIdentityText()) ||
                ReceiptValue(header, "receipt_hash") != CurrentBindingLedgerHash() ||
                ReceiptValue(header, "strategy_hash") != StrategyConfigurationReceiptHash() ||
                ReceiptValue(header, "config") != ConfigurationFingerprint() ||
                ReceiptValue(header, "owner") != (DeploymentOwnerId ?? "").Trim() ||
                ReceiptValue(header, "epoch") != (StageEpochId ?? "").Trim())
            {
                InvalidateOwnershipLedger("identity or field-set mismatch");
                return;
            }

            for (int index = 1; index < lines.Length; index++)
            {
                if (string.IsNullOrWhiteSpace(lines[index]))
                {
                    InvalidateOwnershipLedger("blank record");
                    return;
                }
                OwnershipLedgerRecord record;
                if (!TryParseOwnershipLedgerRecord(lines[index], out record))
                {
                    InvalidateOwnershipLedger("invalid record " + index);
                    return;
                }
                string key = OwnershipLedgerKey(record.ExposureType, record.BrokerId);
                if (_ownershipLedger.ContainsKey(key))
                {
                    InvalidateOwnershipLedger("duplicate record " + key);
                    return;
                }
                _ownershipLedger[key] = record;
            }

            _ownershipLedgerLoaded = true;
            _ownershipLedgerStatus = "valid";
            bool removedAbsent = false;
            foreach (OwnershipLedgerRecord record in _ownershipLedger.Values.ToArray())
            {
                if (record.ExposureType == "POSITION")
                {
                    Position position = Positions.FirstOrDefault(x => x.Id == record.BrokerId);
                    if (position == null)
                    {
                        _ownershipLedger.Remove(OwnershipLedgerKey(record.ExposureType, record.BrokerId));
                        removedAbsent = true;
                    }
                    else if (!OwnershipLedgerMatchesPosition(record, position))
                    {
                        InvalidateOwnershipLedger("live position mismatch " + record.BrokerId);
                        return;
                    }
                }
                else
                {
                    PendingOrder order = PendingOrders.FirstOrDefault(x => x.Id == record.BrokerId);
                    if (order == null)
                    {
                        _ownershipLedger.Remove(OwnershipLedgerKey(record.ExposureType, record.BrokerId));
                        removedAbsent = true;
                    }
                    else if (!OwnershipLedgerMatchesPending(record, order))
                    {
                        InvalidateOwnershipLedger("live pending mismatch " + record.BrokerId);
                        return;
                    }
                }
            }
            if (removedAbsent && !TryPersistOwnershipLedger())
                InvalidateOwnershipLedger(_ownershipLedgerStatus);
        }

        private void InvalidateOwnershipLedger(string status)
        {
            _ownershipLedgerInvalid = true;
            _ownershipLedgerLoaded = false;
            _ownershipLedgerStatus = string.IsNullOrWhiteSpace(status) ? "invalid" : status;
        }

        private static bool TryParseLedgerFields(string text, char separator,
            out Dictionary<string, string> values)
        {
            values = new Dictionary<string, string>(StringComparer.Ordinal);
            if (string.IsNullOrWhiteSpace(text)) return false;
            foreach (string item in text.Split(separator))
            {
                int split = item.IndexOf('=');
                if (split <= 0 || split == item.Length - 1) return false;
                string key = item.Substring(0, split);
                if (values.ContainsKey(key)) return false;
                values[key] = item.Substring(split + 1);
            }
            return true;
        }

        private bool TryParseOwnershipLedgerRecord(string text, out OwnershipLedgerRecord record)
        {
            record = null;
            Dictionary<string, string> values;
            if (!TryParseLedgerFields(text, ';', out values) || values.Count != 16) return false;
            string[] required =
            {
                "type", "id", "label", "symbol", "side", "volume", "price", "time", "signal",
                "risk", "requested", "reference", "stop", "target", "decision", "comment_hash"
            };
            if (required.Any(x => !values.ContainsKey(x))) return false;
            string type = ReceiptValue(values, "type");
            long brokerId;
            TradeType side;
            double volume, brokerPrice, risk, requested, reference, stop, target, decision;
            DateTime brokerTime;
            if ((type != "POSITION" && type != "PENDING") ||
                !long.TryParse(ReceiptValue(values, "id"), NumberStyles.None,
                    CultureInfo.InvariantCulture, out brokerId) || brokerId <= 0 ||
                !Enum.TryParse(ReceiptValue(values, "side"), false, out side) ||
                !TryParsePositiveFinite(ReceiptValue(values, "volume"), out volume) ||
                !TryParsePositiveFinite(ReceiptValue(values, "price"), out brokerPrice) ||
                !DateTime.TryParseExact(ReceiptValue(values, "time"), "o", CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out brokerTime) ||
                !TryParsePositiveFinite(ReceiptValue(values, "risk"), out risk) ||
                !TryParsePositiveFinite(ReceiptValue(values, "requested"), out requested) ||
                !TryParsePositiveFinite(ReceiptValue(values, "reference"), out reference) ||
                !TryParsePositiveFinite(ReceiptValue(values, "stop"), out stop) ||
                !TryParseNonNegativeFinite(ReceiptValue(values, "target"), out target) ||
                !TryParsePositiveFinite(ReceiptValue(values, "decision"), out decision)) return false;
            string label = ReceiptValue(values, "label");
            string symbolName = ReceiptValue(values, "symbol");
            string signal = ReceiptValue(values, "signal");
            string commentHash = ReceiptValue(values, "comment_hash");
            DateTime utc = AsUtc(brokerTime);
            if (!IsMasterLabel(label) || !LedgerTextSafe(symbolName) || !LedgerTextSafe(signal) ||
                commentHash.Length != 64 || !commentHash.All(IsLowerHex) ||
                utc > Server.TimeInUtc.AddMinutes(5) ||
                (_evaluationStartUtc != DateTime.MinValue && utc < _evaluationStartUtc.AddDays(-1)))
                return false;
            record = new OwnershipLedgerRecord
            {
                ExposureType = type,
                BrokerId = brokerId,
                Label = label,
                SymbolName = symbolName,
                Side = side,
                VolumeInUnits = volume,
                BrokerPrice = brokerPrice,
                BrokerTimeUtc = utc,
                SignalId = signal,
                ApprovedRisk = risk,
                RequestedRisk = requested,
                ReferenceDistancePts = reference,
                StopDistancePts = stop,
                TargetDistancePts = target,
                DecisionPrice = decision,
                ExpectedCommentHash = commentHash
            };
            return string.Equals(ExpectedCommentForLedger(record) == null
                    ? "" : ReceiptChecksum(ExpectedCommentForLedger(record)),
                commentHash, StringComparison.Ordinal);
        }

        private static bool TryParsePositiveFinite(string text, out double value)
        {
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&
                   value > 0 && !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static bool TryParseNonNegativeFinite(string text, out double value)
        {
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&
                   value >= 0 && !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static bool LedgerTextSafe(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.IndexOfAny(new[] { ';', '|', '=', '\r', '\n' }) < 0;
        }

        private static DateTime AsUtc(DateTime value)
        {
            if (value == DateTime.MinValue || value.Kind == DateTimeKind.Utc) return value;
            if (value.Kind == DateTimeKind.Local) return value.ToUniversalTime();
            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        private string ExpectedCommentForLedger(OwnershipLedgerRecord record)
        {
            if (record == null) return null;
            return BuildTradeCommentText(ProfileCode, LabelSetup(record.Label),
                record.Side == TradeType.Buy ? "BUY" : "SELL", record.SignalId,
                ConfigurationFingerprint(), (DeploymentOwnerId ?? "").Trim(),
                (StageEpochId ?? "").Trim(), record.ApprovedRisk, record.RequestedRisk,
                record.ReferenceDistancePts, record.StopDistancePts, record.TargetDistancePts,
                record.DecisionPrice);
        }

        private bool LedgerCommentCompatible(string actualComment, OwnershipLedgerRecord record)
        {
            string expected = ExpectedCommentForLedger(record);
            if (expected == null || ReceiptChecksum(expected) != record.ExpectedCommentHash) return false;
            // Exact broker ID plus immutable ledger fields are the persistent evidence. A broker
            // may replace the comment with arbitrary text; only an explicit Atlas authority-token
            // contradiction may veto otherwise exact ledger ownership.
            return !HasExplicitContradictoryCurrentAuth(actualComment, expected);
        }

        private bool OwnershipLedgerMatchesPosition(OwnershipLedgerRecord record, Position position)
        {
            if (record == null || position == null || record.ExposureType != "POSITION" ||
                record.BrokerId != position.Id || record.Label != position.Label ||
                record.SymbolName != position.SymbolName || record.Side != position.TradeType)
                return false;
            Symbol symbol = Symbols.GetSymbol(position.SymbolName);
            return VolumeMatches(symbol, position.VolumeInUnits, record.VolumeInUnits) &&
                   Math.Abs(position.EntryPrice - record.BrokerPrice) <=
                       Math.Max(TickSizeFor(symbol) * 0.01, 1e-8) &&
                   Math.Abs((AsUtc(position.EntryTime) - AsUtc(record.BrokerTimeUtc)).TotalSeconds) <= 1.0 &&
                   LedgerCommentCompatible(position.Comment, record);
        }

        private bool OwnershipLedgerMatchesPending(OwnershipLedgerRecord record, PendingOrder order)
        {
            if (record == null || order == null || record.ExposureType != "PENDING" ||
                record.BrokerId != order.Id || record.Label != order.Label ||
                record.SymbolName != order.SymbolName || record.Side != order.TradeType)
                return false;
            Symbol symbol = Symbols.GetSymbol(order.SymbolName);
            double tolerance = Math.Max(TickSizeFor(symbol) * 0.51, 1e-9);
            if (symbol == null || symbol.PipSize <= 0 || !order.StopLossPips.HasValue) return false;
            double stopPips = record.StopDistancePts / symbol.PipSize;
            bool stopMatches = Math.Abs(order.StopLossPips.Value - stopPips) <= 0.01;
            bool targetMatches = record.TargetDistancePts <= 0
                ? !order.TakeProfitPips.HasValue
                : order.TakeProfitPips.HasValue &&
                  Math.Abs(order.TakeProfitPips.Value - record.TargetDistancePts / symbol.PipSize) <= 0.01;
            return VolumeMatches(symbol, order.VolumeInUnits, record.VolumeInUnits) &&
                   Math.Abs(order.TargetPrice - record.BrokerPrice) <= tolerance &&
                   Math.Abs(order.TargetPrice - record.DecisionPrice) <= tolerance &&
                   Math.Abs((AsUtc(order.SubmittedTime) - AsUtc(record.BrokerTimeUtc)).TotalSeconds) <= 1.0 &&
                   stopMatches && targetMatches &&
                   LedgerCommentCompatible(order.Comment, record);
        }

        private bool IsLedgerOwnedPosition(Position position)
        {
            if (!_ownershipLedgerLoaded || _ownershipLedgerInvalid || position == null) return false;
            OwnershipLedgerRecord record;
            return _ownershipLedger.TryGetValue(OwnershipLedgerKey("POSITION", position.Id), out record) &&
                   OwnershipLedgerMatchesPosition(record, position);
        }

        private bool IsLedgerOwnedPending(PendingOrder order)
        {
            if (!_ownershipLedgerLoaded || _ownershipLedgerInvalid || order == null) return false;
            OwnershipLedgerRecord record;
            return _ownershipLedger.TryGetValue(OwnershipLedgerKey("PENDING", order.Id), out record) &&
                   OwnershipLedgerMatchesPending(record, order);
        }

        private bool TryRehydratePendingInfoFromLedger(PendingOrder order, out PendingInfo info)
        {
            info = null;
            if (!IsLedgerOwnedPending(order)) return false;
            OwnershipLedgerRecord record = _ownershipLedger[OwnershipLedgerKey("PENDING", order.Id)];
            Symbol symbol = Symbols.GetSymbol(record.SymbolName);
            if (symbol == null) return false;
            var proposal = new TradeProposal
            {
                Engine = LabelEngine(record.Label),
                Setup = LabelSetup(record.Label),
                Label = record.Label,
                Symbol = symbol,
                Side = record.Side,
                RequestedRisk = record.RequestedRisk,
                SlDistancePts = record.StopDistancePts,
                TpDistancePts = record.TargetDistancePts,
                Style = EntryStyle.LimitAtMid,
                DecisionTimeUtc = record.BrokerTimeUtc,
                ReferenceDistancePts = record.ReferenceDistancePts,
                SignalId = record.SignalId
            };
            info = new PendingInfo
            {
                Proposal = proposal,
                ApprovedRisk = record.ApprovedRisk,
                Units = record.VolumeInUnits,
                DecisionPrice = record.DecisionPrice,
                Spread = 0,
                ExpectedComment = ExpectedCommentForLedger(record),
                RegisteredMonotonicTicks = Stopwatch.GetTimestamp()
            };
            return true;
        }

        private OwnershipLedgerRecord LedgerRecordFromProposal(string type, long id, string label,
            string symbolName, TradeType side, double volume, double brokerPrice,
            DateTime brokerTime, TradeProposal proposal,
            double approvedRisk, double decisionPrice, string expectedComment)
        {
            double reference = proposal.ReferenceDistancePts > 0
                ? proposal.ReferenceDistancePts : proposal.SlDistancePts;
            return new OwnershipLedgerRecord
            {
                ExposureType = type,
                BrokerId = id,
                Label = label,
                SymbolName = symbolName,
                Side = side,
                VolumeInUnits = volume,
                BrokerPrice = brokerPrice,
                BrokerTimeUtc = AsUtc(brokerTime),
                SignalId = SafeStateToken(proposal.SignalId),
                ApprovedRisk = approvedRisk,
                RequestedRisk = proposal.RequestedRisk,
                ReferenceDistancePts = reference,
                StopDistancePts = proposal.SlDistancePts,
                TargetDistancePts = proposal.TpDistancePts,
                DecisionPrice = decisionPrice,
                ExpectedCommentHash = ReceiptChecksum(expectedComment ?? "")
            };
        }

        private bool CommitPositionOwnership(Position position, TradeProposal proposal,
            double approvedRisk, double decisionPrice, string expectedComment, int? filledPendingId)
        {
            if (RunningMode != RunningMode.RealTime) return true;
            if (!_ownershipLedgerLoaded || _ownershipLedgerInvalid || position == null || proposal == null)
                return false;
            OwnershipLedgerRecord record = LedgerRecordFromProposal("POSITION", position.Id,
                position.Label, position.SymbolName, position.TradeType, position.VolumeInUnits,
                position.EntryPrice, position.EntryTime, proposal, approvedRisk, decisionPrice, expectedComment);
            _ownershipLedger[OwnershipLedgerKey("POSITION", position.Id)] = record;
            if (filledPendingId.HasValue)
                _ownershipLedger.Remove(OwnershipLedgerKey("PENDING", filledPendingId.Value));
            return TryPersistOwnershipLedger();
        }

        private bool CommitRebuiltPositionOwnership(Position position, PositionMeta meta,
            double requestedRisk, double targetDistance)
        {
            if (RunningMode != RunningMode.RealTime) return true;
            if (!_ownershipLedgerLoaded || _ownershipLedgerInvalid || position == null || meta == null)
                return false;
            var proposal = new TradeProposal
            {
                Setup = meta.Setup,
                Label = meta.Label,
                Side = position.TradeType,
                SignalId = meta.SignalId,
                RequestedRisk = requestedRisk,
                ReferenceDistancePts = meta.ReferenceDistancePts,
                SlDistancePts = meta.StopRiskDistancePts,
                TpDistancePts = targetDistance
            };
            string expected = BuildTradeCommentText(ProfileCode, meta.Setup,
                position.TradeType == TradeType.Buy ? "BUY" : "SELL", meta.SignalId,
                ConfigurationFingerprint(), (DeploymentOwnerId ?? "").Trim(),
                (StageEpochId ?? "").Trim(), meta.ApprovedRisk, requestedRisk,
                meta.ReferenceDistancePts, meta.StopRiskDistancePts, targetDistance,
                meta.DecisionPrice);
            if (!string.Equals(position.Comment ?? "", expected, StringComparison.Ordinal)) return false;
            _ownershipLedger[OwnershipLedgerKey("POSITION", position.Id)] =
                LedgerRecordFromProposal("POSITION", position.Id, position.Label,
                    position.SymbolName, position.TradeType, position.VolumeInUnits,
                    position.EntryPrice, position.EntryTime, proposal, meta.ApprovedRisk, meta.DecisionPrice, expected);
            return TryPersistOwnershipLedger();
        }

        private bool CommitPendingOwnership(PendingOrder order, ActiveEntryMutation mutation)
        {
            if (RunningMode != RunningMode.RealTime) return true;
            if (!_ownershipLedgerLoaded || _ownershipLedgerInvalid || order == null || mutation == null)
                return false;
            OwnershipLedgerRecord record = LedgerRecordFromProposal("PENDING", order.Id,
                order.Label, order.SymbolName, order.TradeType, order.VolumeInUnits,
                order.TargetPrice, order.SubmittedTime, mutation.Proposal, mutation.ApprovedRisk,
                mutation.DecisionPrice, mutation.ExpectedComment);
            _ownershipLedger[OwnershipLedgerKey("PENDING", order.Id)] = record;
            return TryPersistOwnershipLedger();
        }

        private bool RemoveOwnershipLedgerRecord(string type, long brokerId)
        {
            if (RunningMode != RunningMode.RealTime || !_ownershipLedgerLoaded || _ownershipLedgerInvalid)
                return RunningMode != RunningMode.RealTime;
            if (!_ownershipLedger.Remove(OwnershipLedgerKey(type, brokerId))) return true;
            return TryPersistOwnershipLedger();
        }

        private void FailClosedForOwnershipLedger(string reason, Position position, PendingOrder order)
        {
            RecordExecutionIncident("ownership_ledger_" + reason, 0, 0);
            if (position != null && _currentRunCreatedPositionIds.Contains(position.Id))
                EmergencyClose(position, "ownership_ledger_" + reason);
            if (order != null && _currentRunCreatedPendingIds.Contains(order.Id))
                CancelPendingWithRetry(order, "ownership_ledger_" + reason);
        }

        private double MinimumBrokerRisk(Symbol symbol, double stopDistancePts)
        {
            if (symbol == null || stopDistancePts <= 0 || symbol.PipSize <= 0 || symbol.PipValue <= 0) return 0;
            return stopDistancePts / symbol.PipSize * symbol.PipValue * symbol.VolumeInUnitsMin;
        }

        private double VolumeForRisk(Symbol symbol, double riskUsd, double stopDistancePts)
        {
            if (symbol == null || stopDistancePts <= 0 || symbol.PipSize <= 0 || symbol.PipValue <= 0) return 0;
            double lossPerUnit = stopDistancePts / symbol.PipSize * symbol.PipValue;
            if (lossPerUnit <= 0) return 0;
            double units = symbol.NormalizeVolumeInUnits(riskUsd / lossPerUnit, RoundingMode.Down);
            if (units < symbol.VolumeInUnitsMin) return 0;
            units = Math.Min(units, symbol.VolumeInUnitsMax);
            // ===== V25 FIX-3 (HIGH): absolute notional ceiling. =====
            // Dollar risk alone does not bound leverage: a very tight stop yields correct risk but
            // enormous notional, so each tick moves equity a large fraction of R. Clamp units so
            // units * price <= MaxNotionalPerPositionUsd, then re-normalise DOWN and re-check the
            // broker minimum (a clamp that lands below the minimum must reject, never round up).
            if (MaxNotionalPerPositionUsd > 0)
            {
                double referencePrice = symbol.Ask > 0 ? symbol.Ask : symbol.Bid;
                if (referencePrice > 0)
                {
                    double maxUnitsByNotional = MaxNotionalPerPositionUsd / referencePrice;
                    if (units > maxUnitsByNotional)
                    {
                        units = symbol.NormalizeVolumeInUnits(maxUnitsByNotional, RoundingMode.Down);
                        if (units < symbol.VolumeInUnitsMin) return 0;
                    }
                }
            }
            return units;
        }

        private string BuildTradeComment(TradeProposal p, double risk, double decide)
        {
            double referenceDistance = p.ReferenceDistancePts > 0
                ? p.ReferenceDistancePts
                : p.SlDistancePts;
            return BuildTradeCommentText(ProfileCode, SafeStateToken(p.Setup),
                p.Side == TradeType.Buy ? "BUY" : "SELL", p.SignalId ?? "",
                ConfigurationFingerprint(), (DeploymentOwnerId ?? "").Trim(),
                (StageEpochId ?? "").Trim(), risk, p.RequestedRisk, referenceDistance,
                p.SlDistancePts, p.TpDistancePts, decide);
        }

        // Production uses this pure helper directly. The release harness invokes the same private
        // method by reflection, so comment round-trip tests cannot silently drift from live code.
        private static string BuildTradeCommentText(string profileCode, string setup, string side,
            string signal, string config, string owner, string epoch, double risk,
            double requestedRisk, double referenceDistance, double stopDistance,
            double targetDistance, double decisionPrice)
        {
            string safeSignal = SafeStateToken(signal);
            string tradeReference = safeSignal.Length <= 8 ? safeSignal : safeSignal.Substring(0, 8);
            string safeProfile = SafeStateToken(profileCode);
            string safeSetup = SafeStateToken(setup);
            string safeSide = string.Equals(side, "BUY", StringComparison.OrdinalIgnoreCase) ? "BUY" : "SELL";
            string comment = string.Format(CultureInfo.InvariantCulture,
                "ATLAS3 {0} {1} {2} {3};v=311;c={4};o={5};e={6};r={7:F1};q={8:F1};g={9:F2};",
                safeProfile, SetupDisplayName(safeSetup), safeSide, tradeReference,
                SafeStateToken(config), SafeStateToken(owner), SafeStateToken(epoch),
                risk, requestedRisk, referenceDistance);
            return comment.Length <= MaxTradeCommentLength && IsAscii(comment) ? comment : "";
        }

        private static string SetupDisplayName(string setup)
        {
            if (setup == "A1") return "DAX-A1";
            if (setup == "A2") return "DAX-A2";
            if (setup == "A3") return "DAX-A3";
            if (setup == "B1") return "UK-B1";
            if (setup == "B2") return "UK-B2";
            if (setup == "C1") return "NAS-C1";
            if (setup == "C2") return "NAS-C2";
            if (setup == "D1") return "XAU-D1";
            return "TRADE";
        }

        private PositionMeta NewPositionMeta(TradeProposal p, double risk, double decide, Position pos)
        {
            int direction = p.Side == TradeType.Buy ? 1 : -1;
            return new PositionMeta
            {
                Engine = p.Engine,
                Setup = p.Setup,
                Label = p.Label,
                SignalId = p.SignalId,
                InitialRisk = CalculateInitialRiskAtFill(pos, p.Symbol, risk),
                ApprovedRisk = risk,
                StopRiskDistancePts = pos != null && pos.StopLoss.HasValue
                    ? Math.Abs(pos.EntryPrice - pos.StopLoss.Value)
                    : p.SlDistancePts,
                ReferenceDistancePts = p.ReferenceDistancePts > 0 ? p.ReferenceDistancePts : p.SlDistancePts,
                Direction = direction,
                DecisionPrice = decide,
                MfePrice = pos.EntryPrice,
                MaePrice = pos.EntryPrice,
                LastStopPrice = pos.StopLoss ?? 0,
                LastTakeProfitPrice = pos.TakeProfit ?? 0,
                Kind = p.Setup
            };
        }

        // =========================================================================================
        // ACCOUNT GATE, EOD AND OBJECTIVES
        // =========================================================================================
        private void RebuildPragueDay(bool force)
        {
            DateTime now = Server.TimeInUtc;
            string day = PragueDate(now).ToString("yyyy-MM-dd");
            if (!force && day == _pragueDay) return;

            if (!string.IsNullOrEmpty(_pragueDay))
            {
                double v25PriorPeak = _peakEodBalance;
                _peakEodBalance = Math.Max(_peakEodBalance, Account.Balance);
                // V25 FIX-1: flush the ratchet to disk the moment it advances, so a restart cannot
                // lose it. Previously this advance existed only in memory until the process died.
                if (_peakEodBalance > v25PriorPeak) PersistPeakEodBalance();
            }

            _pragueDay = day;
            _dayHalt = false;
            DateTime localDate = PragueDate(now);
            _dayAnchorBalance = DayStartBalanceOverride > 0 && _dayStartOverrideDate == localDate
                ? DayStartBalanceOverride
                : ReconstructPragueDayStartBalance(now);

            if (force)
                ReconstructDynamicRiskState();
            else
                ApplyDynamicRiskDayState(_dayAnchorBalance);

            Journal("DAY_ROLL",
                "\"prague_day\":\"" + day + "\",\"day_anchor\":" + Jn(_dayAnchorBalance) +
                ",\"peak_eod\":" + Jn(_peakEodBalance) + ",\"daily_limit\":" + Jn(OfficialDailyLimit()) +
                ",\"max_loss_limit\":" + Jn(MaxLossLimit()) +
                ",\"dynamic_tier\":\"" + Js(_currentDynamicRiskTier) + "\"" +
                ",\"dynamic_scale\":" + Jn(_currentDynamicRiskScale) +
                ",\"dynamic_day_progress_pct\":" + Jn(_dynamicDayProgressPct) +
                ",\"challenge_ratchet\":" + _challengeProfitTierRatchet +
                ",\"funded_ratchet\":" + _fundedProfitTierRatchet +
                ",\"downside_latched\":" + Bool(IsFunded() ? _fundedDownsideLatched : _challengeDownsideLatched));
        }

        private double ReconstructPragueDayStartBalance(DateTime utcNow)
        {
            DateTime localDate = PragueDate(utcNow);
            DateTime localMidnight = new DateTime(localDate.Year, localDate.Month, localDate.Day, 0, 0, 0, DateTimeKind.Unspecified);
            DateTime utcMidnight;
            try { utcMidnight = TimeZoneInfo.ConvertTimeToUtc(localMidnight, _prague); }
            catch { utcMidnight = utcNow.Date; }
            double realizedToday = 0;
            foreach (var h in History)
                if (h.ClosingTime >= utcMidnight && h.ClosingTime <= utcNow)
                    realizedToday += h.NetProfit;
            return Account.Balance - realizedToday;
        }

        private void ValidateDynamicRiskConfiguration()
        {
            if (!UseDynamicRiskLadder && !FundedPayoutLockEnabled) return;

            double[] values =
            {
                ChallengeDownsideTriggerPct, ChallengeDownsideRecoveryPct, ChallengeDownsideScale,
                ChallengeAcceleratorScale, ChallengeFirstDownshiftPct, ChallengeMidScale,
                ChallengeSecondDownshiftPct, ChallengeLateScale,
                FundedDownsideTriggerPct, FundedDownsideRecoveryPct, FundedDownsideScale,
                FundedAcceleratorScale, FundedDownshiftPct, FundedPayoutApproachScale,
                FundedPayoutLockTargetPct
            };
            if (values.Any(x => double.IsNaN(x) || double.IsInfinity(x)))
            {
                HaltStartup("dynamic-risk configuration contains a non-finite value");
                return;
            }

            if (UseDynamicRiskLadder && ChallengeDynamicRiskEnabled)
            {
                if (ChallengeProfitLadderEnabled)
                {
                    bool profitThresholds = ChallengeFirstDownshiftPct < ChallengeSecondDownshiftPct &&
                                            ChallengeSecondDownshiftPct < ProfitTargetPct;
                    bool profitScales = ChallengeLateScale <= ChallengeMidScale &&
                                        ChallengeMidScale <= ChallengeAcceleratorScale;
                    if (!profitThresholds || !profitScales)
                        HaltStartup("invalid challenge profit ladder: thresholds must ascend below target and scales must downshift monotonically");
                }
                if (ChallengeDownsideDeriskEnabled)
                {
                    double lowestNormalScale = ChallengeProfitLadderEnabled
                        ? ChallengeLateScale
                        : (UseStageRiskScale ? ChallengeRiskScale : PortfolioRiskScale);
                    bool downsideThresholds = ChallengeDownsideTriggerPct < ChallengeDownsideRecoveryPct &&
                                              (!ChallengeProfitLadderEnabled ||
                                               ChallengeDownsideRecoveryPct < ChallengeFirstDownshiftPct);
                    if (!downsideThresholds || ChallengeDownsideScale > lowestNormalScale)
                        HaltStartup("invalid challenge downside de-risk: trigger must precede recovery and its scale must be the lowest");
                }
            }

            if (UseDynamicRiskLadder && FundedDynamicRiskEnabled)
            {
                if (FundedProfitLadderEnabled)
                {
                    bool profitThresholds = FundedDownshiftPct < FundedPayoutLockTargetPct;
                    bool profitScales = FundedPayoutApproachScale <= FundedAcceleratorScale;
                    if (!profitThresholds || !profitScales)
                        HaltStartup("invalid funded profit ladder: downshift must precede payout target and scales must downshift monotonically");
                }
                if (FundedDownsideDeriskEnabled)
                {
                    double lowestNormalScale = FundedProfitLadderEnabled
                        ? FundedPayoutApproachScale
                        : (UseStageRiskScale ? FundedRiskScale : PortfolioRiskScale);
                    bool downsideThresholds = FundedDownsideTriggerPct < FundedDownsideRecoveryPct &&
                                              (!FundedProfitLadderEnabled ||
                                               FundedDownsideRecoveryPct < FundedDownshiftPct);
                    if (!downsideThresholds || FundedDownsideScale > lowestNormalScale)
                        HaltStartup("invalid funded downside de-risk: trigger must precede recovery and its scale must be the lowest");
                }
            }

            if (FundedPayoutLockEnabled && FundedProfitLadderEnabled &&
                FundedPayoutLockTargetPct <= FundedDownshiftPct)
                HaltStartup("funded payout-lock target must be above the funded downshift threshold");

            // The ladder may only reduce risk relative to the ACTIVE stage. Validate only the
            // active stage's table; an unused stage must not prevent this stage from starting.
            if (UseDynamicRiskLadder)
            {
                double v25StageCeiling = StaticPortfolioRiskScale();
                if (v25StageCeiling > 0)
                {
                    double v25MaxLadder = 0;
                    if (!IsFunded() && ChallengeDynamicRiskEnabled)
                    {
                        v25MaxLadder = Math.Max(v25MaxLadder, ChallengeAcceleratorScale);
                        v25MaxLadder = Math.Max(v25MaxLadder, ChallengeMidScale);
                        v25MaxLadder = Math.Max(v25MaxLadder, ChallengeLateScale);
                        v25MaxLadder = Math.Max(v25MaxLadder, ChallengeDownsideScale);
                    }
                    else if (IsFunded() && FundedDynamicRiskEnabled)
                    {
                        v25MaxLadder = Math.Max(v25MaxLadder, FundedAcceleratorScale);
                        v25MaxLadder = Math.Max(v25MaxLadder, FundedPayoutApproachScale);
                        v25MaxLadder = Math.Max(v25MaxLadder, FundedDownsideScale);
                    }
                    if (v25MaxLadder > v25StageCeiling + 1e-9)
                        HaltStartup(string.Format(CultureInfo.InvariantCulture,
                            "dynamic ladder scale {0:F2} exceeds the stage-locked risk ceiling {1:F2}; the ladder may only reduce risk (V25 FIX-4)",
                            v25MaxLadder, v25StageCeiling));
                }
            }

            // ===== V25 FIX-4b (HIGH): the profit ratchet must stay ON while the ladder is on. =====
            // With a ratchet disabled the tier is recomputed from CURRENT day-start progress, so
            // giving back profit walks the ladder BACK UP (LATE 0.60 -> MID 0.70 -> ACCEL 0.80) --
            // i.e. increasing bet size into a drawdown. Refuse that configuration outright.
            if (UseDynamicRiskLadder && ChallengeDynamicRiskEnabled && ChallengeProfitLadderEnabled &&
                !ChallengeProfitTierRatchetEnabled)
                HaltStartup("challenge profit-tier ratchet must remain enabled while the dynamic ladder is on (V25 FIX-4b: without it the ladder scales risk UP after losses)");
            // V25 FIX-4b (completed): the FUNDED side has the identical pathology — with the funded
            // ratchet off, giving back profit walks funded risk 0.50 -> 0.70. The first V25 draft
            // guarded only the Challenge branch while the header claimed the rule was general.
            if (UseDynamicRiskLadder && FundedDynamicRiskEnabled && FundedProfitLadderEnabled &&
                !FundedProfitTierRatchetEnabled)
                HaltStartup("funded profit-tier ratchet must remain enabled while the dynamic ladder is on (V25 FIX-4b: without it the ladder scales risk UP after losses)");
        }

        private void ReconstructDynamicRiskState()
        {
            _challengeProfitTierRatchet = 0;
            _fundedProfitTierRatchet = 0;
            _challengeDownsideLatched = false;
            _fundedDownsideLatched = false;

            DateTime today = PragueDate(Server.TimeInUtc);
            var pnlByDay = new SortedDictionary<DateTime, double>();
            foreach (var trade in History)
            {
                if (trade.ClosingTime < _evaluationStartUtc) continue;
                DateTime day = PragueDate(trade.ClosingTime);
                if (day >= today) continue;
                double value;
                pnlByDay.TryGetValue(day, out value);
                pnlByDay[day] = value + trade.NetProfit;
            }

            double reconstructedBalance = InitialBalance;
            foreach (var pair in pnlByDay)
            {
                ApplyDynamicRiskDayState(reconstructedBalance);
                reconstructedBalance += pair.Value;
            }

            ApplyDynamicRiskDayState(_dayAnchorBalance > 0 ? _dayAnchorBalance : reconstructedBalance);
            Journal("DYNAMIC_RISK_REBUILD",
                "\"completed_days\":" + pnlByDay.Count +
                ",\"tier\":\"" + Js(_currentDynamicRiskTier) + "\"" +
                ",\"scale\":" + Jn(_currentDynamicRiskScale) +
                ",\"day_progress_pct\":" + Jn(_dynamicDayProgressPct) +
                ",\"challenge_ratchet\":" + _challengeProfitTierRatchet +
                ",\"funded_ratchet\":" + _fundedProfitTierRatchet +
                ",\"downside_latched\":" + Bool(IsFunded() ? _fundedDownsideLatched : _challengeDownsideLatched));
        }

        private void ApplyDynamicRiskDayState(double dayStartBalance)
        {
            _dynamicDayProgressPct = InitialBalance > 0
                ? 100.0 * (dayStartBalance - InitialBalance) / InitialBalance
                : 0;

            if (IsFunded())
            {
                int observedTier = _dynamicDayProgressPct >= FundedDownshiftPct ? 1 : 0;
                _fundedProfitTierRatchet = FundedProfitLadderEnabled
                    ? (FundedProfitTierRatchetEnabled
                        ? Math.Max(_fundedProfitTierRatchet, observedTier)
                        : observedTier)
                    : 0;

                if (!FundedDownsideDeriskEnabled)
                    _fundedDownsideLatched = false;
                else if (FundedDownsideHysteresisEnabled)
                {
                    if (_dynamicDayProgressPct <= FundedDownsideTriggerPct) _fundedDownsideLatched = true;
                    else if (_dynamicDayProgressPct >= FundedDownsideRecoveryPct) _fundedDownsideLatched = false;
                }
                else
                    _fundedDownsideLatched = _dynamicDayProgressPct <= FundedDownsideTriggerPct;
            }
            else
            {
                int observedTier = _dynamicDayProgressPct >= ChallengeSecondDownshiftPct ? 2 :
                                   _dynamicDayProgressPct >= ChallengeFirstDownshiftPct ? 1 : 0;
                _challengeProfitTierRatchet = ChallengeProfitLadderEnabled
                    ? (ChallengeProfitTierRatchetEnabled
                        ? Math.Max(_challengeProfitTierRatchet, observedTier)
                        : observedTier)
                    : 0;

                if (!ChallengeDownsideDeriskEnabled)
                    _challengeDownsideLatched = false;
                else if (ChallengeDownsideHysteresisEnabled)
                {
                    if (_dynamicDayProgressPct <= ChallengeDownsideTriggerPct) _challengeDownsideLatched = true;
                    else if (_dynamicDayProgressPct >= ChallengeDownsideRecoveryPct) _challengeDownsideLatched = false;
                }
                else
                    _challengeDownsideLatched = _dynamicDayProgressPct <= ChallengeDownsideTriggerPct;
            }

            RefreshDynamicRiskDecision();
        }

        private void RefreshDynamicRiskDecision()
        {
            double staticScale = StaticPortfolioRiskScale();
            _currentDynamicRiskScale = staticScale;
            _currentDynamicRiskTier = IsFunded() ? "FUNDED_STATIC" : "CHALLENGE_STATIC";

            if (!UseDynamicRiskLadder || Stage == StageMode.Unconfigured) return;
            if (IsFunded())
            {
                if (!FundedDynamicRiskEnabled) return;
                if (FundedDownsideDeriskEnabled && _fundedDownsideLatched)
                {
                    _currentDynamicRiskTier = "FUNDED_DOWNSIDE";
                    _currentDynamicRiskScale = FundedDownsideScale;
                }
                else if (!FundedProfitLadderEnabled)
                {
                    _currentDynamicRiskTier = "FUNDED_STATIC";
                    _currentDynamicRiskScale = staticScale;
                }
                else if (_fundedProfitTierRatchet >= 1)
                {
                    _currentDynamicRiskTier = "FUNDED_APPROACH";
                    _currentDynamicRiskScale = FundedPayoutApproachScale;
                }
                else
                {
                    _currentDynamicRiskTier = "FUNDED_ACCELERATOR";
                    _currentDynamicRiskScale = FundedAcceleratorScale;
                }
                return;
            }

            if (!ChallengeDynamicRiskEnabled) return;
            if (ChallengeDownsideDeriskEnabled && _challengeDownsideLatched)
            {
                _currentDynamicRiskTier = "CHALLENGE_DOWNSIDE";
                _currentDynamicRiskScale = ChallengeDownsideScale;
            }
            else if (!ChallengeProfitLadderEnabled)
            {
                _currentDynamicRiskTier = "CHALLENGE_STATIC";
                _currentDynamicRiskScale = staticScale;
            }
            else if (_challengeProfitTierRatchet >= 2)
            {
                _currentDynamicRiskTier = "CHALLENGE_LATE";
                _currentDynamicRiskScale = ChallengeLateScale;
            }
            else if (_challengeProfitTierRatchet >= 1)
            {
                _currentDynamicRiskTier = "CHALLENGE_MID";
                _currentDynamicRiskScale = ChallengeMidScale;
            }
            else
            {
                _currentDynamicRiskTier = "CHALLENGE_ACCELERATOR";
                _currentDynamicRiskScale = ChallengeAcceleratorScale;
            }
        }

        // ===== V25 FIX-1 support: durable EOD-peak state =====
        // The trailing floor is the single most important number in the S1 profile. In V24 it lived
        // only in memory, so any restart lost it. These three helpers persist it next to the existing
        // execution-state file, scoped by account + stage epoch so a different account or a fresh
        // evaluation can never inherit a stale peak.
        private void InitialisePeakEodStatePath()
        {
            try
            {
                _peakEodStatePath = System.IO.Path.Combine(_journalDir,
                    string.Format(CultureInfo.InvariantCulture, "atlas3_{0}_peakeod_{1}_{2}.state",
                        ProfileCode, Account.Number, string.IsNullOrWhiteSpace(StageEpochId) ? "noepoch" : StageEpochId));
            }
            catch (Exception ex)
            {
                _peakEodStatePath = null;
                FailS1PeakEodState("path_initialisation_failed_" + ex.GetType().Name);
            }
        }

        private double LoadPersistedPeakEodBalance(bool allowMissing)
        {
            if (RunningMode != RunningMode.RealTime || string.IsNullOrWhiteSpace(_peakEodStatePath)) return 0;
            try
            {
                if (!System.IO.File.Exists(_peakEodStatePath))
                {
                    if (!allowMissing) FailS1PeakEodState("existing_state_missing");
                    return 0;
                }
                string raw = System.IO.File.ReadAllText(_peakEodStatePath).Trim();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    FailS1PeakEodState("existing_state_empty");
                    return 0;
                }

                double storedPeak, storedInitial;
                string storedStage;
                if (raw.StartsWith("schema=", StringComparison.Ordinal))
                {
                    Dictionary<string, string> values;
                    string error;
                    if (!TryParseReceipt(raw, out values, out error) ||
                        ReceiptValue(values, "schema") != "2" ||
                        ReceiptValue(values, "profile") != ProfileCode ||
                        ReceiptValue(values, "account_hash") != ExecutionStateAccountHash() ||
                        ReceiptValue(values, "epoch_hash") != ReceiptChecksum(StageEpochId ?? "") ||
                        !double.TryParse(ReceiptValue(values, "peak"), NumberStyles.Float,
                            CultureInfo.InvariantCulture, out storedPeak) ||
                        !double.TryParse(ReceiptValue(values, "initial"), NumberStyles.Float,
                            CultureInfo.InvariantCulture, out storedInitial))
                    {
                        FailS1PeakEodState("signed_state_invalid_" + SafeStateToken(error));
                        return 0;
                    }
                    storedStage = ReceiptValue(values, "stage");
                }
                else
                {
                    // One-time migration of the exact V27 legacy format: peak|initial|stage.
                    string[] parts = raw.Split('|');
                    if (parts.Length != 3 ||
                        !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out storedPeak) ||
                        !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out storedInitial))
                    {
                        FailS1PeakEodState("legacy_state_invalid");
                        return 0;
                    }
                    storedStage = parts[2];
                }

                if (double.IsNaN(storedPeak) || double.IsInfinity(storedPeak) ||
                    double.IsNaN(storedInitial) || double.IsInfinity(storedInitial) ||
                    storedPeak < InitialBalance - 0.01)
                {
                    FailS1PeakEodState("state_values_invalid");
                    return 0;
                }
                if (Math.Abs(storedInitial - InitialBalance) > 0.01 || storedStage != Stage.ToString())
                {
                    Print("S1 peak-EOD state rejected: stored initial ${0:F2}/stage {1} does not match locked ${2:F2}/{3}.",
                        storedInitial, storedStage, InitialBalance, Stage);
                    FailS1PeakEodState("state_identity_mismatch");
                    return 0;
                }
                // V25 FIX-1a: plausibility ceiling. Because the adopted peak is max()'d and then
                // re-persisted, a single mistyped PeakEodBalanceOverride (e.g. 1070000 for 107000)
                // would latch permanently — MaxLossLimit() would sit above equity, tripping the
                // emergency flatten every day, with NO parameter able to lower it again. Reject any
                // stored peak that cannot be real for this account size and say so loudly.
                double plausibleCeiling = InitialBalance * 1.5;
                if (storedPeak > plausibleCeiling)
                {
                    Print("*** S1 peak-EOD state REJECTED: stored peak ${0:F2} exceeds {1:F2} (1.5x initial). Manual review required; do not delete safety state. Path: {2}. ***",
                        storedPeak, plausibleCeiling, _peakEodStatePath);
                    FailS1PeakEodState("state_peak_implausible");
                    return 0;
                }
                return storedPeak;
            }
            catch (Exception ex)
            {
                FailS1PeakEodState("state_read_failed_" + ex.GetType().Name);
                return 0;
            }
        }

        private void PersistPeakEodBalance()
        {
            if (RunningMode != RunningMode.RealTime) return;
            if (string.IsNullOrWhiteSpace(_peakEodStatePath))
            {
                FailS1PeakEodState("state_path_unavailable");
                return;
            }
            try
            {
                string payload = "schema=2|profile=" + ProfileCode +
                    "|account_hash=" + ExecutionStateAccountHash() +
                    "|epoch_hash=" + ReceiptChecksum(StageEpochId ?? "") +
                    "|peak=" + _peakEodBalance.ToString("G17", CultureInfo.InvariantCulture) +
                    "|initial=" + InitialBalance.ToString("G17", CultureInfo.InvariantCulture) +
                    "|stage=" + Stage;
                string signed = payload + "|checksum=" + ReceiptChecksum(payload);
                WriteTextAtomically(_peakEodStatePath, signed);
                string readback = System.IO.File.ReadAllText(_peakEodStatePath).Trim();
                if (!string.Equals(readback, signed, StringComparison.Ordinal))
                    throw new InvalidOperationException("peak-EOD state write/readback mismatch");
            }
            catch (Exception ex)
            {
                FailS1PeakEodState("state_persistence_failed_" + ex.GetType().Name);
            }
        }

        private void FailS1PeakEodState(string reason)
        {
            string safeReason = SanitizeExecutionReason(reason);
            bool firstFailure = !_peakEodSafetyFailed;
            _peakEodSafetyFailed = true;
            HaltStartup("S1 peak-EOD safety state failure: " + safeReason);
            if (RunningMode == RunningMode.RealTime && firstFailure)
                EnterExecutionHardHalt("s1_peak_eod_" + safeReason, 0, 0);
            if (BindingAuthorityCurrent())
            {
                CancelAllMasterPending();
                CloseAllMasterPositions("s1_peak_eod_state_invalid");
            }
        }

        private double ReconstructPeakEodBalance()
        {
            DateTime today = PragueDate(Server.TimeInUtc);
            var pnlByDay = new SortedDictionary<DateTime, double>();
            foreach (var trade in History)
            {
                if (trade.ClosingTime < _evaluationStartUtc) continue;
                DateTime day = PragueDate(trade.ClosingTime);
                if (day >= today) continue;
                double value;
                pnlByDay.TryGetValue(day, out value);
                pnlByDay[day] = value + trade.NetProfit;
            }

            double balance = InitialBalance;
            double peak = InitialBalance;
            foreach (var pair in pnlByDay)
            {
                balance += pair.Value;
                peak = Math.Max(peak, balance);
            }
            double realizedToday = History
                .Where(x => x.ClosingTime >= _evaluationStartUtc && PragueDate(x.ClosingTime) >= today)
                .Sum(x => x.NetProfit);
            _historyReconstructedBalance = balance + realizedToday + VerifiedCashAdjustmentsUsd;
            Print("S1 EOD peak reconstructed from broker history: ${0:F2} across {1} completed Prague day(s).",
                peak, pnlByDay.Count);
            return peak;
        }

        private double OfficialDailyLimit()
        {
            return _dayAnchorBalance - InitialBalance * FtmoDailyLossPct / 100.0;
        }

        private double MaxLossLimit()
        {
            // The true trailing floor is unknown after a state-integrity fault. A conservative
            // displayed value ensures any emergency check remains on the flattening side.
            if (_peakEodSafetyFailed) return InitialBalance * 1.5;
            return _peakEodBalance - InitialBalance * FtmoMaxLossPct / 100.0;
        }

        private void CheckAccountEmergency()
        {
            if (!_accountLeaseOwned) return;
            double internalDailyFloor = _dayAnchorBalance - EffectiveDailyWorstCaseCapUsd();
            double officialDailyEmergencyFloor = OfficialDailyLimit() + EmergencyBufferUsd;
            double dailyEmergencyFloor = Math.Max(internalDailyFloor, officialDailyEmergencyFloor);
            double emergencyMaxLossFloor = MaxLossLimit() + EmergencyBufferUsd;
            if (Account.Equity > dailyEmergencyFloor && Account.Equity > emergencyMaxLossFloor) return;

            if (!_dayHalt)
            {
                _dayHalt = true;
                _lastGateReason = "EMERGENCY account floor";
                Journal("GUARD",
                    "\"kind\":\"account_emergency\",\"equity\":" + Jn(Account.Equity) +
                    ",\"daily_emergency_floor\":" + Jn(dailyEmergencyFloor) +
                    ",\"max_loss_emergency\":" + Jn(emergencyMaxLossFloor));
                Print("!! ATLAS ACCOUNT EMERGENCY: equity ${0:F2}; daily floor ${1:F2}; max-loss emergency ${2:F2}. FLATTENING MASTER BOOK. !!",
                    Account.Equity, dailyEmergencyFloor, emergencyMaxLossFloor);
            }
            CancelAllMasterPending();
            CloseAllMasterPositions("account_emergency");
        }

        private void RefreshObjectiveStats(bool force)
        {
            if (!force && Server.TimeInUtc - _lastObjectiveRefresh < TimeSpan.FromSeconds(15)) return;
            _lastObjectiveRefresh = Server.TimeInUtc;

            var pnlByDay = new Dictionary<DateTime, double>();
            var tradeDays = new HashSet<DateTime>();
            foreach (var h in History)
            {
                if (h.ClosingTime < _evaluationStartUtc) continue;
                DateTime closeDay = PragueDate(h.ClosingTime);
                double value;
                pnlByDay.TryGetValue(closeDay, out value);
                pnlByDay[closeDay] = value + h.NetProfit;
                tradeDays.Add(PragueDate(h.EntryTime));
            }
            foreach (var p in Positions)
                if (p.EntryTime >= _evaluationStartUtc)
                    tradeDays.Add(PragueDate(p.EntryTime));

            _positiveDaysProfit = pnlByDay.Values.Where(x => x > 0).Sum();
            _bestDayProfit = pnlByDay.Values.Where(x => x > 0).DefaultIfEmpty(0).Max();
            _objectiveTradeDays = tradeDays.Count;
            _targetReached = !IsFunded() && Account.Balance >= InitialBalance * (1.0 + ProfitTargetPct / 100.0);
        }

        private bool BestDaySatisfied()
        {
            return _positiveDaysProfit > 0 && _bestDayProfit <= BestDayMaxShare * _positiveDaysProfit + 0.01;
        }

        private void CheckObjectiveCompletion()
        {
            if (IsFunded() || _objectiveLocked || _startupHalt) return;
            double target = InitialBalance * (1.0 + ProfitTargetPct / 100.0);
            bool consistency = BestDaySatisfied();
            if (Account.Equity < target || !consistency)
            {
                if (!HasAnyMasterExposure()) _objectiveClosing = false;
                return;
            }

            _objectiveClosing = true;
            CancelAllMasterPending();
            CloseAllMasterPositions("challenge_objective");
            if (!HasAnyAccountExposure())
            {
                // The objective decision must include the trades just closed above. Without this
                // forced refresh, a large current-day close can turn a previously valid Best Day
                // share into an invalid one while the stale pre-close aggregate still says pass.
                RefreshObjectiveStats(true);
                if (Account.Balance >= target && BestDaySatisfied())
                {
                    _objectiveLocked = true;
                    _lastGateReason = "OBJECTIVE LOCKED";
                    Journal("GUARD",
                        "\"kind\":\"objective_locked\",\"balance\":" + Jn(Account.Balance) +
                        ",\"best_day\":" + Jn(_bestDayProfit) + ",\"positive_days\":" + Jn(_positiveDaysProfit));
                    Print("*** ATLAS CHALLENGE OBJECTIVE LOCKED: balance ${0:F2}, Best Day {1:F1}% of positive days. ***",
                        Account.Balance, _positiveDaysProfit > 0 ? 100.0 * _bestDayProfit / _positiveDaysProfit : 0);
                }
                else
                {
                    _objectiveClosing = false;
                    _lastGateReason = Account.Balance < target
                        ? "Objective close settled below target"
                        : "Best Day requires more profitable days";
                    Journal("GUARD",
                        "\"kind\":\"objective_recheck_failed\",\"balance\":" + Jn(Account.Balance) +
                        ",\"target\":" + Jn(target) + ",\"best_day\":" + Jn(_bestDayProfit) +
                        ",\"positive_days\":" + Jn(_positiveDaysProfit));
                }
            }
            else if (!HasAnyMasterExposure())
                _objectiveClosing = false;
        }

        private double FundedPayoutTargetBalance()
        {
            return InitialBalance * (1.0 + FundedPayoutLockTargetPct / 100.0);
        }

        private bool FundedPayoutCloseCondition()
        {
            return IsFunded() && FundedPayoutLockEnabled && !_fundedPayoutLocked &&
                   Account.Equity >= FundedPayoutTargetBalance() && BestDaySatisfied();
        }

        private bool FundedPayoutCompletionRiskActive()
        {
            if (!IsFunded() || !FundedPayoutLockEnabled || _fundedPayoutLocked) return false;
            double target = FundedPayoutTargetBalance();
            return Math.Max(Account.Balance, Account.Equity) >= target &&
                   (!BestDaySatisfied() || Account.Equity < target);
        }

        private void CheckFundedPayoutCompletion()
        {
            if (!IsFunded() || !FundedPayoutLockEnabled || _startupHalt) return;

            if (_fundedPayoutLocked)
            {
                // Sticky by design: a running instance cannot silently re-arm after a withdrawal.
                // Reconcile the verified cash adjustment and start a new stage epoch while flat.
                if (HasAnyMasterExposure())
                {
                    CancelAllMasterPending();
                    CloseAllMasterPositions("funded_payout_locked_late_exposure");
                }
                return;
            }

            double target = FundedPayoutTargetBalance();
            if (!_fundedPayoutClosing)
            {
                if (Account.Equity < target || !BestDaySatisfied()) return;
                _fundedPayoutClosing = true;
                _lastGateReason = "FUNDED PAYOUT CLOSE IN PROGRESS";
                Journal("GUARD",
                    "\"kind\":\"funded_payout_close_start\",\"equity\":" + Jn(Account.Equity) +
                    ",\"target\":" + Jn(target) + ",\"best_day\":" + Jn(_bestDayProfit) +
                    ",\"positive_days\":" + Jn(_positiveDaysProfit));
            }

            CancelAllMasterPending();
            CloseAllMasterPositions("funded_payout_target");
            if (!HasAnyAccountExposure())
            {
                RefreshObjectiveStats(true);
                if (Account.Balance >= target && BestDaySatisfied())
                {
                    _fundedPayoutLocked = true;
                    _fundedPayoutClosing = false;
                    _lastGateReason = "FUNDED PAYOUT LOCKED";
                    Journal("GUARD",
                        "\"kind\":\"funded_payout_locked\",\"balance\":" + Jn(Account.Balance) +
                        ",\"target\":" + Jn(target) + ",\"best_day\":" + Jn(_bestDayProfit) +
                        ",\"positive_days\":" + Jn(_positiveDaysProfit));
                    Print("*** ATLAS FUNDED PAYOUT LOCKED: balance ${0:F2}, Best Day {1:F1}% of positive days. Restart only after verified payout reconciliation. ***",
                        Account.Balance, _positiveDaysProfit > 0 ? 100.0 * _bestDayProfit / _positiveDaysProfit : 0);
                }
                else
                {
                    _fundedPayoutClosing = false;
                    _lastGateReason = Account.Balance < target
                        ? "Funded payout close settled below target"
                        : "Funded payout waiting for Best Day consistency";
                    Journal("GUARD",
                        "\"kind\":\"funded_payout_recheck_failed\",\"balance\":" + Jn(Account.Balance) +
                        ",\"target\":" + Jn(target) + ",\"best_day\":" + Jn(_bestDayProfit) +
                        ",\"positive_days\":" + Jn(_positiveDaysProfit));
                }
            }
        }

        // =========================================================================================
        // DAX ENGINE — explicit GER40 M15
        // =========================================================================================
        private void InitialiseDax()
        {
            _daxA = new DaxWindow
            {
                Label = DaxALabel,
                Enabled = DaxAEnabled,
                TwoSided = false,
                SignalStart = 570,
                SignalEnd = 600,
                // V33.SPARK CORE-2: A1 entry cutoff truncated 780 (13:00) -> 675 (11:15 London).
                // RunDaxWindow already gates entries on tod >= EntryEnd, so this value is the whole
                // control. The last bar that can fire an A1 entry is the 11:00 M15 bar (tod=660).
                EntryEnd = 675,
                SlPts = DaxASlPts,
                TpPts = DaxATpPts,
                RiskUsd = DaxARiskUsd,
                RangeMin = 20,
                RangeMax = 80,
                MinClosePosition = 0.20,
                RequiredBars = 2
            };
            // ===== V34.ARSENAL: DAX B is the Frankfurt 08:00 cash-open velocity burst. =====
            // The inherited A2 measured 08:15-08:30 London with NO range ceiling and allowed
            // entries to 11:00, so it traded inside Frankfurt opening order-matching chop and
            // then overlapped DAX A1. The re-engineered window is the FIRST cash candle
            // (08:00-08:15 London / 09:00-09:15 Frankfurt), the range is bounded 20-55 pts, the
            // setup is LONG ONLY and gap-aligned, and entries stop at 09:15. RunDaxTimeBackstop
            // flattens any surviving A2 at the same minute so the book is flat before A1.
            _daxB = new DaxWindow
            {
                Label = DaxBLabel,
                Enabled = DaxBEnabled,
                TwoSided = false,
                SignalStart = DaxBSignalStartTod,
                SignalEnd = DaxBSignalEndTod,
                EntryEnd = DaxBEntryEndTod,
                SlPts = DaxBSlPts,
                TpPts = DaxBTpPts,
                RiskUsd = DaxBRiskUsd,
                RangeMin = DaxBRangeMinPts,
                RangeMax = DaxBRangeMaxPts,
                MinClosePosition = DaxBMinClosePosition,
                RequiredBars = 1,
                RequirePriorCashCloseAlignment = true
            };
            _daxA.Reset();
            _daxB.Reset();

            if (_daxM15 == null) return;
            DateTime today = ToUk(Server.TimeInUtc).Date;
            int firstToday = -1;
            for (int i = _daxM15.Count - 1; i >= 0; i--)
            {
                if (ToUk(_daxM15.OpenTimes[i]).Date != today) break;
                firstToday = i;
            }
            _daxLastProcessed = firstToday >= 0 ? firstToday - 1 : _daxM15.Count - 2;
        }

        private void ProcessDaxClosedBars()
        {
            if (_daxM15 == null) return;
            int lastClosed = _daxM15.Count - 2;
            while (_daxLastProcessed < lastClosed)
            {
                _daxLastProcessed++;
                if (_daxLastProcessed >= 1) ProcessDaxBar(_daxLastProcessed);
            }
        }

        private void ProcessDaxBar(int i)
        {
            DateTime openUtc = _daxM15.OpenTimes[i];
            DateTime uk = ToUk(openUtc);
            int tod = uk.Hour * 60 + uk.Minute;
            int endTod = tod + 15;
            double op = _daxM15.OpenPrices[i];
            double hi = _daxM15.HighPrices[i];
            double lo = _daxM15.LowPrices[i];
            double cl = _daxM15.ClosePrices[i];

            if (uk.Date != _daxDay) ResetDaxDay(uk);
            if (endTod >= 990) CloseLabels(new[] { DaxALabel, DaxBLabel, DaxCLabel }, DaxSymbolName, "dax_session_end");
            if (_daxNoTradeDay)
            {
                _daxPrevHigh = hi;
                _daxPrevLow = lo;
                _daxHasPrev = true;
                return;
            }

            RunDaxWindow(_daxA, tod, cl, hi, lo, openUtc);
            RunDaxWindow(_daxB, tod, cl, hi, lo, openUtc);
            if (DaxCEnabled && !IsFunded()) RunDaxCoil(tod, endTod, op, hi, lo, cl, uk, openUtc);

            _daxPrevHigh = hi;
            _daxPrevLow = lo;
            _daxHasPrev = true;
        }

        private void RunDaxWindow(DaxWindow window, int tod, double close, double high, double low, DateTime openUtc)
        {
            if (window == null || !window.Enabled) return;
            if (tod >= window.SignalStart && tod < window.SignalEnd)
            {
                window.High = Math.Max(window.High, high);
                window.Low = Math.Min(window.Low, low);
                window.Close = close;
                window.SeenBars++;
            }
            if (!window.Done && tod >= window.SignalEnd)
            {
                window.Done = true;
                double range = window.SeenBars > 0 ? window.High - window.Low : 0;
                double closePosition = range > 0 ? (window.Close - window.Low) / range : 0;
                bool geometryValid = window.SeenBars >= window.RequiredBars && range >= window.RangeMin &&
                               range <= window.RangeMax && closePosition >= window.MinClosePosition;
                // V34.ARSENAL DAX B gap alignment. An unavailable or non-positive reference
                // fails CLOSED: the day is simply not traded rather than traded unfiltered.
                bool referenceAligned = !window.RequirePriorCashCloseAlignment ||
                    (!double.IsNaN(_daxPriorCashClose) && _daxPriorCashClose > 0 &&
                     window.Close > _daxPriorCashClose);
                window.Valid = geometryValid && referenceAligned;
                Journal("SIGNAL",
                    "\"label\":\"" + window.Label + "\",\"range\":" + Jn(range) +
                    ",\"close_position\":" + Jn(closePosition) + ",\"bars\":" + window.SeenBars +
                    ",\"geometry_valid\":" + Bool(geometryValid) +
                    ",\"requires_prior_cash_close\":" + Bool(window.RequirePriorCashCloseAlignment) +
                    ",\"prior_cash_close\":" + Jn(_daxPriorCashClose) +
                    ",\"reference_aligned\":" + Bool(referenceAligned) +
                    ",\"valid\":" + Bool(window.Valid));
            }
            if (window.Done && !window.Traded && !window.ExpiredLogged && tod >= window.EntryEnd)
            {
                window.ExpiredLogged = true;
                Journal("SESSION_DECISION", "\"label\":\"" + window.Label +
                    "\",\"state\":\"EXPIRED\",\"reason\":\"" + (window.Valid ? "no_breakout_or_no_eligible_entry" : "invalid_opening_range") + "\"");
            }
            if (!window.Done || !window.Valid || window.Traded || _signalRetries.ContainsKey(window.Label) ||
                tod < window.SignalEnd || tod >= window.EntryEnd) return;

            bool up = close >= window.High;
            bool down = window.TwoSided && close <= window.Low;
            if (!up && !down) return;
            if (!CanDaxTrade(window.Label)) return;

            var proposal = new TradeProposal
            {
                Engine = "DAX",
                Setup = window.Label == DaxALabel ? "A1" : "A2",
                Label = window.Label,
                Symbol = _daxSymbol,
                Side = up ? TradeType.Buy : TradeType.Sell,
                // Under the frontier, A1 is budget-allocated rather than table-allocated.
                RequestedRisk = window.Label == DaxALabel ? EffectiveDaxARiskRequest() : window.RiskUsd,
                SlDistancePts = window.SlPts,
                TpDistancePts = window.TpPts,
                Style = EntryStyle.MarketWithPips,
                DecisionTimeUtc = openUtc.AddMinutes(15),
                TriggerPrice = up ? window.High : window.Low,
                TriggerDirection = up ? 1 : -1
            };
            DateTime retryDeadline = tod + 15 >= window.EntryEnd
                ? Server.TimeInUtc
                : Server.TimeInUtc.AddSeconds(30);
            SubmitSignalWithBoundedRetry(proposal, retryDeadline);
        }

        private void RunDaxCoil(int tod, int endTod, double open, double high, double low, double close, DateTime uk, DateTime openUtc)
        {
            if (tod == 480) _daxCDayOpen = open;
            if (tod >= 480 && tod < 510)
            {
                _daxCOrHigh = Math.Max(_daxCOrHigh, high);
                _daxCOrLow = Math.Min(_daxCOrLow, low);
                _daxCOrBars++;
                if (tod == 495) _daxCOrReady = _daxCOrBars >= 2;
                return;
            }
            // V34.ARSENAL: the pre-London squeeze stops accepting entries at 09:15 London. The
            // inherited 11:00 cutoff let late breakouts bleed into European lunch rotation,
            // which is where this setup's drawdown came from. Eligible bars now open at
            // 08:30, 08:45 and 09:00 only.
            if (!_daxCOrReady || _daxCTraded || _signalRetries.ContainsKey(DaxCLabel) || tod < 510 ||
                endTod > DaxCEntryEndTod) return;
            if (_daxHasPrev && high < _daxPrevHigh && low > _daxPrevLow) _daxCInsideSeen = true;
            if (!_daxCInsideSeen || uk.DayOfWeek == DayOfWeek.Monday || _daxCDayOpen <= 0) return;

            bool up = close >= _daxCOrHigh;
            bool down = close <= _daxCOrLow;
            if (!up && !down) return;
            if (!CanDaxTrade(DaxCLabel)) return;

            double sl = DaxCSlBp * 1e-4 * _daxCDayOpen;
            var proposal = new TradeProposal
            {
                Engine = "DAX",
                Setup = "A3",
                Label = DaxCLabel,
                Symbol = _daxSymbol,
                Side = up ? TradeType.Buy : TradeType.Sell,
                RequestedRisk = DaxCRiskUsd,
                SlDistancePts = sl,
                TpDistancePts = sl * DaxCTpMultiple,
                Style = EntryStyle.MarketWithPips,
                DecisionTimeUtc = openUtc.AddMinutes(15),
                TriggerPrice = up ? _daxCOrHigh : _daxCOrLow,
                TriggerDirection = up ? 1 : -1
            };
            DateTime retryDeadline = endTod >= DaxCEntryEndTod
                ? Server.TimeInUtc
                : Server.TimeInUtc.AddSeconds(30);
            SubmitSignalWithBoundedRetry(proposal, retryDeadline);
        }

        private bool CanDaxTrade(string label)
        {
            if (_daxRegimeHalt || _daxDayBlocked || _daxDailyProfitLocked) return false;
            if (HasPosition(label, DaxSymbolName) || TradedToday(label, DaxSymbolName, _daxDay, true)) return false;
            return true;
        }

        private void ResetDaxDay(DateTime uk)
        {
            _signalRetries.Remove(DaxALabel);
            _signalRetries.Remove(DaxBLabel);
            _signalRetries.Remove(DaxCLabel);
            _daxDay = uk.Date;
            _daxNoTradeDay = !IsDaxTradingDay(uk);
            _daxHasPrev = false;
            _daxDayBlocked = false;
            _daxDailyProfitLocked = false;
            _daxA.Reset();
            _daxB.Reset();
            _daxCOrReady = false;
            _daxCInsideSeen = false;
            _daxCTraded = false;
            _daxCOrBars = 0;
            _daxCOrHigh = double.MinValue;
            _daxCOrLow = double.MaxValue;
            _daxCDayOpen = 0;
            _daxBreakEvenRatcheted.Clear();
            _daxBreakEvenRetryNotBeforeTicks.Clear();
            _daxPriorCashClose = ResolvePriorDaxCashClose(uk.Date);
        }

        // V34.ARSENAL: the DAX B trend-alignment reference is the close of the M15 bar that ends
        // the previous session at 16:30 London (the 16:15 bar). Resolved from CLOSED history
        // only, newest first. If that exact bar is missing — half day, data gap, holiday — the
        // last closed bar of the most recent earlier London date is used instead. NaN means no
        // reference exists, which blocks DAX B for the day rather than trading it unfiltered.
        private double ResolvePriorDaxCashClose(DateTime londonToday)
        {
            if (_daxM15 == null || _daxM15.Count < 2) return double.NaN;
            double fallback = double.NaN;
            DateTime fallbackDay = DateTime.MinValue;
            for (int i = _daxM15.Count - 2; i >= 0; i--)
            {
                DateTime uk = ToUk(_daxM15.OpenTimes[i]);
                if (uk.Date >= londonToday) continue;
                if (fallbackDay == DateTime.MinValue)
                {
                    fallbackDay = uk.Date;
                    fallback = _daxM15.ClosePrices[i];
                }
                else if (uk.Date != fallbackDay) break;
                if (uk.Hour * 60 + uk.Minute == DaxBPriorCashCloseTod) return _daxM15.ClosePrices[i];
            }
            return fallback;
        }

        private void CheckDaxLocalProfitLock()
        {
            if (_daxDay == DateTime.MinValue || _daxDailyProfitLocked) return;
            double maxDay = DaxDailyMaxProfitUsd();
            double pnl = ModuleDayPnl(new[] { DaxALabel, DaxBLabel, DaxCLabel }, DaxSymbolName, _daxDay, true);
            if (maxDay > 0 && pnl >= 0.70 * maxDay)
            {
                _daxDailyProfitLocked = true;
                Journal("GUARD", "\"kind\":\"dax_daily_profit_lock\",\"pnl\":" + Jn(pnl) + ",\"threshold\":" + Jn(0.70 * maxDay));
            }
        }

        private void RebuildDaxCurrentDayGuards()
        {
            DateTime today = ToUk(Server.TimeInUtc).Date;
            string[] labels =
            {
                DaxALabel, DaxBLabel, DaxCLabel, "ASetup", "BSetup", "CSetup"
            };
            var closes = History
                .Where(x => x.SymbolName == DaxSymbolName && labels.Contains(x.Label) &&
                            ToUk(x.ClosingTime).Date == today)
                .OrderBy(x => x.ClosingTime)
                .ToList();

            _daxConsecutiveLosses = 0;
            foreach (var trade in closes)
                _daxConsecutiveLosses = trade.NetProfit > 0 ? 0 : _daxConsecutiveLosses + 1;
            _daxDayBlocked = _daxConsecutiveLosses >= 3;

            double maxDay = DaxDailyMaxProfitUsd();
            double pnl = closes.Sum(x => x.NetProfit) +
                         Positions.Where(x => x.SymbolName == DaxSymbolName && labels.Contains(x.Label))
                                  .Sum(x => x.NetProfit);
            _daxDailyProfitLocked = maxDay > 0 && pnl >= 0.70 * maxDay;

            if (_daxDayBlocked || _daxDailyProfitLocked)
                Journal("GUARD",
                    "\"kind\":\"dax_restart_day_state\",\"consecutive_losses\":" + _daxConsecutiveLosses +
                    ",\"day_pnl\":" + Jn(pnl) + ",\"profit_locked\":" + Bool(_daxDailyProfitLocked));
        }

        private void RunDaxTimeBackstop()
        {
            DateTime uk = ToUk(Server.TimeInUtc);
            int tod = uk.Hour * 60 + uk.Minute;
            if (tod >= 985)
                CloseLabels(new[] { DaxALabel, DaxBLabel, DaxCLabel }, DaxSymbolName, "dax_timer_flatten");
            // V34.ARSENAL: the DAX B velocity burst is a one-hour lane. Any A2 position still
            // open at 09:15 London is closed at market so the DAX book is flat before A1 opens
            // at 09:30. CloseLabels is a no-op once flat, so repeating this for the rest of the
            // session costs nothing and needs no separate per-day latch.
            else if (tod >= DaxBHardFlattenTod)
                CloseLabels(new[] { DaxBLabel }, DaxSymbolName, "dax_b_velocity_window_flatten");
            RunDaxBreakEvenRatchet();
        }

        // V34.ARSENAL: the authorised stop distance for a live position, preferring the durable
        // ownership ledger and falling back to reconstructed position metadata. Returns 0 when
        // neither source knows it, which callers must treat as "do not act".
        private double AuthorizedStopDistancePts(Position position)
        {
            if (position == null) return 0;
            OwnershipLedgerRecord record;
            if (_ownershipLedger.TryGetValue(OwnershipLedgerKey("POSITION", position.Id), out record) &&
                record != null && record.StopDistancePts > 0)
                return record.StopDistancePts;
            PositionMeta meta;
            if (_positionMeta.TryGetValue(position.Id, out meta) && meta != null &&
                meta.StopRiskDistancePts > 0)
                return meta.StopRiskDistancePts;
            return 0;
        }

        // V33.SPARK CORE-2 introduced this as a DAX A1 break-even ratchet; V34.ARSENAL extends
        // it to DAX A3 (the pre-London squeeze) as well.
        // Once a live A1 position has run +1.5R (37.5 pts) in its favour, the stop is moved to
        // entry + 2.0 pts in the trade's direction, converting the remaining trade to a free option.
        //
        // Deliberate design constraints, so this does not weaken the inherited safety model:
        //   - Authority: gated on CanManagePosition, the same predicate EmergencyClose enforces
        //     internally. A non-owned or unbound position is skipped, never mutated.
        //   - Direction: the stop is only ever TIGHTENED. ValidPositionProtection rejects a stop
        //     WIDER than the authorised distance, so a ratchet can never trip the protection audit;
        //     the explicit worse-than test below also prevents a late call from loosening a stop.
        //   - Idempotence: _daxBreakEvenRatcheted latches per position ID, because this method runs
        //     once per second from both OnTimer branches.
        //   - Take-profit is passed through unchanged; ValidPositionProtection requires a live target.
        private void RunDaxBreakEvenRatchet()
        {
            if (_daxSymbol == null || !BindingAuthorityCurrent()) return;
            foreach (Position pos in Positions
                .Where(x => x.SymbolName == DaxSymbolName &&
                            (x.Label == DaxALabel || x.Label == DaxCLabel)).ToArray())
            {
                if (_daxBreakEvenRatcheted.Contains(pos.Id)) continue;
                if (_closeRequestMeta.ContainsKey(pos.Id) || _closeRetry.Contains(pos.Id)) continue;
                long dueTicks;
                if (RunningMode == RunningMode.RealTime &&
                    _daxBreakEvenRetryNotBeforeTicks.TryGetValue(pos.Id, out dueTicks) &&
                    Stopwatch.GetTimestamp() < dueTicks) continue;
                if (!CanManagePosition(pos)) continue;
                if (!pos.StopLoss.HasValue) continue;

                // A1 keeps its frozen absolute +37.5 pt trigger (1.5R of the fixed 25.0 pt A1
                // stop). A3's stop is a basis-point fraction of the DAX day open and therefore
                // varies with price, so its +1.5R trigger is derived from the position's own
                // authorised stop distance. A position whose stop distance is unknown is
                // skipped rather than ratcheted against a guessed denominator.
                double triggerPts;
                double offsetPts;
                if (pos.Label == DaxALabel)
                {
                    triggerPts = DaxABreakEvenTriggerPts;
                    offsetPts = DaxABreakEvenOffsetPts;
                }
                else
                {
                    double authorizedStop = AuthorizedStopDistancePts(pos);
                    if (authorizedStop <= 0) continue;
                    triggerPts = DaxCBreakEvenTriggerRMultiple * authorizedStop;
                    offsetPts = DaxCBreakEvenOffsetPts;
                }

                double gainPts = pos.TradeType == TradeType.Buy
                    ? _daxSymbol.Bid - pos.EntryPrice
                    : pos.EntryPrice - _daxSymbol.Ask;
                if (double.IsNaN(gainPts) || gainPts < triggerPts) continue;

                double bePrice = NormalizePriceToTick(_daxSymbol, pos.TradeType == TradeType.Buy
                    ? pos.EntryPrice + offsetPts
                    : pos.EntryPrice - offsetPts);
                bool stopIsWorse = pos.TradeType == TradeType.Buy
                    ? pos.StopLoss.Value < bePrice
                    : pos.StopLoss.Value > bePrice;
                if (!stopIsWorse)
                {
                    _daxBreakEvenRatcheted.Add(pos.Id);
                    _daxBreakEvenRetryNotBeforeTicks.Remove(pos.Id);
                    continue;
                }

                TradeResult modify = ModifyPosition(pos, bePrice, pos.TakeProfit, ProtectionType.Absolute);
                if (modify == null || !modify.IsSuccessful)
                {
                    // Leave the position unlatched so a later pass retries, but back off first: the
                    // original authorised stop is still in place, so a failed ratchet is not a risk
                    // event and must not be re-called or re-journalled every second.
                    _daxBreakEvenRetryNotBeforeTicks[pos.Id] =
                        MonotonicDeadlineAfterSeconds(BrokerRetryBackoffSeconds);
                    Journal("GUARD", "\"kind\":\"dax_be_ratchet_rejected\",\"position_id\":" + pos.Id +
                        ",\"label\":\"" + Js(pos.Label) + "\",\"be_price\":" + Jn(bePrice) +
                        ",\"error\":\"" + Js(modify == null ? "null" : modify.Error.ToString()) + "\"");
                    continue;
                }

                _daxBreakEvenRatcheted.Add(pos.Id);
                _daxBreakEvenRetryNotBeforeTicks.Remove(pos.Id);
                // Record the price the broker just accepted, not a read-back of pos.StopLoss, which
                // is not guaranteed to reflect the modify synchronously. RecordExitExecutionQuality
                // uses LastStopPrice as the expected price on a stop exit; a stale value there would
                // under-report slippage against the ratcheted stop.
                PositionMeta meta;
                if (_positionMeta.TryGetValue(pos.Id, out meta) && meta != null)
                    meta.LastStopPrice = bePrice;
                Journal("PROTECT", "\"kind\":\"dax_be_ratchet\",\"position_id\":" + pos.Id +
                    ",\"label\":\"" + Js(pos.Label) + "\",\"entry\":" + Jn(pos.EntryPrice) +
                    ",\"gain_pts\":" + Jn(gainPts) + ",\"trigger_pts\":" + Jn(triggerPts) +
                    ",\"new_stop\":" + Jn(bePrice) + ",\"offset_pts\":" + Jn(offsetPts));
            }
        }

        // =========================================================================================
        // UK ENGINE — explicit UK100 M15 + H4
        // =========================================================================================
        private void InitialiseUk()
        {
            if (UkBRequested)
                Print("UK B label {0} is present for identity, but the FTMO production master forces this experimental setup OFF.", UkBLabel);
            if (_ukM15 == null) return;
            DateTime today = ToUk(Server.TimeInUtc).Date;
            int firstToday = -1;
            for (int i = _ukM15.Count - 1; i >= 0; i--)
            {
                if (ToUk(_ukM15.OpenTimes[i]).Date != today) break;
                firstToday = i;
            }
            _ukLastProcessed = firstToday >= 0 ? firstToday - 1 : _ukM15.Count - 2;
        }

        private void ProcessUkClosedBars()
        {
            if (_ukM15 == null) return;
            int lastClosed = _ukM15.Count - 2;
            while (_ukLastProcessed < lastClosed)
            {
                _ukLastProcessed++;
                if (_ukLastProcessed >= 1) ProcessUkBar(_ukLastProcessed);
            }
        }

        private void ProcessUkBar(int i)
        {
            DateTime openUtc = _ukM15.OpenTimes[i];
            DateTime london = ToUk(openUtc);
            TimeSpan tod = london.TimeOfDay;
            double open = _ukM15.OpenPrices[i];
            double high = _ukM15.HighPrices[i];
            double low = _ukM15.LowPrices[i];
            double close = _ukM15.ClosePrices[i];

            if (london.Date != _ukDay) ResetUkDay(london.Date);
            if (tod == new TimeSpan(8, 0, 0)) _ukDayOpen = open;

            // Setup B intentionally remains disabled in the FTMO Master.

            if (!UkAEnabled) return;
            if (tod >= new TimeSpan(8, 0, 0) && tod < new TimeSpan(8, 30, 0))
            {
                _ukOrHigh = Math.Max(_ukOrHigh, high);
                _ukOrLow = Math.Min(_ukOrLow, low);
                if (tod == new TimeSpan(8, 15, 0)) _ukOrReady = true;
                return;
            }
            if (!_ukOrReady || _ukATraded) return;
            if (tod < new TimeSpan(8, 30, 0) || tod + TimeSpan.FromMinutes(15) > new TimeSpan(11, 0, 0)) return;

            // V34.ARSENAL: compression is still tracked for the Strategy Radar, but it is no
            // longer a precondition. The re-engineered B1 is a momentum SWEEP of the
            // 08:00-08:30 opening range, not a coil; waiting for an inside bar first is what
            // left the inherited setup entering after the opening impulse had been spent.
            double previousHigh = _ukM15.HighPrices[i - 1];
            double previousLow = _ukM15.LowPrices[i - 1];
            if (high < previousHigh && low > previousLow) _ukCompressionSeen = true;

            TradeType? side = null;
            if (close >= _ukOrHigh) side = TradeType.Buy;
            else if (close <= _ukOrLow) side = TradeType.Sell;
            if (!side.HasValue) return;

            if (london.DayOfWeek == DayOfWeek.Monday || london.Month == 12) return;
            if (_ukDayOpen <= 0 || HasPosition(UkALabel, UkSymbolName) ||
                HasPending(UkALabel, UkSymbolName) || TradedToday(UkALabel, UkSymbolName, _ukDay, true)) return;
            if (_ukKillHalt) return;
            if (UkH4Gate && !UkH4Aligned(side.Value)) return;
            // V34.ARSENAL: the sweep now journals a SIGNAL record like the DAX and NAS engines
            // do, so a rejected or accepted sweep is reconstructable from the audit trail.
            // Compression is reported here as context; it is no longer an entry precondition.
            Journal("SIGNAL",
                "\"label\":\"" + UkALabel + "\",\"kind\":\"uk_open_range_sweep\"" +
                ",\"side\":\"" + side.Value + "\",\"or_high\":" + Jn(_ukOrHigh) +
                ",\"or_low\":" + Jn(_ukOrLow) + ",\"break_close\":" + Jn(close) +
                ",\"compression_seen\":" + Bool(_ukCompressionSeen) +
                ",\"sl_dist\":" + Jn(UkSweepSlPts) + ",\"tp_dist\":" + Jn(UkSweepTpPts));
            // V34.ARSENAL: commodity-sector confluence. The FTSE 100 is weighted to oil, mining
            // and banking, so a BUY sweep is only taken when Brent or Copper is green on the
            // day. The gate is deliberately asymmetric — it does not constrain SELL sweeps —
            // because that is the rule the research measured.
            string confluenceReason;
            if (side.Value == TradeType.Buy && !UkBuyConfluenceSatisfied(out confluenceReason))
            {
                Journal("SKIP", "\"label\":\"" + UkALabel + "\",\"reason\":\"" +
                    Js(confluenceReason) + "\"");
                return;
            }
            if (_stateRebuildMode)
            {
                _ukATraded = true;
                Journal("SKIP", "\"label\":\"" + UkALabel + "\",\"reason\":\"missed_signal_on_restart\",\"decision_utc\":\"" +
                    openUtc.AddMinutes(15).ToString("o") + "\"");
                return;
            }
            if (SpreadTooWide(_ukSymbol, UkMaxSpreadPts)) return;
            // V34.ARSENAL: immediate MARKET entry with a fixed 11.0 pt stop and 20.0 pt target
            // (1.82R), sized to the FTSE's average daily range. The inherited limit-at-mid with
            // a 13bp stop and a 2.5R multiple could not be filled and carried on this index:
            // pullback limits filled into rotations that never expanded to target. The setup
            // code stays "B1" because LabelSetup() round-trips it through the ownership ledger
            // and the broker comment; UkLimitEntry/UkLimitExpiryBars remain declared for
            // .cbotset compatibility and now default OFF.
            var proposal = new TradeProposal
            {
                Engine = "UK",
                Setup = "B1",
                Label = UkALabel,
                Symbol = _ukSymbol,
                Side = side.Value,
                RequestedRisk = UkARiskUsd,
                SlDistancePts = UkSweepSlPts,
                TpDistancePts = UkSweepTpPts,
                Style = UkLimitEntry ? EntryStyle.LimitAtMid : EntryStyle.MarketWithPips,
                ExpiryBars = UkLimitExpiryBars,
                DecisionTimeUtc = openUtc.AddMinutes(15),
                TriggerPrice = side.Value == TradeType.Buy ? _ukOrHigh : _ukOrLow,
                TriggerDirection = side.Value == TradeType.Buy ? 1 : -1
            };
            if (SubmitProposal(proposal)) _ukATraded = true;
        }

        private void ResetUkDay(DateTime day)
        {
            _ukDay = day;
            _ukOrHigh = double.MinValue;
            _ukOrLow = double.MaxValue;
            _ukDayOpen = 0;
            _ukOrReady = false;
            _ukCompressionSeen = false;
            _ukATraded = false;
        }

        private bool UkH4Aligned(TradeType side)
        {
            if (_ukH4 == null || _ukH4Ema == null || _ukH4.Count < 52) return false;
            int i = _ukH4.Count - 2;
            double close = _ukH4.ClosePrices[i];
            double ema = _ukH4Ema.Result[i];
            if (double.IsNaN(ema) || ema <= 0) return false;
            return side == TradeType.Buy ? close > ema : close < ema;
        }

        // V34.ARSENAL: resolve the configured commodity confluence symbols exactly once. A
        // missing symbol is tolerated here and never becomes a startup halt; the caller alone
        // decides what an empty resolved set means for a trade decision.
        private void EnsureUkConfluenceContexts()
        {
            if (_ukConfluenceInitialised) return;
            _ukConfluenceInitialised = true;
            foreach (string raw in (UkConfluenceSymbolsCsv ?? "").Split(','))
            {
                string symbolName = raw.Trim();
                if (symbolName.Length == 0) continue;
                try
                {
                    if (Symbols.GetSymbol(symbolName) == null) continue;
                    Bars daily = MarketData.GetBars(TimeFrame.Daily, symbolName);
                    if (daily != null) _ukConfluenceDaily.Add(daily);
                }
                catch (Exception ex)
                {
                    Print("ATLAS UK CONFLUENCE | {0} unavailable: {1}", symbolName, ex.Message);
                }
            }
            Journal("UK_CONFLUENCE_INIT",
                "\"enabled\":" + Bool(UkConfluenceFilterOn) +
                ",\"configured\":\"" + Js(UkConfluenceSymbolsCsv ?? "") + "\"" +
                ",\"resolved\":" + _ukConfluenceDaily.Count);
        }

        // True when at least one configured commodity is green on the day (current daily close
        // above that day's open). Fails CLOSED: with the filter on and no usable data the buy
        // sweep is refused, because the researched edge is conditioned on this confluence and
        // an unverifiable condition is not a satisfied one.
        private bool UkBuyConfluenceSatisfied(out string reason)
        {
            reason = "";
            if (!UkConfluenceFilterOn) return true;
            EnsureUkConfluenceContexts();
            bool anyUsable = false;
            foreach (Bars daily in _ukConfluenceDaily)
            {
                if (daily == null || daily.Count < 1) continue;
                int i = daily.Count - 1;
                double dayOpen = daily.OpenPrices[i];
                double dayClose = daily.ClosePrices[i];
                if (dayOpen <= 0 || dayClose <= 0 || double.IsNaN(dayOpen) || double.IsNaN(dayClose))
                    continue;
                anyUsable = true;
                if (dayClose > dayOpen) return true;
            }
            if (!anyUsable)
            {
                if (!_ukConfluenceWarned)
                {
                    _ukConfluenceWarned = true;
                    Print("*** ATLAS UK B1 CONFLUENCE UNAVAILABLE: no configured commodity symbol resolved with usable daily data. BUY sweeps are refused. Set the broker's real Brent/Copper symbol names, or turn the filter off. ***");
                }
                reason = "uk_confluence_data_unavailable";
                return false;
            }
            reason = "uk_confluence_not_green";
            return false;
        }

        private void RunUkTimeBackstop()
        {
            // V34.ARSENAL: the London sweep is a morning lane. The whole UK book is flattened at
            // 11:30 London rather than held to the 16:30 cash close.
            DateTime london = ToUk(Server.TimeInUtc);
            if (london.Hour * 60 + london.Minute < UkSessionFlattenTod) return;
            CloseLabels(new[] { UkALabel, UkBLabel }, UkSymbolName, "uk_session_flatten_1130");
            CancelPendingLabel(UkALabel, UkSymbolName);
        }

        // =========================================================================================
        // GOLD ENGINE — V34.ARSENAL XAUUSD H1 STRUCTURAL BREAK-OF-STRUCTURE (setup D1)
        // =========================================================================================
        // A non-correlated commodity swing lane. Everything is evaluated on CLOSED H1 bars:
        //   - A 3-candle fractal pivot is confirmed only once the bar after it has closed, and the
        //     pivot used for a break is always strictly older than the breaking bar, so a break can
        //     never be self-confirming.
        //   - A closed bar whose CLOSE is beyond the most recent confirmed pivot is the entry.
        //   - The stop sits beyond the OPPOSING structural pivot plus a buffer, raised to
        //     GoldMinimumStopUsd when structure is tighter than that, and the whole setup is
        //     rejected when structure is wider than GoldMaximumStopUsd (a broken structure, not a
        //     swing entry). VolumeForRisk still applies the absolute notional ceiling on top.
        //   - The target is GoldTargetRMultiple x the stop distance (1.5R-2.0R research band).
        // Gold is the only multi-day lane in the portfolio: it is deliberately absent from the
        // European cross-day flatten and from every intraday session backstop. GoldMaxHoldHours is
        // the bound that replaces them. Every proposal still passes the single atomic account gate.
        private void InitialiseGold()
        {
            if (!GoldEnabled || _goldH1 == null) return;
            _goldLastProcessed = Math.Max(-1, _goldH1.Count - 2);
            _goldSwingHigh = double.NaN;
            _goldSwingLow = double.NaN;
            _goldLastBrokenHigh = double.NaN;
            _goldLastBrokenLow = double.NaN;
            // Seed pivot state from recent closed history so the first live bar can already be a
            // valid break, without replaying historical bars as tradable signals.
            int start = Math.Max(3, _goldH1.Count - GoldMinimumBars);
            for (int i = start; i <= _goldH1.Count - 2; i++) UpdateGoldFractals(i);
        }

        // Confirms the fractal centred on bar i-2 from its neighbours i-3 and i-1. Every bar used is
        // closed and strictly older than bar i, which is what keeps a break independent of its pivot.
        private void UpdateGoldFractals(int i)
        {
            if (_goldH1 == null || i < 3 || i > _goldH1.Count - 1) return;
            double centreHigh = _goldH1.HighPrices[i - 2];
            double centreLow = _goldH1.LowPrices[i - 2];
            if (centreHigh > _goldH1.HighPrices[i - 3] && centreHigh > _goldH1.HighPrices[i - 1])
                _goldSwingHigh = centreHigh;
            if (centreLow < _goldH1.LowPrices[i - 3] && centreLow < _goldH1.LowPrices[i - 1])
                _goldSwingLow = centreLow;
        }

        private void ProcessGoldClosedBars()
        {
            if (!GoldEnabled || _goldH1 == null) return;
            int lastClosed = _goldH1.Count - 2;
            while (_goldLastProcessed < lastClosed)
            {
                _goldLastProcessed++;
                if (_goldLastProcessed >= 3) ProcessGoldBar(_goldLastProcessed);
            }
        }

        private void ProcessGoldBar(int i)
        {
            UpdateGoldFractals(i);
            if (_goldSymbol == null || _goldKillHalt) return;
            if (HasPosition(GoldLabel, GoldSymbolName) || HasPending(GoldLabel, GoldSymbolName)) return;
            if (_signalRetries.ContainsKey(GoldLabel)) return;

            double close = _goldH1.ClosePrices[i];
            if (close <= 0 || double.IsNaN(close)) return;

            int direction = 0;
            double structuralPivot = double.NaN;
            if (!double.IsNaN(_goldSwingHigh) && close > _goldSwingHigh &&
                _goldSwingHigh != _goldLastBrokenHigh)
            {
                direction = 1;
                structuralPivot = _goldSwingLow;
            }
            else if (!double.IsNaN(_goldSwingLow) && close < _goldSwingLow &&
                     _goldSwingLow != _goldLastBrokenLow)
            {
                direction = -1;
                structuralPivot = _goldSwingHigh;
            }
            if (direction == 0) return;

            // Consume the broken pivot immediately, whatever happens next, so a single structural
            // break can only ever produce one proposal even when the gate later rejects it.
            double brokenPivot;
            if (direction > 0)
            {
                _goldLastBrokenHigh = _goldSwingHigh;
                brokenPivot = _goldLastBrokenHigh;
            }
            else
            {
                _goldLastBrokenLow = _goldSwingLow;
                brokenPivot = _goldLastBrokenLow;
            }

            if (double.IsNaN(structuralPivot) || structuralPivot <= 0)
            {
                Journal("SKIP", "\"label\":\"" + GoldLabel +
                    "\",\"reason\":\"gold_no_opposing_structural_pivot\"");
                return;
            }

            TradeType side = direction > 0 ? TradeType.Buy : TradeType.Sell;
            double entryQuote = side == TradeType.Buy ? _goldSymbol.Ask : _goldSymbol.Bid;
            if (entryQuote <= 0 || double.IsNaN(entryQuote) || double.IsInfinity(entryQuote)) return;
            double structuralStop = direction > 0
                ? entryQuote - (structuralPivot - GoldStopBufferUsd)
                : (structuralPivot + GoldStopBufferUsd) - entryQuote;
            double stopDistance = Math.Max(structuralStop, GoldMinimumStopUsd);
            if (GoldMaximumStopUsd > 0 && stopDistance > GoldMaximumStopUsd)
            {
                Journal("SKIP", "\"label\":\"" + GoldLabel +
                    "\",\"reason\":\"gold_structural_stop_too_wide\",\"stop\":" + Jn(stopDistance) +
                    ",\"maximum\":" + Jn(GoldMaximumStopUsd));
                return;
            }
            double targetDistance = stopDistance * GoldTargetRMultiple;
            if (stopDistance <= 0 || targetDistance <= 0) return;

            DateTime decisionUtc = _goldH1.OpenTimes[i].AddHours(1);
            Journal("SIGNAL",
                "\"label\":\"" + GoldLabel + "\",\"kind\":\"gold_break_of_structure\"" +
                ",\"direction\":" + direction + ",\"break_close\":" + Jn(close) +
                ",\"broken_pivot\":" + Jn(brokenPivot) +
                ",\"structural_pivot\":" + Jn(structuralPivot) +
                ",\"structural_stop\":" + Jn(structuralStop) +
                ",\"stop_dist\":" + Jn(stopDistance) + ",\"tp_dist\":" + Jn(targetDistance) +
                ",\"target_r\":" + Jn(GoldTargetRMultiple));

            if (_stateRebuildMode)
            {
                Journal("SKIP", "\"label\":\"" + GoldLabel +
                    "\",\"reason\":\"missed_signal_on_restart\",\"decision_utc\":\"" +
                    decisionUtc.ToString("o") + "\"");
                return;
            }
            if (SpreadTooWide(_goldSymbol, GoldMaxSpreadUsd)) return;

            var proposal = new TradeProposal
            {
                Engine = "GOLD",
                Setup = "D1",
                Label = GoldLabel,
                Symbol = _goldSymbol,
                Side = side,
                RequestedRisk = EffectiveGoldRiskRequest(),
                SlDistancePts = stopDistance,
                TpDistancePts = targetDistance,
                Style = EntryStyle.MarketWithPips,
                ReferenceDistancePts = stopDistance,
                DecisionTimeUtc = decisionUtc,
                TriggerPrice = brokenPivot,
                TriggerDirection = direction
            };
            SubmitSignalWithBoundedRetry(proposal, Server.TimeInUtc.AddSeconds(30));
        }

        // Gold's only time-based control. Every other lane is bounded by its trading session; a swing
        // lane is not, so an explicit maximum hold stops a stalled position carrying risk forever.
        private void RunGoldTimeBackstop()
        {
            if (!GoldEnabled || GoldMaxHoldHours <= 0 || _goldSymbol == null) return;
            DateTime now = Server.TimeInUtc;
            foreach (Position pos in Positions
                .Where(x => x.SymbolName == GoldSymbolName && x.Label == GoldLabel).ToArray())
            {
                if (_closeRequestMeta.ContainsKey(pos.Id) || _closeRetry.Contains(pos.Id)) continue;
                if ((now - AsUtc(pos.EntryTime)).TotalHours < GoldMaxHoldHours) continue;
                Journal("GUARD",
                    "\"kind\":\"gold_max_hold_elapsed\",\"position_id\":" + pos.Id +
                    ",\"entry_utc\":\"" + AsUtc(pos.EntryTime).ToString("o") + "\"" +
                    ",\"max_hold_hours\":" + GoldMaxHoldHours);
                EmergencyClose(pos, "gold_max_hold_elapsed");
            }
        }

        // =========================================================================================
        // NAS ENGINE — explicit US100 M5
        // =========================================================================================
        private void InitialiseNas()
        {
            _nasEtDay = ToEastern(Server.TimeInUtc).ToString("yyyy-MM-dd");
            _nasCalendarYear = ToEastern(Server.TimeInUtc).Year;
            _nasPreviousRthClose = FindPriorNasRthClose();
            _nasTodayLastRthClose = FindNasRthClose(_nasEtDay);
            RebuildPositionMetadataFromOpenBook();
            RebuildNasTodayState();
        }

        private void RebuildNasTodayState()
        {
            DateTime today = ToEastern(Server.TimeInUtc).Date;
            _nasDayRiskUsed = 0;
            foreach (var h in History
                .Where(x => x.SymbolName == NasSymbolName && ToEastern(x.EntryTime).Date == today &&
                            (x.Label == NasCoreLabel || x.Label == NasReLabel ||
                             x.Label == "Bell Core" || x.Label == "Bell-2RE"))
                .OrderBy(x => x.EntryTime))
                ApplyNasTodayState(h.Label, h.TradeType, h.Comment);

            foreach (var p in Positions
                .Where(x => x.SymbolName == NasSymbolName && ToEastern(x.EntryTime).Date == today &&
                            (x.Label == NasCoreLabel || x.Label == NasReLabel)))
                ApplyNasTodayState(p.Label, p.TradeType, p.Comment);

            if (_nasCoreDone && !_nasReDone &&
                !HasPosition(NasCoreLabel, NasSymbolName) && !HasPosition(NasReLabel, NasSymbolName))
                Print("NAS restart note: today's completed core is restored, but a missed stop-event re-entry is not synthesized.");
        }

        private void ApplyNasTodayState(string label, TradeType side, string comment)
        {
            bool core = label == NasCoreLabel || label == "Bell Core";
            bool reentry = label == NasReLabel || label == "Bell-2RE";
            if (!core && !reentry) return;

            double risk = ParseCommentNumber(comment, "q=");
            if (risk <= 0) risk = ParseCommentNumber(comment, "r=");
            if (risk > 0) _nasDayRiskUsed += risk;
            if (reentry) _nasReDone = true;
            if (!core) return;

            _nasCoreDone = true;
            _nasSignalDone = true;
            double range = ParseCommentNumber(comment, "g=");
            if (range > 0) _nasDriveRange = range;
            _nasDriveDirection = side == TradeType.Buy ? 1 : -1;
        }

        private void MarkNasDecisionMissedBeforeArm()
        {
            DateTime et = ToEastern(Server.TimeInUtc);
            if (_nasPendingReentry && !_nasReDone)
            {
                _nasPendingReentry = false;
                _nasReDone = true;
                _signalRetries.Remove(NasReLabel);
                Journal("SKIP", "\"label\":\"" + NasReLabel +
                    "\",\"reason\":\"missed_before_arm_or_restart\",\"armed_utc\":\"" +
                    Server.TimeInUtc.ToString("o") + "\"");
            }
            if (_nasSignalDone || _nasCoreDone || et.TimeOfDay < new TimeSpan(9, 45, 0)) return;
            if (HasPosition(NasCoreLabel, NasSymbolName) ||
                TradedToday(NasCoreLabel, NasSymbolName, et.Date, false)) return;
            _nasSignalDone = true;
            Journal("SKIP", "\"label\":\"" + NasCoreLabel +
                "\",\"reason\":\"missed_before_arm_or_restart\",\"armed_utc\":\"" +
                Server.TimeInUtc.ToString("o") + "\"");
            Print("NAS C1 marked MISSED_BEFORE_ARM for {0}; V34.ARSENAL never catches up the 09:45 ET decision.",
                et.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        private void NasRolloverIfNeeded()
        {
            DateTime et = ToEastern(Server.TimeInUtc);
            string day = et.ToString("yyyy-MM-dd");
            if (day == _nasEtDay) return;

            _signalRetries.Remove(NasCoreLabel);
            _signalRetries.Remove(NasReLabel);

            if (!double.IsNaN(_nasTodayLastRthClose)) _nasPreviousRthClose = _nasTodayLastRthClose;
            CloseLabels(new[] { NasCoreLabel, NasReLabel }, NasSymbolName, "nas_day_rollover");
            _nasEtDay = day;
            if (et.Year != _nasCalendarYear)
            {
                _nasCalendarYear = et.Year;
                SeedNasCalendars();
            }
            _nasSignalDone = false;
            _nasCoreDone = false;
            _nasEarlyLaggardDone = false;
            _nasLaggardDone = false;
            _nasFlattened = false;
            _nasReDone = false;
            _nasPendingReentry = false;
            _nasDayRiskUsed = 0;
            _nasDriveRange = 0;
            _nasDriveDirection = 0;
            _nasSessionOpen = double.NaN;
            _nasTodayLastRthClose = double.NaN;
        }

        private void EvaluateNasSignalIfDue()
        {
            DateTime et = ToEastern(Server.TimeInUtc);
            if (_nasSignalDone || _signalRetries.ContainsKey(NasCoreLabel) || et.TimeOfDay < new TimeSpan(9, 45, 0)) return;
            DateTime scheduledEt = et.Date.AddHours(9).AddMinutes(45);
            if (et > scheduledEt.AddSeconds(NasDecisionGraceSeconds))
            {
                _nasSignalDone = true;
                Journal("SKIP", "\"label\":\"" + NasCoreLabel + "\",\"reason\":\"missed_signal_on_restart\",\"delay_ms\":" +
                    Jn((et - scheduledEt).TotalMilliseconds));
                return;
            }
            if (!NasCoreEnabled || _nasKillHalt || _nasCoreDone || _nasFlattened ||
                et.DayOfWeek == DayOfWeek.Monday ||
                _nasHalfDays.Contains(_nasEtDay) || _nasFullHolidays.Contains(_nasEtDay) ||
                HasPosition(NasCoreLabel, NasSymbolName) ||
                TradedToday(NasCoreLabel, NasSymbolName, et.Date, false))
            {
                _nasSignalDone = true;
                return;
            }

            double opening = double.NaN;
            double closing = double.NaN;
            double high = double.MinValue;
            double low = double.MaxValue;
            bool b930 = false;
            bool b935 = false;
            bool b940 = false;
            for (int i = _nasM5.Count - 1; i >= 0 && i >= _nasM5.Count - 80; i--)
            {
                DateTime barEt = ToEastern(_nasM5.OpenTimes[i]);
                if (barEt.ToString("yyyy-MM-dd") != _nasEtDay) continue;
                if (barEt.TimeOfDay == new TimeSpan(9, 30, 0))
                {
                    opening = _nasM5.OpenPrices[i];
                    b930 = true;
                }
                else if (barEt.TimeOfDay == new TimeSpan(9, 35, 0))
                    b935 = true;
                else if (barEt.TimeOfDay == new TimeSpan(9, 40, 0))
                {
                    closing = _nasM5.ClosePrices[i];
                    b940 = true;
                }
                else
                    continue;
                high = Math.Max(high, _nasM5.HighPrices[i]);
                low = Math.Min(low, _nasM5.LowPrices[i]);
            }
            if (!(b930 && b935 && b940)) return;
            // V34.ARSENAL: publish the 09:30 ET session open for the C2 trap fade.
            _nasSessionOpen = opening;
            double range = high - low;
            if (range <= 0) { _nasSignalDone = true; return; }
            // ===== V25 FIX-3 (HIGH): minimum drive-range floor. =====
            // V24 only rejected range <= 0. Because units = risk / (0.90 * range), a compressed
            // 09:30-09:45 range (half-day, holiday-adjacent, pre-FOMC coil) drove position size
            // toward VolumeInUnitsMax: dollar RISK stayed nominally bounded but dollar-per-TICK did
            // not, implying tens-of-millions notional on a 100k account. A single adverse print then
            // moves equity a large fraction of R, which the gate's 15% execution reserve cannot cover.
            if (range < NasMinimumDriveRangePts)
            {
                Journal("SKIP", "\"label\":\"" + NasCoreLabel + "\",\"reason\":\"drive_range_below_minimum\",\"range\":" +
                    Jn(range) + ",\"minimum\":" + Jn(NasMinimumDriveRangePts));
                _nasSignalDone = true;
                return;
            }
            // ===== V33.SPARK CORE-2: drive-range CEILING, symmetric with the FIX-3 floor above. =====
            // An unusually wide 09:30-09:45 range is an exhaustion drive, not a trend initiation: the
            // 09:45 entry buys the top of a completed move and faces the reversal. Sizing is already
            // inversely proportional to range, so this is a signal-quality reject, not a size guard.
            // Latches _nasSignalDone exactly as the floor does, so the decision is evaluated once.
            if (NasMaximumDriveRangePts > 0 && range > NasMaximumDriveRangePts)
            {
                Journal("SKIP", "\"label\":\"" + NasCoreLabel + "\",\"reason\":\"drive_range_above_maximum\",\"range\":" +
                    Jn(range) + ",\"maximum\":" + Jn(NasMaximumDriveRangePts));
                _nasSignalDone = true;
                return;
            }
            double bodyFraction = Math.Abs(closing - opening) / range;
            if (bodyFraction < NasBodyFractionMin) { _nasSignalDone = true; return; }

            int direction = closing > opening ? 1 : -1;
            double gap = double.IsNaN(_nasPreviousRthClose) ? 0 : opening - _nasPreviousRthClose;
            bool aligned = !double.IsNaN(_nasPreviousRthClose) && Math.Sign(gap) == direction && Math.Abs(gap) > 0;
            double effectiveDayBudget = EffectiveNasC1DayBudget();
            double requestedRisk = effectiveDayBudget * (aligned ? NasAlignedPct : NasMisalignedPct) / 100.0;
            requestedRisk = Math.Min(requestedRisk, Math.Max(0, effectiveDayBudget - _nasDayRiskUsed));
            if (requestedRisk <= 0 || NewsBlocked()) { _nasSignalDone = true; return; }

            TradeType side = direction > 0 ? TradeType.Buy : TradeType.Sell;
            double absoluteSl = NormalizePriceToTick(_nasSymbol,
                closing - direction * NasCoreStopDriveMultiple * range);
            double absoluteTp = NormalizePriceToTick(_nasSymbol,
                closing + direction * NasCoreTargetDriveMultiple * range);
            double entryQuote = side == TradeType.Buy ? _nasSymbol.Ask : _nasSymbol.Bid;
            double stopDistance = side == TradeType.Buy ? entryQuote - absoluteSl : absoluteSl - entryQuote;
            double targetDistance = side == TradeType.Buy ? absoluteTp - entryQuote : entryQuote - absoluteTp;
            if (stopDistance <= 0 || targetDistance <= 0)
            {
                Journal("SKIP", "\"label\":\"" + NasCoreLabel + "\",\"reason\":\"invalid_absolute_protection_geometry\"");
                _nasSignalDone = true;
                return;
            }

            DateTime scheduledUtc = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(scheduledEt, DateTimeKind.Unspecified), _eastern);

            var proposal = new TradeProposal
            {
                Engine = "NAS",
                Setup = "C1",
                Label = NasCoreLabel,
                Symbol = _nasSymbol,
                Side = side,
                RequestedRisk = requestedRisk,
                SlDistancePts = stopDistance,
                TpDistancePts = targetDistance,
                Style = EntryStyle.MarketAbsoluteProtection,
                AbsoluteSl = absoluteSl,
                AbsoluteTp = absoluteTp,
                ReferenceDistancePts = range,
                DecisionTimeUtc = scheduledUtc
            };
            Journal("SIGNAL",
                "\"label\":\"" + NasCoreLabel + "\",\"config_id\":\"" + ConfigurationFingerprint() +
                "\",\"drive_range\":" + Jn(range) + ",\"body_fraction\":" + Jn(bodyFraction) +
                ",\"gap_aligned\":" + Bool(aligned) + ",\"sl_drive_multiple\":" + Jn(NasCoreStopDriveMultiple) +
                ",\"tp_drive_multiple\":" + Jn(NasCoreTargetDriveMultiple) +
                ",\"requested_risk\":" + Jn(requestedRisk) +
                ",\"decision_delay_ms\":" + Jn((et - scheduledEt).TotalMilliseconds));
            SubmitSignalWithBoundedRetry(proposal, scheduledUtc.AddSeconds(NasDecisionGraceSeconds));
        }

        private void OnNasM5BarOpened()
        {
            if (_nasM5 == null || _nasM5.Count < 2) return;
            int i = _nasM5.Count - 2;
            DateTime etOpen = ToEastern(_nasM5.OpenTimes[i]);
            DateTime etClose = etOpen.AddMinutes(5);
            if (etClose.ToString("yyyy-MM-dd") == _nasEtDay &&
                etClose.TimeOfDay >= new TimeSpan(9, 35, 0) &&
                etClose.TimeOfDay <= new TimeSpan(16, 0, 0))
                _nasTodayLastRthClose = _nasM5.ClosePrices[i];

            // V34.ARSENAL: capture the 09:30 ET session open from the bar itself, so the C2
            // trap-fade reference exists even on a day where C1 never reached its own signal
            // evaluation (for example a spread or news reject before the geometry was built).
            if (etOpen.ToString("yyyy-MM-dd") == _nasEtDay &&
                etOpen.TimeOfDay == new TimeSpan(9, 30, 0))
                _nasSessionOpen = _nasM5.OpenPrices[i];

            if (_deploymentArmed && _nasPendingReentry && !_nasReDone && !_nasKillHalt && !_nasFlattened)
                ExecuteNasTrapFade(_nasM5.OpenTimes[_nasM5.Count - 1]);
        }

        // ===== V34.ARSENAL: NAS C2 — 10:00 ET FAILED-DRIVE LIQUIDITY TRAP FADE =====
        // The inherited C2 fired on the next M5 bar after ANY C1 stop-out up to 14:00 ET, sized
        // its stop at the whole opening drive range and carried NO take-profit at all. It was a
        // re-entry, not a thesis, and it re-armed into exactly the regime that had just paid to
        // stop C1 out. The re-engineered C2 is an explicit institutional trap fade that fires
        // only when every one of the following holds:
        //   1. C1 was stopped out at or before 10:20 ET (armed in OnPositionClosed).
        //   2. The clock is inside the 10:00-10:20 ET liquidity-sweep window.
        //   3. The last CLOSED M5 bar swept THROUGH the 09:30 ET session open against the
        //      failed drive and closed beyond it, so the opening breakout has demonstrably
        //      failed rather than merely paused.
        //   4. That same bar closed with a rejection body of at least NasTrapBodyFractionMin of
        //      its range in the fade direction.
        // Direction is the OPPOSITE of the stopped C1 leg, which _nasPendingReDirection already
        // carries. Geometry is 0.60x the drive range for the stop and 3.00x for the target.
        private void ExecuteNasTrapFade(DateTime decisionUtc)
        {
            if (!_deploymentArmed) return;
            if (_signalRetries.ContainsKey(NasReLabel)) return;
            if (!NasReentryEnabled || IsFunded() || _nasDriveRange <= 0 || NewsBlocked())
            {
                _nasPendingReentry = false;
                _nasReDone = true;
                return;
            }
            DateTime et = ToEastern(Server.TimeInUtc);
            if (HasPosition(NasCoreLabel, NasSymbolName) || HasPosition(NasReLabel, NasSymbolName) ||
                HasPending(NasCoreLabel, NasSymbolName) || HasPending(NasReLabel, NasSymbolName) ||
                TradedToday(NasReLabel, NasSymbolName, et.Date, false))
            {
                _nasPendingReentry = false;
                _nasReDone = true;
                return;
            }
            // Before the window: wait. After it: consume the arm so it cannot leak into the
            // afternoon the way the inherited re-entry could.
            if (et.TimeOfDay < NasTrapWindowStart) return;
            if (et.TimeOfDay > NasTrapWindowEnd)
            {
                _nasPendingReentry = false;
                _nasReDone = true;
                Journal("SKIP", "\"label\":\"" + NasReLabel +
                    "\",\"reason\":\"nas_trap_window_expired\"");
                return;
            }
            int fadeDirection = _nasPendingReDirection > 0 ? 1 : -1;
            string confirmation;
            if (!NasTrapSweepConfirmed(fadeDirection, out confirmation)) return;

            // Both terms are pre-scale requests, matching how _nasDayRiskUsed accumulates. The
            // day budget term now uses the same effective budget C1 was sized from, so the trap
            // fade genuinely draws the residual rather than an unrelated static parameter.
            double risk = Math.Min(EffectiveNasReentryRisk(),
                Math.Max(0, EffectiveNasC1DayBudget() - _nasDayRiskUsed));
            if (risk <= 0) { _nasPendingReentry = false; _nasReDone = true; return; }

            double stopDistance = _nasDriveRange * NasTrapStopDriveMultiple;
            double targetDistance = _nasDriveRange * NasTrapTargetDriveMultiple;
            if (stopDistance <= 0 || targetDistance <= 0)
            {
                _nasPendingReentry = false;
                _nasReDone = true;
                Journal("SKIP", "\"label\":\"" + NasReLabel +
                    "\",\"reason\":\"nas_trap_invalid_geometry\"");
                return;
            }

            var proposal = new TradeProposal
            {
                Engine = "NAS",
                Setup = "C2",
                Label = NasReLabel,
                Symbol = _nasSymbol,
                Side = fadeDirection > 0 ? TradeType.Buy : TradeType.Sell,
                RequestedRisk = risk,
                SlDistancePts = stopDistance,
                TpDistancePts = targetDistance,
                Style = EntryStyle.MarketWithPips,
                ReferenceDistancePts = _nasDriveRange,
                DecisionTimeUtc = decisionUtc
            };
            Journal("SIGNAL",
                "\"label\":\"" + NasReLabel + "\",\"kind\":\"nas_trap_fade\"" +
                ",\"fade_direction\":" + fadeDirection +
                ",\"session_open\":" + Jn(_nasSessionOpen) +
                ",\"drive_range\":" + Jn(_nasDriveRange) +
                ",\"sl_dist\":" + Jn(stopDistance) + ",\"tp_dist\":" + Jn(targetDistance) +
                ",\"confirmation\":\"" + Js(confirmation) + "\"");
            SubmitSignalWithBoundedRetry(proposal, Server.TimeInUtc.AddSeconds(30));
        }

        // The last CLOSED M5 bar must sweep through the 09:30 ET open in the fade direction and
        // close beyond it with a rejection body. Returns false WITHOUT consuming the armed fade
        // whenever the confirmation has simply not printed yet inside the window.
        private bool NasTrapSweepConfirmed(int fadeDirection, out string detail)
        {
            detail = "";
            if (_nasM5 == null || _nasM5.Count < 2) return false;
            if (double.IsNaN(_nasSessionOpen) || _nasSessionOpen <= 0) return false;
            int i = _nasM5.Count - 2;
            DateTime barEt = ToEastern(_nasM5.OpenTimes[i]);
            if (barEt.ToString("yyyy-MM-dd") != _nasEtDay) return false;
            if (barEt.TimeOfDay < NasTrapWindowStart.Subtract(TimeSpan.FromMinutes(5)) ||
                barEt.TimeOfDay > NasTrapWindowEnd) return false;

            double barOpen = _nasM5.OpenPrices[i];
            double barHigh = _nasM5.HighPrices[i];
            double barLow = _nasM5.LowPrices[i];
            double barClose = _nasM5.ClosePrices[i];
            double barRange = barHigh - barLow;
            if (barRange <= 0) return false;

            bool sweptOpen = fadeDirection > 0
                ? barHigh > _nasSessionOpen && barClose > _nasSessionOpen
                : barLow < _nasSessionOpen && barClose < _nasSessionOpen;
            if (!sweptOpen) return false;

            double signedBody = (barClose - barOpen) * fadeDirection;
            if (signedBody <= 0 || signedBody / barRange < NasTrapBodyFractionMin) return false;

            detail = "swept_session_open_with_rejection_body";
            return true;
        }

        private void RunNasTimeChecks()
        {
            if (_nasSymbol == null) return;
            NasRolloverIfNeeded();
            DateTime et = ToEastern(Server.TimeInUtc);
            bool halfDay = _nasHalfDays.Contains(_nasEtDay);
            TimeSpan flatten = halfDay ? new TimeSpan(12, 55, 0) : new TimeSpan(15, 30, 0);

            // V34.ARSENAL: an armed trap fade that never received its sweep confirmation dies
            // with the 10:20 ET window. RunNasTimeChecks runs at 1 Hz and on every US100 tick,
            // so this is the reliable expiry path even if no further M5 bar callback arrives.
            if (_nasPendingReentry && !_nasReDone && et.TimeOfDay > NasTrapWindowEnd)
            {
                _nasPendingReentry = false;
                _nasReDone = true;
                Journal("SKIP", "\"label\":\"" + NasReLabel +
                    "\",\"reason\":\"nas_trap_window_expired\"");
            }

            // ===== V33.SPARK CORE-2: multi-tier laggard cut. =====
            // Tier 1 at 11:30 ET cuts the clearly-failing trade an hour earlier than the inherited
            // Tier 2, on a looser -0.50R threshold; Tier 2 at 12:30 ET keeps its original -0.30R.
            // Both are one-shot sweeps latched per ET day, matching the inherited Tier-2 pattern, so
            // a position that survives a sweep is not re-evaluated every timer tick.
            // The Tier-1 window is closed at 12:30 so a bot started after 12:30 goes straight to the
            // stricter Tier 2 rather than applying the looser threshold late.
            // R is measured exactly as Tier 2 already measured it: signed price progress over the
            // drive-derived reference distance. Positions without usable metadata are skipped, never
            // closed on a guessed denominator.
            if (!_nasEarlyLaggardDone && et.TimeOfDay >= new TimeSpan(11, 30, 0) &&
                et.TimeOfDay < new TimeSpan(12, 30, 0))
            {
                _nasEarlyLaggardDone = true;
                foreach (var p in Positions.Where(x => x.SymbolName == NasSymbolName && x.Label == NasCoreLabel).ToArray())
                {
                    PositionMeta meta;
                    if (!_positionMeta.TryGetValue(p.Id, out meta) || meta.ReferenceDistancePts <= 0) continue;
                    double current = p.TradeType == TradeType.Buy ? _nasSymbol.Bid : _nasSymbol.Ask;
                    double unrealisedR = meta.Direction * (current - p.EntryPrice) / meta.ReferenceDistancePts;
                    if (unrealisedR < -0.50) EmergencyClose(p, "nas_early_laggard_cut_1130");
                }
            }

            if (!_nasLaggardDone && et.TimeOfDay >= new TimeSpan(12, 30, 0))
            {
                _nasLaggardDone = true;
                foreach (var p in Positions.Where(x => x.SymbolName == NasSymbolName && x.Label == NasCoreLabel).ToArray())
                {
                    PositionMeta meta;
                    if (!_positionMeta.TryGetValue(p.Id, out meta) || meta.ReferenceDistancePts <= 0) continue;
                    double current = p.TradeType == TradeType.Buy ? _nasSymbol.Bid : _nasSymbol.Ask;
                    double unrealisedR = meta.Direction * (current - p.EntryPrice) / meta.ReferenceDistancePts;
                    if (unrealisedR < -0.30) EmergencyClose(p, "nas_laggard_cut_1230");
                }
            }

            if (et.TimeOfDay >= flatten)
            {
                CloseLabels(new[] { NasCoreLabel, NasReLabel }, NasSymbolName, "nas_time_flatten");
                _nasFlattened = !HasPosition(NasCoreLabel, NasSymbolName) && !HasPosition(NasReLabel, NasSymbolName);
            }
        }

        private bool NewsBlocked()
        {
            // V25 FIX-2: was "!FundedNewsGuard || !IsFunded() || ..." — the IsFunded() gate disabled
            // news protection on Challenge/evaluation accounts. Now active in every armed stage.
            if (!FundedNewsGuard || Stage == StageMode.Unconfigured || _restrictedEvents.Count == 0) return false;
            DateTime now = Server.TimeInUtc;
            foreach (var ev in _restrictedEvents)
            {
                if (now < ev.AddMinutes(-NewsPreFlatLeadMinutes)) break;
                if (now <= ev.AddMinutes(2)) return true;
            }
            return false;
        }

        private void EnforceRestrictedNewsFlatBook()
        {
            // V25 FIX-2: same de-gating as NewsBlocked() — flat-book enforcement now runs on Challenge.
            if (!FundedNewsGuard || Stage == StageMode.Unconfigured || _restrictedEvents.Count == 0) return;
            DateTime now = Server.TimeInUtc;
            foreach (DateTime ev in _restrictedEvents)
            {
                DateTime preFlat = ev.AddMinutes(-NewsPreFlatLeadMinutes);
                DateTime restrictedStart = ev.AddMinutes(-2);
                if (now < preFlat) break;
                if (now > ev.AddMinutes(2)) continue;

                string key = ev.ToString("o");
                if (now < restrictedStart)
                {
                    if (_newsPreFlatDone.Add(key))
                    {
                        Journal("NEWS_SAFETY",
                            "\"event_utc\":\"" + key + "\",\"action\":\"pre_flat\",\"lead_minutes\":" + NewsPreFlatLeadMinutes);
                        CancelAllMasterPending();
                        CloseAllMasterPositions("restricted_news_pre_flat");
                    }
                    return;
                }

                if (HasAnyMasterExposure() && !_startupHalt)
                {
                    Journal("GUARD", "\"kind\":\"restricted_news_window_exposure\",\"event_utc\":\"" + key + "\"");
                    HaltStartup("master exposure remains inside a restricted-news execution window");
                }
                return;
            }
        }

        private double FindPriorNasRthClose()
        {
            if (_nasM5 == null) return double.NaN;
            string today = ToEastern(Server.TimeInUtc).ToString("yyyy-MM-dd");
            string targetDate = null;
            for (int i = _nasM5.Count - 1; i >= 0; i--)
            {
                DateTime etClose = ToEastern(_nasM5.OpenTimes[i]).AddMinutes(5);
                string day = etClose.ToString("yyyy-MM-dd");
                if (day == today) continue;
                if (targetDate == null) targetDate = day;
                if (day != targetDate) break;
                if (etClose.TimeOfDay >= new TimeSpan(9, 35, 0) && etClose.TimeOfDay <= new TimeSpan(16, 0, 0))
                    return _nasM5.ClosePrices[i];
            }
            return double.NaN;
        }

        private double FindNasRthClose(string etDay)
        {
            if (_nasM5 == null || string.IsNullOrEmpty(etDay)) return double.NaN;
            for (int i = _nasM5.Count - 1; i >= 0; i--)
            {
                DateTime etClose = ToEastern(_nasM5.OpenTimes[i]).AddMinutes(5);
                string day = etClose.ToString("yyyy-MM-dd");
                if (day != etDay)
                {
                    if (string.CompareOrdinal(day, etDay) < 0) break;
                    continue;
                }
                if (etClose.TimeOfDay >= new TimeSpan(9, 35, 0) &&
                    etClose.TimeOfDay <= new TimeSpan(16, 0, 0))
                    return _nasM5.ClosePrices[i];
            }
            return double.NaN;
        }

        // =========================================================================================
        // ORDER / POSITION EVENTS
        // =========================================================================================
        private void OnPendingFilled(PendingOrderFilledEventArgs args)
        {
            InvalidateOneClickExecutionRearmReview();
            if (args == null || args.PendingOrder == null)
            {
                RecordExecutionIncident("pending_fill_event_missing_order", 0, 0);
                HaltStartup("pending fill event did not identify its order");
                return;
            }
            if (!_processedPendingFillIds.Add(args.PendingOrder.Id)) return;
            if (ConsumeDirectLimitTerminalReplay(args)) return;

            bool terminalPendingClaimed = TryPromotePendingFromActiveMutation(
                args.PendingOrder, "pending_filled_callback");
            ActiveEntryMutation activeMutation = _activeEntryMutation;
            bool exactActivePendingEvent = terminalPendingClaimed &&
                PendingCoreMatchesMutation(args.PendingOrder, activeMutation) &&
                !HasExplicitContradictoryCurrentAuth(
                    args.PendingOrder.Comment, activeMutation.ExpectedComment);
            if (exactActivePendingEvent)
                activeMutation.PendingFillCallbackSeen = true;
            RecordPendingCommentDiagnostic(args.PendingOrder, "pending_filled_callback");
            bool ownedPending = CanManagePending(args.PendingOrder);
            if (!ownedPending)
            {
                // A shape-matched event with contradictory metadata is only an unresolved in-flight
                // candidate. Defer classification until the broker call returns an authoritative ID;
                // never mutate the order or resulting position from this candidate alone.
                if (exactActivePendingEvent)
                {
                    Journal("OWNERSHIP_DEFERRED",
                        "\"kind\":\"pending_fill_candidate\",\"pending_id\":" + args.PendingOrder.Id +
                        ",\"label\":\"" + Js(args.PendingOrder.Label) + "\"");
                    return;
                }
                if (IsMasterLabel(args.PendingOrder.Label))
                {
                    RecordExecutionIncident("foreign_or_nonowned_atlas_pending_fill", 0, 0);
                    CancelAllMasterPending();
                    HaltStartup("foreign/non-owned Atlas pending fill observed; resulting exposure is preserved for manual review");
                }
                return;
            }
            bool cancelRequested = _cancelRequested.Remove(args.PendingOrder.Id);
            _cancelRetry.Remove(args.PendingOrder.Id);
            _cancelRetryNotBeforeTicks.Remove(args.PendingOrder.Id);
            _cancelRequestMonotonicTicks.Remove(args.PendingOrder.Id);
            PendingInfo info;
            if (!_pendingInfo.TryGetValue(args.PendingOrder.Id, out info))
            {
                if (TryRehydratePendingInfoFromLedger(args.PendingOrder, out info))
                {
                    _pendingInfo[args.PendingOrder.Id] = info;
                    Journal("OWNERSHIP_RECONCILED",
                        "\"kind\":\"pending_restart_fill\",\"pending_id\":" + args.PendingOrder.Id +
                        ",\"position_id\":" + (args.Position == null ? 0 : args.Position.Id));
                }
            }
            if (info == null)
            {
                bool failClosedOrphan = cancelRequested || EntryHaltActive();
                _currentRunCreatedPendingIds.Remove(args.PendingOrder.Id);
                _expectedPendingComments.Remove(args.PendingOrder.Id);
                RecordExecutionIncident("orphan_pending_fill", 0, 0);
                if (args.Position != null && IsMasterLabel(args.Position.Label))
                {
                    Journal("GUARD",
                        "\"kind\":\"orphan_pending_fill\",\"order_id\":" + args.PendingOrder.Id +
                        ",\"position_id\":" + args.Position.Id + ",\"label\":\"" + Js(args.Position.Label) + "\"");
                    RebuildPositionMetadata(args.Position);
                    string protectionReason;
                    if (failClosedOrphan)
                        EmergencyClose(args.Position, cancelRequested ? "late_fill_after_cancel" : "late_fill_during_halt");
                    else if (!ValidPositionProtection(args.Position, out protectionReason))
                    {
                        RecordExecutionIncident("orphan_fill_invalid_protection_" + protectionReason, 0, 0);
                        EmergencyClose(args.Position, "orphan_fill_" + protectionReason);
                    }
                    HaltStartup("unreconciled Atlas3 pending fill detected");
                }
                else
                    HaltStartup("owned pending fill event had no reconcilable Atlas3 position");
                return;
            }
            if (args.Position == null)
            {
                _pendingInfo.Remove(args.PendingOrder.Id);
                _currentRunCreatedPendingIds.Remove(args.PendingOrder.Id);
                _expectedPendingComments.Remove(args.PendingOrder.Id);
                RecordExecutionIncident("pending_fill_without_position", 0, 0);
                HaltStartup("owned pending fill event returned no position");
                return;
            }
            // Promotion occurs only here, where the callback supplies the exact pending order ID,
            // stored metadata and a newly opened matching position.
            if (!RegisterPositionFromPendingFill(args.Position, args.PendingOrder, info))
            {
                _pendingInfo.Remove(args.PendingOrder.Id);
                _currentRunCreatedPendingIds.Remove(args.PendingOrder.Id);
                _expectedPendingComments.Remove(args.PendingOrder.Id);
                RecordExecutionIncident("pending_fill_position_mismatch", 0, 0);
                HaltStartup("pending fill position did not exactly match the current-run pending entry; exposure preserved");
                return;
            }
            Symbol fillSymbol = Symbols.GetSymbol(args.Position.SymbolName);
            double fillStep = fillSymbol == null ? 1.0 :
                Math.Max(fillSymbol.VolumeInUnitsStep, 1.0);
            if (args.Position.VolumeInUnits + fillStep * 0.5 < info.Units)
                RecordExecutionIncident("partial_fill", 0, 0);
            bool haltBeforeFillQuality = EntryHaltActive();
            _pendingInfo.Remove(args.PendingOrder.Id);
            _currentRunCreatedPendingIds.Remove(args.PendingOrder.Id);
            _expectedPendingComments.Remove(args.PendingOrder.Id);
            RecordExecutionQuality(info.Proposal, info.DecisionPrice, args.Position.EntryPrice, 0, false);
            bool haltAfterFillQuality = EntryHaltActive();
            bool failClosed = cancelRequested || haltBeforeFillQuality || haltAfterFillQuality;
            string fillProtectionReason;
            if (!ValidPositionProtection(args.Position, out fillProtectionReason))
            {
                RecordExecutionIncident("pending_fill_invalid_protection_" + fillProtectionReason, 0, 0);
                EmergencyClose(args.Position, "pending_fill_" + fillProtectionReason);
                HaltStartup("owned pending fill arrived without valid broker protection");
                return;
            }
            if (failClosed)
            {
                RecordExecutionIncident(cancelRequested
                    ? "late_fill_after_cancel"
                    : "late_fill_during_global_halt", 0, 0);
                Journal("GUARD",
                    "\"kind\":\"late_pending_fill\",\"order_id\":" + args.PendingOrder.Id +
                    ",\"position_id\":" + args.Position.Id + ",\"label\":\"" + Js(args.Position.Label) +
                    "\",\"cancel_requested\":" + Bool(cancelRequested) +
                    ",\"day_halt\":" + Bool(_dayHalt) + ",\"startup_halt\":" + Bool(_startupHalt) +
                    ",\"objective_closing\":" + Bool(_objectiveClosing) +
                    ",\"objective_locked\":" + Bool(_objectiveLocked) +
                    ",\"funded_payout_closing\":" + Bool(_fundedPayoutClosing) +
                    ",\"funded_payout_locked\":" + Bool(_fundedPayoutLocked));
                EmergencyClose(args.Position, cancelRequested ? "late_fill_after_cancel" : "late_fill_during_halt");
                return;
            }
            Journal("ENTRY",
                "\"label\":\"" + Js(info.Proposal.Label) + "\",\"engine\":\"" + info.Proposal.Engine +
                "\",\"setup\":\"" + info.Proposal.Setup + "\",\"type\":\"limit\",\"order_id\":" + args.PendingOrder.Id +
                ",\"position_id\":" + args.Position.Id + ",\"fill\":" + Jn(args.Position.EntryPrice) +
                ",\"approved_risk\":" + Jn(info.ApprovedRisk) +
                ",\"dynamic_tier\":\"" + Js(_currentDynamicRiskTier) + "\"" +
                ",\"dynamic_scale\":" + Jn(_currentDynamicRiskScale) +
                ",\"initial_risk\":" + Jn(_positionMeta[args.Position.Id].InitialRisk) +
                ",\"units\":" + Jn(args.Position.VolumeInUnits));
            if (activeMutation != null && activeMutation.PendingOrderId.HasValue &&
                activeMutation.PendingOrderId.Value == args.PendingOrder.Id &&
                activeMutation.PositionId.HasValue && activeMutation.PositionId.Value == args.Position.Id)
            {
                activeMutation.EntryJournalWritten = true;
                activeMutation.PendingFillHandledSuccessfully = true;
            }
        }

        private void OnPendingCancelled(PendingOrderCancelledEventArgs args)
        {
            InvalidateOneClickExecutionRearmReview();
            if (args == null || args.PendingOrder == null) return;
            if (!_processedPendingCancelIds.Add(args.PendingOrder.Id)) return;
            bool terminalPendingClaimed = TryPromotePendingFromActiveMutation(
                args.PendingOrder, "pending_cancelled_callback");
            RecordPendingCommentDiagnostic(args.PendingOrder, "pending_cancelled_callback");
            if (terminalPendingClaimed &&
                PendingCoreMatchesMutation(args.PendingOrder, _activeEntryMutation) &&
                !HasExplicitContradictoryCurrentAuth(
                    args.PendingOrder.Comment, _activeEntryMutation.ExpectedComment))
                _activeEntryMutation.PendingCancelledSeen = true;
            _cancelRequested.Remove(args.PendingOrder.Id);
            _cancelRetry.Remove(args.PendingOrder.Id);
            _cancelRetryNotBeforeTicks.Remove(args.PendingOrder.Id);
            _cancelRequestMonotonicTicks.Remove(args.PendingOrder.Id);
            _currentRunCreatedPendingIds.Remove(args.PendingOrder.Id);
            _expectedPendingComments.Remove(args.PendingOrder.Id);
            if (!RemoveOwnershipLedgerRecord("PENDING", args.PendingOrder.Id))
                RecordExecutionIncident("ownership_ledger_pending_terminal_remove_failed", 0, 0);
            PendingInfo info;
            if (!_pendingInfo.TryGetValue(args.PendingOrder.Id, out info)) return;
            _pendingInfo.Remove(args.PendingOrder.Id);
            Journal("SKIP",
                "\"label\":\"" + Js(info.Proposal.Label) + "\",\"order_id\":" + args.PendingOrder.Id +
                ",\"reason\":\"pending_cancelled_" + args.Reason + "\"");
        }

        private List<string> GetGlobalEntryBlockers()
        {
            var blockers = new List<string>();
            if (RunningMode == RunningMode.RealTime &&
                (!_bindingValid || !RuntimeIdentityMatchesReceipt()))
                blockers.Add("deployment_binding_not_authorized_or_identity_changed");
            if (!_accountLeaseOwned) blockers.Add("account_lease_not_owned");
            if (RunningMode == RunningMode.RealTime && _bindingValid && RuntimeIdentityMatchesReceipt() &&
                _accountLeaseOwned && !_managementAuthority)
                blockers.Add("deployment_management_authority_not_established");
            if (RunningMode == RunningMode.RealTime && _bindingValid &&
                (_ownershipLedgerInvalid || (_managementAuthority && !_ownershipLedgerLoaded)))
                blockers.Add("ownership_ledger_not_valid_" + SafeStateToken(_ownershipLedgerStatus));
            if (_stateRebuildMode) blockers.Add("startup_state_rebuild");
            if (_startupHalt) blockers.Add("startup_halt_" + _haltReason);
            if (RunningMode == RunningMode.RealTime && JournalOn && _journalDead) blockers.Add("journal_failed");
            if (!_dataReady) blockers.Add("data_not_ready");
            if (_dayHalt) blockers.Add("account_day_halt");
            if (_objectiveLocked) blockers.Add("objective_locked");
            if (_objectiveClosing) blockers.Add("objective_close_in_progress");
            if (_fundedPayoutLocked || _fundedPayoutClosing || FundedPayoutCloseCondition())
                blockers.Add(_fundedPayoutLocked ? "funded_payout_locked" : "funded_payout_close_in_progress");
            if (_executionGuardState == ExecutionGuardState.LatencyCooldown)
                blockers.Add("execution_latency_cooldown_until_" + StateDateText(_latencyCooldownUntilUtc));
            else if (_executionGuardState == ExecutionGuardState.HardHalt)
                blockers.Add("execution_quality_hard_halt_manual_rearm_required_" + _executionGuardReason);
            if (NewsBlocked()) blockers.Add("restricted_news_window");
            if (ExclusiveAccount &&
                (Positions.Any(x => !IsRuntimeRecognizedPosition(x)) ||
                 PendingOrders.Any(x => !IsRuntimeRecognizedPending(x))))
                blockers.Add("external_or_non_owned_account_exposure");
            // Keep an unexplained lifecycle inconsistency fail-closed, but do not mislabel a known
            // data/news/execution blocker as a receipt or identity failure.
            if (RunningMode == RunningMode.RealTime && blockers.Count == 0)
            {
                if (_deploymentState != DeploymentState.Running)
                    blockers.Add("deployment_not_running_" + _deploymentState.ToString().ToLowerInvariant());
                else if (!_deploymentArmed)
                    blockers.Add("deployment_internal_arm_not_set");
            }
            return blockers;
        }

        // Lifecycle enforcement belongs here, not in DrawMasterHud. Rendering stays read-only even
        // when the operator hides the HUD; READY -> BLOCKED still cancels exact-owned pending entry.
        private void EnforceReadyStateTransition()
        {
            TrackReadyState(GetGlobalEntryBlockers());
        }

        private void TrackReadyState(List<string> blockers)
        {
            string signature = blockers == null || blockers.Count == 0
                ? "YES"
                : "NO|" + string.Join("|", blockers);
            if (string.Equals(signature, _lastReadySignature, StringComparison.Ordinal)) return;
            bool transitionedToBlocked = signature.StartsWith("NO|", StringComparison.Ordinal) &&
                string.Equals(_lastReadySignature, "YES", StringComparison.Ordinal);
            _lastReadySignature = signature;
            if (transitionedToBlocked && BindingAuthorityCurrent() && PendingOrders.Any(IsPersistentOrCurrentPending))
            {
                Print("ATLAS GLOBAL BLOCK TRANSITION: cancelling exact-owned pending entries.");
                CancelAllMasterPending();
            }
            Print("ATLAS GLOBAL SAFETY READY | {0}", signature);
            Journal("GLOBAL_SAFETY_READY_CHANGED", "\"ready\":" + Bool(signature == "YES") +
                ",\"blockers\":\"" + Js(blockers == null ? "" : string.Join(" || ", blockers)) + "\"");
        }

        private bool EntryHaltActive()
        {
            return GetGlobalEntryBlockers().Count > 0;
        }

        private bool EntryHaltRequiresClosingOpenedPosition(Position position)
        {
            List<string> blockers = GetGlobalEntryBlockers();
            if (position == null || !_requestAuthorizedImmediateFillPositionIds.Contains(position.Id) ||
                _executionGuardState != ExecutionGuardState.LatencyCooldown)
                return blockers.Count > 0;

            blockers.RemoveAll(x => x.StartsWith("execution_latency_cooldown_until_", StringComparison.Ordinal) ||
                x == "deployment_not_running_blocked" ||
                x == "deployment_internal_arm_not_set");
            return blockers.Count > 0;
        }

        private void OnPositionOpened(PositionOpenedEventArgs args)
        {
            InvalidateOneClickExecutionRearmReview();
            var p = args.Position;
            if (p == null) return;
            if (!_processedPositionOpenedIds.Add(p.Id)) return;
            if (!IsMasterLabel(p.Label))
            {
                if (ExclusiveAccount)
                {
                    CancelAllMasterPending();
                    HaltStartup("external/manual position opened; owned pending orders were cancelled and new entries are blocked");
                }
                return;
            }
            // A broker can deliver PositionOpened after a synchronous safety close has already
            // been requested. The exact ID is already inside the close state machine; do not add a
            // second foreign-exposure incident or issue a duplicate close.
            if (_closeRequestMeta.ContainsKey(p.Id) || _closeRetry.Contains(p.Id))
            {
                Journal("BROKER_CALLBACK_DIAGNOSTIC",
                    "\"kind\":\"delayed_position_opened_after_close_request\",\"position_id\":" + p.Id +
                    ",\"label\":\"" + Js(p.Label) + "\"");
                return;
            }
            if (!IsPersistentOrCurrentPosition(p))
            {
                // Candidate recognition only defers foreign classification. It deliberately grants
                // no management authority; the successful TradeResult or exact PendingFilled event
                // must promote the ID before this instance may mutate the position.
                if (PositionCoreMatchesMutation(p, _activeEntryMutation) ||
                    IsKnownPendingFillCandidate(p))
                {
                    ActiveEntryMutation mutation = _activeEntryMutation;
                    if (PositionCoreMatchesMutation(p, mutation))
                    {
                        if (mutation.PositionId.HasValue && mutation.PositionId.Value != p.Id)
                        {
                            RecordExecutionIncident("multiple_position_candidates_for_active_mutation", 0, 0);
                            HaltStartup("multiple position callbacks matched one active entry mutation; exposure preserved");
                            return;
                        }
                        mutation.PositionId = p.Id;
                    }
                    Journal("OWNERSHIP_DEFERRED",
                        "\"kind\":\"position_opened_candidate\",\"position_id\":" + p.Id +
                        ",\"label\":\"" + Js(p.Label) + "\"");
                    return;
                }
                RecordExecutionIncident("foreign_or_nonowned_atlas_position_opened", 0, 0);
                CancelAllMasterPending();
                HaltStartup("foreign/non-owned Atlas position opened; V34.ARSENAL preserved it and blocked new entries");
                return;
            }
            RecordPositionCommentDiagnostic(p, "position_opened_callback");
            if (!CanManagePosition(p))
            {
                RecordExecutionIncident("foreign_or_nonowned_atlas_position_opened", 0, 0);
                CancelAllMasterPending();
                HaltStartup("foreign/non-owned Atlas position opened; V34.ARSENAL preserved it and blocked new entries");
                return;
            }
            bool closesForEntryHalt = EntryHaltRequiresClosingOpenedPosition(p);
            if (closesForEntryHalt)
            {
                Journal("GUARD",
                    "\"kind\":\"position_opened_during_entry_halt\",\"position_id\":" + p.Id +
                    ",\"label\":\"" + Js(p.Label) + "\"");
                RecordExecutionIncident("position_opened_during_entry_halt", 0, 0);
                EmergencyClose(p, "position_opened_during_entry_halt");
                return;
            }
            if (_requestAuthorizedImmediateFillPositionIds.Contains(p.Id) &&
                _executionGuardState == ExecutionGuardState.LatencyCooldown)
                Journal("GUARD",
                    "\"kind\":\"causative_fill_preserved_during_latency_cooldown\",\"position_id\":" +
                    p.Id + ",\"label\":\"" + Js(p.Label) + "\"");
            string protectionReason;
            if (ValidPositionProtection(p, out protectionReason)) return;

            Journal("GUARD",
                "\"kind\":\"invalid_position_protection\",\"position_id\":" + p.Id +
                ",\"label\":\"" + Js(p.Label) + "\",\"reason\":\"" + Js(protectionReason) + "\"");
            RecordExecutionIncident("opened_position_invalid_protection_" + protectionReason, 0, 0);
            EmergencyClose(p, "position_protection_" + protectionReason);
        }

        private void OnPositionClosed(PositionClosedEventArgs args)
        {
            InvalidateOneClickExecutionRearmReview();
            var p = args.Position;
            MarkDirectLimitTerminalPositionClosed(p);
            if (!IsMasterLabel(p.Label)) return;
            if (!IsPersistentOrCurrentPosition(p)) return;
            PositionMeta meta;
            _positionMeta.TryGetValue(p.Id, out meta);
            double initialRisk = meta != null ? meta.InitialRisk : ParseCommentNumber(p.Comment, "r=");
            double r = initialRisk > 0 ? p.NetProfit / initialRisk : 0;
            double mfeR = meta != null && meta.StopRiskDistancePts > 0
                ? meta.Direction * (meta.MfePrice - p.EntryPrice) / meta.StopRiskDistancePts
                : 0;
            double maeR = meta != null && meta.StopRiskDistancePts > 0
                ? meta.Direction * (meta.MaePrice - p.EntryPrice) / meta.StopRiskDistancePts
                : 0;
            CloseRequestMeta managedClose;
            string managedReason = _closeRequestMeta.TryGetValue(p.Id, out managedClose) ? managedClose.Reason : "";
            RecordExitExecutionQuality(p, args.Reason, meta);
            Journal("EXIT",
                "\"label\":\"" + Js(p.Label) + "\",\"position_id\":" + p.Id +
                ",\"reason\":\"" + args.Reason + "\",\"net\":" + Jn(p.NetProfit) +
                ",\"r\":" + Jn(r) + ",\"mfe_r\":" + Jn(mfeR) + ",\"mae_r\":" + Jn(maeR) +
                ",\"managed_reason\":\"" + Js(managedReason) + "\"" +
                ",\"managed_close_attempts\":" + (managedClose != null ? managedClose.Attempts : 0) +
                ",\"balance\":" + Jn(Account.Balance));

            if (p.Label == DaxALabel || p.Label == DaxBLabel || p.Label == DaxCLabel)
            {
                if (p.NetProfit > 0) _daxConsecutiveLosses = 0;
                else
                {
                    _daxConsecutiveLosses++;
                    if (_daxConsecutiveLosses >= 3) _daxDayBlocked = true;
                }
                if (p.Label == DaxALabel || p.Label == DaxBLabel) PushDaxOutcome(p.NetProfit > 0);
            }

            if (p.Label == UkALabel)
            {
                _ukRollingR.Enqueue(r);
                while (_ukRollingR.Count > 60) _ukRollingR.Dequeue();
                if (_ukRollingR.Count == 60 && _ukRollingR.Sum() < -20)
                {
                    bool firstAlert = !_ukKillAlert;
                    _ukKillAlert = true;
                    _ukKillHalt = ModuleHealthAction == HealthGuardMode.LegacyPermanent;
                    if (firstAlert)
                        Journal("HEALTH_STATE", "\"module\":\"UK\",\"state\":\"ALERT\",\"rolling_r\":" +
                            Jn(_ukRollingR.Sum()) + ",\"action\":\"" + ModuleHealthAction + "\"");
                }
            }

            if (p.Label == NasCoreLabel || p.Label == NasReLabel)
            {
                _nasLiveCloses++;
                _nasKillQueue.Enqueue(r);
                while (_nasKillQueue.Count > 40) _nasKillQueue.Dequeue();
                if (_nasLiveCloses >= 10 && _nasKillQueue.Sum() < -12)
                {
                    bool firstAlert = !_nasKillAlert;
                    _nasKillAlert = true;
                    _nasKillHalt = ModuleHealthAction == HealthGuardMode.LegacyPermanent;
                    if (firstAlert)
                        Journal("HEALTH_STATE", "\"module\":\"NAS\",\"state\":\"ALERT\",\"rolling_r\":" +
                            Jn(_nasKillQueue.Sum()) + ",\"action\":\"" + ModuleHealthAction + "\"");
                }

                if (p.Label == NasCoreLabel && args.Reason == PositionCloseReason.StopLoss &&
                    NasReentryEnabled && !IsFunded() && !_nasReDone && !_nasKillHalt)
                {
                    // V34.ARSENAL: the trap fade only exists inside the 10:00-10:20 ET liquidity
                    // window, so a stop-out after 10:20 can no longer arm it. The inherited
                    // 14:00 ET arm window belonged to the old unconditional re-entry.
                    DateTime et = ToEastern(Server.TimeInUtc);
                    if (et.TimeOfDay <= NasTrapWindowEnd)
                    {
                        _nasPendingReentry = true;
                        int direction = meta != null ? meta.Direction : (p.TradeType == TradeType.Buy ? 1 : -1);
                        _nasPendingReDirection = -direction;
                    }
                }
            }

            // V34.ARSENAL: gold rolling-R health, mirroring the UK lane's shadow/permanent rule.
            if (p.Label == GoldLabel)
            {
                _goldRollingR.Enqueue(r);
                while (_goldRollingR.Count > GoldRollingKillWindow) _goldRollingR.Dequeue();
                if (_goldRollingR.Count == GoldRollingKillWindow &&
                    _goldRollingR.Sum() < GoldRollingKillR)
                {
                    bool firstGoldAlert = !_goldKillAlert;
                    _goldKillAlert = true;
                    _goldKillHalt = ModuleHealthAction == HealthGuardMode.LegacyPermanent;
                    if (firstGoldAlert)
                        Journal("HEALTH_STATE", "\"module\":\"GOLD\",\"state\":\"ALERT\",\"rolling_r\":" +
                            Jn(_goldRollingR.Sum()) + ",\"action\":\"" + ModuleHealthAction + "\"");
                }
            }

            _positionMeta.Remove(p.Id);
            _closeRequestMeta.Remove(p.Id);
            _closeRetry.Remove(p.Id);
            _closeRetryNotBeforeTicks.Remove(p.Id);
            _currentRunCreatedPositionIds.Remove(p.Id);
            _requestAuthorizedImmediateFillPositionIds.Remove(p.Id);
            _expectedPositionComments.Remove(p.Id);
            _positionCommentDiagnosticsWritten.Remove(p.Id);
            if (!RemoveOwnershipLedgerRecord("POSITION", p.Id))
                RecordExecutionIncident("ownership_ledger_position_terminal_remove_failed", 0, 0);
            RefreshObjectiveStats(true);
        }

        private void PushDaxOutcome(bool win)
        {
            _daxMonitorTotal++;
            if (_daxMonitorTotal <= 20 && win) _daxFirst20Wins++;
            _daxWinQueue.Enqueue(win);
            while (_daxWinQueue.Count > 30) _daxWinQueue.Dequeue();
            bool triggered = (_daxMonitorTotal == 20 && _daxFirst20Wins < 5) ||
                             (_daxWinQueue.Count == 30 && _daxWinQueue.Count(x => x) < 6);
            bool recovered = ModuleHealthAction == HealthGuardMode.ShadowOnly && _daxRegimeAlert &&
                             _daxWinQueue.Count == 30 && _daxWinQueue.Count(x => x) >= 9;
            if (recovered)
            {
                _daxRegimeAlert = false;
                _daxRegimeHalt = false;
                Journal("HEALTH_STATE", "\"module\":\"DAX\",\"state\":\"RECOVERED\",\"wins30\":" +
                    _daxWinQueue.Count(x => x));
            }
            if (triggered)
            {
                bool firstAlert = !_daxRegimeAlert;
                _daxRegimeAlert = true;
                _daxRegimeHalt = ModuleHealthAction == HealthGuardMode.LegacyPermanent;
                if (firstAlert)
                    Journal("HEALTH_STATE", "\"module\":\"DAX\",\"state\":\"ALERT\",\"wins30\":" +
                        _daxWinQueue.Count(x => x) + ",\"action\":\"" + ModuleHealthAction + "\"");
            }
        }

        // =========================================================================================
        // PROTECTION, FLATTENING, RESTART
        // =========================================================================================
        private void EnforceEuropeanCrossDayFlatBook()
        {
            DateTime londonToday = ToUk(Server.TimeInUtc).Date;
            foreach (var p in Positions.Where(x => IsPersistentOrCurrentPosition(x) &&
                (x.SymbolName == DaxSymbolName || x.SymbolName == UkSymbolName)).ToArray())
            {
                if (ToUk(p.EntryTime).Date >= londonToday) continue;
                if (_closeRequestMeta.ContainsKey(p.Id)) continue;
                string market = p.SymbolName == DaxSymbolName ? "DAX" : "UK";
                Journal("GUARD",
                    "\"kind\":\"cross_day_position\",\"market\":\"" + market +
                    "\",\"position_id\":" + p.Id + ",\"label\":\"" + Js(p.Label) +
                    "\",\"entry_utc\":\"" + p.EntryTime.ToString("o") + "\",\"detected_utc\":\"" +
                    Server.TimeInUtc.ToString("o") + "\"");
                EmergencyClose(p, market.ToLowerInvariant() + "_cross_day_flatten");
            }
        }

        private void EmergencyClose(Position p, string reason)
        {
            if (p == null) return;
            if (!CanManagePosition(p))
            {
                Journal("GUARD", "\"kind\":\"foreign_position_preserved\",\"position_id\":" + p.Id +
                    ",\"label\":\"" + Js(p.Label) + "\",\"requested_action\":\"close\",\"reason\":\"" + Js(reason) + "\"");
                HaltStartup("protected ownership boundary: refused to close non-owned Atlas position " + p.Id);
                return;
            }
            // A broker call remains in flight until its Closed event, or until the bounded
            // callback watchdog below proves that the event was lost. Repeated tick/backstop
            // paths must never submit a duplicate mutation for the same position.
            if (_closeRequestMeta.ContainsKey(p.Id) || _closeRetry.Contains(p.Id)) return;
            RegisterCloseRequest(p, reason);
            TradeResult result = null;
            Exception brokerException = null;
            try
            {
                result = ClosePosition(p);
            }
            catch (Exception ex)
            {
                brokerException = ex;
            }
            finally
            {
                CompleteCloseRequestBrokerCall(p.Id);
            }
            if (brokerException != null)
            {
                _closeRetry.Add(p.Id);
                _closeRetryNotBeforeTicks[p.Id] = MonotonicDeadlineAfterSeconds(BrokerRetryBackoffSeconds);
                RecordExecutionIncident("close_exception_" + brokerException.GetType().Name, 0, 0);
                Journal("GUARD", "\"kind\":\"close_retry\",\"position_id\":" + p.Id +
                    ",\"reason\":\"" + Js(reason) + "\",\"exception\":\"" +
                    Js(brokerException.GetType().Name) + "\"");
                return;
            }
            if (result == null || !result.IsSuccessful)
            {
                _closeRetry.Add(p.Id);
                _closeRetryNotBeforeTicks[p.Id] = MonotonicDeadlineAfterSeconds(BrokerRetryBackoffSeconds);
                RecordExecutionIncident("close_rejected_" + (result == null ? "null" : result.Error.ToString()), 0, 0);
                Journal("GUARD", "\"kind\":\"close_retry\",\"position_id\":" + p.Id + ",\"reason\":\"" + Js(reason) + "\"");
            }
            else
            {
                _closeRetry.Remove(p.Id);
                _closeRetryNotBeforeTicks.Remove(p.Id);
            }
        }

        private void RegisterCloseRequest(Position position, string reason)
        {
            if (position == null) return;
            CloseRequestMeta existing;
            if (_closeRequestMeta.TryGetValue(position.Id, out existing))
            {
                Symbol currentSymbol = Symbols.GetSymbol(position.SymbolName);
                existing.Attempts++;
                existing.DecisionPrice = currentSymbol == null ? existing.DecisionPrice :
                    (position.TradeType == TradeType.Buy ? currentSymbol.Bid : currentSymbol.Ask);
                existing.RequestedWallTimeUtc = DateTime.UtcNow;
                existing.BrokerCallReturnedWallTimeUtc = DateTime.MinValue;
                existing.RequestMonotonicTimestamp = Stopwatch.GetTimestamp();
                existing.BrokerCallReturnedMonotonicTimestamp = 0;
                existing.BrokerCallMs = 0;
                existing.BrokerCallCompleted = false;
                Journal("CLOSE_REQUEST_ATTEMPT",
                    "\"position_id\":" + position.Id + ",\"label\":\"" + Js(position.Label) +
                    "\",\"reason\":\"" + Js(existing.Reason) + "\",\"attempt\":" + existing.Attempts);
                return;
            }
            Symbol symbol = Symbols.GetSymbol(position.SymbolName);
            double decisionPrice = symbol == null ? 0 :
                (position.TradeType == TradeType.Buy ? symbol.Bid : symbol.Ask);
            _closeRequestMeta[position.Id] = new CloseRequestMeta
            {
                Reason = reason ?? "",
                DecisionPrice = decisionPrice,
                Attempts = 1
            };
            Journal("CLOSE_REQUEST",
                "\"position_id\":" + position.Id + ",\"label\":\"" + Js(position.Label) +
                "\",\"reason\":\"" + Js(reason) + "\",\"decision_price\":" + Jn(decisionPrice));
            CloseRequestMeta created = _closeRequestMeta[position.Id];
            created.RequestedWallTimeUtc = DateTime.UtcNow;
            created.RequestMonotonicTimestamp = Stopwatch.GetTimestamp();
        }

        private void CompleteCloseRequestBrokerCall(long positionId)
        {
            CloseRequestMeta request;
            if (!_closeRequestMeta.TryGetValue(positionId, out request)) return;
            request.BrokerCallMs = MonotonicElapsedMilliseconds(request.RequestMonotonicTimestamp);
            request.BrokerCallReturnedMonotonicTimestamp = Stopwatch.GetTimestamp();
            request.BrokerCallReturnedWallTimeUtc = DateTime.UtcNow;
            request.BrokerCallCompleted = true;
        }

        private void RetryEmergencyCloses()
        {
            if (_closeRetry.Count == 0) return;
            long nowTicks = Stopwatch.GetTimestamp();
            if (RunningMode == RunningMode.RealTime && nowTicks < _nextCloseRetryOperationTicks) return;
            foreach (long id in _closeRetry.ToArray())
            {
                long dueTicks;
                if (RunningMode == RunningMode.RealTime &&
                    _closeRetryNotBeforeTicks.TryGetValue(id, out dueTicks) && nowTicks < dueTicks)
                    continue;
                Position p = Positions.FirstOrDefault(x => x.Id == id);
                if (p == null)
                {
                    CloseRequestMeta missingEventRequest;
                    if (_closeRequestMeta.TryGetValue(id, out missingEventRequest) &&
                        missingEventRequest.BrokerCallReturnedMonotonicTimestamp > 0 &&
                        MonotonicElapsedMilliseconds(
                            missingEventRequest.BrokerCallReturnedMonotonicTimestamp) <
                            BrokerMutationEventTimeoutSeconds * 1000.0)
                    {
                        _closeRetryNotBeforeTicks[id] =
                            MonotonicDeadlineAfterSeconds(BrokerRetryBackoffSeconds);
                        continue;
                    }
                    HandleMissingCloseEvent(id, "position_absent_during_retry");
                    continue;
                }
                if (!CanManagePosition(p))
                {
                    _closeRetry.Remove(id);
                    _closeRetryNotBeforeTicks.Remove(id);
                    _closeRequestMeta.Remove(id);
                    _positionMeta.Remove(id);
                    _currentRunCreatedPositionIds.Remove(id);
                    _requestAuthorizedImmediateFillPositionIds.Remove(id);
                    _expectedPositionComments.Remove(id);
                    HaltStartup("ownership changed while an emergency close retry was pending; position preserved");
                    continue;
                }
                _nextCloseRetryOperationTicks =
                    MonotonicDeadlineAfterSeconds(BrokerRetryAggregateSpacingSeconds);
                RegisterCloseRequest(p, "retry");
                TradeResult result = null;
                Exception brokerException = null;
                try
                {
                    result = ClosePosition(p);
                }
                catch (Exception ex)
                {
                    brokerException = ex;
                }
                finally
                {
                    CompleteCloseRequestBrokerCall(p.Id);
                }
                if (brokerException != null)
                {
                    _closeRetryNotBeforeTicks[id] =
                        MonotonicDeadlineAfterSeconds(BrokerRetryBackoffSeconds);
                    RecordExecutionIncident("close_retry_exception_" +
                        brokerException.GetType().Name, 0, 0);
                    break;
                }
                if (result != null && result.IsSuccessful)
                {
                    _closeRetry.Remove(id);
                    _closeRetryNotBeforeTicks.Remove(id);
                }
                else
                {
                    _closeRetryNotBeforeTicks[id] =
                        MonotonicDeadlineAfterSeconds(BrokerRetryBackoffSeconds);
                    RecordExecutionIncident("close_retry_failed_" + (result == null ? "null" : result.Error.ToString()), 0, 0);
                }
                break;
            }
        }

        private void HandleMissingCloseEvent(long positionId, string reason)
        {
            Journal("GUARD",
                "\"kind\":\"close_event_missing\",\"position_id\":" + positionId +
                ",\"reason\":\"" + Js(reason) + "\"");
            RecordExecutionIncident("close_event_missing_" + reason, 0, 0);
            _closeRetry.Remove(positionId);
            _closeRetryNotBeforeTicks.Remove(positionId);
            _closeRequestMeta.Remove(positionId);
            _positionMeta.Remove(positionId);
            _currentRunCreatedPositionIds.Remove(positionId);
            _requestAuthorizedImmediateFillPositionIds.Remove(positionId);
            _expectedPositionComments.Remove(positionId);
            if (!RemoveOwnershipLedgerRecord("POSITION", positionId))
                HaltStartup("ownership ledger could not reconcile a broker-confirmed absent position");
            RefreshObjectiveStats(true);
        }

        private void ReconcileBrokerMutationCallbacks()
        {
            if (RunningMode != RunningMode.RealTime) return;
            foreach (var pair in _closeRequestMeta.ToArray())
            {
                if (_closeRetry.Contains(pair.Key) || !pair.Value.BrokerCallCompleted ||
                    pair.Value.BrokerCallReturnedMonotonicTimestamp <= 0 ||
                    MonotonicElapsedMilliseconds(pair.Value.BrokerCallReturnedMonotonicTimestamp) <
                        BrokerMutationEventTimeoutSeconds * 1000.0)
                    continue;
                Position position = Positions.FirstOrDefault(x => x.Id == pair.Key);
                if (position == null)
                {
                    HandleMissingCloseEvent(pair.Key, "closed_callback_timeout");
                    continue;
                }
                RecordExecutionIncident("close_success_without_flatten", 0, pair.Value.BrokerCallMs);
                _closeRetry.Add(pair.Key);
                _closeRetryNotBeforeTicks[pair.Key] = Stopwatch.GetTimestamp();
            }

            foreach (int pendingId in _cancelRequested.ToArray())
            {
                if (_cancelRetry.Contains(pendingId)) continue;
                long requestedTicks;
                if (!_cancelRequestMonotonicTicks.TryGetValue(pendingId, out requestedTicks) ||
                    requestedTicks <= 0 ||
                    MonotonicElapsedMilliseconds(requestedTicks) <
                        BrokerMutationEventTimeoutSeconds * 1000.0)
                    continue;
                PendingOrder pending = PendingOrders.FirstOrDefault(x => x.Id == pendingId);
                if (pending == null)
                {
                    Journal("GUARD",
                        "\"kind\":\"pending_cancel_event_missing\",\"pending_id\":" + pendingId);
                    RecordExecutionIncident("pending_cancel_event_missing", 0, 0);
                    _cancelRequested.Remove(pendingId);
                    _cancelRetryNotBeforeTicks.Remove(pendingId);
                    _cancelRequestMonotonicTicks.Remove(pendingId);
                    _pendingInfo.Remove(pendingId);
                    _currentRunCreatedPendingIds.Remove(pendingId);
                    _expectedPendingComments.Remove(pendingId);
                    if (!RemoveOwnershipLedgerRecord("PENDING", pendingId))
                        HaltStartup("ownership ledger could not reconcile a broker-confirmed absent pending order");
                    if (BindingAuthorityCurrent())
                        CloseAllMasterPositions("pending_cancel_event_missing");
                    continue;
                }
                RecordExecutionIncident("pending_cancel_success_without_removal", 0, 0);
                _cancelRetry.Add(pendingId);
                _cancelRetryNotBeforeTicks[pendingId] = Stopwatch.GetTimestamp();
            }
        }

        private void CloseAllMasterPositions(string reason)
        {
            foreach (var p in Positions.Where(CanManagePosition).ToArray())
                EmergencyClose(p, reason);
        }

        private void CloseLabels(IEnumerable<string> labels, string symbolName, string reason)
        {
            var set = new HashSet<string>(labels);
            foreach (var p in Positions.Where(x => x.SymbolName == symbolName && set.Contains(x.Label) && CanManagePosition(x)).ToArray())
                EmergencyClose(p, reason);
        }

        private void CancelAllMasterPending()
        {
            foreach (var o in PendingOrders.Where(CanManagePending).ToArray())
                CancelPendingWithRetry(o, "cancel_all_master");
        }

        private void CancelPendingLabel(string label, string symbolName)
        {
            foreach (var o in PendingOrders.Where(x => x.Label == label && x.SymbolName == symbolName && CanManagePending(x)).ToArray())
                CancelPendingWithRetry(o, "cancel_label");
        }

        private bool CancelPendingWithRetry(PendingOrder order, string reason)
        {
            if (order == null) return true;
            if (!CanManagePending(order))
            {
                Journal("GUARD", "\"kind\":\"foreign_pending_preserved\",\"pending_id\":" + order.Id +
                    ",\"label\":\"" + Js(order.Label) + "\",\"requested_action\":\"cancel\",\"reason\":\"" + Js(reason) + "\"");
                HaltStartup("ownership boundary: refused to cancel non-owned Atlas pending order " + order.Id);
                return false;
            }
            if (_cancelRequested.Contains(order.Id) || _cancelRetry.Contains(order.Id)) return false;
            long nowTicks = Stopwatch.GetTimestamp();
            if (RunningMode == RunningMode.RealTime && nowTicks < _nextCancelOperationTicks)
            {
                _cancelRetry.Add(order.Id);
                _cancelRetryNotBeforeTicks[order.Id] = _nextCancelOperationTicks;
                return false;
            }
            _nextCancelOperationTicks =
                MonotonicDeadlineAfterSeconds(BrokerRetryAggregateSpacingSeconds);
            _cancelRequested.Add(order.Id);
            _cancelRequestMonotonicTicks[order.Id] = Stopwatch.GetTimestamp();
            TradeResult result = null;
            Exception brokerException = null;
            try
            {
                result = CancelPendingOrder(order);
            }
            catch (Exception ex)
            {
                brokerException = ex;
            }
            if (brokerException != null)
            {
                _cancelRetry.Add(order.Id);
                _cancelRetryNotBeforeTicks[order.Id] =
                    MonotonicDeadlineAfterSeconds(BrokerRetryBackoffSeconds);
                RecordExecutionIncident("pending_cancel_exception_" +
                    brokerException.GetType().Name, 0, 0);
                Journal("GUARD",
                    "\"kind\":\"pending_cancel_retry\",\"pending_id\":" + order.Id +
                    ",\"reason\":\"" + Js(reason) + "\",\"exception\":\"" +
                    Js(brokerException.GetType().Name) + "\"");
                return false;
            }
            if (result != null && result.IsSuccessful)
            {
                _cancelRetry.Remove(order.Id);
                _cancelRetryNotBeforeTicks.Remove(order.Id);
                return true;
            }
            _cancelRetry.Add(order.Id);
            _cancelRetryNotBeforeTicks[order.Id] =
                MonotonicDeadlineAfterSeconds(BrokerRetryBackoffSeconds);
            RecordExecutionIncident("pending_cancel_failed_" + (result == null ? "null" : result.Error.ToString()), 0, 0);
            Journal("GUARD",
                "\"kind\":\"pending_cancel_retry\",\"pending_id\":" + order.Id +
                ",\"reason\":\"" + Js(reason) + "\"");
            return false;
        }

        private void RetryPendingCancels()
        {
            if (_cancelRetry.Count == 0) return;
            long nowTicks = Stopwatch.GetTimestamp();
            if (RunningMode == RunningMode.RealTime && nowTicks < _nextCancelOperationTicks) return;
            foreach (int id in _cancelRetry.ToArray())
            {
                long dueTicks;
                if (RunningMode == RunningMode.RealTime &&
                    _cancelRetryNotBeforeTicks.TryGetValue(id, out dueTicks) && nowTicks < dueTicks)
                    continue;
                PendingOrder order = PendingOrders.FirstOrDefault(x => x.Id == id);
                if (order == null)
                {
                    Journal("GUARD",
                        "\"kind\":\"pending_terminal_event_missing\",\"pending_id\":" + id);
                    RecordExecutionIncident("pending_terminal_event_missing", 0, 0);
                    _cancelRetry.Remove(id);
                    _cancelRetryNotBeforeTicks.Remove(id);
                    _cancelRequested.Remove(id);
                    _cancelRequestMonotonicTicks.Remove(id);
                    _pendingInfo.Remove(id);
                    _currentRunCreatedPendingIds.Remove(id);
                    _expectedPendingComments.Remove(id);
                    if (!RemoveOwnershipLedgerRecord("PENDING", id))
                        HaltStartup("ownership ledger could not reconcile a broker-confirmed absent pending order");
                    if (BindingAuthorityCurrent())
                        CloseAllMasterPositions("pending_terminal_event_missing");
                    continue;
                }
                if (!CanManagePending(order))
                {
                    _cancelRetry.Remove(id);
                    _cancelRetryNotBeforeTicks.Remove(id);
                    _cancelRequested.Remove(id);
                    _cancelRequestMonotonicTicks.Remove(id);
                    _pendingInfo.Remove(id);
                    _currentRunCreatedPendingIds.Remove(id);
                    _expectedPendingComments.Remove(id);
                    HaltStartup("ownership changed while a pending-order cancel retry was queued; order preserved");
                    continue;
                }
                _nextCancelOperationTicks =
                    MonotonicDeadlineAfterSeconds(BrokerRetryAggregateSpacingSeconds);
                _cancelRequested.Add(id);
                _cancelRequestMonotonicTicks[id] = Stopwatch.GetTimestamp();
                TradeResult result = null;
                Exception brokerException = null;
                try
                {
                    result = CancelPendingOrder(order);
                }
                catch (Exception ex)
                {
                    brokerException = ex;
                }
                if (brokerException != null)
                {
                    _cancelRetryNotBeforeTicks[id] =
                        MonotonicDeadlineAfterSeconds(BrokerRetryBackoffSeconds);
                    RecordExecutionIncident("pending_cancel_retry_exception_" +
                        brokerException.GetType().Name, 0, 0);
                    break;
                }
                if (result != null && result.IsSuccessful)
                {
                    _cancelRetry.Remove(id);
                    _cancelRetryNotBeforeTicks.Remove(id);
                }
                else
                {
                    _cancelRetryNotBeforeTicks[id] =
                        MonotonicDeadlineAfterSeconds(BrokerRetryBackoffSeconds);
                    RecordExecutionIncident("pending_cancel_retry_failed_" + (result == null ? "null" : result.Error.ToString()), 0, 0);
                }
                break;
            }
        }

        private bool HasAnyMasterExposure()
        {
            return Positions.Any(x => IsMasterLabel(x.Label)) || PendingOrders.Any(x => IsMasterLabel(x.Label));
        }

        private bool HasAnyAccountExposure()
        {
            return Positions.Any() || PendingOrders.Any();
        }

        private void ReconcileStartupExposure()
        {
            if (!_accountLeaseOwned) return;

            var legacyPositions = Positions.Where(x => IsLegacyLabel(x.Label)).ToList();
            var legacyPending = PendingOrders.Where(x => IsLegacyLabel(x.Label)).ToList();
            if (legacyPositions.Count > 0 || legacyPending.Count > 0)
                HaltStartup("legacy cBot exposure detected and preserved; remove/manage it before V34.ARSENAL can trade");

            if (ExclusiveAccount &&
                (Positions.Any(x => !IsOwnedPosition(x)) || PendingOrders.Any(x => !IsOwnedPending(x))))
                HaltStartup("external, legacy or non-owned Atlas exposure is present; it is preserved and V34.ARSENAL entries are blocked");

            foreach (var order in PendingOrders.Where(x => IsMasterLabel(x.Label)).ToArray())
            {
                if (!IsOwnedPending(order))
                {
                    HaltStartup("non-owned Atlas pending order " + order.Id + " preserved; manual review required");
                    continue;
                }
                bool cancelOk = CancelPendingWithRetry(order, "restart_reconciliation");
                Journal("GUARD",
                    "\"kind\":\"restart_pending_cancel\",\"order_id\":" + order.Id +
                    ",\"label\":\"" + Js(order.Label) + "\",\"ok\":" + Bool(cancelOk));
                if (!cancelOk)
                    HaltStartup("failed to cancel Atlas3 pending order during restart reconciliation");
            }
            if (PendingOrders.Any(IsOwnedPending))
                HaltStartup("owned Atlas pending order remains after restart reconciliation");

            foreach (var position in Positions.Where(x => IsMasterLabel(x.Label)).ToArray())
            {
                if (!IsOwnedPosition(position))
                {
                    string foreignProtection;
                    bool protectedForeign = ValidPositionProtection(position, out foreignProtection);
                    HaltStartup(protectedForeign
                        ? "protected non-owned Atlas position " + position.Id + " preserved; restore its original deployment or flatten manually"
                        : "CRITICAL: non-owned Atlas position " + position.Id + " has invalid protection (" + foreignProtection + "); V34.ARSENAL will not mutate it");
                    continue;
                }
                RebuildPositionMetadata(position);
                if (!_positionMeta.ContainsKey(position.Id))
                {
                    Journal("GUARD",
                        "\"kind\":\"restart_metadata_invalid\",\"position_id\":" + position.Id +
                        ",\"label\":\"" + Js(position.Label) + "\"");
                    string metadataProtection;
                    if (!ValidPositionProtection(position, out metadataProtection))
                    {
                        RecordExecutionIncident("restart_metadata_and_protection_invalid_" + metadataProtection, 0, 0);
                        EmergencyClose(position, "restart_owned_invalid_metadata_and_protection");
                    }
                    HaltStartup("owned V34.ARSENAL position metadata could not be reconstructed; protected exposure was preserved and entries are blocked");
                    continue;
                }
                string reason;
                if (ValidPositionProtection(position, out reason)) continue;
                Journal("GUARD",
                    "\"kind\":\"restart_invalid_protection\",\"position_id\":" + position.Id +
                    ",\"label\":\"" + Js(position.Label) + "\",\"reason\":\"" + Js(reason) + "\"");
                RecordExecutionIncident("restart_invalid_protection_" + reason, 0, 0);
                EmergencyClose(position, "restart_" + reason);
                HaltStartup("invalid Atlas3 position protection found during restart");
            }
        }

        private void RebuildPositionMetadataFromOpenBook()
        {
            foreach (var p in Positions.Where(IsOwnedPosition))
                RebuildPositionMetadata(p);
        }

        private void RebuildPositionMetadata(Position p)
        {
            if (p == null || !IsOwnedPosition(p)) return;
            OwnershipLedgerRecord ledgerRecord;
            bool ledgerAuthority = _ownershipLedger.TryGetValue(
                OwnershipLedgerKey("POSITION", p.Id), out ledgerRecord) &&
                OwnershipLedgerMatchesPosition(ledgerRecord, p);
            if (!ledgerAuthority) return;
            double approvedRisk = ledgerRecord.ApprovedRisk;
            double referenceDistance = ledgerRecord.ReferenceDistancePts;
            double stopDistance = ledgerRecord.StopDistancePts;
            if (stopDistance <= 0 && p.StopLoss.HasValue)
                stopDistance = Math.Abs(p.EntryPrice - p.StopLoss.Value);
            if (stopDistance <= 0) stopDistance = referenceDistance;
            if (approvedRisk <= 0 || referenceDistance <= 0 || stopDistance <= 0) return;
            Symbol symbol = Symbols.GetSymbol(p.SymbolName);
            double initialRisk = CalculateInitialRiskAtFill(p, symbol, approvedRisk);
            int direction = p.TradeType == TradeType.Buy ? 1 : -1;
            _positionMeta[p.Id] = new PositionMeta
            {
                Engine = LabelEngine(p.Label),
                Setup = LabelSetup(p.Label),
                Label = p.Label,
                SignalId = ledgerRecord.SignalId,
                InitialRisk = initialRisk,
                ApprovedRisk = approvedRisk,
                StopRiskDistancePts = stopDistance,
                ReferenceDistancePts = referenceDistance,
                Direction = direction,
                DecisionPrice = ledgerRecord.DecisionPrice,
                MfePrice = p.EntryPrice,
                MaePrice = p.EntryPrice,
                LastStopPrice = p.StopLoss ?? 0,
                LastTakeProfitPrice = p.TakeProfit ?? 0,
                Kind = LabelSetup(p.Label)
            };
        }

        private bool ValidPositionProtection(Position position, out string reason)
        {
            reason = "";
            if (position == null) { reason = "missing_position"; return false; }
            Symbol symbol = Symbols.GetSymbol(position.SymbolName);
            if (symbol == null) { reason = "missing_symbol"; return false; }
            if (!position.StopLoss.HasValue) { reason = "missing_stop"; return false; }

            double exit = position.TradeType == TradeType.Buy ? symbol.Bid : symbol.Ask;
            bool stopValid = position.TradeType == TradeType.Buy
                ? position.StopLoss.Value < exit
                : position.StopLoss.Value > exit;
            if (!stopValid) { reason = "crossed_or_invalid_stop"; return false; }

            double expectedStopDistance = 0;
            OwnershipLedgerRecord record;
            if (_ownershipLedger.TryGetValue(OwnershipLedgerKey("POSITION", position.Id), out record) &&
                record != null)
                expectedStopDistance = record.StopDistancePts;
            else
            {
                PositionMeta meta;
                if (_positionMeta.TryGetValue(position.Id, out meta) && meta != null)
                    expectedStopDistance = meta.StopRiskDistancePts;
            }
            double actualStopDistance = position.TradeType == TradeType.Buy
                ? Math.Max(0, position.EntryPrice - position.StopLoss.Value)
                : Math.Max(0, position.StopLoss.Value - position.EntryPrice);
            double stopTolerance = Math.Max(symbol.TickSize, symbol.PipSize * 0.01) * 1.01;
            if (expectedStopDistance > 0 &&
                actualStopDistance > expectedStopDistance + stopTolerance)
            {
                reason = "stop_widened_beyond_authorized_risk";
                return false;
            }

            // V34.ARSENAL: the inherited C2 re-entry carried no take-profit and was therefore
            // exempted from the target audit here. The trap fade always requests a 3.0x
            // drive-range target, so the exemption is removed and EVERY master position must
            // now carry a live, correctly-sided broker target.
            if (!position.TakeProfit.HasValue) { reason = "missing_target"; return false; }
            bool targetValid = position.TradeType == TradeType.Buy
                ? position.TakeProfit.Value > exit
                : position.TakeProfit.Value < exit;
            if (!targetValid) { reason = "crossed_or_invalid_target"; return false; }
            return true;
        }

        private void AuditOwnedPositionProtection()
        {
            if (!BindingAuthorityCurrent()) return;
            foreach (Position position in Positions.Where(IsPersistentOrCurrentPosition).ToArray())
            {
                if (_closeRequestMeta.ContainsKey(position.Id) || _closeRetry.Contains(position.Id))
                    continue;
                string reason;
                if (ValidPositionProtection(position, out reason)) continue;
                Journal("GUARD",
                    "\"kind\":\"runtime_position_protection_invalid\",\"position_id\":" +
                    position.Id + ",\"label\":\"" + Js(position.Label) +
                    "\",\"reason\":\"" + Js(reason) + "\"");
                RecordExecutionIncident("runtime_position_protection_invalid_" + reason, 0, 0);
                EmergencyClose(position, "runtime_position_protection_" + reason);
            }
        }

        private void TrackPositionExtremes()
        {
            foreach (var p in Positions)
            {
                PositionMeta meta;
                if (!_positionMeta.TryGetValue(p.Id, out meta)) continue;
                Symbol symbol = Symbols.GetSymbol(p.SymbolName);
                if (symbol == null) continue;
                double price = p.TradeType == TradeType.Buy ? symbol.Bid : symbol.Ask;
                if (meta.Direction * (price - meta.MfePrice) > 0) meta.MfePrice = price;
                if (meta.Direction * (price - meta.MaePrice) < 0) meta.MaePrice = price;
                meta.LastStopPrice = p.StopLoss ?? meta.LastStopPrice;
                meta.LastTakeProfitPrice = p.TakeProfit ?? meta.LastTakeProfitPrice;
            }
        }

        // =========================================================================================
        // HISTORY, STATE AND CALENDAR HELPERS
        // =========================================================================================
        private void SeedModuleMonitors()
        {
            // Canonical/idempotent rebuild. Management-only events may have recorded a close while
            // entry preflight was blocked; replaying History must never double-count that close.
            _daxWinQueue.Clear();
            _daxMonitorTotal = 0;
            _daxFirst20Wins = 0;
            _daxRegimeAlert = false;
            _daxRegimeHalt = false;
            _ukRollingR.Clear();
            _ukKillAlert = false;
            _ukKillHalt = false;
            _nasKillQueue.Clear();
            _nasLiveCloses = 0;
            _nasKillAlert = false;
            _nasKillHalt = false;
            _goldRollingR.Clear();
            _goldKillAlert = false;
            _goldKillHalt = false;
            var daxHistory = History
                .Where(h => h.SymbolName == DaxSymbolName &&
                            (h.Label == DaxALabel || h.Label == DaxBLabel || h.Label == "ASetup" || h.Label == "BSetup") &&
                            h.ClosingTime >= _evaluationStartUtc)
                .OrderBy(h => h.ClosingTime)
                .ToList();
            foreach (var h in daxHistory)
            {
                _daxMonitorTotal++;
                if (_daxMonitorTotal <= 20 && h.NetProfit > 0) _daxFirst20Wins++;
                _daxWinQueue.Enqueue(h.NetProfit > 0);
                while (_daxWinQueue.Count > 30) _daxWinQueue.Dequeue();
            }
            bool daxFirst20Failed = _daxMonitorTotal >= 20 && _daxFirst20Wins < 5;
            bool daxRollingFailed = _daxWinQueue.Count == 30 && _daxWinQueue.Count(x => x) < 6;
            _daxRegimeAlert = ModuleHealthAction == HealthGuardMode.LegacyPermanent
                ? daxFirst20Failed || daxRollingFailed
                : (_daxMonitorTotal == 20 && daxFirst20Failed) || daxRollingFailed;
            _daxRegimeHalt = _daxRegimeAlert && ModuleHealthAction == HealthGuardMode.LegacyPermanent;

            var ukHistory = History
                .Where(h => h.SymbolName == UkSymbolName &&
                            (h.Label == UkALabel || h.Label == "Coil30") &&
                            h.ClosingTime >= _evaluationStartUtc)
                .OrderBy(h => h.ClosingTime)
                .ToList();
            foreach (var h in ukHistory.Skip(Math.Max(0, ukHistory.Count - 60)))
            {
                double risk = ParseCommentNumber(h.Comment, "r=");
                if (risk <= 0 && h.Label == "Coil30" && _ukSymbol != null)
                {
                    double slPts = UkSlBp * 1e-4 * h.EntryPrice;
                    risk = slPts / _ukSymbol.PipSize * _ukSymbol.PipValue * h.VolumeInUnits;
                }
                if (risk > 0)
                {
                    double r = h.NetProfit / risk;
                    if (Math.Abs(r) <= 10) _ukRollingR.Enqueue(r);
                }
            }
            _ukKillAlert = _ukRollingR.Count == 60 && _ukRollingR.Sum() < -20;
            _ukKillHalt = _ukKillAlert && ModuleHealthAction == HealthGuardMode.LegacyPermanent;

            var nasHistory = History
                .Where(h => h.SymbolName == NasSymbolName &&
                            (h.Label == NasCoreLabel || h.Label == NasReLabel || h.Label == "Bell Core" || h.Label == "Bell-2RE") &&
                            h.ClosingTime >= _evaluationStartUtc)
                .OrderBy(h => h.ClosingTime)
                .ToList();
            double seedR = 0;
            int seedN = 0;
            _nasLiveCloses = nasHistory.Count(h => ParseCommentNumber(h.Comment, "r=") > 0);
            foreach (var h in nasHistory.Skip(Math.Max(0, nasHistory.Count - 40)))
            {
                double risk = ParseCommentNumber(h.Comment, "r=");
                if (risk > 0)
                {
                    double tradeR = h.NetProfit / risk;
                    seedR += tradeR;
                    seedN++;
                    _nasKillQueue.Enqueue(tradeR);
                }
            }
            _nasKillAlert = _nasLiveCloses >= 10 && _nasKillQueue.Sum() < -12;
            _nasKillHalt = _nasKillAlert && ModuleHealthAction == HealthGuardMode.LegacyPermanent;
            if (seedN > 0)
                Print("NAS health seed: n={0}, R={1:F1}, mode={2}.", seedN, seedR, ModuleHealthAction);
            // V34.ARSENAL: gold health seed. Gold trades are multi-day, so a 60-close window is
            // a long lookback by design and will normally stay unfilled (and therefore inert).
            var goldHistory = History
                .Where(h => h.SymbolName == GoldSymbolName && h.Label == GoldLabel &&
                            h.ClosingTime >= _evaluationStartUtc)
                .OrderBy(h => h.ClosingTime)
                .ToList();
            foreach (var h in goldHistory.Skip(Math.Max(0, goldHistory.Count - GoldRollingKillWindow)))
            {
                double goldRisk = ParseCommentNumber(h.Comment, "r=");
                if (goldRisk <= 0) continue;
                double goldR = h.NetProfit / goldRisk;
                if (Math.Abs(goldR) <= 10) _goldRollingR.Enqueue(goldR);
            }
            _goldKillAlert = _goldRollingR.Count == GoldRollingKillWindow &&
                _goldRollingR.Sum() < GoldRollingKillR;
            _goldKillHalt = _goldKillAlert && ModuleHealthAction == HealthGuardMode.LegacyPermanent;
            Journal("HEALTH_REBUILD",
                "\"mode\":\"" + ModuleHealthAction + "\",\"dax_alert\":" + Bool(_daxRegimeAlert) +
                ",\"uk_alert\":" + Bool(_ukKillAlert) + ",\"nas_alert\":" + Bool(_nasKillAlert) +
                ",\"gold_alert\":" + Bool(_goldKillAlert));
        }

        private bool TradedToday(string label, string symbolName, DateTime day, bool ukDay)
        {
            if (HasPosition(label, symbolName)) return true;
            foreach (var h in History)
            {
                if (h.SymbolName != symbolName || h.Label != label) continue;
                DateTime entryDay = ukDay ? ToUk(h.EntryTime).Date : ToEastern(h.EntryTime).Date;
                if (entryDay == day) return true;
            }

            // Migration safety: old labels on the same symbol also consume today's slot.
            string legacy = LegacyForNewLabel(label);
            if (!string.IsNullOrEmpty(legacy))
                foreach (var h in History)
                {
                    if (h.SymbolName != symbolName || h.Label != legacy) continue;
                    DateTime entryDay = ukDay ? ToUk(h.EntryTime).Date : ToEastern(h.EntryTime).Date;
                    if (entryDay == day) return true;
                }
            return false;
        }

        private double ModuleDayPnl(IEnumerable<string> labels, string symbolName, DateTime day, bool ukDay)
        {
            var set = new HashSet<string>(labels);
            double pnl = 0;
            foreach (var h in History)
            {
                if (h.SymbolName != symbolName || !set.Contains(h.Label)) continue;
                DateTime closeDay = ukDay ? ToUk(h.ClosingTime).Date : ToEastern(h.ClosingTime).Date;
                if (closeDay == day) pnl += h.NetProfit;
            }
            foreach (var p in Positions)
                if (p.SymbolName == symbolName && set.Contains(p.Label)) pnl += p.NetProfit;
            return pnl;
        }

        private bool HasPosition(string label, string symbolName)
        {
            return Positions.Any(x => x.Label == label && x.SymbolName == symbolName);
        }

        private bool HasPending(string label, string symbolName)
        {
            return PendingOrders.Any(x => x.Label == label && x.SymbolName == symbolName);
        }

        private bool IsDaxTradingDay(DateTime uk)
        {
            DayOfWeek d = uk.DayOfWeek;
            if (d == DayOfWeek.Friday || d == DayOfWeek.Saturday || d == DayOfWeek.Sunday) return false;
            if (uk.Month == 11 || uk.Month == 12) return false;
            return !IsDaxHoliday(uk.Date);
        }

        private bool IsDaxHoliday(DateTime date)
        {
            int y = date.Year;
            DateTime easter = EasterSunday(y);
            if (date == new DateTime(y, 1, 1)) return true;
            if (date == easter.AddDays(-2) || date == easter.AddDays(1)) return true;
            if (date == new DateTime(y, 5, 1) || date == new DateTime(y, 10, 3)) return true;
            if (date == NthWeekday(y, 1, DayOfWeek.Monday, 3)) return true;
            if (date == NthWeekday(y, 2, DayOfWeek.Monday, 3)) return true;
            if (date == LastWeekday(y, 5, DayOfWeek.Monday)) return true;
            if (date == new DateTime(y, 6, 19) || date == new DateTime(y, 7, 4)) return true;
            return date == NthWeekday(y, 9, DayOfWeek.Monday, 1);
        }

        private void SeedNasCalendars()
        {
            int year = ToEastern(Server.TimeInUtc).Year;
            for (int y = year - 1; y <= year + 2; y++)
            {
                AddNasHalfDay(new DateTime(y, 7, 3));
                AddNasHalfDay(NthWeekday(y, 11, DayOfWeek.Thursday, 4).AddDays(1));
                AddNasHalfDay(new DateTime(y, 12, 24));

                DateTime newYear = new DateTime(y, 1, 1);
                if (newYear.DayOfWeek == DayOfWeek.Sunday) AddNasHoliday(newYear.AddDays(1));
                else if (newYear.DayOfWeek != DayOfWeek.Saturday) AddNasHoliday(newYear);
                AddNasHoliday(NthWeekday(y, 1, DayOfWeek.Monday, 3));
                AddNasHoliday(NthWeekday(y, 2, DayOfWeek.Monday, 3));
                AddNasHoliday(EasterSunday(y).AddDays(-2));
                AddNasHoliday(LastWeekday(y, 5, DayOfWeek.Monday));
                AddNasHoliday(ObservedFixed(new DateTime(y, 6, 19)));
                AddNasHoliday(ObservedFixed(new DateTime(y, 7, 4)));
                AddNasHoliday(NthWeekday(y, 9, DayOfWeek.Monday, 1));
                AddNasHoliday(NthWeekday(y, 11, DayOfWeek.Thursday, 4));
                AddNasHoliday(ObservedFixed(new DateTime(y, 12, 25)));
            }
            foreach (string value in (ExtraHalfDaysCsv ?? "").Split(','))
                if (!string.IsNullOrWhiteSpace(value)) _nasHalfDays.Add(value.Trim());
        }

        private void AddNasHalfDay(DateTime date)
        {
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                _nasHalfDays.Add(date.ToString("yyyy-MM-dd"));
        }

        private void AddNasHoliday(DateTime date)
        {
            _nasFullHolidays.Add(date.ToString("yyyy-MM-dd"));
        }

        private void ParseRestrictedEvents()
        {
            _restrictedEvents.Clear();
            _restrictedEventsParseValid = true;
            foreach (string item in (RestrictedEventsCsv ?? "").Split(';'))
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                DateTime parsed;
                if (DateTime.TryParseExact(item.Trim(), "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out parsed))
                    _restrictedEvents.Add(DateTime.SpecifyKind(parsed, DateTimeKind.Utc));
                else
                    _restrictedEventsParseValid = false;
            }
            _restrictedEvents.Sort();

            DateTime verifiedThrough;
            if (DateTime.TryParseExact((RestrictedEventsVerifiedThroughText ?? "").Trim(), "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out verifiedThrough))
                _restrictedEventsVerifiedThroughUtc = DateTime.SpecifyKind(verifiedThrough.Date.AddDays(1), DateTimeKind.Utc);
            else
                _restrictedEventsVerifiedThroughUtc = DateTime.MinValue;

            // ===== V25 FIX-2a (CRITICAL, regression fix within V25) =====
            // "News calendar verified through UTC" ships BLANK. V24 never noticed, because the
            // coverage halt was gated on IsFunded(). V25 FIX-2 de-gated it for Challenge — which
            // turned that blank default into an UNCONDITIONAL StartupHalt on an evaluation account:
            // MinValue < (today + 8d) is always true, the check runs on every bar, and _startupHalt
            // never clears. Both bots would have sat dead on Monday having placed zero trades.
            //
            // Fix: when the operator has not explicitly attested, DERIVE implicit coverage from the
            // last event actually present in the calendar. The shipped list runs to 2026-12-09, so
            // the guard stays dormant while the calendar is genuinely current and still fails CLOSED
            // once it lapses (~1 week before the final event). An explicit attestation still wins
            // if it is later than the implicit date, so operator discipline is preserved.
            if (_restrictedEventsVerifiedThroughUtc == DateTime.MinValue && _restrictedEvents.Count > 0)
            {
                DateTime lastEvent = _restrictedEvents[_restrictedEvents.Count - 1];   // list is sorted above
                _restrictedEventsVerifiedThroughUtc = DateTime.SpecifyKind(lastEvent.Date.AddDays(1), DateTimeKind.Utc);
                Print("V25 FIX-2a: news calendar not explicitly attested; implicit coverage derived from the last listed event -> {0:yyyy-MM-dd}. Set 'News calendar verified through UTC' to override.",
                    _restrictedEventsVerifiedThroughUtc.AddDays(-1));
            }
        }

        private void ValidateRestrictedEventCoverage()
        {
            // V25 FIX-2: the calendar-coverage halt now protects Challenge accounts too. The shipped
            // FOMC list ends 2026-12-09; without this, an evaluation account would simply trade on
            // with an exhausted calendar and no warning.
            if (RunningMode != RunningMode.RealTime || !FundedNewsGuard
                || Stage == StageMode.Unconfigured) return;
            if (_startupHalt) return;
            if (!_restrictedEventsParseValid)
            {
                HaltStartup("restricted-news calendar contains a malformed UTC timestamp");
                if (BindingAuthorityCurrent()) CancelAllMasterPending();
                return;
            }
            if (_restrictedEvents.Count == 0)
            {
                HaltStartup("restricted-news guard requires at least one event before arming");
                if (BindingAuthorityCurrent()) CancelAllMasterPending();
                return;
            }
            if (_restrictedEventsVerifiedThroughUtc < Server.TimeInUtc.Date.AddDays(8))
            {
                HaltStartup("restricted-news calendar must be operator-verified at least seven days ahead before arming");
                if (BindingAuthorityCurrent()) CancelAllMasterPending();
            }
        }

        private void CheckExclusiveAccountIntegrity()
        {
            if (!ExclusiveAccount || !_accountLeaseOwned || _startupHalt) return;
            int externalPositions = Positions.Count(x => !IsRuntimeRecognizedPosition(x));
            int externalPending = PendingOrders.Count(x => !IsRuntimeRecognizedPending(x));
            if (externalPositions == 0 && externalPending == 0) return;

            Journal("GUARD",
                "\"kind\":\"external_exposure_runtime\",\"positions\":" + externalPositions +
                ",\"pending\":" + externalPending);
            CancelAllMasterPending();
            HaltStartup("external/manual exposure appeared while exclusive account ownership is enabled");
        }

        private void AcquireAccountLease()
        {
            if (RunningMode != RunningMode.RealTime)
            {
                _accountLeaseOwned = true;
                return;
            }

            try
            {
                string lockDir = System.IO.Path.Combine(_journalDir, "locks");
                System.IO.Directory.CreateDirectory(lockDir);
                string lockFile = System.IO.Path.Combine(lockDir,
                    "Atlas3_acc" + Account.Number.ToString(CultureInfo.InvariantCulture) + ".lock");
                _accountLease = new System.IO.FileStream(
                    lockFile, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                _accountLeaseOwned = true;
            }
            catch (Exception ex)
            {
                _accountLeaseOwned = false;
                HaltStartup("another Atlas3 master owns this account, or the account lock failed: " + ex.Message);
            }
        }

        private void ParseEvaluationStart()
        {
            string text = (EvaluationStartText ?? "").Trim();
            if (text.Length == 0)
            {
                if (RunningMode == RunningMode.RealTime)
                    HaltStartup("evaluation start date is required in LIVE mode");
                _evaluationStartUtc = DateTime.MinValue;
                return;
            }

            DateTime parsed;
            if (!DateTime.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out parsed))
            {
                HaltStartup("evaluation start date must be yyyy-MM-dd");
                return;
            }

            try
            {
                DateTime pragueMidnight = DateTime.SpecifyKind(parsed.Date, DateTimeKind.Unspecified);
                _evaluationStartUtc = TimeZoneInfo.ConvertTimeToUtc(pragueMidnight, _prague);
            }
            catch
            {
                HaltStartup("evaluation start date could not be converted from Prague time");
            }
        }

        private void ParseDayStartOverrideDate()
        {
            string text = (DayStartOverrideDateText ?? "").Trim();
            if (DayStartBalanceOverride <= 0 && text.Length == 0)
            {
                _dayStartOverrideDate = DateTime.MinValue;
                return;
            }
            if (DayStartBalanceOverride <= 0 || text.Length == 0)
            {
                HaltStartup("day-start override amount and Prague date must be supplied together");
                return;
            }

            DateTime parsed;
            if (!DateTime.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out parsed))
            {
                HaltStartup("day-start override Prague date must be yyyy-MM-dd");
                return;
            }
            _dayStartOverrideDate = parsed.Date;
        }

        private void ResolveTimeZones()
        {
            _eastern = ResolveZone("America/New_York", "Eastern Standard Time");
            _prague = ResolveZone("Europe/Prague", "Central Europe Standard Time");
            if (_eastern == null) HaltStartup("America/New_York timezone unavailable");
            if (_prague == null) HaltStartup("Europe/Prague timezone unavailable");
        }

        private TimeZoneInfo ResolveZone(string iana, string windows)
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(iana); }
            catch
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(windows); }
                catch { return null; }
            }
        }

        private bool ValidateDataContexts()
        {
            if (_daxM15 == null || _ukM15 == null || _ukH4 == null || _nasM5 == null) return false;
            // V34.ARSENAL: gold joins the identity/history contract only while its lane is on.
            if (GoldEnabled && _goldH1 == null) return false;
            bool identity = _daxM15.SymbolName == DaxSymbolName && _daxM15.TimeFrame == TimeFrame.Minute15 &&
                            _ukM15.SymbolName == UkSymbolName && _ukM15.TimeFrame == TimeFrame.Minute15 &&
                            _ukH4.SymbolName == UkSymbolName && _ukH4.TimeFrame == TimeFrame.Hour4 &&
                            _nasM5.SymbolName == NasSymbolName && _nasM5.TimeFrame == TimeFrame.Minute5 &&
                            (!GoldEnabled || (_goldH1.SymbolName == GoldSymbolName &&
                                              _goldH1.TimeFrame == TimeFrame.Hour1));
            bool history = _daxM15.Count >= 4 && _ukM15.Count >= 4 && _nasM5.Count >= 80 &&
                           (!UkH4Gate || _ukH4.Count >= 52) &&
                           (!GoldEnabled || _goldH1.Count >= GoldMinimumBars);
            if (!identity) HaltStartup("explicit Bars identity/timeframe assertion failed");
            if (!history)
            {
                if (_dataWarmupSinceUtc == DateTime.MinValue) _dataWarmupSinceUtc = Server.TimeInUtc;
                LoadMinimumHistory(_daxM15, 4);
                LoadMinimumHistory(_ukM15, 4);
                if (UkH4Gate) LoadMinimumHistory(_ukH4, 52);
                LoadMinimumHistory(_nasM5, 80);
                if (GoldEnabled) LoadMinimumHistory(_goldH1, GoldMinimumBars);
                if (_lastDataWarmupLogUtc == DateTime.MinValue ||
                    Server.TimeInUtc - _lastDataWarmupLogUtc >= TimeSpan.FromSeconds(30))
                {
                    _lastDataWarmupLogUtc = Server.TimeInUtc;
                    Print("ATLAS DATA WARMUP | DAX M15={0} [{1:o}..{2:o}] | UK M15={3} [{4:o}..{5:o}] | UK H4={6} [{7:o}..{8:o}] | NAS M5={9} [{10:o}..{11:o}] | GOLD H1={12}",
                        _daxM15.Count, FirstBarTime(_daxM15), LastBarTime(_daxM15),
                        _ukM15.Count, FirstBarTime(_ukM15), LastBarTime(_ukM15),
                        _ukH4.Count, FirstBarTime(_ukH4), LastBarTime(_ukH4),
                        _nasM5.Count, FirstBarTime(_nasM5), LastBarTime(_nasM5),
                        _goldH1 == null ? 0 : _goldH1.Count);
                }
                if (RunningMode == RunningMode.RealTime && !_dataBlockedLogged &&
                    Server.TimeInUtc - _dataWarmupSinceUtc > TimeSpan.FromMinutes(5))
                {
                    _dataBlockedLogged = true;
                    Print("*** ATLAS DATA BLOCKED (RECOVERABLE): required multi-market history is still incomplete; entries remain blocked and loading will continue. ***");
                    Journal("DATA_BLOCKED", "\"detail\":\"" + Js(DataWarmupDetail()) + "\"");
                }
            }
            else if (_dataBlockedLogged)
            {
                _dataBlockedLogged = false;
                Print("ATLAS DATA RECOVERED: all required contexts are ready; activation may resume automatically.");
                Journal("DATA_RECOVERED", "\"dax_m15\":" + _daxM15.Count +
                    ",\"uk_m15\":" + _ukM15.Count + ",\"uk_h4\":" + _ukH4.Count +
                    ",\"nas_m5\":" + _nasM5.Count);
            }
            return identity && history;
        }

        private void LoadMinimumHistory(Bars bars, int minimum)
        {
            if (bars == null || bars.Count >= minimum) return;
            try
            {
                for (int attempt = 0; attempt < 3 && bars.Count < minimum; attempt++)
                    if (bars.LoadMoreHistory() <= 0) break;
            }
            catch (Exception ex)
            {
                Print("ATLAS DATA LOAD WARNING | {0} {1} | {2}", bars.SymbolName, bars.TimeFrame, ex.Message);
            }
        }

        private static DateTime FirstBarTime(Bars bars)
        {
            return bars == null || bars.Count == 0 ? DateTime.MinValue : bars.OpenTimes[0];
        }

        private static DateTime LastBarTime(Bars bars)
        {
            return bars == null || bars.Count == 0 ? DateTime.MinValue : bars.OpenTimes[bars.Count - 1];
        }

        // =========================================================================================
        // DISPLAY AND JOURNAL
        // =========================================================================================
        private void DrawMasterHud()
        {
            List<string> readinessBlockers = GetGlobalEntryBlockers();
            if (!ShowHud || Chart == null) return;
            RefreshDynamicRiskDecision();
            bool unbounded;
            double openRisk = CalculateAllOpenRemainingRisk(out unbounded);
            bool pendingUnbounded;
            double pendingRisk = CalculateAllPendingRisk(out pendingUnbounded);
            double bestShare = _positiveDaysProfit > 0 ? 100.0 * _bestDayProfit / _positiveDaysProfit : 0;
            bool readyToTrade = readinessBlockers.Count == 0;
            bool displayRiskBlocked = unbounded || pendingUnbounded;
            string trafficState = DesignTrafficState(displayRiskBlocked, readinessBlockers);
            Color stateColor = DesignStateColor(trafficState);

            if (!ShowExecutionCockpit)
            {
                ClearDesignObjects();
                string compact = "ATLAS3 V34.ARSENAL R1 | S1 CHALLENGE\n" +
                    trafficState + " — " +
                    DesignBlockerText(DesignPriorityBlocker(readinessBlockers)) + "\n" +
                    "NEXT ACTION — " + DesignNextAction(readinessBlockers) + "\n" +
                    "ACCOUNT " + MaskedAccountNumber() + " | " + (Account.IsLive ? "LIVE" : "DEMO") +
                    " | HOST " + SymbolName + " " + Bars.TimeFrame +
                    " | RECEIPT " + DesignBindingState() + "\n" +
                    "EQUITY $" + Account.Equity.ToString("N0") +
                    " | TIER " + _currentDynamicRiskTier + " x" + _currentDynamicRiskScale.ToString("F2") +
                    " | RISK $" + (openRisk + pendingRisk).ToString("N0") +
                    " | FRONTIER " + FrontierModeText() +
                    " | LEDGER " + DesignLedgerState();
                Chart.DrawStaticText("ATLAS3_MASTER_HUD", compact, VerticalAlignment.Top, HorizontalAlignment.Left, stateColor);
                return;
            }

            Chart.RemoveObject("ATLAS3_MASTER_HUD");
            double effectiveFloor = DesignRequiredFloor();
            double floorBuffer = Account.Equity - effectiveFloor;
            double committedRisk = openRisk + pendingRisk + _transientReservedRisk;
            double executionReserve = committedRisk * ExecutionReservePct / 100.0;
            double riskHeadroom = Math.Max(0,
                (Account.Equity - effectiveFloor) / (1.0 + ExecutionReservePct / 100.0) - committedRisk);
            double riskCapacity = Math.Max(1.0, committedRisk + riskHeadroom);
            bool riskBlocked = unbounded || pendingUnbounded;
            string riskGauge = riskBlocked ? "[UNVERIFIED]" : DesignGauge(committedRisk, riskCapacity);
            string headroomText = riskBlocked ? "BLOCKED" : "$" + riskHeadroom.ToString("N0");
            string objectiveLine = IsFunded()
                ? "PAYOUT  " + (!FundedPayoutLockEnabled ? "LOCK OFF" : (_fundedPayoutLocked ? "LOCKED" :
                    (Account.Balance >= FundedPayoutTargetBalance() ? "TARGET REACHED" : "PENDING"))) +
                  (FundedPayoutLockEnabled ? " @ " + FundedPayoutLockTargetPct.ToString("F2") + "%" : "")
                : "TARGET  " + (_targetReached ? "REACHED" : "PENDING");
            string control =
                "ATLAS3 V34.ARSENAL R1  |  S1 CHALLENGE\n" +
                trafficState + " — " +
                    DesignBlockerText(DesignPriorityBlocker(readinessBlockers)) + "\n" +
                "NEXT ACTION — " + DesignNextAction(readinessBlockers) + "\n" +
                "ACCOUNT " + MaskedAccountNumber() + " | " + (Account.IsLive ? "LIVE" : "DEMO") +
                    " | PROFILE S1 CHALLENGE\n" +
                "HOST " + SymbolName + " " + Bars.TimeFrame +
                    (SymbolName == RequiredHostSymbol && Bars.TimeFrame == TimeFrame.Minute5 ? " [OK]" : " [BLOCKED]") +
                    " | RECEIPT " + DesignBindingState() +
                    " | START " + (_bindingValid ? _effectiveEvaluationStartText : _proposedEvaluationStartText) + " PRAGUE\n" +
                "BINDING DETAIL  " + DesignTrim(_bindingStatus, 90) + "\n" +
                "EQUITY  $" + Account.Equity.ToString("N0").PadLeft(8) +
                "   BALANCE  $" + Account.Balance.ToString("N0").PadLeft(8) + "\n" +
                "RISK TIER " + _currentDynamicRiskTier + "  x" + _currentDynamicRiskScale.ToString("F2") +
                "   DAY " + _dynamicDayProgressPct.ToString("+0.00;-0.00;0.00") + "%\n" +
                "FLOOR   $" + effectiveFloor.ToString("N0").PadLeft(8) +
                "   BUFFER   $" + floorBuffer.ToString("N0").PadLeft(8) + "\n" +
                "FRONTIER " + FrontierModeText().PadRight(6) +
                "  DAILY $" + FrontierDailyRiskBudgetUsd().ToString("N0") +
                "   DAX $" + (FrontierDailyRiskBudgetUsd() * FrontierDaxShare).ToString("N0") +
                "   NAS $" +
                    (FrontierDailyRiskBudgetUsd() * FrontierNasBudgetShare * NasAlignedPct / 100.0).ToString("N0") +
                " / $" +
                    (FrontierDailyRiskBudgetUsd() * FrontierNasBudgetShare * NasMisalignedPct / 100.0).ToString("N0") + "\n" +
                "GATE RISK " + riskGauge +
                "  $" + committedRisk.ToString("N0") + "   HEADROOM " + headroomText + "\n" +
                "OPEN    $" + openRisk.ToString("N0") + (unbounded ? " UNBOUNDED" : "") +
                "   PENDING $" + pendingRisk.ToString("N0") + (pendingUnbounded ? " UNBOUNDED" : "") +
                "   RESERVE $" + executionReserve.ToString("N0") + "\n" +
                objectiveLine +
                "   BEST DAY  " + bestShare.ToString("F1") + "% / 50.0%\n" +
                "SYSTEM  DATA " + (_dataReady ? "OK" : "WAIT") +
                "  LEASE " + (_accountLeaseOwned ? "OWNED" : "BLOCKED") +
                "  LEDGER " + DesignLedgerState() +
                "  JOURNAL " + (_journalDead ? "FAILED" : (_journalFailures > 0 ? "RETRY" : "OK")) +
                "  EXEC " + DesignExecutionGuardState() + "\n" +
                "ENGINE " + DesignEngineCheck() + "   TICKS D/U/N " +
                DesignTickAge(_lastDaxPulseUtc) + "/" + DesignTickAge(_lastUkPulseUtc) + "/" + DesignTickAge(_lastNasPulseUtc) + "\n" +
                "ENTRY CHECKS  MODULE QUOTE / TICK AGE / SPREAD / CAPACITY AT PROPOSAL TIME\n" +
                ((_deploymentArmed || _preflightBlockers.Count == 0) ? "" :
                    (_bindingValid ? "SAFETY BLOCKERS  " : "SETUP BLOCKERS  ") +
                    DesignTrim(string.Join(" | ", _preflightBlockers), 120) + "\n") +
                "LAST DECISION  " + DesignTrim(_lastGateReason, 68);

            string engines =
                "STRATEGY RADAR\n" +
                "DAX   A1 " + DesignModuleState(DaxALabel, DaxSymbolName, DaxAEnabled, _daxA != null && _daxA.Valid, _daxA != null && _daxA.Traded, _daxRegimeHalt || _daxDayBlocked || _daxDailyProfitLocked).PadRight(9) +
                " A2 " + DesignModuleState(DaxBLabel, DaxSymbolName, DaxBEnabled, _daxB != null && _daxB.Valid, _daxB != null && _daxB.Traded, _daxRegimeHalt || _daxDayBlocked || _daxDailyProfitLocked).PadRight(9) +
                " A3 " + DesignModuleState(DaxCLabel, DaxSymbolName, DaxCEnabled, _daxCOrReady, _daxCTraded, _daxRegimeHalt || _daxDayBlocked || _daxDailyProfitLocked) + "\n" +
                "UK100 B1 " + DesignModuleState(UkALabel, UkSymbolName, UkAEnabled, _ukOrReady, _ukATraded, _ukKillHalt).PadRight(9) +
                " B2 FORCED OFF\n" +
                "NAS   C1 " + DesignModuleState(NasCoreLabel, NasSymbolName, NasCoreEnabled, _nasDriveDirection != 0, _nasCoreDone, _nasKillHalt).PadRight(9) +
                " C2 " + DesignModuleState(NasReLabel, NasSymbolName, NasReentryEnabled, _nasPendingReentry, _nasReDone, _nasKillHalt) + "\n" +
                "XAU   D1 " + DesignModuleState(GoldLabel, GoldSymbolName, GoldEnabled,
                    !double.IsNaN(_goldSwingHigh) || !double.IsNaN(_goldSwingLow), false, _goldKillHalt);

            var controlObject = Chart.DrawStaticText("ATLAS3_DESIGN_CONTROL", control, VerticalAlignment.Top, HorizontalAlignment.Left, stateColor);
            controlObject.FontSize = 11;
            controlObject.IsBold = true;
            var engineObject = Chart.DrawStaticText("ATLAS3_DESIGN_ENGINES", engines, VerticalAlignment.Bottom, HorizontalAlignment.Right, DesignMuted);
            engineObject.FontSize = 10;
            var bookObject = Chart.DrawStaticText("ATLAS3_DESIGN_BOOK", DesignBookText(), VerticalAlignment.Top, HorizontalAlignment.Right, DesignText);
            bookObject.FontSize = 10;
            DrawDesignHostLevels();
        }

        private string DesignTrafficState(bool riskUnverified, List<string> blockers)
        {
            if (!riskUnverified && blockers != null && blockers.Count == 0) return "[GREEN] READY";
            bool firstUse = !_bindingValid &&
                (_bindingStatus ?? "").IndexOf("first-run", StringComparison.OrdinalIgnoreCase) >= 0;
            string priority = DesignPriorityBlocker(blockers);
            bool hardBlock = riskUnverified || _startupHalt || _journalDead || _ownershipLedgerInvalid ||
                _dayHalt || _capacityFrozen ||
                _executionGuardState == ExecutionGuardState.HardHalt ||
                priority.Contains("external_or_non_owned") ||
                priority.Contains("management_authority") ||
                ((priority.Contains("binding_not_authorized") || priority.Contains("identity_changed")) && !firstUse);
            if (hardBlock) return "[RED] BLOCKED";
            bool normalWait =
                ((!_bindingValid && (_bindingStatus ?? "").IndexOf("first-run", StringComparison.OrdinalIgnoreCase) >= 0) ||
                 !_dataReady || !_accountLeaseOwned ||
                 _executionGuardState == ExecutionGuardState.LatencyCooldown);
            return normalWait ? "[AMBER] WAIT" : "[RED] BLOCKED";
        }

        private string DesignPriorityBlocker(List<string> blockers)
        {
            if (blockers == null || blockers.Count == 0) return "";
            string[] priority =
            {
                "startup_halt", "hard_halt", "journal", "external_or_non_owned",
                "management_authority", "binding_not_authorized", "identity_changed",
                "day_halt", "objective", "payout", "news", "lease",
                "latency_cooldown", "data_not_ready", "not_running", "arm_not_set"
            };
            foreach (string token in priority)
            {
                string match = blockers.FirstOrDefault(x =>
                    (x ?? "").IndexOf(token, StringComparison.Ordinal) >= 0);
                if (!string.IsNullOrEmpty(match)) return match;
            }
            return blockers[0] ?? "";
        }

        private string DesignBlockerText(string blocker)
        {
            string value = blocker ?? "";
            if (value.Length == 0) return "ALL SAFETY CHECKS PASSED";
            if (value.Contains("binding_not_authorized") || value.Contains("identity_changed"))
                return !_bindingValid &&
                    (_bindingStatus ?? "").IndexOf("first-run", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "FIRST-USE CONFIRMATION REQUIRED"
                    : "DEPLOYMENT RECEIPT OR IDENTITY MISMATCH";
            if (value.Contains("lease")) return "ANOTHER ATLAS INSTANCE HOLDS THIS ACCOUNT";
            if (value.Contains("data_not_ready")) return "MARKET DATA IS WARMING UP";
            if (value.Contains("latency_cooldown")) return "EXECUTION COOLDOWN IS ACTIVE";
            if (value.Contains("hard_halt")) return "EXECUTION SAFETY HARD HALT";
            if (value.Contains("journal")) return "SAFETY JOURNAL IS UNAVAILABLE";
            if (value.Contains("external_or_non_owned")) return "EXTERNAL OR UNRECOGNISED EXPOSURE EXISTS";
            if (value.Contains("news")) return "RESTRICTED NEWS WINDOW";
            if (value.Contains("day_halt")) return "DAILY SAFETY LIMIT IS ACTIVE";
            if (value.Contains("objective")) return "OBJECTIVE CONTROL IS ACTIVE";
            if (value.Contains("startup_halt")) return "STARTUP SAFETY CHECK FAILED";
            if (value.Contains("not_running") || value.Contains("arm_not_set"))
                return "DEPLOYMENT IS NOT RUNNING";
            return DesignTrim(value.Replace("_", " ").ToUpperInvariant(), 72);
        }

        private string DesignNextAction(List<string> blockers)
        {
            if (blockers == null || blockers.Count == 0)
                return "NONE — AUTOMATIC MONITORING IS ACTIVE";
            if (SymbolName != RequiredHostSymbol || Bars == null || Bars.TimeFrame != TimeFrame.Minute5)
                return "ATTACH ONE INSTANCE TO US100.cash M5";
            if (_executionGuardState == ExecutionGuardState.HardHalt)
            {
                string refusal;
                if (!CanOfferOneClickExecutionRearm(out refusal))
                    return "SAFE REARM BLOCKED — SAVE LOG AND RESOLVE: " + DesignTrim(refusal, 52);
                double elapsed = _oneClickRearmReviewStartedTicks <= 0
                    ? 0
                    : MonotonicElapsedMilliseconds(_oneClickRearmReviewStartedTicks) / 1000.0;
                return elapsed >= OneClickRearmReviewSeconds
                    ? "CLICK SAFE REARM ONCE — ENROLLMENT REMAINS SEPARATE"
                    : "WAIT FOR THE 5-SECOND SAFETY REVIEW — KEEP THE ACCOUNT FLAT";
            }
            if (_journalDead || _ownershipLedgerInvalid)
                return "STOP AND REVIEW THE SAFETY LOG — DO NOT BYPASS";
            if (blockers.Any(x => x.Contains("external_or_non_owned")))
                return "REVIEW OR FLATTEN FOREIGN EXPOSURE — DO NOT BYPASS";
            if (!_bindingValid &&
                (_bindingStatus ?? "").IndexOf("first-run", StringComparison.OrdinalIgnoreCase) < 0)
                return "STOP — DEPLOYMENT RECEIPT OR IDENTITY NEEDS REVIEW";
            if (_startupHalt || _dayHalt || _capacityFrozen)
                return "STOP AND REVIEW THE SAFETY LOG — DO NOT BYPASS";
            if (!_accountLeaseOwned) return "STOP THE DUPLICATE ATLAS INSTANCE";
            if (!_bindingValid)
                return "REVIEW DETAILS BELOW, THEN CLICK CONFIRM & ARM";
            if (!_dataReady) return "WAIT FOR MARKET DATA — THIS IS AUTOMATIC";
            if (_executionGuardState == ExecutionGuardState.LatencyCooldown)
                return "WAIT — RECOVERY IS AUTOMATIC WHEN SAFE";
            return "STOP AND REVIEW THE FIRST BLOCKER AND LOG — DO NOT BYPASS";
        }

        private string DesignLedgerState()
        {
            if (_ownershipLedgerInvalid) return "INVALID";
            if (!_ownershipLedgerLoaded) return _bindingValid ? "NOT READY" : "PENDING";
            int positions = _ownershipLedger.Values.Count(x => x.ExposureType == "POSITION");
            int orders = _ownershipLedger.Values.Count(x => x.ExposureType == "PENDING");
            return "OK P" + positions + "/O" + orders;
        }

        private string DesignRuntimeState()
        {
            if (_startupHalt) return "STARTUP HALT";
            if (RunningMode == RunningMode.RealTime && !_bindingValid) return "SETUP REQUIRED";
            if (RunningMode == RunningMode.RealTime && JournalOn && _journalDead) return "JOURNAL HALT";
            if (_executionGuardState == ExecutionGuardState.HardHalt) return "EXEC HARD HALT";
            if (_executionGuardState == ExecutionGuardState.LatencyCooldown) return "EXEC COOLDOWN";
            if (_dayHalt) return "DAY HALT";
            if (_fundedPayoutLocked) return "PAYOUT LOCKED";
            if (_fundedPayoutClosing) return "PAYOUT CLOSING";
            if (_objectiveLocked) return "OBJECTIVE LOCKED";
            if (_objectiveClosing) return "OBJECTIVE CLOSING";
            if (!_accountLeaseOwned) return "LEASE BLOCKED";
            if (!_dataReady) return "WARMING UP";
            if (_capacityFrozen) return "CAPACITY FROZEN";
            if (RunningMode == RunningMode.RealTime &&
                (_deploymentState != DeploymentState.Running || !_deploymentArmed)) return "BLOCKED";
            return GetGlobalEntryBlockers().Count == 0 ? "GLOBAL SAFETY READY" : "BLOCKED";
        }

        private string DesignBindingState()
        {
            if (!_bindingValid) return "SETUP REQUIRED";
            return RuntimeIdentityMatchesReceipt() ? "EXACT" : "IDENTITY MISMATCH";
        }

        private string DesignExecutionGuardState()
        {
            if (_executionGuardState == ExecutionGuardState.HardHalt) return "HARD HALT";
            if (_executionGuardState == ExecutionGuardState.LatencyCooldown)
                return "COOLDOWN TO " + StateDateText(_latencyCooldownUntilUtc);
            return "OK";
        }

        private Color DesignStateColor(string trafficState)
        {
            if (string.Equals(trafficState, "[GREEN] READY", StringComparison.Ordinal))
                return DesignHealthy;
            if (string.Equals(trafficState, "[AMBER] WAIT", StringComparison.Ordinal))
                return DesignWarning;
            return DesignDanger;
        }

        private double DesignRequiredFloor()
        {
            double internalDailyFloor = _dayAnchorBalance - EffectiveDailyWorstCaseCapUsd();
            double bufferedOfficialDailyFloor = OfficialDailyLimit() + EmergencyBufferUsd;
            double bufferedMaxLossFloor = MaxLossLimit() + MaxLossSafetyBufferUsd;
            return Math.Max(Math.Max(internalDailyFloor, bufferedOfficialDailyFloor), bufferedMaxLossFloor);
        }

        private string DesignEngineCheck()
        {
            return _lastEngineCheckUtc == DateTime.MinValue
                ? "WAIT"
                : _lastEngineCheckUtc.ToString("HH:mm:ss'Z'");
        }

        private string DesignTickAge(DateTime timestamp)
        {
            if (timestamp == DateTime.MinValue) return "WAIT";
            double age = Math.Max(0, (Server.TimeInUtc - timestamp).TotalSeconds);
            if (age < 60) return age.ToString("F0") + "s";
            if (age < 3600) return Math.Floor(age / 60).ToString("F0") + "m";
            return Math.Floor(age / 3600).ToString("F0") + "h";
        }

        private string DesignModuleState(string label, string symbolName, bool enabled, bool armed, bool completed, bool blocked)
        {
            if (!enabled) return "OFF";
            if (_signalRetries.ContainsKey(label)) return "RETRYING";
            DateTime london = ToUk(Server.TimeInUtc);
            DateTime eastern = ToEastern(Server.TimeInUtc);
            if (label == DaxALabel || label == DaxBLabel || label == DaxCLabel)
            {
                if (!IsDaxTradingDay(london)) return "BLACKOUT";
                if (label == DaxCLabel && london.DayOfWeek == DayOfWeek.Monday) return "MONDAY";
                int minute = london.Hour * 60 + london.Minute;
                // V34.ARSENAL: A1 expires at its 11:15 truncation, A2 and A3 at 09:15.
                int expiry = label == DaxALabel ? 675 : DaxBEntryEndTod;
                if (!completed && minute >= expiry) return "EXPIRED";
            }
            if (label == UkALabel)
            {
                if (london.DayOfWeek == DayOfWeek.Monday) return "MONDAY";
                if (london.Month == 12) return "DECEMBER";
                if (!completed && london.TimeOfDay > new TimeSpan(11, 0, 0)) return "EXPIRED";
            }
            if (label == NasCoreLabel)
            {
                if (eastern.DayOfWeek == DayOfWeek.Monday) return "MONDAY";
                if (_nasFullHolidays.Contains(_nasEtDay)) return "HOLIDAY";
                if (_nasHalfDays.Contains(_nasEtDay)) return "HALF-DAY";
                if (!completed && eastern.TimeOfDay > new TimeSpan(9, 45, 0).Add(TimeSpan.FromSeconds(NasDecisionGraceSeconds))) return "EXPIRED";
            }
            if (HasPosition(label, symbolName)) return "LIVE";
            if (HasPending(label, symbolName)) return "PENDING";
            if (blocked) return "HALTED";
            if (completed) return "DONE";
            return armed ? "ARMED" : "SCANNING";
        }

        private string DesignBookText()
        {
            var lines = new List<string> { "EXECUTION BOOK" };
            var designPositions = Positions.Where(x => IsMasterLabel(x.Label)).OrderBy(x => x.SymbolName).ToArray();
            var designOrders = PendingOrders.Where(x => IsMasterLabel(x.Label)).OrderBy(x => x.SymbolName).ToArray();
            int externalCount = designPositions.Count(x => !IsPersistentOrCurrentPosition(x)) +
                designOrders.Count(x => !IsPersistentOrCurrentPending(x));
            if (externalCount > 0)
                lines.Add("[RED] WARNING: " + externalCount +
                    " EXTERNAL / UNRECOGNISED ITEM(S) — AUTOMATIC ENTRIES BLOCKED");
            foreach (var p in designPositions.Take(5))
            {
                PositionMeta meta;
                _positionMeta.TryGetValue(p.Id, out meta);
                double initialRisk = meta != null ? meta.InitialRisk : ParseCommentNumber(p.Comment, "r=");
                double r = initialRisk > 0 ? p.NetProfit / initialRisk : 0;
                bool remainingVerified;
                double remainingRisk = DesignPositionRemainingRisk(p, out remainingVerified);
                Symbol symbol = Symbols.GetSymbol(p.SymbolName);
                string currentText = symbol == null
                    ? "UNVERIFIED"
                    : (p.TradeType == TradeType.Buy ? symbol.Bid : symbol.Ask).ToString("F2");
                string initialRiskText = initialRisk > 0 ? "$" + initialRisk.ToString("N0") : "UNVERIFIED";
                lines.Add("");
                lines.Add(p.SymbolName + "  " + (p.TradeType == TradeType.Buy ? "LONG" : "SHORT") +
                    "  " + p.Label + "  [" +
                    (IsPersistentOrCurrentPosition(p) ? "OWNED" : "EXTERNAL / REVIEW") + "]");
                lines.Add("Entry " + p.EntryPrice.ToString("F2") + "  Now " + currentText +
                          "  Size " + p.VolumeInUnits.ToString("N0"));
                lines.Add("SL " + (p.StopLoss.HasValue ? p.StopLoss.Value.ToString("F2") : "NONE") +
                          "  TP " + (p.TakeProfit.HasValue ? p.TakeProfit.Value.ToString("F2") : "OPEN"));
                lines.Add("P/L $" + p.NetProfit.ToString("N0") + "  " + r.ToString("+0.00;-0.00;0.00") + "R" +
                          "  Initial " + initialRiskText +
                          "  Remaining " + (remainingVerified ? "$" + remainingRisk.ToString("N0") : "UNVERIFIED"));
            }
            if (designPositions.Length > 5)
            {
                lines.Add("");
                lines.Add("+" + (designPositions.Length - 5) + " MORE POSITIONS - see Positions");
            }
            foreach (var order in designOrders.Take(3))
            {
                bool pendingRiskVerified;
                double livePendingRisk = DesignPendingRisk(order, out pendingRiskVerified);
                lines.Add("");
                lines.Add(order.SymbolName + "  " + (order.TradeType == TradeType.Buy ? "BUY LIMIT" : "SELL LIMIT") +
                    "  PENDING  [" +
                    (IsPersistentOrCurrentPending(order) ? "OWNED" : "EXTERNAL / REVIEW") + "]");
                lines.Add(order.Label + "  @" + order.TargetPrice.ToString("F2") +
                          "  Size " + order.VolumeInUnits.ToString("N0") +
                          "  Risk " + (pendingRiskVerified ? "$" + livePendingRisk.ToString("N0") : "UNVERIFIED"));
            }
            if (designOrders.Length > 3)
            {
                lines.Add("");
                lines.Add("+" + (designOrders.Length - 3) + " MORE ORDERS - see Orders");
            }
            if (lines.Count == 1)
            {
                lines.Add("");
                lines.Add("No open or pending ATLAS3 trades");
                lines.Add("Radar is monitoring enabled setups");
            }
            return string.Join("\n", lines);
        }

        private double DesignPositionRemainingRisk(Position position, out bool verified)
        {
            verified = false;
            Symbol symbol = Symbols.GetSymbol(position.SymbolName);
            if (symbol == null || !position.StopLoss.HasValue || symbol.PipSize <= 0 || symbol.PipValue <= 0)
                return 0;
            double currentExit = position.TradeType == TradeType.Buy ? symbol.Bid : symbol.Ask;
            double remainingDistance = position.TradeType == TradeType.Buy
                ? currentExit - position.StopLoss.Value
                : position.StopLoss.Value - currentExit;
            if (remainingDistance <= 0) return 0;
            verified = true;
            return remainingDistance / symbol.PipSize * symbol.PipValue * position.VolumeInUnits;
        }

        private double DesignPendingRisk(PendingOrder order, out bool verified)
        {
            verified = false;
            Symbol symbol = Symbols.GetSymbol(order.SymbolName);
            if (symbol == null || symbol.PipValue <= 0 || !order.StopLossPips.HasValue || order.StopLossPips.Value <= 0)
                return 0;
            double risk = order.StopLossPips.Value * symbol.PipValue * order.VolumeInUnits;
            PendingInfo info;
            if (_pendingInfo.TryGetValue(order.Id, out info))
                risk = Math.Max(risk, info.ApprovedRisk);
            verified = true;
            return risk;
        }

        private void DrawDesignHostLevels()
        {
            var active = new HashSet<string>();
            if (DrawHostTradeLevels)
            {
                foreach (var p in Positions.Where(x => IsMasterLabel(x.Label) && x.SymbolName == SymbolName))
                {
                    string root = "ATLAS3_DESIGN_POS_" + p.Id + "_";
                    string entry = root + "ENTRY";
                    Chart.DrawHorizontalLine(entry, p.EntryPrice, DesignEntry, 1, LineStyle.Dots);
                    active.Add(entry);
                    if (p.StopLoss.HasValue)
                    {
                        string sl = root + "SL";
                        Chart.DrawHorizontalLine(sl, p.StopLoss.Value, DesignStop, 2, LineStyle.Solid);
                        active.Add(sl);
                    }
                    if (p.TakeProfit.HasValue)
                    {
                        string tp = root + "TP";
                        Chart.DrawHorizontalLine(tp, p.TakeProfit.Value, DesignTarget, 2, LineStyle.Solid);
                        active.Add(tp);
                    }
                }
                foreach (var order in PendingOrders.Where(x => IsMasterLabel(x.Label) && x.SymbolName == SymbolName))
                {
                    string pending = "ATLAS3_DESIGN_PENDING_" + order.Id;
                    Chart.DrawHorizontalLine(pending, order.TargetPrice, DesignPending, 1, LineStyle.Dots);
                    active.Add(pending);
                }
            }
            foreach (string name in _designLevelObjects.Where(x => !active.Contains(x)).ToArray())
                Chart.RemoveObject(name);
            _designLevelObjects.Clear();
            foreach (string name in active) _designLevelObjects.Add(name);
        }

        private void ClearDesignObjects()
        {
            if (Chart == null) return;
            Chart.RemoveObject("ATLAS3_MASTER_HUD");
            Chart.RemoveObject("ATLAS3_DESIGN_CONTROL");
            Chart.RemoveObject("ATLAS3_DESIGN_ENGINES");
            Chart.RemoveObject("ATLAS3_DESIGN_BOOK");
            foreach (string name in _designLevelObjects.ToArray()) Chart.RemoveObject(name);
            _designLevelObjects.Clear();
        }

        private string DesignGauge(double used, double cap)
        {
            int filled = cap > 0 ? (int)Math.Round(12.0 * Math.Max(0, Math.Min(1, used / cap))) : 12;
            return "[" + new string('#', filled) + new string('-', 12 - filled) + "]";
        }

        private string DesignTrim(string value, int max)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= max) return value ?? "";
            return value.Substring(0, max - 3) + "...";
        }

        private void Journal(string eventName, string fields)
        {
            string line = "{\"ts\":\"" + Server.TimeInUtc.ToString("o") + "\",\"ver\":\"" + Version +
                          "\",\"profile\":\"" + ProfileCode + "\",\"run\":\"" + _runId +
                          "\",\"event\":\"" + Js(eventName) + "\"" +
                          (string.IsNullOrEmpty(fields) ? "" : "," + fields) + "}";
            Print("JOURNAL|{0}", line);
            if (!JournalOn || _journalDead || RunningMode != RunningMode.RealTime ||
                Server.TimeInUtc < _journalRetryAt) return;
            try
            {
                System.IO.Directory.CreateDirectory(_journalDir);
                string file = System.IO.Path.Combine(_journalDir,
                    "Atlas3_v34_arsenal_" + ProfileCode + "_acc" + Account.Number + "_" + PragueDate(Server.TimeInUtc).ToString("yyyy-MM-dd") + ".jsonl");
                System.IO.File.AppendAllText(file, line + Environment.NewLine);
                _journalFailures = 0;
                _journalRetryAt = DateTime.MinValue;
            }
            catch (Exception ex)
            {
                _journalFailures++;
                _journalRetryAt = Server.TimeInUtc.AddSeconds(Math.Min(60, 5 * _journalFailures));
                _journalDead = _journalFailures >= 5;
                Print("ATLAS journal write failed ({0}/5; next retry {1:o}): {2}",
                    _journalFailures, _journalRetryAt, ex.Message);
                if (_journalDead)
                    Print("*** ATLAS JOURNAL FAILED: five consecutive IO errors; review the red HUD before arming. ***");
            }
        }

        // =========================================================================================
        // SMALL HELPERS
        // =========================================================================================
        private double EffectivePortfolioRiskScale()
        {
            if (Stage == StageMode.Unconfigured) return 0;
            RefreshDynamicRiskDecision();
            return _currentDynamicRiskScale;
        }

        private double StaticPortfolioRiskScale()
        {
            if (!UseStageRiskScale) return PortfolioRiskScale;
            if (Stage == StageMode.Unconfigured) return 0;
            return IsFunded() ? FundedRiskScale : ChallengeRiskScale;
        }

        // =========================================================================================
        // FRONTIER RISK ARCHITECTURE — RED / ORANGE / GREEN
        // =========================================================================================
        // Each mode expresses the DAILY RISK BUDGET as a fraction of this profile's own official
        // daily-loss limit, so one enum yields 3.0%-based budgets on S1 and 5.0%-based budgets on S2
        // with no duplicated tables. The budget is then split across the two frontier-allocated core
        // engines only: DAX A1 takes 55%, and the NAS C1 day budget takes 60% which the inherited
        // aligned/misaligned multipliers resolve to 45% / 30% of the daily budget.
        //
        // The frontier raises the REQUEST. It never raises the approval: the atomic account gate,
        // the correlation throttle, the same-direction cluster cap, the near-floor halving, the
        // execution reserve and the capacity clip all still run afterwards and remain the binding
        // constraints. Nothing in this section can move an equity floor.
        private bool FrontierActive()
        {
            return FrontierMode != FrontierRiskMode.Off && Stage != StageMode.Unconfigured &&
                InitialBalance > 0;
        }

        private double FrontierModeFraction()
        {
            if (FrontierMode == FrontierRiskMode.Red) return FrontierRedFraction;
            if (FrontierMode == FrontierRiskMode.Orange) return FrontierOrangeFraction;
            if (FrontierMode == FrontierRiskMode.Green) return FrontierGreenFraction;
            return 0;
        }

        private string FrontierModeText()
        {
            return FrontierActive() ? FrontierMode.ToString().ToUpperInvariant() : "OFF";
        }

        private double FrontierDailyRiskBudgetUsd()
        {
            if (!FrontierActive()) return 0;
            return InitialBalance * FtmoDailyLossPct / 100.0 * FrontierModeFraction();
        }

        // Frontier allocations are stated as the risk that must SURVIVE the portfolio risk scale, so
        // the request handed to the gate is pre-divided by that scale and multiplied back inside
        // TryApproveProposal. A non-positive or non-finite scale yields 0, which the gate rejects as
        // effective_risk_zero — the frontier fails closed rather than guessing a denominator.
        private double FrontierScaledRequest(double approvedTarget)
        {
            double scale = EffectivePortfolioRiskScale();
            if (approvedTarget <= 0 || scale <= 0 || double.IsNaN(scale) || double.IsInfinity(scale))
                return 0;
            return approvedTarget / scale;
        }

        private double EffectiveDaxARiskRequest()
        {
            if (!FrontierActive()) return DaxARiskUsd;
            return FrontierScaledRequest(FrontierDailyRiskBudgetUsd() * FrontierDaxShare);
        }

        private double EffectiveNasReentryRisk()
        {
            if (!FrontierActive()) return NasReentryRiskUsd;
            return FrontierScaledRequest(FrontierDailyRiskBudgetUsd() * FrontierNasReentryShare);
        }

        // Gold sits OUTSIDE the daily budget by design: it is a non-correlated multi-day swing, not
        // part of the same-day DAX+NAS stopout constraint the budget is derived from. It is therefore
        // allocated as a flat percentage of the locked initial balance.
        private double EffectiveGoldRiskRequest()
        {
            if (!FrontierActive()) return GoldRiskUsd;
            return FrontierScaledRequest(InitialBalance * GoldSwingAllocationPct / 100.0);
        }

        // The internal daily worst-case cap becomes the frontier budget. The buffered official daily
        // floor and the max-loss floor are UNCHANGED and the gate still takes the maximum of all
        // three, so this can raise the internal cap toward the official limit but can never push the
        // binding floor past it. In RED the official floor plus the emergency buffer normally stays
        // the stricter of the two, and the gate clips to it — that is the intended fail-closed order.
        private double EffectiveDailyWorstCaseCapUsd()
        {
            if (!FrontierActive()) return DailyWorstCaseCapUsd;
            return FrontierDailyRiskBudgetUsd();
        }

        // The DAX daily profit lock compares REALISED dollars, so its ceiling must be built from the
        // approved (post-scale) risk rather than the pre-scale request. Under the frontier this keeps
        // the lock meaningful; with the static table it restores the comparison that was intended.
        private double DaxDailyMaxProfitUsd()
        {
            double scale = EffectivePortfolioRiskScale();
            if (scale <= 0 || double.IsNaN(scale) || double.IsInfinity(scale)) return 0;
            double maxDay = 0;
            if (DaxAEnabled) maxDay += EffectiveDaxARiskRequest() * scale * DaxATpPts / DaxASlPts;
            if (DaxBEnabled) maxDay += DaxBRiskUsd * scale * DaxBTpPts / DaxBSlPts;
            if (DaxCEnabled && !IsFunded()) maxDay += DaxCRiskUsd * scale * DaxCTpMultiple;
            return maxDay;
        }

        private double EffectiveNasC1DayBudget()
        {
            double budget = FrontierActive()
                ? FrontierScaledRequest(FrontierDailyRiskBudgetUsd() * FrontierNasBudgetShare)
                : NasDayBudgetUsd;
            return budget * (IsFunded() ? NasFundedAdoptionRiskPct / 100.0 : 1.0);
        }

        private double ReconstructedStageInitialBalance()
        {
            double realised = History
                .Where(x => x.ClosingTime >= _evaluationStartUtc)
                .Sum(x => x.NetProfit);
            return Account.Balance - realised - VerifiedCashAdjustmentsUsd;
        }

        private double TickSizeFor(Symbol symbol)
        {
            if (symbol == null) return 0.00001;
            if (symbol.TickSize > 0) return symbol.TickSize;
            return Math.Pow(10.0, -Math.Max(0, symbol.Digits));
        }

        private double NormalizePriceToTick(Symbol symbol, double price)
        {
            double tick = TickSizeFor(symbol);
            if (tick <= 0 || double.IsNaN(price) || double.IsInfinity(price)) return price;
            double normalized = Math.Round(price / tick, MidpointRounding.AwayFromZero) * tick;
            return Math.Round(normalized, symbol.Digits, MidpointRounding.AwayFromZero);
        }

        private string StrategyConfigurationManifest()
        {
            if (!string.IsNullOrEmpty(_cachedStrategyManifest)) return _cachedStrategyManifest;
            // Reflection prevents a newly added behavior parameter from being silently omitted from
            // the binding fingerprint. Only presentation and one-shot execution-rearm controls are
            // excluded. Calendar content/attestation is validated fail-closed at runtime but is not
            // position ownership: a routine calendar refresh must not orphan an open owned trade.
            var excluded = new HashSet<string>(StringComparer.Ordinal)
            {
                "ReleaseNotice", "RequiredHostNotice", "ShowHud", "ShowExecutionCockpit",
                "DrawHostTradeLevels", "RearmExecutionHalt", "ExecutionRearmNonce",
                "RestrictedEventsCsv", "RestrictedEventsVerifiedThroughText"
            };
            var fields = new List<string>
            {
                "V34.ARSENAL-STRATEGY-" + ReleaseToken, "profile=" + ProfileCode, "stage=Challenge", "initial=100000.00",
                "currency=USD", "dynamic=false", "labels=" + string.Join(",", MasterLabels)
            };
            foreach (var property in GetType().GetProperties()
                .Where(p => !excluded.Contains(p.Name) &&
                            p.GetCustomAttributes(typeof(ParameterAttribute), true).Length > 0)
                .OrderBy(p => p.Name, StringComparer.Ordinal))
            {
                object value = property.GetValue(this, null);
                var formattable = value as IFormattable;
                string textValue = formattable != null
                    ? formattable.ToString(null, CultureInfo.InvariantCulture)
                    : (value == null ? "" : value.ToString());
                fields.Add(property.Name + "=" + textValue);
            }
            _cachedStrategyManifest = string.Join("|", fields);
            return _cachedStrategyManifest;
        }

        private string StrategyConfigurationFingerprint()
        {
            if (string.IsNullOrEmpty(_cachedStrategyFingerprint))
                _cachedStrategyFingerprint = FingerprintText(StrategyConfigurationManifest());
            return _cachedStrategyFingerprint;
        }

        private string StrategyConfigurationReceiptHash()
        {
            if (string.IsNullOrEmpty(_cachedStrategyReceiptHash))
                _cachedStrategyReceiptHash = ReceiptChecksum(StrategyConfigurationManifest());
            return _cachedStrategyReceiptHash;
        }

        private string ConfigurationManifest()
        {
            string identity = !string.IsNullOrEmpty(_boundRuntimeIdentityFingerprint)
                ? _boundRuntimeIdentityFingerprint
                : FingerprintText(AccountIdentityText());
            string key = (ExpectedAccountNumber ?? "").Trim() + "|" + identity + "|" +
                (EvaluationStartText ?? "").Trim() + "|" + (DeploymentOwnerId ?? "").Trim() + "|" +
                (StageEpochId ?? "").Trim() + "|" + StrategyConfigurationFingerprint();
            if (string.Equals(key, _cachedDeploymentKey, StringComparison.Ordinal) &&
                !string.IsNullOrEmpty(_cachedDeploymentManifest))
                return _cachedDeploymentManifest;
            _cachedDeploymentKey = key;
            _cachedDeploymentManifest = "V34.ARSENAL-DEPLOYMENT-" + ReleaseToken + "|profile=" + ProfileCode +
                "|stage=Challenge|account=" + (ExpectedAccountNumber ?? "").Trim() +
                "|identity=" + identity +
                "|initial=100000.00|currency=USD" +
                "|evaluationStart=" + (EvaluationStartText ?? "").Trim() +
                "|owner=" + (DeploymentOwnerId ?? "").Trim() +
                "|epoch=" + (StageEpochId ?? "").Trim() +
                "|strategy=" + StrategyConfigurationFingerprint();
            _cachedDeploymentFingerprint = FingerprintText(_cachedDeploymentManifest);
            return _cachedDeploymentManifest;
        }

        private string ConfigurationFingerprint()
        {
            ConfigurationManifest();
            return _cachedDeploymentFingerprint;
        }

        private bool IsFunded()
        {
            return Stage == StageMode.Funded;
        }

        private static bool ValidIdentityToken(string value)
        {
            string token = (value ?? "").Trim();
            if (token.Length < 4 || token.Length > 8) return false;
            return token.All(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_');
        }

        private bool IsMasterLabel(string label)
        {
            return !string.IsNullOrEmpty(label) && MasterLabels.Contains(label);
        }

        private bool IsCurrentTradeComment(string comment)
        {
            string value = comment ?? "";
            if (!value.StartsWith("ATLAS3 " + ProfileCode + " ", StringComparison.Ordinal) ||
                value.Length > MaxTradeCommentLength || !IsAscii(value)) return false;
            string[] parts = value.Split(';');
            if (parts.Length != 9 || parts[8].Length != 0 ||
                parts[1] != "v=311" || !parts[2].StartsWith("c=", StringComparison.Ordinal) ||
                !parts[3].StartsWith("o=", StringComparison.Ordinal) ||
                !parts[4].StartsWith("e=", StringComparison.Ordinal) ||
                !parts[5].StartsWith("r=", StringComparison.Ordinal) ||
                !parts[6].StartsWith("q=", StringComparison.Ordinal) ||
                !parts[7].StartsWith("g=", StringComparison.Ordinal)) return false;
            string[] readable = parts[0].Split(' ');
            double risk, requested, reference;
            return readable.Length == 5 && readable[0] == "ATLAS3" &&
                   readable[1] == ProfileCode && IsCompactSetupDisplay(readable[2]) &&
                   (readable[3] == "BUY" || readable[3] == "SELL") &&
                   readable[4].Length == 8 && ValidIdentityToken(readable[4]) &&
                   ParseCompleteCommentToken(value, "c=").Length == 8 &&
                   ValidIdentityToken(ParseCompleteCommentToken(value, "o=")) &&
                   ValidIdentityToken(ParseCompleteCommentToken(value, "e=")) &&
                   double.TryParse(parts[5].Substring(2), NumberStyles.Float,
                       CultureInfo.InvariantCulture, out risk) && risk > 0 &&
                   double.TryParse(parts[6].Substring(2), NumberStyles.Float,
                       CultureInfo.InvariantCulture, out requested) && requested > 0 &&
                   double.TryParse(parts[7].Substring(2), NumberStyles.Float,
                       CultureInfo.InvariantCulture, out reference) && reference > 0 &&
                   !double.IsNaN(risk) && !double.IsInfinity(risk) &&
                   !double.IsNaN(requested) && !double.IsInfinity(requested) &&
                   !double.IsNaN(reference) && !double.IsInfinity(reference);
        }

        private static bool IsCompactSetupDisplay(string value)
        {
            return value == "DAX-A1" || value == "DAX-A2" || value == "DAX-A3" ||
                   value == "UK-B1" || value == "UK-B2" ||
                   value == "NAS-C1" || value == "NAS-C2" || value == "XAU-D1";
        }

        private bool IsOwnedComment(string comment)
        {
            string value = comment ?? "";
            return _bindingValid && ValidIdentityToken(DeploymentOwnerId) && ValidIdentityToken(StageEpochId) &&
                   IsCurrentTradeComment(value) &&
                   ParseCompleteCommentToken(value, "c=") == ConfigurationFingerprint() &&
                   ParseCompleteCommentToken(value, "o=") == (DeploymentOwnerId ?? "").Trim() &&
                   ParseCompleteCommentToken(value, "e=") == (StageEpochId ?? "").Trim();
        }

        private bool IsOwnedPosition(Position position)
        {
            return position != null && IsMasterLabel(position.Label) &&
                   IsLedgerOwnedPosition(position);
        }

        private bool IsOwnedPending(PendingOrder order)
        {
            return order != null && IsMasterLabel(order.Label) &&
                   IsLedgerOwnedPending(order);
        }

        // Persistent ownership is an exact durable-ledger match. Current-run ownership is an exact ID
        // learned from a successful TradeResult or a tightly correlated callback while this instance
        // owns the entry gate. In-flight candidates may suppress premature external classification,
        // but they never grant close/modify/cancel authority.
        private bool IsPersistentOrCurrentPosition(Position position)
        {
            return position != null && IsMasterLabel(position.Label) &&
                   (IsOwnedPosition(position) || _currentRunCreatedPositionIds.Contains(position.Id));
        }

        private bool IsPersistentOrCurrentPending(PendingOrder order)
        {
            return order != null && IsMasterLabel(order.Label) &&
                   (IsOwnedPending(order) || _currentRunCreatedPendingIds.Contains(order.Id));
        }

        // Recognition is deliberately non-authoritative. It prevents one exact, recent and unique
        // current-run pending fill from being misclassified as external while cTrader has exposed
        // the Position but has not yet delivered PendingOrders.Filled. Management authority still
        // requires the existing result/event/ledger promotion paths.
        private bool IsRuntimeRecognizedPosition(Position position)
        {
            return IsPersistentOrCurrentPosition(position) ||
                   PositionCoreMatchesMutation(position, _activeEntryMutation) ||
                   IsKnownPendingFillCandidate(position);
        }

        private bool IsRuntimeRecognizedPending(PendingOrder order)
        {
            return IsPersistentOrCurrentPending(order) ||
                   PendingCoreMatchesMutation(order, _activeEntryMutation);
        }

        private bool CanManagePosition(Position position)
        {
            return BindingAuthorityCurrent() && IsPersistentOrCurrentPosition(position);
        }

        private bool CanManagePending(PendingOrder order)
        {
            return BindingAuthorityCurrent() && IsPersistentOrCurrentPending(order);
        }

        private bool ActiveMutationFresh(ActiveEntryMutation mutation)
        {
            if (mutation == null || mutation.Proposal == null || !_gateBusy ||
                !ReferenceEquals(_activeEntryMutation, mutation) ||
                mutation.StartedMonotonicTimestamp <= 0) return false;
            double elapsedMs = MonotonicElapsedMilliseconds(mutation.StartedMonotonicTimestamp);
            return elapsedMs >= 0 && elapsedMs <= ActiveEntryMutationMaxSeconds * 1000.0;
        }

        private bool VolumeMatches(Symbol symbol, double actual, double expected)
        {
            if (symbol == null || actual <= 0 || expected <= 0) return false;
            double tolerance = Math.Max(symbol.VolumeInUnitsStep * 0.51, 1e-9);
            return Math.Abs(actual - expected) <= tolerance;
        }

        private bool FilledVolumeMatches(Symbol symbol, double actual, double requested)
        {
            if (symbol == null || actual <= 0 || requested <= 0) return false;
            double tolerance = Math.Max(symbol.VolumeInUnitsStep * 0.51, 1e-9);
            // A broker-confirmed partial fill remains this mutation's exposure. The existing router
            // independently records partial_fill as a hard execution incident.
            return actual <= requested + tolerance;
        }

        private bool CommentCompatibleWithExpected(string actualComment, string expectedComment)
        {
            string actual = actualComment ?? "";
            string expected = expectedComment ?? "";
            if (string.Equals(actual, expected, StringComparison.Ordinal)) return true;
            // Empty or an exact observed prefix is broker degradation, never ownership authority.
            // Core mutation fields must match independently before this compatibility is consulted.
            return actual.Length == 0 || expected.StartsWith(actual, StringComparison.Ordinal);
        }

        private bool HasExplicitContradictoryCurrentAuth(string actualComment, string expectedComment)
        {
            string actual = actualComment ?? "";
            string expected = expectedComment ?? "";
            // A broker may cut anywhere, including halfway through an authority value. Compare only
            // complete semicolon-terminated tokens; an incomplete suffix grants no authority and
            // cannot create a false contradiction. Any completed mismatch or duplicate key remains
            // fail-closed, even when both completed values happen to match the expected value.
            foreach (string key in new[] { "v=", "c=", "o=", "e=" })
            {
                string expectedValue = ParseCompleteCommentToken(expected, key);
                int completedMatches = 0;
                int start = 0;
                while (start < actual.Length)
                {
                    int end = actual.IndexOf(';', start);
                    if (end < 0) break;
                    int length = end - start;
                    if (length >= key.Length &&
                        string.CompareOrdinal(actual, start, key, 0, key.Length) == 0)
                    {
                        completedMatches++;
                        if (completedMatches > 1 ||
                            !string.Equals(actual.Substring(start + key.Length, length - key.Length).Trim(),
                                expectedValue, StringComparison.Ordinal)) return true;
                    }
                    start = end + 1;
                }
            }
            return false;
        }

        private bool PositionCoreMatchesMutation(Position position, ActiveEntryMutation mutation)
        {
            if (position == null || !ActiveMutationFresh(mutation)) return false;
            TradeProposal proposal = mutation.Proposal;
            if (mutation.PositionId.HasValue && mutation.PositionId.Value != position.Id) return false;
            if (position.Label != proposal.Label || position.SymbolName != proposal.Symbol.Name ||
                position.TradeType != proposal.Side) return false;
            if (!FilledVolumeMatches(proposal.Symbol, position.VolumeInUnits, mutation.Units)) return false;
            if (position.EntryTime < mutation.StartedUtc.AddSeconds(-5) ||
                position.EntryTime > Server.TimeInUtc.AddSeconds(5)) return false;
            return true;
        }

        private bool PendingCoreMatchesMutation(PendingOrder order, ActiveEntryMutation mutation)
        {
            if (order == null || !ActiveMutationFresh(mutation)) return false;
            TradeProposal proposal = mutation.Proposal;
            if (mutation.PendingOrderId.HasValue && mutation.PendingOrderId.Value != order.Id) return false;
            double priceTolerance = Math.Max(TickSizeFor(proposal.Symbol) * 0.51, 1e-9);
            DateTime submittedUtc = AsUtc(order.SubmittedTime);
            return order.Label == proposal.Label && order.SymbolName == proposal.Symbol.Name &&
                   order.TradeType == proposal.Side &&
                   VolumeMatches(proposal.Symbol, order.VolumeInUnits, mutation.Units) &&
                   Math.Abs(order.TargetPrice - mutation.DecisionPrice) <= priceTolerance &&
                   submittedUtc >= AsUtc(mutation.StartedUtc).AddSeconds(-2) &&
                   submittedUtc <= Server.TimeInUtc.AddSeconds(5);
        }

        private bool PendingMatchesMutation(PendingOrder order, ActiveEntryMutation mutation)
        {
            return PendingCoreMatchesMutation(order, mutation) &&
                   CommentCompatibleWithExpected(order.Comment, mutation.ExpectedComment);
        }

        // A successful PlaceLimitOrder call may return the filled Position before cTrader publishes
        // PendingOrders.Filled. Keep one short-lived, one-shot receipt so that delayed terminal event
        // is recognized as a replay of the already ledger-owned fill rather than foreign exposure.
        private void RememberDirectLimitTerminalExpectation(ActiveEntryMutation mutation,
            Position position)
        {
            if (mutation == null || mutation.Proposal == null || position == null) return;
            PruneDirectLimitTerminalExpectations();
            _directLimitTerminalExpectations[position.Id] = new DirectLimitTerminalExpectation
            {
                PositionId = position.Id,
                Label = mutation.Proposal.Label,
                SymbolName = mutation.Proposal.Symbol.Name,
                Side = mutation.Proposal.Side,
                Units = mutation.Units,
                TargetPrice = mutation.DecisionPrice,
                StartedUtc = AsUtc(mutation.StartedUtc),
                ResultUtc = AsUtc(Server.TimeInUtc),
                ExpectedComment = mutation.ExpectedComment ?? "",
                ExpiresMonotonicTicks = MonotonicDeadlineAfterSeconds(
                    BrokerMutationEventTimeoutSeconds)
            };
        }

        private void PruneDirectLimitTerminalExpectations()
        {
            if (_directLimitTerminalExpectations.Count == 0) return;
            long nowTicks = Stopwatch.GetTimestamp();
            foreach (long positionId in _directLimitTerminalExpectations
                .Where(x => x.Value == null || x.Value.ExpiresMonotonicTicks <= 0 ||
                            nowTicks > x.Value.ExpiresMonotonicTicks)
                .Select(x => x.Key).ToList())
                _directLimitTerminalExpectations.Remove(positionId);
        }

        private void MarkDirectLimitTerminalPositionClosed(Position position)
        {
            if (position == null) return;
            DirectLimitTerminalExpectation expected;
            if (!_directLimitTerminalExpectations.TryGetValue(position.Id, out expected) ||
                expected == null || expected.ExpiresMonotonicTicks <= 0 ||
                Stopwatch.GetTimestamp() > expected.ExpiresMonotonicTicks) return;
            Symbol symbol = Symbols.GetSymbol(expected.SymbolName);
            if (symbol == null) return;
            double volumeTolerance = Math.Max(symbol.VolumeInUnitsStep * 0.51, 1e-9);
            DateTime entryUtc = AsUtc(position.EntryTime);
            bool exact = _currentRunCreatedPositionIds.Contains(position.Id) &&
                IsLedgerOwnedPosition(position) && position.Id == expected.PositionId &&
                position.Label == expected.Label && position.SymbolName == expected.SymbolName &&
                position.TradeType == expected.Side &&
                Math.Abs(position.VolumeInUnits - expected.Units) <= volumeTolerance &&
                entryUtc >= expected.StartedUtc.AddSeconds(-2) &&
                entryUtc <= expected.ResultUtc.AddSeconds(5) &&
                !HasExplicitContradictoryCurrentAuth(position.Comment, expected.ExpectedComment);
            if (exact) expected.ClosedSeen = true;
        }

        private bool ConsumeDirectLimitTerminalReplay(PendingOrderFilledEventArgs args)
        {
            if (args == null || args.PendingOrder == null || args.Position == null) return false;
            DirectLimitTerminalExpectation expected;
            if (!_directLimitTerminalExpectations.TryGetValue(args.Position.Id, out expected) ||
                expected == null) return false;
            if (expected.ExpiresMonotonicTicks <= 0 ||
                Stopwatch.GetTimestamp() > expected.ExpiresMonotonicTicks)
            {
                _directLimitTerminalExpectations.Remove(args.Position.Id);
                return false;
            }

            PendingOrder order = args.PendingOrder;
            Position position = args.Position;
            Symbol symbol = Symbols.GetSymbol(expected.SymbolName);
            if (symbol == null) return false;
            double volumeTolerance = Math.Max(symbol.VolumeInUnitsStep * 0.51, 1e-9);
            double priceTolerance = Math.Max(TickSizeFor(symbol) * 0.51, 1e-9);
            DateTime submittedUtc = AsUtc(order.SubmittedTime);
            DateTime entryUtc = AsUtc(position.EntryTime);
            bool liveAuthority = _currentRunCreatedPositionIds.Contains(position.Id) &&
                IsLedgerOwnedPosition(position);
            bool exact = (liveAuthority || expected.ClosedSeen) &&
                position.Id == expected.PositionId &&
                position.Label == expected.Label && position.SymbolName == expected.SymbolName &&
                position.TradeType == expected.Side &&
                Math.Abs(position.VolumeInUnits - expected.Units) <= volumeTolerance &&
                entryUtc >= expected.StartedUtc.AddSeconds(-2) &&
                entryUtc <= expected.ResultUtc.AddSeconds(5) &&
                order.Label == expected.Label && order.SymbolName == expected.SymbolName &&
                order.TradeType == expected.Side &&
                Math.Abs(order.VolumeInUnits - expected.Units) <= volumeTolerance &&
                Math.Abs(order.TargetPrice - expected.TargetPrice) <= priceTolerance &&
                submittedUtc >= expected.StartedUtc.AddSeconds(-2) &&
                submittedUtc <= expected.ResultUtc.AddSeconds(5) &&
                !HasExplicitContradictoryCurrentAuth(order.Comment, expected.ExpectedComment) &&
                !HasExplicitContradictoryCurrentAuth(position.Comment, expected.ExpectedComment);
            if (!exact) return false;

            _directLimitTerminalExpectations.Remove(position.Id);
            Journal("BROKER_CALLBACK_DIAGNOSTIC",
                "\"kind\":\"delayed_direct_limit_fill_replay\",\"pending_id\":" + order.Id +
                ",\"position_id\":" + position.Id + ",\"label\":\"" + Js(position.Label) + "\"");
            return true;
        }

        private PendingInfo PendingInfoFromMutation(ActiveEntryMutation mutation)
        {
            return new PendingInfo
            {
                Proposal = mutation.Proposal,
                ApprovedRisk = mutation.ApprovedRisk,
                Units = mutation.Units,
                DecisionPrice = mutation.DecisionPrice,
                Spread = mutation.Spread,
                ExpectedComment = mutation.ExpectedComment,
                RegisteredMonotonicTicks = mutation.StartedMonotonicTimestamp > 0
                    ? mutation.StartedMonotonicTimestamp
                    : Stopwatch.GetTimestamp()
            };
        }

        private bool RegisterSuccessfulMarketResult(Position position)
        {
            ActiveEntryMutation mutation = _activeEntryMutation;
            // The successful broker result is the authoritative current-run ID. Comment alteration
            // is diagnosed but does not invalidate the exact returned position identity.
            if (!PositionCoreMatchesMutation(position, mutation))
            {
                RecordExecutionIncident("market_result_not_exact_active_mutation", 0, 0);
                HaltStartup("market result did not exactly match the active Atlas entry mutation; exposure preserved");
                return false;
            }
            if (mutation.PositionId.HasValue && mutation.PositionId.Value != position.Id)
            {
                RecordExecutionIncident("market_callback_result_position_id_mismatch", 0, 0);
                HaltStartup("market callback/result position IDs disagreed; exposure preserved");
                return false;
            }
            mutation.PositionId = position.Id;
            _currentRunCreatedPositionIds.Add(position.Id);
            _expectedPositionComments[position.Id] = mutation.ExpectedComment ?? "";
            if (mutation.Proposal.Style == EntryStyle.LimitAtMid)
                mutation.PendingFilledSeen = true;
            if (!_positionMeta.ContainsKey(position.Id))
                _positionMeta[position.Id] = NewPositionMeta(
                    mutation.Proposal, mutation.ApprovedRisk, mutation.DecisionPrice, position);
            RecordPositionCommentDiagnostic(position, "market_result");
            if (!CommitPositionOwnership(position, mutation.Proposal, mutation.ApprovedRisk,
                    mutation.DecisionPrice, mutation.ExpectedComment, null))
            {
                FailClosedForOwnershipLedger("position_commit_failed", position, null);
                return false;
            }
            if (HasExplicitContradictoryCurrentAuth(position.Comment, mutation.ExpectedComment))
            {
                FailClosedForOwnershipLedger("position_comment_authority_contradiction", position, null);
                return false;
            }
            return true;
        }

        private bool RegisterSuccessfulPendingResult(PendingOrder order)
        {
            ActiveEntryMutation mutation = _activeEntryMutation;
            // As with market results, an exact successful returned order ID is authoritative even
            // when the broker has shortened or altered the comment.
            if (!PendingCoreMatchesMutation(order, mutation))
            {
                RecordExecutionIncident("pending_result_not_exact_active_mutation", 0, 0);
                HaltStartup("pending result did not exactly match the active Atlas entry mutation");
                return false;
            }
            if (mutation.PendingOrderId.HasValue && mutation.PendingOrderId.Value != order.Id)
            {
                RecordExecutionIncident("pending_callback_result_order_id_mismatch", 0, 0);
                HaltStartup("pending callback/result order IDs disagreed");
                return false;
            }
            if (mutation.PendingCancelledSeen)
            {
                RecordExecutionIncident("pending_cancelled_before_placement_returned", 0, 0);
                HaltStartup("pending order cancelled before placement result reconciliation");
                return false;
            }
            mutation.PendingOrderId = order.Id;
            if (mutation.PendingFillCallbackSeen)
            {
                if (!mutation.PositionId.HasValue || !mutation.PendingFillHandledSuccessfully)
                {
                    RecordExecutionIncident("pending_fill_callback_without_position_reconciliation", 0, 0);
                    HaltStartup("pending fill callback completed without an exact position reconciliation");
                    return false;
                }
                return true;
            }
            _currentRunCreatedPendingIds.Add(order.Id);
            _expectedPendingComments[order.Id] = mutation.ExpectedComment ?? "";
            if (!_pendingInfo.ContainsKey(order.Id))
                _pendingInfo[order.Id] = PendingInfoFromMutation(mutation);
            RecordPendingCommentDiagnostic(order, "pending_result");
            if (!CommitPendingOwnership(order, mutation))
            {
                FailClosedForOwnershipLedger("pending_commit_failed", null, order);
                return false;
            }
            if (HasExplicitContradictoryCurrentAuth(order.Comment, mutation.ExpectedComment))
            {
                FailClosedForOwnershipLedger("pending_comment_authority_contradiction", null, order);
                return false;
            }
            return true;
        }

        private void GuardFailedBrokerResultWithEarlyExposure(string reason)
        {
            ActiveEntryMutation mutation = _activeEntryMutation;
            if (mutation == null ||
                (!mutation.PositionId.HasValue && !mutation.PendingOrderId.HasValue &&
                 !mutation.PendingFilledSeen && !mutation.PendingFillCallbackSeen))
                return;
            RecordExecutionIncident(reason, 0, 0);
            HaltStartup("broker result failed after an entry callback exposed a matching Atlas mutation; exposure preserved");
        }

        private bool TryPromotePendingFromActiveMutation(PendingOrder order, string source)
        {
            if (IsPersistentOrCurrentPending(order)) return true;
            ActiveEntryMutation mutation = _activeEntryMutation;
            // Filled/Cancelled is a terminal broker event while this process owns the single entry
            // gate. Exact target price, submitted time, label/symbol/side/volume and the one active
            // mutation make the pending ID causative even if the venue replaces its comment with
            // arbitrary text. Explicit complete V31.1 authority-token contradictions remain a hard veto.
            if (!PendingCoreMatchesMutation(order, mutation) ||
                HasExplicitContradictoryCurrentAuth(order.Comment, mutation.ExpectedComment)) return false;
            if (mutation.PendingOrderId.HasValue && mutation.PendingOrderId.Value != order.Id) return false;
            mutation.PendingOrderId = order.Id;
            _currentRunCreatedPendingIds.Add(order.Id);
            _expectedPendingComments[order.Id] = mutation.ExpectedComment ?? "";
            if (!_pendingInfo.ContainsKey(order.Id))
                _pendingInfo[order.Id] = PendingInfoFromMutation(mutation);
            Journal("OWNERSHIP_RECONCILED",
                "\"kind\":\"pending_inflight\",\"source\":\"" + Js(source) +
                "\",\"pending_id\":" + order.Id + ",\"label\":\"" + Js(order.Label) + "\"");
            return true;
        }

        private bool PendingInfoMatchesPosition(PendingInfo info, Position position)
        {
            if (info == null || info.Proposal == null || position == null) return false;
            TradeProposal proposal = info.Proposal;
            return position.Label == proposal.Label && position.SymbolName == proposal.Symbol.Name &&
                   position.TradeType == proposal.Side &&
                   FilledVolumeMatches(proposal.Symbol, position.VolumeInUnits, info.Units);
        }

        private bool PositionRecentlyOpened(Position position)
        {
            if (position == null || position.EntryTime == DateTime.MinValue) return false;
            return position.EntryTime >= Server.TimeInUtc.AddSeconds(-ActiveEntryMutationMaxSeconds) &&
                   position.EntryTime <= Server.TimeInUtc.AddSeconds(5);
        }

        private bool PendingOrderMatchesInfo(PendingOrder order, PendingInfo info)
        {
            if (order == null || info == null || info.Proposal == null) return false;
            TradeProposal proposal = info.Proposal;
            return order.Label == proposal.Label && order.SymbolName == proposal.Symbol.Name &&
                   order.TradeType == proposal.Side &&
                   VolumeMatches(proposal.Symbol, order.VolumeInUnits, info.Units);
        }

        private bool IsKnownPendingFillCandidate(Position position)
        {
            if (position == null || !PositionRecentlyOpened(position)) return false;
            int matches = _pendingInfo.Count(pair =>
                _currentRunCreatedPendingIds.Contains(pair.Key) &&
                pair.Value != null && pair.Value.RegisteredMonotonicTicks > 0 &&
                PendingInfoMatchesPosition(pair.Value, position) &&
                CommentCompatibleWithExpected(position.Comment, pair.Value.ExpectedComment));
            return matches == 1;
        }

        private bool RegisterPositionFromPendingFill(Position position, PendingOrder filledOrder,
            PendingInfo info)
        {
            if (filledOrder == null || info == null || !_pendingInfo.ContainsKey(filledOrder.Id) ||
                !IsPersistentOrCurrentPending(filledOrder) ||
                !PendingOrderMatchesInfo(filledOrder, info) ||
                !PositionRecentlyOpened(position) || !PendingInfoMatchesPosition(info, position))
                return false;
            ActiveEntryMutation mutation = _activeEntryMutation;
            if (mutation != null && mutation.PendingOrderId.HasValue &&
                mutation.PendingOrderId.Value == filledOrder.Id)
            {
                if (mutation.PositionId.HasValue && mutation.PositionId.Value != position.Id) return false;
                mutation.PositionId = position.Id;
                mutation.PendingFilledSeen = true;
            }
            _currentRunCreatedPositionIds.Add(position.Id);
            _expectedPositionComments[position.Id] = info.ExpectedComment ?? "";
            _positionMeta[position.Id] = NewPositionMeta(
                info.Proposal, info.ApprovedRisk, info.DecisionPrice, position);
            RecordPositionCommentDiagnostic(position, "pending_fill");
            if (!CommitPositionOwnership(position, info.Proposal, info.ApprovedRisk,
                    info.DecisionPrice, info.ExpectedComment, filledOrder.Id))
            {
                FailClosedForOwnershipLedger("pending_fill_position_commit_failed", position, filledOrder);
                return false;
            }
            if (HasExplicitContradictoryCurrentAuth(position.Comment, info.ExpectedComment) ||
                HasExplicitContradictoryCurrentAuth(filledOrder.Comment, info.ExpectedComment))
            {
                FailClosedForOwnershipLedger("pending_fill_comment_authority_contradiction", position, filledOrder);
                return false;
            }
            return true;
        }

        private bool TryCompleteImmediateLimitFill(TradeProposal proposal, double approvedRisk,
            double units, double decisionPrice)
        {
            ActiveEntryMutation mutation = _activeEntryMutation;
            if (mutation == null || !mutation.PositionId.HasValue ||
                mutation.Proposal == null || mutation.Proposal.Style != EntryStyle.LimitAtMid)
                return false;
            if (mutation.PendingFillHandledSuccessfully) return true;

            Position position = Positions.FirstOrDefault(x => x.Id == mutation.PositionId.Value);
            if (position == null || !IsPersistentOrCurrentPosition(position)) return false;
            double fillStep = Math.Max(proposal.Symbol.VolumeInUnitsStep, 1.0);
            if (position.VolumeInUnits + fillStep * 0.5 < units)
                RecordExecutionIncident("partial_fill", 0, 0);
            string protectionReason;
            if (!ValidPositionProtection(position, out protectionReason))
            {
                RecordExecutionIncident("immediate_limit_fill_invalid_protection_" + protectionReason, 0, 0);
                EmergencyClose(position, "immediate_limit_fill_" + protectionReason);
                HaltStartup("immediate limit fill arrived without valid broker protection");
                return false;
            }
            if (EntryHaltActive())
            {
                RecordExecutionIncident("immediate_limit_fill_during_global_halt", 0, 0);
                EmergencyClose(position, "immediate_limit_fill_during_halt");
                return false;
            }

            RecordExecutionQuality(proposal, decisionPrice, position.EntryPrice, 0, false);
            if (EntryHaltActive())
            {
                RecordExecutionIncident("immediate_limit_fill_quality_halt", 0, 0);
                EmergencyClose(position, "immediate_limit_fill_quality_halt");
                return false;
            }
            if (!mutation.EntryJournalWritten)
            {
                Journal("ENTRY",
                    "\"label\":\"" + Js(proposal.Label) + "\",\"engine\":\"" + proposal.Engine +
                    "\",\"setup\":\"" + proposal.Setup + "\",\"type\":\"limit_immediate\"" +
                    ",\"order_id\":" + (mutation.PendingOrderId ?? 0) +
                    ",\"position_id\":" + position.Id + ",\"fill\":" + Jn(position.EntryPrice) +
                    ",\"approved_risk\":" + Jn(approvedRisk) +
                    ",\"dynamic_tier\":\"" + Js(_currentDynamicRiskTier) + "\"" +
                    ",\"dynamic_scale\":" + Jn(_currentDynamicRiskScale) +
                    ",\"initial_risk\":" + Jn(_positionMeta[position.Id].InitialRisk) +
                    ",\"units\":" + Jn(units));
                mutation.EntryJournalWritten = true;
            }
            mutation.PendingFilledSeen = true;
            mutation.PendingFillHandledSuccessfully = true;
            return true;
        }

        private string SignalIdForPosition(Position position)
        {
            PositionMeta meta;
            if (position != null && _positionMeta.TryGetValue(position.Id, out meta) &&
                !string.IsNullOrEmpty(meta.SignalId)) return meta.SignalId;
            OwnershipLedgerRecord ledger;
            if (position != null && _ownershipLedgerLoaded && !_ownershipLedgerInvalid &&
                _ownershipLedger.TryGetValue(OwnershipLedgerKey("POSITION", position.Id), out ledger) &&
                OwnershipLedgerMatchesPosition(ledger, position) && !string.IsNullOrEmpty(ledger.SignalId))
                return ledger.SignalId;
            return position == null ? "" : ParseCommentToken(position.Comment ?? "", "s=");
        }

        private string SignalIdForPending(PendingOrder order)
        {
            PendingInfo info;
            if (order != null && _pendingInfo.TryGetValue(order.Id, out info) &&
                info != null && info.Proposal != null && !string.IsNullOrEmpty(info.Proposal.SignalId))
                return info.Proposal.SignalId;
            OwnershipLedgerRecord ledger;
            if (order != null && _ownershipLedgerLoaded && !_ownershipLedgerInvalid &&
                _ownershipLedger.TryGetValue(OwnershipLedgerKey("PENDING", order.Id), out ledger) &&
                OwnershipLedgerMatchesPending(ledger, order) && !string.IsNullOrEmpty(ledger.SignalId))
                return ledger.SignalId;
            return order == null ? "" : ParseCommentToken(order.Comment ?? "", "s=");
        }

        private void RecordPositionCommentDiagnostic(Position position, string source)
        {
            if (position == null || !_currentRunCreatedPositionIds.Contains(position.Id)) return;
            string expectedValue;
            bool hasExpected = _expectedPositionComments.TryGetValue(position.Id, out expectedValue);
            string observedValue = position.Comment ?? "";
            bool FullyRoundTrip = hasExpected &&
                string.Equals(expectedValue ?? "", observedValue, StringComparison.Ordinal);
            if (FullyRoundTrip) return;
            // An early exact callback must not consume the one-shot. Record only when degradation
            // is actually observed, so a later callback/result with a shortened comment is visible.
            if (!_positionCommentDiagnosticsWritten.Add(position.Id)) return;
            bool persistentAuthValid = IsOwnedPosition(position);
            Journal("BROKER_COMMENT_DEGRADED",
                "\"kind\":\"position\",\"source\":\"" + Js(source) +
                "\",\"position_id\":" + position.Id + ",\"label\":\"" + Js(position.Label) +
                "\",\"expected_length\":" + (expectedValue ?? "").Length +
                ",\"observed_length\":" + observedValue.Length +
                ",\"expected_hash\":\"" + ReceiptChecksum(expectedValue ?? "").Substring(0, 12) +
                "\",\"observed_hash\":\"" + ReceiptChecksum(observedValue).Substring(0, 12) +
                "\",\"persistent_auth_valid\":" + Bool(persistentAuthValid) +
                ",\"prefix_ok\":" + Bool((expectedValue ?? "").StartsWith(observedValue, StringComparison.Ordinal)) +
                ",\"config_ok\":" + Bool(ParseCompleteCommentToken(observedValue, "c=") == ConfigurationFingerprint()) +
                ",\"owner_ok\":" + Bool(ParseCompleteCommentToken(observedValue, "o=") == (DeploymentOwnerId ?? "").Trim()) +
                ",\"epoch_ok\":" + Bool(ParseCompleteCommentToken(observedValue, "e=") == (StageEpochId ?? "").Trim()) + "");
        }

        private void RecordPendingCommentDiagnostic(PendingOrder order, string source)
        {
            if (order == null || !_currentRunCreatedPendingIds.Contains(order.Id)) return;
            string expectedValue;
            bool hasExpected = _expectedPendingComments.TryGetValue(order.Id, out expectedValue);
            string observedValue = order.Comment ?? "";
            bool FullyRoundTrip = hasExpected &&
                string.Equals(expectedValue ?? "", observedValue, StringComparison.Ordinal);
            if (FullyRoundTrip) return;
            if (!_pendingCommentDiagnosticsWritten.Add(order.Id)) return;
            bool persistentAuthValid = IsOwnedPending(order);
            Journal("BROKER_COMMENT_DEGRADED",
                "\"kind\":\"pending\",\"source\":\"" + Js(source) +
                "\",\"pending_id\":" + order.Id + ",\"label\":\"" + Js(order.Label) +
                "\",\"expected_length\":" + (expectedValue ?? "").Length +
                ",\"observed_length\":" + observedValue.Length +
                ",\"expected_hash\":\"" + ReceiptChecksum(expectedValue ?? "").Substring(0, 12) +
                "\",\"observed_hash\":\"" + ReceiptChecksum(observedValue).Substring(0, 12) +
                "\",\"persistent_auth_valid\":" + Bool(persistentAuthValid) +
                ",\"prefix_ok\":" + Bool((expectedValue ?? "").StartsWith(observedValue, StringComparison.Ordinal)) +
                ",\"config_ok\":" + Bool(ParseCompleteCommentToken(observedValue, "c=") == ConfigurationFingerprint()) +
                ",\"owner_ok\":" + Bool(ParseCompleteCommentToken(observedValue, "o=") == (DeploymentOwnerId ?? "").Trim()) +
                ",\"epoch_ok\":" + Bool(ParseCompleteCommentToken(observedValue, "e=") == (StageEpochId ?? "").Trim()) + "");
        }

        private bool IsLegacyLabel(string label)
        {
            return !string.IsNullOrEmpty(label) && LegacyLabels.Contains(label);
        }

        private string LegacyForNewLabel(string label)
        {
            if (label == DaxALabel) return "ASetup";
            if (label == DaxBLabel) return "BSetup";
            if (label == DaxCLabel) return "CSetup";
            if (label == UkALabel) return "Coil30";
            if (label == UkBLabel) return "RefBreakX";
            if (label == NasCoreLabel) return "Bell Core";
            if (label == NasReLabel) return "Bell-2RE";
            return "";
        }

        private string LabelEngine(string label)
        {
            if (label == DaxALabel || label == DaxBLabel || label == DaxCLabel) return "DAX";
            if (label == UkALabel || label == UkBLabel) return "UK";
            if (label == GoldLabel) return "GOLD";
            return "NAS";
        }

        private string LabelSetup(string label)
        {
            if (label == DaxALabel) return "A1";
            if (label == DaxBLabel) return "A2";
            if (label == DaxCLabel) return "A3";
            if (label == UkALabel) return "B1";
            if (label == UkBLabel) return "B2";
            if (label == NasCoreLabel) return "C1";
            if (label == GoldLabel) return "D1";
            return "C2";
        }

        private void HaltStartup(string reason)
        {
            _startupHalt = true;
            if (RunningMode == RunningMode.RealTime)
            {
                _deploymentArmed = false;
                if (_bindingValid) _deploymentState = DeploymentState.Blocked;
            }
            if (string.IsNullOrEmpty(_haltReason)) _haltReason = reason;
            if (!_startupReasons.Contains(reason))
            {
                _startupReasons.Add(reason);
                Print("*** ATLAS STARTUP HALT: {0}. ALL NEW ENTRIES BLOCKED. ***", reason);
            }
        }

        private DateTime ToEastern(DateTime utc)
        {
            return _eastern == null ? utc : TimeZoneInfo.ConvertTimeFromUtc(utc, _eastern);
        }

        private DateTime PragueDate(DateTime utc)
        {
            return (_prague == null ? utc : TimeZoneInfo.ConvertTimeFromUtc(utc, _prague)).Date;
        }

        private DateTime ToUk(DateTime utc)
        {
            return utc.AddHours(IsUkSummer(utc) ? 1 : 0);
        }

        private bool IsUkSummer(DateTime utc)
        {
            int year = utc.Year;
            return utc >= LastWeekday(year, 3, DayOfWeek.Sunday).AddHours(1) &&
                   utc < LastWeekday(year, 10, DayOfWeek.Sunday).AddHours(1);
        }

        private static DateTime NthWeekday(int year, int month, DayOfWeek dayOfWeek, int n)
        {
            DateTime date = new DateTime(year, month, 1);
            int count = 0;
            while (true)
            {
                if (date.DayOfWeek == dayOfWeek && ++count == n) return date;
                date = date.AddDays(1);
            }
        }

        private static DateTime LastWeekday(int year, int month, DayOfWeek dayOfWeek)
        {
            DateTime date = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            while (date.DayOfWeek != dayOfWeek) date = date.AddDays(-1);
            return date;
        }

        private static DateTime ObservedFixed(DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday) return date.AddDays(-1);
            if (date.DayOfWeek == DayOfWeek.Sunday) return date.AddDays(1);
            return date;
        }

        private static DateTime EasterSunday(int year)
        {
            int a = year % 19, b = year / 100, c = year % 100, d = b / 4, e = b % 4;
            int f = (b + 8) / 25, g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30, i = c / 4, k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7, m = (a + 11 * h + 22 * l) / 451;
            int month = (h + l - 7 * m + 114) / 31;
            int day = (h + l - 7 * m + 114) % 31 + 1;
            return new DateTime(year, month, day);
        }

        private static string SafeStateToken(string value)
        {
            string token = (value ?? "").Trim();
            if (token.Length == 0) return "UNSET";
            string safe = new string(token.Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_').ToArray());
            return safe.Length == 0 ? "UNSET" : safe;
        }

        private static bool IsAscii(string value)
        {
            return (value ?? "").All(ch => ch <= 0x7F);
        }

        private static double ParseCommentNumber(string comment, string key)
        {
            if (string.IsNullOrEmpty(comment)) return 0;
            foreach (string part in comment.Split(';'))
            {
                if (!part.StartsWith(key, StringComparison.Ordinal)) continue;
                string numeric = part.Substring(key.Length).Trim();
                int separator = numeric.IndexOf(' ');
                if (separator >= 0) numeric = numeric.Substring(0, separator);
                double value;
                if (double.TryParse(numeric, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                    return value;
            }
            return 0;
        }

        private static string ParseCommentToken(string comment, string key)
        {
            if (string.IsNullOrEmpty(comment) || string.IsNullOrEmpty(key)) return "";
            foreach (string part in comment.Split(';'))
                if (part.StartsWith(key, StringComparison.Ordinal))
                    return part.Substring(key.Length).Trim();
            return "";
        }

        private static string ParseCompleteCommentToken(string comment, string key)
        {
            if (string.IsNullOrEmpty(comment) || string.IsNullOrEmpty(key)) return "";
            int start = 0;
            while (start < comment.Length)
            {
                int end = comment.IndexOf(';', start);
                if (end < 0) return "";
                int length = end - start;
                if (length >= key.Length &&
                    string.CompareOrdinal(comment, start, key, 0, key.Length) == 0)
                    return comment.Substring(start + key.Length, length - key.Length).Trim();
                start = end + 1;
            }
            return "";
        }

        private static string Jn(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return "null";
            return value.ToString("0.#####", CultureInfo.InvariantCulture);
        }

        private static string Js(string value)
        {
            return (value ?? "")
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }

        private static string Bool(bool value)
        {
            return value ? "true" : "false";
        }
    }
}
