// ============================================================================
// ShadowLogger v1 — Zone Console, Session 1
// Watch-only cBot: logs rectangles ("zones") and price touches to a JSONL
// journal, sends a nightly Telegram digest, pings a heartbeat URL.
// CONTAINS NO ORDER CODE BY DESIGN — it cannot trade.
// Attach one instance to every chart you draw zones on.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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
            public bool Inside;
            public DateTime TouchStartUtc;
            public double TouchMin, TouchMax;
            public int TouchesToday;
        }

        private readonly Dictionary<string, RectState> _rects = new Dictionary<string, RectState>();
        private string _journalPath;
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
            _journalPath = Path.Combine(dir, SymbolName + "-shadow.jsonl");

            Chart.ObjectsAdded += OnObjectsAdded;
            Chart.ObjectsRemoved += OnObjectsRemoved;
            Chart.ObjectsUpdated += OnObjectsUpdated;

            foreach (var rect in Chart.Objects.OfType<ChartRectangle>())
                TrackRect(rect, "zone_seen");

            _secondsToHeartbeat = HeartbeatSeconds;
            Timer.Start(TimeSpan.FromSeconds(1));

            Journal("logger_start", null, ("watching", _rects.Count.ToString(CultureInfo.InvariantCulture)));
            Telegram(string.Format(CultureInfo.InvariantCulture,
                "🟢 ShadowLogger started on {0} — watching {1} rectangle(s)",
                SymbolName, _rects.Count), silent: false);
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
                if (_rects.Remove(rect.Name))
                    Journal("zone_removed", rect);
            }
        }

        private void OnObjectsUpdated(ChartObjectsUpdatedEventArgs e)
        {
            foreach (var rect in e.ChartObjects.OfType<ChartRectangle>())
            {
                RectState st;
                if (!_rects.TryGetValue(rect.Name, out st)) { TrackRect(rect, "zone_added"); continue; }
                var newComment = rect.Comment ?? "";
                if (newComment != st.Comment)
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
            var st = new RectState { Comment = rect.Comment ?? "" };
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
                if (height <= 0) continue;

                double mid = (Symbol.Bid + Symbol.Ask) / 2.0;
                bool overlap = Symbol.Bid <= top && Symbol.Ask >= bottom;

                if (!st.Inside && overlap)
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
                    double buffer = height * TouchHysteresis;
                    if (mid > top + buffer || mid < bottom - buffer)
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

            var nowUtc = Server.Time;
            if (nowUtc.Hour == DigestHourUtc && _lastDigestDateUtc.Date != nowUtc.Date)
            {
                _lastDigestDateUtc = nowUtc;
                SendDigest();
            }
        }

        private void Heartbeat()
        {
            if (string.IsNullOrWhiteSpace(HeartbeatUrl)) return;
            try
            {
                var resp = HttpShared.GetAsync(HeartbeatUrl).GetAwaiter().GetResult();
                if (!resp.IsSuccessStatusCode) throw new Exception("HTTP " + (int)resp.StatusCode);
                _heartbeatFailures = 0;
            }
            catch (Exception ex)
            {
                _heartbeatFailures++;
                if (_heartbeatFailures == 5)
                    Journal("heartbeat_failing", null, ("error", ex.Message));
            }
        }

        private void SendDigest()
        {
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
                "📒 {0} digest — zones watched: {1} ({2} configured) · touches today: {3}",
                SymbolName, _rects.Count, configured, _touchesTodayTotal);
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
                  .Append("\",\"symbol\":\"").Append(J(SymbolName)).Append('"');
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
                HttpShared.PostAsync(url, new FormUrlEncodedContent(payload)).GetAwaiter().GetResult();
            }
            catch (Exception ex) { Print("telegram send failed: {0}", ex.Message); }
        }
    }
}
