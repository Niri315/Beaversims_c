
using Beaversims.Core.Common;
using Beaversims.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Beaversims.Core.Specs.Paladin.Holy
{
    internal class Main

    {
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
            var user = allUnits.GetUser();
            var holyShock = (Abilities.HolyShock)user.Abilities.Get(Abilities.HolyShock.name);
            var gj = (Abilities.GreaterJudgment)user.Abilities.Get(Abilities.GreaterJudgment.name);
            var martyr = (Abilities.LightOfTheMartyr)user.Abilities.Get(Abilities.LightOfTheMartyr.name);
            var cs = (Abilities.CrusaderStrike)user.Abilities.Get(Abilities.CrusaderStrike.name);
            SpecInit(user);

            var beacons = MasteryTracker.FindStarterBeacons(allUnits);
            var judg = (Abilities.Judgment)user.Abilities.Get(Abilities.Judgment.name);
            MasteryTracker.FindCoords(events);
            var statLogger = new Logger("StatTracker", fight, user.Id.TypeId);

            foreach (Event evt in events)
            {
                // Loop for tracking buffs and collecting data.
                BuffTracker.TrackBuffs(evt, allUnits, statLogger);
                evt.CreateAltEvents(user, evt);
                MasteryTracker.TrackBeacons(evt, beacons, user);
                MasteryTracker.SetMasteryEff(evt, beacons, user);
                CastProcessor.ProcessCast(evt, user);
                DupliEffects.AddBeaconPolHeal(evt);
                Awakening.TrackAwakening(evt, user);
                judg.TrackGJCritChance(evt, user);
                HCGM.TrackEmpLod(evt, user);


                if (evt is ThroughputEvent tEvt)
                {
                    //ProcessEvents.StoreNormTotals(tEvt, fight, user);
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

            HCGM.ModifyHCCGMSources(user, fight); // 
            Shared.HCCGM.SetHCCGM(user);
            HCGM.ModifyHCGM(user, fight);
            holyShock.AlterHCGM(user);

            Sim.TrinketSim.SimTrinkets(events, user, fight);

            //foreach (var ability in user.Abilities)
            //{
            //    Console.WriteLine($"{ability.Name} HCCGM: {ability.HCCGM}, True HCCGM: {ability.TrueHCCGM(user)}, cast gain mod: {ability.HasteCastGainMod}");
            //}

            var degree = Environment.ProcessorCount;
            Parallel.For(0, user.AltGearSets.Count, new ParallelOptions { MaxDegreeOfParallelism = degree }, i =>
            {
              
                    //Console.WriteLine(y);
                    foreach (Event evt in events)
                    {
                        // Loop for setting gains.
                        var altEvent = evt.AltEvents[i];

                        if (evt is ThroughputEvent tEvt)
                        {
                            evt.AltEvents[i].Amount = tEvt.Amount.Clone();
                            Awakening.JudgAcCritGains(tEvt, user, i);
                            StatGains.AutoStatGainsMisc(tEvt, user, i);
                            if (tEvt.IsHealDoneEvent() && evt is HealEvent hEvt)
                            {
                                martyr.MartyrAntiGains(hEvt, user, i);
                                StatGains.AutoStatGainsHeal((HealEvent)tEvt, user, i);
                                if (evt.Ability.ScalesWith(StatName.Mastery))
                                {
                                    MasteryTracker.MasteryGains((HealEvent)tEvt, user, i);
                                }
                                gj.CritGains(tEvt, user, i);
                            }
                            if (tEvt.IsDmgDoneEvent())
                            {
                                StatGains.AutoStatGainsDmg((DamageEvent)tEvt, user, i);
                            }

                        }
                    }
                    FullAllocs.FullAllocGains(events, user, i);
                    DupliEffects.altSelfless(events, user, i);
                    DupliEffects.altBeacon(events, user, i);
                    Shared.DupliEffects.AltSummerSource(events, user, i);
                    Shared.DupliEffects.AltLeechSource(events, user, i);
                    foreach (Event evt in events)
                    {
                        if (evt is ThroughputEvent tEvt)
                        {
                            Shared.ProcessEvents.StoreTotals(tEvt, user, i);
                        }
                    }
                
            });
        }
    }
}
