using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Sim
{
    internal class ScUtils
    {

        public static double GetEffectRPP(int ilvl, int scalingClass)
        {
            var rpp = Data.Dbc.RandomPropPoints.Data[ilvl + 1];
            double effectRpp = scalingClass switch
            {
                -1 => rpp.Epic[0],
                -7 => rpp.Epic[0], 
                -8 => rpp.DamageSecondary,
                -9 => rpp.DamageReplaceStat,
                _ => 0 
            };

            return effectRpp;
        }

        //public static double GetItemRPP(int ilvl, int scalingClass)
        //{
        //    var rpp = Data.Dbc.RandomPropPoints.Data[ilvl + 1];
        //    double effectRpp = scalingClass switch
        //    {
        //        -1 => rpp.Epic[0],
        //        -7 => rpp.Epic[0],
        //        -8 => rpp.DamageSecondary,
        //        -9 => rpp.DamageReplaceStat,
        //        _ => 0
        //    };

        //    return effectRpp;
        //}
    }
}
