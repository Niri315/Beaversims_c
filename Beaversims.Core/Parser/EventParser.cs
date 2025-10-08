
using Beaversims.Core.Common;
using System.Text.Json;

namespace Beaversims.Core.Parser
{
    internal static class EventParser
    {
        private static readonly HashSet<int> bannedAbilityIds = new()
        {
            443330, // Engulf. There are 2 different casts... Unsure which is the "real" one, if an issue occurs its the other one.

        };
        private static readonly HashSet<string> empoweredAbilities = new()
        {
            // TODO get the strings from ability classes instead
            "Dream Breath",
            "Fire Breath",
            "Spiritbloom"
        };

        private static readonly HashSet<string> healEvents = new()
        {
            "absorbed",
            "healabsorbed",
            "heal"
        };
        private static readonly HashSet<string> castEvents = new()
        {
            "cast",
            "empowerend",
        };

        private static readonly HashSet<string> throughputEvents = new(healEvents.Concat(new[] { "damage" }));
        private static readonly HashSet<string> absorbedEvents = new(healEvents.Where(e => e != "heal"));

        public static bool IsThroughputEvent(string eventType) => throughputEvents.Contains(eventType);
        public static bool IsHealEvent(string eventType) => healEvents.Contains(eventType);
        public static bool IsDamageEvent(string eventType) => eventType == "damage";

        public static bool IsAbsorbedEvent(string eventType) => absorbedEvents.Contains(eventType);
        public static bool IsCastEvent(string eventType) => castEvents.Contains(eventType);
        public static bool IsBuffEvent(string eventType) => eventType.Contains("buff");


        public static bool SkipEvent(JsonElement logEvent)
        {
            if (logEvent.GetProperty("type").GetString() == "combatantinfo")
                return true;
            if (logEvent.GetProperty("type").GetString() == "staggerclear")  // TEMP REMOVE LATER
                return true;
            if (logEvent.GetProperty("type").GetString() == "staggerprevented")  // TEMP REMOVE LATER
                return true;
            if (logEvent.TryGetProperty("fake", out var fake) && fake.GetBoolean())
                return true;
            // Empowered casts have both a cast and empowerend event. Skip cast and use empowerend only.
            if (logEvent.TryGetProperty("ability", out var ability) &&
                empoweredAbilities.Contains(ability.GetProperty("name").GetString()!) &&
                logEvent.GetProperty("type").GetString() == "cast")
                return true;
            return false;
        }
        
        public static void AddNpcUnit(UnitId unitId, UnitRepo allUnits)
        {
            if (!allUnits.Contains(unitId))
            {
                var npcUnit = new Unit("NPC", unitId);
                allUnits.Add(npcUnit);
            }
        }

        public static void AdjustUnitIds(JsonElement logEvent, Event evt, UnitRepo allUnits)
        {
            var user = allUnits.GetUser();

            int sourceTypeId;
            int sourceInstanceId = 0;
            int targetTypeId = logEvent.GetProperty("targetID").GetInt32();
            int targetInstanceId = 0;

            if (logEvent.TryGetProperty("healerID", out JsonElement healerId)) // For heal absorb events
            {
                sourceTypeId = healerId.GetInt32();

                if (logEvent.TryGetProperty("healerInstance", out JsonElement healerInstance))
                {
                    sourceInstanceId = healerInstance.GetInt32();
                }
            }
            else
            {
                sourceTypeId = logEvent.GetProperty("sourceID").GetInt32();

                if (logEvent.TryGetProperty("sourceInstance", out JsonElement sourceInstance))
                {
                    sourceInstanceId = sourceInstance.GetInt32();
                }
            }

            if (logEvent.TryGetProperty("targetInstance", out JsonElement targetInstance))
            {
                targetInstanceId = targetInstance.GetInt32();
            }

            var sourceUnitId = new UnitId(sourceTypeId, sourceInstanceId);
            var targetUnitId = new UnitId(targetTypeId, targetInstanceId);

            if (logEvent.GetProperty("type").GetString() == "summon")
            {
                user.SummonIds.Add(targetTypeId);
            }

            if (user.SummonIds.Contains(sourceTypeId) || sourceTypeId == user.Id.TypeId)
            {
                evt.UserSuperSource = true;
            }

            AddNpcUnit(sourceUnitId, allUnits);
            AddNpcUnit(targetUnitId, allUnits);

            evt.SourceUnit = allUnits.Get(sourceUnitId);
            evt.TargetUnit = allUnits.Get(targetUnitId);
        }

