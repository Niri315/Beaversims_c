using Beaversims.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Shared
{
    // Math using altEvent amounts/stats alongside original is CORRECT. Done thorough testing and doublechecking. It looks weird but that math works out.
    // DNDC DNDC DNDC DNDC

    internal static class StatGains
    {
        public static void PrimaryAltAmount(TpEvent evt, Stat stat, int i, bool antiGain = false)
        {

            var altEvent = evt.AltEvents[i];

            var altStat = altEvent.UserStats.Get(stat.Name);


            var gainPerPrimRaw = altEvent.Amount.Raw / stat.TrueEff();
            var gainRaw = gainPerPrimRaw * (altStat.TrueEff() - stat.TrueEff());
            //if (i == 0 && evt.Timestamp > 5 && evt.Timestamp < 10)
            //{
            //    Console.WriteLine("----------");
            //    Console.WriteLine($"gainPerPrimRaw - {gainPerPrimRaw}");
            //    Console.WriteLine($"stat.Multi - {stat.Multi}");
            //    Console.WriteLine($"stat.Rating - {stat.Rating}");
            //    Console.WriteLine($" stat.TrueEff() - {stat.TrueEff()}");
            //    Console.WriteLine($"altStat.TrueEff() - {altStat.TrueEff()}");
            //    Console.WriteLine($"stat diff: {altStat.TrueEff() - stat.TrueEff()}");
            //    Console.WriteLine($" Gain raw - {gainRaw}");
            //    Console.WriteLine($" altEvent.Amount.Raw - {altEvent.Amount.Raw}");
            //}
          
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

      

        public static void SecondaryAltAmount(TpEvent evt, SecondaryStat stat, int i, double? amount=null, double mod = 1, bool antiGain = false)
        {

            var altEvent = evt.AltEvents[i];
            if (amount == null)
            {
                amount = altEvent.Amount.Raw;
            }

            var gainPerRatingRaw = Calc.SecondaryGainCalc(stat, amount.Value, stat.PercentRate);
            var gainPerEffstatRaw = stat.RemoveDryMult(gainPerRatingRaw);
            var altStat = altEvent.UserStats.Get(stat.Name);
            var gainRaw = gainPerEffstatRaw * (altStat.TrueEff() - stat.TrueEff()) * mod;
            if (antiGain)
            {
                gainRaw *= -1;

            }
            if (evt.Ability.SimDupliAbility)
            {
                altEvent.NukeRaw += gainRaw;
            }
            //if (i == 4 && (altStat.TrueEff() - stat.TrueEff() != 0 ))
            //{
            //    Console.WriteLine("----");
            //    Console.WriteLine($"altEvent.Amount.Raw: {altEvent.Amount.Raw}, stat.PercentRate: {stat.PercentRate}, (altStat.TrueEff(): {altStat.TrueEff()}, stat.TrueEff(): {stat.TrueEff()}");
            //    Console.WriteLine($"gainRaw: {gainRaw}");
            //}
            //if (gainRaw is double.NaN)
            //{
            //    Console.WriteLine(evt.AbilityName);
            //}
            altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);

        }

        public static void CritAltAmount(TpEvent evt, Crit crit, int i, bool isCrit, double critInc, bool userAbilityUhr = true, double? estNonCritValue = null, bool antiGain = false)
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
            var gainRaw = gainPerEffstatRaw * (altCrit.TrueEff() - crit.TrueEff());
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
                    gainEff = gainRaw * ability.CritUhr();
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


        public static void DefAltAmount(TpEvent evt, NonPrimaryStat stat, int i, double percentRate, bool antiGain = false)
        {
   
            var altEvent = evt.AltEvents[i];

            var gainPerRatingRaw = Calc.DefGainCalc(stat, altEvent.Amount.Raw, percentRate);
            var gainPerEffstatRaw = stat.RemoveDryMult(gainPerRatingRaw);
            var altStat = altEvent.UserStats.Get(stat.Name);
            var gainRaw = -1 * gainPerEffstatRaw * (altStat.TrueEff() - stat.TrueEff());
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

        public static void LeechAltAmount(TpEvent evt, Stat stat, int i, bool antiGain = false)
        {

            var altEvent = evt.AltEvents[i];
            var altStat = altEvent.UserStats.Get(stat.Name);
            var gainPerPrimRaw = altEvent.Amount.Raw / stat.TrueEff();
            var gainRaw = gainPerPrimRaw * (altStat.TrueEff() - stat.TrueEff());
            if (antiGain) { gainRaw *= -1; }
            altEvent.NukeRaw += gainRaw; // Always SimDupli
            altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);

        }



        public static void PrimaryGains(TpEvent evt, User user, StatName statName, int i, bool antiGain = false)
        {
            var stat = evt.UserStats.Get(statName);
            PrimaryAltAmount(evt, stat, i, antiGain: antiGain);
        }

        public static void VersGains(TpEvent evt, User user, int i, bool antiGain = false)
        {
            var statName = StatName.Vers;
            var stat = (Vers)evt.UserStats.Get(statName);
            SecondaryAltAmount(evt, stat, i, antiGain: antiGain);


        }

        public static void CritGainsDmg(TpEvent evt, User user, int i, bool antiGain = false)
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

        private static bool IsCastScaler(TpEvent tpEvent, Ability ability)
        {
            if (ability.HasteScalers.Contains(HST.Cast) && (tpEvent.SourceUnit is User || ability.IncludePetCasts))
            {
                return true;
            }
            return false;
        }

        private static bool IsTickScaler(TpEvent tpEvent) => tpEvent.Tick && tpEvent.Ability.HasteScalers.Contains(HST.Tick) && !tpEvent.NonScInstaTick;
        private static bool IsAutoScaler(TpEvent tpEvent) => tpEvent.Ability.HasteScalers.Contains(HST.Auto);


        
        public static void HasteGains(TpEvent evt, User user, int i, Ability ability = null, bool antiGain = false)
        {
            ability ??= evt.Ability;

            var statName = StatName.Haste;
            var stat = (SecondaryStat)evt.UserStats.Get(statName);

            if (IsCastScaler(evt, ability))
            {
                var QIM = ability.TrueQIM(user, i);
                double HCGM;
                if (evt.IsDmgDoneEvent())
                {
                    HCGM = ability.TrueDmgHCGM(user);
                }
                else
                {
                    HCGM = ability.TrueHealHCGM(user);
                }

                SecondaryAltAmount(evt, stat, i, mod: QIM * HCGM * ability.HasteGainMod * user.Spec.HasteGainMod, antiGain: antiGain);

            }
            if (IsTickScaler(evt))
            {
                SecondaryAltAmount(evt, stat, i, mod: ability.HasteGainMod * user.Spec.HasteGainMod, antiGain: antiGain);

            }
            if (IsAutoScaler(evt))
            {
                double autoMod = 0.0;
                if (evt.IsDmgDoneEvent())
                {
                    autoMod = ability.HasteAutoModDmg;
                }
                else
                {
                    autoMod = ability.HasteAutoModHeal;
                }
                SecondaryAltAmount(evt, stat, i, mod: autoMod * ability.HasteGainMod * user.Spec.HasteGainMod, antiGain: antiGain);

            }
        }

        public static void VersDefGains(TpEvent tpEvent, int i, bool antiGain = false)
        {
            if (tpEvent.IsDrEvent())
            {
                var statName = StatName.Vers;
                var vers = (Vers)tpEvent.UserStats.Get(statName);
                DefAltAmount(tpEvent, vers, i, vers.DefPercentRate, antiGain: antiGain);

            }
        }
        public static void AvoidanceGains(TpEvent tpEvent, int i, bool antiGain = false)
        {
            if (tpEvent.IsAvoidanceEvent())
            {
                var statName = StatName.Avoidance;
                var stat = (Avoidance)tpEvent.UserStats.Get(statName);
                DefAltAmount(tpEvent, stat, i, stat.PercentRate, antiGain: antiGain);
            }
        }

        public static void SuppStamGains(TpEvent tpEvent, int i, bool antiGain = false)
        {
            var statName = StatName.Stamina;
            var ability = tpEvent.Ability;
            if (ability.SuppStamScaler && tpEvent.TargetUnit is User)
            {
                var stat = (Stamina)tpEvent.UserStats.Get(statName);
                PrimaryAltAmount(tpEvent, stat, i, antiGain: antiGain);

            }
        }
      
 

        public static void LeechGains_simple(TpEvent evt, int i, bool antiGain = false)
        {
            if (evt.IsHealDoneEvent() && evt.AbilityName == Abilities.Leech.name)
            {
                var leechStat = evt.UserStats.Get(StatName.Leech);

                LeechAltAmount(evt, leechStat, i, antiGain: antiGain);  // Might as well just do the same as with primary stats.
            }
        }

        public static void LeechGains_adv(TpEvent evt, User user, int i)
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
            double sourceHitRaw;
            double sourceCritRaw;
            double sourceAmountRaw;

            if (ability.ReverseEffect) 
            {
                critInc = crit.IncHeal + sourceAbility.BonusCritIncHeal;
                sourceHitRaw = sourceAbility.Heal.Hit.Raw;
                sourceCritRaw = sourceAbility.Heal.Crit.Raw;
                sourceAmountRaw = sourceAbility.Heal.Raw;
            }
            else 
            { 
                critInc = crit.IncDmg + sourceAbility.BonusCritIncDmg;
                sourceHitRaw = sourceAbility.Damage.Hit.Dmg;
                sourceCritRaw = sourceAbility.Damage.Crit.Dmg;
                sourceAmountRaw = sourceAbility.Damage.Dmg;
            }

            var estNonCritAmount = evt.Amount.Eff * ((sourceHitRaw + (sourceCritRaw / critInc)) / sourceAmountRaw);


            CritAltAmount(evt, crit, i, false, critInc, userAbilityUhr:false, estNonCritValue:estNonCritAmount, antiGain: antiGain);

        }
        public static void CritGainsHealDerived(HealEvent evt, User user, int i, Ability ability = null, Ability sourceAbility = null, bool antiGain = false)
        {

            ability ??= evt.Ability;
            sourceAbility ??= user.Abilities.Get(ability.SourceAbility);

            var statName = StatName.Crit;
            var crit = (Crit)evt.UserStats.Get(statName);
            double critInc;
            double sourceHitRaw;
            double sourceCritRaw;
            double sourceAmountRaw;


            if (ability.ReverseEffect) 
            { 
                critInc = crit.IncDmg + sourceAbility.BonusCritIncDmg;
                sourceHitRaw = sourceAbility.Damage.Hit.Dmg;
                sourceCritRaw = sourceAbility.Damage.Crit.Dmg;
                sourceAmountRaw = sourceAbility.Damage.Dmg;
            }
            else 
            { 
                critInc = crit.IncHeal + sourceAbility.BonusCritIncHeal;
                sourceHitRaw = sourceAbility.Heal.Hit.Raw;
                sourceCritRaw = sourceAbility.Heal.Crit.Raw;
                sourceAmountRaw = sourceAbility.Heal.Raw;
            }

            var estNonCritAmount = evt.Amount.Raw * ((sourceHitRaw + (sourceCritRaw / critInc)) / sourceAmountRaw);

   

            CritAltAmount(evt, crit, i, false, critInc, userAbilityUhr: false, estNonCritValue: estNonCritAmount, antiGain: antiGain);

        }
        public static void AutoStatGains(TpEvent evt, User user, int i)
        {
            if (evt.IsHealDoneEvent() || evt.IsDmgDoneEvent())
            {
                var ability = evt.Ability;
                if (ability.ScalesWith(StatName.Intellect))
                {
                    PrimaryGains(evt, user, StatName.Intellect, i);
                }
                if (ability.ScalesWith(StatName.Stamina))
                {
                    PrimaryGains(evt, user, StatName.Stamina, i);
                }
                if (ability.ScalesWith(StatName.Vers))
                {
                    VersGains(evt, user, i);
                }
                if (ability.ScalesWith(StatName.Crit) && !evt.Ability.DerivedCritScaler)
                {
                    if (evt.IsDmgDoneEvent())
                    {
                        CritGainsDmg(evt, user, i);

                    }
                    else if (evt.IsHealDoneEvent())
                    {
                        CritGainsHeal((HealEvent)evt, user, i);
                    }
                }
                if (ability.ScalesWith(StatName.Haste))
                {
                    HasteGains(evt, user, i);
                }
                if (evt.Ability.DerivedCritScaler && evt.IsDmgDoneEvent())
                {
                    CritGainsDmgDerived((DamageEvent)evt, user, i);
                }
                if (evt.Ability.DerivedCritScaler && evt.IsHealDoneEvent())
                {
                    CritGainsHealDerived((HealEvent)evt, user, i);
                }
            }
            VersDefGains(evt, i);
            AvoidanceGains(evt, i);
            SuppStamGains(evt, i);
            if (user.Abilities.Get(Shared.Abilities.Leech.name).Heal.Raw > 0)
            {
                LeechGains_simple(evt, i);
            }
            {
                LeechGains_adv(evt, user, i);
            }
        }
    }
}


