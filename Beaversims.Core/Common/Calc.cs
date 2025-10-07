using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    internal static class Calc
    {
        public static double PrimaryGainCalc(Stat stat, double amount)
        {
            return (amount / stat.Eff) * stat.Multi;
        }
        public static double SecondaryGainCalc(SecondaryStat stat, double amount, double percentRate)
        {
            var gain = amount / (stat.Eff + (100 * percentRate));
            gain = stat.ApplyDryMult(gain);
            return gain;
        }

        public static double DefGainCalc(NonPrimaryStat stat, double amount, double percentRate)
        {
            var pureAmountRaw = (amount * (1 + (stat.Eff / percentRate / 100)));
            var gain = pureAmountRaw * (1 / percentRate / 100) * (1 - (stat.Bracket * 0.1)) * stat.Multi;
            return gain;
        }

        public static double CritGainCalc(SecondaryStat stat, double amount, bool isCrit, double critInc)
        {
            if (isCrit)
            {
                amount /= critInc;
            }

            return (amount / (stat.PercentRate * 100)) * (critInc - 1) * (1 - (stat.Bracket * 0.1)) * stat.Multi;
        }
        public static double TrueCastTimeCalc(Haste haste, double castTime)
        {
            return castTime / (haste.Eff / (haste.PercentRate * 100) + 1);
        }

    }
}
