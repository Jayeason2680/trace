// ============================================================================
// ZoneValidator — Zone Console core, Session 1
// ATR-based sanity checks a zone must pass before it may arm. Pure C#.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ZoneConsole.Core
{
    public sealed class ZoneGeometry
    {
        public string Symbol;
        public double Top;
        public double Bottom;
        public Direction Direction;
        public double Height => Top - Bottom;
        public double Mid => (Top + Bottom) / 2.0;
    }

    public sealed class ValidationResult
    {
        public List<string> Errors = new List<string>();
        public List<string> Warnings = new List<string>();
        public bool Ok => Errors.Count == 0;
    }

    public static class ZoneValidator
    {
        public const double MinHeightAtr = 0.15;
        public const double MaxHeightAtr = 3.0;
        public const double MaxDistanceAtr = 15.0;
        public const int MaxExpiryDays = 84;

        /// <param name="dailyAtr">ATR(14) of the DAILY timeframe, in price units.</param>
        /// <param name="currentPrice">mid price now.</param>
        /// <param name="existing">already-armed zones on the same symbol (for overlap check).</param>
        public static ValidationResult Validate(
            ZoneRecord rec, ZoneGeometry geo, double dailyAtr, double currentPrice,
            DateTime nowUtc, IEnumerable<ZoneGeometry> existing)
        {
            var r = new ValidationResult();

            if (geo.Top <= geo.Bottom)
                r.Errors.Add("rectangle has no height (top <= bottom)");
            if (dailyAtr <= 0)
            {
                r.Errors.Add("daily ATR unavailable — cannot sanity-check; refuse to arm");
                return r;
            }
            if (!r.Ok) return r;

            double hAtr = geo.Height / dailyAtr;
            if (hAtr < MinHeightAtr)
                r.Errors.Add(Fmt("zone too thin: {0:0.00}× daily ATR (min {1})", hAtr, MinHeightAtr));
            if (hAtr > MaxHeightAtr)
                r.Errors.Add(Fmt("zone too tall: {0:0.00}× daily ATR (max {1})", hAtr, MaxHeightAtr));

            double distance = currentPrice > geo.Top ? currentPrice - geo.Top
                            : currentPrice < geo.Bottom ? geo.Bottom - currentPrice
                            : 0.0;
            double dAtr = distance / dailyAtr;
            if (dAtr > MaxDistanceAtr)
                r.Errors.Add(Fmt("zone too far from price: {0:0.0}× daily ATR (max {1})", dAtr, MaxDistanceAtr));
            if (distance == 0.0)
                r.Warnings.Add("price is already inside the zone — it would arm hot (first touch immediate)");

            // direction sanity: a SELL zone should sit above price, a BUY zone below
            if (rec.Direction == Direction.Sell && currentPrice > geo.Top)
                r.Warnings.Add("SELL zone is below current price — check the direction");
            if (rec.Direction == Direction.Buy && currentPrice < geo.Bottom)
                r.Warnings.Add("BUY zone is above current price — check the direction");

            if (rec.ExpiryUtc <= nowUtc)
                r.Errors.Add("expiry is in the past");
            else if ((rec.ExpiryUtc - nowUtc).TotalDays > MaxExpiryDays)
                r.Errors.Add(Fmt("expiry more than {0} days out", MaxExpiryDays));

            double cap = ZoneRecord.ClassRiskCap(rec.Class);
            if (rec.RiskPercent > cap)
                r.Errors.Add(Fmt("risk {0:0.0}% above class cap {1:0.0}%", rec.RiskPercent, cap));

            foreach (var z in existing ?? Array.Empty<ZoneGeometry>())
            {
                // audit fixes: filter by symbol here (callers may pass the whole book),
                // compare against the authoritative record direction, and use strict
                // inequalities so stacked zones sharing an edge are allowed.
                if (!string.Equals(z.Symbol, geo.Symbol, StringComparison.OrdinalIgnoreCase)) continue;
                if (z.Direction != rec.Direction) continue;
                bool overlap = geo.Bottom < z.Top && geo.Top > z.Bottom;
                if (overlap)
                    r.Errors.Add("overlaps existing same-direction zone [" +
                                 Fmt("{0:0.#####}–{1:0.#####}", z.Bottom, z.Top) + "]");
            }

            return r;
        }

        private static string Fmt(string f, params object[] a) =>
            string.Format(CultureInfo.InvariantCulture, f, a);
    }
}
