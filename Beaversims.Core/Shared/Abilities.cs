using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Beaversims.Core.Abilities
{
    /* -------*
     * Common *
     * -------*/

    internal class Leech : SharedAbility
    {
        public const string name = "Leech";
        public Leech() 
        { 
            Name = name;
        }
    }

    internal class Melee : SharedAbility
    {
        public const string name = "Melee";
        public Melee()
        {
            Name = name;
            Scalers.UnionWith([SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Auto]);
        }
    }

    /* ------- *
     * Paladin *
     * ------- */
    internal class HolyBulwark : SharedAbility
    {
        public const string name = "Holy Bulwark";
        public HolyBulwark()
        {
            Name = name;
            CastTime = Constants.GCD;
            SuppStamScaler = true;
        }
    }

    internal class LesserBulwark : SharedAbility
    {
        public const string name = "Lesser Bulwark";
        public LesserBulwark()
        {
            Name = name;
            CastTime = Constants.GCD;
            SuppStamScaler = true;
        }
    }

    internal class LightforgedBlessing : SharedAbility
    {
        public const string name = "Lightforged Blessing";
        public LightforgedBlessing()
        {
            Name = name;
            CastTime = Constants.GCD;
            SuppStamScaler = true;
        }
    }
    /* ------- *
     * Warlock *
     * ------- */
    internal class Healthstone : SharedAbility
    {
        public const string name = "Healthstone";
        public Healthstone()
        {
            Name = name;
            CastTime = Constants.GCD;
            SuppStamScaler = true;
        }
    }
}
