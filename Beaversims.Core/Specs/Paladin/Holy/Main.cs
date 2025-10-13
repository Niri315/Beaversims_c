
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
            Abilities.HolyShock holyShock = (Abilities.HolyShock)user.Abilities.Get(Abilities.HolyShock.name);
            var gj = (Abilities.GreaterJudgment)user.Abilities.Get(Abilities.GreaterJudgment.name);
            var martyr = (Abilities.LightOfTheMartyr)user.Abilities.Get(Abilities.LightOfTheMartyr.name);
            var cs = (Abilities.CrusaderStrike)user.Abilities.Get(Abilities.CrusaderStrike.name);
            SpecInit(user);

            var beacons = MasteryTracker.FindStarterBeacons(allUnits);
            var judg = (Abilities.Judgment)user.Abilities.Get(Abilities.Judgment.name);
            Console.WriteLine($"Judgment Casts: {judg.Casts}");
            MasteryTracker.FindCoords(events);
            var statLogger = new Logger("StatTracker", fight, user.Id.TypeId);

            foreach (Event evt in events)
            {
                // Loop for tracking buffs and collecting data.
                BuffTracker.TrackBuffs(evt, allUnits, statLogger);
                evt.CreateAltEvents(user);
                MasteryTracker.TrackBeacons(evt, beacons, user);
                MasteryTracker.SetMasteryEff(evt, beacons, user);
                CastProcessor.ProcessCast(evt, user);
                DupliEffects.AddBeaconPolHeal(evt);
                Awakening.TrackAwakening(evt, user);
                judg.TrackGJCritChance(evt, user);
                HCGM.TrackEmpLod(evt, user);
                if (evt is ThroughputEvent tEvt)
                {
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

            HCGM.ModifyHCGMSources(user, fight);
            Shared.HCCGM.SetHCCGM(user);
            HCGM.ModifyHCGM(user, fight);
            holyShock.AlterHCGM(user);
            foreach (var ability in user.Abilities)
            {
                Console.WriteLine($"HCGM: {ability.Name}: {ability.HCCGM}");
            }

            Console.WriteLine($"Judgment HCGM data:");
            Console.WriteLine($"CdTimeTotal:{judg.CdTimeHypo}, True Cast Time:{judg.TrueCastTimeTotal}");

            Console.WriteLine($"Crusader Strike HCGM data:");
            Console.WriteLine($"CdTimeTotal:{cs.CdTimeHypo}, True Cast Time:{cs.TrueCastTimeTotal}");

            Console.WriteLine($"Holy Shock HCGM data:");
            Console.WriteLine($"CdTimeTotal:{holyShock.CdTimeHypo}, True Cast Time:{holyShock.TrueCastTimeTotal}");

            var degree = Environment.ProcessorCount;
            Parallel.For(0, user.altGearSets.Count, new ParallelOptions { MaxDegreeOfParallelism = degree }, i =>
            {

                foreach (Event evt in events)
                {  
                    //if (i == 12 && evt.Timestamp <= 0)
                    //{
                    //    Console.WriteLine($"{user.altGearSets[i].Name}:");
                    //    Console.WriteLine("Rating:");
                    //    Console.WriteLine($" Mastery: {evt.AltEvents[i].UserStats.Get(StatName.Mastery).Rating}:");
                    //    Console.WriteLine($" Vers: {evt.AltEvents[i].UserStats.Get(StatName.Vers).Rating}:");
                    //    Console.WriteLine($" Crit: {evt.AltEvents[i].UserStats.Get(StatName.Crit).Rating}:");
                    //    Console.WriteLine($" Haste: {evt.AltEvents[i].UserStats.Get(StatName.Haste).Rating}:");
                    //    Console.WriteLine($" Intellect: {evt.AltEvents[i].UserStats.Get(StatName.Intellect).Rating}:");
                    //    Console.WriteLine("Eff:");
                    //    Console.WriteLine($" Mastery: {evt.AltEvents[i].UserStats.Get(StatName.Mastery).Eff}:");
                    //    Console.WriteLine($" Vers: {evt.AltEvents[i].UserStats.Get(StatName.Vers).Eff}:");
                    //    Console.WriteLine($" Crit: {evt.AltEvents[i].UserStats.Get(StatName.Crit).Eff}:");
                    //    Console.WriteLine($" Haste: {evt.AltEvents[i].UserStats.Get(StatName.Haste).Eff}:");
                    //    Console.WriteLine($" Intellect: {evt.AltEvents[i].UserStats.Get(StatName.Intellect).Eff}:");
                    //    var mast = (SecondaryStat)evt.AltEvents[i].UserStats.Get(StatName.Mastery);
                    //    Console.WriteLine($" Mastery Bracket: {mast.Bracket}");
                        

                    //}
                    
                    // Loop for setting gains.
                    var altEvent = evt.AltEvents[i];
                    if (evt is ThroughputEvent tEvt)
                    {
             
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

            });

            //for (int i = 0; i < user.altGearSets.Count; i++)
            //{

            //    foreach (Event evt in events)
            //    {
            //        // Loop for setting gains.
            //        var altEvent = evt.AltEvents[i];
            //        Awakening.AwakeningScalers(evt, user);
            //        if (evt is ThroughputEvent tEvt)
            //        {
            //            if (tEvt.IsHealDoneEvent() && evt is HealEvent hEvt)
            //            {
            //                StatGains.AutoStatGainsHeal((HealEvent)tEvt, user, i);
            //                MasteryTracker.MasteryGains((HealEvent)tEvt, user, i);

            //                gj.CritGains(tEvt, user, i);
            //            }
            //            if (tEvt.IsDmgDoneEvent())
            //            {
            //                StatGains.AutoStatGainsDmg((DamageEvent)tEvt, user, i);
            //            }
            //            StatGains.AutoStatGainsMisc(tEvt, user, i);
            //        }
            //    }
            //    Awakening.ResetAwakeningScalers(user);
            //    FullAllocs.FullAllocGains(events, user, i);
            //    DupliEffects.altSelfless(events, user, i);
            //    DupliEffects.altBeacon(events, user, i);
            //    Shared.DupliEffects.AltSummerSource(events, user, i);
            //    Shared.DupliEffects.AltLeechSource(events, user, i);
            //}
        }
    }
}
