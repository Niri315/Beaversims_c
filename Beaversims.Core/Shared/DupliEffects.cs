using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Beaversims.Core.Shared.DupliEffects
{

    internal class Leech : DupliEffect
    {

        public override bool IsProcEvt(TpEvent evt, User user)
        {
            //return true;
            if (!evt.FullyAbsorbed &&
                evt.TargetUnit is not User &&
                evt.SourceUnit is User &&
                evt.Ability.LeechSource)
            {
                return true;

            }
            return false;
        }

        public override double HypoFormula(TpEvent evt, User user)
        {
            if (evt.Amount.Raw == 0)
            {
                return 0;
            }
            var leechStat = (Core.Leech)evt.UserStats.Get(SN.Leech);
            return (evt.Amount.Naeff / evt.Amount.Raw) * ((leechStat.TrueEff() / leechStat.PercentRate) / 100);
        }

        public Leech(Ability ability) : base(ability)
        {
        }
    }


    internal class Summer : DupliEffect
    {
        private Shared.Abilities.BlessingOfSummer SummerAbility
    => (Shared.Abilities.BlessingOfSummer)DupliAbility;

        public override bool IsProcEvt(TpEvent evt, User user)
        {
            return
                DupliAbility.Heal.Raw > 0
                && evt.SummerActive
                && evt.SourceUnit is User
                && !evt.AbsorbAbility
                && evt.Ability.Name != Abilities.BlessingOfSummer.name;
        }

        public override double HypoFormula(TpEvent evt, User user)
        {
            if (evt.Amount.Raw == 0)
            {
                return 0;
            }
            return SummerAbility.Coef;
              
        }

        public Summer(Shared.Abilities.BlessingOfSummer ability) : base(ability)
        {
        }
    }


}


