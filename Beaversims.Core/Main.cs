using Beaversims.Core.Common;
using Beaversims.Core.Parser;
using Beaversims.Core.Shadow.WclClient;
using Beaversims.Core.Shared;
using Beaversims.Core.Sim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    internal class RunMain
    {
        public static Results SwMain(JsonDocument logs, int userId, string reportCode)
        {
            return Run(logs, userId, reportCode, swMode: true, iterationCount: 0);
        }

        public static Results GcMain(JsonDocument logs, int userId, string reportCode, int iterationCount = Constants.defaultIterCount)
        {
            return Run(logs, userId, reportCode, swMode: false, iterationCount);
        }

        private static Results Run(JsonDocument logs, int userId, string reportCode, bool swMode, int iterationCount)
        {

  
            var userEvents = logs.RootElement.GetProperty("data").GetProperty("reportData").GetProperty("report").GetProperty("userEvents").GetProperty("data");
            var playerData = logs.RootElement.GetProperty("data").GetProperty("reportData").GetProperty("report").GetProperty("playerData").GetProperty("data");
            var combatantEvents = logs.RootElement.GetProperty("data").GetProperty("reportData").GetProperty("report").GetProperty("combatantEvents").GetProperty("data");
            var fightData = logs.RootElement.GetProperty("data").GetProperty("reportData").GetProperty("report").GetProperty("fightData")[0];

            JsonElement userInfo = default;

            foreach (var userEvent in userEvents.EnumerateArray())
            {
                if (userEvent.GetProperty("type").ToString() == "combatantinfo")
                {
                    userInfo = userEvent;
                    break;
                }
            }

            var fight = FightParser.ParseFight(fightData, reportCode);
            var allUnits = UnitParser.ParseUnits(playerData, combatantEvents, userInfo, userId, fight, swMode);
            var events = EventParser.ParseUserEvents(userEvents, allUnits, fight);
            var user = allUnits.GetUser();

            ItemSim.CreateGearSets(user);
            user.Spec.SpecIteration(events, allUnits, fight, iterationCount);

            var results = new Results();
            ProcessEvents.SharedIteration(events, fight, user, results);

            Console.WriteLine($"Fight Id : {fight.Id}");
            Console.WriteLine($"Fight Time: {fight.TotalTime}");
            Console.WriteLine($"User Uptime: {user.TrueCastTimeTotal / fight.TotalTime}");
            Console.WriteLine($"Cast Time Total: {user.TrueCastTimeTotal}");

            results.TotalTime = fight.TotalTime;
            results.ToPerSec();
            return results;
        }
    }
}