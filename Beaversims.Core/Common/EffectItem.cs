using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    internal class ScalingData
    {
        public double Coef { get; set; }
        public int Class { get; set; }
        public ScalingData(int _class, double coef)
        {
            Class = _class;
            Coef = coef;
        }
    }

    internal class EffectItem
    {
    }
}
