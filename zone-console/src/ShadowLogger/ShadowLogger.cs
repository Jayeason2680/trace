// ============================================================================
// ShadowLogger v1.1 — Zone Console, Session 1 (post-audit)
// Watch-only cBot: logs rectangles ("zones") and price touches to a JSONL
// journal, sends a nightly Telegram digest, pings a heartbeat URL.
// CONTAINS NO ORDER CODE BY DESIGN — it cannot trade.
// Attach one instance to every chart you draw zones on.
//
// v1.1 audit fixes: non-blocking HTTP (never stalls ticks), band-based touch
// hysteresis (spread-spike safe), persisted digest date (no double-send after
// restart), per-chart journal file (two same-symbol charts can't collide),
// change-detected zone_modified (no drag floods), synthetic touch_end on
// removal/zero-height.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using File = System.IO.File; // cAlgo.API also defines a File type — pin the .NET one (CS0104)
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class ShadowLogger : Robot
    {
        [Parameter("Telegram bot token", DefaultValue = "", Group = "Alerts")]
        public string TelegramToken { get; set; }

        [Parameter("Telegram chat id", DefaultValue = "", Group = "Alerts")]
        public string TelegramChatId { get; set; }

        [Parameter("Heartbeat ping URL", DefaultValue = "", Group = "Alerts")]
        public string HeartbeatUrl { get; set; }

        [Parameter("Heartbeat every (sec)", DefaultValue = 60, MinValue = 30, Group = "Alerts")]
        public int HeartbeatSeconds { get; set; }

        [Parameter("Digest hour (UTC, 14 = 22:00 MYT)", DefaultValue = 14, MinValue = 0, MaxValue = 23, Group = "Alerts")]
        public int DigestHourUtc { get; set; }

        [Parameter("Touch hysteresis (fraction of zone height)", DefaultValue = 0.25, MinValue = 0.05, MaxValue = 1.0, Group = "Logic")]
        public double TouchHysteresis { get; set; }

        private static readonly HttpClient HttpShared = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };

        private sealed class RectState
        {
            public string Comment = "";
            public double Top, Bottom;      // last seen geometry, for change detection
            public bool Inside;
            public DateTime TouchStartUtc;
            public double TouchMin, TouchMax;
            public int TouchesToday;
        }

        private readonly Dictionary<string, RectState> _rects = new Dictionary<string, RectState>();
        private string _journalPath;
        private string _digestStatePath;
        private int _secondsToHeartbeat;
        private int _heartbeatFailures;
        private DateTime _lastDigestDateUtc = DateTime.MinValue;
        private int _touchesTodayTotal;

        // ------------------------------------------------------------ lifecycle
        protected override void OnStart()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                   "ZoneConsole", "journal");
            Directory.CreateDirectory(dir);
            // Per-chart file: two instances on the same symbol (different timeframes)
            // must never share a file (exclusive append handles would drop lines).
            var baseName = SymbolName + "-" + Chart.TimeFrame + "-shadow";
            _journalPath = Path.Combine(dir, baseName + ".jsonl");
            _digestStatePath = Path.Combine(dir, baseName + ".digestdate");
            _lastDigestDateUtc = ReadDigestDate();

            Chart.ObjectsAdded += OnObjectsAdded;
            Chart.ObjectsRemoved += OnObjectsRemoved;
            Chart.ObjectsUpdated += OnObjectsUpdated;

            foreach (var rect in Chart.Objects.OfType<ChartRectangle>())
                TrackRect(rect, "zone_seen");

            _secondsToHeartbeat = HeartbeatSeconds;
            Timer.Start(TimeSpan.FromSeconds(1));

            Journal("logger_start", null, ("watching", _rects.Count.ToString(CultureInfo.InvariantCulture)));
            Telegram(string.Format(CultureInfo.InvariantCulture,
                "🟢 ShadowLogger started on {0} {1} — watching {2} rectangle(s)",
                SymbolName, Chart.TimeFrame, _rects.Count), silent: false);
        }

        protected override void OnStop()
        {
            Journal("logger_stop", null);
            // Deliberately no Telegram here: OnStop also fires on normal platform
            // shutdown; silence-detection is the heartbeat's job, not a farewell message.
        }

        // ------------------------------------------------------------ chart events
        private void OnObjectsAdded(ChartObjectsAddedEventArgs e)
        {
            foreach (var rect in e.ChartObjects.OfType<ChartRectangle>())
                TrackRect(rect, "zone_added");
        }

        private void OnObjectsRemoved(ChartObjectsRemovedEventArgs e)
        {
            foreach (var rect in e.ChartObjects.OfType<ChartRectangle>())
            {
                RectState st;
                if (_rects.TryGetValue(rect.Name, out st))
                {
                    if (st.Inside) // never leave a dangling touch_start in the journal
                        Journal("touch_end", rect, ("reason", "zone_removed"),
                            ("range_min", F(st.TouchMin)), ("range_max", F(st.TouchMax)));
                    _rects.Remove(rect.Name);
                    Journal("zone_removed", rect);
                }
            }
        }

        private void OnObjectsUpdated(ChartObjectsUpdatedEventArgs e)
        {
            foreach (var rect in e.ChartObjects.OfType<ChartRectangle>())
            {
                RectState st;
                if (!_rects.TryGetValue(rect.Name, out st)) { TrackRect(rect, "zone_added"); continue; }

                double top = Math.Max(rect.Y1, rect.Y2), bottom = Math.Min(rect.Y1, rect.Y2);
                var newComment = rect.Comment ?? "";
                bool geomChanged = top != st.Top || bottom != st.Bottom;
                bool commentChanged = newComment != st.Comment;
                if (!geomChanged && !commentChanged) continue; // drag-noise: no journal spam

                st.Top = top; st.Bottom = bottom;
                if (commentChanged)
                {
                    st.Comment = newComment;
                    Journal("zone_configured", rect, ("parse", DescribeComment(newComment)));
                }
                else
                    Journal("zone_modified", rect);
            }
        }

        private void TrackRect(ChartRectangle rect, string evt)
        {
            if (_rects.ContainsKey(rect.Name)) return;
            var st = new RectState
            {
                Comment = rect.Comment ?? "",
                Top = Math.Max(rect.Y1, rect.Y2),
                Bottom = Math.Min(rect.Y1, rect.Y2)
            };
            _rects[rect.Name] = st;
            Journal(evt, rect, ("parse", DescribeComment(st.Comment)));
        }

        // ------------------------------------------------------------ ticks: touch detection
        protected override void OnTick()
        {
            foreach (var rect in Chart.Objects.OfType<ChartRectangle>())
            {
                RectState st;
                if (!_rects.TryGetValue(rect.Name, out st)) continue;

                double top = Math.Max(rect.Y1, rect.Y2);
                double bottom = Math.Min(rect.Y1, rect.Y2);
                double height = top - bottom;
                if (height <= 0)
                {
                    if (st.Inside)
                    {
                        st.Inside = false;
                        Journal("touch_end", rect, ("reason", "zone_collapsed"));
                    }
                    continue;
                }

                double mid = (Symbol.Bid + Symbol.Ask) / 2.0;
                double buffer = height * TouchHysteresis;
                bool enter = Symbol.Bid <= top && Symbol.Ask >= bottom;
                // Exit uses the SAME band plus the buffer, so exit ⇒ enter is false
                // (audit fix: mid-based exit oscillated when spread > 2×buffer).
                bool exit = Symbol.Bid > top + buffer || Symbol.Ask < bottom - buffer;

                if (!st.Inside && enter)
                {
                    st.Inside = true;
                    st.TouchStartUtc = Server.Time;
                    st.TouchMin = st.TouchMax = mid;
                    st.TouchesToday++;
                    _touchesTodayTotal++;
                    Journal("touch_start", rect,
                        ("bid", F(Symbol.Bid)), ("ask", F(Symbol.Ask)),
                        ("spread_pips", F((Symbol.Ask - Symbol.Bid) / Symbol.PipSize)));
                }
                else if (st.Inside)
                {
                    st.TouchMin = Math.Min(st.TouchMin, mid);
                    st.TouchMax = Math.Max(st.TouchMax, mid);
                    if (exit)
                    {
                        st.Inside = false;
                        Journal("touch_end", rect,
                            ("duration_s", F((Server.Time - st.TouchStartUtc).TotalSeconds)),
                            ("range_min", F(st.TouchMin)), ("range_max", F(st.TouchMax)));
                    }
                }
            }
        }

        // ------------------------------------------------------------ timer: heartbeat + digest
        protected override void OnTimer()
        {
            _secondsToHeartbeat--;
            if (_secondsToHeartbeat <= 0)
            {
                _secondsToHeartbeat = HeartbeatSeconds;
                Heartbeat();
            }

            // >= so a restart that misses the exact hour still sends once, later that day;
            // the persisted date prevents double-sends across restarts (audit fix).
            var nowUtc = Server.Time;
            if (nowUtc.Hour >= DigestHourUtc && _lastDigestDateUtc.Date != nowUtc.Date)
            {
                _lastDigestDateUtc = nowUtc;
                WriteDigestDate(nowUtc);
                SendDigest();
            }
        }

        private void Heartbeat()
        {
            if (string.IsNullOrWhiteSpace(HeartbeatUrl)) return;
            // Fire-and-forget: never block the tick thread (audit fix). Result is
            // marshalled back to the main thread before touching state.
            HttpShared.GetAsync(HeartbeatUrl).ContinueWith(t =>
            {
                bool ok = t.Status == TaskStatus.RanToCompletion && t.Result.IsSuccessStatusCode;
                if (t.Status == TaskStatus.RanToCompletion) t.Result.Dispose();
                BeginInvokeOnMainThread(() =>
                {
                    if (ok) { _heartbeatFailures = 0; return; }
                    _heartbeatFailures++;
                    if (_heartbeatFailures == 5)
                        Journal("heartbeat_failing", null, ("failures", "5"));
                });
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        private void SendDigest()
        {
            // prune states whose rectangles no longer exist (renames leave orphans)
            var live = new HashSet<string>(Chart.Objects.OfType<ChartRectangle>().Select(o => o.Name));
            foreach (var dead in _rects.Keys.Where(k => !live.Contains(k)).ToList())
                _rects.Remove(dead);

            int configured = _rects.Values.Count(r => !string.IsNullOrWhiteSpace(r.Comment));
            var expiring = new List<string>();
            foreach (var rect in Chart.Objects.OfType<ChartRectangle>())
            {
                var exp = TryGetExpiry(rect.Comment);
                if (exp.HasValue && (exp.Value - Server.Time).TotalDays <= 7)
                    expiring.Add(rect.Name + " (" + exp.Value.ToString("dd MMM", CultureInfo.InvariantCulture) + ")");
            }
            var sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.InvariantCulture,
                "📒 {0} {1} digest — zones watched: {2} ({3} configured) · touches today: {4}",
                SymbolName, Chart.TimeFrame, _rects.Count, configured, _touchesTodayTotal);
            if (expiring.Count > 0)
                sb.Append("\n⏳ expiring ≤7d: ").Append(string.Join(", ", expiring));
            Telegram(sb.ToString(), silent: true);
            Journal("daily_digest", null, ("touches", _touchesTodayTotal.ToString(CultureInfo.InvariantCulture)));
            _touchesTodayTotal = 0;
            foreach (var st in _rects.Values) st.TouchesToday = 0;
        }

        // ------------------------------------------------------------ helpers
        // Minimal, deliberately duplicated view of the v1 grammar (ZoneExec owns the
        // full parser). Here we only classify + surface expiry for the digest.
        private static string DescribeComment(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment)) return "draft(empty)";
            var t = comment.Trim().ToUpperInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            bool cls = t.Length > 0 && (t[0] == "RED" || t[0] == "YEL" || t[0] == "YELLOW");
            bool dir = t.Length > 1 && (t[1] == "BUY" || t[1] == "SELL");
            return cls && dir ? "configured(" + t[0] + " " + t[1] + ")" : "unparsed";
        }

        private static DateTime? TryGetExpiry(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment)) return null;
            foreach (var tok in comment.ToUpperInvariant().Split(' '))
                if (tok.StartsWith("EXP:", StringComparison.Ordinal))
                {
                    DateTime d;
                    if (DateTime.TryParseExact(tok.Substring(4), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out d))
                        return d.Date.AddDays(1).AddSeconds(-1); // end of that UTC day
                }
            return null;
        }

        private DateTime ReadDigestDate()
        {
            try
            {
                if (File.Exists(_digestStatePath))
                {
                    DateTime d;
                    if (DateTime.TryParseExact(File.ReadAllText(_digestStatePath).Trim(), "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out d))
                        return d;
                }
            }
            catch (Exception ex) { Print("digest state read failed: {0}", ex.Message); }
            return DateTime.MinValue;
        }

        private void WriteDigestDate(DateTime d)
        {
            try { File.WriteAllText(_digestStatePath, d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)); }
            catch (Exception ex) { Print("digest state write failed: {0}", ex.Message); }
        }

        private static string F(double v) => v.ToString("0.#####", CultureInfo.InvariantCulture);

        private static string J(string s)
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

        private void Journal(string evt, ChartRectangle rect, params (string k, string v)[] extra)
        {
            try
            {
                var sb = new StringBuilder(256);
                sb.Append("{\"ts\":\"").Append(Server.Time.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture))
                  .Append("\",\"event\":\"").Append(J(evt))
                  .Append("\",\"symbol\":\"").Append(J(SymbolName))
                  .Append("\",\"tf\":\"").Append(J(Chart.TimeFrame.ToString())).Append('"');
                if (rect != null)
                {
                    sb.Append(",\"zone\":\"").Append(J(rect.Name))
                      .Append("\",\"top\":").Append(F(Math.Max(rect.Y1, rect.Y2)))
                      .Append(",\"bottom\":").Append(F(Math.Min(rect.Y1, rect.Y2)))
                      .Append(",\"comment\":\"").Append(J(rect.Comment ?? "")).Append('"');
                }
                foreach (var (k, v) in extra)
                    sb.Append(",\"").Append(J(k)).Append("\":\"").Append(J(v)).Append('"');
                sb.Append('}');
                File.AppendAllText(_journalPath, sb.ToString() + Environment.NewLine);
            }
            catch (Exception ex) { Print("journal write failed: {0}", ex.Message); }
        }

        private void Telegram(string text, bool silent)
        {
            if (string.IsNullOrWhiteSpace(TelegramToken) || string.IsNullOrWhiteSpace(TelegramChatId)) return;
            try
            {
                var url = "https://api.telegram.org/bot" + TelegramToken + "/sendMessage";
                var payload = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("chat_id", TelegramChatId),
                    new KeyValuePair<string, string>("text", text),
                    new KeyValuePair<string, string>("disable_notification", silent ? "true" : "false")
                };
                // Fire-and-forget: never block the tick thread (audit fix).
                HttpShared.PostAsync(url, new FormUrlEncodedContent(payload)).ContinueWith(t =>
                {
                    if (t.Status == TaskStatus.RanToCompletion) { t.Result.Dispose(); return; }
                    BeginInvokeOnMainThread(() => Print("telegram send failed"));
                }, TaskContinuationOptions.ExecuteSynchronously);
            }
            catch (Exception ex) { Print("telegram send failed: {0}", ex.Message); }
        }
    }
}