        public static void SetAbility(JsonElement logEvent, Event evt, User user, Logger logger)
            //todo
        {
            var eventType = logEvent.GetProperty("type").GetString();
            var abilityData = logEvent.GetProperty("ability");


            Ability ability = null;

            if (eventType == "healabsorbed")
            {
                abilityData = logEvent.GetProperty("extraAbility");  // Ability that does the healing is under extraAbility for healabsorb events.
            }

            var abilityName = abilityData.GetProperty("name").GetString();

            if (evt is CastEvent || evt is ThroughputEvent)
            {
                if (!user.Abilities.Contains(abilityName))
                {
                    ability = AbilityFactory.Create(abilityName);

                    if (ability != null)
                    {
                        user.Abilities.Add(ability);
                    }

                    else if (evt.UserSuperSource)
                    {
                        ability = new Ability();
                        ability.Name = abilityName;
                        user.Abilities.Add(ability);
                        logger.Log(abilityName);
                    }
                    else
                    {
                        ability = new Ability();
                        ability.Name = abilityName;
                    }
                }
                else
                {
                    ability = user.Abilities.Get(abilityName);
                }
            }

            evt.Ability = ability;
            evt.AbilityName = abilityName;
            evt.AbilityId = abilityData.GetProperty("guid").GetInt32();

        }


        public static void ParseBuffEvents(JsonElement logEvent, BuffEvent buffEvent)
        {
            var eventType = logEvent.GetProperty("type").GetString();
            buffEvent.BuffStacks = logEvent.GetInt32OrDefault("stack", 1);
            if (eventType == "applybuff" || eventType == "applydebuff")
            {
                buffEvent.BuffApplyEvent = true;
                buffEvent.BuffIncEvent = true;
            }
            if (eventType == "removebuff" || eventType == "removedebuff")
            {
                buffEvent.BuffRemoveEvent = true;
            }
            if (eventType.Contains("debuff"))
            {
                buffEvent.DebuffEvent = true;
            }
            if (eventType.Contains("stack"))
            {
                buffEvent.BuffStackEvent = true;
                if (eventType == "applybuffstack")
                {
                    buffEvent.BuffIncEvent = true;
                }
            }
            if (eventType == "refreshbuff")
            {
                buffEvent.BuffRefreshEvent = true;
            }
        }

        public static void ParseCastEvents(JsonElement logEvent, CastEvent castEvent, User user)
        {
            castEvent.EmpCastLevel = logEvent.GetInt32OrDefault("empowermentLevel");
        }

        public static void ParseThroughputEvents(JsonElement logEvent, ThroughputEvent tpEvent, User user)
        {
            var eventType = logEvent.GetProperty("type").GetString();

            var naeff = logEvent.GetProperty("amount").GetInt32();
            var absorbed = logEvent.GetInt32OrDefault("absorbed");
            var overheal = logEvent.GetInt32OrDefault("overheal");
            if (tpEvent is HealEvent healEvent)
            {
                if (IsAbsorbedEvent(eventType)) //Both absorb effects and fully absorbed normal healing.
                {
                    naeff = 0;
                    absorbed = logEvent.GetProperty("amount").GetInt32();

                    healEvent.FullyAbsorbed = true;
                    if (eventType == "absorbed")  //Only true absorb effects.
                    {
                        healEvent.AbsorbAbility = true;
                    }
                    else // Non absorb effects, fully absorbed healing.
                    {
                    }
                }
            }
            // Need to have this down here since naeff and absorbed are reverse for absorbs/heal absorbs.
            var naraw = naeff + overheal;

            tpEvent.Amount.Eff = naeff + absorbed;
            tpEvent.Amount.Raw = naraw + absorbed;
            tpEvent.Amount.Naeff = naeff;
            tpEvent.Amount.Naraw = naraw;

            tpEvent.Tick = logEvent.TryGetProperty("tick", out var tick) && tick.GetBoolean();
            tpEvent.Crit = logEvent.TryGetProperty("hitType", out var hitType) && hitType.GetInt32() == 2;
            tpEvent.Aoe = logEvent.TryGetProperty("isAoE", out var aoe) && aoe.GetBoolean();

        }

