using Beaversims.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Shared
{
    internal static class StatGains
    {

        public static void PrimaryAltAmount(ThroughputEvent evt, Stat stat, int i, bool antiGain = false)
        {

            var altEvent = evt.AltEvents[i];

            var altStat = altEvent.UserStats.Get(stat.Name);
            var gainPerPrimRaw = altEvent.Amount.Raw / stat.Eff;
            var gainRaw = gainPerPrimRaw * (altStat.Eff - stat.Eff);
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

        public static void SecondaryAltAmount(ThroughputEvent evt, SecondaryStat stat, int i, double mod = 1, bool antiGain = false)
        {

            var altEvent = evt.AltEvents[i];
            var gainPerRatingRaw = Calc.SecondaryGainCalc(stat, altEvent.Amount.Raw, stat.PercentRate);
            var gainPerEffstatRaw = stat.RemoveDryMult(gainPerRatingRaw);
            var altStat = altEvent.UserStats.Get(stat.Name);
            var gainRaw = gainPerEffstatRaw * (altStat.Eff - stat.Eff) * mod;
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

        public static void CritAltAmount(ThroughputEvent evt, Crit crit, int i, bool isCrit, double critInc, bool userAbilityUhr = true, double? estNonCritValue = null, bool antiGain = false)
        {
            var ability = evt.Ability;

            double amount;
            var altEvent = evt.AltEvents[i];
            if (estNonCritValue != null)
            {
                amount = estNonCritValue.Value;
            }
            else
            {
                amount = altEvent.Amount.Raw;
            }
            var gainPerRatingRaw = Calc.CritGainCalc(crit, amount, isCrit, critInc);
            var gainPerEffstatRaw = crit.RemoveDryMult(gainPerRatingRaw);
            var altCrit = altEvent.UserStats.Get(crit.Name);
            var gainRaw = gainPerEffstatRaw * (altCrit.Eff - crit.Eff);
            if (antiGain)
            {
                gainRaw *= -1;

            }
            if (evt.Ability.SimDupliAbility)
            {
                altEvent.NukeRaw += gainRaw;
            }
            var gainEff = 0.0;

            if (userAbilityUhr)
            {
                if (evt.IsHealDoneEvent())
                {
                    gainEff = gainRaw * ability.CritUr();
                }
                else
                {
                    gainEff = gainRaw;
                }

                var gainNaraw = evt.AltRawToNarawConvert(gainRaw, i);
                var gainNaeff = evt.AltEffToNaeffConvert(gainEff, i);
                altEvent.Amount.Raw += gainRaw;
                altEvent.Amount.Eff += gainEff;
                altEvent.Amount.Naeff += gainNaeff;
                altEvent.Amount.Naraw += gainNaraw;

            }
            else
            {
                altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);

            }
            //if (i == 8 && altEvent.Amount.Eff < 0 && evt is HealEvent)
            //{
            //    Console.WriteLine(evt.AbilityName);
            //    Console.WriteLine(altEvent.Amount.Raw);
            //    Console.WriteLine(gainEff);
            //    Console.WriteLine(gainEff);
            //    Console.WriteLine(altEvent.Amount.Eff);
            //}
        }


        public static void DefAltAmount(ThroughputEvent evt, NonPrimaryStat stat, int i, double percentRate, bool antiGain = false)
        {
   
            var altEvent = evt.AltEvents[i];

            var gainPerRatingRaw = Calc.DefGainCalc(stat, altEvent.Amount.Raw, percentRate);
            var gainPerEffstatRaw = stat.RemoveDryMult(gainPerRatingRaw);
            var altStat = altEvent.UserStats.Get(stat.Name);
            var gainRaw = -1 * gainPerEffstatRaw * (altStat.Eff - stat.Eff);
            if (antiGain) { 
                gainRaw *= -1; 
            }
            if (evt.Ability.SimDupliAbility)
            {
                altEvent.NukeRaw += gainRaw;
            }
            //Console.WriteLine(gainRaw);
            altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);
            
        }

        public static void LeechAltAmount(ThroughputEvent evt, Stat stat, int i, bool antiGain = false)
        {

            var altEvent = evt.AltEvents[i];
            var altStat = altEvent.UserStats.Get(stat.Name);
            var gainPerPrimRaw = altEvent.Amount.Raw / stat.Eff;
            var gainRaw = gainPerPrimRaw * (altStat.Eff - stat.Eff);
            if (antiGain) { gainRaw *= -1; }
            altEvent.NukeRaw += gainRaw; // Always SimDupli
            altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);

        }


        public static void PrimaryGainsDmg(ThroughputEvent evt, User user, StatName statName, int i, bool antiGain = false)
        {
            var stat = evt.UserStats.Get(statName);
            var gainType = GainType.Dmg;
            var gain = Calc.PrimaryGainCalc(stat, evt.Amount.Eff);
            evt.Gains[statName][gainType] += gain;

            PrimaryAltAmount(evt, stat, i, antiGain: antiGain);

            user.Spec.DupliGainsDmg(evt, user, statName, gain);

        }

        public static void PrimaryGainsHeal(HealEvent evt, User user, StatName statName, int i, bool antiGain = false)
        {

            var stat = evt.UserStats.Get(statName);
            var gainType = GainType.Eff;
            var gainRaw = Calc.PrimaryGainCalc(stat, evt.Amount.Raw);
            var gain = evt.RawToEffConvert(gainRaw);
            evt.Gains[statName][gainType] += gain;

            PrimaryAltAmount(evt, stat, i, antiGain: antiGain);

            user.Spec.DupliGainsHeal(evt, user, statName, gainRaw);

        }

        public static void VersGainsDmg(ThroughputEvent evt, User user, int i, bool antiGain = false)
        {
            var statName = StatName.Vers;
            var stat = (Vers)evt.UserStats.Get(statName);
            SecondaryAltAmount(evt, stat, i, antiGain: antiGain);

        }

        public static void VersGainsHeal(HealEvent evt, User user, int i, bool antiGain = false)
        {
            var statName = StatName.Vers;

            var stat = (Vers)evt.UserStats.Get(statName);

            SecondaryAltAmount(evt, stat, i, antiGain: antiGain);


        }

        public static void CritGainsDmg(ThroughputEvent evt, User user, int i, bool antiGain = false)
        {
            var statName = StatName.Crit;
            var ability = evt.Ability;
            var crit = (Crit)evt.UserStats.Get(statName);
            var isCrit = evt.Crit;
            double critInc;
            if (ability.ReverseEffect) { critInc = crit.IncHeal + ability.BonusCritIncHeal; }
            else { critInc = crit.IncDmg + ability.BonusCritIncDmg; }
            CritAltAmount(evt, crit, i, isCrit, critInc, antiGain: antiGain);




        }
        public static void CritGainsHeal(HealEvent evt, User user, int i, bool antiGain = false)
        {
            var statName = StatName.Crit;
            var ability = evt.Ability;

            var crit = (Crit)evt.UserStats.Get(statName);
            var isCrit = evt.Crit;
            double critInc;
            if (ability.ReverseEffect) { critInc = crit.IncDmg + ability.BonusCritIncDmg; }
            else { critInc = crit.IncHeal + ability.BonusCritIncHeal; }
            CritAltAmount(evt, crit, i, isCrit, critInc, antiGain: antiGain);



        }

        private static bool IsCastScaler(ThroughputEvent tpEvent, Ability ability)
        {
            if (ability.HasteScalers.Contains(HST.Cast) && tpEvent.SourceUnit is User)
            {
                return true;
            }
            return false;
        }

        private static bool IsTickScaler(ThroughputEvent tpEvent) => tpEvent.Tick && tpEvent.Ability.HasteScalers.Contains(HST.Tick);
        private static bool IsAutoScaler(ThroughputEvent tpEvent) => tpEvent.Ability.HasteScalers.Contains(HST.Auto);

        public static void HasteGainsDmg(ThroughputEvent evt, User user, int i, Ability ability = null, bool antiGain = false)
        {
            ability ??= evt.Ability;
            var statName = StatName.Haste;
            var stat = (SecondaryStat)evt.UserStats.Get(statName);

            if (IsCastScaler(evt, ability))
            {
                SecondaryAltAmount(evt, stat, i, mod: user.HCGM * ability.HCCGM * ability.HasteCastGainMod * ability.HasteGainMod * user.Spec.HasteGainMod, antiGain: antiGain);
            }
            if (IsTickScaler(evt))
            {
                SecondaryAltAmount(evt, stat, i, mod: ability.HasteGainMod * user.Spec.HasteGainMod, antiGain: antiGain);
            }
            if (IsAutoScaler(evt))
            {
                SecondaryAltAmount(evt, stat, i, mod: ability.HasteGainMod * user.Spec.HasteGainMod, antiGain: antiGain);
            }



        }
        public static void HasteGainsHeal(HealEvent evt, User user, int i, Ability ability = null, bool antiGain = false)
        {
            ability ??= evt.Ability;

            var statName = StatName.Haste;
            var stat = (SecondaryStat)evt.UserStats.Get(statName);

            if (IsCastScaler(evt, ability))
            {
                SecondaryAltAmount(evt, stat, i, mod: user.HCGM * ability.HCCGM * ability.HasteCastGainMod * ability.HasteGainMod * user.Spec.HasteGainMod, antiGain: antiGain);

            }
            if (IsTickScaler(evt))
            {
                SecondaryAltAmount(evt, stat, i, mod: ability.HasteGainMod * user.Spec.HasteGainMod, antiGain: antiGain);

            }
            if (IsAutoScaler(evt))
            {
                SecondaryAltAmount(evt, stat, i, mod: ability.HasteGainMod * user.Spec.HasteGainMod, antiGain: antiGain);

            }
        }

        public static void VersDefGains(ThroughputEvent tpEvent, int i, bool antiGain = false)
        {
            if (tpEvent.IsDrEvent())
            {
                var statName = StatName.Vers;
                var vers = (Vers)tpEvent.UserStats.Get(statName);
                DefAltAmount(tpEvent, vers, i, vers.DefPercentRate, antiGain: antiGain);

            }
        }
        public static void AvoidanceGains(ThroughputEvent tpEvent, int i, bool antiGain = false)
        {
            if (tpEvent.IsAvoidanceEvent())
            {
                var statName = StatName.Avoidance;
                var stat = (Avoidance)tpEvent.UserStats.Get(statName);
                DefAltAmount(tpEvent, stat, i, stat.PercentRate, antiGain: antiGain);
            }
        }

        public static void SuppStamGains(ThroughputEvent tpEvent, int i, bool antiGain = false)
        {
            var statName = StatName.Stamina;
            var ability = tpEvent.Ability;
            if (ability.SuppStamScaler && tpEvent.TargetUnit is User)
            {
                var stat = (Stamina)tpEvent.UserStats.Get(statName);
                PrimaryAltAmount(tpEvent, stat, i, antiGain: antiGain);

            }
        }
        public static void AutoStatGainsHeal(HealEvent evt, User user, int i)
        {
            var ability = evt.Ability;

            if (ability.ScalesWith(StatName.Intellect))
            {
                PrimaryGainsHeal(evt, user, StatName.Intellect, i);
            }

            if (ability.ScalesWith(StatName.Stamina))
            {
                PrimaryGainsHeal(evt, user, StatName.Stamina, i);
            }
            if (ability.ScalesWith(StatName.Vers))
            {
                VersGainsHeal(evt, user, i);
            }
            if (ability.ScalesWith(StatName.Crit))
            {
                CritGainsHeal(evt, user, i);
            }
            if (ability.ScalesWith(StatName.Haste))
            {
                HasteGainsHeal(evt, user, i);
            }


        }
        public static void AutoStatGainsDmg(DamageEvent evt, User user, int i)
        {
            var ability = evt.Ability;
            if (ability.ScalesWith(StatName.Intellect))
            {
                PrimaryGainsDmg(evt, user, StatName.Intellect, i);
            }
            if (ability.ScalesWith(StatName.Stamina))
            {
                PrimaryGainsDmg(evt, user, StatName.Stamina, i);
            }
            if (ability.ScalesWith(StatName.Vers))
            {
                VersGainsDmg(evt, user, i);
            }
            if (ability.ScalesWith(StatName.Crit))
            {
                CritGainsDmg(evt, user, i);
            }
            if (ability.ScalesWith(StatName.Haste))
            {
                HasteGainsDmg(evt, user, i);
            }
        }


        public static void LeechGains_simple(ThroughputEvent evt, int i, bool antiGain = false)
        {
            if (evt.IsHealDoneEvent() && evt.AbilityName == Abilities.Leech.name)
            {
                var leechStat = evt.UserStats.Get(StatName.Leech);

                LeechAltAmount(evt, leechStat, i, antiGain: antiGain);  // Might as well just do the same as with primary stats.
            }
        }

        public static void LeechGains_adv(ThroughputEvent evt, User user)
        {
            //if (Shared.DupliEffects.IsLeechSourceEvent(evt))
            //{
            //    var leechStat = (Leech)evt.UserStats.Get(StatName.Leech);
            //    var leechAbility = user.Abilities.Get(Abilities.Leech.name);
            //    var gain = (evt.Amount.Naraw / (leechStat.PercentRate * 100)) * leechStat.Multi * leechAbility.HypoTrueUr();
            //    evt.Gains[StatName.Leech][GainType.Eff] *= gain;
            //}
        }

        public static void CritGainsDmgDerived(DamageEvent evt, User user, int i, Ability ability = null, Ability sourceAbility = null, bool antiGain = false)
        {

            ability ??= evt.Ability;
            sourceAbility ??= user.Abilities.Get(ability.SourceAbility);

            var statName = StatName.Crit;
            var crit = (Crit)evt.UserStats.Get(statName);
            double critInc;

            if (ability.ReverseEffect) { critInc = crit.IncHeal + sourceAbility.BonusCritIncHeal; }
            else { critInc = crit.IncDmg + sourceAbility.BonusCritIncDmg; }

            var estNonCritAmount = evt.Amount.Eff * ((sourceAbility.Damage.Hit.Dmg + (sourceAbility.Damage.Crit.Dmg / critInc)) / sourceAbility.Damage.Dmg);


            CritAltAmount(evt, crit, i, false, critInc, userAbilityUhr:false, estNonCritValue:estNonCritAmount, antiGain: antiGain);

        }
        public static void CritGainsHealDerived(HealEvent evt, User user, int i, Ability ability = null, Ability sourceAbility = null, bool antiGain = false)
        {

            ability ??= evt.Ability;
            sourceAbility ??= user.Abilities.Get(ability.SourceAbility);

            var statName = StatName.Crit;
            var crit = (Crit)evt.UserStats.Get(statName);
            double critInc;

            if (ability.ReverseEffect) { critInc = crit.IncDmg + sourceAbility.BonusCritIncDmg; }
            else { critInc = crit.IncHeal + sourceAbility.BonusCritIncHeal; }

            var estNonCritAmount = evt.Amount.Raw * ((sourceAbility.Heal.Hit.Raw + (sourceAbility.Heal.Crit.Raw / critInc))/ sourceAbility.Heal.Raw);;

   

            CritAltAmount(evt, crit, i, false, critInc, userAbilityUhr: false, estNonCritValue: estNonCritAmount, antiGain: antiGain);

        }

        public static void AutoStatGainsMisc(ThroughputEvent evt, User user, int i)
        {
            VersDefGains(evt, i);
            AvoidanceGains(evt, i);
            SuppStamGains(evt, i);
            if (evt.Ability.DerivedCritScaler && evt.IsDmgDoneEvent())
            {
                CritGainsDmgDerived((DamageEvent)evt, user, i);
            }
            if (evt.Ability.DerivedCritScaler && evt.IsHealDoneEvent())
            {
                CritGainsHealDerived((HealEvent)evt, user, i);
            }
            if (user.HasPermaLeech)
            {
                LeechGains_simple(evt, i);
            }
            else
            {

            }
        }
    }
}


