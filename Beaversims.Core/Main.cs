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
            return Run(logs, userId, reportCode, simMode: SimMode.SW, iterationCount: 0);
        }

        public static Results TgMain(JsonDocument logs, int userId, string reportCode, JsonDocument gearSets, int iterationCount = Constants.defaultIterCount)
        {
            return Run(logs, userId, reportCode, simMode: SimMode.TopGear, iterationCount, gearSets);
        }

        public static Results SaMain(JsonDocument logs, int userId, string reportCode)
        {
            return Run(logs, userId, reportCode, simMode: SimMode.StatAlloc, iterationCount: 0);
        }


        public static Results Run(JsonDocument logs, int userId, string reportCode, SimMode simMode, int iterationCount = Constants.defaultIterCount, JsonDocument? gearSets= null)
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
            var allUnits = UnitParser.ParseUnits(playerData, combatantEvents, userInfo, userId, fight, simMode);
            var events = EventParser.ParseUserEvents(userEvents, allUnits, fight);
            var user = allUnits.GetUser();
            foreach (var ability in user.Abilities)
            {
                if (ability.GCD)
                {
                    ability.CastTime = Constants.GCD;
                }
            }
 
            var summer_de = new Shared.DupliEffects.Summer((Shared.Abilities.BlessingOfSummer)user.Abilities.Get(Shared.Abilities.BlessingOfSummer.name));
            var leech_de = new Shared.DupliEffects.Leech(user.Abilities.Get(Shared.Abilities.Leech.name));
          
            user.SharedDupliEffects.Add(summer_de);
            user.SharedDupliEffects.Add(leech_de);

            ItemSim.CreateGearSets(user, gearSets);

            user.Spec.SpecIteration(events, allUnits, fight, iterationCount);

            var results = new Results();
            ProcessEvents.SharedIteration(events, fight, user, results);

            Console.WriteLine($"Fight Id : {fight.Id}");
            Console.WriteLine($"Fight Time: {fight.TotalTime}");
            Console.WriteLine($"User Uptime: {user.TrueCastTimeTotal / fight.TotalTime}");
            Console.WriteLine($"Cast Time Total: {user.TrueCastTimeTotal}");

            results.TotalTime = fight.TotalTime;
            results.ToPerSec();
            results.SpecName = Utils.SpecNameToString(user.Spec.SpecName);
            results.HeroTlName = Utils.HeroTlNameToString(user.Spec.HeroTlName);
            results.FightId = fight.EncounterId;
            results.FightName = fight.Name;
            results.PlayerName = user.Name;
            results.Success = fight.Success;
            results.WipePercent = fight.WipePercent;
            results.Difficulty = fight.Difficulty;
            return results;
        }
    }
}