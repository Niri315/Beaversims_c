using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Beaversims.Core.Parser
{
    internal static class FightParser
    {
        public static Fight ParseFight(JsonElement fightData)
        {

            var fight = new Fight();
            var totalLogTime = fightData.GetProperty("endTime").GetInt32() - fightData.GetProperty("startTime").GetInt32();
            fight.TotalTime = Utils.ConvertLogTime(totalLogTime, 0);
            return fight;
        }

    }
}