        public static void ParseEventTypes(JsonElement logEvent, Event evt, User user)
        {
            var abilityData = logEvent.GetProperty("ability");
            var eventType = logEvent.GetProperty("type").GetString();

            if (evt is ThroughputEvent tpEvent)
            {
                ParseThroughputEvents(logEvent, tpEvent, user);
            }
            else if (evt is CastEvent castEvent)
            {
                ParseCastEvents(logEvent, castEvent, user);
            }
            else if (evt is BuffEvent buffEvent)
            {
                ParseBuffEvents(logEvent, buffEvent);
            }

        }

        public static Event CreateEvent(JsonElement logEvent)
        {
            var eventType = logEvent.GetProperty("type").GetString()!;
            if (IsHealEvent(eventType))
            {
                return new HealEvent();
                
            }
            else if (IsDamageEvent(eventType))
            {
                return new DamageEvent();
            }
            else if (IsCastEvent(eventType))
            {
                return new CastEvent();
            }
            else if (IsBuffEvent(eventType))
            {
                return new BuffEvent();
            }
            else
            {
                return new Event();
            }
        }



        public static void SetAbilityData(Event evt, User user)
        {
            var ability = evt.Ability;
            if (evt is CastEvent)
            {

                if (evt.SourceUnit == user)
                {
                    ability.Casts += 1;
                }
            }
            if (evt is ThroughputEvent tpEvent && evt.UserSuperSource)
            {
                if (tpEvent is HealEvent)
                {
                    ability.Heal.Eff += tpEvent.Amount.Eff;
                    ability.Heal.Raw += tpEvent.Amount.Raw;
                    ability.Heal.Count += 1;


                    if (tpEvent.SourceUnit == user)
                    {
                        ability.Heal.NonSummon.Eff += tpEvent.Amount.Eff;
                        ability.Heal.NonSummon.Raw += tpEvent.Amount.Raw;
                        ability.Heal.NonSummon.Count += 1;

                        if (tpEvent.TargetUnit is not User)
                        {
                            ability.Heal.Nsnsna.Eff += tpEvent.Amount.Naeff;
                            ability.Heal.Nsnsna.Raw += tpEvent.Amount.Naraw;
                            ability.Heal.Nsnsna.Count += 1;
                        }
                    }
                    if (tpEvent.Crit)
                    {
                        ability.Heal.Crit.Eff += tpEvent.Amount.Eff;
                        ability.Heal.Crit.Raw += tpEvent.Amount.Raw;
                        ability.Heal.Crit.Count += 1;
                    }
                    else
                    {
                        ability.Heal.Hit.Eff += tpEvent.Amount.Eff;
                        ability.Heal.Hit.Raw += tpEvent.Amount.Raw;
                        ability.Heal.Hit.Count += 1;
                    }
                }
                else if (evt.TargetUnit != user && evt is DamageEvent)  // Filtering out self damage.
                {
                    ability.Damage.Dmg += tpEvent.Amount.Eff;
                    ability.Damage.Count += 1;
                    if (tpEvent.Crit)
                    {
                        ability.Damage.Crit.Dmg += tpEvent.Amount.Eff;
                        ability.Damage.Crit.Count += 1;
                    }
                    else
                    {
                        ability.Damage.Hit.Dmg += tpEvent.Amount.Eff;
                        ability.Damage.Hit.Count += 1;
                    }
                    if (tpEvent.SourceUnit is User)
                    {
                        ability.Damage.NonSummon.Dmg += tpEvent.Amount.Naeff;
                        ability.Damage.NonSummon.Count += 1;

                        ability.Damage.Nsnsna.Dmg += tpEvent.Amount.Naeff;
                        ability.Damage.Nsnsna.Count += 1;
                    }
                }
            }
        }

        public static void AdjustEvent(Event evt)
        {
            if (evt is ThroughputEvent tpEvent && tpEvent.Ability.ForceTick)
            {
                tpEvent.Tick = true;
            }

        }

        public static void TrackHp(JsonElement logEvent, Event evt)
        {
            if (evt is ThroughputEvent)
            {
                if (logEvent.TryGetProperty("hitPoints", out _))
            {
                var sourceUnit = evt.SourceUnit;
                var targetUnit = evt.TargetUnit;
                var hp = logEvent.GetProperty("hitPoints").GetInt64();
                var maxHp = logEvent.GetProperty("maxHitPoints").GetInt64();
              
                    evt.TargetHp = hp;
                    evt.TargetMaxHp = maxHp;
                    targetUnit.Hp = hp;
                    targetUnit.MaxHp = maxHp;
                    if (sourceUnit == targetUnit)
                    {
                        evt.SourceHp = hp;
                        evt.SourceMaxHp = maxHp;
                        sourceUnit.Hp = hp;
                        sourceUnit.MaxHp = maxHp;
                    }

                }
            }
               
        }

