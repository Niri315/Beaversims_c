using System.Text.Json;
using Beaversims.Core.Utils;

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
            if (logEvent.TryGetProperty("fake", out var fake) && fake.GetBoolean())
                return true;
            // Empowered casts have both a cast and empowerend event. Skip cast and use empowerend only.
            if (logEvent.TryGetProperty("ability", out var ability) &&
                empoweredAbilities.Contains(ability.GetProperty("name").GetString()!) &&
                logEvent.GetProperty("type").GetString() == "cast")
                return true;
            return false;
        }

        //public static Event CreateEvent(JsonElement logEvent)
        //{
        //    var eventType = logEvent.GetProperty("type").GetString();
        //    var abilityData = logEvent.GetProperty("ability");
        //    var _event = new Event;
        //    if (IsThroughputEvent(eventType))
        //    {


        //        var naeff = logEvent.GetProperty("amount").GetInt32();
        //        var absorbed = logEvent.GetInt32OrDefault("absorbed");
        //        var overheal = logEvent.GetInt32OrDefault("overheal");

        //        if (IsHealEvent(eventType))
        //        {
        //            var _event = new HealEvent;
        //            _event.Type = EventType.Heal;
        //            if (IsAbsorbedEvent(eventType))
        //            {
        //                naeff = 0;
        //                absorbed = logEvent.GetProperty("amount").GetInt32();

        //                _event.FullyAbsorbed = true;
        //                if (eventType == "absorbed")
        //                {
        //                    _event.AbsorbAbility = true;
        //                }
        //                else //healabsorbed
        //                {
        //                    abilityData = logEvent.GetProperty("extraAbility");  // Ability that does the healing is under extraAbility for healabsorb events.
        //                }
        //            }
        //        }
        //        else  // Is damage event.
        //        {
        //            var _event = new DamageEvent;
        //            _event.Type = EventType.Damage;
        //        }

        //        // Need to have this down here since naeff and absorbed are reverse for heal absorbs.
        //        var naraw = naeff + overheal;

        //        _event.AmountEff = naeff + absorbed;
        //        _event.AmountRaw = naraw + absorbed;
        //        _event.AmountAbsorb = absorbed;
        //        _event.AmountOverheal = overheal;
        //        _event.AmountNaeff = naeff;
        //        _event.AmountNaraw = naraw;

        //        _event.Tick = logEvent.TryGetProperty("tick", out var tick) && tick.GetBoolean();
        //        _event.Crit = logEvent.TryGetProperty("hitType", out var hitType) && hitType.GetInt32() == 2;
        //        _event.Aoe = logEvent.TryGetProperty("isAoE", out var aoe) && aoe.GetBoolean();

        //    }
        //    else if (IsCastEvent(eventType))
        //    {
        //        _event.Type = EventType.Cast;
        //        _event.EmpCastLevel = logEvent.GetInt32OrDefault("empowermentLevel");

        //    }
        //    else if (IsBuffEvent(eventType))
        //    {
        //        _event.Type = EventType.Buff;
        //        _event.BuffStacks = logEvent.GetInt32OrDefault("stack", 1);
        //        if (eventType == "applybuff" || eventType == "applydebuff")
        //        {
        //            _event.BuffApplyEvent = true;
        //            _event.BuffIncEvent = true;
        //        }
        //        if (eventType == "removebuff" || eventType == "removedebuff")
        //        {
        //            _event.BuffRemoveEvent = true;
        //        }
        //        if (eventType.Contains("debuff"))
        //        {
        //            _event.DebuffEvent = true;
        //        }
        //        if (eventType.Contains("stack"))
        //        {
        //            _event.BuffStackEvent = true;
        //            if (eventType == "applybuffstack")
        //            {
        //                _event.BuffIncEvent = true;
        //            }
        //        }
        //        if (eventType == "refreshbuff")
        //        {
        //            _event.BuffRefreshEvent = true;
        //        }
        //    }
        //    return _event;
        //

        public static Event CreateEvent(JsonElement logEvent)
        {
            var eventType = logEvent.GetProperty("type").GetString();

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



        public static List<Event> ParseUserEvents(JsonElement userEvents)
        {
            var events = new List<Event>();
            var startLogTime = userEvents[0].GetProperty("timestamp").GetInt32();

            foreach (var logEvent in userEvents.EnumerateArray())
            {
                if (SkipEvent(logEvent))
                {
                    continue;
                }
                else
                {
                    var _event = new Event();

                    _event = CreateEvent(logEvent);
                    _event.timestamp = TimeUtils.ConvertLogTime(logEvent.GetProperty("timestamp").GetInt32(), startLogTime);

                    events.Add(_event);
                }

            }

            return events;
        }
        public static void PrintReportKeys(string json)
        {
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("data", out var data) &&
                data.TryGetProperty("reportData", out var reportData) &&
                reportData.TryGetProperty("report", out var report) &&
                report.ValueKind == JsonValueKind.Object)
            {
                Console.WriteLine("Keys in 'report':");
                foreach (var prop in report.EnumerateObject())
                {
                    Console.WriteLine($"- {prop.Name}");
                }
            }
            else
            {
                Console.WriteLine("Could not find 'report' in the JSON.");
            }
        }
    }
}
