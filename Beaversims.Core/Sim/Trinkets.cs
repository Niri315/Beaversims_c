using Beaversims.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Sim
{
    internal class AstralAntenna : EffectItem
    {
        public ScalingData ScalingData { get; set; } = new ScalingData(-7, 1.558467 * 0.97);
        public AstralAntenna() { }
    }
}
