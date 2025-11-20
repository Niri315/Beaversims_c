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

        private static bool IsReversion(string abilityName)
        {
            if (abilityName == Abilities.Reversion.name || abilityName == Abilities.ReversionEcho.name)
            {
                return true;
            }
            return false;
        }

        public static void TrackReversions(Event evt, User user)
        {
            if (evt is BuffEvent bEvt && IsReversion(evt.AbilityName) && evt.SourceUnit is User)
            {
                var revAbility = (Abilities.PresAbility)user.Abilities.Get(evt.AbilityName);
                var revTracker = evt.TargetUnit.ReversionTracker;
                if (bEvt.BuffApplyEvent)
                {
                    revTracker[revAbility] = evt.Timestamp;
                }
                else if (bEvt.BuffRemoveEvent)
                {
                    if (revTracker.ContainsKey(revAbility))
                    {
                        var dur = evt.Timestamp - revTracker[revAbility];
                        revAbility.TotalDur += dur;
                        revAbility.ExtendedDur += Math.Max(dur - revAbility.Duration, 0);
                    }
                }
                else if (bEvt.BuffRefreshEvent)
                {
                    if (revTracker.ContainsKey(revAbility))
                    {
                        var dur = evt.Timestamp - revTracker[revAbility];
                        revAbility.TotalDur += dur;
                        revAbility.ExtendedDur += Math.Max(dur - revAbility.Duration, 0);
                    }
                    revTracker[revAbility] = evt.Timestamp;
                }
            }
        }

        private static double CritExtensionCalc(double hasteEff, double critEff, double dur)
        {
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
        private static (double, double) RevExtensionRatios(Event evt)
        {
            var haste = evt.UserStats.Get(StatName.Haste);
            var crit = evt.UserStats.Get(StatName.Crit);
            var originalVal = CritExtensionCalc(haste.TrueEff(), crit.TrueEff(), 12);
            var hasteIncVal = CritExtensionCalc(haste.TrueEff() + 1, crit.TrueEff(), 12);
            var critIncVal = CritExtensionCalc(haste.TrueEff(), crit.TrueEff() + 1, 12);
            var hasteIncDiff = hasteIncVal - originalVal;
            var critIncDiff = critIncVal - originalVal;

            var critRatio = critIncDiff / (hasteIncDiff + critIncDiff);
            var hasteRatio = hasteIncDiff / (hasteIncDiff + critIncDiff);
            return (critRatio, hasteRatio);

        }

        public static void CritExtensionGain_i(PresAbility ability)
        {
            ability.
        }
        

        public static void CritExtensionGains(List<Event> tpEvents, User user)
        {
            (var critRatio, var hasteRatio) = RevExtensionRatios(tpEvents[0]);
            var rev = (Abilities.Reversion)user.Abilities.Get(Abilities.Reversion.name);
            var revEcho = (Abilities.ReversionEcho)user.Abilities.Get(Abilities.ReversionEcho.name);

            Console.WriteLine($" Reversion - ext dur: {rev.ExtendedDur} Total dur: {rev.TotalDur}");
            Console.WriteLine($" Reversion Echo - ext dur: {revEcho.ExtendedDur} Total dur: {revEcho.TotalDur}");
        }
        public static void FullAllocCalcs(List<Event> tpEvents, User user)
        {

            CritExtensionGains(tpEvents, user);
        }
    }
}
