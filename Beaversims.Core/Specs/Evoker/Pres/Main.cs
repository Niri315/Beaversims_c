
using Beaversims.Core.Common;
using Beaversims.Core.Shared;
using Beaversims.Core.Sim;
using Microsoft.VisualBasic.FileIO;
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
        //public static void LfmPrimGains(TpEvent evt, User user, int i)
        //{
        //    if (evt.AbilityName == Abilities.FireBreath.name) return; // TODO implement fb
        //    // Todo test how it functions with Time in Need
        //    if (Talents.LifeforceMender.abilities.Contains(evt.AbilityName)) 
        //    {

        //        var ability = evt.Ability;
        //        double spc;
        //        if (evt.IsHealDoneEvent())
        //        {
        //            spc = ability.SpcHeal;
        //        }
        //        else if (evt.IsDmgDoneEvent())
        //        {
        //            spc = ability.SpcDmg;
        //        }
        //        else
        //        {
        //            return;
        //        }
            
        //        if (user.HasTalent(Talents.LifeforceMender.id))
        //        {
        //            var lfm = (Talents.LifeforceMender)user.Talents[Talents.LifeforceMender.id];
        //            var expHpVal = lfm.Coef * evt.SourceMaxHp;
        //            var expSpVal = spc * evt.UserStats.Get(StatName.Intellect).TrueEff();
        //            var x = evt.Amount.Raw / (expHpVal + expSpVal);
        //            Console.WriteLine(evt.AbilityName);
        //            Console.WriteLine($"{expHpVal} {expSpVal} {evt.SourceMaxHp} {lfm.Coef} {evt.UserStats.Get(StatName.Intellect).TrueEff()}");
        //            Console.WriteLine(evt.Amount.Raw);
        //            Console.WriteLine(x);

        //        }
        //    }
        //}

        public static void StatGainMathTest()
        {
            var evt = new HealEvent();
            evt.Amount.Raw = 10000;
            var altEvt = new AltEvent();
            altEvt.Amount = new AmountContainer();
            altEvt.Amount.Raw = evt.Amount.Raw;
            evt.AltEvents.Add(altEvt);
            evt.Ability = new Ability();
            var altStats = new StatTracker();
            altEvt.UserStats = altStats;

            altStats.InitMastery(Specs.Paladin.Holy.HolyPaladin.masteryPr_s);
            altStats.Get(StatName.Intellect).Rating = 100000;
            altStats.Get(StatName.Vers).Rating = 780;
            altStats.Get(StatName.Mastery).Rating = 14000;
            altStats.UpdateAllStats();
        
            var intOri = new Intellect();
            intOri.Rating = 150000;
            intOri.FullUpdate();
            var oriVers = new Vers();
            oriVers.Rating = 7800;
            oriVers.FullUpdate();
            var oriMastery = new Mastery(Specs.Evoker.Pres.PreservationEvoker.masteryPr_s);
            oriMastery.Rating = 1400;
            oriMastery.FullUpdate();


            evt.MasteryActive = true;
            StatGains.PrimaryAltAmount(evt, intOri, 0);
            StatGains.SecondaryAltAmount(evt, oriVers, 0);
            StatGains.SecondaryAltAmount(evt, oriMastery, 0);
            Console.WriteLine($"Final Test Amount: {altEvt.Amount.Raw}");

        }

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
            if (user.HasTalent(Talents.DoubleTime.id))
            {
                var db = (Abilities.PresAbility)user.Abilities.Get(Abilities.DreamBreath.name);
                var dbEcho = (Abilities.PresAbility)user.Abilities.Get(Abilities.DreamBreathEcho.name);
                foreach (var ability in user.Abilities) 
                    {
                    Console.WriteLine($"{ability.Name}");
                    }
                var fb = (Abilities.PresAbility)user.Abilities.Get(Abilities.FireBreath.name);
                //var fb = (Abilities.PresAbility)user.Abilities.Get(Abilities.FireBreath.name);
                db.CritExtendAbility = true;
                dbEcho.CritExtendAbility = true;
                fb.CritExtendAbility = true;
            }
            if (user.HasTalent(Talents.FontOfMagic.id))
            {
                user.MaxEmpLevel += Talents.FontOfMagic.levelInc;
            }
        }

        public static void SpecMain(List<Event> events, UnitRepo allUnits, Fight fight, int iterationCount)
        {

            var user = allUnits.GetUser();
            var prepullRefStats = user.RefStats.Clone();
            SpecInit(user);
            StatGainMathTest();
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
                CastProcessor.ProcessCast(evt, user);
                if (evt.RemoveMe) continue;
                evt.CreateAltEvents(user, evt);
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
            events.RemoveAll(e => e.RemoveMe);

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
                    //LfmPrimGains(evt, user, i);
                    StatGains.AutoStatGains(evt, user, i);
                    if (evt.IsHealDoneEvent() && evt is HealEvent hEvt)
                    {
                        if (evt.Ability.ScalesWith(StatName.Mastery))
                        {
                            MasteryTracker.MasteryGains((HealEvent)evt, user, i);
                        }
                    }
                }
                FullAllocs.FullAllocCalcs(tpEvents, user, i);

                DupliEffects.AltEnkindle(tpEvents, user, i);
                DupliEffects.AltLifebind(tpEvents, user, i);

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
