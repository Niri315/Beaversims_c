using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Shadow.ImportLogs
{
    internal class Queries
    {
        public static string StandardSimQuery(string reportCode, int fightId, int userId)
        {
            return $@"
    query {{
        reportData {{
            report(code: ""{reportCode}"") {{
                fightData: fights (
                    fightIDs: [{fightId}],
                ){{
                    id,
                    startTime,
                    endTime,
                    encounterID,
                    fightPercentage,
                    inProgress,
                    name,
                    kill,
                    difficulty
                }}
                userEvents: events(
                    fightIDs: [{fightId}],
                    useAbilityIDs: false
                    includeResources: true
                    sourceID: {userId}
                ) {{ data }}
                playerData: table(
                    fightIDs: [{fightId}]
                )
                combatantEvents: events(
                    dataType: CombatantInfo
                    fightIDs: [{fightId}]
                ) {{ data }}                      
            }}
        }}
    }}
";
        }
        public static string FightsQuery(string reportCode)
        {
            return $@"
{{
    reportData {{
        report(code: ""{reportCode}"") {{
            fights {{
                id
                startTime
                endTime
                name
                kill
                difficulty
                encounterID
                fightPercentage
                friendlyPlayers
            }}
        }}
    }}
}}";
        }
    }
}
