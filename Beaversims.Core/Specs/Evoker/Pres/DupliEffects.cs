using Beaversims.Core;
using Beaversims.Core.Specs.Evoker.Pres.Abilities;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Beaversims.Core.Specs.Evoker.Pres.DupliEffects
{
    internal class Lifebind : DupliEffect
    {

        public override bool IsProcEvt(TpEvent evt, User user)
        {
            var lifebindBuff = user.GetBuff(Abilities.Lifebind.buffId);

            if (lifebindBuff is Buff
                && evt.LifebindCount > 0
                && lifebindBuff.SourceId == user.Id
                && evt.IsHealDoneEvent()
                && evt.TargetUnit.HasBuff(Abilities.Lifebind.buffId)
                && !evt.AbsorbAbility
                && evt.AbilityName != Abilities.Lifebind.name
                )
            {
                //evt.LifebindEvent = true;

                return true;

            }
            return false;
        }

        public override double HypoFormula(TpEvent evt, User user)
        {
            var count = 0;
            if (evt.TargetUnit is User)
            {
                count = evt.LifebindCount;
            }
            else
            {
                count = 1;
            }
            return Abilities.Lifebind.coef * count;
        }

        public Lifebind(Ability ability) : base(ability)
        {
        }
    }

    internal class Enkindle : DupliEffect
    {

        public override bool IsProcEvt(TpEvent evt, User user)
        {
            if (evt.Ability.Spender)
            {

                return true;
            }
            return false;
        }

        public override double HypoFormula(TpEvent evt, User user)
        {
            return Abilities.Enkindle.coef;
        }

        public Enkindle(Ability ability) : base(ability)
        {
        }
    }
}