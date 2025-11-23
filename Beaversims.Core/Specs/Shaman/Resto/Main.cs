
using Beaversims.Core.Common;
using Beaversims.Core.Shared;
using Beaversims.Core.Sim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Beaversims.Core.Specs.Shaman.Resto
{
    internal class Main

    {
        private static void SpecInit(User user)
        {

        }

        public static void SpecMain(List<Event> events, UnitRepo allUnits, Fight fight, int iterationCount)
        {


            var user = allUnits.GetUser();
            var prepullRefStats = user.RefStats.Clone();
            SpecInit(user);

            var statLogger = new Logger("StatTracker", fight, user.Id.TypeId);
            var refStatLogger = new Logger("Ref Stat Tracker", fight, user.Id.TypeId);

            foreach (Event evt in events)
            {
                // Loop for tracking buffs and collecting data.
                BuffTracker.TrackBuffs(evt, allUnits, statLogger, refStatLogger);
                evt.CreateAltEvents(user, evt);
                CastProcessor.ProcessCast(evt, user);
             



                if (evt is TpEvent tEvt)
                {
                    ProcessEvents.OriginalTotals(tEvt, user);
                    Shared.DupliEffects.SharedHypo(tEvt, user);
                    if (evt is HealEvent hEvt)
                    {
                        MasteryTracker.SetMasteryEff(hEvt, user);
                    }
                    if (evt is DamageEvent dEvt)
                    {
                    }

                }
            }
            Utils.CleanUp(allUnits, events); // To avoid accidental usage + remove events we no longer need.
            HCGM.ModifyHCGM(user, fight);
            Shared.CIM.SetCIM(user);
            Utils.RemoveImpurities(events, user);




            var degree = Environment.ProcessorCount;
            Parallel.For(0, user.AltGearSets.Count, new ParallelOptions { MaxDegreeOfParallelism = degree }, i =>
            {


                var altGearSet = user.AltGearSets[i];
                var altEventList = new List<Event>(events);
                altGearSet.AltEventList = altEventList;
                altGearSet.ProcEvents = Sim.EffectsSim.SimEffects(altEventList, user, fight, i, iterationCount);

                List<TpEvent> tpEvents = altGearSet.AltEventList.OfType<TpEvent>().Where(e => e.SimEvent == false).ToList();
                //Making sure we don't include on use dmg/heal events from sims.
                foreach (TpEvent evt in tpEvents)
                {
                    // Loop for setting gains.
                    var altEvent = evt.AltEvents[i];
                    evt.AltEvents[i].Amount = evt.Amount.Clone();
                    StatGains.AutoStatGains(evt, user, i);
                    if (evt.IsHealDoneEvent() && evt is HealEvent hEvt)
                    {
                        if (evt.Ability.ScalesWith(StatName.Mastery))
                        {
                            MasteryTracker.MasteryGains((HealEvent)evt, user, i);
                        }
                    }
                }


                // Only summer and leech will react to this.
                tpEvents = altGearSet.AltEventList.OfType<TpEvent>().ToList(); // Updating here to include non proc sim events
                Shared.ProcessEvents.AddProcEvents(tpEvents, altGearSet.ProcEvents, iterationCount, user); 
                Shared.DupliEffects.AltSummerSource(tpEvents, user, i);
                Shared.DupliEffects.AltLeechSource(tpEvents, user, i);


                Shared.ProcessEvents.StoreTotals(tpEvents, user, i, fight);
                
            });
        }
    }
}
