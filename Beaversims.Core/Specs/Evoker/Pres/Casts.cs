using Beaversims.Core.Shared;
using Beaversims.Core.Specs.Evoker.Pres.Abilities;
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
            bool abilityGcd = evt.Ability.GCD;
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
            if (evt.Ability is PresAbility presAbility && presAbility.EmpAbility && evt.EmpCastLevel == 0)
            {
                if (user.HasBuff(Abilities.TipTheScales.buffId))
                {
                    evt.EmpCastLevel = user.MaxEmpLevel;
                    castTime = Constants.GCD;
                    abilityGcd = true;
                }
                else
                {
                    evt.RemoveMe = true;
                    return 0;
                }
            }

            if (user.HasBuff(Talents.AncientFlame.buffId) && Talents.AncientFlame.affectedSpells.Contains(evt.AbilityName))
            {
                castTime *= 1 - Talents.AncientFlame.coef;
   
            }
      

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



                if (evt.AbilityId == Abilities.Stasis.releaseCastId)
                {
                    user.LastStasisRelease = evt.Timestamp;
                    Console.WriteLine("----");
                    foreach (var abilityName in user.StasisStore)
                    {
                        var _ability = (Abilities.PresAbility)user.Abilities.Get(abilityName);
                        _ability.StasisCount++;
                        Console.WriteLine(abilityName);
                        
                    }
                }


                if (evt.Timestamp > user.LastStasisRelease + 1 && evt.Timestamp < user.LastStasisRelease + 30)
                {
                    user.StasisStore = [];
                }
                if (user.StasisStore.Contains(evt.AbilityName) && user.LastStasisRelease > evt.Timestamp - 1)
                {
                    var _ability = (Abilities.PresAbility)user.Abilities.Get(evt.AbilityName);
                    _ability.StasisCount++;
                    user.StasisStore.Remove(evt.AbilityName);
                    evt.RemoveMe = true;
                    return;
                    //Console.WriteLine($" Stasis Release Cast: {evt.Timestamp} - {evt.AbilityName}");
                    //Console.WriteLine($"user.LastStasisRelease - {user.LastStasisRelease}");
                }


                if (castTime > 0 && !ability.ZeroHasteCTG)
                {
                    var postReductCT = ApplyReductEffects(cEvt, user, castTime);
                    if (evt.RemoveMe) return;
                    //cEvt.PostReductCT = postReductCT;
                    var scalingReductRatio = postReductCT / castTime;
                    //cEvt.ScalingReductRatio = scalingReductRatio;
                    Shared.CIM.CastTimeGains(cEvt, user, postReductCT, scalingReductRatio);
                }
            }
        }
    }
}
