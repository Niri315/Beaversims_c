using Beaversims.Core.Shared;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
  TODO LIST

High Prio:

    Grace Period
    Apex Talents
    Lifeforce mender
    Twin Flame
    Draconic Instincts
    Full Alloc crit

Mid Prio:
    
    Nozdormu's teachings
    Ouroboros
    Time of Need
    Titans gift
    Titanic Precision
    
    
Low Prio:

    Inner flame
    
 */

/* Emp cast times
    finish = 4
    reach 4 = 3,25
    reach 3 = 2,5
    reach 2 = 1,75
    min = 1,5 (GCD)
 */

namespace Beaversims.Core.Specs.Evoker.Pres.Abilities
{
    internal abstract class PresAbility : Ability
    {
        public bool EchoEffect { get; set; } = false;
        public int ApplyyRefreshCount { get; set; } = 0;
        public int EssenceBurstCount { get; set; } = 0;
        public double EbProcChance { get; set; }
        public int StasisCount { get; set; } = 0;
        public double ExtendedDur {  get; set; } = 0;
        public double TotalDur { get; set; } = 0;
        
    }
    internal class EssenceRegenDummy : PresAbility
    {
        public const string name = "EssenceRegenDummy";
        public override double CIMDerivedQIM(User user)
        {
            return 1.0;
        }
        public override double TrueQIM(User user, int i)
        {
            return 1.0 * user.HasteCapCTGLossMod(i);
        }
        public EssenceRegenDummy()
        {
            Name = name;
            //OneHardCIM = true;
        }
    }

    internal class AzureStrike : PresAbility
    {
        public const string name = "Azure Strike";

