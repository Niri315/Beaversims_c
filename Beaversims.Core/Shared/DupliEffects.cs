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
        public static bool IsLeechSourceEvent(TpEvent evt) => !evt.FullyAbsorbed &&
            evt.TargetUnit is not User &&
            evt.SourceUnit is User &&
            evt.Ability.LeechSource &&
            evt.Ability.CanDupli;

        public static void LeechHypo(TpEvent evt, User user)
        {
            if (user.HasPermaLeech && IsLeechSourceEvent(evt))
            {   
                var leechAbility = user.Abilities.Get(Abilities.Leech.name);
                var leechStat = (Leech)evt.UserStats.Get(StatName.Leech);
                var hypoRaw = evt.Amount.Naraw / (leechStat.PercentRate * 100) * leechStat.TrueEff();
                hypoRaw = leechStat.ApplyDryMult(hypoRaw);
                leechAbility.Heal.Hypo += hypoRaw;
            }
        }

        public static void LeechSourceGains(TpEvent evt, User user, StatName statName, double gainNaraw, GainType gainType)
        {
            // If called with checking IsLeechSourceGain(), send Naraw.
            // Otherwise, send Nsnsnaraw and check if event qualifies as leech source event.

            if (user.HasPermaLeech)
            {
                gainType = Utils.GainTypeToHeal(gainType);
                var leechAbility = user.Abilities.Get(Abilities.Leech.name);
                var leechStat = (Leech)evt.UserStats.Get(StatName.Leech);
                var gain = gainNaraw * (leechStat.TrueEff() / (leechStat.PercentRate * 100)) * leechAbility.HypoTrueUhr();
                gain = leechStat.ApplyDryMult(gain);
                evt.Gains[statName][gainType] += gain;
            }
        }
        public static bool IsSummerEvent(User user, TpEvent evt)

        {
            var summer = user.Abilities.Get(Abilities.BlessingOfSummer.name);

            return
                summer.Heal.Raw > 0
                && evt.SummerActive
                && evt.SourceUnit is User
                && !evt.AbsorbAbility
                && evt.Ability.CanDupli
                && evt.Ability.Name != Abilities.BlessingOfSummer.name;
        }

        public static void SummerHypo(TpEvent evt, User user)
        {

            if (IsSummerEvent(user, evt))
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



        public static void SharedHypo(TpEvent evt, User user)
        {
            LeechHypo(evt, user);
            SummerHypo(evt, user);

        }

        public static void AltSummerSource(List<TpEvent> events, User user, int i)
        {
            var summer = (Abilities.BlessingOfSummer)user.Abilities.Get(Abilities.BlessingOfSummer.name);
            var evtHasSummer = false;
            foreach (var evt in events)
            {
                if (evt.SimEvent)
                {
                    evt.SummerActive = evtHasSummer;
                }
                else
                {
                    evtHasSummer = evt.SummerActive;
                }
                if (IsSummerEvent(user, evt))
                {
                    AmountContainer amountCont;
                    if (evt.SimEvent)
                    {
                        amountCont = evt.Amount;
                        Console.WriteLine($"{evt.AbilityName}: {evt.Amount.Raw}");

                    }
                    else
                    {
                        amountCont = evt.AltEvents[i].Amount;
                    }
                    var hypoAmount = amountCont.Raw * summer.Coef;

                    if (evt.IsHealDoneEvent())
                    {
                        summer.AltDamage[i].Hypo += hypoAmount;
                    }
                    else if (evt.IsDmgDoneEvent())
                    {
                        summer.AltHeal[i].Hypo += hypoAmount;
                    }
                }
            }
            foreach (var evt in events)
            {
                if (evt.AbilityName == Abilities.BlessingOfSummer.name)
                {
                    var altEvent = evt.AltEvents[i];
                    if (evt.IsHealDoneEvent())
                    {
                        var gainRaw = altEvent.Amount.Raw * summer.AltHypoTrueRawR(i) - (altEvent.Amount.Raw + altEvent.NukeRaw);
                        altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);
                    }
                    else if (evt.IsDmgDoneEvent())
                    {
                        var gainRaw = altEvent.Amount.Raw * summer.AltHypoTrueDmgR(i) - (altEvent.Amount.Raw + altEvent.NukeRaw);
                        altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);
                    }
                }
            }
        }

        public static void AltLeechSource(List<TpEvent> events, User user, int i)
        {
            var leechAbility = (Abilities.Leech)user.Abilities.Get(Abilities.Leech.name);

            foreach (var evt in events)
            {
                if (user.HasPermaLeech && IsLeechSourceEvent(evt))
                {
                    AmountContainer amountCont;
                    StatTracker stats;
                    if (evt.SimEvent)
                    {
                        amountCont = evt.Amount;
                        stats = evt.UserStats;
                    }
                    else
                    {
                        amountCont = evt.AltEvents[i].Amount;
                        stats = evt.AltEvents[i].UserStats;
                    }

                    var leechStat = (Leech)stats.Get(StatName.Leech);
                    leechAbility.AltHeal[i].Hypo += amountCont.Naraw / (leechStat.PercentRate * 100) * leechStat.TrueEff();
                }
            }

            foreach (var evt in events)
            {
                if (evt.AbilityName == Abilities.Leech.name)
                {

                    var altEvent = evt.AltEvents[i];
                    var gainRaw = altEvent.Amount.Raw * leechAbility.AltHypoTrueRawR(i) - (altEvent.Amount.Raw + altEvent.NukeRaw);
                    altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);

                }
            }
        }

    }
}

