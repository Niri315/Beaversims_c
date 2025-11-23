using Beaversims.Core.Common;
using Beaversims.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Druid.Resto
{ 
    // TODO need a reset function in case of death for buffs.

    internal static class MasteryTracker
        // Midnight removal : Grove tending, spring blossoms, cultivation, cenward.
    {   // Rejuv, Germ, LB, LB, Regrowth, grove tending, wild growth, spring blossoms, tranq, cultivation, cenward, symbiotic blooms, Reactive Resin,
        public static readonly List<int> harmonyBuffIds = [774, 155777, 33763, 188550, 8936, 383193, 48438, 207386, 157982, 102352, 200389, 439530, 468152];
        // Rejuv, Germ, Regrowth
        public static readonly List<int> QIMIncBuffIds = [774, 155777, 8936];
        public static readonly List<string> QIMIncBuffs = [Abilities.Rejuvenation.name, Abilities.RejuvenationGermination.name, Abilities.Regrowth.name];
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
                        if (QIMIncBuffIds.Contains(buff.Id))
                        {
                            unit.QIMIncHarmonyCount += BuffHarmonyLevel(user, buff.Name);
                        }
                    }
                }
            }
        }
        //public int QIMHypoIncLevel { get; set; } = 0;
        public static void SetMasteryEff(Event evt, User user)
        {

            if (evt is BuffEvent bEvt && evt.SourceUnit is User && harmonyBuffIds.Contains(evt.AbilityId))
            {
                if (bEvt.BuffApplyEvent)
                {
                    evt.TargetUnit.HarmonyLevel += BuffHarmonyLevel(user, evt.AbilityName);
                    if (QIMIncBuffIds.Contains(evt.AbilityId))
                    {
                        evt.TargetUnit.QIMIncHarmonyCount += BuffHarmonyLevel(user, evt.AbilityName);
                    }
                }
                else if (bEvt.BuffRemoveEvent)
                {
                    evt.TargetUnit.HarmonyLevel -= BuffHarmonyLevel(user, evt.AbilityName);
                    if (QIMIncBuffIds.Contains(evt.AbilityId))
                    {
                        evt.TargetUnit.QIMIncHarmonyCount -= BuffHarmonyLevel(user, evt.AbilityName);
                    }
                }
            }

            if (evt.IsHealDoneEvent() && evt.Ability.ScalesWith(StatName.Mastery))
            {
                var targetUnit = evt.TargetUnit;
                var hEvt = (HealEvent)evt;

                double harmonyMult = GetHarmonyMult(targetUnit.HarmonyLevel);
                double NonQIMharmonyMult = GetHarmonyMult(targetUnit.HarmonyLevel - targetUnit.QIMIncHarmonyCount);
                if (evt.AbilityName == Abilities.Nourish.name)
                {
                    var nourish = (Abilities.Nourish)evt.Ability;
                    harmonyMult *= nourish.HarmonyCoef;
                    NonQIMharmonyMult *= nourish.HarmonyCoef;
                }
                hEvt.MasteryEffectiveness = harmonyMult;
                hEvt.NonQIMBuffMasteryEffectivness = NonQIMharmonyMult;
                hEvt.MasteryEffWOQIM = Math.Max(GetHarmonyMult(targetUnit.HarmonyLevel - 1), 0);

                //Console.WriteLine($"{harmonyMult} VS {NonQIMharmonyMult}");

                //var buffList = new List<string>();
                //foreach (var buff in evt.TargetUnit.Buffs)
                //{
                //    if (harmonyBuffIds.Contains(buff.Id))
                //    {
                //        buffList.Add(buff.Name);
                //    }
                //}
                //Console.WriteLine("---------");
                //Console.WriteLine($"");
                //Console.WriteLine(string.Join(", ", buffList));
                //Console.WriteLine($" {Utils.ReadableTime(evt.Timestamp)} - {evt.AbilityName}, MasteryEffectiveness: {hEvt.MasteryEffectiveness}, buff Count: {buffList.Count}");
                //Console.WriteLine($"Target: {evt.TargetUnit.Name}");
            }
        }

        public static double MasteryGainCalc(Mastery mastery, double amount, double masteryEffectiveness)
    => (((amount / ((mastery.TrueEff() * masteryEffectiveness) / (mastery.PercentRate * 100) + 1)) * masteryEffectiveness) / (mastery.PercentRate * 100));


        public static void MasteryAltAmount(HealEvent evt, Mastery mastery, Haste haste, int i, User user, bool antiGain = false)
        {
 
            var altEvent = evt.AltEvents[i];
            var altMastery = altEvent.UserStats.Get(mastery.Name);
            var altHaste = altEvent.UserStats.Get(haste.Name);
            var gainRaw = MasteryGainCalc(mastery, altEvent.Amount.Raw, evt.MasteryEffectiveness) * (altMastery.TrueEff() - mastery.TrueEff());


            // Gain from getting more mastery buffs from cast value of haste.
          
          
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

        public static void MasteryGains(HealEvent evt, User user, int i, bool antiGain = false)
        {
            var statName = StatName.Mastery;
            var mastery = (Mastery)evt.UserStats.Get(statName);
            var haste = (Haste)evt.UserStats.Get(StatName.Haste);
            MasteryAltAmount(evt, mastery, haste, i, user, antiGain:antiGain);
            
        }
    }
}
