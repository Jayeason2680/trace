// ============================================================================
// ZoneExec v1.1 — Zone Console, Session 2 (post 4-agent audit, 27 findings fixed)
// DEMO ONLY until every roadmap gate passes.
//
// Turns configured rectangles into risk-capped resting orders with broker-side
// SL/TP and broker-side expiry, under FTMO NORMAL law:
//   - WKD:HOLD rejected; ALL zone positions/orders flatten Friday (param, UTC)
//   - optional daily flatten (param) for instruments with a >2h nightly break
//     (FTMO Normal forbids holding through breaks >2h — set 20:40 on GER40)
//   - news windows (news.txt, UTC ±NewsPauseMinutes): pendings cancelled AND
//     (param, default ON) open ZC positions closed before the window
//   - daily cut-out −3% and account breaker −8% of FTMO initial balance,
//     Prague-midnight reset, persisted across restarts, FLATTEN on trip
// Kill switch: create Documents\ZoneConsole\KILL.txt → flatten + latch off.
// After deleting it, re-save each zone's comment to re-arm (deliberate).
//
// v1.1 audit fixes (highlights): no double-place after restart (existing-order
// adoption); periodic re-arm pass replaces broken RearmAll; ID write-back no
// longer self-churns; passive-side gate (no marketable limits, no in-zone or
// post-stop instant entries) + cooldown after closes; T2 confirm requires a
// recent touch and bounded distance; day anchor persisted (+ late-anchor safe
// mode); breakers flatten; touches keyed by zone id; debounced geometry edits;
// heartbeat URL validated; OnTimer exception-guarded; NaN-ATR refuses to arm.
//
// Single-file for paste-once install. Canonical testable core mirrored in
// src/ZoneExec/{ZoneRecord,ZoneValidator,RiskEngine}.cs — keep in sync.
// Attach ONE instance per symbol, on the chart you draw that symbol's zones on.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using File = System.IO.File; // cAlgo.API also defines a File type (CS0104)
using System.Linq;
using System.Net.Http;
using System.Text;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class ZoneExec : Robot
    {
        // ---------------- parameters ----------------
        [Parameter("Heartbeat ping URL", DefaultValue = "", Group = "Alerts")]
        public string HeartbeatUrl { get; set; }

        [Parameter("Heartbeat every (sec)", DefaultValue = 60, MinValue = 30, Group = "Alerts")]
        public int HeartbeatSeconds { get; set; }

        [Parameter("Digest hour (UTC, 14 = 22:00 MYT)", DefaultValue = 14, MinValue = 0, MaxValue = 23, Group = "Alerts")]
        public int DigestHourUtc { get; set; }

        [Parameter("FTMO initial balance (0 = snapshot on first start)", DefaultValue = 0.0, Group = "Risk")]
        public double InitialBalanceParam { get; set; }

        [Parameter("Max spread (pips) to allow arming/entry", DefaultValue = 6.0, MinValue = 0.5, Group = "Risk")]
        public double MaxSpreadPips { get; set; }

        [Parameter("Cooldown after a position closes (min)", DefaultValue = 30, MinValue = 0, Group = "Risk")]
        public int RearmCooldownMinutes { get; set; }

        [Parameter("News pause ± minutes", DefaultValue = 15, MinValue = 2, Group = "FTMO rules")]
        public int NewsPauseMinutes { get; set; }

        [Parameter("Close positions before news window", DefaultValue = true, Group = "FTMO rules")]
        public bool NewsFlattenPositions { get; set; }

        [Parameter("Friday flatten UTC (HH:mm)", DefaultValue = "20:30", Group = "FTMO rules")]
        public string FridayFlattenUtc { get; set; }

        [Parameter("Daily flatten UTC (HH:mm, empty = off; set 20:40 on indices like GER40)", DefaultValue = "", Group = "FTMO rules")]
        public string DailyFlattenUtc { get; set; }

        // ---------------- constants (mirror RiskEngine.cs) ----------------
        private const double DailyCutoutPercent = 3.0;    // of FTMO initial balance (FTMO fails at 5)
        private const double AccountBreakerPercent = 8.0; // of FTMO initial balance (FTMO fails at 10)
        private const double SlBufferFrac = 0.25;
        private const double T2MaxChaseFrac = 0.5;        // confirm entry must be within 0.5×height of the zone
        private const int T2ConfirmMaxBars = 12;          // confirm within 12 M15 bars (3h) of last touch
        private const int EditQuietSeconds = 2;           // debounce for drag edits
        private const string LabelPrefix = "ZC:";

        private static readonly HttpClient HttpShared = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };

        private enum ZStatus { Draft, Rejected, Armed, WaitingConfirm, Filled, Retired, Expired, Killed }

        private sealed class Zone
        {
            public string RectName;
            public string Id = "";
            public ZoneRecord Rec;
            public double Top, Bottom;
            public ZStatus Status = ZStatus.Draft;
            public int TouchesUsed;
            public DateTime LastTouchUtc = DateTime.MinValue;
            public bool InTouch;
            public string LastComment = "";
            public DateTime LastRejectJournal = DateTime.MinValue;
            public DateTime PendingSyncUtc = DateTime.MinValue;  // debounce stamp; MinValue = clean
            public DateTime CooldownUntilUtc = DateTime.MinValue;
            public string LastPaintHex = "";
        }

        private readonly Dictionary<string, Zone> _zones = new Dictionary<string, Zone>();
        private readonly Dictionary<string, int> _loadedTouches = new Dictionary<string, int>();       // by zone id
        private readonly Dictionary<string, DateTime> _loadedLastTouch = new Dictionary<string, DateTime>();
        private string _dir, _journalPath, _statePath, _newsPath, _killPath;
        private int _secToHeartbeat; private int _hbFail; private bool _hbDisabled;
        private DateTime _lastDigestUtc = DateTime.MinValue;
        private double _initialBalance;
        private double _dayStartRef; private DateTime _dayStartDatePrague = DateTime.MinValue;
        private DateTime _dailyTrippedUntilUtc = DateTime.MinValue;
        private DateTime _anchorLateBlockUntilUtc = DateTime.MinValue;
        private bool _accountTripped, _killLatched, _newsSuspended, _tzBroken, _stateDirty;
        private DateTime _lastStateFlushUtc = DateTime.MinValue;
        private DateTime _lastRearmPassUtc = DateTime.MinValue;
        private DateTime _lastNewsStaleWarnUtc = DateTime.MinValue;
        private AverageTrueRange _dailyAtr;
        private Bars _dailyBars, _m15Bars;
        private DateTime _lastM15Seen = DateTime.MinValue;
        private TimeZoneInfo _prague;
        private TimeSpan _fridayCut, _dailyCut; private bool _dailyCutEnabled;

        // ============================================================ lifecycle
        protected override void OnStart()
        {
            if (RunningMode != RunningMode.RealTime)
            { Print("ZoneExec requires live charts (drawings do not exist in backtest); stopping."); Stop(); return; }

            _dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ZoneConsole");
            Directory.CreateDirectory(Path.Combine(_dir, "journal"));
            var baseName = SymbolName + "-" + Chart.TimeFrame + "-acct" + Account.Number + "-exec";
            _journalPath = Path.Combine(_dir, "journal", baseName + ".jsonl");
            _statePath = Path.Combine(_dir, "journal", baseName + ".state");
            _newsPath = Path.Combine(_dir, "news.txt");
            _killPath = Path.Combine(_dir, "KILL.txt");

            _prague = ResolvePrague();
            if (_prague == null)
            {
                _tzBroken = true;
                Journal("tz_error", null, ("msg", "Prague timezone unavailable - ARMING DISABLED (day anchor would be wrong)"));
            }

            if (!TimeSpan.TryParseExact(FridayFlattenUtc, "hh\\:mm", CultureInfo.InvariantCulture, out _fridayCut))
            { _fridayCut = new TimeSpan(20, 30, 0); Journal("param_warning", null, ("msg", "bad Friday flatten '" + FridayFlattenUtc + "', using 20:30")); }
            _dailyCutEnabled = TimeSpan.TryParseExact(DailyFlattenUtc ?? "", "hh\\:mm", CultureInfo.InvariantCulture, out _dailyCut);
            if (!_dailyCutEnabled && !string.IsNullOrWhiteSpace(DailyFlattenUtc))
                Journal("param_warning", null, ("msg", "bad Daily flatten '" + DailyFlattenUtc + "' - daily flatten OFF"));

            if (!string.IsNullOrWhiteSpace(HeartbeatUrl) && !Uri.TryCreate(HeartbeatUrl, UriKind.Absolute, out _))
            { _hbDisabled = true; Journal("param_warning", null, ("msg", "heartbeat URL invalid - heartbeat disabled")); }

            _dailyBars = MarketData.GetBars(TimeFrame.Daily);
            _dailyAtr = Indicators.AverageTrueRange(_dailyBars, 14, MovingAverageType.Simple);
            _m15Bars = MarketData.GetBars(TimeFrame.Minute15);

            LoadState();
            if (InitialBalanceParam > 0 && Math.Abs(InitialBalanceParam - _initialBalance) > 0.005)
            {
                if (_initialBalance > 0) Journal("anchor_changed", null, ("old", F(_initialBalance)), ("new", F(InitialBalanceParam)));
                _initialBalance = InitialBalanceParam; MarkStateDirty();
            }
            if (_initialBalance <= 0)
            {
                _initialBalance = Account.Balance; MarkStateDirty();
                Journal("anchor_set", null, ("initial_balance", F(_initialBalance)),
                        ("note", "snapshot of current balance - set the parameter explicitly if this is not the FTMO starting balance"));
            }
            if (Math.Abs(Account.Balance - _initialBalance) / Math.Max(_initialBalance, 1) > 0.01)
                Journal("anchor_note", null, ("balance", F(Account.Balance)), ("anchor", F(_initialBalance)));

            // events only stamp zones dirty; all processing is debounced in OnTimer
            Chart.ObjectsAdded += e => { foreach (var r in e.ChartObjects.OfType<ChartRectangle>()) StampDirty(r.Name); };
            Chart.ObjectsUpdated += e => { foreach (var r in e.ChartObjects.OfType<ChartRectangle>()) StampDirty(r.Name); };
            Chart.ObjectsRemoved += e => { foreach (var r in e.ChartObjects.OfType<ChartRectangle>()) OnZoneRemoved(r.Name); };

            ReconcileBrokerState();
            foreach (var r in Chart.Objects.OfType<ChartRectangle>()) SyncZone(r, force: true);

            _secToHeartbeat = HeartbeatSeconds;
            Timer.Start(TimeSpan.FromSeconds(1));
            Journal("exec_start", null, ("zones", _zones.Count.ToString(CultureInfo.InvariantCulture)),
                    ("initial_balance", F(_initialBalance)),
                    ("account", Account.IsLive ? "LIVE" : "demo"));
            if (Account.IsLive)
                Journal("warning", null, ("msg", "LIVE ACCOUNT - proceed only if every roadmap gate is passed"));
        }

        protected override void OnStop() { Journal("exec_stop", null); FlushState(); }

        private static TimeZoneInfo ResolvePrague()
        {
            foreach (var id in new[] { "Central Europe Standard Time", "Europe/Prague" })
                try { return TimeZoneInfo.FindSystemTimeZoneById(id); } catch { }
            return null;
        }

        private void StampDirty(string rectName)
        {
            Zone z;
            if (!_zones.TryGetValue(rectName, out z)) { z = new Zone { RectName = rectName }; _zones[rectName] = z; }
            z.PendingSyncUtc = Server.Time;
        }

        // ============================================================ zone sync (debounced)
        private void SyncZone(ChartRectangle rect, bool force = false)
        {
            Zone z;
            if (!_zones.TryGetValue(rect.Name, out z))
            { z = new Zone { RectName = rect.Name }; _zones[rect.Name] = z; }
            z.PendingSyncUtc = DateTime.MinValue;

            double top = RoundTick(Math.Max(rect.Y1, rect.Y2)), bottom = RoundTick(Math.Min(rect.Y1, rect.Y2));
            var comment = rect.Comment ?? "";

            // Self-writeback no-op guard (audit fix): the comment we wrote back
            // (same content, our ID appended) is not an edit.
            bool geomSame = top == z.Top && bottom == z.Bottom;
            if (!force && geomSame && comment == z.LastComment) return;

            // NB: the ID-writeback echo is already caught by the LastComment guard above
            // (WriteBackId syncs z.LastComment). Any comment change reaching here is a
            // genuine edit and must re-validate + re-place — no adopt-without-revalidate
            // shortcut (audit v1.1: that shortcut left stale orders on risk/dir/HOLD edits).
            List<string> errors;
            var rec = ZoneRecord.TryParse(comment, Server.Time, out errors);

            bool changed = comment != z.LastComment || !geomSame;
            z.Top = top; z.Bottom = bottom; z.LastComment = comment;

            if ((z.Status == ZStatus.Armed || z.Status == ZStatus.WaitingConfirm) && changed)
            {
                CancelZoneOrders(z, "zone_edited");
                z.Status = ZStatus.Draft;
            }

            if (rec == null)
            {
                if (errors.Count > 0)
                {
                    z.Status = ZStatus.Rejected;
                    Journal("zone_rejected", z, ("errors", string.Join(" | ", errors)));
                    Paint(rect, z, "#808080");
                }
                else if (string.IsNullOrWhiteSpace(comment) && z.Status != ZStatus.Killed)
                    z.Status = ZStatus.Draft;
                z.Rec = null;
                return;
            }

            if (rec.Weekend == WeekendPolicy.Hold)
            {
                // cancel any resting order this zone may already carry before dropping Rec
                // (audit v1.1 fix 7: a HOLD edit must not leak a live order)
                if (!string.IsNullOrEmpty(rec.Id))
                    foreach (var o in PendingOrders.Where(o => (o.Label ?? "") == LabelPrefix + rec.Id && o.SymbolName == SymbolName).ToList())
                        o.Cancel();
                z.Status = ZStatus.Rejected; z.Rec = null;
                Journal("zone_rejected", z, ("errors", "WKD:HOLD not allowed on FTMO Normal"));
                Paint(rect, z, "#808080");
                return;
            }

            z.Rec = rec;
            if (string.IsNullOrEmpty(rec.Id))
            {
                rec.Id = ZoneRecord.NewId();
                WriteBackId(rect, z, rec.Id);
            }
            z.Id = rec.Id;
            int tLoad; DateTime ltLoad;
            if (_loadedTouches.TryGetValue(z.Id, out tLoad)) { z.TouchesUsed = tLoad; _loadedTouches.Remove(z.Id); }
            if (_loadedLastTouch.TryGetValue(z.Id, out ltLoad)) { z.LastTouchUtc = ltLoad; _loadedLastTouch.Remove(z.Id); }

            if (z.Status == ZStatus.Killed)
            {
                if (!changed) return;
                z.Status = ZStatus.Draft; // audit v1.1 fix 1: re-saving the comment re-engages a killed zone (documented behaviour)
            }
            if (changed && z.Status != ZStatus.Draft && z.Status != ZStatus.Rejected &&
                z.Status != ZStatus.Armed && z.Status != ZStatus.WaitingConfirm)
                return; // Filled/Retired/Expired keep their status; edits there are cosmetic

            TryArm(z, rect);
        }

        private void OnZoneRemoved(string name)
        {
            Zone z;
            if (!_zones.TryGetValue(name, out z)) return;
            CancelZoneOrders(z, "zone_removed");
            _zones.Remove(name);
            Journal("zone_removed", z);
            MarkStateDirty();
        }

        // ============================================================ arming
        private bool ArmingBlocked(out string why)
        {
            var now = Server.Time;
            if (_killLatched) { why = "kill latched"; return true; }
            if (_tzBroken) { why = "timezone unavailable"; return true; }
            if (_accountTripped) { why = "account breaker"; return true; }
            if (now < _dailyTrippedUntilUtc) { why = "daily cut-out"; return true; }
            if (now < _anchorLateBlockUntilUtc) { why = "late day-anchor safe mode"; return true; }
            if (_newsSuspended) { why = "news window"; return true; }
            if (InWeekendNoArmWindow(now)) { why = "weekend no-arm window"; return true; }
            // After the daily flatten time (indices), stay out until the next UTC day so
            // arm→flatten→arm can't churn through the nightly break (audit v1.1 fix 3).
            if (_dailyCutEnabled && now.DayOfWeek != DayOfWeek.Saturday && now.DayOfWeek != DayOfWeek.Sunday
                && now.TimeOfDay >= _dailyCut) { why = "past daily flatten (nightly break)"; return true; }
            why = ""; return false;
        }

        private bool InWeekendNoArmWindow(DateTime nowUtc)
        {
            if (nowUtc.DayOfWeek == DayOfWeek.Friday && nowUtc.TimeOfDay >= _fridayCut) return true;
            if (nowUtc.DayOfWeek == DayOfWeek.Saturday) return true;
            if (nowUtc.DayOfWeek == DayOfWeek.Sunday && nowUtc.TimeOfDay < new TimeSpan(21, 0, 0)) return true;
            return false;
        }

        private void TryArm(Zone z, ChartRectangle rect)
        {
            if (z.Rec == null) return;
            string why;
            if (ArmingBlocked(out why)) { JournalRejectOnce(z, "blocked", why); return; }
            if (Server.Time < z.CooldownUntilUtc) { JournalRejectOnce(z, "cooldown", "recent close on this zone"); return; }

            // Existing-order adoption (audit fix: restart must not double-place)
            if (!string.IsNullOrEmpty(z.Id) &&
                PendingOrders.Any(o => (o.Label ?? "") == LabelPrefix + z.Id && o.SymbolName == SymbolName))
            {
                if (z.Status != ZStatus.Armed)
                { z.Status = ZStatus.Armed; Journal("zone_adopted", z, ("note", "existing resting order found - adopted, not re-placed")); Paint(rect, z, z.Rec.Direction == Direction.Buy ? "#0ca30c" : "#d03b3b"); }
                return;
            }
            if (Positions.Any(p => (p.Label ?? "") == LabelPrefix + z.Id && p.SymbolName == SymbolName))
            { if (z.Status != ZStatus.Filled) { z.Status = ZStatus.Filled; Journal("zone_adopted", z, ("note", "existing position found")); } return; }

            // Touch budget already spent → retire, never re-place (audit v1.1 fix 4).
            // FlattenEverything demotes to Draft, and touches persist across restarts, so
            // without this a budget-exhausted zone would silently re-arm a fresh order.
            if (z.TouchesUsed >= z.Rec.TouchBudget)
            { z.Status = ZStatus.Retired; Journal("zone_retired", z, ("reason", "touch budget already spent")); return; }

            double atr = _dailyAtr.Result.LastValue;
            if (double.IsNaN(atr) || atr <= 0)
            { JournalRejectOnce(z, "blocked", "daily ATR unavailable/NaN - refusing to arm"); return; }

            double mid = (Symbol.Bid + Symbol.Ask) / 2.0;
            var geo = new ZoneGeometry { Symbol = SymbolName, Top = z.Top, Bottom = z.Bottom, Direction = z.Rec.Direction };
            var others = _zones.Values.Where(o => o != z && o.Rec != null &&
                            (o.Status == ZStatus.Armed || o.Status == ZStatus.WaitingConfirm || o.Status == ZStatus.Filled))
                         .Select(o => new ZoneGeometry { Symbol = SymbolName, Top = o.Top, Bottom = o.Bottom, Direction = o.Rec.Direction });
            // Validator errors keep Rec so the periodic pass can retry (audit v1.1 fix 6:
            // transient errors like too-far / overlap-with-a-zone-that-later-retires must
            // not dead-end until a manual re-save). Logging is throttled.
            var val = ZoneValidator.Validate(z.Rec, geo, atr, mid, Server.Time, others);
            if (!val.Ok)
            { z.Status = ZStatus.Rejected; JournalRejectOnce(z, "validation", string.Join(" | ", val.Errors)); Paint(rect, z, "#808080"); return; }

            // Passive-side gate (audit fix: never a marketable limit, never in-zone,
            // never a post-stop instant re-entry). BUY zone: price must be fully
            // above the zone; SELL zone: fully below.
            bool buy = z.Rec.Direction == Direction.Buy;
            bool passive = buy ? Symbol.Bid > z.Top : Symbol.Ask < z.Bottom;
            if (!passive) { JournalRejectOnce(z, "waiting", "price not on the approach side of the zone - will arm when it is"); return; }

            var spreadPips = (Symbol.Ask - Symbol.Bid) / Symbol.PipSize;
            if (spreadPips > MaxSpreadPips)
            { JournalRejectOnce(z, "anomaly", Fmt("spread {0:0.0} pips > max {1:0.0}", spreadPips, MaxSpreadPips)); return; }

            var capErrors = CheckCapsLive(z.Rec);
            if (capErrors.Count > 0) { JournalRejectOnce(z, "caps", string.Join(" | ", capErrors)); return; }

            double height = z.Top - z.Bottom;
            double entry = z.Rec.EntryAt == EntryPrice.Mid ? (z.Top + z.Bottom) / 2.0 : (buy ? z.Top : z.Bottom);
            double sl = buy ? z.Bottom - height * SlBufferFrac : z.Top + height * SlBufferFrac;
            double stopDist = Math.Abs(entry - sl);
            double tp = buy ? entry + z.Rec.RewardRiskTarget * stopDist : entry - z.Rec.RewardRiskTarget * stopDist;

            var sizing = ComputeVolumeLive(z.Rec, stopDist);
            if (!sizing.Ok)
            { z.Status = ZStatus.Rejected; JournalRejectOnce(z, "sizing", string.Join(" | ", sizing.Errors)); Paint(rect, z, "#808080"); return; }
            foreach (var w in sizing.Warnings) Journal("zone_warning", z, ("warning", w));

            if (z.Rec.Entry == EntryStyle.T1RestingLimit)
            {
                var expiry = OrderExpiryUtc(z.Rec);
                if (expiry <= Server.Time) { JournalRejectOnce(z, "blocked", "no valid order window before expiry/weekend"); return; }
                var res = PlaceLimitOrder(buy ? TradeType.Buy : TradeType.Sell, SymbolName,
                                          sizing.VolumeUnits, entry, LabelPrefix + z.Id,
                                          stopDist / Symbol.PipSize, Math.Abs(tp - entry) / Symbol.PipSize, expiry);
                if (!res.IsSuccessful) { JournalRejectOnce(z, "order_rejected", res.Error.ToString()); return; }
                z.Status = ZStatus.Armed;
                Journal("zone_armed", z, ("entry", F(entry)), ("sl", F(sl)), ("tp", F(tp)),
                        ("volume", F(sizing.VolumeUnits)), ("risk_pct", F(sizing.AchievedRiskPercent)),
                        ("order_expiry_utc", expiry.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)), ("style", "T1"));
                Paint(rect, z, buy ? "#0ca30c" : "#d03b3b");
            }
            else
            {
                z.Status = ZStatus.WaitingConfirm;
                Journal("zone_armed", z, ("entry", "on-confirm"), ("style", "T2"),
                        ("volume_planned", F(sizing.VolumeUnits)));
                Paint(rect, z, "#eda100");
            }
        }

        // ============================================================ ticks: touches
        protected override void OnTick()
        {
            foreach (var z in _zones.Values)
            {
                if (z.Rec == null) continue;
                double height = z.Top - z.Bottom; if (height <= 0) continue;
                bool overlap = Symbol.Bid <= z.Top && Symbol.Ask >= z.Bottom;
                double buffer = height * 0.25;
                bool exited = Symbol.Bid > z.Top + buffer || Symbol.Ask < z.Bottom - buffer;

                if (!z.InTouch && overlap)
                {
                    z.InTouch = true;
                    z.TouchesUsed++;
                    z.LastTouchUtc = Server.Time;
                    MarkStateDirty();
                    Journal("touch", z, ("n", z.TouchesUsed.ToString(CultureInfo.InvariantCulture)),
                            ("spread_pips", F((Symbol.Ask - Symbol.Bid) / Symbol.PipSize)));
                    if (z.TouchesUsed > z.Rec.TouchBudget && (z.Status == ZStatus.Armed || z.Status == ZStatus.WaitingConfirm))
                    {
                        CancelZoneOrders(z, "touch_budget_spent");
                        z.Status = ZStatus.Retired;
                        Journal("zone_retired", z);
                    }
                }
                else if (z.InTouch && exited) z.InTouch = false;
            }
        }

        private void CheckT2Confirms()
        {
            int last = _m15Bars.Count - 2; if (last < 1) return;
            var barTime = _m15Bars.OpenTimes[last]; if (barTime == _lastM15Seen) return;
            // do not consume bars while suspended (audit fix): evaluate later
            string why;
            if (ArmingBlocked(out why)) return;
            _lastM15Seen = barTime;
            double close = _m15Bars.ClosePrices[last];

            foreach (var z in _zones.Values)
            {
                if (z.Rec == null || z.Status != ZStatus.WaitingConfirm) continue;
                if (z.TouchesUsed == 0 || z.TouchesUsed > z.Rec.TouchBudget) continue;
                // recency: confirm must come within T2ConfirmMaxBars of the last touch
                if (z.LastTouchUtc == DateTime.MinValue ||
                    (Server.Time - z.LastTouchUtc).TotalMinutes > T2ConfirmMaxBars * 15) continue;
                bool buy = z.Rec.Direction == Direction.Buy;
                if (!(buy ? close > z.Top : close < z.Bottom)) continue;

                double height = z.Top - z.Bottom;
                double entry = buy ? Symbol.Ask : Symbol.Bid;
                // distance guard: no chasing far breakouts (audit fix)
                if (buy ? entry > z.Top + height * T2MaxChaseFrac : entry < z.Bottom - height * T2MaxChaseFrac)
                { Journal("t2_entry_skipped", z, ("reason", "price too far from zone at confirm")); continue; }
                double sl = buy ? z.Bottom - height * SlBufferFrac : z.Top + height * SlBufferFrac;
                double stopDist = Math.Abs(entry - sl); if (stopDist <= 0) continue;
                double tp = buy ? entry + z.Rec.RewardRiskTarget * stopDist : entry - z.Rec.RewardRiskTarget * stopDist;

                var sizing = ComputeVolumeLive(z.Rec, stopDist);
                var capErr = CheckCapsLive(z.Rec);
                if (!sizing.Ok || capErr.Count > 0)
                { Journal("t2_entry_blocked", z, ("errors", string.Join(" | ", sizing.Errors.Concat(capErr)))); continue; }

                var res = ExecuteMarketOrder(buy ? TradeType.Buy : TradeType.Sell, SymbolName, sizing.VolumeUnits,
                                             LabelPrefix + z.Id, stopDist / Symbol.PipSize, Math.Abs(tp - entry) / Symbol.PipSize);
                if (res.IsSuccessful)
                { z.Status = ZStatus.Filled; Journal("t2_entered", z, ("entry", F(entry)), ("sl", F(sl)), ("tp", F(tp)), ("volume", F(sizing.VolumeUnits))); }
                else Journal("t2_entry_blocked", z, ("errors", res.Error.ToString()));
            }
        }

        // ============================================================ timer: guardian loop
        protected override void OnTimer()
        {
            try { TimerBody(); }
            catch (Exception ex) { Journal("timer_error", null, ("error", ex.Message)); }
        }

        private void TimerBody()
        {
            var now = Server.Time;

            if (!_hbDisabled && --_secToHeartbeat <= 0) { _secToHeartbeat = HeartbeatSeconds; Heartbeat(); }

            bool killNow = File.Exists(_killPath);
            if (killNow && !_killLatched)
            {
                _killLatched = true; MarkStateDirty();
                Journal("killswitch", null, ("action", "flatten+cancel all ZC; arming latched off"));
                FlattenEverything("killswitch");
            }
            else if (!killNow && _killLatched)
            {
                _killLatched = false; MarkStateDirty();
                Journal("killswitch_cleared", null, ("note", "zones stay KILLED until each comment is re-saved (deliberate)"));
            }

            UpdateBreakers(now);
            UpdateNewsWindow(now);
            SessionFlattenCheck(now);
            ExpirySweep(now);
            CheckT2Confirms();
            SyncFills();

            // debounced edit processing (audit fix: no order churn while dragging)
            foreach (var z in _zones.Values.Where(x => x.PendingSyncUtc != DateTime.MinValue &&
                         (now - x.PendingSyncUtc).TotalSeconds >= EditQuietSeconds).ToList())
            {
                var rect = Chart.Objects.OfType<ChartRectangle>().FirstOrDefault(r => r.Name == z.RectName);
                if (rect != null) SyncZone(rect);
                else z.PendingSyncUtc = DateTime.MinValue;
            }

            // periodic re-arm pass (audit fix: zones must come back after news
            // windows, cut-out lapse, weekend, cooldowns — without manual edits)
            if ((now - _lastRearmPassUtc).TotalSeconds >= 5)
            {
                _lastRearmPassUtc = now;
                string why;
                if (!ArmingBlocked(out why))
                    // Draft (came back from a block) and Rejected-with-Rec (transient
                    // validation error, e.g. was too far, now in range) both retry here.
                    foreach (var z in _zones.Values.Where(x => (x.Status == ZStatus.Draft || x.Status == ZStatus.Rejected) && x.Rec != null).ToList())
                    {
                        var rect = Chart.Objects.OfType<ChartRectangle>().FirstOrDefault(r => r.Name == z.RectName);
                        if (rect != null) TryArm(z, rect);
                    }
            }

            if (_stateDirty && (now - _lastStateFlushUtc).TotalSeconds >= 5) FlushState();

            if (now.Hour >= DigestHourUtc && _lastDigestUtc.Date != now.Date)
            { _lastDigestUtc = now; MarkStateDirty(); Digest(); }
        }

        private void UpdateBreakers(DateTime nowUtc)
        {
            if (_tzBroken) return;
            var pragueNow = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, _prague);
            var prg = pragueNow.Date;
            if (prg != _dayStartDatePrague)
            {
                bool firstEver = _dayStartDatePrague == DateTime.MinValue;
                // Detected materially after Prague midnight ⇒ the rollover was processed
                // late (bot was down over midnight). Continuous running detects it within
                // ~1s, so a >10-min offset means the true day-start equity is unknown.
                // (audit v1.1 fix 5: the old >1.5-day gap test missed one-midnight outages.)
                bool detectedLate = !firstEver && pragueNow.TimeOfDay > TimeSpan.FromMinutes(10);
                // On a genuine first install, only distrust the anchor if the account
                // already shows an intraday loss we didn't witness.
                bool firstEverSuspect = firstEver && Account.Equity < _initialBalance * 0.995;

                _dayStartDatePrague = prg;
                _dayStartRef = Math.Max(Account.Balance, Account.Equity);
                MarkStateDirty();
                Journal("day_anchor", null, ("ref", F(_dayStartRef)),
                        ("prague_date", prg.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
                if (detectedLate || firstEverSuspect)
                {
                    _anchorLateBlockUntilUtc = TimeZoneInfo.ConvertTimeToUtc(prg.AddDays(1), _prague);
                    Journal("anchor_late", null, ("reason", detectedLate ? "rollover processed late (outage over midnight)" : "first install with an existing intraday loss"),
                            ("action", "no new arming until next Prague midnight; open positions keep broker stops"));
                }
            }

            if (!_accountTripped && Account.Equity <= _initialBalance * (1 - AccountBreakerPercent / 100.0))
            {
                _accountTripped = true; MarkStateDirty();
                Journal("account_breaker", null, ("equity", F(Account.Equity)),
                        ("action", "FLATTEN ALL - written review required to reset (delete 'accountTripped' line in state file after review)"));
                FlattenEverything("account_breaker");
            }

            if (nowUtc >= _dailyTrippedUntilUtc &&
                _dayStartRef > 0 && _dayStartRef - Account.Equity >= _initialBalance * DailyCutoutPercent / 100.0)
            {
                _dailyTrippedUntilUtc = TimeZoneInfo.ConvertTimeToUtc(prg.AddDays(1), _prague);
                MarkStateDirty();
                Journal("daily_cutout", null, ("drawdown", F(_dayStartRef - Account.Equity)),
                        ("until_utc", _dailyTrippedUntilUtc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)),
                        ("action", "FLATTEN ALL - protects the FTMO -5% floor (audit fix: freezing alone stranded open risk)"));
                FlattenEverything("daily_cutout");
            }
        }

        private void UpdateNewsWindow(DateTime nowUtc)
        {
            bool inWindow = false; DateTime newest = DateTime.MinValue; bool haveFile = false;
            try
            {
                if (File.Exists(_newsPath))
                {
                    haveFile = true;
                    foreach (var line in File.ReadAllLines(_newsPath))
                    {
                        var t = line.Trim(); if (t.Length == 0 || t.StartsWith("#")) continue;
                        DateTime ev;
                        if (DateTime.TryParseExact(t, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out ev))
                        {
                            if (ev > newest) newest = ev;
                            if (Math.Abs((nowUtc - ev).TotalMinutes) <= NewsPauseMinutes) inWindow = true;
                        }
                    }
                }
            }
            catch (Exception ex) { Print("news read failed: {0}", ex.Message); }

            // stale/missing news.txt fails LOUD, not silent (audit fix)
            if ((!haveFile || newest < nowUtc.AddDays(-7)) && (nowUtc - _lastNewsStaleWarnUtc).TotalHours >= 24)
            {
                _lastNewsStaleWarnUtc = nowUtc;
                Journal("news_file_stale", null, ("msg", haveFile ? "newest event older than 7 days - refresh news.txt (Sunday routine)" : "news.txt missing - no news protection"));
            }

            if (inWindow && !_newsSuspended)
            {
                _newsSuspended = true;
                Journal("news_pause_start", null);
                CancelAllZcOrders("news_pause");
                if (NewsFlattenPositions)
                    foreach (var p in Positions.Where(p => (p.Label ?? "").StartsWith(LabelPrefix) && p.SymbolName == SymbolName).ToList())
                    { var r = p.Close(); Journal(r.IsSuccessful ? "news_position_closed" : "close_failed", null, ("label", p.Label)); }
                else
                    foreach (var p in Positions.Where(p => (p.Label ?? "").StartsWith(LabelPrefix) && p.SymbolName == SymbolName))
                        Journal("news_position_alert", null, ("label", p.Label), ("note", "SL/TP execution inside a restricted window can breach FTMO funded rules"));
            }
            else if (!inWindow && _newsSuspended)
            {
                _newsSuspended = false;
                Journal("news_pause_end", null, ("note", "periodic pass will re-arm eligible zones"));
            }
        }

        private void SessionFlattenCheck(DateTime nowUtc)
        {
            bool due = (nowUtc.DayOfWeek == DayOfWeek.Friday && nowUtc.TimeOfDay >= _fridayCut) ||
                       (_dailyCutEnabled && nowUtc.TimeOfDay >= _dailyCut && nowUtc.DayOfWeek != DayOfWeek.Saturday && nowUtc.DayOfWeek != DayOfWeek.Sunday);
            if (!due) return;
            bool anything = Positions.Any(p => (p.Label ?? "").StartsWith(LabelPrefix) && p.SymbolName == SymbolName) ||
                            PendingOrders.Any(o => (o.Label ?? "").StartsWith(LabelPrefix) && o.SymbolName == SymbolName);
            if (!anything) return;
            Journal("session_flatten", null, ("rule", nowUtc.DayOfWeek == DayOfWeek.Friday ? "FTMO Normal: no weekend holds" : "no holds through the >2h daily break"));
            FlattenEverything("session_flatten");
        }

        private void ExpirySweep(DateTime nowUtc)
        {
            foreach (var z in _zones.Values)
            {
                if (z.Rec == null) continue;
                if (nowUtc > z.Rec.ExpiryUtc && (z.Status == ZStatus.Armed || z.Status == ZStatus.WaitingConfirm || z.Status == ZStatus.Draft))
                { CancelZoneOrders(z, "expired"); z.Status = ZStatus.Expired; Journal("zone_expired", z); }
            }
        }

        private void SyncFills()
        {
            foreach (var z in _zones.Values)
            {
                if (z.Rec == null || string.IsNullOrEmpty(z.Id)) continue;
                bool hasPos = Positions.Any(p => (p.Label ?? "") == LabelPrefix + z.Id && p.SymbolName == SymbolName);
                if (hasPos && z.Status != ZStatus.Filled)
                {
                    // covers the cancel-vs-fill race too (audit fix): a position always wins
                    if (z.Status == ZStatus.Retired || z.Status == ZStatus.Draft)
                        Journal("fill_race_note", z, ("was", z.Status.ToString()));
                    z.Status = ZStatus.Filled;
                    Journal("zone_filled", z);
                }
                else if (!hasPos && z.Status == ZStatus.Filled)
                {
                    bool stillPending = PendingOrders.Any(o => (o.Label ?? "") == LabelPrefix + z.Id && o.SymbolName == SymbolName);
                    if (!stillPending)
                    {
                        z.Status = z.TouchesUsed >= z.Rec.TouchBudget ? ZStatus.Retired : ZStatus.Draft;
                        z.CooldownUntilUtc = Server.Time.AddMinutes(RearmCooldownMinutes); // audit fix: no instant re-entry
                        MarkStateDirty();
                        Journal("position_closed", z, ("touches_used", z.TouchesUsed.ToString(CultureInfo.InvariantCulture)),
                                ("next", z.Status.ToString()), ("cooldown_min", RearmCooldownMinutes.ToString(CultureInfo.InvariantCulture)));
                    }
                }
            }
        }

        // ============================================================ order plumbing
        private DateTime OrderExpiryUtc(ZoneRecord rec)
        {
            var now = Server.Time;
            int daysToFriday = ((int)DayOfWeek.Friday - (int)now.DayOfWeek + 7) % 7;
            var friday = now.Date.AddDays(daysToFriday).Add(_fridayCut);
            if (friday <= now) friday = friday.AddDays(7);
            var cap = rec.ExpiryUtc < friday ? rec.ExpiryUtc : friday;
            if (_dailyCutEnabled)
            {
                // next daily flatten (today's if still ahead, else tomorrow's) — an order
                // must never survive the nightly break (audit v1.1 fix 3).
                var nextDailyCut = now.Date.Add(_dailyCut);
                if (nextDailyCut <= now) nextDailyCut = nextDailyCut.AddDays(1);
                if (nextDailyCut < cap) cap = nextDailyCut;
            }
            return cap;
        }

        private void CancelZoneOrders(Zone z, string reason)
        {
            if (string.IsNullOrEmpty(z.Id)) return;
            foreach (var o in PendingOrders.Where(o => (o.Label ?? "") == LabelPrefix + z.Id && o.SymbolName == SymbolName).ToList())
            {
                var r = o.Cancel();
                if (!r.IsSuccessful) { r = o.Cancel(); }
                Journal(r.IsSuccessful ? "order_cancelled" : "cancel_failed", z, ("reason", reason));
            }
        }

        private void CancelAllZcOrders(string reason)
        {
            foreach (var o in PendingOrders.Where(o => (o.Label ?? "").StartsWith(LabelPrefix) && o.SymbolName == SymbolName).ToList())
            { var r = o.Cancel(); if (!r.IsSuccessful) r = o.Cancel(); if (!r.IsSuccessful) Journal("cancel_failed", null, ("label", o.Label)); }
            foreach (var z in _zones.Values)
                if (z.Status == ZStatus.Armed) z.Status = ZStatus.Draft;
            Journal("orders_cancelled_all", null, ("reason", reason));
        }

        private void FlattenEverything(string reason)
        {
            foreach (var o in PendingOrders.Where(o => (o.Label ?? "").StartsWith(LabelPrefix) && o.SymbolName == SymbolName).ToList())
            { var r = o.Cancel(); if (!r.IsSuccessful) r = o.Cancel(); if (!r.IsSuccessful) Journal("cancel_failed", null, ("label", o.Label)); }
            foreach (var p in Positions.Where(p => (p.Label ?? "").StartsWith(LabelPrefix) && p.SymbolName == SymbolName).ToList())
            { var r = p.Close(); if (!r.IsSuccessful) r = p.Close(); if (!r.IsSuccessful) Journal("close_failed", null, ("label", p.Label), ("note", "CLOSE MANUALLY NOW - flatten did not complete")); }
            foreach (var z in _zones.Values)
                if (z.Status == ZStatus.Armed || z.Status == ZStatus.WaitingConfirm || z.Status == ZStatus.Filled)
                {
                    z.Status = reason == "killswitch" ? ZStatus.Killed : ZStatus.Draft;
                    z.CooldownUntilUtc = Server.Time.AddMinutes(RearmCooldownMinutes);
                }
            Journal("flatten_all", null, ("reason", reason));
        }

        private void ReconcileBrokerState()
        {
            // A zone counts as "known" only if it parses, has an id, AND is still
            // arm-eligible; a WKD:HOLD edit is a rejection, so its leftover order must be
            // treated as an orphan and cancelled here (audit v1.1 fix 7).
            var known = new HashSet<string>(Chart.Objects.OfType<ChartRectangle>()
                        .Select(r => { List<string> e; var rec = ZoneRecord.TryParse(r.Comment ?? "", Server.Time, out e);
                                       return rec != null && !string.IsNullOrEmpty(rec.Id) && rec.Weekend != WeekendPolicy.Hold ? LabelPrefix + rec.Id : null; })
                        .Where(x => x != null));
            foreach (var o in PendingOrders.Where(o => (o.Label ?? "").StartsWith(LabelPrefix) && o.SymbolName == SymbolName).ToList())
                if (!known.Contains(o.Label))
                { var r = o.Cancel(); Journal(r.IsSuccessful ? "orphan_order_cancelled" : "cancel_failed", null, ("label", o.Label)); }
            foreach (var p in Positions.Where(p => (p.Label ?? "").StartsWith(LabelPrefix) && p.SymbolName == SymbolName))
                if (!known.Contains(p.Label))
                    Journal("orphan_position_alert", null, ("label", p.Label), ("note", "position without a zone - review manually; broker SL/TP still active"));
        }

        // ============================================================ caps & sizing
        private List<string> CheckCapsLive(ZoneRecord rec)
        {
            var open = new List<OpenExposure>();
            foreach (var p in Positions.Where(p => (p.Label ?? "").StartsWith(LabelPrefix)))
                open.Add(new OpenExposure { Symbol = p.SymbolName, Direction = p.TradeType == TradeType.Buy ? Direction.Buy : Direction.Sell, RiskPercentAtEntry = RiskOfLabel(p.Label) });
            // pendings reserve capacity too (deliberately conservative: all could fill)
            foreach (var o in PendingOrders.Where(o => (o.Label ?? "").StartsWith(LabelPrefix)))
                open.Add(new OpenExposure { Symbol = o.SymbolName, Direction = o.TradeType == TradeType.Buy ? Direction.Buy : Direction.Sell, RiskPercentAtEntry = RiskOfLabel(o.Label) });
            return RiskEngine.CheckCaps(rec, SymbolName, open);
        }

        private double RiskOfLabel(string label)
        {
            var z = _zones.Values.FirstOrDefault(x => x.Rec != null && LabelPrefix + x.Id == label);
            return z != null ? z.Rec.RiskPercent : 1.0; // unknown → assume worst class cap
        }

        private SizingResult ComputeVolumeLive(ZoneRecord rec, double stopDistance)
        {
            return RiskEngine.ComputeVolume(rec, _initialBalance, stopDistance,
                Symbol.TickSize, Symbol.TickValue, Symbol.VolumeInUnitsStep, Symbol.VolumeInUnitsMin);
        }

        // ============================================================ helpers
        private double RoundTick(double price) =>
            Symbol.TickSize > 0 ? Math.Round(price / Symbol.TickSize) * Symbol.TickSize : price;

        private void Paint(ChartRectangle rect, Zone z, string hex)
        {
            if (z.LastPaintHex == hex) return;
            try { rect.Color = Color.FromHex(hex); z.LastPaintHex = hex; } catch { }
        }

        private void WriteBackId(ChartRectangle rect, Zone z, string id)
        {
            try
            {
                var newComment = (rect.Comment ?? "").TrimEnd() + " ID:" + id;
                rect.Comment = newComment;
                z.LastComment = newComment; // audit fix: our own write must not read as an edit
            }
            catch (Exception ex) { Print("id writeback failed: {0}", ex.Message); }
        }

        private void JournalRejectOnce(Zone z, string kind, string detail)
        {
            if ((Server.Time - z.LastRejectJournal).TotalMinutes < 30) return;
            z.LastRejectJournal = Server.Time;
            Journal("arm_blocked", z, ("kind", kind), ("detail", detail));
        }

        private void Digest()
        {
            int armed = _zones.Values.Count(z => z.Status == ZStatus.Armed || z.Status == ZStatus.WaitingConfirm);
            int filled = _zones.Values.Count(z => z.Status == ZStatus.Filled);
            Journal("daily_digest", null, ("armed", armed.ToString(CultureInfo.InvariantCulture)),
                    ("filled", filled.ToString(CultureInfo.InvariantCulture)),
                    ("equity", F(Account.Equity)), ("day_ref", F(_dayStartRef)),
                    ("daily_tripped", (Server.Time < _dailyTrippedUntilUtc).ToString()),
                    ("account_tripped", _accountTripped.ToString()),
                    ("kill_latched", _killLatched.ToString()));
        }

        private void Heartbeat()
        {
            if (string.IsNullOrWhiteSpace(HeartbeatUrl) || _hbDisabled) return;
            try
            {
                HttpShared.GetAsync(HeartbeatUrl).ContinueWith(t =>
                {
                    bool ok = t.Status == System.Threading.Tasks.TaskStatus.RanToCompletion && t.Result.IsSuccessStatusCode;
                    if (t.Status == System.Threading.Tasks.TaskStatus.RanToCompletion) t.Result.Dispose();
                    BeginInvokeOnMainThread(() =>
                    {
                        if (ok) { _hbFail = 0; return; }
                        if (++_hbFail == 5) Journal("heartbeat_failing", null, ("failures", "5"));
                    });
                }, System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously);
            }
            catch (Exception ex) { Print("heartbeat error: {0}", ex.Message); }
        }

        private void MarkStateDirty() { _stateDirty = true; }

        private void LoadState()
        {
            try
            {
                if (!File.Exists(_statePath)) return;
                foreach (var line in File.ReadAllLines(_statePath))
                {
                    var kv = line.Split(new[] { '=' }, 2); if (kv.Length != 2) continue;
                    switch (kv[0])
                    {
                        case "initialBalance": double.TryParse(kv[1], NumberStyles.Float, CultureInfo.InvariantCulture, out _initialBalance); break;
                        case "dayStartRef": double.TryParse(kv[1], NumberStyles.Float, CultureInfo.InvariantCulture, out _dayStartRef); break;
                        case "dayStartDatePrague": DateTime.TryParseExact(kv[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _dayStartDatePrague); break;
                        case "dailyTrippedUntilUtc": DateTime.TryParse(kv[1], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out _dailyTrippedUntilUtc); break;
                        case "accountTripped": bool.TryParse(kv[1], out _accountTripped); break;
                        case "killLatched": bool.TryParse(kv[1], out _killLatched); break;
                        case "lastDigestUtc": DateTime.TryParse(kv[1], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out _lastDigestUtc); break;
                        default:
                            if (kv[0].StartsWith("touches:"))
                            { int n; if (int.TryParse(kv[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out n)) _loadedTouches[kv[0].Substring(8)] = n; }
                            else if (kv[0].StartsWith("lasttouch:"))
                            { DateTime d; if (DateTime.TryParse(kv[1], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out d)) _loadedLastTouch[kv[0].Substring(10)] = d; }
                            break;
                    }
                }
            }
            catch (Exception ex) { Print("state load failed: {0}", ex.Message); }
        }

        private void FlushState()
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("initialBalance=").Append(F(_initialBalance)).AppendLine();
                sb.Append("dayStartRef=").Append(F(_dayStartRef)).AppendLine();
                sb.Append("dayStartDatePrague=").Append(_dayStartDatePrague.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).AppendLine();
                sb.Append("dailyTrippedUntilUtc=").Append(_dailyTrippedUntilUtc.ToString("o", CultureInfo.InvariantCulture)).AppendLine();
                sb.Append("accountTripped=").Append(_accountTripped).AppendLine();
                sb.Append("killLatched=").Append(_killLatched).AppendLine();
                sb.Append("lastDigestUtc=").Append(_lastDigestUtc.ToString("o", CultureInfo.InvariantCulture)).AppendLine();
                foreach (var z in _zones.Values.Where(z => !string.IsNullOrEmpty(z.Id) && z.TouchesUsed > 0))
                {
                    sb.Append("touches:").Append(z.Id).Append('=').Append(z.TouchesUsed.ToString(CultureInfo.InvariantCulture)).AppendLine();
                    if (z.LastTouchUtc != DateTime.MinValue)
                        sb.Append("lasttouch:").Append(z.Id).Append('=').Append(z.LastTouchUtc.ToString("o", CultureInfo.InvariantCulture)).AppendLine();
                }
                File.WriteAllText(_statePath, sb.ToString());
                _stateDirty = false; _lastStateFlushUtc = Server.Time;
            }
            catch (Exception ex) { Print("state save failed: {0}", ex.Message); }
        }

        private static string F(double v) => v.ToString("0.#####", CultureInfo.InvariantCulture);
        private static string Fmt(string f, params object[] a) => string.Format(CultureInfo.InvariantCulture, f, a);

        private static string Jesc(string s)
        {
            var sb = new StringBuilder();
            foreach (var c in s ?? "")
            {
                if (c == '"' || c == '\\') sb.Append('\\').Append(c);
                else if (c == '\n') sb.Append("\\n");
                else if (c < ' ') sb.Append(' ');
                else sb.Append(c);
            }
            return sb.ToString();
        }

        private void Journal(string evt, Zone z, params (string k, string v)[] extra)
        {
            try
            {
                var sb = new StringBuilder(256);
                sb.Append("{\"ts\":\"").Append(Server.Time.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture))
                  .Append("\",\"event\":\"").Append(Jesc(evt))
                  .Append("\",\"symbol\":\"").Append(Jesc(SymbolName)).Append('"');
                if (z != null)
                {
                    sb.Append(",\"zone\":\"").Append(Jesc(z.RectName))
                      .Append("\",\"id\":\"").Append(Jesc(z.Id))
                      .Append("\",\"status\":\"").Append(z.Status.ToString())
                      .Append("\",\"top\":").Append(F(z.Top)).Append(",\"bottom\":").Append(F(z.Bottom));
                }
                foreach (var (k, v) in extra)
                    sb.Append(",\"").Append(Jesc(k)).Append("\":\"").Append(Jesc(v)).Append('"');
                sb.Append('}');
                File.AppendAllText(_journalPath, sb.ToString() + Environment.NewLine);
            }
            catch (Exception ex) { Print("journal write failed: {0}", ex.Message); }
        }

        // ====================================================================
        // Embedded core (mirrors src/ZoneExec/*.cs — keep changes in sync)
        // ====================================================================
        public enum ZoneClass { Red, Yellow }
        public enum Direction { Buy, Sell }
        public enum EntryStyle { T1RestingLimit = 1, T2TouchConfirm = 2 }
        public enum EntryPrice { Mid, Edge }
        public enum WeekendPolicy { Flat, Hold }

        public sealed class ZoneRecord
        {
            public ZoneClass Class; public Direction Direction;
            public double RiskPercent; public DateTime ExpiryUtc;
            public EntryStyle Entry; public int TouchBudget;
            public EntryPrice EntryAt; public WeekendPolicy Weekend;
            public string Id = ""; public double RewardRiskTarget;
            public List<string> Warnings = new List<string>();

            public static double ClassRiskCap(ZoneClass c) => c == ZoneClass.Red ? 1.0 : 0.5;
            public static double ClassRiskDefault(ZoneClass c) => c == ZoneClass.Red ? 0.8 : 0.4;
            public static int ClassDefaultExpiryDays(ZoneClass c) => c == ZoneClass.Red ? 42 : 14;

            public static ZoneRecord TryParse(string comment, DateTime nowUtc, out List<string> errors)
            {
                errors = new List<string>();
                if (string.IsNullOrWhiteSpace(comment)) return null;
                var tokens = comment.Trim().ToUpperInvariant().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length < 2) { errors.Add("need at least CLASS and DIRECTION, e.g. \"RED SELL\""); return null; }

                var rec = new ZoneRecord();
                switch (tokens[0])
                {
                    case "RED": rec.Class = ZoneClass.Red; break;
                    case "YEL": case "YELLOW": rec.Class = ZoneClass.Yellow; break;
                    default: errors.Add("first token must be RED or YEL, got '" + tokens[0] + "'"); return null;
                }
                switch (tokens[1])
                {
                    case "BUY": rec.Direction = Direction.Buy; break;
                    case "SELL": rec.Direction = Direction.Sell; break;
                    default: errors.Add("second token must be BUY or SELL, got '" + tokens[1] + "'"); return null;
                }

                rec.RiskPercent = ClassRiskDefault(rec.Class);
                rec.ExpiryUtc = EndOfUtcDay(nowUtc.Date.AddDays(ClassDefaultExpiryDays(rec.Class)));
                rec.Entry = rec.Class == ZoneClass.Red ? EntryStyle.T1RestingLimit : EntryStyle.T2TouchConfirm;
                rec.TouchBudget = rec.Class == ZoneClass.Red ? 2 : 1;
                rec.EntryAt = rec.Class == ZoneClass.Red ? EntryPrice.Mid : EntryPrice.Edge;
                rec.Weekend = WeekendPolicy.Flat;
                rec.RewardRiskTarget = rec.Class == ZoneClass.Red ? 2.5 : 2.0;

                for (int i = 2; i < tokens.Length; i++)
                {
                    var t = tokens[i]; double d; int n; DateTime dt;
                    if (t[0] == 'R' && LooksNumeric(t, 1))
                    {
                        if (t.Length > 1 && double.TryParse(t.Substring(1), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d))
                        {
                            var cap = ClassRiskCap(rec.Class);
                            if (d < 0.1) errors.Add("risk below the 0.1% floor: " + t);
                            else if (d > cap) errors.Add(string.Format(CultureInfo.InvariantCulture,
                                "risk {0}% above the {1} cap {2}% — rejected, not clamped", d, rec.Class, cap));
                            else rec.RiskPercent = d;
                        }
                        else errors.Add("bad risk token (write e.g. R0.8, decimal point, no % sign): " + t);
                    }
                    else if (t.StartsWith("EXP:", StringComparison.Ordinal))
                    {
                        if (DateTime.TryParseExact(t.Substring(4), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dt))
                            rec.ExpiryUtc = EndOfUtcDay(dt.Date);
                        else errors.Add("bad date (use EXP:yyyy-mm-dd): " + t);
                    }
                    else if (t == "T1") rec.Entry = EntryStyle.T1RestingLimit;
                    else if (t == "T2") rec.Entry = EntryStyle.T2TouchConfirm;
                    else if (t.StartsWith("TCH:", StringComparison.Ordinal))
                    {
                        if (int.TryParse(t.Substring(4), NumberStyles.Integer, CultureInfo.InvariantCulture, out n) && n >= 1 && n <= 3)
                            rec.TouchBudget = n;
                        else errors.Add("touch budget must be 1..3: " + t);
                    }
                    else if (t == "E:MID") rec.EntryAt = EntryPrice.Mid;
                    else if (t == "E:EDGE") rec.EntryAt = EntryPrice.Edge;
                    else if (t == "WKD:FLAT") rec.Weekend = WeekendPolicy.Flat;
                    else if (t == "WKD:HOLD") rec.Weekend = WeekendPolicy.Hold;
                    else if (t.StartsWith("ID:", StringComparison.Ordinal) && t.Length > 3) rec.Id = t.Substring(3);
                    else rec.Warnings.Add("ignored unknown token: " + t);
                }
                return errors.Count > 0 ? null : rec;
            }

            public static string NewId()
            {
                var g = Guid.NewGuid().ToByteArray();
                return BitConverter.ToString(g, 0, 4).Replace("-", "");
            }

            private static bool LooksNumeric(string t, int from)
            {
                if (t.Length <= from) return true;
                for (int i = from; i < t.Length; i++)
                { var c = t[i]; if ((c < '0' || c > '9') && c != '.' && c != ',') return false; }
                return true;
            }

            private static DateTime EndOfUtcDay(DateTime utcDate) =>
                DateTime.SpecifyKind(utcDate.Date, DateTimeKind.Utc).AddDays(1).AddSeconds(-1);
        }

        public sealed class ZoneGeometry
        {
            public string Symbol; public double Top; public double Bottom; public Direction Direction;
            public double Height => Top - Bottom; public double Mid => (Top + Bottom) / 2.0;
        }

        public sealed class ValidationResult
        {
            public List<string> Errors = new List<string>();
            public List<string> Warnings = new List<string>();
            public bool Ok => Errors.Count == 0;
        }

        public static class ZoneValidator
        {
            public const double MinHeightAtr = 0.15, MaxHeightAtr = 3.0, MaxDistanceAtr = 15.0;
            public const int MaxExpiryDays = 84;

            public static ValidationResult Validate(ZoneRecord rec, ZoneGeometry geo, double dailyAtr,
                double currentPrice, DateTime nowUtc, IEnumerable<ZoneGeometry> existing)
            {
                var r = new ValidationResult();
                if (geo.Top <= geo.Bottom) r.Errors.Add("rectangle has no height (top <= bottom)");
                if (double.IsNaN(dailyAtr) || dailyAtr <= 0)
                { r.Errors.Add("daily ATR unavailable — cannot sanity-check; refuse to arm"); return r; }
                if (!r.Ok) return r;

                double hAtr = geo.Height / dailyAtr;
                if (hAtr < MinHeightAtr) r.Errors.Add(Fmt2("zone too thin: {0:0.00}x daily ATR (min {1})", hAtr, MinHeightAtr));
                if (hAtr > MaxHeightAtr) r.Errors.Add(Fmt2("zone too tall: {0:0.00}x daily ATR (max {1})", hAtr, MaxHeightAtr));

                double distance = currentPrice > geo.Top ? currentPrice - geo.Top
                                : currentPrice < geo.Bottom ? geo.Bottom - currentPrice : 0.0;
                if (distance / dailyAtr > MaxDistanceAtr)
                    r.Errors.Add(Fmt2("zone too far from price: {0:0.0}x daily ATR (max {1})", distance / dailyAtr, MaxDistanceAtr));

                if (rec.ExpiryUtc <= nowUtc) r.Errors.Add("expiry is in the past");
                else if ((rec.ExpiryUtc - nowUtc).TotalDays > MaxExpiryDays) r.Errors.Add(Fmt2("expiry more than {0} days out", MaxExpiryDays));

                double cap = ZoneRecord.ClassRiskCap(rec.Class);
                if (rec.RiskPercent > cap) r.Errors.Add(Fmt2("risk {0:0.0}% above class cap {1:0.0}%", rec.RiskPercent, cap));

                foreach (var zz in existing ?? Enumerable.Empty<ZoneGeometry>())
                {
                    if (!string.Equals(zz.Symbol, geo.Symbol, StringComparison.OrdinalIgnoreCase)) continue;
                    if (zz.Direction != rec.Direction) continue;
                    if (geo.Bottom < zz.Top && geo.Top > zz.Bottom)
                        r.Errors.Add("overlaps existing same-direction zone [" + Fmt2("{0:0.#####}-{1:0.#####}", zz.Bottom, zz.Top) + "]");
                }
                return r;
            }
            private static string Fmt2(string f, params object[] a) => string.Format(CultureInfo.InvariantCulture, f, a);
        }

        public sealed class OpenExposure
        { public string Symbol; public Direction Direction; public double RiskPercentAtEntry; }

        public sealed class SizingResult
        {
            public double VolumeUnits; public double AchievedRiskPercent;
            public List<string> Errors = new List<string>(); public List<string> Warnings = new List<string>();
            public bool Ok => Errors.Count == 0 && VolumeUnits > 0;
        }

        public static class RiskEngine
        {
            public const double RiskRoundingTolerance = 0.10;

            public static SizingResult ComputeVolume(ZoneRecord rec, double equity, double stopDistance,
                double tickSize, double tickValue, double volumeStep, double volumeMin)
            {
                var res = new SizingResult();
                if (equity <= 0) { res.Errors.Add("equity must be positive"); return res; }
                if (stopDistance <= 0) { res.Errors.Add("stop distance must be positive"); return res; }
                if (tickSize <= 0 || tickValue <= 0 || volumeStep <= 0)
                { res.Errors.Add("bad symbol economics (tickSize/tickValue/volumeStep)"); return res; }

                double riskMoney = equity * rec.RiskPercent / 100.0;
                double lossPerUnit = stopDistance / tickSize * tickValue;
                if (lossPerUnit <= 0) { res.Errors.Add("loss-per-unit computed as zero"); return res; }

                double rawUnits = riskMoney / lossPerUnit;
                double stepped = Math.Floor(rawUnits / volumeStep + 1e-9) * volumeStep;
                if (stepped < volumeMin)
                { res.Errors.Add(Fmt3("size below broker minimum ({0} < {1}) — skip rather than oversize", stepped, volumeMin)); return res; }

                res.VolumeUnits = stepped;
                res.AchievedRiskPercent = stepped * lossPerUnit / equity * 100.0;
                double dev = Math.Abs(res.AchievedRiskPercent - rec.RiskPercent) / rec.RiskPercent;
                if (dev > RiskRoundingTolerance)
                    res.Warnings.Add(Fmt3("achieved risk {0:0.00}% deviates {1:0}% from configured after rounding",
                                          res.AchievedRiskPercent, dev * 100));
                return res;
            }

            public static List<string> CheckCaps(ZoneRecord rec, string symbol, IReadOnlyList<OpenExposure> open)
            {
                var errors = new List<string>();
                open = open ?? new List<OpenExposure>();
                if (open.Count >= ZoneExecCaps.MaxOpenPositions)
                    errors.Add(Fmt3("max positions/orders reached ({0})", ZoneExecCaps.MaxOpenPositions));
                double totalAfter = open.Sum(o => o.RiskPercentAtEntry) + rec.RiskPercent;
                if (totalAfter > ZoneExecCaps.MaxTotalOpenRiskPercent + 1e-9)
                    errors.Add(Fmt3("total open risk would be {0:0.0}% (max {1:0.0}%)", totalAfter, ZoneExecCaps.MaxTotalOpenRiskPercent));

                var after = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                foreach (var o in open) AddFactors(after, o.Symbol, o.Direction, o.RiskPercentAtEntry);
                AddFactors(after, symbol, rec.Direction, rec.RiskPercent);
                foreach (var kv in after)
                    if (Math.Abs(kv.Value) > ZoneExecCaps.MaxFactorRiskPercent + 1e-9)
                        errors.Add(Fmt3("factor '{0}' exposure would be {1:+0.0;-0.0}% (cap ±{2:0.0}%)", kv.Key, kv.Value, ZoneExecCaps.MaxFactorRiskPercent));
                return errors;
            }

            public static void AddFactors(IDictionary<string, double> acc, string symbol, Direction dir, double riskPercent)
            {
                double sign = dir == Direction.Buy ? 1.0 : -1.0;
                foreach (var (factor, weight) in FactorLegs(symbol))
                { double v; acc.TryGetValue(factor, out v); acc[factor] = v + sign * weight * riskPercent; }
            }

            public static IEnumerable<(string factor, double weight)> FactorLegs(string symbol)
            {
                var s = (symbol ?? "").ToUpperInvariant().Replace("/", "").Replace("_", "").Replace(".CASH", "").Replace(".", "");
                string[] fx = { "USD","EUR","GBP","JPY","AUD","NZD","CAD","CHF","SGD","MYR","CNH",
                                "NOK","SEK","DKK","ZAR","MXN","TRY","HKD","PLN","HUF","CZK" };
                if (s.StartsWith("XAU")) return Legs(("GOLD", 1), (Quote(s, "XAU"), -1));
                if (s.StartsWith("XAG")) return Legs(("SILVER", 1), (Quote(s, "XAG"), -1));
                if (s.Contains("US500") || s.Contains("SPX")) return Legs(("US_EQ", 1), ("RISK_ON", 1));
                if (s.Contains("US30") || s.Contains("DJ")) return Legs(("US_EQ", 1), ("RISK_ON", 1));
                if (s.Contains("NAS") || s.Contains("US100") || s.Contains("USTEC")) return Legs(("US_EQ", 1), ("RISK_ON", 1));
                if (s.Contains("DE40") || s.Contains("GER") || s.Contains("DAX")) return Legs(("EU_EQ", 1), ("RISK_ON", 1));
                if (s.Contains("UK100") || s.Contains("FTSE")) return Legs(("EU_EQ", 1), ("RISK_ON", 1));
                if (s.Contains("XTI") || s.Contains("WTI") || s.Contains("OIL") || s.Contains("BRENT")) return Legs(("OIL", 1), ("RISK_ON", 1));
                if (s.Length >= 6)
                {
                    var b = s.Substring(0, 3); var q = s.Substring(3, 3);
                    bool bK = Array.IndexOf(fx, b) >= 0, qK = Array.IndexOf(fx, q) >= 0;
                    if (bK && qK) return Legs((b, 1), (q, -1));
                    if (bK) return Legs((b, 1), (s, 1));
                    if (qK) return Legs((q, -1), (s, 1));
                }
                return Legs((s.Length == 0 ? "UNKNOWN" : s, 1));
            }
            private static string Quote(string s, string prefix)
            { var rest = s.Substring(prefix.Length); return rest.Length >= 3 ? rest.Substring(0, 3) : "USD"; }
            private static IEnumerable<(string, double)> Legs(params (string, double)[] legs) => legs;
            private static string Fmt3(string f, params object[] a) => string.Format(CultureInfo.InvariantCulture, f, a);
        }

        public static class ZoneExecCaps
        {
            public const int MaxOpenPositions = 3;
            public const double MaxTotalOpenRiskPercent = 3.0;
            public const double MaxFactorRiskPercent = 2.0;
        }
    }
}
