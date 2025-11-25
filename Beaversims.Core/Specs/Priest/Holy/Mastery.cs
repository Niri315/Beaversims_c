using Beaversims.Core.Common;
using Beaversims.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Priest.Holy
{ 
    internal static class MasteryTracker
    {



        public static void MasteryGains(TpEvent evt, User user, int i, bool antiGain = false)
        {
            if (evt.AbilityName == Abilities.EchoOfLight.name && evt.IsHealDoneEvent())
            {
                var statName = StatName.Mastery;

                var stat = evt.UserStats.Get(statName);
                var altEvent = evt.AltEvents[i];
                var altStat = altEvent.UserStats.Get(statName);
                var gainPerPrimRaw = altEvent.Amount.Raw / stat.TrueEff();
                var gainRaw = gainPerPrimRaw * (altStat.TrueEff() - stat.TrueEff());
                if (antiGain) { gainRaw *= -1; }
                altEvent.NukeRaw += gainRaw; // Always SimDupli
                altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);
            }
       
        }
    }
}
