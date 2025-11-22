using Beaversims.Core.Shared;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Beaversims.Core.Specs.Evoker.Pres
{
    internal class CastProcessor
    {
        
        private const double empLevelCTInc = 0.75;

        private static double ApplyReductEffects(CastEvent evt, User user, double castTime)
        {
            if (evt.EmpCastLevel > 1)
            {
                var ctPerEmpLevel = empLevelCTInc;
                var tc = user.GetBuff(Talents.TemporalCompression.buffId);
                if (tc is Buff)
                {
                    ctPerEmpLevel *= 1 - (tc.Stacks * Talents.TemporalCompression.coef);
                }
                castTime += (evt.EmpCastLevel - 1) * ctPerEmpLevel;
            }

            if (user.HasBuff(Talents.AncientFlame.buffId) && Talents.AncientFlame.affectedSpells.Contains(evt.AbilityName))
            {
                castTime *= 1 - Talents.AncientFlame.coef;
   
            }
            bool abilityGcd = evt.Ability.GCD;

            if (user.HasBuff(Talents.Lifespark.buffId) && Talents.Lifespark.affectedSpells.Contains(evt.AbilityName))
            {
                abilityGcd = true;
                castTime = Constants.GCD;

            }

            if (user.HasBuff(Talents.FlowState.buffId) && !abilityGcd)
            {
                var fs = (Talents.FlowState)user.Talents[Talents.FlowState.id];
                castTime *= 1 - fs.Coef;
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
