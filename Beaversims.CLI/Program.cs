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
        static async Task Main(string[] args)
        {
            //var logLink = "https://www.warcraftlogs.com/reports/PxzAyBCDL7acXRvg?fight=11&type=healing&source=6"; //Salad Nali LS
            //var logLink = "https://www.warcraftlogs.com/reports/PJWrjZv6xTpLYmct?fight=128&type=healing&source=1863";  //Salad Ellesmere Herald
            //var logLink = "https://www.warcraftlogs.com/reports/m4vPb3J71twFXVTA?fight=17&type=damage-done&source=166";  //WTF mastery?
            //var logLink = "https://www.warcraftlogs.com/reports/jknw3D642CpcALgq?fight=54&type=healing&source=22"; // Leech + martyr -> need fi
            //var logLink = "https://www.warcraftlogs.com/reports/vlhzymap2dgxfwkr?fight=34&type=healing&source=24"; //frac nali antenna
            //var logLink = "https://www.warcraftlogs.com/reports/8DqKYV9vhZmj7QJW?fight=32&type=healing&source=19"; // Erooxdruid Soulhunters
            //var logLink = "https://www.warcraftlogs.com/reports/aB9HyhkFgKGnjM3R?fight=51&type=casts&source=2"; // dim druid Ns
            //var logLink = "https://www.warcraftlogs.com/reports/aB9HyhkFgKGnjM3R?fight=30&type=healing&source=2"; // frac druid
            //var logLink = "https://www.warcraftlogs.com/reports/tDhM4LRkB8yamrVF?fight=13&type=damage-done&source=143"; // embrace the dream
            //var logLink = "https://www.warcraftlogs.com/reports/87JLGncYpP9Mwgd3?fight=20&type=damage-done&source=15"; // dream of cenarius
            //var logLink = "https://www.warcraftlogs.com/reports/XBHvP9xVrpjDZn13?fight=14&type=casts&translate=true&source=3"; // evoker chrono
            //var logLink = "https://www.warcraftlogs.com/reports/cAWLyDhNpKxBJF1P?fight=64&type=healing&source=22"; // evoker flameshaper
            //var logLink = "https://www.warcraftlogs.com/reports/FcWt4wLRP6GQJzAh?fight=2&source=6&type=healing"; // 0 heal evoker
            //var logLink = "https://www.warcraftlogs.com/reports/bBWMANk9yjdTPR76?fight=10&type=summary&source=22"; //  stat alloc source
            //var logLink = "https://www.warcraftlogs.com/reports/LZWxmFpnKg81v4D2?fight=6&type=healing&source=7"; //  Seteth
            //var logLink = "https://www.warcraftlogs.com/reports/HqRJWQ3y16F9ZwPL?fight=16&type=healing&source=16"; //  ancestral awakening'
            //var logLink = "https://www.warcraftlogs.com/reports/rfyXTLpA1zRqxNhV?fight=15&type=healing&source=124"; //  ancestral awakening
            //var logLink = "https://www.warcraftlogs.com/reports/tLWgY37r4PGAX9m1?fight=11&type=summary&source=222"; // beacon of faith + pol
            //var logLink = "https://www.warcraftlogs.com/reports/AZN3YFaBt4prDyz1?fight=11&type=healing&source=3"; // enkindle
            var logLink = "https://www.warcraftlogs.com/reports/QMkHwKWZ8b9xjDyv?fight=262&type=damage-done&source=1374"; // veneration

            SimMode simMode = SimMode.SW;
            var totalTime = Stopwatch.StartNew();
            var linkElements = WclClient.ParseLogLink(logLink);

            var reportCode = linkElements[0];
            int fightId = int.Parse(linkElements[1]);
            int userId = int.Parse(linkElements[2]);
            var logs = await WclClient.GetLogs(reportCode, fightId, userId);
            Results finalResults;

            var iterationCount = 0;
            JsonDocument gearSets = null;


            if (simMode == SimMode.TopGear) 
            {
                string gearSetsPath = Path.Combine(Utils.ProjectRoot(), "Shadow", "gearSetsJson.json");
                string gearsetsJson = System.IO.File.ReadAllText(gearSetsPath);
                gearSets = JsonDocument.Parse(gearsetsJson);
                iterationCount = Constants.defaultIterCount;
            }
            else if (simMode == SimMode.Trinkets)
            {
                iterationCount = Constants.defaultIterCount;
                iterationCount = 2000;
            }

            finalResults = RunMain.Run(logs, userId, reportCode, simMode, iterationCount, gearSets);
            if (simMode == SimMode.StatAlloc)
            {
                //finalResults = RunMain.Run(logs, userId, reportCode, simMode, 0);
                TestUtils.PrintTopStatAllocs(finalResults);
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
