using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HST = Beaversims.Core.HasteScalerType;
using ST = Beaversims.Core.ScalerType;

namespace Beaversims.Core
{
    internal abstract class SharedAbility : Ability
    {
    }

    internal class Leech : SharedAbility
    {
        public const string Name_s = "Leech";
        public override string Name { get; set; } = Name_s;
    }
    internal class Melee : SharedAbility
    {
        public const string Name_s = "Melee";
        public override string Name { get; set; } = Name_s;
        public override HashSet<ST> Scalers { get; set; } = new() {ST.Crit, ST.Haste, ST.Vers };
        public override HashSet<HST> HasteScalers { get; set; } = new() { HST.Auto };
    }
}
