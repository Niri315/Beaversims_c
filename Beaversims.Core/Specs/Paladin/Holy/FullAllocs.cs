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


        public static void HaaGains(List<Event> events, User user)
        {
            if (!user.HasTalent(Talents.HammerAndAnvil.id)) { return; }

            var abilities = user.Abilities;
            var haa = (Abilities.HammerAndAnvil)user.Abilities.Get(Abilities.HammerAndAnvil.name);
            if (haa.Heal.Raw == 0) { return; }

            var judg = (Abilities.Judgment)user.Abilities.Get(Abilities.Judgment.name);
            var cs = (Abilities.CrusaderStrike)user.Abilities.Get(Abilities.CrusaderStrike.name);
            var lesserWep = (Abilities.LesserWeapon)user.Abilities.Get(Abilities.LesserWeapon.name);

            var wepRaw = lesserWep.Heal.Raw;
            var haaRaw = haa.Heal.Raw;
            var wepDmg = lesserWep.Damage.Dmg;
            var bulwarkRaw = 0.0;

            Shared.Abilities.LesserBulwark lesserBulwark;
            if (abilities.Contains(Shared.Abilities.LesserBulwark.name))
            {
                lesserBulwark = (Shared.Abilities.LesserBulwark)user.Abilities.Get(Shared.Abilities.LesserBulwark.name);
                bulwarkRaw += lesserBulwark.Heal.Raw;
            }
            else
            {
                lesserBulwark = new Shared.Abilities.LesserBulwark();
            }

            var gainPerTriggerHaaRaw = haaRaw / ((cs.Damage.Crit.Count * cs.HaaFactor) + judg.Damage.Crit.Count) / 100 / Crit.percentRate;
            var gainPerWepRaw = ((wepRaw / ((cs.Damage.Crit.Count) + judg.Damage.Crit.Count)) / 100) / Crit.percentRate;
            var gainPerBulwarkRaw = ((bulwarkRaw / ((cs.Damage.Crit.Count) + judg.Damage.Crit.Count)) / 100) / Crit.percentRate;
            var gainPerWepDmg = ((wepDmg / ((cs.Damage.Crit.Count) + judg.Damage.Crit.Count)) / 100) / Crit.percentRate;


            var posHaaEvtCount = 0;
            var statName = StatName.Crit;
            foreach (var evt in events)
            {
                if (IsPosHaaEvent(evt))
                {
                    posHaaEvtCount += 1;
                    var crit = (Crit)evt.UserStats.Get(statName);

                    var gainHaaRaw = crit.ApplyDryMult(gainPerTriggerHaaRaw);
                    var gainWepRaw = crit.ApplyDryMult(gainPerWepRaw);
                    var gainBulwarkRaw = crit.ApplyDryMult(gainPerBulwarkRaw);
                    var gainDmg = crit.ApplyDryMult(gainPerWepDmg);
                    if (evt.AbilityName == Abilities.CrusaderStrike.name)
                    {
                        gainHaaRaw *= cs.HaaFactor;
                    }
                    var gainHaaEff = haa.RawToEffConvert(gainHaaRaw);
                    var gainWepEff = lesserWep.RawToEffConvert(gainWepRaw);
                    var gainEff = gainHaaEff + gainWepEff;


                    lesserBulwark = (Shared.Abilities.LesserBulwark)user.Abilities.Get(Shared.Abilities.LesserBulwark.name);
                    var gainBulwarkEff = lesserBulwark.RawToEffConvert(gainBulwarkRaw);
                    gainEff += gainBulwarkEff;


                    evt.Gains[statName][GainType.Eff] += gainEff;
                    evt.Gains[statName][GainType.Dmg] += gainDmg;

                    var gainNsnsnarawHaa = haa.RawToNsnsnarawConvert(gainHaaRaw);

                    Shared.DupliEffects.LeechSourceGains((ThroughputEvent)evt, user, statName, gainNsnsnarawHaa, GainType.Eff);
                    Shared.DupliEffects.SummerGains((ThroughputEvent)evt, user, statName, gainHaaRaw, haa, evt.SummerActive, false, user, GainType.Eff);
                    // Not sending summer/leech (eff) for lesser wep as its not from user.
                }
                if (evt is ThroughputEvent)
                {
                    for (int i = 0; i < evt.AltEvents.Count; i++)
                    {
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
                }
              
            }

            var csProcCount = cs.Damage.Crit.Count;
            var judgProcCount = judg.Damage.Crit.Count;
            var haaCount = haa.Heal.Count; // aoe
            var correctingCoef = posHaaEvtCount / haaCount;

            foreach (var evt in events)
            {
                if (evt is ThroughputEvent tEvt)
                {
                    for (int i = 0; i < evt.AltEvents.Count; i++)
                    {
                        var crit = evt.UserStats.Get(StatName.Crit);
                        var altCrit = evt.AltEvents[i].UserStats.Get(StatName.Crit);
                        var altEvent = evt.AltEvents[i];
                        var gainRaw = 0.0;
                        if (evt.AbilityName == haa.Name)
                        {
                            gainRaw = (altCrit.Eff - crit.Eff) * correctingCoef * haa.AltHeal[i].Raw / ((cs.Damage.Crit.Count) + judg.Damage.Crit.Count) / 100 / Crit.percentRate;
                        }
                        if (evt.AbilityName == lesserBulwark.Name)
                        {
                            gainRaw = (altCrit.Eff - crit.Eff) * correctingCoef * lesserBulwark.AltHeal[i].Raw / ((cs.Damage.Crit.Count) + judg.Damage.Crit.Count) / 100 / Crit.percentRate;
                        }
                        if (evt.AbilityName == lesserWep.Name)
                        {
                            if (evt.IsHealDoneEvent())
                            {
                                gainRaw = (altCrit.Eff - crit.Eff) * correctingCoef * lesserWep.AltHeal[i].Raw / ((cs.Damage.Crit.Count) + judg.Damage.Crit.Count) / 100 / Crit.percentRate;
                            }
                            else if (evt.IsDmgDoneEvent())
                            {
                                gainRaw = (altCrit.Eff - crit.Eff) * correctingCoef * lesserWep.AltDamage[i].Dmg / ((cs.Damage.Crit.Count) + judg.Damage.Crit.Count) / 100 / Crit.percentRate;
                            }

                        }
                        altEvent.Amount.UpdateAltGainsFromEvtData(tEvt, gainRaw, i);

                    }
                }
               
            }
        }

        public static void SunSearGains(List<Event> events, User user)
        {

            var abilities = user.Abilities;
            var sunsear = (Abilities.SunSear)user.Abilities.Get(Abilities.SunSear.name);
            if (sunsear.Heal.Raw == 0) { return; }

            var holyshock = (Abilities.HolyShock)user.Abilities.Get(Abilities.HolyShock.name);
            var lod = (Abilities.LightOfDawn)user.Abilities.Get(Abilities.LightOfDawn.name);
            var gainPerTriggerRaw = sunsear.Heal.Raw / (holyshock.Heal.Crit.Count + lod.Heal.Crit.Count) / 100 / Crit.percentRate;
            foreach (var evt in events)
            {
                if (evt is ThroughputEvent)
                {
                    for (int i = 0; i < evt.AltEvents.Count; i++)
                    {
                        var altEvent = evt.AltEvents[i];
                        if (evt.AbilityName == Abilities.SunSear.name)
                        {
                            sunsear.AltHeal[i].Raw += altEvent.Amount.Raw;
                            sunsear.AltHeal[i].Eff += altEvent.Amount.Eff;
                        }
                    }
                }

            }

            var posTriggerCount = holyshock.Heal.Count + lod.Heal.Count;
            var sunsearCount = sunsear.Heal.Count;
            var correctingCoef = posTriggerCount / sunsearCount;

            foreach (var evt in events)
            {
                if (evt is ThroughputEvent tEvt)
                {
                    for (int i = 0; i < evt.AltEvents.Count; i++)
                    {
                        var crit = evt.UserStats.Get(StatName.Crit);
                        var altCrit = evt.AltEvents[i].UserStats.Get(StatName.Crit);
                        var altEvent = evt.AltEvents[i];
                        var gainRaw = 0.0;
                        if (evt.AbilityName == sunsear.Name)
                        {
                            gainRaw = (altCrit.Eff - crit.Eff) * correctingCoef * sunsear.AltHeal[i].Raw / (holyshock.Heal.Crit.Count + lod.Heal.Crit.Count) / 100 / Crit.percentRate;
                        }
                       
                        altEvent.Amount.UpdateAltGainsFromEvtData(tEvt, gainRaw, i);

                    }
                }

            }
        }

        public static void FullAllocGains(List<Event> events, User user)
        {
            HaaGains(events, user);
            SunSearGains(events, user);
        }
    }
}
