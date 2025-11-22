using Beaversims.Core.Shared.Abilities;
using Beaversims.Core.Specs.Evoker.Pres.Abilities;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Evoker.Pres
{


    internal class FullAllocs
    {



        public static double GetEmpTickDur(int empCastLevel, Ability ability)
        {
            if (ability.Name == Abilities.FireBreath.name)
            {
                return Math.Min(30 - (empCastLevel * 6), 24);
            }
            if (ability.Name == Abilities.DreamBreath.name)
            {
                return Math.Min(20 - (empCastLevel * 4), 16);
            }
            return 0;
        }

        public static void TrackReversions(Event evt, User user)
        {
           
            // We are only using values during tracking events rather than total parsed values.
            // If we miss any buff, we will miss the equivalent value as well which is correct for the math to work out.

            if (evt.AbilityName is null) { return; }
            if (!user.Abilities.Contains(evt.AbilityName))
            {
                return;
            }
            var ability = user.Abilities.Get(evt.AbilityName);
            if (ability is PresAbility presAbility)
            {

                if (evt is CastEvent cEvt && presAbility.CritExtendAbility && cEvt.EmpCastLevel > 0)
                {
                    if (presAbility.EchoVersion != null)
                    {
                        var echoVersion = (Abilities.PresAbility)user.Abilities.Get(presAbility.EchoVersion);
                        echoVersion.LastEmpLEvel = cEvt.EmpCastLevel;
                    }
                    presAbility.LastEmpLEvel = cEvt.EmpCastLevel;
                   
                }

                if (evt is BuffEvent bEvt && presAbility.CritExtendAbility && evt.SourceUnit is User && evt.AbilityId == ability.BuffId)
                {
                    //Console.WriteLine(evt.AbilityName);
                    var revAbility = (Abilities.PresAbility)user.Abilities.Get(evt.AbilityName);
                    var revTracker = evt.TargetUnit.ReversionTracker;
                    double originalDur;
                    if (presAbility.EmpAbility)
                    {
                        originalDur = GetEmpTickDur(presAbility.LastEmpLEvel, revAbility);
                        //Console.WriteLine($"{evt.Timestamp} - {revAbility.Name}: Exp Duration: {originalDur}, buff Id: {evt.AbilityId}, Target Id: {evt.TargetUnit.Id.TypeId}");
                    }
                    else
                    {
                        originalDur = revAbility.Duration;
                    }

                    if (bEvt.BuffApplyEvent)
                    {

                        revTracker[revAbility] = new double[2];
                        revTracker[revAbility][0] = evt.Timestamp;
                        revTracker[revAbility][1] = originalDur;
                    }
                    else if (bEvt.BuffRemoveEvent)
                    {
                        if (revTracker.ContainsKey(revAbility))
                        {

                            var totalDur = evt.Timestamp - revTracker[revAbility][0];
                            revAbility.TotalDur += totalDur;
                            revAbility.ExtendedDur += Math.Max(totalDur - revTracker[revAbility][1], 0);
                            evt.TargetUnit.ReversionTracker.Remove(revAbility);
                        }
                    }
                    else if (bEvt.BuffRefreshEvent)
                    {
                        if (revTracker.ContainsKey(revAbility))
                        {

                            var totalDur = evt.Timestamp - revTracker[revAbility][0];
                            revAbility.TotalDur += totalDur;
                            revAbility.ExtendedDur += Math.Max(totalDur - revTracker[revAbility][1], 0);
                        }
                        else
                        {
                            revTracker[revAbility] = new double[2];
                        }
                        revTracker[revAbility][0] = evt.Timestamp;
                        revTracker[revAbility][1] = originalDur;
                    }
                }
                // Disabling this as there are issues. 
                // With replacement logic we dont need to care about putting in any failsafe either.

                //if (evt.IsHealDoneEvent() && presAbility.CritExtendAbility && evt.TargetUnit.ReversionTracker.ContainsKey(evt.Ability))
                //{
                //    var hEvt = (HealEvent)evt;
                //    var revAbility = (Abilities.PresAbility)evt.Ability;
                //    revAbility.TotalCount_ö++;
                //    if (hEvt.Crit)
                //    {
                //        revAbility.CritCount_ö++;
                //    }
                //    var AppliedTime = evt.TargetUnit.ReversionTracker[evt.Ability];
                //    if (AppliedTime + evt.Ability.Duration < evt.Timestamp)
                //    {
                //        presAbility.CritExtendExtraHeal += hEvt.Amount.Raw;
                //    }
                //}
            }
          
        }

        private static double CritExtensionCalc(double hasteEff, double critEff, double dur)
        {
            // Only using this to get rough ratio between crit vs haste allocation of gains.
            // It's not reliable on its own as it doesnt take overlapping effects into account.

            int n = 100;

            double crit_p = critEff / (Crit.percentRate * 100);

            double tick_count = (dur / 2.0) * (1 + (hasteEff / (Haste.percentRate * 100)));

            double summation = 0.0;
            for (int i = 1; i <= n; i++)
            {
                summation += tick_count * Math.Pow(crit_p, i);
            }
            return summation;
        }

        private static (double, double) RevExtensionRatios(Event evt, int i)
        {   
            var altEvent = evt.AltEvents[i];
            var haste = altEvent.UserStats.Get(StatName.Haste);
            var crit = altEvent.UserStats.Get(StatName.Crit);
            var originalVal = CritExtensionCalc(haste.TrueEff(), crit.TrueEff(), 12);
            var hasteIncVal = CritExtensionCalc(haste.TrueEff() + Haste.percentRate, crit.TrueEff(), 12);
            var critIncVal = CritExtensionCalc(haste.TrueEff(), crit.TrueEff() + Crit.percentRate, 12);
            var hasteIncDiff = hasteIncVal - originalVal;
            var critIncDiff = critIncVal - originalVal;

            var critRatio = critIncDiff / (hasteIncDiff + critIncDiff);
            var hasteRatio = hasteIncDiff / (hasteIncDiff + critIncDiff);
            return (critRatio, hasteRatio);

        }
        

        public static void CritExtensionGains(List<TpEvent> tpEvents, User user, int i)
        {
            // TODO get avg stats and use it rather than starter stats.
            (var critRatio, var hasteRatio) = RevExtensionRatios(tpEvents[0], i);
            List<Abilities.PresAbility> critExtendAbilities = user.Abilities
                .OfType<Abilities.PresAbility>()
                .Where(a => a.CritExtendAbility)
                .ToList();
            foreach (var extAbility in critExtendAbilities)
            {

                var extendValRatio = extAbility.ExtendedDur / extAbility.TotalDur;
                double tpRaw;
                double critCount;
                if (extAbility.Name == Abilities.FireBreath.name)
                {
                    tpRaw = extAbility.Damage.Dmg;
                    critCount = extAbility.Damage.Crit.Count;
                }
                else
                {
                    tpRaw = extAbility.Heal.Raw;
                    critCount = extAbility.Heal.Crit.Count;
                }

                var critExtendExtraHeal = tpRaw * extendValRatio;

                var healPerCrit_fake = critExtendExtraHeal / critCount;  // Since we are going back to using full ability heal.
                var healPerCrit_true = healPerCrit_fake * critRatio;
                var healPerHaste_true = healPerCrit_fake * hasteRatio;

                var healPerEffCrit = healPerCrit_true / (Crit.percentRate * 100);
                var healPerEffHaste = healPerHaste_true / (100 * Haste.percentRate);
                extAbility.ExtHPC_e = healPerEffCrit;
                extAbility.ExtHPH_e = healPerEffHaste;

                Console.WriteLine($" {extAbility.Name} - ext dur: {extAbility.ExtendedDur} Total dur: {extAbility.TotalDur}");
                Console.WriteLine($" {extAbility.Name} - extendValRatio: {extendValRatio}");
                Console.WriteLine($" {extAbility.Name} - ExtendedHeal: {critExtendExtraHeal}");
                Console.WriteLine($" {extAbility.Name} - Total Heal: {extAbility.Heal.Raw}");
                Console.WriteLine($"critRatio {critRatio}  hasteRatio {hasteRatio}");
                Console.WriteLine($" {extAbility.Name} - Heal Per crit true: {healPerCrit_true}, Heal Per Haste true: {healPerHaste_true}");
            }
            foreach (var evt in tpEvents)
            {

                if ((evt.IsHealDoneEvent() || evt.IsDmgDoneEvent()) && critExtendAbilities.Contains(evt.Ability))
                {
                    var extAbility = (PresAbility)evt.Ability;
                    var crit = evt.UserStats.Get(StatName.Crit);
                    var haste = evt.UserStats.Get(StatName.Haste);
                    var altEvent = evt.AltEvents[i];
                    var altCrit = altEvent.UserStats.Get(StatName.Crit);
                    var altHaste = altEvent.UserStats.Get(StatName.Haste);

                    var critDiff = altCrit.TrueEff() - crit.TrueEff();
                    var hasteDiff = altHaste.TrueEff() - haste.TrueEff();
                    var gainCritRaw = critDiff * extAbility.ExtHPC_e;
                    var gainHasteRaw = hasteDiff * extAbility.ExtHPH_e;

                    altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainCritRaw + gainHasteRaw, i);
                }
            }
        }
        public static void FullAllocCalcs(List<TpEvent> tpEvents, User user, int i)
        {

            CritExtensionGains(tpEvents, user, i);
        }
    }
}
