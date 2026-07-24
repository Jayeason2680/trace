// ============================================================================
// RiskEngine — Zone Console core, Session 1
// Position sizing + the caps that are allowed to say no. Pure C#.
// Audit fixes baked in: both currency legs counted (#7), factor groups for
// metals/indices (#7), round-DOWN sizing with ±10% tolerance alarm (#T6),
// gap-exposure sizing for WKD:HOLD (#1), explicit cut-out semantics (#4/#10).
// ============================================================================
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ZoneConsole.Core
{
    /// <summary>What the engine needs to know about each already-open trade.</summary>
    public sealed class OpenExposure
    {
        public string Symbol;
        public Direction Direction;
        public double RiskPercentAtEntry;   // as configured when the trade was armed
    }

    public sealed class SizingResult
    {
        public double VolumeUnits;          // broker units, already stepped DOWN
        public double AchievedRiskPercent;  // risk actually carried after rounding
        public List<string> Errors = new List<string>();
        public List<string> Warnings = new List<string>();
        public bool Ok => Errors.Count == 0 && VolumeUnits > 0;
    }

    public static class RiskEngine
    {
        public const int MaxOpenPositions = 3;
        public const double MaxTotalOpenRiskPercent = 3.0;   // honest ceiling (3 × 1.0), audit fix #S5
        public const double MaxFactorRiskPercent = 2.0;      // per currency / factor group
        public const double DailyCutoutPercent = 3.0;        // freezes NEW risk; does not flatten
        public const double AccountBreakerPercent = 10.0;    // audit fix: slow-bleed backstop
        public const double GapStopMultiplier = 2.0;         // WKD:HOLD sized as if stop were 2× away
        public const double RiskRoundingTolerance = 0.10;    // ±10% achieved-vs-configured

        // ------------------------------------------------------------ sizing
        /// <param name="equity">account equity in account currency.</param>
        /// <param name="stopDistance">entry→stop distance in PRICE units (always > 0).</param>
        /// <param name="tickSize">symbol tick size (price units).</param>
        /// <param name="tickValue">account-currency value of one tick for 1 unit of volume.</param>
        /// <param name="volumeStep">broker volume step (units).</param>
        /// <param name="volumeMin">broker minimum volume (units).</param>
        public static SizingResult ComputeVolume(
            ZoneRecord rec, double equity, double stopDistance,
            double tickSize, double tickValue, double volumeStep, double volumeMin)
        {
            var res = new SizingResult();
            if (equity <= 0) { res.Errors.Add("equity must be positive"); return res; }
            if (stopDistance <= 0) { res.Errors.Add("stop distance must be positive"); return res; }
            if (tickSize <= 0 || tickValue <= 0 || volumeStep <= 0)
            { res.Errors.Add("bad symbol economics (tickSize/tickValue/volumeStep)"); return res; }

            double effectiveStop = rec.Weekend == WeekendPolicy.Hold
                ? stopDistance * GapStopMultiplier      // audit fix #1: gap-exposed holds sized smaller
                : stopDistance;

            double riskMoney = equity * rec.RiskPercent / 100.0;
            double lossPerUnit = effectiveStop / tickSize * tickValue; // loss for 1 unit over the stop
            if (lossPerUnit <= 0) { res.Errors.Add("loss-per-unit computed as zero"); return res; }

            double rawUnits = riskMoney / lossPerUnit;
            double stepped = Math.Floor(rawUnits / volumeStep) * volumeStep; // audit fix: round DOWN
            if (stepped < volumeMin)
            {
                res.Errors.Add(Fmt(
                    "size below broker minimum ({0} < {1}) — risk {2:0.00}% too small for this stop; skip the trade rather than oversize",
                    stepped, volumeMin, rec.RiskPercent));
                return res;
            }

            res.VolumeUnits = stepped;
            res.AchievedRiskPercent = stepped * lossPerUnit / equity * 100.0
                                      * (rec.Weekend == WeekendPolicy.Hold ? 1.0 / GapStopMultiplier : 1.0);
            // achieved% reported against the REAL stop; the gap multiplier is a sizing
            // haircut, not a change to the actual stop distance.

            double configured = rec.RiskPercent / (rec.Weekend == WeekendPolicy.Hold ? GapStopMultiplier : 1.0);
            if (configured > 0)
            {
                double dev = Math.Abs(res.AchievedRiskPercent - configured) / configured;
                if (dev > RiskRoundingTolerance)
                    res.Warnings.Add(Fmt("achieved risk {0:0.00}% deviates {1:0}% from configured after rounding",
                                          res.AchievedRiskPercent, dev * 100));
            }
            return res;
        }

        // ------------------------------------------------------------ caps
        /// <summary>May a NEW trade with this record/symbol open now? (audit fixes #7, #S5)</summary>
        public static List<string> CheckCaps(
            ZoneRecord rec, string symbol, IReadOnlyList<OpenExposure> open)
        {
            var errors = new List<string>();
            open = open ?? Array.Empty<OpenExposure>();

            if (open.Count >= MaxOpenPositions)
                errors.Add(Fmt("max positions reached ({0})", MaxOpenPositions));

            double totalAfter = open.Sum(o => o.RiskPercentAtEntry) + rec.RiskPercent;
            if (totalAfter > MaxTotalOpenRiskPercent + 1e-9)
                errors.Add(Fmt("total open risk would be {0:0.0}% (max {1:0.0}%)",
                               totalAfter, MaxTotalOpenRiskPercent));

            // factor exposure: BOTH legs of every pair, signed by direction
            var after = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var o in open) AddFactors(after, o.Symbol, o.Direction, o.RiskPercentAtEntry);
            AddFactors(after, symbol, rec.Direction, rec.RiskPercent);
            foreach (var kv in after)
                if (Math.Abs(kv.Value) > MaxFactorRiskPercent + 1e-9)
                    errors.Add(Fmt("factor '{0}' exposure would be {1:+0.0;-0.0}% (cap ±{2:0.0}%)",
                                   kv.Key, kv.Value, MaxFactorRiskPercent));
            return errors;
        }

        /// <summary>Signed factor legs for a symbol. BUY EURUSD = +EUR −USD.</summary>
        public static void AddFactors(IDictionary<string, double> acc, string symbol,
                                      Direction dir, double riskPercent)
        {
            double sign = dir == Direction.Buy ? 1.0 : -1.0;
            foreach (var (factor, weight) in FactorLegs(symbol))
            {
                double v;
                acc.TryGetValue(factor, out v);
                acc[factor] = v + sign * weight * riskPercent;
            }
        }

        /// <summary>
        /// Map a symbol to (factor, weight) legs. FX pairs → base +1, quote −1.
        /// Metals/indices/energy → asset factor + quote currency + a shared RISK_ON group.
        /// Unknown symbols map to their own name (still capped individually).
        /// </summary>
        public static IEnumerable<(string factor, double weight)> FactorLegs(string symbol)
        {
            var s = (symbol ?? "").ToUpperInvariant().Replace("/", "").Replace("_", "");
            string[] fx = { "USD", "EUR", "GBP", "JPY", "AUD", "NZD", "CAD", "CHF", "SGD", "MYR", "CNH" };

            if (s.StartsWith("XAU")) return Legs(("GOLD", 1), (Quote(s, "XAU"), -1));
            if (s.StartsWith("XAG")) return Legs(("SILVER", 1), (Quote(s, "XAG"), -1));
            if (s.Contains("US500") || s.Contains("SPX")) return Legs(("US_EQ", 1), ("RISK_ON", 1));
            if (s.Contains("US30") || s.Contains("DJ")) return Legs(("US_EQ", 1), ("RISK_ON", 1));
            if (s.Contains("NAS") || s.Contains("US100") || s.Contains("USTEC")) return Legs(("US_EQ", 1), ("RISK_ON", 1));
            if (s.Contains("DE40") || s.Contains("GER") || s.Contains("DAX")) return Legs(("EU_EQ", 1), ("RISK_ON", 1));
            if (s.Contains("UK100") || s.Contains("FTSE")) return Legs(("EU_EQ", 1), ("RISK_ON", 1));
            if (s.Contains("XTI") || s.Contains("WTI") || s.Contains("OIL") || s.Contains("BRENT"))
                return Legs(("OIL", 1), ("RISK_ON", 1));

            if (s.Length >= 6)
            {
                var b = s.Substring(0, 3); var q = s.Substring(3, 3);
                if (Array.IndexOf(fx, b) >= 0 && Array.IndexOf(fx, q) >= 0)
                    return Legs((b, 1), (q, -1));
            }
            return Legs((s.Length == 0 ? "UNKNOWN" : s, 1));
        }

        private static string Quote(string s, string prefix)
        {
            var rest = s.Substring(prefix.Length);
            return rest.Length >= 3 ? rest.Substring(0, 3) : "USD";
        }
        private static IEnumerable<(string, double)> Legs(params (string, double)[] legs) => legs;

        // ------------------------------------------------------------ breakers
        /// <summary>
        /// Daily cut-out: equity vs the day-start snapshot (broker day, UTC).
        /// Trips at −3%: FREEZES new arms/orders; open positions keep their broker-side
        /// stops — flattening is a human decision announced by a PAGE (audit: explicit,
        /// signed semantics). Account breaker at −10% from high-water mark: everything
        /// disarms and stays disarmed until a written review.
        /// </summary>
        public static (bool dailyTripped, bool accountTripped, string message) CheckBreakers(
            double equityNow, double dayStartEquity, double highWaterEquity)
        {
            bool daily = dayStartEquity > 0 &&
                         (dayStartEquity - equityNow) / dayStartEquity * 100.0 >= DailyCutoutPercent;
            bool acct = highWaterEquity > 0 &&
                        (highWaterEquity - equityNow) / highWaterEquity * 100.0 >= AccountBreakerPercent;
            string msg = acct ? Fmt("ACCOUNT BREAKER: −{0:0.0}% from high-water — all zones disarm; written review required", AccountBreakerPercent)
                      : daily ? Fmt("DAILY CUT-OUT: −{0:0.0}% today — no new risk until tomorrow; positions keep broker-side stops", DailyCutoutPercent)
                      : "";
            return (daily, acct, msg);
        }

        private static string Fmt(string f, params object[] a) =>
            string.Format(CultureInfo.InvariantCulture, f, a);
    }
}