        public AzureStrike()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class BlessingOfTheBronze : PresAbility
    {
        public const string name = "Blessing of the Bronze";
        public BlessingOfTheBronze()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class CauterizingFlame : PresAbility
    {
        public const string name = "Cauterizing Flame";
        public CauterizingFlame()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Vers]);
        }
    }
    internal class ChronoFlames : PresAbility
    {
        // Cast only.
        public const string name = "Chrono Flames";
        public ChronoFlames()
        {
            Name = name;
            CastTime = 2;
            EbProcChance = 0.3;
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class ChronoFlame : PresAbility
    {
        // Heal/dmg only
        public const string name = "Chrono Flame";
        public ChronoFlame()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class ConsumeFlame : PresAbility
    {
        public const string name = "Consume Flame";

        public ConsumeFlame()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Auto]);
        }
    }

    internal class DeepBreath : PresAbility
    {
        public const string name = "Deep Breath";

        public DeepBreath()
        {
            Name = name;
            GCD = true;
            ZeroHasteCTG = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Auto]);
        }
    }
    internal class Disintegrate : PresAbility
    {
        // Haste doesn't affect amount at all, tooltip is lying.
        public const string name = "Disintegrate";
        public Disintegrate()
        {
            Name = name;
            CastTime = 3;
            Channeled = true;
            Spender = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

 

    internal class DreamBreath : PresAbility
    {
        public const string name = "Dream Breath";

        // Haste scaling seems to now affect tick only.
        public DreamBreath()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Tick]);
        }
    }
    internal class DreamBreathEcho : PresAbility
    {
        public const string name = "Dream Breath (Echo)";

        // Haste scaling seems to now affect tick only.
        public DreamBreathEcho()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Tick, HST.Cast]);
            EchoEffect = true;
        }
    }

    internal class DreamFlight : PresAbility
    {
        public const string name = "Dream Flight";

        public DreamFlight()
        {
            Name = name;
            GCD = true;
            ZeroHasteCTG = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Tick]);
        }
    }

    internal class Echo : PresAbility
    {
        public const string name = "Echo";
        public Echo()
        {
            Name = name;
            GCD = true;
            Spender = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class EmeraldBlossom : PresAbility
    {
        public const string name = "Emerald Blossom";

        public EmeraldBlossom()
        {
            Name = name;
            GCD = true;
            Spender = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }
    internal class Engulf : PresAbility
    {
        public const string name = "Engulf";
        public Engulf()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class Enkindle : PresAbility
    {
        // Doesnt scale with anything including haste after the fact.
        public const string name = "Enkindle";
        public const double coef = 0.2;
        public Enkindle()
        {
            Name = name;
            SimDupliAbility = true;
        }
    }


    internal class Expunge : PresAbility
    {
        public const string name = "Expunge";
        public Expunge()
        {
            Name = name;
            GCD = true;
        }
    }
    internal class FireBreath : PresAbility
    {
        public const string name = "Fire Breath";
        public int leapingFlamesCount = 0;
        public FireBreath()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Auto]);
        }
    }

    internal class FlutteringSeedlings : PresAbility
    {
        public const string name = "Fluttering Seedlings";

        public FlutteringSeedlings()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            CIMSources.Add(new CIMSource(EmeraldBlossom.name, 1.0));
        }
    }
    internal class GoldenHour : PresAbility
    {
        public const string name = "Golden Hour";

        public GoldenHour()
        {
            Name = name;
            Scalers.UnionWith([SN.Haste]);
            HasteScalers.UnionWith([HST.Cast]);
            CIMSources.Add(new CIMSource(Reversion.name, 1.0));
        }
    }
    internal class Landslide : PresAbility
    {
        public const string name = "Landslide";
        public Landslide()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }
    internal class Lifebind : PresAbility
    {
        public const string name = "Lifebind";
        public const int buffId = 373267;
        public const double coef = 0.6; 

        public Lifebind()
        {
            Name = name;
            // CIM cast haste scaler based on echo generation, on top of normal dupli calc.
            Scalers.UnionWith([SN.Haste]);
            HasteScalers.UnionWith([HST.Cast]);
            SimDupliAbility = true;
        }
    }
    internal class LifeGiversFlame : PresAbility
    {
        // Not scaling with mastery
        public const string name = "Life-Giver's Flame";
    
        public LifeGiversFlame()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            DerivedCritScaler = true;
            ReverseEffect = true;
            SourceAbility = FireBreath.name;
            HasteScalers.UnionWith([HST.Auto]);  // Auto mod in HCGM
        }
    }

    internal class LivingFlame : PresAbility
    {
        public const string name = "Living Flame";
        public LivingFlame()
        {
            Name = name;
            CastTime = 2;

            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            EbProcChance = 0.3;
        }
    }

    internal class Naturalize : PresAbility
    {
        public const string name = "Naturalize";

        public Naturalize()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class OppressingRoar : PresAbility
    {
        public const string name = "Oppressing Roar";
        public OppressingRoar()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class Panacea : PresAbility
    {
        public const string name = "Panacea";
        public Panacea()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            // Sources in HCGM
        }
    }

    internal class Rewind : PresAbility
    {
        public const string name = "Rewind";

        public Rewind()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class RenewingBlaze : PresAbility
    {
        public const string name = "Renewing Blaze";
        public RenewingBlaze()
        {
            Name = name;
            //Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            // anti gain vers / avoidance def.
        }
    }

    internal class Reversion : PresAbility
    {
        public const string name = "Reversion";
 

        public Reversion()
        {
            Name = name;
            GCD = true;
            Duration = 12;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Tick, HST.Cast]);
            OneHardCIM = true; // Gonna be used alongside TA almost all the time. Having matching CIM makes sense practically.
            EbProcChance = 0.25;
        }
    }
    internal class ReversionEcho : PresAbility
    {
        public const string name = "Reversion (Echo)";

        public ReversionEcho()
        {
            Name = name;
            //GCD = true;
            Duration = 12;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Tick, HST.Cast]);
            EchoEffect = true;
        }
    }

    internal class SleepWalk : PresAbility
    {
        public const string name = "Sleep Walk";
        public SleepWalk()
        {
            Name = name;
            CastTime = 1.7;
        }
    }

    internal class SourceOfMagic : PresAbility
    {
        public const string name = "Source of Magic";
        public SourceOfMagic()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class SpatialParadox : PresAbility
    {
        public const string name = "Spatial Paradox";
        public SpatialParadox()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class Spiritbloom : PresAbility
    {
        public const string name = "Spiritbloom";
        public Spiritbloom()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Vers]);
        }
    }

    internal class SpiritbloomEcho : PresAbility
    {
        public const string name = "Spiritbloom (Echo)";
        public SpiritbloomEcho()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            EchoEffect = true;
        }
    }

    internal class Stasis : PresAbility
    {
        public const string name = "Stasis";
        // Living flame & Chrono flame currently missing (only heal casts should be included).
        public static readonly HashSet<string> allowedSpells = [
            CauterizingFlame.name, 
            DreamBreath.name,
            Echo.name,
            Expunge.name,
            Naturalize.name,
            Reversion.name,
            Spiritbloom.name,
            VerdantEmbrace.name,
            EmeraldBlossom.name,
            Engulf.name
        ];
        public const int storeBuffId = 370537;
        public const int releaseCastId = 370564;
        public Stasis()
        {
            Name = name;
        }
    }
    internal class TailSwipe : PresAbility
    {
        public const string name = "Tail Swipe";

        public TailSwipe()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class TemporalAnomaly : PresAbility
    {
        public const string name = "Temporal Anomaly";

        public TemporalAnomaly()
        {
            Name = name;
            CastTime = 1.5;
            Scalers.UnionWith([SN.Intellect, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            OneHardCIM = true; // Not really realistically any situation where CIM for this should be higher than 1.
        }
    }

    internal class TimeSpiral : PresAbility
    {
        public const string name = "Time Spiral";
        public TimeSpiral()
        {
            Name = name;
            GCD = true;
        }
    }

    internal class Unravel : PresAbility
    {
        public const string name = "Unravel";
        public Unravel()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers]);
        }
    }

    internal class VerdantEmbrace : PresAbility
    {
        // Since its so tightly linked to DB we wont have CIM > 0.
        public const string name = "Verdant Embrace";
        public VerdantEmbrace()
        {
            Name = name;
            GCD = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Vers]);
        }
    }
    internal class Zephyr : PresAbility
    {
        public const string name = "Zephyr";
        public Zephyr()
        {
            Name = name;
            GCD = true;
        }
    }
}


