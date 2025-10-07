
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beaversims.Core.Common;
using Beaversims.Core.Shared;


namespace Beaversims.Core.Specs.DummySpec
{
    internal class Main

    {
 

        public static void SpecMain(List<Event> events, UnitRepo allUnits, Fight fight)
        {

            var user = allUnits.GetUser();
            var statLogger = new Logger("StatTracker", fight, user.Id.TypeId);
            foreach (Event evt in events)
            {
                // Loop for tracking buffs and collecting data.
                BuffTracker.TrackBuffs(evt, allUnits, statLogger);

            }
            Utils.CleanUp(allUnits); // To avoid accidental usage.
            foreach (Event evt in events)
            {
                // Hypo loop
                if (evt is ThroughputEvent tEvt)
                {
                    Shared.DupliEffects.SharedHypo(tEvt, user);
                }

            }

     
            foreach (Event evt in events)
            {
                // Loop for setting gains.
             
                if (evt is ThroughputEvent tpEvent)
                {
                    if (tpEvent.IsHealDoneEvent())
                    {
                        StatGains.AutoStatGainsHeal((HealEvent)tpEvent, user);
                    }
                    if (tpEvent.IsDmgDoneEvent())
                    {
                        StatGains.AutoStatGainsDmg((DamageEvent)tpEvent, user);
                    }
                    StatGains.AutoStatGainsMisc(tpEvent, user);
                }
            }
        }
    }
}
