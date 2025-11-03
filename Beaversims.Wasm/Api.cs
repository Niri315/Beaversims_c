using System.Text.Json;
using System.Runtime.InteropServices.JavaScript;
using Beaversims.Core;

namespace Beaversims.Wasm;

public static partial class Api
{
    [JSExport]
    public static string RunStatWeights(string logsJson, int userId, string reportCode)
    {
        try
        {
            using var logs = JsonDocument.Parse(logsJson);
            var r = RunMain.SwMain(logs, userId, reportCode);

            // ---- map to a serializer-friendly DTO ----
            var dto = new
            {
                totalTime = r.TotalTime,
                originalTotals = r.OriginalTotals
                    .ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value),   // string keys + double
                altGearSets = r.altGearSets.Select(gs => new
                {
                    // include what you actually need; keep it primitive
                    gains = gs.Gains.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value),
                    id = gs.Id,
                    name = gs.Name
                })
            };

            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
            return json;
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    //[JSExport]
    //public static string RunTopGear(string logsJson, int userId, string reportCode, string directiveJson)
    //{
    //    try
    //    {
    //        using var logs = JsonDocument.Parse(logsJson);
    //        var directive = JsonSerializer.Deserialize<GearDirective>(directiveJson);
    //        var results = RunMain.GcMain(logs, userId, reportCode, directive); // or your own entrypoint
    //        var dto = MapResultsToDto(results); // same shape you used for SW (id, name, gains)
    //        return JsonSerializer.Serialize(dto, new JsonSerializerOptions
    //        {
    //            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        return JsonSerializer.Serialize(new { error = ex.Message });
    //    }
    //}
}