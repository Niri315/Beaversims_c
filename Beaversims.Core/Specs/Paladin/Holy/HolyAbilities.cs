using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HST = Beaversims.Core.HasteScalerType;
using ST = Beaversims.Core.ScalerType;

namespace Beaversims.Core.Specs.Paladin.Holy.Abilities
{
    internal abstract class HpalAbility : Ability
    {
    }

    internal class AvengingCrusader : HpalAbility
    {
        public const string Name_s = "Avenging Crusader";
        public override string Name { get; set; } = Name_s;
        public override double ManaCost_p { get; set; } = 0.03;
        public override double CastTime { get; set; } = Constants.GCD;
        public override bool Direct {  get; set; } = true;
        public override HashSet<ST> Scalers { get; set; } = new() { ST.Int, ST.Crit, ST.Haste, ST.Mastery, ST.Vers };
        public override HashSet<HST> HasteScalers { get; set; } = new() { HST.Cast };

    }

    internal class Consecration : HpalAbility
    {
        public const string Name_s = "Consecration";
        public override string Name { get; set; } = Name_s;
        public override bool ForceTick {  get; set; } = true;  //Behaves like tick.

    }

    internal class CrusaderStrike : HpalAbility
    {
        public const string Name_s = "Crusader Strike";
        public override string Name { get; set; } = Name_s;
        public override double ManaCost_p { get; set; } = 0.006;
        public override double CastTime { get; set; } = Constants.GCD;
        public override bool Direct { get; set; } = true;
        public override HashSet<ST> Scalers { get; set; } = new() { ST.Int, ST.Crit, ST.Haste, ST.Vers };
        public override HashSet<HST> HasteScalers { get; set; } = new() { HST.Cast };

    }
    internal class Judgment : HpalAbility
    {
        public const string Name_s = "Judgment";
        public override string Name { get; set; } = Name_s;
        public override double ManaCost_p { get; set; } = 0.0168;
        public override double CastTime { get; set; } = Constants.GCD;
        public override bool Direct { get; set; } = true;
        public override HashSet<ST> Scalers { get; set; } = new() { ST.Int, ST.Crit, ST.Haste, ST.Vers };
        public override HashSet<HST> HasteScalers { get; set; } = new() { HST.Cast };
    }


}

