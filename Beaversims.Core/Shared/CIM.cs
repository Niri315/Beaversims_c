using Beaversims.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace Beaversims.Core.Shared
{
    internal class CIM
    {

        public static void CastTimeGains(CastEvent evt, User user, double castTime)
        {
            // We're just gonna store loss from hitting GCD cap and apply it later when fetching full HCGM.
            // For this we are not having the GCD Cap affect gain nor true cast time total.
            // Alot easier than having to deal with it here and with CIM.
            // Shouldnt really matter in any case. This way we can keep it universal.

            var ability = evt.Ability;
            var haste = (Haste)evt.UserStats.Get(StatName.Haste);
            var gain = Calc.SecondaryGainCalc(haste, castTime, haste.PercentRate);

            var trueCastTime = Calc.TrueCastTimeCalc(haste, castTime);
            //trueCastTime = Math.Max(trueCastTime, Constants.castTimeCap);


            for (int i = 0; i < evt.AltEvents.Count; i++)
            {

                var trueCastTime_i = Calc.TrueCastTimeCalc((Haste)evt.AltEvents[i].UserStats.Get(StatName.Haste), castTime);
                if (trueCastTime_i < Constants.castTimeCap)
                {
                    user.AltGearSets[i].HasteCapCTLoss += Constants.castTimeCap - trueCastTime_i;
                }
            }
            ability.CastTimeGain += gain;
            ability.TrueCastTimeTotal += trueCastTime;
            user.TrueCastTimeTotal += trueCastTime;

            if (trueCastTime < Constants.castTimeCap)
            {
                user.HasteCapCTLoss += Constants.castTimeCap - trueCastTime;
            }
        }

        public static void TestCIMMath(User user)
        {
            var totalCastTimeGain = 0.0;
            var totalCastTimeGainCIM = 0.0;
            foreach (var ability in user.Abilities)
            {
                if (ability.CastTimeGain > 0)
                {
                    totalCastTimeGainCIM += ability.CastTimeGain * ability.CIM;
                    totalCastTimeGain += ability.CastTimeGain;
                }

            }
            var tot = totalCastTimeGainCIM - totalCastTimeGain;
            const double tolerance = 1e-9;

            if (Math.Abs(tot) > tolerance)
            {

                throw new InvalidOperationException(
                    $"CIM Imbalance."
                );
            }
 
        }



        public static void SetCIM(User user)
        {

            var abilities = user.Abilities;
            double scaleGain = 0;
            double nonScaleGain = 0;

            foreach (var ability in user.Abilities)
            {
                ability.CIMSourceRelCheck();
                ability.MaxCIM = Math.Max((user.TrueCastTimeTotal - user.HasteCapCTLoss) / ability.CdTimeHypo, 1.0);
            }

            bool allDone = false;
            while (!allDone)
            {
                scaleGain = 0;
                nonScaleGain = 0;

                foreach (var ability in abilities)
                {
                    if (ability.HasteScalers.Contains(HST.Cast) && !ability.ZeroCIM)
                    {

                        scaleGain += ability.CastTimeGain * ability.CIMRatio;
                    }
                    else
                    {
                        nonScaleGain += ability.CastTimeGain;
                    }
                }

                var totalGain = scaleGain + nonScaleGain;
                var hcgmTotalCoef = 0.0;
                if (scaleGain > 0)
                {
                    hcgmTotalCoef = totalGain / scaleGain;
                }

                bool anyUpdated = false;

                foreach (var ability in user.Abilities)
                {
                    if (ability.MaxCIM < hcgmTotalCoef && !ability.CIMInitDone && ability.HasteScalers.Contains(HST.Cast) && !ability.ZeroCIM)
                    {
                        ability.CIMInitDone = true;
                        anyUpdated = true;
                    }
                    if (ability.CIMInitDone)
                    {
                        ability.CIMRatio = scaleGain * (ability.MaxCIM - 1) / nonScaleGain;
                    }
                }
                allDone = !anyUpdated;
            }

            scaleGain = 0;
            nonScaleGain = 0;

            foreach (var ability in abilities)
            {
                if (ability.HasteScalers.Contains(HST.Cast) && !ability.ZeroCIM)
                {

                    scaleGain += ability.CastTimeGain * ability.CIMRatio;
                }
                else
                {
                    nonScaleGain += ability.CastTimeGain;
                }
            }


            if (scaleGain > 0)
            {
                foreach (var ability in abilities)
                {
                    if (ability.HasteScalers.Contains(HST.Cast) && ability.CastTimeGain > 0 && !ability.ZeroCIM)
                    {
                        ability.CIM = (scaleGain + (nonScaleGain * ability.CIMRatio)) / (scaleGain);
                        ability.CIMInitDone = true;

                    }
                    else //if // (!ability.HasteScalers.Contains(HST.Cast))
                    {

                        ability.CIM = 0;
                    }
                }
            }
            TestCIMMath(user);
        }
    }
}

