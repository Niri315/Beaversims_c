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

        public static void PrimaryAltAmount(ThroughputEvent evt, Stat stat)
        {
            foreach (var altEvent in evt.AltEvents)
            {
                var altStat = altEvent.UserStats.Get(stat.Name);
                var gainPerPrimRaw = altEvent.Amount.Raw / stat.Eff;
                var gainRaw = gainPerPrimRaw * (altStat.Eff - stat.Eff);
                altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw);
            }
        }

        public static void SecondaryAltAmount(ThroughputEvent evt, SecondaryStat stat, double mod = 1)
        {
            foreach (var altEvent in evt.AltEvents)
            {
                var gainPerRatingRaw = Calc.SecondaryGainCalc(stat, altEvent.Amount.Raw, stat.PercentRate);
                var gainPerEffstatRaw = stat.RemoveDryMult(gainPerRatingRaw);
                var altStat = altEvent.UserStats.Get(stat.Name);
                var gainRaw = gainPerEffstatRaw * (altStat.Eff - stat.Eff) * mod;
                altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw);
            }
        }

        public static void CritAltAmount(ThroughputEvent evt, Crit crit, bool isCrit, double critInc)
        {
            var ability = evt.Ability;
            foreach (var altEvent in evt.AltEvents)
            {
                var gainPerRatingRaw = Calc.CritGainCalc(crit, altEvent.Amount.Raw, isCrit, critInc);
                var gainPerEffstatRaw = crit.RemoveDryMult(gainPerRatingRaw);
                var altCrit = altEvent.UserStats.Get(crit.Name);
                var gainRaw = gainPerEffstatRaw * (altCrit.Eff - crit.Eff);
                var gainEff = gainRaw * ability.CritUr();
                altEvent.Amount.Raw += gainRaw;
                altEvent.Amount.Eff += gainEff;  // Get UHR from ability - naraw and naeff should be fine to get from evt data.
                altEvent.Amount.Naraw += evt.RawToNarawConvert(gainRaw);
                altEvent.Amount.Naeff += evt.EffToNaeffConvert(gainEff);
            }
        }


        public static void DefAltAmount(ThroughputEvent evt, NonPrimaryStat stat, double percentRate)
        {
            //todo eff/naraw etc.
            foreach (var altEvent in evt.AltEvents)
            {

                var gainPerRatingRaw = Calc.DefGainCalc(stat, altEvent.Amount.Raw, percentRate);
                var gainPerEffstatRaw = stat.RemoveDryMult(gainPerRatingRaw);
                var altStat = altEvent.UserStats.Get(stat.Name);
                var gainRaw = -1 * gainPerEffstatRaw * (altStat.Eff - stat.Eff);
                altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw);
            }
        }




        public static void PrimaryGainsDmg(ThroughputEvent evt, User user, StatName statName)
        {
            var stat = evt.UserStats.Get(statName);
            var gainType = GainType.Dmg;
            var gain = Calc.PrimaryGainCalc(stat, evt.Amount.Eff);
            evt.Gains[statName][gainType] += gain;

            PrimaryAltAmount(evt, stat);

            user.Spec.DupliGainsDmg(evt, user, statName, gain);

        }

        public static void PrimaryGainsHeal(HealEvent evt, User user, StatName statName)
        {

            var stat = evt.UserStats.Get(statName);
            var gainType = GainType.Eff;
            var gainRaw = Calc.PrimaryGainCalc(stat, evt.Amount.Raw);
            var gain = evt.RawToEffConvert(gainRaw);
            evt.Gains[statName][gainType] += gain;

            PrimaryAltAmount(evt, stat);

            user.Spec.DupliGainsHeal(evt, user, statName, gainRaw);

        }

        public static void VersGainsDmg(ThroughputEvent evt, User user)
        {
            var statName = StatName.Vers;
            var stat = (Vers)evt.UserStats.Get(statName);
            var gainType = GainType.Dmg;
            var gain = Calc.SecondaryGainCalc(stat, evt.Amount.Eff, stat.PercentRate);
            evt.Gains[statName][gainType] += gain;

            SecondaryAltAmount(evt, stat);
            

            user.Spec.DupliGainsDmg(evt, user, statName, gain);



        }
        public static void VersGainsHeal(HealEvent evt, User user)
        {
            var statName = StatName.Vers;

            var stat = (Vers)evt.UserStats.Get(statName);
            var gainType = GainType.Eff;
            var gainRaw = Calc.SecondaryGainCalc(stat, evt.Amount.Raw, stat.PercentRate);
            var gain = evt.RawToEffConvert(gainRaw);
            evt.Gains[statName][gainType] += gain;

            SecondaryAltAmount(evt, stat);

            user.Spec.DupliGainsHeal(evt, user, statName, gainRaw);

        }

        public static void CritGainsDmg(ThroughputEvent evt, User user)
        {
            var statName = StatName.Crit;
            var ability = evt.Ability;

            var crit = (Crit)evt.UserStats.Get(statName);
            var isCrit = evt.Crit;
            double critInc;
            var gainType = GainType.Dmg;
            if (ability.ReverseEffect) { critInc = crit.IncHeal + ability.BonusCritIncHeal; }
            else { critInc = crit.IncDmg + ability.BonusCritIncDmg; }
            var gain = Calc.CritGainCalc(crit, evt.Amount.Eff, isCrit, critInc);

            evt.Gains[statName][gainType] += gain;

            CritAltAmount(evt, crit, isCrit, critInc);

            user.Spec.DupliGainsDmg(evt, user, statName, gain);


        }
        public static void CritGainsHeal(HealEvent evt, User user)
        {
            var statName = StatName.Crit;
            var ability = evt.Ability;

            var crit = (Crit)evt.UserStats.Get(statName);
            var isCrit = evt.Crit;
            double critInc;
            var gainType = GainType.Eff;
            if (ability.ReverseEffect) { critInc = crit.IncDmg + ability.BonusCritIncDmg; }
            else { critInc = crit.IncHeal + ability.BonusCritIncHeal; }
            var gainRaw = Calc.CritGainCalc(crit, evt.Amount.Raw, isCrit, critInc);
            var gain = gainRaw * ability.CritUr();

            evt.Gains[statName][gainType] += gain;

            CritAltAmount(evt, crit, isCrit, critInc);

            user.Spec.DupliGainsHeal(evt, user, statName, gainRaw);


        }

        private static bool IsCastScaler(ThroughputEvent tpEvent)
        {
            if (tpEvent.Ability.HasteScalers.Contains(HST.Cast) && tpEvent.SourceUnit is User)
            {
                return true;
            }
            return false;
        }

        private static bool IsTickScaler(ThroughputEvent tpEvent) => tpEvent.Tick && tpEvent.Ability.HasteScalers.Contains(HST.Tick);
        private static bool IsAutoScaler(ThroughputEvent tpEvent) => tpEvent.Ability.HasteScalers.Contains(HST.Auto);

        public static void HasteGainsDmg(ThroughputEvent evt, User user)
        {
            var ability = evt.Ability;
            var statName = StatName.Haste;
            var stat = (SecondaryStat)evt.UserStats.Get(statName);
            var baseGain = Calc.SecondaryGainCalc(stat, evt.Amount.Eff, stat.PercentRate);
            double gain = 0.0;

            var gainType = GainType.Dmg;
            if (IsCastScaler(evt))
            {
                gain += baseGain * user.HCGM * ability.HGCM;
                SecondaryAltAmount(evt, stat, mod: user.HCGM * ability.HGCM * ability.HasteGainMod * user.Spec.HasteGainMod);
            }
            if (IsTickScaler(evt))
            {
                gain += baseGain;
                SecondaryAltAmount(evt, stat, mod: ability.HasteGainMod * user.Spec.HasteGainMod);
            }
            if (IsAutoScaler(evt))
            {
                gain += baseGain;
                SecondaryAltAmount(evt, stat, mod: ability.HasteGainMod * user.Spec.HasteGainMod);
            }

            gain *= ability.HasteGainMod * user.Spec.HasteGainMod;


            evt.Gains[statName][gainType] += gain;

            user.Spec.DupliGainsDmg(evt, user, statName, gain);


        }
        public static void HasteGainsHeal(HealEvent evt, User user)
        {
            var ability = evt.Ability;
            var statName = StatName.Haste;
            var stat = (SecondaryStat)evt.UserStats.Get(statName);
            var baseGainRaw = Calc.SecondaryGainCalc(stat, evt.Amount.Raw, stat.PercentRate);
            double gainRaw = 0.0;
            var gainType = GainType.Eff;

            if (IsCastScaler(evt))
            {
                gainRaw += baseGainRaw * user.HCGM * ability.HGCM;
                SecondaryAltAmount(evt, stat, mod: user.HCGM * ability.HGCM * ability.HasteGainMod * user.Spec.HasteGainMod);

            }
            if (IsTickScaler(evt))
            {
                gainRaw += baseGainRaw;
                SecondaryAltAmount(evt, stat, mod: ability.HasteGainMod * user.Spec.HasteGainMod);

            }
            if (IsAutoScaler(evt))
            {
                gainRaw += baseGainRaw;
                SecondaryAltAmount(evt, stat, mod: ability.HasteGainMod * user.Spec.HasteGainMod);

            }
            gainRaw *= ability.HasteGainMod * user.Spec.HasteGainMod;
            
            var gain = evt.RawToEffConvert(gainRaw);
            evt.Gains[statName][gainType] += gain;
            user.Spec.DupliGainsHeal(evt, user, statName, gainRaw);

        }

        public static void VersDefGains(ThroughputEvent tpEvent)
        {
            if (tpEvent.IsDrEvent())
            {
                var statName = StatName.Vers;
                var vers = (Vers)tpEvent.UserStats.Get(statName);
                var gain = Calc.DefGainCalc(vers, tpEvent.Amount.Eff, vers.DefPercentRate);
                tpEvent.Gains[statName][GainType.Def] += gain;

                DefAltAmount(tpEvent, vers, vers.DefPercentRate);

            }
        }
        public static void AvoidanceGains(ThroughputEvent tpEvent)
        {
            if (tpEvent.IsAvoidanceEvent())
            {
                var statName = StatName.Avoidance;
                var stat = (Avoidance)tpEvent.UserStats.Get(statName);
                var gain = Calc.DefGainCalc(stat, tpEvent.Amount.Eff, stat.PercentRate);
                tpEvent.Gains[statName][GainType.Def] += gain;

                DefAltAmount(tpEvent, stat, stat.PercentRate);
            }
        }

        public static void SuppStamGains(ThroughputEvent tpEvent)
        {
            var statName = StatName.Stamina;
            var ability = tpEvent.Ability;
            if (ability.SuppStamScaler && tpEvent.TargetUnit is User)
            {
                GainType gainType;
                if (tpEvent.UserSuperSource)
                {
                    gainType = GainType.Eff;
                }
                else
                {
                    gainType = GainType.SupEff;
                }
                var stat = (Stamina)tpEvent.UserStats.Get(statName);
                var gain = Calc.PrimaryGainCalc(stat, tpEvent.Amount.Eff);
                tpEvent.Gains[statName][gainType] += gain;

                PrimaryAltAmount(tpEvent, stat);

            }
        }
        public static void AutoStatGainsHeal(HealEvent evt, User user)
        {
            var ability = evt.Ability;

            if (ability.ScalesWith(StatName.Intellect))
            {
                PrimaryGainsHeal(evt, user, StatName.Intellect);
            }
            if (ability.ScalesWith(StatName.Stamina))
            {
                PrimaryGainsHeal(evt, user, StatName.Stamina);
            }
            if (ability.ScalesWith(StatName.Vers))
            {
                VersGainsHeal(evt, user);
            }
            if (ability.ScalesWith(StatName.Crit))
            {
                CritGainsHeal(evt, user);
            }
            if (ability.ScalesWith(StatName.Haste))
            {
                HasteGainsHeal(evt, user);
            }
        }
        public static void AutoStatGainsDmg(DamageEvent evt, User user)
        {
            var ability = evt.Ability;
            if (ability.ScalesWith(StatName.Intellect))
            {
                PrimaryGainsDmg(evt, user, StatName.Intellect);
            }
            if (ability.ScalesWith(StatName.Stamina))
            {
                PrimaryGainsDmg(evt, user, StatName.Stamina);
            }
            if (ability.ScalesWith(StatName.Vers))
            {
                VersGainsDmg(evt, user);
            }
            if (ability.ScalesWith(StatName.Crit))
            {
                CritGainsDmg(evt, user);
            }
            if (ability.ScalesWith(StatName.Haste))
            {
                HasteGainsDmg(evt, user);
            }
        }


        public static void LeechGains_simple(ThroughputEvent evt)
        {
            if (evt.IsHealDoneEvent() && evt.AbilityName == Abilities.Leech.name)
            {
                var leechStat = evt.UserStats.Get(StatName.Leech);
                var gain = evt.Amount.Eff / leechStat.Eff;
                evt.Gains[StatName.Leech][GainType.Eff] += gain;
                PrimaryAltAmount(evt, leechStat);  // Might as well just do the same as with primary stats.
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

        public static void CritGainsDmgDerived(DamageEvent evt, User user)
        {

            var ability = evt.Ability;

          
            var sourceAbility = user.Abilities.Get(ability.SourceAbility);
            var statName = StatName.Crit;
            var crit = (Crit)evt.UserStats.Get(statName);
            double critInc;
            var gainType = GainType.Dmg;

            if (ability.ReverseEffect) { critInc = crit.IncHeal + sourceAbility.BonusCritIncHeal; }
            else { critInc = crit.IncDmg + sourceAbility.BonusCritIncDmg; }

            var estNonCritAmount = evt.Amount.Eff * ((sourceAbility.Damage.Hit.Dmg + (sourceAbility.Damage.Crit.Dmg / critInc)) / sourceAbility.Damage.Dmg);
            var gain = Calc.CritGainCalc(crit, estNonCritAmount, false, critInc);

            evt.Gains[statName][gainType] += gain;
            user.Spec.DupliGainsHeal(evt, user, statName, gain);

            CritAltAmount(evt, crit, false, critInc);

        }
        public static void CritGainsHealDerived(HealEvent evt, User user)
        {
           
            var ability = evt.Ability;


            var sourceAbility = user.Abilities.Get(ability.SourceAbility);
            var statName = StatName.Crit;
            var crit = (Crit)evt.UserStats.Get(statName);
            double critInc;
            var gainType = GainType.Eff;

            if (ability.ReverseEffect) { critInc = crit.IncDmg + sourceAbility.BonusCritIncDmg; }
            else { critInc = crit.IncHeal + sourceAbility.BonusCritIncHeal; }

            var estNonCritAmount = evt.Amount.Raw * ((sourceAbility.Heal.Hit.Raw + (sourceAbility.Heal.Crit.Raw / critInc))/ sourceAbility.Heal.Raw);;

            var gainRaw = Calc.CritGainCalc(crit, estNonCritAmount, false, critInc);
            var gain = gainRaw * ability.CritUr();

            evt.Gains[statName][gainType] += gain;
            user.Spec.DupliGainsHeal(evt, user, statName, gainRaw);

            CritAltAmount(evt, crit, false, critInc);

        }

        public static void AutoStatGainsMisc(ThroughputEvent evt, User user)
        {
            VersDefGains(evt);
            AvoidanceGains(evt);
            SuppStamGains(evt);
            if (evt.Ability.DerivedCritScaler && evt.IsDmgDoneEvent())
            {
                CritGainsDmgDerived((DamageEvent)evt, user);
            }
            if (evt.Ability.DerivedCritScaler && evt.IsHealDoneEvent())
            {
                CritGainsHealDerived((HealEvent)evt, user);
            }
            if (user.HasPermaLeech)
            {
                LeechGains_simple(evt);
            }
            else
            {

            }
        }
    }
}


