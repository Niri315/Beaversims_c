
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static void SpecMain(List<Event> events, UnitRepo allUnits)
        {
            var user = allUnits.GetUser();
            SpecInit(user);
            var beacons = MasteryTracker.FindStarterBeacons(allUnits);
            MasteryTracker.FindCoords(events);
            foreach (Event evt in events)
            {
                // Loop for tracking buffs and collecting data.
                BuffTracker.TrackBuffs(evt, allUnits);
                MasteryTracker.TrackBeacons(evt, beacons, user);
                MasteryTracker.SetMasteryEff(evt, beacons, user);
                CastProcessor.ProcessCast(evt, user);
                if (evt is HealEvent hEvt)
                {
                    DupliEffects.BeaconHypo(hEvt, user);
                }
            }
            Abilities.HolyShock holyShock = (Abilities.HolyShock)user.Abilities.Get(Abilities.HolyShock.name);
            holyShock.SetHCGM();

            user.Stats = null;  // To avoid accidental usage.
            MasteryTracker.CleanUpCoords(allUnits);
            HCGM.SetHCGM(user);
            foreach (Event evt in events)
            {
                // Loop for setting gains.
                if (evt is ThroughputEvent tpEvent)
                {   
                    if (tpEvent.IsHealDoneEvent())
                    {
                        StatGains.AutoStatGainsHeal((HealEvent)tpEvent, user);
                        MasteryTracker.MasteryGains((HealEvent)tpEvent, user);
                    }
                    if (tpEvent.IsDmgDoneEvent())
                    {
                        StatGains.AutoStatGainsDmg((DamageEvent)tpEvent, user);
                    }
                    StatGains.AutoStatGainsMisc(tpEvent);
                }
            }
        }
    }
}
