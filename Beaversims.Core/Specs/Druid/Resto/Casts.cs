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
    {
        private static double ApplyReductEffects(CastEvent evt, User user, double castTime)
        {
            if (Abilities.NaturesSwiftness.abilities.Contains(evt.AbilityName) && user.HasBuff(Abilities.NaturesSwiftness.buffId))
            {
                return Constants.GCD;
            }
            return castTime;
        }



        public static void ProcessCast(Event evt, User user)
        {
            if (evt is CastEvent cEvt && evt.SourceUnit is User)
            {
                var ability = cEvt.Ability;
                var castTime = ability.CastTime;

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
