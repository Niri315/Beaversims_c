using Beaversims.Core.Shared;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Druid.Resto
{
    internal class CastProcessor
        // Obs ! 
    {
        private static double ApplyReductEffects(CastEvent evt, User user, double castTime)
        {
            if (Abilities.NaturesSwiftness.abilities.Contains(evt.AbilityName) && user.HasBuff(Abilities.NaturesSwiftness.buffId))
            {
                return Constants.GCD;
            }
            if (evt.UserHasHotw)
            {
                var ability = (Abilities.RestoAbility)evt.Ability;
                if (ability.BalanceSpell)
                {
                    castTime *= (1 - Abilities.HeartOfTheWild.balanceSpellsCTCoef);
                }
            }
            return castTime;
        }



        public static void ProcessCast(Event evt, User user)
        {
            if (evt is CastEvent cEvt && evt.SourceUnit is User)
            {

                //Console.WriteLine($"{evt.Timestamp}: {evt.Ability.Name}");

                var ability = cEvt.Ability;
                var castTime = ability.CastTime;
                //Console.WriteLine($"{evt.Timestamp}: {evt.Ability.Name} - Cast time: {castTime}");

                if (castTime > 0 && !ability.ZeroHasteCTG)
                {
                    var postReductCT = ApplyReductEffects(cEvt, user, castTime);
                    //cEvt.PostReductCT = postReductCT;
                    var scalingReductRatio = postReductCT / castTime;
                    //cEvt.ScalingReductRatio = scalingReductRatio;
                    Shared.CIM.CastTimeGains(cEvt, user, postReductCT, scalingReductRatio);
                }
            }
        }
    }
}
