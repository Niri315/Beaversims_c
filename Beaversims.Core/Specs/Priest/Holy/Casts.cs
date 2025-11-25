using Beaversims.Core.Shared;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Priest.Holy
{
    internal class CastProcessor
    {
        private static double ApplyReductEffects(CastEvent evt, User user, double castTime)
        {
            return castTime;
        }



        public static void ProcessCast(Event evt, User user)
        {
            if (evt is CastEvent cEvt)
            {

                var ability = cEvt.Ability;
                var castTime = ability.CastTime;

                if (castTime > 0 && !ability.ZeroHasteCTG)
                {
                    var postReductCT = ApplyReductEffects(cEvt, user, castTime);
                    var scalingReductRatio = postReductCT / castTime;

                    Shared.CIM.CastTimeGains(cEvt, user, postReductCT, scalingReductRatio);
                }
            }
        }
    }
}
