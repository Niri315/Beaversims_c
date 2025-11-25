using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Shaman.Resto.Talents
{
    internal class ImprovedEarthlivingWeapon : Talent
    {
        public const int id = 101936;
        public const double coef = 1.5;
        public const string ability = Abilities.EarthlivingWeapon.name;
        public ImprovedEarthlivingWeapon(int rank) : base(id, rank)
        {

        }
    }

    internal class AncestralAwakening : Talent
    {
        public const int id = 101927;
        public double Coef {  get; set; }
        public const double procChance = 0.3;
        public const double critProcChance = 0.6;
        public AncestralAwakening(int rank) : base(id, rank)
        {
            Coef = rank * 0.25;
        }
    }
}


