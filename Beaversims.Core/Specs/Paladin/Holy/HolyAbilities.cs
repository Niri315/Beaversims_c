using Beaversims.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/* TODO List
 // REMOVED FOR MIDNIGHT SKIP - Awakening - Extract build in gains from haste and get remaining value.
Infusion of Light - Remove haste value from iol increases due to low true HCCGM from holy shock.
Truth Prevails - dupli effect
Tempered in Battle - dupli effect only
Blessing of An'she


HASTE:
For herald its most likely a +-=0 scenario with the most significant inaccurasies lying in
undervalue for awakening, and overvalue for iol.

For Lightsmith, its probably an undervalue due to 
*/

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
        public double judgSourceDmg = 0.0;
        public double csSourceDmg = 0.0;
        public double onUseDur = 15;
        public double awakeningDur = 8;
        public const int buffId = 216331;
        public AvengingCrusader()
        {
            Name = name;
            ManaCost_p = 0.03;
            Direct = true;
            ReverseEffect = true;
            Scalers.UnionWith([SN.Intellect, SN.Haste, SN.Mastery, SN.Vers]);  // Crit seperate for awakening.
            HasteScalers.UnionWith([HST.Cast]);
            // CIM/HCCGM Sources added later.
        }
    }
    internal class AvengingWrath : HpalAbility
    {
        public const string name = "Avenging Wrath";
        public double onUseDur = 20;
        public double awakeningDur = 12;
        public const int buffId = 31884;
        public AvengingWrath()
        {
            Name = name;
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
        public const int initAbsorbId = 395180;

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

        public override double HypoTrueUhr()
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
            return HypoTrueUhr() * AltHeal[i].Hypo / (Heal.Eff - PolHeal.Eff);
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
            SimDupliAbility = true;
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
            Duration = 12;
            Cd = 9;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast, HST.Auto]);
            ForceTick = true; // Behaves like tick with procs etc.
            //HCGM Sources in HCGM.
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
            Cd = 7.8;
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
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            // HCCGM sources in HCGM
        }
    }

    internal class DivineToll : HpalAbility
    {
        public const string name = "Divine Toll";
        public int holyShockCount = 5;
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
            CastTime = 1.5;  // still 1.5 with infusion as GCD, no need to change.
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
            HasteScalers.UnionWith([HST.Cast, HST.Auto]);
            CIMSources.Add(new CIMSource(Consecration.name, 1.0));
        }
    }

    internal class GreaterJudgment : HpalAbility
    {
        public const string name = "Greater Judgment";

        public void CritGains(TpEvent evt, User user, int i)
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

                StatGains.CritAltAmount(evt, crit, i, false, 2, userAbilityUhr:false, estNonCritValue:estNonCritAmount);
            }
        }
        public GreaterJudgment()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            CIMSources.Add(new CIMSource(Judgment.name, 1.0));

        }
    }



    internal class HammerAndAnvil : HpalAbility
    {
        public const string name = "Hammer and Anvil";
        public HammerAndAnvil()
        {
            Name = name;
            ManaCost_p = 0.028;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            CIMSources.Add(new CIMSource(Judgment.name, 1.0));
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
    {        // Todo track HoW for HCGM, important for veneration.
        public const string name = "Hammer of Wrath";
        public HammerOfWrath()
        {
            Name = name;
            ManaCost_p = 0.006;
            CastTime = Constants.GCD;
            Cd = 19;
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
        public double HolyPowerScaleCount { get; set; } = 0;
        public double HolyPowerNonScaleCount { get; set; } = 0;
        public HolyShock()
        {
            Name = name;
            ManaCost_p = 0.028;
            CastTime = Constants.GCD;
            Cd = 9.5;
            Direct = true;
            Spell = true;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast, HST.Auto]);
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
                GJCritEffRepo += evt.UserStats.Get(StatName.Crit).TrueEff();
                GJCount += 1;
            }
        }
        public Judgment()
        {
            Name = name;
            ManaCost_p = 0.0168;
            CastTime = Constants.GCD;
            Cd = 11;
            Scalers.UnionWith([SN.Intellect, SN.Haste, SN.Vers]); // Crit seperate for awakening.
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
            CIMSources.Add(new CIMSource(Judgment.name, 1.0));

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
        public int EmpCasts { get; set; } = 0;
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

    internal class LightOfTheMartyr : HpalAbility
    {
        //OBS ! Absorb healing wont show for Leech (maybe more abilities) on wowlogs overall, but the events are present. 
        // TODO - This is messing up the dupli calcs. Need a nukeLeech type system for all of them.
        public const string name = "Light of the Martyr";
        public void MartyrAntiGains(HealEvent evt, User user, int i)
        {
            if (evt.HealAbsorbAbilityName == name)
            {
                var holyshock = user.Abilities.Get(Abilities.HolyShock.name);
                // Same as Holy Shock Scalers
                Shared.StatGains.PrimaryGains(evt, user, StatName.Intellect, i, antiGain:true);
                Shared.StatGains.VersGains(evt, user, i, antiGain: true);
                //Shared.StatGains.HasteGainsHeal(evt, user, i, ability: holyshock, antiGain: true);
                // Getting Past the checks to send directly as cast scaler.
                var haste = (Haste)evt.UserStats.Get(StatName.Haste);
                Shared.StatGains.SecondaryAltAmount(evt, haste, i, mod: holyshock.HasteAutoModHeal * holyshock.HasteGainMod * user.Spec.HasteGainMod, antiGain: true);
                Shared.StatGains.SecondaryAltAmount(evt, haste, i, mod: holyshock.TrueQIM(user, i) * holyshock.TrueHealHCGM(user) * holyshock.HasteGainMod * user.Spec.HasteGainMod, antiGain: true);
                MasteryTracker.MasteryGains(evt, user, i, antiGain: true);
                Shared.StatGains.CritGainsHealDerived(evt, user, i, sourceAbility:holyshock, antiGain: true);
            }
        }
        public LightOfTheMartyr()
        {   
            // Scaler checks not needed, sending gains in Martyr.
            Name = name;
            SourceAbility = HolyShock.name;
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
            HasteScalers.UnionWith([HST.Auto]);
        }
    }

    internal class OverflowingLight : HpalAbility
    {
        // Just using derived crit for this.
        public const string name = "Overflowing Light";

        public OverflowingLight()
        {
            Name = name;
            ManaCost_p = 0.028;
            Direct = true;
            Spell = true;
            Scalers.UnionWith([SN.Intellect, SN.Haste, SN.Mastery, SN.Vers]);
            DerivedCritScaler = true;
            SourceAbility = HolyShock.name;
            // Adjusting Auto mod alongside HolyShock in HCGM.
            HasteScalers.UnionWith([HST.Cast, HST.Auto]);
            CIMSources.Add(new CIMSource(HolyShock.name, 1.0));
            HCGMSources.Add(new HCGMSource(HolyShock.name, 1.0));

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
            CIMSources.Add(new CIMSource(LightOfDawn.name, 1.0));
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
            Scalers.UnionWith([SN.Intellect, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            DerivedCritScaler = true;
            SourceAbility = HolyLight.name;
            CIMSources.Add(new CIMSource(HolyLight.name, 1.0));
            HCGMSources.Add(new HCGMSource(HolyLight.name, 1.0));
        }
    }

    internal class RiteOfAdjuration : HpalAbility
    // TODO Confirm haste scaling
    {
        public const string name = "Rite of Adjuration";
        public RiteOfAdjuration()
        {
            Name = name;
            CastTime = 2;
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
            CIMSources.Add(new CIMSource(WordOfGlory.name, 1.0));

        }
    }

    internal class SacredWeapon : HpalAbility
        // Scales with the paladin's stats.
        // Scales with mastery.
        // Scales with haste.
    {
        public const string name = "Sacred Weapon";
        public const int buffId = 432502;
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
        public const double csReduct = 1.5;
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
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            CIMSources.Add(new CIMSource(ShieldOfTheRighteous.name, 1.0));

        }
    }

    internal class SunsAvatar : HpalAbility
    {
        public const string name = "Sun's Avatar";
        public double AwakeningHealRaw { get; set; } = 0;
        public SunsAvatar()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]); // Cast gain from awakening only.
            // CIM sources in HCGM.
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
            // CIM sources in HCGM
        }
    }
    internal class TruthPrevails : HpalAbility
    {
        //Todo need a derived crit scaler for the dupli value.
        public const string name = "Truth Prevails";
        public const double dupliId = 461529;
        public const double normalId = 461546;
        public TruthPrevails()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Crit, SN.Haste, SN.Mastery, SN.Vers]);
            HasteScalers.UnionWith([HST.Cast]);
            CIMSources.Add(new CIMSource(Judgment.name, 1.0));

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
            CIMSources.Add(new CIMSource(HammerOfWrath.name, 1.0));
            HCGMSources.Add(new HCGMSource(HammerOfWrath.name, 1.0));

        }
    }

    internal class WordOfGlory : HpalAbility
    {
        public const string name = "Word of Glory";
        public WordOfGlory()
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
}


