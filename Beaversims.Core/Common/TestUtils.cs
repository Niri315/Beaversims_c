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
        public static void PrintAltGearResults(List<GearSet> gearSets)
        {
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

            // Header
            Console.Write("".PadRight(30));
            Console.Write($"{"Total",10}");
            foreach (var gt in orderedGainTypes)
                Console.Write($"{gt,10}");
            Console.WriteLine();

            // Rows
            foreach (var gs in gearSets)
            {
                Console.Write($"{gs.Name ?? $"Set {gs.Id}",-30}");

                double val2 = gs.Gains[GainType.Eff] + gs.Gains[GainType.Dmg] + gs.Gains[GainType.Def];
                Console.Write($"{val2,10:0.00}");
                foreach (var gt in orderedGainTypes)
                {
                    double val = 0;
                    if (gs.Gains != null && gs.Gains.TryGetValue(gt, out double gain))
                        val = gain;

                    Console.Write($"{val,10:0.00}");
                }


                Console.WriteLine();
            }
        }

    }
}
