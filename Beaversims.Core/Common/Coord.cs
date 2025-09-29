using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    internal record Coord
    {
        public double x;
        public double y;
        public Coord(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
