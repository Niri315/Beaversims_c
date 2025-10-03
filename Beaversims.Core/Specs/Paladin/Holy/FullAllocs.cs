using Beaversims.Core.Shared.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Paladin.Holy
{
    internal class FullAllocs
    {
        private static bool IsPosHaaEvent(Event evt) =>
            evt.IsDmgDoneEvent() &&
            (evt.AbilityName == Abilities.Judgment.name || evt.AbilityName == Abilities.CrusaderStrike.name) &&
            !evt.AwakenedJudgment;
            

        public static void HaaGains(List<Event> events, User user)
        {
            // TODO - Make function properly with intervals.

            var abilities = user.Abilities;
            var haa = (Abilities.HammerAndAnvil)user.Abilities.Get(Abilities.HammerAndAnvil.name);
            if (haa.Heal.Raw == 0) { return; }

            var judg = (Abilities.Judgment)user.Abilities.Get(Abilities.Judgment.name);
            var cs = (Abilities.CrusaderStrike)user.Abilities.Get(Abilities.CrusaderStrike.name);
            var lesserWep = (Abilities.LesserWeapon)user.Abilities.Get(Abilities.LesserWeapon.name);

            var armsRaw = lesserWep.Heal.Raw;
            var armsEff = lesserWep.Heal.Eff;
            var haaRaw = haa.Heal.Raw;
            var effectTotalDmg = lesserWep.Damage.Dmg;

            if (abilities.Contains(Shared.Abilities.LesserBulwark.name))
            {
                var lesserBulwark = (Shared.Abilities.LesserBulwark)user.Abilities.Get(Shared.Abilities.LesserBulwark.name);
                armsRaw += lesserBulwark.Heal.Raw;
                armsEff += lesserBulwark.Heal.Eff;
            }

            var gainPerTriggerHaaRaw = ((haaRaw / ((cs.Damage.Crit.Count * cs.HaaFactor) + judg.Damage.Crit.Count)) / 100) / Crit.percentRate;
            var gainPerArmsRaw = (armsRaw / ((cs.Damage.Crit.Count + judg.Damage.Crit.Count)) / 100) / Crit.percentRate;
            var gainPerTriggerDmg = (effectTotalDmg / ((cs.Damage.Crit.Count + judg.Damage.Crit.Count)) / 100) / Crit.percentRate;

            var armsUr = 0.0;
            if (armsRaw > 0)
            {
                armsUr = armsEff / armsRaw;
            }

            var statName = StatName.Crit;
            foreach (var evt in events)
            {
                if (IsPosHaaEvent(evt))
                {
                    var crit = (Crit)evt.UserStats.Get(statName);

                    var gainHaaRaw = crit.ApplyDryMult(gainPerTriggerHaaRaw);
                    var gainArmsRaw = crit.ApplyDryMult(gainPerArmsRaw);
                    var gainDmg = crit.ApplyDryMult(gainPerTriggerDmg);
                    if (evt.AbilityName == Abilities.CrusaderStrike.name)
                    {
                        gainHaaRaw *= cs.HaaFactor;
                    }
                    var gainHaaEff = haa.RawToEffConvert(gainHaaRaw);
                    var gainArmsEff = gainArmsRaw * armsUr;
                    var gainEff = gainHaaEff + gainArmsEff;


                    evt.Gains[statName][GainType.Eff] += gainEff;
                    evt.Gains[statName][GainType.Dmg] += gainDmg;

                    var gainNsnsnarawHaa = haa.RawToNsnsnarawConvert(gainHaaRaw);
                    var tEvt = (ThroughputEvent)evt;

                    Shared.DupliEffects.LeechSourceGains((ThroughputEvent)evt, user, statName, gainNsnsnarawHaa, GainType.Eff);
                    Shared.DupliEffects.SummerGains((ThroughputEvent)evt, user, statName, gainHaaRaw, haa, evt.SummerActive, false, user, GainType.Eff);
                    // Not sending summer/leech (eff) for lesser wep as its not from user.
                }
            }

        }
        public static void FullAllocGains(List<Event> events, User user)
        {
            HaaGains(events, user);
        }
    }
}
