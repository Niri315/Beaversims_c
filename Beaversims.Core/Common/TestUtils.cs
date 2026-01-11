using Beaversims.Core.Sim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Common
{
    internal class TestUtils
    {
        public static void PrintStatWeights(GainMatrix swGains)
        {
            var allGainTypes = swGains
                .SelectMany(s => s.Value.Keys)
                .Distinct()
                .OrderBy(gt => gt)
                .ToList();

            // Print header
            Console.Write("".PadRight(15)); // space for stat name column
            foreach (var gainType in allGainTypes)
            {
                Console.Write(gainType.ToString().PadRight(10));
            }
            Console.WriteLine();

            // Print each stat row
            foreach (var statEntry in swGains)
            {
                Console.Write(statEntry.Key.ToString().PadRight(15));
                foreach (var gainType in allGainTypes)
                {
                    statEntry.Value.TryGetValue(gainType, out var value);
                    Console.Write(value.ToString("F2").PadRight(10));
                }
                Console.WriteLine();
            }
        }
        public static void PrintAltGearResults(Results results)
        {
            var gearSets = results.altGearSets;
            var originals = results.OriginalTotals;
            if (gearSets == null || gearSets.Count == 0)
            {
                Console.WriteLine("No gear sets to display.");
                return;
            }

            // Define the exact GainType order you want
            var orderedGainTypes = new[]
            {
                GainType.Eff,
                GainType.Dmg,
                GainType.Def,
                GainType.SupEff,
                GainType.SupDmg,
                GainType.MsEff,
                GainType.MsDmg,
                GainType.BalEff,
                GainType.BalDmg
            };

            // Determine padding based on the longest gear set name
            int longestNameLength = gearSets
                .Select(gs => (gs.Name ?? $"Set {gs.Id}").Length)
                .DefaultIfEmpty(0)
                .Max();

            int nameColumnWidth = Math.Max(longestNameLength + 2, 20); // Add a small buffer for readability

            // Header
            Console.Write("".PadRight(nameColumnWidth));
            Console.Write($"{"Total",10}");
            foreach (var gt in orderedGainTypes)
                Console.Write($"{gt,10}");
            Console.WriteLine();

            Console.Write($"{"Originals".PadRight(nameColumnWidth)}");
            double val_o = originals[GainType.Eff] + originals[GainType.Dmg] + originals[GainType.Def];
            Console.Write($"{val_o,11:0.00}");

            foreach (var gt in orderedGainTypes)
            {
                double val = 0;
                if (originals != null && originals.TryGetValue(gt, out double gain))
                    val = gain;

                Console.Write($"{val,11:0.00}");
            }

            Console.WriteLine();
            // Rows
            foreach (var gs in gearSets)
            {
                string name = gs.Name ?? $"Set {gs.Id}";
                Console.Write($"{name.PadRight(nameColumnWidth)}");

                double val2 = gs.Gains[GainType.Eff] + gs.Gains[GainType.Dmg] + gs.Gains[GainType.Def];
                Console.Write($"{val2,11:0.00}");

                foreach (var gt in orderedGainTypes)
                {
                    double val = 0;
                    if (gs.Gains != null && gs.Gains.TryGetValue(gt, out double gain))
                        val = gain;

                    Console.Write($"{val,11:0.00}");
                }

                Console.WriteLine();
            }
        }



        public static void PrintTopStatAllocs(Results results)
        {
            if (results?.altGearSets == null || results.altGearSets.Count == 0)
            {
                Console.WriteLine("No gear sets available.");
                return;
            }

            // Safely get a gain value, treating missing entries as 0
            double Get(GearSet gs, GainType t)
            {
                if (gs?.Gains == null) return 0;
                return gs.Gains.TryGetValue(t, out var v) ? v : 0;
            }

            // Find the best gearset by a given score selector
            (GearSet gs, double val) Best(Func<GearSet, double> selector)
            {
                GearSet best = null;
                double bestVal = double.NegativeInfinity;

                foreach (var g in results.altGearSets)
                {
                    var s = selector(g);
                    if (s > bestVal)
                    {
                        bestVal = s;
                        best = g;
                    }
                }
                return (best, bestVal);
            }

            // Try to get a human-friendly name; fall back to ToString()
            string NameOf(GearSet g)
            {
                if (g == null) return "<none>";
                var nameProp = g.GetType().GetProperty("Name");
                var name = nameProp?.GetValue(g) as string;
                return string.IsNullOrWhiteSpace(name) ? g.ToString() : name;
            }

            var topEff = Best(g => Get(g, GainType.Eff));
            var topDmg = Best(g => Get(g, GainType.Dmg));
            var topEffDmg = Best(g => Get(g, GainType.Eff) + Get(g, GainType.Dmg));
            var topEffDef = Best(g => Get(g, GainType.Eff) + Get(g, GainType.Def));
            var topEffDmgDef = Best(g => Get(g, GainType.Eff) + Get(g, GainType.Dmg) + Get(g, GainType.Def));

            Console.WriteLine("Top GearSets by Category:");
            Console.WriteLine($"eff:              {NameOf(topEff.gs)} ({topEff.val:F3})");
            Console.WriteLine($"dmg:              {NameOf(topDmg.gs)} ({topDmg.val:F3})");
            Console.WriteLine($"eff + dmg:        {NameOf(topEffDmg.gs)} ({topEffDmg.val:F3})");
            Console.WriteLine($"eff + def:        {NameOf(topEffDef.gs)} ({topEffDef.val:F3})");
            Console.WriteLine($"eff + dmg + def:  {NameOf(topEffDmgDef.gs)} ({topEffDmgDef.val:F3})");
        }
        public static void PrintTrinketCompResults(Results results)
        {
            var gearSets = results.altGearSets;
            var originals = results.OriginalTotals;
            var nullTrink1 = results.altGearSets[0];
            if (gearSets == null || gearSets.Count == 0)
            {
                Console.WriteLine("No gear sets to display.");
                return;
            }

            // Define the exact GainType order you want
            var orderedGainTypes = new[]
            {
                GainType.Eff,
                GainType.Dmg,
                GainType.Def,
                GainType.SupEff,
                GainType.SupDmg,
                GainType.MsEff,
                GainType.MsDmg,
                GainType.BalEff,
                GainType.BalDmg
            };

            // Determine padding based on the longest gear set name
            int longestNameLength = gearSets
                .Select(gs => (gs.Name ?? $"Set {gs.Id}").Length)
                .DefaultIfEmpty(0)
                .Max();

            int nameColumnWidth = Math.Max(longestNameLength + 2, 20); // Add a small buffer for readability

            // Header
            Console.Write("".PadRight(nameColumnWidth));
            Console.Write($"{"Total",10}");
            foreach (var gt in orderedGainTypes)
                Console.Write($"{gt,10}");
            Console.WriteLine();

            Console.Write($"{"Originals".PadRight(nameColumnWidth)}");
            double val_o = originals[GainType.Eff] + originals[GainType.Dmg] + originals[GainType.Def];
            Console.Write($"{val_o,11:0.00}");

            foreach (var gt in orderedGainTypes)
            {
                double val = 0;
                if (originals != null && originals.TryGetValue(gt, out double gain))
                    val = gain;

                Console.Write($"{val,11:0.00}");
            }

            Console.WriteLine();
            // Rows
            var nullTrink1Tot = nullTrink1.Gains[GainType.Eff] + nullTrink1.Gains[GainType.Dmg] + nullTrink1.Gains[GainType.Def];
            foreach (var gs in gearSets)
            {
                string name = gs.Name ?? $"Set {gs.Id}";
                Console.Write($"{name.PadRight(nameColumnWidth)}");

                double val2 = gs.Gains[GainType.Eff] + gs.Gains[GainType.Dmg] + gs.Gains[GainType.Def] - nullTrink1Tot;
                Console.Write($"{val2,11:0.00}");

                foreach (var gt in orderedGainTypes)
                {
                    double val = 0;
                    if (gs.Gains != null && gs.Gains.TryGetValue(gt, out double gain))
                        val = gain - nullTrink1.Gains[gt];

                    Console.Write($"{val,11:0.00}");
                }

                Console.WriteLine();
            }
        }

    }
}
