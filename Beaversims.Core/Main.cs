using Beaversims.Core.Common;
using Beaversims.Core.Parser;
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
    internal class Results
    {
        public double TotalTime { get; set; } = 0;
        public GainMatrix StatGains { get; set; } = Utils.InitGainMatrix();
        public GainMatrix swGains {  get; set; } = Utils.InitGainMatrix();
        public void SetSwGains()
        {
            foreach (var statEntry in StatGains)
            {
                var stat = statEntry.Key;
                foreach (var gainEntry in statEntry.Value)
                {
                    var gainType = gainEntry.Key;
                    swGains[stat][gainType] = gainEntry.Value / TotalTime;
                }
            }
        }
        public Results() { }
    }
    internal class Main
    {
        public static Results Run(JsonDocument logs, int userId, string reportCode)
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
            var allUnits = UnitParser.ParseUnits(playerData, combatantEvents, userInfo, userId, fight);
            var events = EventParser.ParseUserEvents(userEvents, allUnits, fight);

            var user = allUnits.GetUser();
            user.Spec.SpecIteration(events, allUnits, fight);
            var results = new Results();
            ProcessEvents.SharedIteration(events, fight, user, results);
            ItemSim.TopItems(events, user, fight);
            Console.WriteLine($"Fight Id : {fight.Id}");
            Console.WriteLine($"User HCGM: {user.HCGM}");
            results.TotalTime = fight.TotalTime;



            return results;
        }
    }
}
