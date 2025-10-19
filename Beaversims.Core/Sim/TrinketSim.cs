using Beaversims.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Sim
{
    internal class TrinketSim
    {
        public static void AddStatInc(Event evt, User user)
        {
            for (int x = 0; x < user.AltGearSets.Count; x++)
            {
                var gearSet = user.AltGearSets[x];
                foreach (var stat in gearSet.IncRatings)
                {
                    if (stat.Value == 0) { continue; }
                    var statName = stat.Key;
                    var altEvent = evt.AltEvents[x];
                    var altStat = altEvent.UserStats.Get(statName);
                    var effDiff = altStat.GetEffDiff(stat.Value, false);
                    evt.AltEvents[x].SimStatEffInc[stat.Key] += effDiff;

                }
            }
            //foreach (var simEffect in user.SimEffects)
            //{
            //    foreach (var entry in simEffect.RatingInc)
            //    {
            //        var gearSetId = entry.Key;
            //        var altEvent = evt.AltEvents[gearSetId];
            //        var statDict = entry.Value;
            //        foreach (var stat in statDict)
            //        {
            //            var statName = stat.Key;
            //            var altStat = altEvent.UserStats.Get(statName);
            //            var effDiff = altStat.GetEffDiff(stat.Value, false);
            //            altEvent.SimStatEffInc[statName] += effDiff;
            //        }

            //    }
            //}
        }

        public static void ApplyStatDiffs(List<Event> events, User user)
        {
            foreach (var evt in events)
            {
                //for (int x = 0; x < evt.AltEvents.Count; x++)
                //{
                //    var altEvent = evt.AltEvents[x];
                //    var gearSet = user.AltGearSets[x];

                //    foreach (var stat in altEvent.SimStatEffInc)
                //    {
                //        var altStat = evt.AltEvents[x].UserStats.Get(stat.Key);
                //        altStat.Eff += stat.Value / Constants.iterationCount;
                //    }
                //}
                for (int x = 0; x < evt.AltEvents.Count; x++)
                {
                    var altEvent = evt.AltEvents[x];
                    var gearSet = user.AltGearSets[x];

                    foreach (var stat in altEvent.SimStatEffInc)
                    {
                        var altStat = evt.AltEvents[x].UserStats.Get(stat.Key);
                        altStat.Eff += stat.Value / Constants.iterationCount;
                    }
                }
            }
        }

        public static void StatProcTrinkets(List<Event> events, User user, Fight fight)
        {
            if (Constants.swOption || Constants.deactivateSims)
            {
                return; 
            }
            //var degree = Environment.ProcessorCount;


            foreach (var specialEffect in user.SimEffects)
            {
                specialEffect.Init(events, user, fight);
                    
            }

            for (int y = 0; y < Constants.iterationCount; y++)
            {
                for (int x = 0; x < user.AltGearSets.Count; x++) 
                { 
                    var gearSet = user.AltGearSets[x];
                    // Resetting stat increases.
                    gearSet.IncRatings = Utils.InitStatDict();
                    gearSet.IncEffs = Utils.InitStatDict();
                }
                foreach (var specialEffect in user.SimEffects)
                {
                    specialEffect.Reset(user, fight);
                }

                foreach (var evt in events)
                {
                    foreach (var specialEffect in user.SimEffects)
                    {
                        specialEffect.Call(evt, user);
                    }
                    AddStatInc(evt, user);
                }
            }
            ApplyStatDiffs(events, user);
        }
    }
}
