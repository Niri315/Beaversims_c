using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    public enum ScalerType { Prim3, Int, Stam, Vers, Crit, Mastery, Haste, Highest }
    public enum HasteScalerType { Cast, Auto }

    internal abstract class Stat
    {
        public double Rating { get; set; } = 0;
        public double Base { get; set; } = 0;
        public double Multi { get; set; } = 0;
        public double PostDr { get; set; } = 0;
        public double Eff { get; set; } = 0;
        public int Bracket { get; set; } = 0;

        public abstract int Level { get; set; }

    }

    internal class Intellect : Stat
    {
        public ScalerType Name {  get; set; } = ScalerType.Int;
        public override int Level { get; set; } = 1;
    }

    internal class Versatility : Stat
    {
        public ScalerType Name { get; set; } = ScalerType.Vers;
        public override int Level { get; set; } = 2;
    }
}
