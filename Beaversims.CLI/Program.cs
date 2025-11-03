using Beaversims.Core;
using Beaversims.Core.Common;
using Beaversims.Core.Shadow.WclClient;
using Beaversims.Core.Sim;
using System.Diagnostics;
using System.Text.Json;
using static System.Net.WebRequestMethods;




//var simAll = false; 
//var ignoreTrash = true;
//var ignoreWipes = false;
//var bossName = "Dimensius, the All-Devouring";




namespace Beaversims.CLI
{
    internal class Program
    {
        static async Task Main(string[] args) // <-- Make Main async
        {
            //var logLink = "https://www.warcraftlogs.com/reports/PxzAyBCDL7acXRvg?fight=11&type=healing&source=6"; //Salad Nali LS
            //var logLink = "https://www.warcraftlogs.com/reports/PJWrjZv6xTpLYmct?fight=128&type=healing&source=1863";  //Salad Ellesmere Herald
            //var logLink = "https://www.warcraftlogs.com/reports/m4vPb3J71twFXVTA?fight=17&type=damage-done&source=166";  //WTF mastery?
            //var logLink = "https://www.warcraftlogs.com/reports/jknw3D642CpcALgq?fight=54&type=healing&source=22"; // Leech + martyr -> need fi
            //var logLink = "https://www.warcraftlogs.com/reports/vlhzymap2dgxfwkr?fight=34&type=healing&source=24"; //frac nali antenna
            var logLink = "https://www.warcraftlogs.com/reports/J1Z84KGD2BfyTHrP?fight=11&source=3"; //  

            var swMode = true;
            var totalTime = Stopwatch.StartNew();
            var linkElements = WclClient.ParseLogLink(logLink);

            var reportCode = linkElements[0];
            int fightId = int.Parse(linkElements[1]);
            int userId = int.Parse(linkElements[2]);
            var logs = await WclClient.GetLogs(reportCode, fightId, userId);
            Results finalResults;

            if (swMode)
            {
                finalResults = RunMain.SwMain(logs, userId, reportCode);
            }
            else
            {
                finalResults = RunMain.GcMain(logs, userId, reportCode);
            }

            //TestUtils.PrintStatWeights(finalResults.swGains);
            TestUtils.PrintAltGearResults(finalResults);

            totalTime.Stop();
            Console.WriteLine($"Total Time: {totalTime}");
        }
    }
}


//if (simAll)
//{
//    var fightLogs = await WclClient.GetFights(reportCode);
//    var fights = fightLogs.RootElement
//        .GetProperty("data").GetProperty("reportData")
//        .GetProperty("report").GetProperty("fights")
//        .EnumerateArray()
//        .Where(f =>
//            (!ignoreTrash || f.GetProperty("encounterID").GetInt32() != 0) &&
//            (!ignoreWipes || f.GetProperty("kill").GetBoolean()) &&
//            f.GetProperty("friendlyPlayers").EnumerateArray().Any(p => p.GetInt32() == userId) &&
//        (bossName == "" || f.GetProperty("name").GetString() == bossName))
//        .Select(f => f.GetProperty("id").GetInt32())
//        .ToList();

//    var degree = Environment.ProcessorCount;
//    var lockObj = new object();

//    await Parallel.ForEachAsync(fights, new ParallelOptions { MaxDegreeOfParallelism = degree }, async (fightId, ct) =>
//    {
//        var logs = await WclClient.GetLogs(reportCode, fightId, userId);
//        var results = Main.Run(logs, userId, reportCode);
//        lock (lockObj)
//        {
//            // Merge into the shared finalResults
//            finalResults.TotalTime += results.TotalTime;

//            foreach (var statEntry in results.StatGains)
//            {
//                var stat = statEntry.Key;
//                foreach (var gainEntry in statEntry.Value)
//                {
//                    var gainType = gainEntry.Key;
//                    finalResults.StatGains[stat][gainType] += gainEntry.Value;
//                }
//            }
//            for (int i = 0; i < results.altGearSets.Count; i++)
//            {
//                var altGearSet = results.altGearSets[i];
//                if (i < finalResults.altGearSets.Count)
//                {
//                    foreach (var gainEntry in altGearSet.Gains)
//                    {
//                        finalResults.altGearSets[i].Gains[gainEntry.Key] += gainEntry.Value;
//                    }
//                }
//                else
//                {
//                    finalResults.altGearSets.Add(ItemSim.DeepCloneGearset(altGearSet));
//                }
//            }
//        }
//    });
//}
//else
//{
//var logs = await WclClient.GetLogs(reportCode, fightId, userId);
//finalResults = Main.Run(logs, userId, reportCode);
//}
