using Beaversims.Core.Common;
using Beaversims.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Shaman.Resto
{ 
    internal static class MasteryTracker
    {  
        public static void SetMasteryEff(HealEvent evt, User user)
        {
            var hp_p = evt.TargetHp_p();
            if (hp_p == null)
            {

                hp_p = 1;
            }
            evt.MasteryEffectiveness = 1 - hp_p.Value;
        }

        public static double MasteryGainCalc(Mastery mastery, double amount, double masteryEffectiveness)
    => (((amount / ((mastery.TrueEff() * masteryEffectiveness) / (mastery.PercentRate * 100) + 1)) * masteryEffectiveness) / (mastery.PercentRate * 100));


        public static void MasteryAltAmount(HealEvent evt, Mastery mastery, Haste haste, int i, User user, bool antiGain = false)
        {
 
            var altEvent = evt.AltEvents[i];
            var altMastery = altEvent.UserStats.Get(mastery.Name);
            var altHaste = altEvent.UserStats.Get(haste.Name);
            var gainRaw = MasteryGainCalc(mastery, altEvent.Amount.Raw, evt.MasteryEffectiveness) * (altMastery.TrueEff() - mastery.TrueEff());
          
          
            if (antiGain)
            {
                gainRaw *= -1;
            }

            if (evt.Ability.SimDupliAbility)
            {
                altEvent.NukeRaw += gainRaw;
            }

            altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);

        }

        public static void MasteryGains(HealEvent evt, User user, int i, bool antiGain = false)
        {
            var statName = StatName.Mastery;
            var mastery = (Mastery)evt.UserStats.Get(statName);
            var haste = (Haste)evt.UserStats.Get(StatName.Haste);
            MasteryAltAmount(evt, mastery, haste, i, user, antiGain:antiGain);
            
        }
    }
}
