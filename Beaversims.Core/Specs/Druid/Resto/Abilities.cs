using Beaversims.Core.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
  TODO LIST

High Prio:
    
    Dungeon/ <20 raid CIM
    mastery + haste interaction

    Midnight:

    Apex Talents
    
    Gift of the Wild
    Intensity (crit inc)

    Spirit of the thicket
    Nature's bounty

    Sylvan beckoning check if haste scaling rppm
    

Mid Prio:
    
    Nature's splendor (remove haste cast value from regrowth)
    Yseras gift rejuv haste cast scaling
    Some haste cast value from bursting growth
    

Low Prio:

    Bearform stamina.
    Starsurge cd tracking + CIM
    Multidotting CIM
    Lycaras inspiration
    Omen of clarity / clearcasting (Only for mana currently)
    Verdancy procs based on photosynthesis? haste tick value
    Check if flourish healing is categorized as flourish healing in events.
    
    Incarnation cast changes (pretty much all 1.5 anyways)
    Call of the elder druid (remove haste value from dmg incs)
    Blooming infusion
    Rampant Growth

    
 */

namespace Beaversims.Core.Specs.Druid.Resto.Abilities
{
    internal abstract class RestoAbility : Ability
    {
        public int ApplyyRefreshCount { get; set; } = 0;
        public int SotfCount { get; set; } = 0;
        public int EotdCount { get; set; } = 0;
        public bool BalanceSpell {  get; set; } = false;
        public bool CIMDepMastIncScaler { get; set; } = false;
    }

    internal class AessinasRenewal : RestoAbility
    {
        public const string name = "Aessina's Renewal";
        public AessinasRenewal()
        {
            Name = name;
            Scalers.UnionWith([SN.Stamina]);
        }
    }

