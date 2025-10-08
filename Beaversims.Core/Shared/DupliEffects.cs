using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Beaversims.Core.Shared
{
    internal class DupliEffects
    {
        public static bool IsLeechSourceEvent(ThroughputEvent evt) => !evt.FullyAbsorbed &&
            evt.TargetUnit is not User &&
            evt.SourceUnit is User &&
            evt.Ability.LeechSource &&
            evt.Ability.CanDupli;

        public static void LeechHypo(ThroughputEvent evt, User user)
        {
            if (user.HasPermaLeech && IsLeechSourceEvent(evt))
            {   
                var leechAbility = user.Abilities.Get(Abilities.Leech.name);
                var leechStat = (Leech)evt.UserStats.Get(StatName.Leech);
                var hypoRaw = evt.Amount.Naraw / (leechStat.PercentRate * 100) * leechStat.Eff;
                hypoRaw = leechStat.ApplyDryMult(hypoRaw);
                leechAbility.Heal.Hypo += hypoRaw;
            }
        }

        public static void LeechSourceGains(ThroughputEvent evt, User user, StatName statName, double gainNaraw, GainType gainType)
        {
            // If called with checking IsLeechSourceGain(), send Naraw.
            // Otherwise, send Nsnsnaraw and check if event qualifies as leech source event.

            if (user.HasPermaLeech)
            {
                gainType = Utils.GainTypeToHeal(gainType);
                var leechAbility = user.Abilities.Get(Abilities.Leech.name);
                var leechStat = (Leech)evt.UserStats.Get(StatName.Leech);
                var gain = gainNaraw * (leechStat.Eff / (leechStat.PercentRate * 100)) * leechAbility.HypoTrueUr();
                gain = leechStat.ApplyDryMult(gain);
                evt.Gains[statName][gainType] += gain;
            }
        }
        public static bool IsSummerEvent(User user, Ability ability, bool summerActive, Unit sourceUnit, bool absorbAbility)
        {
            var summer = user.Abilities.Get(Abilities.BlessingOfSummer.name);

            return
                summer.Heal.Raw > 0
                && summerActive
                && sourceUnit is User
                && !absorbAbility
                && ability.CanDupli
                && ability.Name != Abilities.BlessingOfSummer.name;
        }

        public static void SummerHypo(ThroughputEvent evt, User user)
        {

            if (IsSummerEvent(user, evt.Ability, evt.SummerActive, evt.SourceUnit, evt.AbsorbAbility))
            {
                var summer = (Abilities.BlessingOfSummer)user.Abilities.Get(Abilities.BlessingOfSummer.name);
                var hypoAmount = evt.Amount.Raw * summer.Coef;
                if (evt.IsHealDoneEvent())
                {
                    summer.Damage.Hypo += hypoAmount;
                }
                else if (evt.IsDmgDoneEvent())
                {
                    summer.Heal.Hypo += hypoAmount;
                }
            }
        }

        public static void SummerGains(ThroughputEvent evt, User user, StatName statName, double gainRaw, Ability ability, bool summerActive, bool absorbAbility, Unit sourceUnit, GainType gainType)
        {
            if (IsSummerEvent(user, ability, summerActive, sourceUnit, absorbAbility))
            {
                var summer = (Abilities.BlessingOfSummer)user.Abilities.Get(Abilities.BlessingOfSummer.name);
                double trueHypoCoefRaw = 0;
                double trueHypoCoef = 0;
                if (gainType == GainType.Eff || gainType == GainType.MsEff || gainType == GainType.BalEff || gainType == GainType.SupEff)
                {
                    trueHypoCoefRaw = summer.HypoTrueDmgR();
                    trueHypoCoef = trueHypoCoefRaw;
                }
                else if (gainType == GainType.Dmg || gainType == GainType.MsDmg || gainType == GainType.BalDmg || gainType == GainType.SupDmg)
                {
                    trueHypoCoefRaw = summer.HypoTrueRawR();
                    trueHypoCoef = summer.HypoTrueUr();

                }
                var sourceGainHypo = summer.Coef * gainRaw;
                var sourceGainRaw = sourceGainHypo * trueHypoCoefRaw;
                var sourceGain = sourceGainHypo * trueHypoCoef;

                gainType = Utils.ReverseGainType(gainType);

                evt.Gains[statName][gainType] += sourceGain;
                var gainNsnsnaraw = summer.RawToNsnsnarawConvert(sourceGainRaw);
                LeechSourceGains(evt, user, statName, gainNsnsnaraw, gainType);
            }
        }


        public static void SharedHypo(ThroughputEvent evt, User user)
        {
            LeechHypo(evt, user);
            SummerHypo(evt, user);
        }

        public static void AltSummerSource(List<Event> events, User user)
        {
            var summer = (Abilities.BlessingOfSummer)user.Abilities.Get(Abilities.BlessingOfSummer.name);
            var testHypoHeal = 0.0;
            var testHypoDmg = 0.0;

            var totalAmountRaw = 0.0;
            var totalAltAmountRaw = 0.0;

            foreach (var evt in events)
            {
                if (evt is ThroughputEvent tEvt)
                {
                    if (IsSummerEvent(user, evt.Ability, evt.SummerActive, evt.SourceUnit, tEvt.AbsorbAbility))
                    {
                        totalAmountRaw += tEvt.Amount.Raw;
                        for (int i = 0; i < evt.AltEvents.Count; i++)
                        {
                            var altEvent = evt.AltEvents[i];
                            totalAltAmountRaw += altEvent.Amount.Raw;

                            var hypoAmount = altEvent.Amount.Raw * summer.Coef;
                            var testAmount = tEvt.Amount.Raw * summer.Coef;
                            if (evt.IsHealDoneEvent())
                            {
                                summer.AltDamage[i].Hypo += hypoAmount;
                                testHypoDmg += testAmount;
                            }
                            else if (evt.IsDmgDoneEvent())
                            {
                                summer.AltHeal[i].Hypo += hypoAmount;
                                testHypoHeal += testAmount;
                            }

                        }
                    }
                }
            }

            foreach (var evt in events)
            {
                if (evt.AbilityName == Abilities.BlessingOfSummer.name && evt is ThroughputEvent tEvt)
                {
                    for (int i = 0; i < evt.AltEvents.Count; i++)
                    {
                        var altEvent = evt.AltEvents[i];
                        if (evt.IsHealDoneEvent())
                        {
                            var gainRaw = altEvent.Amount.Raw * summer.AltHypoTrueRawR(i) - altEvent.Amount.Raw;
                            altEvent.Amount.UpdateAltGainsFromEvtData(tEvt, gainRaw, i);
                        }
                        else if (evt.IsDmgDoneEvent())
                        {
                            var gainRaw = altEvent.Amount.Raw * summer.AltHypoTrueDmgR(i) - altEvent.Amount.Raw;
                            altEvent.Amount.UpdateAltGainsFromEvtData(tEvt, gainRaw, i);
                        }
                    }
                }
            }
        }

        public static void AltLeechSource(List<Event> events, User user)
        {
            var leechAbility = (Abilities.Leech)user.Abilities.Get(Abilities.Leech.name);

            foreach (var evt in events)
            {
                if (evt is ThroughputEvent tEvt)
                {
                    if (user.HasPermaLeech && IsLeechSourceEvent(tEvt))
                    {
                        for (int i = 0; i < evt.AltEvents.Count; i++)
                        {
                            var altEvent = evt.AltEvents[i];
                            var leechStat = (Leech)altEvent.UserStats.Get(StatName.Leech);
                            leechAbility.AltHeal[i].Hypo += altEvent.Amount.Naraw / (leechStat.PercentRate * 100) * leechStat.Eff;
                        }
                    }
                }
            }

            foreach (var evt in events)
            {
                if (evt.AbilityName == Abilities.Leech.name && evt is ThroughputEvent tEvt)
                {
                    for (int i = 0; i < evt.AltEvents.Count; i++)
                    {
                        var altEvent = evt.AltEvents[i];
                        var gainRaw = altEvent.Amount.Raw * leechAbility.AltHypoTrueRawR(i) - altEvent.Amount.Raw;
                        altEvent.Amount.UpdateAltGainsFromEvtData(tEvt, gainRaw, i);

                        //altEvent.Amount.Raw *= leechAbility.AltHypoTrueRawR(i);
                        //altEvent.Amount.Eff = tEvt.RawToEffConvert(altEvent.Amount.Raw);
                        //altEvent.Amount.Naraw = tEvt.RawToNarawConvert(altEvent.Amount.Raw);
                        //altEvent.Amount.Naeff = tEvt.EffToNaeffConvert(altEvent.Amount.Eff);
                    }
                }
            }
        }

    }
}

