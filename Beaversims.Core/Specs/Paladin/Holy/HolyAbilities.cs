using Beaversims.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Beaversims.Core.Specs.Paladin.Holy.Abilities
{
    internal abstract class HpalAbility : Ability
    {
    }

    internal class AJustReward : HpalAbility
    {
        public const string name = "A Just Reward";
        public AJustReward()
        {
            Name = name;
            Direct = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Vers]);
        }
    }

    internal class AvengingCrusader : HpalAbility
    {
        public const string name = "Avenging Crusader";
        public AvengingCrusader()
        {
            Name = name;
            ManaCost_p = 0.03;
            CastTime = Constants.GCD;
            Direct = true;
            ReverseEffect = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }
    internal class AuraMastery : HpalAbility
    {
        public const string name = "Aura Mastery";
        public AuraMastery()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class BarrierOfFaith : HpalAbility
        //TODO
    {
        public const string name = "Barrier of Faith";
        public BarrierOfFaith()
        {
            Name = name;
            ManaCost_p = 0.024;
            CastTime = Constants.GCD;
        }
    }

    internal class BeaconOfFaith : HpalAbility
    //PoL and cast ONLY
    {
        public const string name = "Beacon of Faith";
        public const int buffId = 156910;
        public BeaconOfFaith()
        {
            Name = name;
            ManaCost_p = 0.005;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Vers]);  
        }
    }

    internal class BeaconOfLight : HpalAbility
        //todo PoL effect
    {
        public const string name = "Beacon of Light";
        public const int polId = 53653; // Heal from pillar of light passive light/faith effect.
        public const int dupliId = 53652; // Heal 
        public const int buffId = 53563;
        public double Coef { get; set; } = 0.2;

        public HealData PolHeal { get; } = new();

        public override double HypoTrueUr()
        {
            if (Heal.Hypo == 0) { return 0;}
            return (Heal.Eff - PolHeal.Eff) / Heal.Hypo;
        }
        public override double HypoTrueRawR()
        {
            if (Heal.Hypo == 0) { return 0; }
            return (Heal.Raw - PolHeal.Raw) / Heal.Hypo;
        }

        public override double AltHypoTrueUr(int i)
        {
            if (Heal.Eff == 0) { return 0; }
            return HypoTrueUr() * AltHeal[i].Hypo / (Heal.Eff - PolHeal.Eff);
        }
        public override double AltHypoTrueRawR(int i)
        {
            if (Heal.Raw == 0) { return 0; }
            return HypoTrueRawR() * AltHeal[i].Hypo / (Heal.Raw - PolHeal.Raw);
        }
        public BeaconOfLight()
        {
            Name = name;
            ManaCost_p = 0.005;
            CastTime = Constants.GCD;
        }
    }

    internal class BeaconOfVirtue : HpalAbility
    {
        public const string name = "Beacon of Virtue";
        public const int buffId = 200025;

        public BeaconOfVirtue()
        {
            Name = name;
            ManaCost_p = 0.05;
            CastTime = Constants.GCD;
        }
    }

    internal class BestowLight : HpalAbility
    {
        public const string name = "Bestow Light";

        public BestowLight()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Mastery, SN.Vers]);
        }
    }

    internal class BlessingOfFreedom : HpalAbility
    {
        public const string name = "Blessing of Freedom";
        public BlessingOfFreedom()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class BlessingOfProtection : HpalAbility
    {
        public const string name = "Blessing of Protection";
        public BlessingOfProtection()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class BlindingLight : HpalAbility
    {
        public const string name = "Blinding Light";
        public BlindingLight()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class Cleanse : HpalAbility
    {
        public const string name = "Cleanse";

        public Cleanse()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class Consecration : HpalAbility
    {
        public const string name = "Consecration";
        public Consecration()
        {
            Name = name;
            CastTime = Constants.GCD;
            Spell = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast, HST.Auto]);
        }
    }

    internal class ConsecrationAura : HpalAbility
    {
        public const string name = "Consecration Aura";

        public ConsecrationAura()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class CrusaderStrike : HpalAbility
    {
        public const string name = "Crusader Strike";
        public double HaaFactor { get; set; } = 0.75;
        public CrusaderStrike()
        {
            Name = name;
            ManaCost_p = 0.006;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class Dawnlight : HpalAbility
    {
        public const string name = "Dawnlight";
        public Dawnlight()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Auto]);
        }
    }

    internal class DivineShield : HpalAbility
    {
        public const string name = "Divine Shield";
        public DivineShield()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class DivineGuidance : HpalAbility
        // Not direct.
    {
        public const string name = "DivineGuidance";
        public DivineGuidance()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class DivineToll : HpalAbility
    {
        public const string name = "Divine Toll";
        public DivineToll()
        {
            Name = name;
            ManaCost_p = 0.03;
            CastTime = Constants.GCD;
        }
    }

    internal class EternalFlame : HpalAbility
    {
        public const string name = "Eternal Flame";
        public EternalFlame()
        {
            Name = name;
            ManaCost_p = 0.006;
            CastTime = Constants.GCD;
            Direct = true;
            Spell = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast, HST.Tick]);
        }
    }

    internal class EyeForAnEye : HpalAbility
    {
        public const string name = "Eye for an Eye";
        public EyeForAnEye()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Vers]);
        }
    }

    internal class FlashOfLight : HpalAbility
    {
        public const string name = "Flash of Light";
        public FlashOfLight()
        {
            Name = name;
            ManaCost_p = 0.006;
            CastTime = 1.5;
            Direct = true;
            Spell = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class ForgesReckoning : HpalAbility
        // Damage from extra sacred weapon from cd use. 
        // TODO confirm haste scaling.
    {
        public const string name = "Forge's Reckoning";
        public ForgesReckoning()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Auto]);
        }
    }


    internal class GoldenPath : HpalAbility
        // Ticking effect DOES scale with haste.
    {
        public const string name = "Golden Path";
        public GoldenPath()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast, HST.Tick]);
        }
    }

    internal class GreaterJudgment : HpalAbility
    {
        public const string name = "Greater Judgment";

        public void CritGains(ThroughputEvent evt, User user)
        {
            // todo awakening no crit scaler

            if (evt.AbilityName == Name) 
            {
                var judg = (Judgment)user.Abilities.Get(Judgment.name);
                var statName = StatName.Crit;
                var crit = (Crit)evt.UserStats.Get(statName);
                var avgCritChance = judg.GJCritEffRepo / judg.GJCount / crit.PercentRate / 100;
                var hitCount = Heal.Count * (1 -  avgCritChance);
                var critCount = Heal.Count * avgCritChance;
                var amountPerHit = Heal.Eff / ((2 * critCount) + hitCount);
                var amountPerCrit = amountPerHit * 2;
                var hitAmount = amountPerHit * hitCount;
                var critAmount = amountPerCrit * critCount;

                var estNonCritAmount = evt.Amount.Eff * ((hitAmount + (critAmount / 2)) / Heal.Eff);

                var gainEff = Calc.CritGainCalc(crit, estNonCritAmount, false, 2);
                StatGains.CritAltAmount(evt, crit, false, 2, userAbilityUhr:false);
                evt.Gains[statName][GainType.Eff] += gainEff;
            }
        }
        public GreaterJudgment()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }



    internal class HammerAndAnvil : HpalAbility
    {
        public const string name = "Hammer and Anvil";
        public HammerAndAnvil()
        {
            Name = name;
            ManaCost_p = 0.028;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }
    internal class HammerOfJustice : HpalAbility
    {
        public const string name = "Hammer of Justice";
        public HammerOfJustice()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }

    internal class HammerOfWrath : HpalAbility
    {
        public const string name = "Hammer of Wrath";
        public HammerOfWrath()
        {
            Name = name;
            ManaCost_p = 0.006;
            CastTime = Constants.GCD;
            Direct = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class HolyLight : HpalAbility
    {
        public const string name = "Holy Light";
        public HolyLight()
        {
            Name = name;
            ManaCost_p = 0.07;
            CastTime = 2;
            Direct = true;
            Spell = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class HolyPrism : HpalAbility
    {
        public const string name = "Holy Prism";
        public HolyPrism()
        {
            Name = name;
            ManaCost_p = 0.026;
            CastTime = Constants.GCD;
            Direct = true;
            Spell = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Vers]);
        }
    }

    internal class HolyRitual : HpalAbility
    {
        public const string name = "Holy Ritual";
        public HolyRitual()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Vers]);
        }
    }

    internal class HolyShock : HpalAbility
    {
        public const string name = "Holy Shock";
        public void SetHCGM()
        {
            // TODO sth casts holy shocks twice for herald. Second sunrise. only 15%, doesnt give extra Holy power.
            HGCM = Casts / (Heal.Count + Damage.Count);
        }
        public HolyShock()
        {
            Name = name;
            ManaCost_p = 0.028;
            CastTime = Constants.GCD;
            Direct = true;
            Spell = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class Intercession : HpalAbility
    {
        public const string name = "Intercession";
        public Intercession()
        {
            Name = name;
            CastTime = 2;
        }
    }
    internal class Judgment : HpalAbility
    {
        public const string name = "Judgment";
        public const int awakening15Id = 414193;
        public double GJCritEffRepo = 0.0;
        public int GJCount = 0;  // Dont really need this but adds safety.

        public void TrackGJCritChance(Event evt, User user)
        {
            if (evt.IsDmgDoneEvent() && evt.AbilityName == Name)
            {
                GJCritEffRepo += evt.UserStats.Get(StatName.Crit).Eff;
                GJCount += 1;
            }
        }



        public Judgment()
        {
            Name = name;
            ManaCost_p = 0.0168;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class JudgmentOfLight : HpalAbility
    {
        public const string name = "Judgment of Light";
        public JudgmentOfLight()
        {
            Name = name;

            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class LayOnHands : HpalAbility
    {
        public const string name = "Lay on Hands";
        public LayOnHands()
        {
            Name = name;
            Spell = true;
            Scalers.UnionWith([SN.Stamina]);
        }
    }

    internal class LesserWeapon : HpalAbility
    {
        public const string name = "Lesser Weapon";
        public LesserWeapon()
        {
            Name = name;
            ManaCost_p = 0.0168;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Auto]);
        }
    }

    internal class LightOfDawn : HpalAbility
    {
        public const string name = "Light of Dawn";
        public LightOfDawn()
        {
            Name = name;
            ManaCost_p = 0.006;
            CastTime = Constants.GCD;
            Direct = true;
            Spell = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class MercifulAuras : HpalAbility
        // Does not scale with haste, does scale with mastery.
    {
        public const string name = "Merciful Auras";
        public MercifulAuras()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class PillarOfLights : HpalAbility
    // Ability name is different from talent name. They will probably fix it someday, keep an eye out.
    {
        public const string name = "Pillar of Lights";
        public PillarOfLights()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Vers]);
        }
    }

    internal class RadiantAura : HpalAbility
        // Light of Dawn from Sacred Weapon.
    {
        public const string name = "Radiant Aura";
        public RadiantAura()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }
    internal class Repentance : HpalAbility
    {
        public const string name = "Repentance";
        public Repentance()
        {
            Name = name;
            CastTime = Constants.GCD;
        }
    }
    internal class ResplendentLight : HpalAbility
    {
        public const string name = "Resplendent Light";
        public ResplendentLight()
        {
            Name = name;
        }
    }

    internal class RiteOfAdjuration : HpalAbility
    // TODO Confirm haste scaling
    {
        public const string name = "Rite of Adjuration";
        public RiteOfAdjuration()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Auto]);
        }
    }

    internal class SacredWord : HpalAbility
    // Word of Glory from Sacred Weapon.
    {
        public const string name = "Sacred Word";
        public SacredWord()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class SacredWeapon : HpalAbility
        // TODO Need mastery testing.
        // Scales with haste.
    {
        public const string name = "Sacred Weapon";
        public SacredWeapon()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Auto]);
        }
    }

    internal class SavedByTheLight : HpalAbility
    {
        public const string name = "Saved by the Light";
        public SavedByTheLight()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Vers]);
        }
    }

    internal class SealOfTheCrusader : HpalAbility
    {
        public const string name = "Seal of the Crusader";
        public SealOfTheCrusader()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Auto]);
        }
    }

    internal class SelflessHealer : HpalAbility
    {
        public const string name = "Selfless Healer";
        public SelflessHealer()
        {
            Name = name;
        }
    }

    internal class ShieldOfTheRighteous : HpalAbility
    {
        public const string name = "Shield of the Righteous";
        public ShieldOfTheRighteous()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }
    internal class ShiningRighteousness : HpalAbility
    {
        public const string name = "Shining Righteousness";
        public ShiningRighteousness()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class SunsAvatar : HpalAbility
    {
        public const string name = "Sun's Avatar";
        public SunsAvatar()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Mastery, SN.Vers]);
        }
    }

    internal class SunSear : HpalAbility
    {
        public const string name = "Sun Sear";
        public SunSear()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast, HST.Tick]);
        }
    }
    internal class TruthPrevails : HpalAbility
    {
        public const string name = "Truth Prevails";
        public TruthPrevails()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class TurnEvil : HpalAbility
    {
        public const string name = "Turn Evil";
        public TurnEvil()
        {
            Name = name;
            CastTime = 1.5;
        }
    }

    internal class TyrsDeliverance : HpalAbility
    {
        public const string name = "Tyr's Deliverance";
        public TyrsDeliverance()
        {
            Name = name;
            CastTime = 2;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Auto]);
        }
    }

    internal class Veneration : HpalAbility
    {
        public const string name = "Veneration";
        public Veneration()
        {
            Name = name;
            ReverseEffect = true;
            Direct = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }

    internal class WordOfGlory : HpalAbility
    {
        public const string name = "Word of Glory";
        public WordOfGlory()
        {
            Name = name;
            ManaCost_p = 0.006;
            Direct = true;
            Spell = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
        }
    }
}

