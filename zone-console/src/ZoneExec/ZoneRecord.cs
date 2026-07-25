// ============================================================================
// ZoneRecord — Zone Console core, Session 1
// Parses the v1 comment grammar into a structured record. Pure C#: no cAlgo
// dependency, unit-testable. Unknown tokens are warnings, never errors
// (forward compatibility, audit fix #11).
// ============================================================================
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ZoneConsole.Core
{
    public enum ZoneClass { Red, Yellow }
    public enum Direction { Buy, Sell }
    public enum EntryStyle { T1RestingLimit = 1, T2TouchConfirm = 2 }
    public enum EntryPrice { Mid, Edge }
    public enum WeekendPolicy { Flat, Hold }

    public sealed class ZoneRecord
    {
        public ZoneClass Class;
        public Direction Direction;
        public double RiskPercent;          // of equity, e.g. 0.8
        public DateTime ExpiryUtc;          // end of the stated UTC day
        public EntryStyle Entry;
        public int TouchBudget;
        public EntryPrice EntryAt;
        public WeekendPolicy Weekend;
        public string Id = "";
        public double RewardRiskTarget;     // class default: RED 2.5, YEL 2.0

        public List<string> Warnings = new List<string>();

        public static double ClassRiskCap(ZoneClass c) => c == ZoneClass.Red ? 1.0 : 0.5;
        public static double ClassRiskDefault(ZoneClass c) => c == ZoneClass.Red ? 0.8 : 0.4;
        public static int ClassDefaultExpiryDays(ZoneClass c) => c == ZoneClass.Red ? 42 : 14;

        /// <summary>
        /// Parse a rectangle comment. Returns null with errors filled when the text is
        /// not a zone instruction. Empty/whitespace comment = draft (null, no errors).
        /// </summary>
        public static ZoneRecord TryParse(string comment, DateTime nowUtc, out List<string> errors)
        {
            errors = new List<string>();
            if (string.IsNullOrWhiteSpace(comment)) return null; // silent draft

            var tokens = comment.Trim().ToUpperInvariant()
                                .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2)
            {
                errors.Add("need at least CLASS and DIRECTION, e.g. \"RED SELL\"");
                return null;
            }

            var rec = new ZoneRecord();

            switch (tokens[0])
            {
                case "RED": rec.Class = ZoneClass.Red; break;
                case "YEL":
                case "YELLOW": rec.Class = ZoneClass.Yellow; break;
                default: errors.Add("first token must be RED or YEL, got '" + tokens[0] + "'"); return null;
            }
            switch (tokens[1])
            {
                case "BUY": rec.Direction = Direction.Buy; break;
                case "SELL": rec.Direction = Direction.Sell; break;
                default: errors.Add("second token must be BUY or SELL, got '" + tokens[1] + "'"); return null;
            }

            // class defaults (Appendix A)
            rec.RiskPercent = ClassRiskDefault(rec.Class);
            rec.ExpiryUtc = EndOfUtcDay(nowUtc.Date.AddDays(ClassDefaultExpiryDays(rec.Class)));
            rec.Entry = rec.Class == ZoneClass.Red ? EntryStyle.T1RestingLimit : EntryStyle.T2TouchConfirm;
            rec.TouchBudget = rec.Class == ZoneClass.Red ? 2 : 1;
            rec.EntryAt = rec.Class == ZoneClass.Red ? EntryPrice.Mid : EntryPrice.Edge;
            rec.Weekend = WeekendPolicy.Flat;
            rec.RewardRiskTarget = rec.Class == ZoneClass.Red ? 2.5 : 2.0;

            for (int i = 2; i < tokens.Length; i++)
            {
                var t = tokens[i];
                double d; int n; DateTime dt;

                // Risk token: anything that LOOKS like a risk token must parse strictly,
                // or the zone rejects — never defaults, never clamps (audit fixes #2/#3).
                if (t[0] == 'R' && LooksNumeric(t, 1))
                {
                    if (t.Length > 1 && double.TryParse(t.Substring(1),
                        NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d))
                    {
                        var cap = ClassRiskCap(rec.Class);
                        if (d < 0.1)
                            errors.Add("risk below the 0.1% floor: " + t);
                        else if (d > cap)
                            errors.Add(string.Format(CultureInfo.InvariantCulture,
                                "risk {0}% above the {1} cap {2}% — rejected, not clamped", d, rec.Class, cap));
                        else
                            rec.RiskPercent = d;
                    }
                    else
                        errors.Add("bad risk token (write e.g. R0.8, decimal point, no % sign): " + t);
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
                    if (int.TryParse(t.Substring(4), NumberStyles.Integer, CultureInfo.InvariantCulture, out n)
                        && n >= 1 && n <= 3)
                        rec.TouchBudget = n;
                    else errors.Add("touch budget must be 1..3: " + t);
                }
                else if (t == "E:MID") rec.EntryAt = EntryPrice.Mid;
                else if (t == "E:EDGE") rec.EntryAt = EntryPrice.Edge;
                else if (t == "WKD:FLAT") rec.Weekend = WeekendPolicy.Flat;
                else if (t == "WKD:HOLD") rec.Weekend = WeekendPolicy.Hold;
                else if (t.StartsWith("ID:", StringComparison.Ordinal) && t.Length > 3)
                    rec.Id = t.Substring(3);
                else
                    rec.Warnings.Add("ignored unknown token: " + t); // audit fix: never reject
            }

            return errors.Count > 0 ? null : rec;
        }

        /// <summary>
        /// Zone identity (audit fix #1): NEVER derived from geometry — a resize must not
        /// change the id, and opposite-direction zones must not share one. The executing
        /// cBot calls NewId() once at first arm and writes it back into the comment as
        /// "ID:xxxx", so the rectangle itself carries its identity from then on.
        /// </summary>
        public static string NewId()
        {
            var g = Guid.NewGuid().ToByteArray();
            return BitConverter.ToString(g, 0, 4).Replace("-", ""); // 8 hex chars
        }

        /// <summary>True when the rest of the token is empty or made only of digits/./,
        /// — i.e. the user clearly meant a risk number.</summary>
        private static bool LooksNumeric(string t, int from)
        {
            if (t.Length <= from) return true; // bare "R"
            for (int i = from; i < t.Length; i++)
            {
                var c = t[i];
                if ((c < '0' || c > '9') && c != '.' && c != ',') return false;
            }
            return true;
        }

        private static DateTime EndOfUtcDay(DateTime utcDate) =>
            DateTime.SpecifyKind(utcDate.Date, DateTimeKind.Utc).AddDays(1).AddSeconds(-1);
    }
}
