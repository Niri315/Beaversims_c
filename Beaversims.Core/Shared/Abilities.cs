using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Beaversims.Core.Shared.Abilities
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
            LeechSource = false;
            CanDupli = false;
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

    internal class BlessingOfSummer : SharedAbility
    {
        public const string name = "Blessing of Summer";
        public const int buffId = 388007;
        public double Coef { get; set; } = 0.12;
        public BlessingOfSummer()
        {
            Name = name;
        }
    }

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
    /* ------ *
     * Shaman *
     * ------ */
    internal class SpiritLink : SharedAbility
    {
        public const string name = "Spirit Link";
        public SpiritLink()
        {
            Name = name;
            IgnoreDr = true;
            LeechSource = false;
            CanDupli = false;
        }
    }
    /* --- *
     * WW3 *
     * --- */
    internal class EtherealReconstitution : SharedAbility
    {
        public const string name = "Ethereal Reconstitution";
        public EtherealReconstitution()
        {
            Name = name;
            Scalers.UnionWith([SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Auto]);
        }
    }
    internal class EtherealGuard : SharedAbility
    {
        public const string name = "Ethereal Guard";
        public EtherealGuard()
        {
            Name = name;
            Scalers.UnionWith([SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Auto]);
            DerivedCritScaler = true;
            SourceAbility = EtherealReconstitution.name;
        }
    }
    internal class VoidglassBarrier : SharedAbility
    {
        public const string name = "Voidglass Barrier";
        public VoidglassBarrier()
        {
            Name = name;
            Scalers.UnionWith([SN.Vers]);
        }
    }
    internal class InvigoratingHealingPotion : SharedAbility
    {
        public const string name = "Invigorating Healing Potion";
        public InvigoratingHealingPotion()
        {
            Name = name;
            Scalers.UnionWith([SN.Vers]);
        }
    }
    internal class LoomitharsLivingSilk : SharedAbility
    {
        public const string name = "Loom'ithar's Living Silk";
        public LoomitharsLivingSilk()
        {
            Name = name;
            Scalers.UnionWith([SN.Vers]);
        }
    }
}
