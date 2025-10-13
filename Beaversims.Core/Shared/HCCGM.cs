using Beaversims.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Beaversims.Core.Shared
{
    internal class HCCGM
    {

        public static void CastTimeGains(CastEvent cEvt, User user, double castTime)
        {
            var ability = cEvt.Ability;
            var haste = (Haste)cEvt.UserStats.Get(StatName.Haste);
            var gain = Calc.SecondaryGainCalc(haste, castTime, haste.PercentRate);

            var trueCastTime = Calc.TrueCastTimeCalc(haste, castTime);
            ability.TrueCastTimeTotal += trueCastTime;

            // TODO need to check GCD cap for all i
            if (trueCastTime > Constants.castTimeCap)
            {

                ability.CastTimeGain += gain;
            }
            else
            {
                trueCastTime = Constants.castTimeCap;
            }

            user.TrueCastTimeTotal += trueCastTime;
        }
        public static void TestHCCGMMath(User user)
        {
            var totalCastTimeGain = 0.0;
            var totalCastTimeGainHCGM = 0.0;
            foreach (var ability in user.Abilities)
            {
                if (ability.CastTimeGain > 0)
                {
                    totalCastTimeGainHCGM += ability.CastTimeGain * ability.HCCGM;
                    totalCastTimeGain += ability.CastTimeGain;
                }

            }
            Console.WriteLine($"HCCGM Test: {totalCastTimeGainHCGM} VS {totalCastTimeGain}");
        }



        public static void SetHCCGM(User user)
        {

            var abilities = user.Abilities;
            double scaleGain = 0;
            double nonScaleGain = 0;

            bool allDone = false;
            while (!allDone)
            {
                scaleGain = 0;
                nonScaleGain = 0;

                foreach (var ability in abilities)
                {
                    if (ability.HasteScalers.Contains(HST.Cast))
                    {

                        scaleGain += ability.CastTimeGain * ability.HCCGMRatio;
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
                    ability.MaxHCCGM = Math.Max(user.TrueCastTimeTotal / ability.CdTimeHypo, 1.0);
                    if (ability.MaxHCCGM < hcgmTotalCoef && !ability.HCCGMInitDone && ability.HasteScalers.Contains(HST.Cast))
                    {
                        ability.HCCGMInitDone = true;
                        anyUpdated = true;
                    }
                    if (ability.HCCGMInitDone)
                    {
                        ability.HCCGMRatio = scaleGain * (ability.MaxHCCGM - 1) / nonScaleGain;
                    }
                }
                allDone = !anyUpdated;
            }

            scaleGain = 0;
            nonScaleGain = 0;

            foreach (var ability in abilities)
            {
                if (ability.HasteScalers.Contains(HST.Cast))
                {
                    if (ability.HCCGMInitDone)
                    {
                        scaleGain += ability.CastTimeGain * ability.HCCGMRatio;
                    }
                    else
                    {
                        scaleGain += ability.CastTimeGain;
                    }

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
                    if (ability.HasteScalers.Contains(HST.Cast) && (ability.HCCGMSources.Count == 0))
                    {
                        ability.HCCGM *= (scaleGain + (nonScaleGain * ability.HCCGMRatio)) / (scaleGain);
                        ability.HCCGMInitDone = true;
                    }
                    else if (!ability.HasteScalers.Contains(HST.Cast))
                    {
                        ability.HCCGM = 0;
                    }
                }
                bool followUpDone = false;
                while (!followUpDone)
                {
                    bool anyUpdated = false;

                    foreach (var ability in abilities)
                    {
                        if (!ability.HasteScalers.Contains(HST.Cast)) continue;
                        if (ability.HCCGMInitDone) continue;
                        if (ability.HCCGMSources.Count == 0) continue;


                        double hccgmCoef = 0.0;
                        bool skipAbility = false;

                        foreach (var hcgmSource in ability.HCCGMSources)
                        {
                            var sourceAbility = user.Abilities.Get(hcgmSource.Name);

                            if (!sourceAbility.HCCGMInitDone)
                            {
                                skipAbility = true;
                                break;
                            }

                            hccgmCoef += hcgmSource.HCCGMReliance * sourceAbility.HCCGM;
                        }

                        if (skipAbility)
                            continue;
                        ability.HCCGM *= hccgmCoef;
                        ability.HCCGMInitDone = true;
                        anyUpdated = true;

                    }
                    followUpDone = !anyUpdated;
                }
            }
            TestHCCGMMath(user);
        }
    }
}

//foreach (var ability in abilities)
//{
//    if (ability.HasteScalers.Contains(HST.Cast))
//    {
//        ability.HCGM = (scaleGain + (nonScaleGain * ability.HCGMRatio)) / (scaleGain);
//    }
//    else
//    {
//        ability.HCGM = 0;
//    }
//}
//bool progressMade;

//do
//{
//    progressMade = false;

//    foreach (var ability in abilities)
//    {
//        if (!ability.HasteScalers.Contains(HST.Cast)) continue;
//        if (ability.HCGMFollowupDone) continue;

//        if (ability.HCGMSources.Count == 0) continue;

//        double hcgmCoef = 0.0;
//        bool skipAbility = false;

//        foreach (var hcgmSource in ability.HCGMSources)
//        {
//            var sourceAbility = user.Abilities.Get(hcgmSource.Name);
//            if (!sourceAbility.HCGMFollowupDone)
//            {
//                skipAbility = true;
//                break;
//            }

//            hcgmCoef += hcgmSource.HCGMReliance * sourceAbility.HCGM;
//        }

//        if (skipAbility)
//            continue;

//        ability.HCGM = hcgmCoef;
//        ability.HCGMFollowupDone = true;
//        progressMade = true;
//    }

//} while (progressMade);