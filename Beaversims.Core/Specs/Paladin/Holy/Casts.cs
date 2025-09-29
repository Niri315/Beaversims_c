using Beaversims.Core.Shared;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Paladin.Holy
{
    internal class CastProcessor
    {

        private static double ApplyReductEffects(CastEvent cEvt, User user, double castTime)
        {
            return castTime;
        }

        public static void ProcessCast(Event evt, User user)
        {
            if (evt is CastEvent cEvt && evt.SourceUnit is User)
            {
                HolyPaladin userSpec = (HolyPaladin) user.Spec;

                var castTime = cEvt.Ability.CastTime;
                if (castTime > 0)
                {
                    castTime = ApplyReductEffects(cEvt, user, castTime);
                    HCGM.CastTimeGains(cEvt, user, castTime);
                }
            }
        }
    }
}
