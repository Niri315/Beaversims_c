
using Beaversims.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


// OBS! TODO
// Current way of checking haste cap doesnt take trinket effects into account
// We could reset castTimeCap before the sim iterations and set the avg value.
// Or we do a total rework and make everything gear dependant.
// Whatever solution we decide on it should be one that is sparing for computing time
// giga minmaxing is not worth if sim time blows up.
// Whatever we do we should incorporate crit cap for rdruid etc.


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

            // TODO doesnt take sim stat incs into account currently...
            for (int i = 0; i < evt.AltEvents.Count; i++)
            {


                var trueCastTime_i = Calc.TrueCastTimeCalc((Haste)evt.AltEvents[i].UserStats.Get(StatName.Haste), castTime);
                if (trueCastTime_i < Constants.castTimeCap)
                {
                    //user.AltGearSets[i].HasteCapCTLoss += Constants.castTimeCap - trueCastTime_i;
                    user.AltGearSets[i].HasteCapCTGLoss += gain;
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
                // We need to sort scalingCTgain for shaman for sure..
                // For druid/paladin doesnt really matter.
                //ability.MaxCIM *= ability.ScalingCTGain / ability.CTGain;  // This doesnt work...
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
    }
}

