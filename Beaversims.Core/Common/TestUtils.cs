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


    }
}
