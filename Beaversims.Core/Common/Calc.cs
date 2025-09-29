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
        public static double SecondaryGainCalc(NonPrimaryStat stat, double amount, double percentRate)
        {
            return (amount / (stat.Eff + (100 * percentRate))) * (1 - (stat.Bracket * 0.1)) * stat.Multi;
        }
        public static double CritGainCalc(SecondaryStat stat, double amount, bool isCrit, double critInc)
        {
            return (amount / (stat.PercentRate * 100)) * (critInc - 1) * (1 - (stat.Bracket * 0.1)) * stat.Multi;
        }
        public static double TrueCastTimeCalc(Haste haste, double castTime)
        {
            return castTime / (haste.Eff / (haste.PercentRate * 100) + 1);
        }
    }
}
