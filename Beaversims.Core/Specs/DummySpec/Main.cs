
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
        public static void SpecMain(List<Event> events, UnitRepo allUnits, Fight fight, int iterationCount)
        {
            var user = allUnits.GetUser();
            var statLogger = new Logger("StatTracker", fight, user.Id.TypeId);
            foreach (Event evt in events)
            {
                // Loop for tracking buffs and collecting data.
                BuffTracker.TrackBuffs(evt, allUnits, statLogger);
  
                if (evt is TpEvent tEvt)
                {
                    //Shared.DupliEffects.SharedHypo(tEvt, user);
                }

            }
            Utils.CleanUp(allUnits, events); // To avoid accidental usage.

            for (int i = 0; i < user.AltGearSets.Count; i++)
            {

                foreach (Event evt in events)
                { // Loop for setting gains.
                    evt.CreateAltEvents(user, evt);
                    var altEvent = evt.AltEvents[i];
                    if (evt is TpEvent tpEvent)
                    {
                        StatGains.AutoStatGains(tpEvent, user, i);
                    }
                }
            }
        }
    }
}
