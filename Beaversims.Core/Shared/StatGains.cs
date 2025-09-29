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
        public static void PrimaryGainsDmg(ThroughputEvent evt, User user, StatName statName)
        {
            var stat = evt.UserStats.Get(statName);
            var gainType = GainType.Dmg;
            var gain = Calc.PrimaryGainCalc(stat, evt.Amount.Eff);
            evt.Gains[statName][gainType] += gain;
            user.Spec.DupliGainsDmg(evt, user, statName, gain);
            
        }
        public static void PrimaryGainsHeal(HealEvent evt, User user, StatName statName)
        {

            var stat = evt.UserStats.Get(statName);
            var gainType = GainType.Eff;
            var gainRaw = Calc.PrimaryGainCalc(stat, evt.Amount.Raw);
            var gain = evt.RawToEffConvert(gainRaw);
            evt.Gains[statName][gainType] += gain;
            user.Spec.DupliGainsHeal(evt, user, statName, gainRaw);

        }

        public static void VersGainsDmg(ThroughputEvent evt, User user)
        {
            var statName = StatName.Vers;
            var stat = (Vers)evt.UserStats.Get(statName);
            var gainType = GainType.Dmg;
            var gain = Calc.SecondaryGainCalc(stat, evt.Amount.Eff, stat.PercentRate);
            evt.Gains[statName][gainType] += gain;
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
            }
            if (IsTickScaler(evt))
            {
                gain += baseGain;
            }
            if (IsAutoScaler(evt))
            {
                gain += baseGain;
            }
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
            }
            if (IsTickScaler(evt))
            {
                gainRaw += baseGainRaw;
            }
            if (IsAutoScaler(evt))
            {
                gainRaw += baseGainRaw;
            }
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
                var gain = Calc.SecondaryGainCalc(vers, tpEvent.Amount.Eff, vers.DefPercentRate);
                tpEvent.Gains[statName][GainType.Def] += gain;
            }
        }
        public static void AvoidanceGains(ThroughputEvent tpEvent)
        {
            if (tpEvent.IsAvoidanceEvent())
            {
                var statName = StatName.Avoidance;
                var stat = (Avoidance)tpEvent.UserStats.Get(statName);
                var gain = Calc.SecondaryGainCalc(stat, tpEvent.Amount.Eff, stat.PercentRate);
                tpEvent.Gains[statName][GainType.Def] += gain;
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

        public static void AutoStatGainsMisc(ThroughputEvent evt)
        {
            VersDefGains(evt);
            AvoidanceGains(evt);
            SuppStamGains(evt);
        }
    }
}


