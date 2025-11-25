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
    
    Dispersing Light
    Trail of Light
    Ultimate Serenity
    Divine Image

Mid Prio:


Low Prio:

    
 */

namespace Beaversims.Core.Specs.Priest.Holy.Abilities
{
    internal abstract class HpriestAbility : Ability
    {

    }

    internal class AngelicFeather : HpriestAbility
    {
        public const string name = "Angelic Feather";
        public AngelicFeather()
        {
            Name = name;
            GCD = true;
        }
    }
    internal class Apotheosis : HpriestAbility
    {
        public const string name = "Apotheosis";
        public Apotheosis()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class BindingHeal : HpriestAbility
    {
        public const string name = "Binding Heal";
        public BindingHeal()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            CIMSources.Add(new CIMSource(FlashHeal.name, 1.0));
            DerivedCritScaler = true;
            SourceAbility = FlashHeal.name;

        }
    }
    internal class BurningVehemence : HpriestAbility
    {
        public const string name = "Burning Vehemence";
        public BurningVehemence()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers]);
        }
    }

    internal class CosmicRipple : HpriestAbility
    {
        public const string name = "Cosmic Ripple";
        public CosmicRipple()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            // Should have have QIM as holy words.
        }
    }

    internal class DispelMagic : HpriestAbility
    {
        public const string name = "Dispel Magic";
        public DispelMagic()
        {
            Name = name;
            GCD = true;
        }
    }
    internal class DivineHymn : HpriestAbility
    {
        public const string name = "Divine Hymn";
        public DivineHymn()
        {
            Name = name;
            CastTime = 5;
            Channeled = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Vers]);
        }
    }

    internal class DominateMind : HpriestAbility
    {
        public const string name = "Dominate Mind";
        public DominateMind()
        {
            Name = name;
            CastTime = 1.8;
        }
    }
    internal class EchoOfLight : HpriestAbility
    {
        public const string name = "Echo of Light";
        public EchoOfLight()
        {
            Name = name;
            SimDupliAbility = true;
            DupliEffectType = DupliEffectType.Heal;
        }
    }

    internal class FlashHeal : HpriestAbility
    {
        public const string name = "Flash Heal";
        public FlashHeal()
        {
            Name = name;
            CastTime = 1.5;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }
    internal class Heal : HpriestAbility
    {
        public const string name = "Heal";
        public Heal()
        {
            Name = name;
            CastTime = 2.5;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class HolyFire : HpriestAbility
    {
        public const string name = "Holy Fire";
        public HolyFire()
        {
            Name = name;
            CastTime = 1.5;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Tick]);
        }
    }
    internal class HolyNova : HpriestAbility
    {
        public const string name = "Holy Nova";
        public HolyNova()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class HolyWordChastise : HpriestAbility
    {
        public const string name = "Holy Word: Chastise";
        public HolyWordChastise()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Vers]);
        }
    }

    internal class HolyWordSanctify : HpriestAbility
    {
        public const string name = "Holy Word: Sanctify";
        public HolyWordSanctify()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            // Rest rel or CIM sources?

        }
    }

    internal class HolyWordSerenity : HpriestAbility
    {
        public const string name = "Holy Word: Serenity";
        public HolyWordSerenity()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            // Rest rel or CIM sources?

        }
    }
    internal class Levitate : HpriestAbility
    {
        public const string name = "Levitate";
        public Levitate()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class MassDispel : HpriestAbility
    {
        public const string name = "Mass Dispel";
        public MassDispel()
        {
            Name = name;
            CastTime = 1.5;
        }
    }
    internal class MindControl : HpriestAbility
    {
        public const string name = "Mind Control";
        public MindControl()
        {
            Name = name;
            CastTime = 1.8;
        }
    }
    internal class MindSoothe : HpriestAbility
    {
        public const string name = "Mind Soothe";
        public MindSoothe()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class PowerWordFortitude : HpriestAbility
    {
        public const string name = "Power Word: Fortitude";
        public PowerWordFortitude()
        {
            Name = name;
            GCD = true;
        }
    }
    internal class PowerWordShield : HpriestAbility
    {
        public const string name = "Power Word: Shield";
        public PowerWordShield()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            OneHardCIM = true;
        }
    }

    internal class PrayerOfHealing : HpriestAbility
    {
        public const string name = "Prayer of Healing";
        public PrayerOfHealing()
        {
            Name = name;
            CastTime = 2.5;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }
    internal class PrayerOfMending : HpriestAbility
    {
        public const string name = "Prayer of Mending";
        public PrayerOfMending()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Haste, SN.Crit, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            OneHardCIM = true;
        }
    }
    internal class Purify : HpriestAbility
    {
        public const string name = "Purify";
        public Purify()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class Renew : HpriestAbility
    {
        public const string name = "Renew";
        public Renew()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Haste, SN.Crit, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast, HST.Tick]);
        }
    }

    internal class PsychicScream : HpriestAbility
    {
        public const string name = "Psychic Scream";
        public PsychicScream()
        {
            Name = name;
            GCD = true;
        }
    }
    internal class ShackleUndead : HpriestAbility
    {
        public const string name = "Shackle Undead";
        public ShackleUndead()
        {
            Name = name;
            CastTime = 1.5;
        }
    }
    internal class ShadowWordDeath : HpriestAbility
    {
        public const string name = "Shadow Word: Death";
        public ShadowWordDeath()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers]);
        }
    }
    internal class ShadowWordPain : HpriestAbility
    {
        public const string name = "Shadow Word: Pain";
        public ShadowWordPain()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Tick, HST.Cast]);
        }
    }
    internal class Smite : HpriestAbility
    {
        public const string name = "Smite";
        public Smite()
        {
            Name = name;
            CastTime = 1.5;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Tick, HST.Cast]);
        }
    }
}


