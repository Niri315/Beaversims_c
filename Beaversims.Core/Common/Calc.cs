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
        public static double TrueCdCalc(Haste haste, double cd)
        {
            return cd / (haste.Eff / (haste.PercentRate * 100) + 1);
        }

        public static int GetBracket(double rating, double drRate)
        {
            var x = rating / drRate;

            if (x <= 30)
            {
                return 0;
            }
            else if (x <= 40)
            {
                return 1;
            }
            else if (x <= 50)
            {
                return 2;
            }
            else if (x <= 60)
            {
                return 3;
            }
            else if (x <= 70)
            {
                return 4;
            }
            else
            {
                return 5;
            }
        }
        public static double CalcPostDr(int bracket, double rating, double drRate)
        {
            var postDiminishAmount = 0.0;

            if (bracket == 0)
            {
                postDiminishAmount += rating;
            }
            else if (bracket == 1)
            {
                postDiminishAmount += drRate * 30;  // 0–30% amount
                postDiminishAmount += (rating - (drRate * 30)) * 0.9;  // 30–39% amount
            }
            else if (bracket == 2)
            {
                postDiminishAmount += drRate * 30;
                postDiminishAmount += drRate * 10 * 0.9;
                postDiminishAmount += (rating - (drRate * 30) - (drRate * 10)) * 0.8;  // 39–47% amount
            }
            else if (bracket == 3)
            {
                postDiminishAmount += drRate * 30;
                postDiminishAmount += drRate * 10 * 0.9;
                postDiminishAmount += drRate * 10 * 0.8;
                postDiminishAmount += (rating - (drRate * 30) - (drRate * 10) - (drRate * 10)) * 0.7;
            }
            else if (bracket == 4)
            {
                postDiminishAmount += drRate * 30;
                postDiminishAmount += drRate * 10 * 0.9;
                postDiminishAmount += drRate * 10 * 0.8;
                postDiminishAmount += drRate * 10 * 0.7;
                postDiminishAmount += (rating - (drRate * 30) - (drRate * 10) - (drRate * 10) - (drRate * 10)) * 0.6;
            }
            else
            {
                postDiminishAmount += drRate * 30;
                postDiminishAmount += drRate * 10 * 0.9;
                postDiminishAmount += drRate * 10 * 0.8;
                postDiminishAmount += drRate * 10 * 0.7;
                postDiminishAmount += drRate * 10 * 0.6;
                postDiminishAmount += (rating - (drRate * 30) - (drRate * 10) - (drRate * 10) - (drRate * 10) - (drRate * 10)) * 0.5;
            }

            return postDiminishAmount;
        }


    }
}
