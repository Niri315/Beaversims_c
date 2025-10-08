
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beaversims.Core.Common;
using Beaversims.Core.Shared;


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
            SpecInit(user);
            var beacons = MasteryTracker.FindStarterBeacons(allUnits);
            var judg = (Abilities.Judgment)user.Abilities.Get(Abilities.Judgment.name);
            var gj = (Abilities.GreaterJudgment)user.Abilities.Get(Abilities.GreaterJudgment.name);

            MasteryTracker.FindCoords(events);
            var statLogger = new Logger("StatTracker", fight, user.Id.TypeId);

            foreach (Event evt in events)
            {
                // Loop for tracking buffs and collecting data.
                BuffTracker.TrackBuffs(evt, allUnits, statLogger);
                MasteryTracker.TrackBeacons(evt, beacons, user);
                MasteryTracker.SetMasteryEff(evt, beacons, user);
                CastProcessor.ProcessCast(evt, user);
                DupliEffects.AddBeaconPolHeal(evt);
                Awakening.TrackAwakening(evt, user);
                judg.TrackGJCritChance(evt, user);

            }
            Utils.CleanUp(allUnits); // To avoid accidental usage.
            foreach (Event evt in events)
            {
                // Hypo loop
                if (evt is ThroughputEvent tEvt)
                {
                    Shared.DupliEffects.SharedHypo(tEvt, user);
                    if (tEvt is HealEvent hEvt)
                    {
                        DupliEffects.BeaconHypo(hEvt, user);
                    }
                }

            }

            Abilities.HolyShock holyShock = (Abilities.HolyShock)user.Abilities.Get(Abilities.HolyShock.name);
            holyShock.SetHCGM();

            MasteryTracker.CleanUpCoords(allUnits);
            HCGM.SetHCGM(user);

            foreach (Event evt in events)
            {
                // Loop for setting gains.


                var testStatTracker = evt.UserStats.Clone();
                //testStatTracker.Get(StatName.Mastery).ChangeAmount(1, StatAmountType.Rating, removal: false);
                //testStatTracker.Get(StatName.Intellect).ChangeAmount(1, StatAmountType.Rating, removal: false);
                //testStatTracker.Get(StatName.Vers).ChangeAmount(1, StatAmountType.Rating, removal: false);
                //testStatTracker.Get(StatName.Haste).ChangeAmount(1, StatAmountType.Rating, removal: false);
                testStatTracker.Get(StatName.Crit).ChangeAmount(1, StatAmountType.Rating, removal: false);
                //testStatTracker.Get(StatName.Leech).ChangeAmount(1, StatAmountType.Rating, removal: false);
                //testStatTracker.Get(StatName.Avoidance).ChangeAmount(1, StatAmountType.Rating, removal: false);
                //testStatTracker.Get(StatName.Stamina).ChangeAmount(1, StatAmountType.Rating, removal: false);



                testStatTracker.UpdateAllStats();
                evt.AltEvents.Add(new AltEvent(testStatTracker));

                Awakening.AwakeningScalers(evt, user);
                if (evt is ThroughputEvent tpEvent)
                {
                    evt.AltEvents[0].Amount = tpEvent.Amount.Clone();
                    //Console.WriteLine(evt.altEvents[0].Amount.Eff);
                    if (tpEvent.IsHealDoneEvent())
                    {
                        StatGains.AutoStatGainsHeal((HealEvent)tpEvent, user);
                        MasteryTracker.MasteryGains((HealEvent)tpEvent, user);
                        gj.CritGains(tpEvent, user);
                    }
                    if (tpEvent.IsDmgDoneEvent())
                    {
                        StatGains.AutoStatGainsDmg((DamageEvent)tpEvent, user);
                    }
                    StatGains.AutoStatGainsMisc(tpEvent, user);
                }
            }
            FullAllocs.FullAllocGains(events, user);

            DupliEffects.altBeacon(events, user);
            Shared.DupliEffects.AltSummerSource(events, user);
            Shared.DupliEffects.AltLeechSource(events, user);

        }
    }
}
