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
                }),
                specName = r.SpecName,             
                heroTlName = r.HeroTlName,          
                fightId = r.FightId,                
                fightName = r.FightName,   
                playerName = r.PlayerName,
                success = r.Success,
                difficulty = r.Difficulty,
                wipePercent = r.WipePercent

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

    [JSExport]
    public static string RunTopGear(string logsJson, int userId, string reportCode, string directiveJson)
    {
        try
        {
            using var logs = JsonDocument.Parse(logsJson);
            using var gearSets = JsonDocument.Parse(directiveJson);
            var r = RunMain.TgMain(logs, userId, reportCode, gearSets);

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
                }),
                success = r.Success,
                specName = r.SpecName,
                heroTlName = r.HeroTlName,
                fightId = r.FightId,
                fightName = r.FightName,
                playerName = r.PlayerName,
                difficulty = r.Difficulty,
                wipePercent = r.WipePercent,
            };

            return JsonSerializer.Serialize(dto, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}