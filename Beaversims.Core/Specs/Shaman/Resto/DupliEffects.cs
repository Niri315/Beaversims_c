using Beaversims.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Beaversims.Core.Specs.Shaman.Resto.DupliEffects
{

    //internal class RestorativeMists : DupliEffect
    //{
    //    public override bool IsProcEvt(TpEvent evt, User user)
    //    {
    //        if (evt.IsHealDoneEvent() &&
    //            user.HasBuff(Abilities.Ascendance._buffId))
    //        {
    //            return true;
         
    //        }
    //        return false;
    //    }

    //    public override double HypoFormula(TpEvent evt, User user)
    //    {
    //        return Ability_de.DeCoef1;
    //    }

    //    public RestorativeMists(Ability ability) : base(ability)
    //    {
    //    }
    //}

    internal class WhisperingWaves : DupliEffect
    {
        // Todo track riptide count for higher accuracy - low prio
        private double Coef { get; set; } = Abilities.WhisperingWaves.coef;

        public override bool IsProcEvt(TpEvent evt, User user)
        {
            if (evt.IsHealDoneEvent() &&
                evt.AbilityName == Abilities.HealingWave.name)
            {
                return true;

            }
            return false;
        }

        public override double HypoFormula(TpEvent evt, User user)
        {
            return Coef;
        }

        public WhisperingWaves(Ability ability) : base(ability)
        {
        }
    }
    internal class AncestralAwakening : DupliEffect
    {
        private double ProcChance { get; set; } = Talents.AncestralAwakening.procChance;
        private double CritProcChance { get; set; } = Talents.AncestralAwakening.critProcChance;
        private double Coef { get; set; }
        
        // No ticks
        // No pets

        private static readonly HashSet<string> abilities = [Abilities.HealingWave.name, Abilities.Riptide.name];

        public override bool IsProcEvt(TpEvent evt, User user)
        {
            if (evt.IsHealDoneEvent() &&
                abilities.Contains(evt.AbilityName) &&
                !evt.Tick &&
                evt.SourceUnit is User)
            {
                return true;

            }
            return false;
        }

        public override double HypoFormula(TpEvent evt, User user)
        {
            if (evt.Crit)
            {
     
                return CritProcChance * Coef;
            };
            return ProcChance * Coef;
        }

        public AncestralAwakening(Ability ability, double coef) : base(ability)
        {
            Coef = coef;
        }
    }
}
