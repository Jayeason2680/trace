// ============================================================================
// ZoneExec v1 — Zone Console, Session 2 (DEMO ONLY until every gate passes)
// Turns configured rectangles into risk-capped resting orders with broker-side
// SL/TP and broker-side expiry. FTMO NORMAL rules are law in this build:
//   - WKD:HOLD rejected; ALL zone positions/orders flatten Friday (UTC time param)
//   - news pause: no arming and resting orders suspended ±NewsPauseMinutes
//     around timestamps listed in Documents\ZoneConsole\news.txt (UTC)
//   - daily cut-out −3% and account breaker −8%, both anchored to the FTMO
//     initial balance, daily reset at midnight Prague time
// Kill switch without Telegram: create Documents\ZoneConsole\KILL.txt →
// every ZC position closes, every ZC order cancels, arming latches off until
// the file is deleted.
//
// Self-contained single file for paste-once install. The canonical, testable
// core lives in src/ZoneExec/{ZoneRecord,ZoneValidator,RiskEngine}.cs — keep
// logic changes mirrored there.
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

        [Parameter("News pause ± minutes", DefaultValue = 15, MinValue = 2, Group = "FTMO rules")]
        public int NewsPauseMinutes { get; set; }

        [Parameter("Friday flatten time UTC (HH:mm)", DefaultValue = "20:30", Group = "FTMO rules")]
        public string FridayFlattenUtc { get; set; }

        // ---------------- constants (mirror RiskEngine.cs) ----------------
        private const int MaxOpenPositions = 3;
        private const double MaxTotalOpenRiskPercent = 3.0;
        private const double MaxFactorRiskPercent = 2.0;
        private const double DailyCutoutPercent = 3.0;    // of FTMO initial balance
        private const double AccountBreakerPercent = 8.0; // of FTMO initial balance
        private const double SlBufferFrac = 0.25;         // SL beyond far edge, fraction of height
        private const string LabelPrefix = "ZC:";

        private static readonly HttpClient HttpShared = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };

        // ---------------- per-zone runtime state ----------------
        private enum ZStatus { Draft, Rejected, Armed, WaitingConfirm, Filled, Retired, Expired, Killed }

        private sealed class Zone
        {
            public string RectName;
            public string Id = "";
            public ZoneRecord Rec;
            public double Top, Bottom;
            public ZStatus Status = ZStatus.Draft;
            public int TouchesUsed;
            public bool InTouch;
            public string LastComment = "";
            public DateTime LastRejectJournal = DateTime.MinValue;
        }

        private readonly Dictionary<string, Zone> _zones = new Dictionary<string, Zone>();
        private string _dir, _journalPath, _statePath, _newsPath, _killPath;
        private int _secToHeartbeat; private int _hbFail;
        private DateTime _lastDigestUtc = DateTime.MinValue;
        private double _initialBalance;                    // FTMO anchor
        private double _dayStartRef; private DateTime _dayStartDatePrague = DateTime.MinValue;
        private DateTime _dailyTrippedUntilUtc = DateTime.MinValue;
        private bool _accountTripped, _killLatched, _newsSuspended, _selfWrite;
        private AverageTrueRange _dailyAtr;
        private Bars _dailyBars, _m15Bars;
        private DateTime _lastM15Seen = DateTime.MinValue;
        private TimeZoneInfo _prague;

        // ============================================================ lifecycle
        protected override void OnStart()
        {
            _dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ZoneConsole");
            Directory.CreateDirectory(Path.Combine(_dir, "journal"));
            var baseName = SymbolName + "-" + Chart.TimeFrame + "-exec";
            _journalPath = Path.Combine(_dir, "journal", baseName + ".jsonl");
            _statePath = Path.Combine(_dir, "journal", baseName + ".state");
            _newsPath = Path.Combine(_dir, "news.txt");
            _killPath = Path.Combine(_dir, "KILL.txt");

            try { _prague = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"); }
            catch { _prague = TimeZoneInfo.CreateCustomTimeZone("CET+1", TimeSpan.FromHours(1), "CET+1", "CET+1"); }

            _dailyBars = MarketData.GetBars(TimeFrame.Daily);
            _dailyAtr = Indicators.AverageTrueRange(_dailyBars, 14, MovingAverageType.Simple);
            _m15Bars = MarketData.GetBars(TimeFrame.Minute15);

            LoadState();
            if (_initialBalance <= 0)
            {
                _initialBalance = InitialBalanceParam > 0 ? InitialBalanceParam : Account.Balance;
                SaveState();
                Journal("anchor_set", null, ("initial_balance", F(_initialBalance)));
            }

            Chart.ObjectsAdded += e => { if (!_selfWrite) foreach (var r in e.ChartObjects.OfType<ChartRectangle>()) SyncZone(r); };
            Chart.ObjectsUpdated += e => { if (!_selfWrite) foreach (var r in e.ChartObjects.OfType<ChartRectangle>()) SyncZone(r); };
            Chart.ObjectsRemoved += e => { foreach (var r in e.ChartObjects.OfType<ChartRectangle>()) OnZoneRemoved(r.Name); };

            ReconcileBrokerState();
            foreach (var r in Chart.Objects.OfType<ChartRectangle>()) SyncZone(r);

            _secToHeartbeat = HeartbeatSeconds;
            Timer.Start(TimeSpan.FromSeconds(1));
            Journal("exec_start", null, ("zones", _zones.Count.ToString(CultureInfo.InvariantCulture)),
                    ("initial_balance", F(_initialBalance)), ("account_is_live", IsBacktesting ? "backtest" : Account.IsLive ? "LIVE" : "demo"));
            if (Account.IsLive)
                Journal("warning", null, ("msg", "LIVE ACCOUNT DETECTED - demo gate not waived by code; proceed only if gates passed"));
        }

        protected override void OnStop() { Journal("exec_stop", null); SaveState(); }

        // ============================================================ zone sync (draw/edit)
        private void SyncZone(ChartRectangle rect)
        {
            Zone z;
            if (!_zones.TryGetValue(rect.Name, out z))
            {
                z = new Zone { RectName = rect.Name };
                _zones[rect.Name] = z;
            }
            double top = Math.Max(rect.Y1, rect.Y2), bottom = Math.Min(rect.Y1, rect.Y2);
            var comment = rect.Comment ?? "";
            bool changed = comment != z.LastComment || top != z.Top || bottom != z.Bottom;
            z.Top = top; z.Bottom = bottom;
            if (!changed) return;

            bool commentChanged = comment != z.LastComment;
            z.LastComment = comment;

            // An armed zone whose comment or geometry changes is DISARMED first —
            // the drawing is the instruction; if the instruction changed, re-approve it.
            if ((z.Status == ZStatus.Armed || z.Status == ZStatus.WaitingConfirm) && changed)
            {
                CancelZoneOrders(z, "zone_edited");
                z.Status = ZStatus.Draft;
            }

            List<string> errors;
            var rec = ZoneRecord.TryParse(comment, Server.Time, out errors);
            if (rec == null)
            {
                if (errors.Count > 0 && commentChanged)
                {
                    z.Status = ZStatus.Rejected;
                    Journal("zone_rejected", z, ("errors", string.Join(" | ", errors)));
                    Paint(rect, "#808080");
                }
                else if (string.IsNullOrWhiteSpace(comment))
                    z.Status = ZStatus.Draft; // silent draft
                z.Rec = null;
                return;
            }

            // FTMO Normal law: no weekend holds, ever.
            if (rec.Weekend == WeekendPolicy.Hold)
            {
                z.Status = ZStatus.Rejected; z.Rec = null;
                Journal("zone_rejected", z, ("errors", "WKD:HOLD not allowed on FTMO Normal - all zones flatten before the weekend"));
                Paint(rect, "#808080");
                return;
            }

            z.Rec = rec;
            if (string.IsNullOrEmpty(rec.Id))
            {
                rec.Id = ZoneRecord.NewId();
                WriteBackId(rect, rec.Id);
            }
            z.Id = rec.Id;
            TryArm(z, rect);
        }

        private void OnZoneRemoved(string name)
        {
            Zone z;
            if (!_zones.TryGetValue(name, out z)) return;
            CancelZoneOrders(z, "zone_removed");
            _zones.Remove(name);
            Journal("zone_removed", z);
        }

        // ============================================================ arming
        private void TryArm(Zone z, ChartRectangle rect)
        {
            if (_killLatched || _accountTripped || Server.Time < _dailyTrippedUntilUtc || _newsSuspended)
            { JournalRejectOnce(z, "blocked", _killLatched ? "kill latched" : _accountTripped ? "account breaker" :
                Server.Time < _dailyTrippedUntilUtc ? "daily cut-out" : "news window"); return; }

            double atr = _dailyAtr.Result.LastValue;
            double mid = (Symbol.Bid + Symbol.Ask) / 2.0;
            var geo = new ZoneGeometry { Symbol = SymbolName, Top = z.Top, Bottom = z.Bottom, Direction = z.Rec.Direction };
            var others = _zones.Values.Where(o => o != z && o.Rec != null &&
                            (o.Status == ZStatus.Armed || o.Status == ZStatus.WaitingConfirm || o.Status == ZStatus.Filled))
                         .Select(o => new ZoneGeometry { Symbol = SymbolName, Top = o.Top, Bottom = o.Bottom, Direction = o.Rec.Direction });
            var val = ZoneValidator.Validate(z.Rec, geo, atr, mid, Server.Time, others);
            if (!val.Ok)
            { z.Status = ZStatus.Rejected; Journal("zone_rejected", z, ("errors", string.Join(" | ", val.Errors))); Paint(rect, "#808080"); return; }
            foreach (var w in val.Warnings) Journal("zone_warning", z, ("warning", w));

            var spreadPips = (Symbol.Ask - Symbol.Bid) / Symbol.PipSize;
            if (spreadPips > MaxSpreadPips)
            { JournalRejectOnce(z, "anomaly", Fmt("spread {0:0.0} pips > max {1:0.0} - waiting", spreadPips, MaxSpreadPips)); return; }

            var capErrors = CheckCapsLive(z.Rec);
            if (capErrors.Count > 0)
            { JournalRejectOnce(z, "caps", string.Join(" | ", capErrors)); return; }

            // ---- prices ----
            bool buy = z.Rec.Direction == Direction.Buy;
            double height = z.Top - z.Bottom;
            double entry = z.Rec.EntryAt == EntryPrice.Mid ? (z.Top + z.Bottom) / 2.0 : (buy ? z.Top : z.Bottom);
            double sl = buy ? z.Bottom - height * SlBufferFrac : z.Top + height * SlBufferFrac;
            double stopDist = Math.Abs(entry - sl);
            double tp = buy ? entry + z.Rec.RewardRiskTarget * stopDist : entry - z.Rec.RewardRiskTarget * stopDist;

            var sizing = ComputeVolumeLive(z.Rec, stopDist);
            if (!sizing.Ok)
            { z.Status = ZStatus.Rejected; Journal("zone_rejected", z, ("errors", string.Join(" | ", sizing.Errors))); Paint(rect, "#808080"); return; }
            foreach (var w in sizing.Warnings) Journal("zone_warning", z, ("warning", w));

            if (z.Rec.Entry == EntryStyle.T1RestingLimit)
            {
                var label = LabelPrefix + z.Id;
                var expiry = OrderExpiryUtc(z.Rec);
                double slPips = stopDist / Symbol.PipSize;
                double tpPips = Math.Abs(tp - entry) / Symbol.PipSize;
                var res = PlaceLimitOrder(buy ? TradeType.Buy : TradeType.Sell, SymbolName,
                                          sizing.VolumeUnits, entry, label, slPips, tpPips, expiry);
                if (!res.IsSuccessful)
                { JournalRejectOnce(z, "order_rejected", res.Error.ToString()); return; }
                z.Status = ZStatus.Armed;
                Journal("zone_armed", z, ("entry", F(entry)), ("sl", F(sl)), ("tp", F(tp)),
                        ("volume", F(sizing.VolumeUnits)), ("risk_pct", F(sizing.AchievedRiskPercent)),
                        ("order_expiry_utc", expiry.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)),
                        ("style", "T1"));
                Paint(rect, buy ? "#0ca30c" : "#d03b3b");
            }
            else // T2: no resting order; wait for touch, then one M15 close in our favour
            {
                z.Status = ZStatus.WaitingConfirm;
                Journal("zone_armed", z, ("entry", "on-confirm"), ("sl", F(sl)), ("tp", F(tp)),
                        ("volume", F(sizing.VolumeUnits)), ("style", "T2"));
                Paint(rect, "#eda100");
            }
        }

        // ============================================================ ticks: touches + T2
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
                    SaveState();
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
            // acts once per closed M15 bar
            int last = _m15Bars.Count - 2; if (last < 1) return;
            var barTime = _m15Bars.OpenTimes[last]; if (barTime == _lastM15Seen) return;
            _lastM15Seen = barTime;
            double close = _m15Bars.ClosePrices[last];

            foreach (var z in _zones.Values)
            {
                if (z.Rec == null || z.Status != ZStatus.WaitingConfirm) continue;
                if (z.TouchesUsed == 0 || z.TouchesUsed > z.Rec.TouchBudget) continue;
                bool buy = z.Rec.Direction == Direction.Buy;
                double zmid = (z.Top + z.Bottom) / 2.0;
                bool confirmed = buy ? close > z.Top : close < z.Bottom; // closed back beyond the zone in our favour
                if (!confirmed) continue;
                if (_newsSuspended || _killLatched || _accountTripped || Server.Time < _dailyTrippedUntilUtc) continue;

                double height = z.Top - z.Bottom;
                double sl = buy ? z.Bottom - height * SlBufferFrac : z.Top + height * SlBufferFrac;
                double entry = buy ? Symbol.Ask : Symbol.Bid;
                double stopDist = Math.Abs(entry - sl); if (stopDist <= 0) continue;
                double tp = buy ? entry + z.Rec.RewardRiskTarget * stopDist : entry - z.Rec.RewardRiskTarget * stopDist;
                var sizing = ComputeVolumeLive(z.Rec, stopDist);
                var capErr = CheckCapsLive(z.Rec);
                if (!sizing.Ok || capErr.Count > 0)
                { Journal("t2_entry_blocked", z, ("errors", string.Join(" | ", sizing.Errors.Concat(capErr)))); continue; }

                var res = ExecuteMarketOrder(buy ? TradeType.Buy : TradeType.Sell, SymbolName, sizing.VolumeUnits,
                                             LabelPrefix + z.Id, stopDist / Symbol.PipSize, Math.Abs(tp - entry) / Symbol.PipSize);
                if (res.IsSuccessful)
                {
                    z.Status = ZStatus.Filled;
                    Journal("t2_entered", z, ("entry", F(entry)), ("sl", F(sl)), ("tp", F(tp)), ("volume", F(sizing.VolumeUnits)));
                }
                else Journal("t2_entry_blocked", z, ("errors", res.Error.ToString()));
            }
        }

        // ============================================================ timer: the guardian loop
        protected override void OnTimer()
        {
            var now = Server.Time;

            if (--_secToHeartbeat <= 0) { _secToHeartbeat = HeartbeatSeconds; Heartbeat(); }

            // kill file — checked every second, latches
            bool killNow = File.Exists(_killPath);
            if (killNow && !_killLatched)
            {
                _killLatched = true; SaveState();
                Journal("killswitch", null, ("action", "flatten+cancel all ZC, arming latched off"));
                FlattenEverything("killswitch");
            }
            else if (!killNow && _killLatched)
            {
                _killLatched = false; SaveState();
                Journal("killswitch_cleared", null, ("note", "zones stay disarmed; re-arm by re-saving each comment"));
                foreach (var z in _zones.Values) if (z.Status == ZStatus.Killed) z.Status = ZStatus.Draft;
            }

            UpdateBreakers(now);
            UpdateNewsWindow(now);
            FridayFlattenCheck(now);
            ExpirySweep(now);
            CheckT2Confirms();
            SyncFills();

            if (now.Hour >= DigestHourUtc && _lastDigestUtc.Date != now.Date)
            { _lastDigestUtc = now; SaveState(); Digest(); }
        }

        private void UpdateBreakers(DateTime nowUtc)
        {
            // Prague-midnight day anchor, FTMO-style: reference = max(balance, equity) at day start
            var prg = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, _prague).Date;
            if (prg != _dayStartDatePrague)
            {
                _dayStartDatePrague = prg;
                _dayStartRef = Math.Max(Account.Balance, Account.Equity);
                SaveState();
                Journal("day_anchor", null, ("ref", F(_dayStartRef)), ("prague_date", prg.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
            }

            if (!_accountTripped && Account.Equity <= _initialBalance * (1 - AccountBreakerPercent / 100.0))
            {
                _accountTripped = true; SaveState();
                Journal("account_breaker", null, ("equity", F(Account.Equity)),
                        ("limit", F(_initialBalance * (1 - AccountBreakerPercent / 100.0))),
                        ("action", "cancel all ZC orders; positions keep broker-side stops; written review required"));
                CancelAllZcOrders("account_breaker");
            }

            if (nowUtc >= _dailyTrippedUntilUtc &&
                _dayStartRef - Account.Equity >= _initialBalance * DailyCutoutPercent / 100.0)
            {
                // latch until next Prague midnight, expressed in UTC
                var nextPragueMidnight = TimeZoneInfo.ConvertTimeToUtc(prg.AddDays(1), _prague);
                _dailyTrippedUntilUtc = nextPragueMidnight; SaveState();
                Journal("daily_cutout", null, ("drawdown", F(_dayStartRef - Account.Equity)),
                        ("until_utc", nextPragueMidnight.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)),
                        ("action", "no new risk today; resting ZC orders cancelled; positions keep broker-side stops"));
                CancelAllZcOrders("daily_cutout");
            }
        }

        private void UpdateNewsWindow(DateTime nowUtc)
        {
            bool inWindow = false;
            try
            {
                if (File.Exists(_newsPath))
                    foreach (var line in File.ReadAllLines(_newsPath))
                    {
                        var t = line.Trim(); if (t.Length == 0 || t.StartsWith("#")) continue;
                        DateTime ev;
                        if (DateTime.TryParseExact(t, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out ev) &&
                            Math.Abs((nowUtc - ev).TotalMinutes) <= NewsPauseMinutes)
                        { inWindow = true; break; }
                    }
            }
            catch (Exception ex) { Print("news read failed: {0}", ex.Message); }

            if (inWindow && !_newsSuspended)
            {
                _newsSuspended = true;
                Journal("news_pause_start", null, ("action", "resting ZC orders cancelled; re-arm after window"));
                CancelAllZcOrders("news_pause");
            }
            else if (!inWindow && _newsSuspended)
            {
                _newsSuspended = false;
                Journal("news_pause_end", null, ("action", "re-arming eligible zones"));
                RearmAll();
            }
        }

        private void FridayFlattenCheck(DateTime nowUtc)
        {
            TimeSpan cut;
            if (!TimeSpan.TryParseExact(FridayFlattenUtc, "hh\\:mm", CultureInfo.InvariantCulture, out cut))
                cut = new TimeSpan(20, 30, 0);
            if (nowUtc.DayOfWeek == DayOfWeek.Friday && nowUtc.TimeOfDay >= cut)
            {
                bool anything = Positions.Any(p => (p.Label ?? "").StartsWith(LabelPrefix) && p.SymbolName == SymbolName) ||
                                PendingOrders.Any(o => (o.Label ?? "").StartsWith(LabelPrefix) && o.SymbolName == SymbolName);
                if (anything)
                {
                    Journal("weekend_flatten", null, ("rule", "FTMO Normal: no weekend holds"));
                    FlattenEverything("weekend_flatten");
                }
            }
        }

        private void ExpirySweep(DateTime nowUtc)
        {
            foreach (var z in _zones.Values)
            {
                if (z.Rec == null) continue;
                if (nowUtc > z.Rec.ExpiryUtc && (z.Status == ZStatus.Armed || z.Status == ZStatus.WaitingConfirm || z.Status == ZStatus.Draft))
                {
                    CancelZoneOrders(z, "expired");
                    z.Status = ZStatus.Expired;
                    Journal("zone_expired", z);
                }
            }
        }

        private void SyncFills()
        {
            foreach (var z in _zones.Values)
            {
                if (z.Rec == null) continue;
                bool hasPos = Positions.Any(p => (p.Label ?? "") == LabelPrefix + z.Id && p.SymbolName == SymbolName);
                if (hasPos && z.Status == ZStatus.Armed)
                { z.Status = ZStatus.Filled; Journal("zone_filled", z); }
                else if (!hasPos && z.Status == ZStatus.Filled)
                {
                    var stillPending = PendingOrders.Any(o => (o.Label ?? "") == LabelPrefix + z.Id && o.SymbolName == SymbolName);
                    if (!stillPending)
                    {
                        z.Status = z.TouchesUsed >= z.Rec.TouchBudget ? ZStatus.Retired : ZStatus.Draft;
                        Journal("position_closed", z, ("touches_used", z.TouchesUsed.ToString(CultureInfo.InvariantCulture)),
                                ("next", z.Status.ToString()));
                        // Draft → the guardian loop re-arms it on the next SyncZone pass if budget remains
                        if (z.Status == ZStatus.Draft) RearmZoneSoon(z);
                    }
                }
            }
        }

        // ============================================================ order plumbing
        private DateTime OrderExpiryUtc(ZoneRecord rec)
        {
            // broker-side expiry = min(zone expiry, next Friday flatten) — a dead VPS
            // can then never fill a stale idea (audit fix)
            var expiry = rec.ExpiryUtc;
            var now = Server.Time;
            int daysToFriday = ((int)DayOfWeek.Friday - (int)now.DayOfWeek + 7) % 7;
            TimeSpan cut;
            if (!TimeSpan.TryParseExact(FridayFlattenUtc, "hh\\:mm", CultureInfo.InvariantCulture, out cut))
                cut = new TimeSpan(20, 30, 0);
            var friday = now.Date.AddDays(daysToFriday).Add(cut);
            if (friday <= now) friday = friday.AddDays(7);
            return expiry < friday ? expiry : friday;
        }

        private void CancelZoneOrders(Zone z, string reason)
        {
            foreach (var o in PendingOrders.Where(o => (o.Label ?? "") == LabelPrefix + z.Id && o.SymbolName == SymbolName).ToList())
            { o.Cancel(); Journal("order_cancelled", z, ("reason", reason)); }
        }

        private void CancelAllZcOrders(string reason)
        {
            foreach (var o in PendingOrders.Where(o => (o.Label ?? "").StartsWith(LabelPrefix) && o.SymbolName == SymbolName).ToList())
            { o.Cancel(); }
            foreach (var z in _zones.Values)
                if (z.Status == ZStatus.Armed) { z.Status = ZStatus.Draft; }
            Journal("orders_cancelled_all", null, ("reason", reason));
        }

        private void FlattenEverything(string reason)
        {
            foreach (var o in PendingOrders.Where(o => (o.Label ?? "").StartsWith(LabelPrefix) && o.SymbolName == SymbolName).ToList())
                o.Cancel();
            foreach (var p in Positions.Where(p => (p.Label ?? "").StartsWith(LabelPrefix) && p.SymbolName == SymbolName).ToList())
                p.Close();
            foreach (var z in _zones.Values)
                if (z.Status == ZStatus.Armed || z.Status == ZStatus.WaitingConfirm || z.Status == ZStatus.Filled)
                    z.Status = reason == "killswitch" ? ZStatus.Killed : ZStatus.Draft;
            Journal("flatten_all", null, ("reason", reason));
        }

        private void RearmAll()
        {
            _selfWrite = false;
            foreach (var rect in Chart.Objects.OfType<ChartRectangle>()) SyncZone(rect);
        }

        private void RearmZoneSoon(Zone z)
        {
            var rect = Chart.Objects.OfType<ChartRectangle>().FirstOrDefault(r => r.Name == z.RectName);
            if (rect != null && z.Rec != null) TryArm(z, rect);
        }

        private void ReconcileBrokerState()
        {
            // Orders/positions labelled ZC: with no matching rectangle on this chart:
            // cancel orders (safe), keep positions but alarm loudly (human decides).
            var known = new HashSet<string>(Chart.Objects.OfType<ChartRectangle>()
                        .Select(r => { List<string> e; var rec = ZoneRecord.TryParse(r.Comment ?? "", Server.Time, out e); return rec != null ? LabelPrefix + rec.Id : null; })
                        .Where(x => x != null));
            foreach (var o in PendingOrders.Where(o => (o.Label ?? "").StartsWith(LabelPrefix) && o.SymbolName == SymbolName).ToList())
                if (!known.Contains(o.Label))
                { o.Cancel(); Journal("orphan_order_cancelled", null, ("label", o.Label)); }
            foreach (var p in Positions.Where(p => (p.Label ?? "").StartsWith(LabelPrefix) && p.SymbolName == SymbolName))
                if (!known.Contains(p.Label))
                    Journal("orphan_position_alert", null, ("label", p.Label), ("note", "position without a zone - review manually; broker-side SL/TP still active"));
        }

        // ============================================================ live caps & sizing
        private List<string> CheckCapsLive(ZoneRecord rec)
        {
            var open = new List<OpenExposure>();
            foreach (var p in Positions.Where(p => (p.Label ?? "").StartsWith(LabelPrefix)))
            {
                // recover configured risk from label-linked zone if known; else estimate 1.0%
                double rp = 1.0;
                var z = _zones.Values.FirstOrDefault(x => x.Rec != null && LabelPrefix + x.Id == p.Label);
                if (z != null) rp = z.Rec.RiskPercent;
                open.Add(new OpenExposure { Symbol = p.SymbolName, Direction = p.TradeType == TradeType.Buy ? Direction.Buy : Direction.Sell, RiskPercentAtEntry = rp });
            }
            // pending ZC orders count toward concurrency too (they can all fill)
            foreach (var o in PendingOrders.Where(o => (o.Label ?? "").StartsWith(LabelPrefix)))
            {
                double rp = 1.0;
                var z = _zones.Values.FirstOrDefault(x => x.Rec != null && LabelPrefix + x.Id == o.Label);
                if (z != null) rp = z.Rec.RiskPercent;
                open.Add(new OpenExposure { Symbol = o.SymbolName, Direction = o.TradeType == TradeType.Buy ? Direction.Buy : Direction.Sell, RiskPercentAtEntry = rp });
            }
            return RiskEngine.CheckCaps(rec, SymbolName, open);
        }

        private SizingResult ComputeVolumeLive(ZoneRecord rec, double stopDistance)
        {
            // risk anchored to FTMO initial balance (not floating equity): predictable
            // % of the account you must protect
            return RiskEngine.ComputeVolume(rec, _initialBalance, stopDistance,
                Symbol.TickSize, Symbol.TickValue, Symbol.VolumeInUnitsStep, Symbol.VolumeInUnitsMin);
        }

        // ============================================================ chart + io helpers
        private void Paint(ChartRectangle rect, string hex)
        {
            try { _selfWrite = true; rect.Color = Color.FromHex(hex); }
            catch { } finally { _selfWrite = false; }
        }

        private void WriteBackId(ChartRectangle rect, string id)
        {
            try { _selfWrite = true; rect.Comment = (rect.Comment ?? "").TrimEnd() + " ID:" + id; }
            catch (Exception ex) { Print("id writeback failed: {0}", ex.Message); }
            finally { _selfWrite = false; }
        }

        private void JournalRejectOnce(Zone z, string kind, string detail)
        {
            if ((Server.Time - z.LastRejectJournal).TotalMinutes < 30) return; // no spam
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
                    ("account_tripped", _accountTripped.ToString()));
        }

        private void Heartbeat()
        {
            if (string.IsNullOrWhiteSpace(HeartbeatUrl)) return;
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
                        case "dailyTrippedUntilUtc": DateTime.TryParse(kv[1], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out _dailyTrippedUntilUtc); break;
                        case "accountTripped": bool.TryParse(kv[1], out _accountTripped); break;
                        case "killLatched": bool.TryParse(kv[1], out _killLatched); break;
                        case "lastDigestUtc": DateTime.TryParse(kv[1], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out _lastDigestUtc); break;
                        default:
                            if (kv[0].StartsWith("touches:"))
                            {
                                var name = kv[0].Substring(8); int n;
                                if (int.TryParse(kv[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out n))
                                { if (!_zones.ContainsKey(name)) _zones[name] = new Zone { RectName = name }; _zones[name].TouchesUsed = n; }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex) { Print("state load failed: {0}", ex.Message); }
        }

        private void SaveState()
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("initialBalance=").Append(F(_initialBalance)).AppendLine();
                sb.Append("dailyTrippedUntilUtc=").Append(_dailyTrippedUntilUtc.ToString("o", CultureInfo.InvariantCulture)).AppendLine();
                sb.Append("accountTripped=").Append(_accountTripped).AppendLine();
                sb.Append("killLatched=").Append(_killLatched).AppendLine();
                sb.Append("lastDigestUtc=").Append(_lastDigestUtc.ToString("o", CultureInfo.InvariantCulture)).AppendLine();
                foreach (var z in _zones.Values.Where(z => z.TouchesUsed > 0))
                    sb.Append("touches:").Append(z.RectName).Append('=').Append(z.TouchesUsed.ToString(CultureInfo.InvariantCulture)).AppendLine();
                File.WriteAllText(_statePath, sb.ToString());
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
                if (dailyAtr <= 0) { r.Errors.Add("daily ATR unavailable — cannot sanity-check; refuse to arm"); return r; }
                if (!r.Ok) return r;

                double hAtr = geo.Height / dailyAtr;
                if (hAtr < MinHeightAtr) r.Errors.Add(Fmt2("zone too thin: {0:0.00}x daily ATR (min {1})", hAtr, MinHeightAtr));
                if (hAtr > MaxHeightAtr) r.Errors.Add(Fmt2("zone too tall: {0:0.00}x daily ATR (max {1})", hAtr, MaxHeightAtr));

                double distance = currentPrice > geo.Top ? currentPrice - geo.Top
                                : currentPrice < geo.Bottom ? geo.Bottom - currentPrice : 0.0;
                if (distance / dailyAtr > MaxDistanceAtr)
                    r.Errors.Add(Fmt2("zone too far from price: {0:0.0}x daily ATR (max {1})", distance / dailyAtr, MaxDistanceAtr));
                if (distance == 0.0) r.Warnings.Add("price is already inside the zone — it would arm hot (first touch immediate)");

                if (rec.Direction == Direction.Sell && currentPrice > geo.Top)
                    r.Warnings.Add("SELL zone is below current price — check the direction");
                if (rec.Direction == Direction.Buy && currentPrice < geo.Bottom)
                    r.Warnings.Add("BUY zone is above current price — check the direction");

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
