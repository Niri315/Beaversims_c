using Beaversims.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Druid.Resto
{ 
    internal static class MasteryTracker
        // Midnight removal : Grove tending, spring blossoms, cultivation, cenward.
    {   // Rejuv, Rejuv, LB, LB, Regrowth, grove tending, wild growth, spring blossoms, tranq, cultivation, cenward, symbiotic blooms, Reactive Resin,
        public static readonly List<int> harmonyBuffIds = [774, 155777, 33763, 188550, 8936, 383193, 48438, 207386, 157982, 102352, 200389, 439530, 468152];

        public static readonly List<double> table = [
            1.0,
            0.7,
            0.6,
            0.5,
            0.4,
            0.3,
            0.2,
            0.1,
        ];
        public static double GetHarmonyMult(int buffCount)
        {
            if (buffCount <= 0) return 0.0;

            buffCount = Math.Min(buffCount, table.Count);

            double sum = 0.0;

            for (int i = 0; i < buffCount; i++)
                sum += table[i];

            return sum;
        }

        public static int BuffHarmonyLevel(User user, string buffName)
        {
            if (buffName == Talents.HarmoniousBlooming.abilityName && user.HasTalent(Talents.HarmoniousBlooming.id))
            {
                var harmBloom = (Talents.HarmoniousBlooming)user.Talents[Talents.HarmoniousBlooming.id];
                return harmBloom.HarmonyCount;
            }
            else
            {
                return 1;
            }
        }

        public static void InitHarmonyBuffs(UnitRepo allUnits, User user)
        {
            foreach (var unit in allUnits)
            {
                foreach (var buff in unit.Buffs)
                {
                    if (harmonyBuffIds.Contains(buff.Id) && buff.SourceId == user.Id)
                    {
                        unit.HarmonyLevel += BuffHarmonyLevel(user, buff.Name);
                    }
                }
            }
        }

        public static void SetMasteryEff(Event evt, User user)
        {
            if (evt is BuffEvent bEvt && evt.SourceUnit is User && harmonyBuffIds.Contains(evt.AbilityId))
            {
                if (bEvt.BuffApplyEvent)
                {
                    evt.TargetUnit.HarmonyLevel += BuffHarmonyLevel(user, evt.AbilityName);
                }
                else if (bEvt.BuffRemoveEvent)
                {
                    evt.TargetUnit.HarmonyLevel -= BuffHarmonyLevel(user, evt.AbilityName);
                }
            }

            if (evt.IsHealDoneEvent() && evt.Ability.ScalesWith(StatName.Mastery))
            {
                var hEvt = (HealEvent)evt;

                double harmonyMult = GetHarmonyMult(evt.TargetUnit.HarmonyLevel);
                if (evt.AbilityName == Abilities.Nourish.name)
                {
                    var nourish = (Abilities.Nourish)evt.Ability;
                    harmonyMult *= nourish.HarmonyCoef;
                }
                hEvt.masteryEffectiveness = harmonyMult;
            }
        }

        public static double MasteryGainCalc(Mastery mastery, double amount, double masteryEffectiveness)
    => (((amount / ((mastery.TrueEff() * masteryEffectiveness) / (mastery.PercentRate * 100) + 1)) * masteryEffectiveness) / (mastery.PercentRate * 100)) * (1 - (mastery.Bracket * 0.1)) * mastery.Multi;


        public static void MasteryAltAmount(HealEvent evt, Mastery stat, int i, double masteryEffectiveness, bool antiGain = false)
        {

            var altEvent = evt.AltEvents[i];
            var gainPerRatingRaw = MasteryGainCalc(stat, altEvent.Amount.Raw, masteryEffectiveness);
            var gainPerEffstatRaw = stat.RemoveDryMult(gainPerRatingRaw);
            var altStat = altEvent.UserStats.Get(stat.Name);
            var gainRaw = gainPerEffstatRaw * (altStat.TrueEff() - stat.TrueEff());
            if (antiGain) { gainRaw *= -1; }
            altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);

        }

        public static void MasteryGains(HealEvent evt, User user, int i, bool antiGain = false)
        {
            var statName = StatName.Mastery;
            var stat = (Mastery)evt.UserStats.Get(statName);
            MasteryAltAmount(evt, stat, i, evt.masteryEffectiveness, antiGain:antiGain);
            
        }
    }
}
