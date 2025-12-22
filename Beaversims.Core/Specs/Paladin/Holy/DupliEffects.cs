using Beaversims.Core;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Beaversims.Core.Specs.Paladin.Holy.DupliEffects
{
    internal class BeaconOfLight : DupliEffect
    {
        private Abilities.BeaconOfLight BeaconAbility
    => (Abilities.BeaconOfLight)DupliAbility;

        public override bool IsProcEvt(TpEvent evt, User user)
        { 
            if (evt.IsHealDoneEvent() && evt.Ability.Direct && evt.SourceUnit is User && !evt.Tick && evt.Ability.ClassAbility)
            {
               
                return true;
            }
             
            return false;
        }
          
        public override double HypoFormula(TpEvent evt, User user)
        {
   
            return BeaconAbility.Coef * evt.BeaconCount;
        }
        public BeaconOfLight(Abilities.BeaconOfLight ability) : base(ability)
        {
        }
    }
    internal class SelflessHealer : DupliEffect
    {
        private Talents.SelflessHealer SelflessTalent
    => (Talents.SelflessHealer)Talent;

        public override bool IsProcEvt(TpEvent evt, User user)
        {  
            if (user.HasTalent(Talents.SelflessHealer.id))
            {
                var selfless_t = (Talents.SelflessHealer)user.Talents[Talents.SelflessHealer.id];
                if (selfless_t.SourceAbilities.Contains(evt.AbilityName) && evt.SourceUnit is User)
                {
                    return true;
                }
            }
            return false;
        }

        public override double HypoFormula(TpEvent evt, User user)
        {


            return SelflessTalent.Coef;

        }
        public SelflessHealer(Abilities.SelflessHealer ability, Talents.SelflessHealer talent) : base(ability, talent)
        {

        }
    }
}
