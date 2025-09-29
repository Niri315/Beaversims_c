using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Beaversims.Core.Shared
{
    internal class HCGM
    {
        public static void CastTimeGains(CastEvent cEvt, User user, double castTime)
        {
            var haste = (Haste)cEvt.UserStats.Get(StatName.Haste);
            var gain = Calc.SecondaryGainCalc(haste, castTime, haste.PercentRate);
            var trueCastTime = Calc.TrueCastTimeCalc(haste, castTime);
            if (trueCastTime > Constants.castTimeCap)
            {
                cEvt.Ability.CastTimeGain += gain;
            }
        }

        public static void SetHCGM(User user)
        {
            double nonScaleGain = 0.0;
            double scaleGain = 0.0;

            var abilities = user.Abilities;
            foreach (var ability in abilities)
            {
                if (ability.HasteScalers.Contains(HST.Cast))
                {
                    scaleGain += ability.CastTimeGain;
                }
                else
                {
                     nonScaleGain += ability.CastTimeGain;
                }
            }
            var totalGain = scaleGain + nonScaleGain;
            if (scaleGain > 0)
            {
                user.HCGM *= totalGain / scaleGain;
            }
            Console.WriteLine($"User HCGM: {user.HCGM}");
        }
    }
}
