using Beaversims.Core.Shared;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

/*
  TODO LIST

High Prio:
    
    Ascendance (different ability IDS for same name ability)

Mid Prio:


Low Prio:

    
 */

namespace Beaversims.Core.Specs.Shaman.Resto.Abilities
{
    internal abstract class RshamAbility : Ability
    {

    }
    internal class AcidRain : HpalAbility
    {
        public const string name = "Acid Rain";
        public AcidRain()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Auto]);
        }
    }

    internal class CapacitorTotem : HpalAbility
    {
        public const string name = "Capacitor Totem";
        public CapacitorTotem()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class ChainHeal : HpalAbility
    {
        public const string name = "Chain Heal";
        public ChainHeal()
        {
            Name = name;
            CastTime = 2;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            IncludePetCasts = true;
            // Todo Make sure chain heals from totems are included properly with haste cast scaling.
        }
    }
    internal class ChainLightning : HpalAbility
    {
        public const string name = "Chain Lightning";
        public ChainLightning()
        {
            Name = name;
            CastTime = 2;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class Downpour : HpalAbility
    {
        public const string name = "Downpour";
        public Downpour()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Vers]);
        }
    }

    internal class EarthElemental : HpalAbility
    {
        public const string name = "Earth Elemental";
        public EarthElemental()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class EarthShield : HpalAbility
    {
        public const string name = "Earth Shield";
        public EarthShield()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Vers]);
        }
    }

    internal class EarthbindTotem : HpalAbility
    {
        public const string name = "Earthbind Totem";
        public EarthbindTotem()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class EarthgrabTotem : HpalAbility
    {
        public const string name = "Earthgrab Totem";
        public EarthgrabTotem()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class EarthlivingWeapon : HpalAbility
    {
        public const string name = "Earthliving Weapon";
        public EarthlivingWeapon()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Haste, SN.Vers]);
            // TODO should probably have cast on this - but a bit weird with riptide heal as a source.
            // Auto for riptide + cast with CIM for rest?
            HasteScalers.UnionWith([HST.Tick, HST.Auto]); 
        }
    }
    internal class FarSight : HpalAbility
    {
        public const string name = "Far Sight";
        public FarSight()
        {
            Name = name;
            CastTime = 2;
        }
    }
    internal class FlameShock : HpalAbility
    {
        public const string name = "Flame Shock";
        public FlameShock()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Tick]);
        }
    }
    internal class FlametongueAttack : HpalAbility
    {
        public const string name = "Flametongue Attack";
        public FlametongueAttack()
        {
            Name = name;

            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Auto]);
        }
    }

    internal class FlametongueWeapon : HpalAbility
    {
        public const string name = "Flametongue Weapon";
        public FlametongueWeapon()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class FrostShock : HpalAbility
    {
        public const string name = "Frost Shock";
        public FrostShock()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers]);
        }
    }
    internal class GhostWolf : HpalAbility
    {
        public const string name = "Ghost Wolf";
        public GhostWolf()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class GreaterPurge : HpalAbility
    {
        public const string name = "Greater Purge";
        public GreaterPurge()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class GustOfWind : HpalAbility
    {
        public const string name = "Gust of Wind";
        public GustOfWind()
        {
            Name = name;
            GCD = true;
        }
    }
    internal class HealingRain : HpalAbility
    {
        public const string name = "Healing Rain";
        public HealingRain()
        {
            Name = name;
            CastTime = 2;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Auto]);
        }
    }
    internal class HealingSurge : HpalAbility
    {
        public const string name = "Healing Surge";
        public HealingSurge()
        {
            Name = name;
            CastTime = 1.5;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            IncludePetCasts = true;
        }
    }

    internal class HealingStream : HpalAbility
    {
        public const string name = "Healing Stream";
        public HealingStream()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Auto]);
        }
    }

    internal class HealingStreamTotem : HpalAbility
    {
        public const string name = "Healing Stream Totem";
        public HealingStreamTotem()
        {
            Name = name;
            GCD = true;
        }
    }
    internal class HealingWave : HpalAbility
    {
        public const string name = "Healing Wave";
        public HealingWave()
        {
            Name = name;
            CastTime = 2.5;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            IncludePetCasts = true;
        }
    }

    internal class Hex : HpalAbility
    {
        public const string name = "Hex";
        public Hex()
        {
            Name = name;
            CastTime = 1.7;
        }
    }

    internal class LavaBurst : HpalAbility
    {
        public const string name = "Lava Burst";
        public LavaBurst()
        {
            Name = name;
            CastTime = 2;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers]);
        }
    }
    internal class LightningBolt : HpalAbility
    {
        public const string name = "Lightning Bolt";
        public LightningBolt()
        {
            Name = name;
            CastTime = 2;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }
    internal class LightningShield : HpalAbility
    {
        public const string name = "Lightning Shield";
        public LightningShield()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
        }
    }

    internal class NaturesGuardian : HpalAbility
    {
        public const string name = "Nature's Guardian";
        public NaturesGuardian()
        {
            Name = name;
            Scalers.UnionWith([SN.Stamina]);
        }
    }

    internal class OverflowingShores : HpalAbility
    {
        public const string name = "Overflowing Shores";
        public OverflowingShores()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Vers]);
        }
    }

    internal class PoisonCleansingTotem : HpalAbility
    {
        public const string name = "Poison Cleansing Totem";
        public PoisonCleansingTotem()
        {
            Name = name;
            GCD = true;
        }
    }
    internal class PrimalStrike : HpalAbility
    {
        public const string name = "Primal Strike";
        public PrimalStrike()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class Purge : HpalAbility
    {
        public const string name = "Purge";
        public Purge()
        {
            Name = name;
            GCD = true;
        }
    }
    internal class PurifySpirit : HpalAbility
    {
        public const string name = "Purify Spirit";
        public PurifySpirit()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class ReactiveWarding : HpalAbility
    {
        public const string name = "Reactive Warding";
        public ReactiveWarding()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Vers]);
        }
    }

    internal class Riptide : HpalAbility
    {
        public const string name = "Riptide";
        public Riptide()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Tick]);
        }
    }
    internal class Skyfury : HpalAbility
    {
        public const string name = "Skyfury";
        public Skyfury()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class SpiritLinkTotem : HpalAbility
    {
        public const string name = "Spirit Link Totem";
        public SpiritLinkTotem()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class SurgingTotem : HpalAbility
    {
        public const string name = "Surging Totem";
        public SurgingTotem()
        {
            Name = name;
            GCD = true;
        }
    }
    internal class Tidewaters : HpalAbility
    {
        public const string name = "Tidewaters";
        public Tidewaters()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Vers]);
        }
    }

    internal class TremorTotem : HpalAbility
    {
        public const string name = "Tremor Totem";
        public TremorTotem()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class UnleashLife : HpalAbility
    {
        public const string name = "Unleash Life";
        public UnleashLife()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Vers]);
        }
    }
    internal class WaterShield : HpalAbility
    {
        public const string name = "Water Shield";
        public WaterShield()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class WaterWalking : HpalAbility
    {
        public const string name = "Water Walking";
        public WaterWalking()
        {
            Name = name;
            GCD = true;
        }
    }
    internal class WindBarrier : HpalAbility
    {
        public const string name = "Wind Barrier";
        public WindBarrier()
        {
            Name = name;
            CastTime = 2;
            Scalers.UnionWith([SN.Intellect, SN.Vers]);
        }
    }
    internal class WindRushTotem : HpalAbility
    {
        public const string name = "Wind Rush Totem";
        public WindRushTotem()
        {
            Name = name;
            GCD = true;
        }
    }


}


