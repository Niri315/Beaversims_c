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
    }
}