        public static void SetHp(Event evt, User user)
        {
            var sourceUnit = evt.SourceUnit;
            var targetUnit = evt.TargetUnit;
            if (evt is ThroughputEvent)
            {
                if (evt.TargetHp != null)
                {
                    targetUnit.Hp = evt.TargetHp;
                    targetUnit.MaxHp = evt.TargetMaxHp;
                }
               
                if (sourceUnit == targetUnit && evt.SourceHp != null)
                {
                    sourceUnit.Hp = evt.SourceHp;
                    sourceUnit.MaxHp = evt.SourceMaxHp;
                }
            }
            evt.SourceHp = sourceUnit.Hp;
            evt.SourceMaxHp = sourceUnit.MaxHp;
            evt.TargetHp = targetUnit.Hp;
            evt.TargetMaxHp = targetUnit.MaxHp;
            evt.UserHp = user.Hp;
            evt.UserHp = user.MaxHp;
        }

        public static void ParseCoords(JsonElement logEvent, Event evt)
        // If throughput event coords are for target
        // If cast event coords are for source. Assume same for resourcechange but sourceid == target id always.
        {
            if (logEvent.TryGetProperty("x", out _))
            {
                var x = logEvent.GetProperty("x").GetInt32() / 100.0;
                var y = logEvent.GetProperty("y").GetInt32() / 100.0;
                if (evt is ThroughputEvent)
                {
                    evt.TargetCoords = new Coord(x, y);
                }
                else if (evt is CastEvent)
                {
                    evt.SourceCoords = new Coord(x, y);
                }
                //if (evt is ThroughputEvent)
                //{
                //    evt.TargetUnit.Coords = new Coord(x, y);
                //}
                //else if (evt is CastEvent)
                //{
                //    evt.SourceUnit.Coords = new Coord(x, y);
                //}
                //evt.TargetCoords = evt.TargetUnit.Coords;
                //evt.SourceCoords = evt.SourceUnit.Coords;

            }
        }
        public static void SetCoords(Event evt, User user)
        {
            var sourceUnit = evt.SourceUnit;
            var targetUnit = evt.TargetUnit;
            if (evt.TargetCoords != null)
            {
                targetUnit.Coords = evt.TargetCoords;
            }
            if (evt.SourceCoords != null)
            {
                sourceUnit.Coords = evt.SourceCoords;
            }
            evt.SourceCoords = sourceUnit.Coords;
            evt.TargetCoords = targetUnit.Coords;
            evt.UserCoords = user.Coords;
        }

        public static void EraseData(UnitRepo allUnits)
            // To avoid accidental usage.
        {
            foreach (var unit in allUnits)
            {
                //unit.Coords = null;
                unit.Hp = null;
                unit.MaxHp = null;
            }
        }

        public static List<Event> ParseUserEvents(JsonElement userEvents, UnitRepo allUnits, Fight fight)
        {
            var events = new List<Event>();
            var startLogTime = userEvents[0].GetProperty("timestamp").GetInt32();
            var user = allUnits.GetUser();
            var miaAbilityLogger = new Logger("Uncategorized Abilities", fight, user.Id.TypeId);
            foreach (var logEvent in userEvents.EnumerateArray())
            {
                if (SkipEvent(logEvent))
                {
                    continue;
                }
                else
                {
                    var evt = CreateEvent(logEvent);
                    var timestamp = Utils.ConvertLogTime(logEvent.GetProperty("timestamp").GetInt32(), startLogTime);
                    evt.Timestamp = timestamp;
                    ParseEventTypes(logEvent, evt, user);
                    AdjustUnitIds(logEvent, evt, allUnits);
                    SetAbility(logEvent, evt, user, miaAbilityLogger);
                    AdjustEvent(evt);
                    ParseCoords(logEvent, evt);
                    SetAbilityData(evt, user);
                    TrackHp(logEvent, evt);
                    events.Add(evt);
                }
            }
            //Reverse loop
            for (int i = events.Count - 1; i >= 0; i--)
            {
                var evt = events[i];
                SetHp(evt, user);
                //SetCoords(evt, user);

            }
            EraseData(allUnits);
            return events;
        }
    }
}
