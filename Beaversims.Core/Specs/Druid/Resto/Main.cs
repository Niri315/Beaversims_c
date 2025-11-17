
using Beaversims.Core.Common;
using Beaversims.Core.Shared;
using Beaversims.Core.Sim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace Beaversims.Core.Specs.Druid.Resto
{
    internal class Main

    {
    
        public static void TrackRegrowth(Event evt)
        {
            if (evt.TargetUnit.HasBuff(Abilities.Regrowth.buffId))
            {
                evt.TargetHasRegrowth = true;
            }
        }
        public static void TrackAbundance(Event evt, User user)
        {
            var abundance = user.GetBuff(Talents.Abundance.buffId);
            if (abundance != null)
            {
                evt.AbundanceStacks = abundance.Stacks;
            }
        }

        public static void RegrowthCritGains(TpEvent evt, User user, int i)
        {
            if (evt.AbilityName == Abilities.Regrowth.name)
            {
                var crit = (Crit)evt.UserStats.Get(StatName.Crit);
                var critChance = crit.TrueEff() / (crit.PercentRate * 100);
                if (evt.Tick && user.HasTalent(Talents.StrategicInfusion.id))
                {
                    critChance += Talents.StrategicInfusion.coef;
                }
                else
                {
                    if (evt.TargetHasRegrowth && user.HasTalent(Talents.ImprovedRegrowth.id))
                    {
                        critChance += Talents.ImprovedRegrowth.coef;
                    }
                }
                critChance += Math.Min(evt.AbundanceStacks * Talents.Abundance.coef, Talents.Abundance.cap);
                if (critChance < 1.0)
                {
                    StatGains.CritGainsHeal((HealEvent)evt, user, i);
                }
                else
                {
                    //Log
                }

            }
        }



        private static void SpecInit(User user)
        {
            // Lycara doesnt show up as init buff. If assumption is wrong, it will be corrected as soon as form is changed.
            user.AddBuff(Data.StatBuffs.LycarasTeachingsNoForm.name, Data.StatBuffs.LycarasTeachingsNoForm.id, user, 1, 0.0);
        }

        public static void SpecMain(List<Event> events, UnitRepo allUnits, Fight fight, int iterationCount)
        {


            //StatGainMathTest();
            //return;
            var user = allUnits.GetUser();
            var prepullRefStats = user.RefStats.Clone();
            SpecInit(user);
            MasteryTracker.InitHarmonyBuffs(allUnits, user);
      
            var statLogger = new Logger("StatTracker", fight, user.Id.TypeId);
            var refStatLogger = new Logger("Ref Stat Tracker", fight, user.Id.TypeId);

            foreach (Event evt in events)
            {
                // Loop for tracking buffs and collecting data.
                BuffTracker.TrackBuffs(evt, allUnits, statLogger, refStatLogger);
                evt.CreateAltEvents(user, evt);
                CastProcessor.ProcessCast(evt, user);
                MasteryTracker.SetMasteryEff(evt, user);
                TrackRegrowth(evt);
                TrackAbundance(evt, user);
                HCGM.GatherData(evt, user);

                if (evt is TpEvent tEvt)
                {
                    ProcessEvents.OriginalTotals(tEvt, user);
                    Shared.DupliEffects.SharedHypo(tEvt, user);
                }
            }
            Utils.CleanUp(allUnits); // To avoid accidental usage.
            Shared.CIM.SetCIM(user);
            HCGM.ModifyHCGM(user, fight);
            Utils.RemoveImpurities(events, user);



            foreach (var ability in user.Abilities)
            {
                Console.WriteLine($"{ability.Name}: CIM: {ability.CIM}, Rest rel Ratio: {ability.RestRelCIMRatio}, CTG: {ability.CTGain}");
            }

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
                        RegrowthCritGains(evt, user, i);
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

                foreach (TpEvent evt in tpEvents)
                {
                    Shared.ProcessEvents.StoreTotals(evt, user, i);
                }
            });
        }
    }
}
