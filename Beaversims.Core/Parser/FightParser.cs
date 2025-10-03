
using Beaversims.Core.Common;
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
        public static Fight ParseFight(JsonElement fightData, string reportCode)
        {

            var fight = new Fight();
            var totalLogTime = fightData.GetProperty("endTime").GetInt32() - fightData.GetProperty("startTime").GetInt32();
            fight.TotalTime = Utils.ConvertLogTime(totalLogTime, 0);
            fight.Id = fightData.GetProperty("id").GetInt32();
            fight.EncounterId = fightData.GetProperty("encounterID").GetInt32();
            fight.Name = fightData.GetProperty("name").ToString();
            fight.ReportCode = reportCode;

            return fight;
        }

    }
}
