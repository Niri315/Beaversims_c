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


        public static void CorrectAmounts(TpEvent evt)
        {
            var ability = evt.Ability;
            var statMods = 1.0;
            if (ability.ScalesWith(StatName.Vers)) {
                statMods *= SimUtils.VersMod(evt.UserStats);
            }
            if (ability.ScalesWith(StatName.Crit))
            {
                statMods *= SimUtils.CritMod(evt.UserStats);
            }
            evt.Amount.Raw *= statMods * Constants.defaultHealIncMod;
            evt.Amount.Eff *= statMods * Constants.defaultHealIncMod;
            evt.Amount.Naraw *= statMods * Constants.defaultHealIncMod;
            evt.Amount.Naeff *= statMods * Constants.defaultHealIncMod;
        }

        public static List<TpEvent>? SimEffects(List<Event> events, User user, Fight fight, int i, int iterationCount)
        {
            var simMode = user.SimMode;
            if (simMode == SimMode.SW || simMode == SimMode.StatAlloc || Constants.deactivateSims)
            {
                return null;
            }
            //Utils.AddHeartbeatEvents(events);
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
                StatTracker curAltStats = tempEvents[0].AltEvents[gearSet.Id].UserStats;
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
            var simEvents = new List<Event>();
            simEvents.AddRange(events.Where(e => e.SimEvent));
            simEvents.AddRange(procEvents.Where(e => e.SimEvent));

            for (int e = 0; e < simEvents.Count; e++)
            {
                var evt = simEvents[e];
                CorrectAmounts((TpEvent)evt);
            }

            return procEvents;
        }
    }
}

//using Beaversims.Core.Common;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Beaversims.Core.Sim
//{
//    internal class EffectsSim
//    {
//        //public static void ResetStatInc(Event evt)
//        //{
//        //    for (int x = 0; x < evt.AltEvents.Count; x++)
//        //    {
//        //        foreach (var stat in )
//        //    }
//        //}

//        public static void AddStatInc(Event evt, GearSet gearSet, StatTracker curAltStats, int iterationCount, int i)
//        {

//            foreach (var stat in gearSet.SimIncRatings)
//            {
//                if (stat.Value == 0) { continue; }

//                var statName = stat.Key;
//                var curAltStat = curAltStats.Get(statName);
//                var effDiff = curAltStat.GetEffDiff(stat.Value, false);

//                //evt.AltEvents[x].SimStatEffInc[stat.Key] += effDiff;
//                // Dangerous, need to change how we deal with eff stat for altevents infor the stat gain iteration.
//                //curAltStat.SimExtraEff += effDiff / iterationCount;
//                evt.AltEvents[i].UserStats.Get(statName).SimExtraEff += effDiff / iterationCount;
//                curAltStat.TempExtraEff = effDiff;
//            }
//        }




//        public static List<TpEvent>? SimEffects(List<Event> events, User user, Fight fight, int i, int iterationCount, StatTracker prepullRefStats)
//        {
//            var simMode = user.SimMode;
//            if (simMode == SimMode.SW || simMode == SimMode.StatAlloc || Constants.deactivateSims)
//            {
//                return null;
//            }


//            var gearSet = user.AltGearSets[i];




//            List<TpEvent> procEvents = [];
//            var usePosEvents = new List<Event>(events);  // To find use timings
//            var useEffects = gearSet.OnUseEffects.OrderBy(e => e.Priority).ToList();
//            foreach (var specialEffect in useEffects)
//            {
//                usePosEvents = specialEffect.FindUseTimings(usePosEvents, user, fight, i);
//            }
//            foreach (var specialEffect in useEffects)
//            {
//                specialEffect.Run(events, user, fight, i);
//            }
//            foreach (var specialEffect in gearSet.ProcEffects)
//            {
//                specialEffect.Init(events, user, fight, i);
//            }

//            var prepullGearSetStats = prepullRefStats.Clone();
//            var statDiffs = Enum.GetValues<StatName>().ToDictionary(stat => stat, stat => 0.0);

//            //foreach (var altGear in altGearSet)
//            //{
//            foreach (var stat in gearSet.TotalGearRatings)
//            {
//                statDiffs[stat.Key] += stat.Value;
//            }
//            //}
//            //foreach (var gear in user.Gear)
//            //{
//            foreach (var stat in user.TotalGearRatings)
//            {
//                statDiffs[stat.Key] -= stat.Value;
//            }
//            //}

//            foreach (var stat in statDiffs)
//            {
//                bool removal;
//                var diff = stat.Value;
//                if (stat.Value < 0.0)
//                {
//                    removal = true;
//                    diff *= -1;
//                }
//                else
//                {
//                    removal = false;
//                }
//                prepullGearSetStats.Get(stat.Key).ChangeAmount(diff, StatAmountType.Rating, removal);
//            }
//            prepullGearSetStats.UpdateAllStats();



//            for (int y = 0; y < iterationCount; y++)
//            {
//                List<Event> tempEvents = new List<Event>(events);
//                gearSet.ResetProcEffects();
//                //StatTracker curAltStats = events[0].AltEvents[gearSet.Id].UserStats;


//                gearSet.StatTracker = prepullGearSetStats.Clone();

//                for (int e = 0; e < tempEvents.Count; e++)
//                {
//                    var evt = tempEvents[e];

//                    //var altEvent = new AltEvent();
//                    //if (evt is TpEvent _tEvt)
//                    //{
//                    //    altEvent.Amount = _tEvt.Amount.Clone();
//                    //}


//                    if (evt is BuffEvent bEvt)
//                    {
//                        foreach (var mod in bEvt.RefStatChanges)
//                        {
//                            gearSet.StatTracker.Get(mod.StatName).ChangeAmount(mod.Amount, mod.Type, mod.Removal);
//                        }
//                    }
//                    foreach (var mod in evt.AltEvents[i].SimStatChanges)
//                    {
//                        gearSet.StatTracker.Get(mod.StatName).ChangeAmount(mod.Amount, mod.Type, mod.Removal);
//                    }

//                    if (y == 0)
//                    {
//                        //Console.WriteLine(evt.Timestamp.ToString());    
//                        evt.AltEvents[i].UserStats = gearSet.StatTracker.Clone();
//                    }

//                    //altEvent.UserStats = altStats;
//                    //evt.AltEvents.Add(altEvent);


//                    //if (!evt.Proc)
//                    //{
//                    //    curAltStats = evt.AltEvents[gearSet.Id].UserStats;
//                    //}
//                    AddStatInc(evt, gearSet, gearSet.StatTracker, iterationCount, i);


//                    foreach (var specialEffect in gearSet.ProcEffects)
//                    {
//                        specialEffect.Call(procEvents, tempEvents, evt, user, gearSet.StatTracker, i, iterationCount);
//                    }
//                    if (evt.Proc && evt is TpEvent tEvt)
//                    {
//                        //Console.WriteLine(evt.Timestamp.ToString());
//                        tEvt.UserStats = gearSet.StatTracker;
//                        procEvents.Add(tEvt);
//                    }
//                }


//            }


//            return procEvents;
//        }
//    }
//}
