
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
  
                if (evt is ThroughputEvent tEvt)
                {
                    Shared.DupliEffects.SharedHypo(tEvt, user);
                }

            }
            Utils.CleanUp(allUnits); // To avoid accidental usage.

            for (int i = 0; i < user.AltGearSets.Count; i++)
            {

                foreach (Event evt in events)
                { // Loop for setting gains.
                    evt.CreateAltEvents(user, evt);
                    var altEvent = evt.AltEvents[i];
                    if (evt is ThroughputEvent tpEvent)
                    {
                        if (tpEvent.IsHealDoneEvent())
                        {
                            StatGains.AutoStatGainsHeal((HealEvent)tpEvent, user, i);
                        }
                        if (tpEvent.IsDmgDoneEvent())
                        {
                            StatGains.AutoStatGainsDmg((DamageEvent)tpEvent, user, i);
                        }
                        StatGains.AutoStatGainsMisc(tpEvent, user, i);
                    }
                }
            }
        }
    }
}
