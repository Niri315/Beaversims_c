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

        public static void AddStatInc(Event evt, GearSet gearSet, StatTracker curAltStats, int iterationCount)
        {

            foreach (var stat in gearSet.SimIncRatings)
            {
                if (stat.Value == 0) { continue; }

                var statName = stat.Key;
                var curAltStat = curAltStats.Get(statName);
                var effDiff = curAltStat.GetEffDiff(stat.Value, false);

                //evt.AltEvents[x].SimStatEffInc[stat.Key] += effDiff;
                // Dangerous, need to change how we deal with eff stat for altevents infor the stat gain iteration.
                curAltStat.SimExtraEff += effDiff / iterationCount;
                curAltStat.TempExtraEff = effDiff;
            }
        }


     

        public static List<TpEvent>? SimEffects(List<Event> events, User user, Fight fight, int i, int iterationCount)
        {
            if (user.SwMode || Constants.deactivateSims)
            {
                return null;
            }

            var gearSet = user.AltGearSets[i];
            List<TpEvent> procEvents = [];
            var usePosEvents = new List<Event>(events);  // To find use timings
            var useEffects = gearSet.OnUseEffects.OrderBy(e => e.Priority).ToList();
            foreach (var specialEffect in useEffects)
            {
                usePosEvents = specialEffect.FindUseTimings(usePosEvents, user, fight, i);
            }
            foreach (var specialEffect in useEffects)
            {
                specialEffect.Run(events, user, fight, i);
            }
            foreach (var specialEffect in gearSet.ProcEffects)
            {
                specialEffect.Init(events, user, fight, i);
            }

            for (int y = 0; y < iterationCount; y++)
            {
                List<Event> tempEvents = new List<Event>(events);
                gearSet.ResetProcEffects();
                StatTracker curAltStats = events[0].AltEvents[gearSet.Id].UserStats;
                for (int e = 0; e < tempEvents.Count; e++)
                {
                    var evt = tempEvents[e];
                    if (!evt.Proc)
                    {
                        curAltStats = evt.AltEvents[gearSet.Id].UserStats;
                    }
                    AddStatInc(evt, gearSet, curAltStats, iterationCount);
                    foreach (var specialEffect in gearSet.ProcEffects)
                    {
                        specialEffect.Call(procEvents, tempEvents, evt, user, curAltStats, i, iterationCount);
                    }
                    if (evt.Proc && evt is TpEvent tEvt)
                    {
                        //Console.WriteLine(evt.Timestamp.ToString());
                        tEvt.UserStats = curAltStats;
                        procEvents.Add(tEvt);
                        // Remove the proc event here
                    }
                }
            }

            return procEvents;
        }
    }
}
