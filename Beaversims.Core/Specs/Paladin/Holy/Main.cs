
using Beaversims.Core.Common;
using Beaversims.Core.Shared;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Beaversims.Core.Specs.Paladin.Holy
{
    internal class Main

    {
        //public static void StatGainMathTest()
        //{
        //    var evt = new HealEvent();
        //    evt.Amount.Raw = 10000;
        //    var altEvt = new AltEvent();
        //    altEvt.Amount = new AmountContainer();
        //    altEvt.Amount.Raw = evt.Amount.Raw;
        //    evt.AltEvents.Add(altEvt);
        //    evt.Ability = new Ability();
        //    var altStats = new StatTracker();
        //    altEvt.UserStats = altStats;

        //    altStats.InitMastery(Specs.Paladin.Holy.HolyPaladin.masteryPr_s);
        //    altStats.Get(StatName.Intellect).Eff = 100000;
        //    altStats.Get(StatName.Vers).Eff = 780;
        //    altStats.Get(StatName.Mastery).Eff = 14000;

        //    var intOri = new Intellect();
        //    intOri.Eff = 150000;
        //    var oriVers = new Vers();
        //    oriVers.Eff = 7800;
        //    var oriMastery = new Mastery(Specs.Paladin.Holy.HolyPaladin.masteryPr_s);
        //    oriMastery.Eff = 1400;

        //    StatGains.PrimaryAltAmount(evt, intOri, 0);
        //    StatGains.SecondaryAltAmount(evt, oriVers, 0);
        //    MasteryTracker.MasteryAltAmount(evt, oriMastery, 0, 1.0);
        //    Console.WriteLine($"Final Test Amount: {altEvt.Amount.Raw}");

        //}

        private static void SpecInit(User user)
        {
            if (user.HasTalent(Talents.Awestruck.id)) 
            {
                var awestruck = (Talents.Awestruck)user.Talents[Talents.Awestruck.id];
                awestruck.SetCritInc(user);
            }
            DupliEffects.SetBeaconCoef(user);
        }

        public static void SpecMain(List<Event> events, UnitRepo allUnits, Fight fight)
        {
            //StatGainMathTest();
            //return;
            var user = allUnits.GetUser();
            SpecInit(user);
            var holyShock = (Abilities.HolyShock)user.Abilities.Get(Abilities.HolyShock.name);
            var gj = (Abilities.GreaterJudgment)user.Abilities.Get(Abilities.GreaterJudgment.name);
            var martyr = (Abilities.LightOfTheMartyr)user.Abilities.Get(Abilities.LightOfTheMartyr.name);
            var cs = (Abilities.CrusaderStrike)user.Abilities.Get(Abilities.CrusaderStrike.name);
            var judg = (Abilities.Judgment)user.Abilities.Get(Abilities.Judgment.name);

            var beacons = MasteryTracker.FindStarterBeacons(allUnits);
            MasteryTracker.FindCoords(events);
            var statLogger = new Logger("StatTracker", fight, user.Id.TypeId);
            var refStatLogger = new Logger("Ref Stat Tracker", fight, user.Id.TypeId);

            foreach (Event evt in events)
            {
                // Loop for tracking buffs and collecting data.
                BuffTracker.TrackBuffs(evt, allUnits, statLogger, refStatLogger);
                evt.CreateAltEvents(user, evt);
          
                CastProcessor.ProcessCast(evt, user);
                MasteryTracker.TrackBeacons(evt, beacons, user);
                MasteryTracker.SetMasteryEff(evt, beacons, user);
                DupliEffects.AddBeaconPolHeal(evt);
                Awakening.TrackAwakening(evt, user);
                judg.TrackGJCritChance(evt, user);
                HCGM.TrackIoL(evt, user);
                HCGM.TrackEmpLod(evt, user);
                Misc.TrackArmaments(evt, user);
                HCGM.TrackAnshe(evt, user);

                if (evt is TpEvent tEvt)
                {
                    ProcessEvents.OriginalTotals(tEvt, user);
                    HCGM.TrackACSource(tEvt, user);
                    Shared.DupliEffects.SharedHypo(tEvt, user);
                    if (tEvt.IsHealDoneEvent() && evt is HealEvent hEvt)
                    {
                        DupliEffects.SelflessHypo(hEvt, user);
                        DupliEffects.BeaconHypo(hEvt, user);
                    }
                }
            }
            Utils.CleanUp(allUnits); // To avoid accidental usage.
            MasteryTracker.CleanUpCoords(allUnits);

            HCGM.ModifyCIMSources(user, fight); // 
            Shared.CIM.SetCIM(user);
            Misc.SetArmamentsAutoMod(user);

            if (!(Constants.swOption || Constants.deactivateSims))
            {
                events.RemoveAll(e => e.Ability != null && e.Ability.SimImpurity && e.UserSuperSource);
            }

            var degree = Environment.ProcessorCount;
            Parallel.For(0, user.AltGearSets.Count, new ParallelOptions { MaxDegreeOfParallelism = degree }, i =>
            {

                var altEventList = new List<Event>(events);
                List<TpEvent> tpEvents = altEventList.OfType<TpEvent>().ToList();
                var procEvents = Sim.EffectsSim.SimEffects(altEventList, user, fight, i);
                // Sim events added here will not be present in the stat gain iteration.
                // But stat tracker will pick up stat changes for tpEvents.

                foreach (TpEvent evt in tpEvents)
                {
                    // Loop for setting gains.
                    var altEvent = evt.AltEvents[i];
                    evt.AltEvents[i].Amount = evt.Amount.Clone();
                    Awakening.JudgAcCritGains(evt, user, i);
                    StatGains.AutoStatGains(evt, user, i);
                    if (evt.IsHealDoneEvent() && evt is HealEvent hEvt)
                    {
                        martyr.MartyrAntiGains(hEvt, user, i);
                        gj.CritGains(evt, user, i);
                        if (evt.Ability.ScalesWith(StatName.Mastery))
                        {
                            MasteryTracker.MasteryGains((HealEvent)evt, user, i);
                        }
                    }
                }
                FullAllocs.FullAllocGains(tpEvents, user, i);
                DupliEffects.altSelfless(tpEvents, user, i);
                DupliEffects.altBeacon(tpEvents, user, i);

                // Only summer and leech will react to this.
                tpEvents = altEventList.OfType<TpEvent>().ToList(); // Updating here to include non proc sim events
                Shared.ProcessEvents.AddProcEvents(tpEvents, procEvents); 

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
