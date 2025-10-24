using Beaversims.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Sim
{
    internal class EffectsSim
    {
        //public static void ResetStatInc(Event evt)
        //{
        //    for (int x = 0; x < evt.AltEvents.Count; x++)
        //    {
        //        foreach (var stat in )
        //    }
        //}

        public static void AddStatInc(Event evt, User user)
        {

            for (int x = 0; x < user.AltGearSets.Count; x++)
            {
                var altEvent = evt.AltEvents[x];
                var gearSet = user.AltGearSets[x];
                foreach (var stat in gearSet.SimIncRatings)
                {
                    if (stat.Value == 0) { continue; }

                    var statName = stat.Key;
                    var altStat = altEvent.UserStats.Get(statName);
                    var effDiff = altStat.GetEffDiff(stat.Value, false);
                    //evt.AltEvents[x].SimStatEffInc[stat.Key] += effDiff;
                    // Dangerous, need to change how we deal with eff stat for altevents infor the stat gain iteration.
                    altStat.SimExtraEff += effDiff / Constants.iterationCount;
                }
            }
            //foreach (var simEffect in user.SimEffects)
            //{
            //    foreach (var entry in simEffect.RatingIncTracker)
            //    {
            //        var gearSetId = entry.Key;
            //        var altEvent = evt.AltEvents[gearSetId];
            //        var statDict = entry.Value;
            //        foreach (var stat in statDict)
            //        {
            //            var statName = stat.Key;
            //            var altStat = altEvent.UserStats.Get(statName);
            //            var effDiff = altStat.GetEffDiff(stat.Value, false);
            //            //Console.WriteLine(stat.Value);
            //            altEvent.SimStatEffInc[statName] += effDiff;
            //        }

            //    }
            //}
        }

        //public static void ApplyStatDiffs(List<Event> events, User user)
        //{
        //    foreach (var evt in events)
        //    {
        //        for (int x = 0; x < evt.AltEvents.Count; x++)
        //        {
        //            var altEvent = evt.AltEvents[x];
        //            var gearSet = user.AltGearSets[x];

        //            foreach (var stat in altEvent.SimStatEffInc)
        //            {
        //                var altStat = evt.AltEvents[x].UserStats.Get(stat.Key);
        //                // Dangerous, need to change how we deal with eff stat for altevents infor the stat gain iteration.
        //                altStat.Eff += stat.Value / Constants.iterationCount;  
        //            }
        //        }
        //        //for (int x = 0; x < evt.AltEvents.Count; x++)
        //        //{
        //        //    var altEvent = evt.AltEvents[x];
        //        //    var gearSet = user.AltGearSets[x];

        //        //    foreach (var stat in altEvent.SimStatEffInc)
        //        //    {
        //        //        var altStat = evt.AltEvents[x].UserStats.Get(stat.Key);
        //        //        altStat.Eff += stat.Value / Constants.iterationCount;
        //        //    }
        //        //}
        //    }
        //}

        public static void ProcEffects(List<Event> events, User user, Fight fight)
        {
            foreach (var specialEffect in user.AllEffects)
            {
                specialEffect.Init(events, user, fight);
            }
          
            for (int y = 0; y < Constants.iterationCount; y++)
            {

                foreach (var specialEffect in user.NonHasteProcEffects)
                {
                    specialEffect.Reset(user, fight);
                }

                for (int i = 0; i < user.AltGearSets.Count; i++)
                {
                    foreach (var hasteEffect in user.AltGearSets[i].HasteProcEffects)
                    {
                        hasteEffect.Reset(user, fight);
                    }
                }

                foreach (var evt in events)
                {
                    foreach (var specialEffect in user.NonHasteProcEffects)
                    {
                        specialEffect.Call(evt, user);
                    }
                    AddStatInc(evt, user);
                    for (int i = 0; i < user.AltGearSets.Count; i++)
                    {
                        foreach (var hasteEffect in user.AltGearSets[i].HasteProcEffects)
                        {
                            hasteEffect.Call(evt, user, i);
                        }
                    }

                }
            }
            //ApplyStatDiffs(events, user);
        }
     

        public static void SimEffects(List<Event> events, User user, Fight fight)
        {
            if (Constants.swOption || Constants.deactivateSims)
            {
                return;
            }

            ProcEffects(events, user, fight);
        }
    }
}
