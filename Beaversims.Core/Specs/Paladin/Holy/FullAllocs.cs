using Beaversims.Core.Shared.Abilities;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
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
            
        public static double GainPerArmsTrigger(CrusaderStrike cs, Judgment judg)
        {
            return (((cs.Damage.Crit.Count + judg.Damage.Crit.Count)) / 100) / Crit.percentRate;
        }


        public static void HaaGains(List<TpEvent> events, User user, int i)
        {
            if (!user.HasTalent(Talents.HammerAndAnvil.id)) { return; }

            var abilities = user.Abilities;
            var haa = (Abilities.HammerAndAnvil)user.Abilities.Get(Abilities.HammerAndAnvil.name);
            if (haa.Heal.Raw == 0) { return; }

            var judg = (Abilities.Judgment)user.Abilities.Get(Abilities.Judgment.name);
            var cs = (Abilities.CrusaderStrike)user.Abilities.Get(Abilities.CrusaderStrike.name);
            var lesserWep = (Abilities.LesserWeapon)user.Abilities.Get(Abilities.LesserWeapon.name);
            var lesserBulwark = (Shared.Abilities.LesserBulwark)user.Abilities.Get(Shared.Abilities.LesserBulwark.name);

            var wepRaw = lesserWep.Heal.Raw;
            var haaRaw = haa.Heal.Raw;
            var wepDmg = lesserWep.Damage.Dmg;
            var bulwarkRaw = lesserBulwark.Heal.Raw;            

            var gainPerTriggerHaaRaw = haaRaw / ((cs.Damage.Crit.Count * cs.HaaFactor) + judg.Damage.Crit.Count) / 100 / Crit.percentRate;
            var gainPerWepRaw = ((wepRaw / ((cs.Damage.Crit.Count) + judg.Damage.Crit.Count)) / 100) / Crit.percentRate;
            var gainPerBulwarkRaw = ((bulwarkRaw / ((cs.Damage.Crit.Count) + judg.Damage.Crit.Count)) / 100) / Crit.percentRate;
            var gainPerWepDmg = ((wepDmg / ((cs.Damage.Crit.Count) + judg.Damage.Crit.Count)) / 100) / Crit.percentRate;

            var posHaaEvtCount = 0;
            foreach (var evt in events)
            {
                if (IsPosHaaEvent(evt))
                {
                    posHaaEvtCount += 1;

                }

                var altEvent = evt.AltEvents[i];
                if (evt.AbilityName == Abilities.HammerAndAnvil.name)
                {
                    haa.AltHeal[i].Raw += altEvent.Amount.Raw;
                    haa.AltHeal[i].Eff += altEvent.Amount.Eff;
                }
                else if (evt.AbilityName == Abilities.LesserWeapon.name)
                {
                    if (evt.IsHealDoneEvent())
                    {
                        lesserWep.AltHeal[i].Raw += altEvent.Amount.Raw;
                        lesserWep.AltHeal[i].Eff += altEvent.Amount.Eff;
                    }
                    else if (evt.IsDmgDoneEvent())
                    {
                        lesserWep.AltDamage[i].Dmg += altEvent.Amount.Raw;
                    }

                }
                else if (evt.AbilityName == Shared.Abilities.LesserBulwark.name)
                {

                    lesserBulwark.AltHeal[i].Raw += altEvent.Amount.Raw;
                    lesserBulwark.AltHeal[i].Eff += altEvent.Amount.Eff;
                }
                
            }

            var csProcCount = cs.Damage.Crit.Count;
            var judgProcCount = judg.Damage.Crit.Count;
            var haaCount = haa.Heal.Count; // aoe
            var correctingCoef = posHaaEvtCount / haaCount;
            foreach (var evt in events)
            {
                if (evt is TpEvent tEvt)
                {
                    var crit = evt.UserStats.Get(StatName.Crit);
                    var altCrit = evt.AltEvents[i].UserStats.Get(StatName.Crit);
                    var altEvent = evt.AltEvents[i];
                    var gainRaw = 0.0;
                    if (evt.AbilityName == haa.Name)
                    {
                        gainRaw = (altCrit.TrueEff() - crit.TrueEff()) * correctingCoef * haa.AltHeal[i].Raw / ((cs.Damage.Crit.Count) + judg.Damage.Crit.Count) / 100 / Crit.percentRate;
                    }
                    if (evt.AbilityName == lesserBulwark.Name)
                    {
                        gainRaw = (altCrit.TrueEff() - crit.TrueEff()) * correctingCoef * lesserBulwark.AltHeal[i].Raw / ((cs.Damage.Crit.Count) + judg.Damage.Crit.Count) / 100 / Crit.percentRate;
                    }
                    if (evt.AbilityName == lesserWep.Name)
                    {
                        if (evt.IsHealDoneEvent())
                        {
                            gainRaw = (altCrit.TrueEff() - crit.TrueEff()) * correctingCoef * lesserWep.AltHeal[i].Raw / ((cs.Damage.Crit.Count) + judg.Damage.Crit.Count) / 100 / Crit.percentRate;
                        }
                        else if (evt.IsDmgDoneEvent())
                        {
                            gainRaw = (altCrit.TrueEff() - crit.TrueEff()) * correctingCoef * lesserWep.AltDamage[i].Dmg / ((cs.Damage.Crit.Count) + judg.Damage.Crit.Count) / 100 / Crit.percentRate;
                        }

                    }
                    altEvent.Amount.UpdateAltGainsFromEvtData(tEvt, gainRaw, i);
                    
                }
            }
        }

        public static void SunSearGains(List<TpEvent> events, User user, int i)
        {

            var abilities = user.Abilities;
            var sunsear = (Abilities.SunSear)user.Abilities.Get(Abilities.SunSear.name);
            if (sunsear.Heal.Raw == 0) { return; }

            var holyshock = (Abilities.HolyShock)user.Abilities.Get(Abilities.HolyShock.name);
            var lod = (Abilities.LightOfDawn)user.Abilities.Get(Abilities.LightOfDawn.name);
            var gainPerTriggerRaw = sunsear.Heal.Raw / (holyshock.Heal.Crit.Count + lod.Heal.Crit.Count) / 100 / Crit.percentRate;
            foreach (var evt in events)
            {
                var altEvent = evt.AltEvents[i];
                if (evt.AbilityName == Abilities.SunSear.name)
                {
                    sunsear.AltHeal[i].Raw += altEvent.Amount.Raw;
                    sunsear.AltHeal[i].Eff += altEvent.Amount.Eff;
                }
            }
            var posTriggerCount = holyshock.Heal.Count + lod.Heal.Count;
            var sunsearCount = sunsear.Heal.Count;
            var correctingCoef = posTriggerCount / sunsearCount;

            foreach (var evt in events)
            {
                var crit = evt.UserStats.Get(StatName.Crit);
                var altCrit = evt.AltEvents[i].UserStats.Get(StatName.Crit);
                var altEvent = evt.AltEvents[i];
                var gainRaw = 0.0;
                if (evt.AbilityName == sunsear.Name)
                {
                    gainRaw = (altCrit.TrueEff() - crit.TrueEff()) * correctingCoef * sunsear.AltHeal[i].Raw / (holyshock.Heal.Crit.Count + lod.Heal.Crit.Count) / 100 / Crit.percentRate;
                } 
                altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);
            }
        }

        public static void FullAllocGains(List<TpEvent> tpEvents, User user, int i)
        {
            HaaGains(tpEvents, user, i);
            SunSearGains(tpEvents, user, i);
        }
    }
}
