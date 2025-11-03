
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

        public static void CastTimeGains(CastEvent evt, User user, double castTime, double scalingRatio)
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
            ability.CTGain += gain;
            ability.ScalingCTGain += gain * scalingRatio;
            user.CastTimeGain += gain;
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
                if (ability.CTGain > 0)
                {
                    totalCastTimeGainCIM += ability.CTGain * ability.CIM;
                    totalCastTimeGain += ability.CTGain;
                }

            }
            var tot = totalCastTimeGainCIM - totalCastTimeGain;
            const double tolerance = 1e-13;
            if (Math.Abs(tot) > tolerance)
            {
                throw new InvalidOperationException(
                    $"CIM Imbalance: {tot}"
                );
            }

        }

        public static void SetCIM(User user)
        {
            var abilities = user.Abilities;

            foreach (var ability in abilities)
            {
                ability.CIMSourceRelCheck();
                ability.MaxCIM = Math.Max((user.TrueCastTimeTotal - user.HasteCapCTLoss) / ability.CdTimeHypo, 1.0);
                ability.MaxCIM *= ability.ScalingCTGain / ability.CTGain;
                //Console.WriteLine(ability.MaxCIM);
            }

            double remTCTG = user.CastTimeGain;
            double remTarget = user.CastTimeGain; 

            var tempAbilities = new List<Ability>(abilities);
            foreach (var ability in tempAbilities)
            {
                if (!ability.HasteScalers.Contains(HST.Cast) || ability.CTGain == 0 || ability.ZeroCIM)
                {
                    ability.CIMInitDone = true;
                    ability.CIM = 0;
                    remTCTG -= ability.CTGain;
                }
            }
            tempAbilities.RemoveAll(a => a.CIMInitDone);

            foreach (var ability in tempAbilities)
            {
                if (ability.RestRelCIM)
                {
                    remTCTG -= (1.0 - ability.RestRelCIMRatio) * ability.CTGain;
                }
            }

            while (true)
            {
                if (remTCTG <= 0) break;

                double restCIM = remTarget / remTCTG;
                bool anyChange = false;

                for (int i = tempAbilities.Count - 1; i >= 0; --i)
                {
                    var ability = tempAbilities[i];
                    if (ability.CIMInitDone) continue;

                    double proposed = restCIM * ability.RestRelCIMRatio;

                    if (proposed > ability.MaxCIM)
                    {
                        ability.CIM = ability.MaxCIM;
                        remTarget -= ability.CTGain * ability.CIM;  
                        remTCTG -= (1.0 - (ability.RestRelCIM ? ability.RestRelCIMRatio : 0.0)) * ability.CTGain;
                        ability.CIMInitDone = true;
                        tempAbilities.RemoveAt(i);
                        anyChange = true;
                    }
                    else
                    {
                        ability.CIM = proposed;
                    }
                }
                if (!anyChange) break;
            }
            TestCIMMath(user);
        }


        //var tempAbilities = new List<Ability>(abilities);
        //foreach (var ability in tempAbilities)
        //{
        //    if (!ability.HasteScalers.Contains(HST.Cast) || ability.CastTimeGain == 0 || ability.ZeroCIM)
        //    {
        //        ability.CIMInitDone = true;
        //        ability.CIM = 0;
        //        remTCTG -= ability.CastTimeGain;
        //    }
        //}
        //tempAbilities.RemoveAll(a => a.CIMInitDone);
        //for (int m = 0; m < 100; m++) {
        //    Console.WriteLine($"remTCTG: {remTCTG}");
        //    foreach (var ability in tempAbilities)
        //    {
        //        var restCIM = user.CastTimeGain / remTCTG;
        //        if (ability.MaxCIM < restCIM)
        //        {
        //            ability.CIM = ability.MaxCIM;
        //            Console.WriteLine($"Pre REM : {remTCTG}");
        //            remTCTG -= (restCIM - ability.MaxCIM) * ability.CastTimeGain;
        //            remTCTG -= (restCIM - ability.MaxCIM) * ability.CastTimeGain;
        //            Console.WriteLine($"Ability: {ability.Name}. Removed: {(restCIM - ability.MaxCIM) * ability.CastTimeGain}. CIM {ability.CIM}, MaxCIM: {ability.MaxCIM}. CTG: {ability.CastTimeGain}");
        //            Console.WriteLine($"Post REM : {remTCTG}");
        //            ability.CIMInitDone = true;
        //        }

        //        //else if (ability.RestRelCIM)
        //        //{
        //        //    ability.CIM = restCIM * ability.RestRelCIMRatio;
        //        //    remTCTG -= (restCIM - (restCIM * ability.RestRelCIMRatio)) * ability.CastTimeGain;
        //        //}
        //        else
        //        {
        //            ability.CIM = restCIM;
        //        }
        //        Console.WriteLine($"Rest CIM: {restCIM}");
        //    }
        //    tempAbilities.RemoveAll(a => a.CIMInitDone);
        //    Console.WriteLine($"remTCTG POST: {remTCTG}");
        //}
        //foreach (var ability in abilities)
        //{
        //    ability.CIM = ability.TCTG * (ability.TrueCastTimeTotal / user.TrueCastTimeTotal);
        //}
        //bool anyUpdated;
        //do
        //{
        //    scaleGain = 0;
        //    nonScaleGain = 0;

        //    foreach (var ability in abilities)
        //    {
        //        if (ability.HasteScalers.Contains(HST.Cast) && ability.CastTimeGain > 0 && !ability.ZeroCIM)
        //        {
        //            scaleGain += ability.CastTimeGain * ability.CIMRatio;
        //        }
        //        else
        //        {
        //            nonScaleGain += ability.CastTimeGain;
        //        }
        //    }

        //    var totalGain = scaleGain + nonScaleGain;
        //    var cimRest = 0.0;
        //    if (scaleGain > 0)
        //    {
        //        cimRest = totalGain / scaleGain;
        //    }

        //    anyUpdated = false;

        //    foreach (var ability in user.Abilities)
        //    {
        //        if (ability.MaxCIM < cimRest && !ability.CIMInitDone && ability.HasteScalers.Contains(HST.Cast) && ability.CastTimeGain > 0 && !ability.ZeroCIM)
        //        {
        //            ability.CIMInitDone = true;
        //            anyUpdated = true;
        //        }

        //        if (ability.CIMInitDone)
        //        {
        //            ability.CIMRatio = scaleGain * (ability.MaxCIM - 1) / nonScaleGain;
        //        }
        //    }
        //}
        //while (anyUpdated);


        //scaleGain = 0;
        //nonScaleGain = 0;

        //foreach (var ability in abilities)
        //{
        //    if (ability.HasteScalers.Contains(HST.Cast) && ability.CastTimeGain > 0 && !ability.ZeroCIM)
        //    {

        //        scaleGain += ability.CastTimeGain * ability.CIMRatio;
        //    }
        //    else
        //    {
        //        nonScaleGain += ability.CastTimeGain;
        //    }
        //}


        //if (scaleGain > 0)
        //{
        //    foreach (var ability in abilities)
        //    {
        //        if (ability.HasteScalers.Contains(HST.Cast) && ability.CastTimeGain > 0 && !ability.ZeroCIM)
        //        {
        //            ability.CIM = (scaleGain + (nonScaleGain * ability.CIMRatio)) / (scaleGain);
        //            ability.CIMInitDone = true;
        //        }
        //        else
        //        {
        //            ability.CIM = 0;
        //        }


        //    }
        //}


        //public static void SetCIM(User user)
        //{

        //    var abilities = user.Abilities;
        //    double scaleGain = 0;
        //    double nonScaleGain = 0;

        //    foreach (var ability in user.Abilities)
        //    {
        //        ability.CIMSourceRelCheck();
        //        ability.MaxCIM = Math.Max((user.TrueCastTimeTotal - user.HasteCapCTLoss) / ability.CdTimeHypo, 1.0);
        //    }

        //    bool anyUpdated;
        //    do
        //    {
        //        scaleGain = 0;
        //        nonScaleGain = 0;

        //        foreach (var ability in abilities)
        //        {
        //            if (ability.HasteScalers.Contains(HST.Cast) && ability.CastTimeGain > 0 && !ability.ZeroCIM)
        //            {
        //                scaleGain += ability.CastTimeGain * ability.CIMRatio;
        //            }
        //            else
        //            {
        //                nonScaleGain += ability.CastTimeGain;
        //            }
        //        }

        //        var totalGain = scaleGain + nonScaleGain;
        //        var cimRest = 0.0;
        //        if (scaleGain > 0)
        //        {
        //            cimRest = totalGain / scaleGain;
        //        }

        //        anyUpdated = false;

        //        foreach (var ability in user.Abilities)
        //        {
        //            if (ability.MaxCIM < cimRest && !ability.CIMInitDone && ability.HasteScalers.Contains(HST.Cast) && ability.CastTimeGain > 0 && !ability.ZeroCIM)
        //            {
        //                ability.CIMInitDone = true;
        //                anyUpdated = true;
        //            }

        //            if (ability.CIMInitDone)
        //            {
        //                ability.CIMRatio = scaleGain * (ability.MaxCIM - 1) / nonScaleGain;
        //            }
        //            //if (ability.RestRelCIM)
        //            //{
        //            //    if (ability.CIMRatio != ability.RestRelCIMRatio - 1)
        //            //    {
        //            //        anyUpdated = true;
        //            //    }
        //            //    ability.CIMRatio = ability.RestRelCIMRatio - 1;
        //            //}
        //        }
        //    }
        //    while (anyUpdated);


        //    scaleGain = 0;
        //    nonScaleGain = 0;

        //    foreach (var ability in abilities)
        //    {
        //        if (ability.HasteScalers.Contains(HST.Cast) && ability.CastTimeGain > 0 && !ability.ZeroCIM)
        //        {

        //            scaleGain += ability.CastTimeGain * ability.CIMRatio;
        //        }
        //        else
        //        {
        //            nonScaleGain += ability.CastTimeGain;
        //        }
        //    }


        //    if (scaleGain > 0)
        //    {
        //        foreach (var ability in abilities)
        //        {
        //            if (ability.HasteScalers.Contains(HST.Cast) && ability.CastTimeGain > 0 && !ability.ZeroCIM)
        //            {
        //                ability.CIM = (scaleGain + (nonScaleGain * ability.CIMRatio)) / (scaleGain);
        //                ability.CIMInitDone = true;
        //            }
        //            else
        //            {
        //                ability.CIM = 0;
        //            }


        //        }
        //    }
        //    TestCIMMath(user);
        //}
    }
}

