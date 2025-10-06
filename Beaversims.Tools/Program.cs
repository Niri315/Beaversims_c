
using System;
using System.Text.Json;

namespace Beaversims.Tools
{
    internal static class Program
    {
        static void CheckItemDuplicates(string inputPath)
        {
            var json = File.ReadAllText(inputPath);
            var items = JsonSerializer.Deserialize<List<JsonElement>>(json)!;

            var dupes = items
                .Select(i => i.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "")
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .GroupBy(n => n)
                .Where(g => g.Count() > 1);

            foreach (var g in dupes)
            {
                Console.WriteLine($"{g.Key} appears {g.Count()} times");
            }
        }

        static void CheckDuplis2Items(string outputPath)
        {
            var lines = File.ReadAllLines(outputPath);
            var keys = lines
                .Where(l => l.TrimStart().StartsWith("["))
                .Select(l => l.Split('=')[0].Trim())
                .ToList();

            var dupes = keys.GroupBy(k => k).Where(g => g.Count() > 1);
            foreach (var g in dupes)
            {
                Console.WriteLine($"Duplicate in generated file: {g.Key} ({g.Count()} times)");
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Generating Files ...");

            // Path to input file in Tools/SimcData
            var baseInput = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "SimcData"));
            var baseOutput = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Beaversims.Core", "Data", "Dbc"));

            var input = Path.Combine(baseInput, "rand_prop_points.inc");
            var output = Path.Combine(baseOutput, "RandPropPoints.cs");
            var namespace_s = "Beaversims.Core.Data.Dbc";
            Generators.ConvertDataFiles.ConvertRandPropPointsToCs(input, output, namespace_s);
            input = Path.Combine(baseInput, "equippable-items-full.json");
            output = Path.Combine(baseOutput, "Items.json");
            Generators.ConvertDataFiles.ConvertJsonItems(input, output);
            input = Path.Combine(baseInput, "sc_scale_data.inc");
            output = Path.Combine(baseOutput, "ScaleData.cs");
            Generators.ConvertDataFiles.ConvertScScaleDataToCs(input, output, namespace_s);

            Console.WriteLine($"Done. File written to: {output}");
        }
    }
}