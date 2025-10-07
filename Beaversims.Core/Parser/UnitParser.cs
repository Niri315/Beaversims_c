using Beaversims.Core.Common;
using Beaversims.Core.Specs.Paladin.Holy;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;



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
            return new DummySpec();
            //throw new InvalidOperationException("No matching spec could be found for the given talent tree.");
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

        public static void GetStarterRatings(User user, JsonElement userInfo)
        {
            
            var stats = user.Stats;
            var leechRating = userInfo.GetProperty("leech").GetInt32();

            if (leechRating > stats.Get(StatName.Leech).Rating)
            {
                user.HasPermaLeech = true;
            }
            stats.Get(StatName.Intellect).Rating = userInfo.GetProperty("intellect").GetInt32();
            stats.Get(StatName.Stamina).Rating = userInfo.GetProperty("stamina").GetInt32();
            stats.Get(StatName.Crit).Rating = userInfo.GetProperty("critSpell").GetInt32();
            stats.Get(StatName.Haste).Rating = userInfo.GetProperty("hasteSpell").GetInt32();
            stats.Get(StatName.Mastery).Rating = userInfo.GetProperty("mastery").GetInt32();
            stats.Get(StatName.Vers).Rating = userInfo.GetProperty("versatilityHealingDone").GetInt32();
            stats.Get(StatName.Leech).Rating = leechRating;
            stats.Get(StatName.Avoidance).Rating = userInfo.GetProperty("avoidance").GetInt32();
            stats.UpdateAllStats();
        }

        
        public static void SetItemsTalents(JsonElement playerData, UnitRepo allUnits)
        {
            var user = allUnits.GetUser();
            foreach (var playerRole in playerData.GetProperty("playerDetails").EnumerateObject())
            {
                foreach (var playerElement in playerRole.Value.EnumerateArray())
                {
                    var playerId = new UnitId(playerElement.GetProperty("id").GetInt32(), 0);
                    var unit = allUnits.Get(playerId);
                    Player player = (Player)unit!;
                    foreach (var item in playerElement.GetProperty("combatantInfo").GetProperty("gear").EnumerateArray())
                    {
                        var itemId = item.GetProperty("id").GetInt32();
                        if (itemId == 0) { continue; }  // No item
                        // Item slot in logs is 1 to short, adding +1 makes it correspond to normal wow rules.
                        var itemSlot = (ItemSlot)item.GetProperty("slot").GetInt32() + 1;
                        var itemName = item.GetProperty("name").GetString();
                        var ilvl = item.GetProperty("itemLevel").GetInt32();

                        if (player is User)
                        {
                            List<int> bonusIds;
                            if (item.TryGetProperty("bonusIDs", out var bonus))
                            {
                                bonusIds = bonus.EnumerateArray()
                                                    .Select(e => e.GetInt32())
                                                    .ToList();
                            }
                            else
                            {
                                bonusIds = new List<int>();
                            }
                                var gainItem = Sim.ItemGenerator.CreateItem(itemName, ilvl, itemSlot, bonusIds);
                            user.Items[itemSlot] = gainItem;
                        }
                        else
                        {
                            player.Items[itemSlot] = new Item(itemId, itemName, ilvl, itemSlot);
                        }

                    }
                    foreach (var talentElement in playerElement.GetProperty("combatantInfo").GetProperty("talentTree").EnumerateArray())
                    {
                        var talentId = talentElement.GetProperty("id").GetInt32();
                        var talentRank = talentElement.GetProperty("rank").GetInt32();
                        Talent talent;
                        if (player == user)
                        {
                            talent = user.Spec.CreateTalent(talentId, talentRank);
                        }
                        else
                        {
                            talent = new Talent(talentId, talentRank);
                        }
                        player.Talents[talentId] = talent;
                    }
                }
            }
        }


        public static bool VantusCheck(string name, Fight fight) => name.EndsWith(fight.Name);


        public static void AddStarterBuffs(JsonElement combatantEvents, UnitRepo allUnits, Fight fight)
        {
            foreach (var playerElement in combatantEvents.EnumerateArray())
            {
                var playerId = new UnitId(playerElement.GetProperty("sourceID").GetInt32(), 0);
                var unit = allUnits.Get(playerId);
                Player player = (Player)unit!;
                foreach (var buffElement in playerElement.GetProperty("auras").EnumerateArray())
                {
                    var buffId = buffElement.GetProperty("ability").GetInt32();
                    var sourceTypeId = buffElement.GetProperty("source").GetInt32();
                    var sourceUnitId = new UnitId(sourceTypeId, 0);
                    var stacks = buffElement.TryGetProperty("stacks", out JsonElement stacksElement)
                        ? stacksElement.GetInt32()
                        : 1;
                    var buffName = buffElement.GetProperty("name").GetString();

                    if (buffName.StartsWith("Vantus Rune"))
                    {
                        if (VantusCheck(buffName, fight))
                        {
                            var user = allUnits.GetUser();
                            user.HasVantus = true;
                        }
                        continue;
                    }

                    Unit sourceUnit;

                    if (allUnits.Contains(sourceUnitId)) 
                    {
                        sourceUnit = allUnits.Get(sourceUnitId);
                    }
                    else
                    {  // Failsafe incase source is from a non player, making a guess of instanceId = 1.
                        // Better than nothing at least.
                        var npcId = new UnitId(sourceTypeId, 1);
                        Unit npcUnit = new Unit("Npc", npcId);
                        allUnits.Add(npcUnit);
                        sourceUnit = npcUnit;
                    }

                    player.AddBuff(buffName, buffId, sourceUnit, stacks);
                }
            }
        }



        public static UnitRepo ParseUnits(JsonElement playerData, JsonElement combatantEvents, JsonElement userInfo, int userId, Fight fight)
        {
            var allUnits = InitPlayers(playerData, userId);
            var user = allUnits.GetUser();
            user.Spec = FindSpec(userInfo);
            user.Spec.InitAbilities(user.Abilities);
            user.Spec.InitSharedAbilities(user.Abilities);
            user.Stats.InitMastery(user.Spec.MasteryPr);
            SetThroughputValues(allUnits, playerData);
            SetItemsTalents(playerData, allUnits);
            user.InitCustomBuffs();
            AddStarterBuffs(combatantEvents, allUnits, fight);
            GetStarterRatings(user, userInfo); // Resetting ratings here.
            // Vantus goes here, after ratings reset.
            if (user.HasVantus) 
            {
                user.AddBuff("Vantus Rune" + fight.Name, Constants.curVantusId, user, 1);
            }

            return allUnits;
        }
    }
}
