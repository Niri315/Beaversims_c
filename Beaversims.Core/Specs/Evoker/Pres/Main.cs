
using Beaversims.Core.Common;
using Beaversims.Core.Shared;
using Beaversims.Core.Sim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace Beaversims.Core.Specs.Evoker.Pres
{
    internal class Main

    {
        private static void SpecInit(User user)
        {
     
            if (user.HasTalent(Talents.NaturalConvergence.id))
            {
                var disintegrate = user.Abilities.Get(Abilities.Disintegrate.name);
                disintegrate.CastTime *= 1 - Talents.NaturalConvergence.coef;
            }
            if (user.HasTalent(Talents.TimelessMagic.id))
            {
                var tm = (Talents.TimelessMagic)user.Talents[Talents.TimelessMagic.id];
                var rev = user.Abilities.Get(Abilities.Reversion.name);
                var revEcho = user.Abilities.Get(Abilities.ReversionEcho.name);
                rev.Duration *= 1 + tm.Coef;
                revEcho.Duration *= 1 + tm.Coef;
                Console.WriteLine($"Rev Duration: {rev.Duration} revEcho dur: {revEcho.Duration}");
            }
        }

        public static void SpecMain(List<Event> events, UnitRepo allUnits, Fight fight, int iterationCount)
        {

            var user = allUnits.GetUser();
            var prepullRefStats = user.RefStats.Clone();
            SpecInit(user);
      
            var statLogger = new Logger("StatTracker", fight, user.Id.TypeId);
            var refStatLogger = new Logger("Ref Stat Tracker", fight, user.Id.TypeId);

            foreach (var buff in user.Buffs)
            {
                if (buff is StatBuff buffStat)
                {
                    Console.WriteLine(buffStat.Name);
                    foreach (var mod in buffStat.StatMods)
                    {
                        Console.WriteLine($"\t {mod.StatName} - {mod.AmountType} - {mod.Amount}");
                    }
                }
            }

      

            foreach (Event evt in events)
            {
                // Loop for tracking buffs and collecting data.
                BuffTracker.TrackBuffs(evt, allUnits, statLogger, refStatLogger);
                evt.CreateAltEvents(user, evt);
                CastProcessor.ProcessCast(evt, user);
                MasteryTracker.SetMasteryEff(evt, user);
                HCGM.GatherData(evt, user);
                FullAllocs.TrackReversions(evt, user);
                if (evt is TpEvent tEvt)
                {
                    ProcessEvents.OriginalTotals(tEvt, user);
                    Shared.DupliEffects.SharedHypo(tEvt, user);
                    DupliEffects.EnkindleHypo(tEvt, user);
                    if (evt is HealEvent hEvt)
                    {
                        DupliEffects.LifebindHypo(hEvt, user);
                    }
                }
            }


            //var testEvt = events[100];
            //Console.WriteLine("--------");
            //var originalInt = testEvt.UserStats.Get(StatName.Intellect);
            //var altInt = testEvt.AltEvents[0].UserStats.Get(StatName.Intellect);
            //Console.WriteLine($"{originalInt.TrueEff()} vs {altInt.TrueEff()}");
            //Console.WriteLine($"Mastery Test: {user.MasteryTest1} vs {user.MasteryTest2} -> {(double)user.MasteryTest1 / (user.MasteryTest2)}");
            Utils.CleanUp(allUnits, events); // To avoid accidental usage.
            HCGM.ModifyCIMyHCGM(user, fight);
            Shared.CIM.SetCIM(user);
            Utils.RemoveImpurities(events, user);

            //foreach (var ability in user.Abilities)
            //{
            //    Console.WriteLine($"{ability.Name}: CIM: {ability.CIM}, True QIM: {ability.TrueQIM(user, 0)}, Rest rel Ratio: {ability.RestRelCIMRatio}, CTG: {ability.CTGain}");
            //}

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
                FullAllocs.FullAllocCalcs(altEventList, user);

                DupliEffects.AltEnkindle(tpEvents, user, i);
                DupliEffects.AltLifebind(tpEvents, user, i);

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
