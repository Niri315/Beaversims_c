using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    internal readonly struct RandomPropData
    {
        public readonly int Ilvl;
        public readonly float DamageReplaceStat;
        public readonly float DamageSecondary;

        // Epic
        public readonly float E0, E1, E2, E3, E4;
        // Rare
        public readonly float R0, R1, R2, R3, R4;
        // Uncommon
        public readonly float U0, U1, U2, U3, U4;

        public RandomPropData(
            int ilvl, float drs, float ds,
            float e0, float e1, float e2, float e3, float e4,
            float r0, float r1, float r2, float r3, float r4,
            float u0, float u1, float u2, float u3, float u4)
        {
            Ilvl = ilvl; DamageReplaceStat = drs; DamageSecondary = ds;
            E0 = e0; E1 = e1; E2 = e2; E3 = e3; E4 = e4;
            R0 = r0; R1 = r1; R2 = r2; R3 = r3; R4 = r4;
            U0 = u0; U1 = u1; U2 = u2; U3 = u3; U4 = u4;
        }
    }

    //internal readonly struct ItemData
    //{
    //    public readonly int Id;

    //    public ItemData(
    //        int id)
    //    {
    //        Id = id;
    //    }
    //}

}

