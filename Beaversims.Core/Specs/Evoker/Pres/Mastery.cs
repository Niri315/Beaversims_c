using Beaversims.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Evoker.Pres
{ 
    internal static class MasteryTracker
        
    {  
        // Wowanalyzer are not including absorbs when parsing the events hence the discrepency in event count.

        public static void SetMasteryEff(Event evt, User user)
        {
          if (evt is HealEvent hEvt && evt.UserSuperSource)
            {
                //if (hEvt.AbsorbAbility)
                //Console.WriteLine($"{evt.Timestamp} - {evt.AbilityName}");
                //if (hEvt.Amount.Raw <= 0)
                //{
                //    return;
                //}
                //if (evt.SourceUnit.Name != user.Name)
                //{
                //    Console.WriteLine(evt.SourceUnit.Name);
                //}
                user.MasteryTest2++;
                if (evt.TargetUnit is User)
                {
                    hEvt.MasteryActive = true;
                    user.MasteryTest1++;
                }
                else
                {
                    var targetHp_p = (evt.TargetHp - hEvt.Amount.Naeff) / evt.TargetMaxHp;
                    var userHp_p = evt.SourceHp_p();
                    if (userHp_p >= targetHp_p)
                    {
                        hEvt.MasteryActive = true;
                        user.MasteryTest1++;
                    }
                    //Console.WriteLine($"{evt.Timestamp} - user: {userHp_p}, user total: {evt.SourceHp_p()} target: {evt.TargetUnit.Name} target hp_p: {targetHp_p} target hp total: {evt.TargetHp}");
                    //Console.WriteLine($"Mastery eff: {hEvt.masteryEffectiveness}");
                }


            }
        }

        public static void MasteryGains(HealEvent evt, User user, int i, bool antiGain = false)
        {
            if (!evt.MasteryActive) return;
            var statName = StatName.Mastery;
            var stat = (Mastery)evt.UserStats.Get(statName);
            Shared.StatGains.SecondaryAltAmount(evt, stat, i);
        }
    }
}
