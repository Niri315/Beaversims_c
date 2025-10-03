using Beaversims.Core;
using Beaversims.Core.Common;
using Beaversims.Core.Shadow.WclClient;
using System.Diagnostics;
using System.Text.Json;
using static System.Net.WebRequestMethods;


var logLink = "https://www.warcraftlogs.com/reports/8DqKYV9vhZmj7QJW?type=healing&fight=48&phase=4&source=14";
var simAll = true;
var ignoreTrash = true;
var ignoreWipes = false;
var bossSpecific = true;
var bossName = "Nexus-King Salhadaar";


var totalTime = Stopwatch.StartNew();

var linkElements = WclClient.ParseLogLink(logLink);

var reportCode = linkElements[0];
int fightId = int.Parse(linkElements[1]);
int userId = int.Parse(linkElements[2]);
var finalResults = new Results();

if (simAll)
{
    var fightLogs = await WclClient.GetFights(reportCode);
    var fights = fightLogs.RootElement
        .GetProperty("data").GetProperty("reportData")
        .GetProperty("report").GetProperty("fights")
        .EnumerateArray()
        .Where(f =>
            (!ignoreTrash || f.GetProperty("encounterID").GetInt32() != 0) &&
            (!ignoreWipes || f.GetProperty("kill").GetBoolean()) &&
            f.GetProperty("friendlyPlayers").EnumerateArray().Any(p => p.GetInt32() == userId) &&
        (!bossSpecific || f.GetProperty("name").GetString() == bossName))
        .Select(f => f.GetProperty("id").GetInt32())
        .ToList();

    var degree = Environment.ProcessorCount;
    var lockObj = new object();

    await Parallel.ForEachAsync(fights, new ParallelOptions { MaxDegreeOfParallelism = degree }, async (fightId, ct) =>
    {
        var logs = await WclClient.GetLogs(reportCode, fightId, userId);
        var results = Main.Run(logs, userId, reportCode);

        lock (lockObj)
        {
            // Merge into the shared finalResults
            finalResults.TotalTime += results.TotalTime;
            foreach (var statEntry in results.StatGains)
            {
                var stat = statEntry.Key;
                foreach (var gainEntry in statEntry.Value)
                {
                    var gainType = gainEntry.Key;
                    finalResults.StatGains[stat][gainType] += gainEntry.Value;
                }
            }
        }
    });
}
else
{
    var logs = await WclClient.GetLogs(reportCode, fightId, userId);
    finalResults = Main.Run(logs, userId, reportCode);
}


finalResults.SetSwGains();
TestUtils.PrintStatWeights(finalResults.swGains);
totalTime.Stop();
Console.WriteLine($"Total Time: {totalTime}");