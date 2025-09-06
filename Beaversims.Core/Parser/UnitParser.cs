using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Beaversims.Core.Parser
{
    internal static class UnitParser
    {

        public static UnitRepository ParseUnits(JsonElement playerData, JsonElement combatantEvents, int userId)
        {
            var allUnits = new UnitRepository();

            foreach (var player in playerData.GetProperty("composition").EnumerateArray())
            {
                var name = player.GetProperty("name").GetString();
                var id = player.GetProperty("id").GetInt32();
                var instanceId = 0;

                var unit = new Unit(name, id, instanceId);
                unit.Role = Enum.Parse<Role>(player.GetProperty("specs")[0].GetProperty("role").GetString()!,ignoreCase: true);
            }
            return allUnits;
        }
    }
}
