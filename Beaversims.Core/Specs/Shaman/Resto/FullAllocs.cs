using Beaversims.Core.Shared.Abilities;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Shaman.Resto
{
    internal class FullAllocs
    {
        public static void AncestralAwakening(List<TpEvent> tpEvents, User user, int i)
        {
            
            var aa = user.Abilities.Get(Abilities.AncestralAwakening.name);
            if (aa.Heal.Raw == 0) return;

            var procChanceDiff = Talents.AncestralAwakening.critProcChance - Talents.AncestralAwakening.procChance;

            var healingWave = user.Abilities.Get(Abilities.HealingWave.name);
            var riptide = user.Abilities.Get(Abilities.Riptide.name);
            var healPerExe = aa.Heal.Raw / aa.Heal.Count;
            var sourceCount = healingWave.Casts + riptide.Casts;  // No pets, only from user.

            var gainPerCritPerCast = procChanceDiff * healPerExe / Crit.percentRate / 100;

            var gainPerAaEvt = gainPerCritPerCast * sourceCount / aa.Heal.Count;

            foreach (var evt in tpEvents)
            {
                var crit = evt.UserStats.Get(StatName.Crit);
                var altCrit = evt.AltEvents[i].UserStats.Get(StatName.Crit);
                var altEvent = evt.AltEvents[i];
                if (evt.AbilityName == aa.Name)
                {
                    var gainRaw = (altCrit.TrueEff() - crit.TrueEff()) * gainPerAaEvt;
                    //altEvent.NukeRaw += gainRaw;
                    altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);
                }

            }
            Console.WriteLine($"gainPerCritPerCast {gainPerCritPerCast}, gainPerAaEvt: {gainPerAaEvt}");
            
            Console.WriteLine($"Count: {aa.Heal.Count}, total heal: {aa.Heal.Raw}");
            Console.WriteLine($"casts - healingWave: {healingWave.Casts}, riptide: {riptide.Casts}");

        }

        public static void FullAllocGains(List<TpEvent> tpEvents, User user, int i)
        {
            AncestralAwakening(tpEvents, user, i);
        }
    }
}
