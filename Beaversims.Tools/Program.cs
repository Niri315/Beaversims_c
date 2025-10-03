
using System;

namespace Beaversims.Tools
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Generating RandPropPoints.cs ...");

            // Path to input file in Tools/SimcData
            var baseInput = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "SimcData"));
            var baseOutput = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Beaversims.Core", "Data", "Dbc"));

            var input = Path.Combine(baseInput, "rand_prop_points.inc");
            var output = Path.Combine(baseOutput, "RandPropPoints.cs");
            var namespace_s = "Beaversims.Core.Data.Dbc";
            Generators.ConvertDataFiles.ConvertRandPropPointsToCs(input, output, namespace_s);
            input = Path.Combine(baseInput, "equippable-items-full.json");
            output = Path.Combine(baseOutput, "Items.cs");
            Generators.ConvertDataFiles.ConvertItemDataJson_ToCs(input, output, namespace_s);

            Console.WriteLine($"Done. File written to: {output}");
        }
    }
}