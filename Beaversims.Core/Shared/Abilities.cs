using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace Beaversims.Core.Shared.Abilities
{
    /* --------*
     * Dummies *
     * --------*/
    internal class ZeroCIMDummy : SharedAbility
    {
        public const string name = "ZeroCIMDummy";
        public ZeroCIMDummy()
        {
            Name = name;
        }
    }

    /* -------*
     * Common *
     * -------*/

    internal class Leech : SharedAbility
    {
        public const string name = "Leech";
        public override double HypoTrueRawR()
        {
            return base.HypoTrueRawR();
        }
        public Leech() 
        { 
            Name = name;
            LeechSource = false;
            CanDupli = false;
            SimDupliAbility = true;
            DupliEffectType = DupliEffectType.Heal;
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
            SimDupliAbility = true;
            DupliEffectType = DupliEffectType.Reverse;
        }
    }

    internal class HolyBulwark : SharedAbility
    {
        public const string name = "Holy Bulwark";
        public const int buffId = 432496;  // NOT the id of the buff that hold the absorb.
        public HolyBulwark()
        {
            Name = name;
            CastTime = Constants.GCD;
            SuppStamScaler = true;
            Scalers.UnionWith([SN.Haste]);
            HasteScalers.UnionWith([HST.Auto]);  // Divine Inspiration.
            ClassAbility = true;
        }
    }

    internal class LesserBulwark : SharedAbility
    {
        public const string name = "Lesser Bulwark";
        public LesserBulwark()
        {
            Name = name;
            SuppStamScaler = true;
            ClassAbility = true;
        }
    }

    internal class LightforgedBlessing : SharedAbility
    {
        public const string name = "Lightforged Blessing";
        public LightforgedBlessing()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Haste]);
            //HasteScalers.UnionWith([HST.Cast]); // Adding in hpal
            //CIMSources.Add(new CIMSource(Specs.Paladin.Holy.Abilities.ShieldOfTheRighteous.name, 1.0));
            SuppStamScaler = true;
            ClassAbility = true;
        }
    }
    internal class GlisteningRadiance : SharedAbility
    {
        public const string name = "Glistening Radiance";
        public const int buffId = 432496;  // NOT the id of the buff that hold the absorb.
        public GlisteningRadiance()
        {
            Name = name;
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
            ClassAbility = true;
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
            ClassAbility = true;
        }
    }
    /* ------ *
     * Evoker *
     * ------ */

    internal class TimeDilation : SharedAbility
    {
        public const string name = "Time Dilation";
        public TimeDilation()
        {
            Name = name;
            LeechSource = false;
            CanDupli = false;
            ClassAbility = true;
        }
    }

    /* ------- *
     * Racials *
     * ------- */
    internal class LightsJudgment : SharedAbility
    {
        public const string name = "Light's Judgment";
        public LightsJudgment()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers]);
        }
    }

    /* ----------- *
     * Timewalking *
     * ----------- */
    internal class BlazeOfLife : SharedAbility
    // Eye of Blazing Power
    {
        public const string name = "Blaze of Life";
        public BlazeOfLife()
        {
            Name = name;
            Scalers.UnionWith([SN.Vers, SN.Crit]);
            SimImpurity = true;
        }
    }
    /* -------- *
     * Midnight *
     * -------- */

    internal class ConsecratedChalice : SharedAbility
    {
        public const string name = "Consecrated Chalice";
        public ConsecratedChalice()
        {
            Name = name;
            Scalers.UnionWith([SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Auto]);
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
            SimImpurity = true;
        }
    }
}