    internal class BearForm : RestoAbility
    {
        public const string name = "Bear Form";
        public BearForm()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class BloodseekerVines : RestoAbility
    {
        public const string name = "Bloodseeker Vines";
        public BloodseekerVines()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste]);
            HasteScalers.UnionWith([HST.Tick]);
        }
    }

    internal class BurstingGrowth : RestoAbility
        // Todo need some cast scaling on this from CIM rejuv.
    {
        public const string name = "Bursting Growth";
        public BurstingGrowth()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Mastery]);
        }
    }
    internal class CatForm : RestoAbility
    {
        public const string name = "Cat Form";
        public CatForm()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class ConvokeTheSpirits : RestoAbility
    {
        public const string name = "Convoke the Spirits";
        public List<string> Abilities {  get; set; }
        public ConvokeTheSpirits()
        {
            Name = name;
            CastTime = 4;
            ZeroHasteCTG = true;
            Abilities = [Rejuvenation.name, RejuvenationGermination.name, Regrowth.name, Thrash.name, Rake.name, Moonfire.name, Wrath.name];
        }
    }


    internal class Cultivation : RestoAbility
    {
        public const string name = "Cultivation";
        public Cultivation()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste, SN.Mastery]);
            HasteScalers.UnionWith([HST.Cast]);  // Tick removed in midnight.
            CIMSources.Add(new CIMSource(Rejuvenation.name, 1.0));
        }
    }

    internal class Cyclone : RestoAbility
    {
        public const string name = "Cyclone";
        public Cyclone()
        {
            Name = name;
            CastTime = 1.7;
            BalanceSpell = true;
        }
    }

    internal class DreamBloom : RestoAbility
    {
        public const string name = "Dream Bloom";
        public DreamBloom()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Mastery]);
        }
    }

    internal class DreamOfCenarius : RestoAbility
    {
        public const string name = "Dream of Cenarius";
        public static readonly Dictionary<string, double> abilityCoefs = new()
        {
            { Wrath.name, 1.0 },
            { Shred.name, 1.0 },
            { Starfire.name, 0.5 },
            { Swipe.name, 0.5 },
        };
        public const double hotwInc = 2.0;
        public DreamOfCenarius()
        {
            Name = name;
            SimDupliAbility = true;
        }
    }

    internal class Efflorescence : RestoAbility
    {
        public const string name = "Efflorescence";
        public Efflorescence()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste, SN.Mastery]);
            HasteScalers.UnionWith([HST.Auto]);
        }
    }

    internal class EmbraceOfTheDream : RestoAbility
    {
        public const string name = "Embrace of the Dream";
        public static readonly HashSet<string> abilitySources = [Rejuvenation.name, RejuvenationGermination.name, Regrowth.name];
        public EmbraceOfTheDream()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste, SN.Mastery]);
            HasteScalers.UnionWith([HST.Cast]);
            CIMSources.Add(new CIMSource(Rejuvenation.name, 1.0));
        }
    }

    internal class EntanglingRoots : RestoAbility
    {
        public const string name = "Entangling Roots";

        public EntanglingRoots()
        {
            Name = name;
            CastTime = 1.7;
            BalanceSpell = true;
        }
    }
    internal class FerociousBite : RestoAbility
    {
        public const string name = "Ferocious Bite";
        public FerociousBite()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class FlowerWalk : RestoAbility
    {
        public const string name = "Flower Walk";
        public FlowerWalk()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Mastery]);
        }
    }

    internal class FrenziedRegeneration : RestoAbility
    {
        public const string name = "Frenzied Regeneration";
        public FrenziedRegeneration()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Stamina]);
        }
    }

    internal class HeartOfTheWild : RestoAbility
    {
        public const string name = "Heart of the Wild";
        public const int buffId = 319454;
        public const double balanceSpellsCTCoef = 0.3;
        public HeartOfTheWild()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }


    internal class Hibernate : RestoAbility
    {
        public const string name = "Hibernate";
        public Hibernate()
        {
            Name = name;
            CastTime = 1.5;
            BalanceSpell = true;
        }
    }

    internal class IncapacitatingRoar : RestoAbility
    {
        public const string name = "Incapacitating Roar";
        public IncapacitatingRoar()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class IncarnationTreeOfLife : RestoAbility
    {
        public const string name = "Incarnation: Tree of Life";
        public IncarnationTreeOfLife()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class Ironfur : RestoAbility
    {
        public const string name = "Ironfur";
        public Ironfur()
        {
            Name = name;
        }
    }

    internal class LethalPreservation : RestoAbility
    {
        public const string name = "Lethal Preservation";
        public LethalPreservation()
        {
            Name = name;
            Scalers.UnionWith([SN.Stamina]);
        }
    }

    internal class Lifebloom : RestoAbility
    {
        public const string name = "Lifebloom";
        public Lifebloom()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste, SN.Mastery]);
            HasteScalers.UnionWith([HST.Tick]);
        }
    }

    internal class Maim : RestoAbility
    {
        public const string name = "Maim";
        public Maim()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers]);
        }
    }

    internal class Mangle : RestoAbility
    {
        public const string name = "Mangle";
        public Mangle()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    

    internal class MarkOfTheWild : RestoAbility
    {
        public const string name = "Mark of the Wild";
        public MarkOfTheWild()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }


    internal class MassEntanglement : RestoAbility
    {
        public const string name = "Mass Entanglement";
        public MassEntanglement()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class MattedFur : RestoAbility
    {
        public const string name = "Matted Fur";
        public MattedFur()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Vers]);
        }
    }

    internal class MightyBash : RestoAbility
    {
        public const string name = "Mighty Bash";
        public MightyBash()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class Moonfire : RestoAbility
    {
        public const string name = "Moonfire";
        public Moonfire()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste]);
            HasteScalers.UnionWith([HST.Tick]);
        }
    }

    internal class MoonkinForm : RestoAbility
    {
        public const string name = "Moonkin Form";
        public MoonkinForm()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class MountForm : RestoAbility
    {
        public const string name = "Mount Form";
        public MountForm()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class NaturesCure : RestoAbility
    {
        public const string name = "Nature's Cure";

        public NaturesCure()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class NaturesSwiftness : RestoAbility
    {
        public const string name = "Nature's Swiftness";
        public const int buffId = 132158;
        public static readonly HashSet<string> abilities = [Regrowth.name, EntanglingRoots.name, Rebirth.name];
        public NaturesSwiftness()
        {
            Name = name;
        }
    }

    internal class Nourish : RestoAbility
        // Only from trees now (?)
    {
        public const string name = "Nourish";
        public double HarmonyCoef { get; set; } = 3.0;
        public Nourish()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste, SN.Mastery]);
            HasteScalers.UnionWith([HST.Auto]);
        }
    }

    internal class Rake : RestoAbility
    {
        public const string name = "Rake";
        public Rake()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste]);
            HasteScalers.UnionWith([HST.Tick]);
        }
    }

    internal class Rebirth : RestoAbility
    {
        public const string name = "Rebirth";
        public Rebirth()
        {
            Name = name;
            CastTime = 2;
        }
    }

    internal class RegenerativeHeartwood : RestoAbility
    {
        public const string name = "Regenerative Heartwood";
        public RegenerativeHeartwood()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste, SN.Mastery]);
            HasteScalers.UnionWith([HST.Auto, HST.Cast]);  // Auto for rejuv ticks.
            CIMSources.Add(new CIMSource(Rejuvenation.name, 1.0));
        }
    }
    internal class Regrowth : RestoAbility
    {
        // (Simc) OOC doesnt seem to scale with haste, and follows rppm system (feral testing).
        // If correct we need to remove CC value from cast value.

        public const string name = "Regrowth";

        public int NonSumDirectCount { get; set; } = 0;
        public const int buffId = 8936;

        public Regrowth()
        {
            Name = name;
            BuffId = buffId;
            CastTime = 1.5;
            CIMDepMastIncScaler = true;
            Scalers.UnionWith([SN.Intellect, SN.Vers, SN.Haste, SN.Mastery]);   //Crit separate to check for cap.
            HasteScalers.UnionWith([HST.Tick, HST.Cast]);
        }
    }

    internal class Rejuvenation : RestoAbility
    {
        public const string name = "Rejuvenation";
        public const int buffId = 774;
        public Rejuvenation()
        {
            Name = name;
            BuffId = buffId;
            CastTime = Constants.GCD;
            InstaTick = true;
            CIMDepMastIncScaler = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste, SN.Mastery]);
            HasteScalers.UnionWith([HST.Tick, HST.Cast]);
        }
    }
    internal class RejuvenationGermination : RestoAbility
    {
        // Not using this as CIM source, just aggregate all relevant data to normal rejuv CIM.
        public const string name = "Rejuvenation (Germination)";
        public const int buffId = 155777;
        public RejuvenationGermination()
        {
            Name = name;
            BuffId = buffId;
            // The logs contain cast events both for Rejuv and Germination for the same cast.
            CastTime  = Constants.GCD;
            ZeroHasteCTG = true;
            InstaTick = true;
            CIMDepMastIncScaler = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste, SN.Mastery]);
            HasteScalers.UnionWith([HST.Tick, HST.Cast]);
        }
    }

    internal class Rip : RestoAbility
    {
        public const string name = "Rip";
        public Rip()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste]);
            HasteScalers.UnionWith([HST.Tick]);
        }
    }

    internal class Shred : RestoAbility
    {
        public const string name = "Shred";
        public Shred()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class Soothe : RestoAbility
    {
        public const string name = "Soothe";
        public Soothe()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    //internal class SpringBlossoms : RestoAbility
    //{
    //    public const string name = "Spring Blossoms";
    //    public SpringBlossoms()
    //    {
    //        Name = name;
    //        Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste, SN.Mastery]);
    //        HasteScalers.UnionWith([HST.Auto]);
    //    }
    //}

    internal class StampedingRoar : RestoAbility
    {
        public const string name = "Stampeding Roar";
        public StampedingRoar()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class Starfire : RestoAbility
    {
        public const string name = "Starfire";
        public Starfire()
        {
            Name = name;
            CastTime = 2.5;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste]);
            HasteScalers.UnionWith([HST.Cast]);
            BalanceSpell = true;
        }
    }

    internal class Starsurge : RestoAbility
    {
        public const string name = "Starsurge";
        public Starsurge()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers]);
        }
    }

    internal class Sunfire : RestoAbility
    {
        public const string name = "Sunfire";
        public Sunfire()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste]);
            HasteScalers.UnionWith([HST.Tick]);
        }
    }
    internal class SymbioticBlooms : RestoAbility
    {
        public const string name = "Symbiotic Blooms";
        public SymbioticBlooms()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste, SN.Mastery]);
            HasteScalers.UnionWith([HST.Tick]);
        }
    }

    internal class Swiftmend : RestoAbility
    {
        public const string name = "Swiftmend";
        public Swiftmend()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Mastery]);
        }
    }

    internal class Swipe : RestoAbility
    {
        public const string name = "Swipe";
        public Swipe()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class SymbioticRelationship : RestoAbility
    {
        // Does not work with summoned healing in or out
        // Only class abilities
        // Non SP abilities works.
        public const string name = "Symbiotic Relationship";
        public const int selfBuffId = 474754;
        public const int targetBuffId = 474750;
        public SymbioticRelationship()
        {
            Name = name;
            CastTime = 1.5;
            SimDupliAbility = true;
        }
    }

    internal class Thrash : RestoAbility
    {
        public const string name = "Thrash";
        public Thrash()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste]);
            HasteScalers.UnionWith([HST.Tick]);
        }
    }

    internal class ThrivingVegetation : RestoAbility
    {
        public const string name = "Thriving Vegetation";
        public ThrivingVegetation()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste, SN.Mastery]);
            HasteScalers.UnionWith([HST.Cast]);
            CIMSources.Add(new CIMSource(Rejuvenation.name, 1.0));
        }
    }

    internal class Tranquility : RestoAbility
    {
        // Only tick scales with haste.
        public const string name = "Tranquility";
        public Tranquility()
        {
            Name = name;
            CastTime = 5;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste, SN.Mastery]);
            HasteScalers.UnionWith([HST.Tick]);
        }
    }

    internal class TravelForm : RestoAbility
    {
        public const string name = "Travel Form";
        public TravelForm()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class TreantForm : RestoAbility
    {
        public const string name = "Treant Form";
        public TreantForm()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class Typhoon : RestoAbility
    {
        public const string name = "Typhoon";
        public Typhoon()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class UrsolsVortex : RestoAbility
    {
        public const string name = "Ursol's Vortex";
        public UrsolsVortex()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }
    internal class Verdancy : RestoAbility
    {
        public const string name = "Verdancy";
        public Verdancy()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Mastery]);
        }
    }

    internal class WildGrowth : RestoAbility
    {
        public const string name = "Wild Growth";
        public WildGrowth()
        {
            Name = name;
            CastTime = 1.5;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste, SN.Mastery]);
            HasteScalers.UnionWith([HST.Tick]);
        }
    }

    internal class Wrath : RestoAbility
    {
        public const string name = "Wrath";
        public Wrath()
        {
            Name = name;
            CastTime = 1.5;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers, SN.Haste]);
            HasteScalers.UnionWith([HST.Cast]);
            BalanceSpell = true;
        }
    }

    internal class YserasGift : RestoAbility
    {
        public const string name = "Ysera's Gift";
        public YserasGift()
        {
            Name = name;
            Scalers.UnionWith([SN.Stamina]);
        }
    }
}


