using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Beaversims.Core.Specs.Paladin.Holy;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;

namespace Beaversims.Core
{
    public readonly record struct UnitId(int TypeId, int InstanceId);
}


namespace Beaversims.Core.Parser
{
    internal static class UnitParser
    {

        public static UnitRepo InitPlayers(JsonElement playerData, int userTypeId)
        {
            var userId = new UnitId(userTypeId, 0);
            var allUnits = new UnitRepo(userId);
            foreach (var playerElement in playerData.GetProperty("composition").EnumerateArray())
            {
                var name = playerElement.GetProperty("name").GetString()!;
                var typeId = playerElement.GetProperty("id").GetInt32();
                var instanceId = 0;
                var playerId = new UnitId(typeId, instanceId);
                var role = Enum.Parse<Role>(playerElement.GetProperty("specs")[0].GetProperty("role").GetString()!, ignoreCase: true);
                Player player;
                if (playerId == userId)
                {
                    player = new User(name, playerId, role);

                }
                else
                {
                    player = new Player(name, playerId, role);

                }
                allUnits.Add(player);
            }
            return allUnits;
        }


        public static Spec FindSpec(JsonElement userInfo)
        {
            foreach (var talentElement in userInfo.GetProperty("talentTree").EnumerateArray())
            {
                var talentId = talentElement.GetProperty("id").GetInt32();
                switch (talentId)
                {
                    case HolyLightsmith.idTalent:
                        return new HolyLightsmith();
                    case HolyHeraldOfTheSun.idTalent:
                        return new HolyHeraldOfTheSun();
                }
            }
            throw new InvalidOperationException("No matching spec could be found for the given talent tree.");
        }


        public static void SetThroughputValues(UnitRepo allUnits, JsonElement playerData)
        {

            static void ApplyTotals(UnitRepo allUnits, JsonElement playerData, string ThroughputType, Action<Player, long> assign)
            {
                foreach (var playerElement in playerData.GetProperty(ThroughputType).EnumerateArray())
                {
                    var playerId = Utils.GetPlayerId(playerElement.GetProperty("id").GetInt32());
                    if (allUnits.Contains(playerId))  //Filtering out non player anomalies.
                    {
                        if (allUnits.Get(playerId) is Player p)
                        {
                            var total = playerElement.GetProperty("total").GetInt64();
                            assign(p, total);
                        }
                    }
                }
            }
            ApplyTotals(allUnits, playerData, "damageDone", (p, v) => p.DamageDone = v);
            ApplyTotals(allUnits, playerData, "healingDone", (p, v) => p.HealingDone = v);
        }


        public static UnitRepo ParseUnits(JsonElement playerData, JsonElement combatantEvents, JsonElement userInfo, int userId)
        {
            var allUnits = InitPlayers(playerData, userId);
            var user = allUnits.GetUser();
            user.Spec = FindSpec(userInfo);
            user.Spec.InitAbilities(user.Abilities);
            user.Spec.InitSharedAbilities(user.Abilities);
            SetThroughputValues(allUnits, playerData);
            return allUnits;
        }
    }
}
