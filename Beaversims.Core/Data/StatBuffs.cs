using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Data.StatBuffs
{  
    /* ------ *
     * Vantus *
     * ------ */
    internal class Vantus : StatBuff
    {
        // Keeping it here since if its used during the fight it will be applied correctly.
        // Adding extra logic on init for it.
        public const string name = "Vantus (custom)";
        public const int id = Constants.curVantusId;
        public Vantus(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            StatMods.Add(
                new StatMod(
                    StatName.Vers,
                    StatAmountType.Rating,
                    1900)
            );
        }
    }
    /* ---------- *
     * Raid Buffs *
     * ---------- */
    internal class ArcaneIntellect : StatBuff
    {
        public const int id = 1459;
        public const string name = "Arcane Intellect";

        public ArcaneIntellect(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            AllowMultiple = false;
            StatMods.Add(
                new StatMod(
                    StatName.Intellect,
                    StatAmountType.Multi,
                    0.03)
            );
        }
    }
    internal class MarkOfTheWild : StatBuff
    {
        public const int id = 1126;
        public const string name = "Mark of the Wild";

        public MarkOfTheWild(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            AllowMultiple = false;
            StatMods.Add(
                new StatMod(
                    StatName.Vers,
                    StatAmountType.Base,
                    3 * Vers.percentRate)
            );
        }
    }
    internal class Skyfury : StatBuff
    {
        public const int id = 462854;
        public const string name = "Skyfury";

        public Skyfury(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            AllowMultiple = false;
            StatMods.Add(
                new StatMod(
                    StatName.Mastery,
                    StatAmountType.Base,
                    2 * Mastery.tooltipPercentRate)
            );
        }
    }
    internal class PowerWordFortitude : StatBuff
    {
        public const int id = 21562;
        public const string name = "Power Word: Fortitude";

        public PowerWordFortitude(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            AllowMultiple = false;
            StatMods.Add(
                new StatMod(
                    StatName.Stamina,
                    StatAmountType.Multi,
                    0.05)
            );
        }
    }
    /* ---------- *
     * BL Effects *
     * ---------- */
    internal class Bloodlust : StatBuff
    {
        public const int id = 2825;
        public const string name = "Bloodlust";

        public Bloodlust(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            StatMods.Add(
                new StatMod(
                    StatName.Haste,
                    StatAmountType.Base,
                    Constants.BlEffectRating)
            );
        }
    }
    internal class FuryOfTheAspects : StatBuff
    {
        public const int id = 390386;
        public const string name = "Fury of the Aspects";

        public FuryOfTheAspects(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            StatMods.Add(
                new StatMod(
                    StatName.Haste,
                    StatAmountType.Base,
                    Constants.BlEffectRating)
            );
        }
    }
    internal class Heroism : StatBuff
    {
        public const int id = 32182;
        public const string name = "Heroism";

        public Heroism(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            StatMods.Add(
                new StatMod(
                    StatName.Haste,
                    StatAmountType.Base,
                    Constants.BlEffectRating)
            );
        }
    }
    internal class PrimalRage : StatBuff
    {
        public const int id = 264667;
        public const string name = "Primal Rage";

        public PrimalRage(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            StatMods.Add(
                new StatMod(
                    StatName.Haste,
                    StatAmountType.Base,
                    Constants.BlEffectRating)
            );
        }
    }
    internal class TimeWarp : StatBuff
    {
        public const int id = 80353;
        public const string name = "Time Warp";

        public TimeWarp(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            StatMods.Add(
                new StatMod(
                    StatName.Haste,
                    StatAmountType.Base,
                    Constants.BlEffectRating)
            );
        }
    }

    /* --------- *
     * Externals *
     * --------- */
    internal class PowerInfusion : StatBuff
    {
        public const int id = 10060;
        public const string name = "Power Infusion";

        public PowerInfusion(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            AllowMultiple = false;
            StatMods.Add(
                new StatMod(
                    StatName.Haste,
                    StatAmountType.Base,
                    20 * Haste.percentRate)
            );
        }
    }
    internal class VampiricAura : StatBuff
    {
        public const int id = 434107;
        public const string name = "Vampiric Aura";

        public VampiricAura(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            StatMods.Add(
                new StatMod(
                    StatName.Leech,
                    StatAmountType.Base,
                    4 * Leech.percentRate)
            );
        }
    }

    /* ------- *
     * Paladin *
     * ------- */


    internal class AvengingWrath : StatBuff
    {
        public const int id = 31884;
        public const string name = "Avenging Wrath";

        public AvengingWrath(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            StatMods.Add(
                new StatMod(
                    StatName.Crit,
                    StatAmountType.Base,
                    15 * Crit.percentRate)
            );
        }
    }

    internal class RelentlessInquisitor : StatBuff
    {
        public const int id = 383389;
        public const string name = "Relentless Inquisitor";

        public RelentlessInquisitor(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            SourceType = BuffSourceType.Talent;
            SourceObjId = 102575;
            StatMods.Add(
                new StatMod(
                    StatName.Haste,
                    StatAmountType.Base,
                    1 * Haste.percentRate)
            );
        }
    }
    internal class SolarGrace : StatBuff
    {
        public const int id = 439841;
        public const string name = "Solar Grace";

        public SolarGrace(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            SourceType = BuffSourceType.Talent;
            SourceObjId = 117691;
            StatMods.Add(
                new StatMod(
                    StatName.Haste,
                    StatAmountType.Base,
                    2 * Haste.percentRate)
            );
        }
    }
    internal class BlessingOfAutumn : StatBuff
    {
        public const int id = 388010;
        public const string name = "Blessing of Autumn";
        public double CdrAmount { get; set; } = 0.3;

        public BlessingOfAutumn(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            Duration = 30;
        }
    }

    /* ----- *
     * Druid *
     * ----- */


    internal class LycarasTeachingsNoForm : StatBuff
    {
        public const int id = 378989;
        public const string name = "LycarasTeachingsNoForm";

        public LycarasTeachingsNoForm(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            AllowMultiple = false;
            SourceType = BuffSourceType.Talent;
            SourceObjId = 103311;
            StatMods.Add(
                new StatMod(
                    StatName.Haste,
                    StatAmountType.Base,
                    3 * Haste.percentRate)
            );
        }
    }

    internal class LycarasTeachingsCat : StatBuff
    {
        public const int id = 378990;
        public const string name = "LycarasTeachingsCat";

        public LycarasTeachingsCat(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            AllowMultiple = false;
            SourceType = BuffSourceType.Talent;
            SourceObjId = 103311;
            StatMods.Add(
                new StatMod(
                    StatName.Crit,
                    StatAmountType.Base,
                    3 * Crit.percentRate)
            );
        }
    }

    internal class LycarasTeachingsBear : StatBuff
    {
        public const int id = 378991;
        public const string name = "LycarasTeachingsBear";

        public LycarasTeachingsBear(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            AllowMultiple = false;
            SourceType = BuffSourceType.Talent;
            SourceObjId = 103311;
            StatMods.Add(
                new StatMod(
                    StatName.Vers,
                    StatAmountType.Base,
                    3 * Vers.percentRate)
            );
        }
    }
    internal class LycarasTeachingsOwl : StatBuff
    {
        public const int id = 378992;
        public const string name = "LycarasTeachingsOwl";

        public LycarasTeachingsOwl(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            AllowMultiple = false;
            SourceType = BuffSourceType.Talent;
            SourceObjId = 103311;
            StatMods.Add(
                new StatMod(
                    StatName.Mastery,
                    StatAmountType.Base,
                    3 * Mastery.tooltipPercentRate)
            );
        }
    }


    /* ----------- *
     * Timewalking *
     * ----------- */

    internal class FelInfusion : StatBuff 
        // Skull of Gul'Dan
    {
        public const int id = 244176;
        public const string name = "Fel Infusion";


        public FelInfusion(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            SimImpurity = true;
            Duration = 20;
            SourceType = BuffSourceType.Item;
            SourceObjId = 150522;
            StatMods.Add(
                new StatMod(
                    StatName.Haste,
                    StatAmountType.Rating,
                    scData: new ScalingData(-7, 1.815054))
            );
        }
    }
    internal class AlacrityOfTheElements : StatBuff
    // Elemental Focus Stone
    {
        public const int id = 65004;
        public const string name = "Alacrity of the Elements";


        public AlacrityOfTheElements(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            SimImpurity = true;
            Duration = 10;
            SourceType = BuffSourceType.Item;
            SourceObjId = 156288;
            StatMods.Add(
                new StatMod(
                    StatName.Haste,
                    StatAmountType.Rating,
                    scData: new ScalingData(-7, 1.8995))
            );
        }
    }

    internal class EnergySiphon : StatBuff
    // Elemental Focus Stone
    {
        public const int id = 65008;
        public const string name = "Energy Siphon";


        public EnergySiphon(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            SimImpurity = true;
            Duration = 20;
            SourceType = BuffSourceType.Item;
            SourceObjId = 156021;
            StatMods.Add(
                new StatMod(
                    StatName.Crit,
                    StatAmountType.Rating,
                    scData: new ScalingData(-7, 2.399108))
            );
        }
    }
    internal class EyeOfTheBroodmother : StatBuff
    {
        public const int id = 65006;
        public const string name = "Eye of the Broodmother";


        public EyeOfTheBroodmother(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            SimImpurity = true;
            Duration = 10;
            MaxStacks = 5;
            SourceType = BuffSourceType.Item;
            SourceObjId = 156036;
            StatMods.Add(
                new StatMod(
                    StatName.Intellect,
                    StatAmountType.Rating,
                    scData: new ScalingData(-1, 0.09978))
            );
        }
    }

    internal class FlameOfTheHeavens : StatBuff
    // Flare of the Heavens
    {
        public const int id = 64713;
        public const string name = "Flame of the Heavens";


        public FlameOfTheHeavens(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            SimImpurity = true;
            Duration = 10;
            SourceType = BuffSourceType.Item;
            SourceObjId = 156230;
            StatMods.Add(
                new StatMod(
                    StatName.Intellect,
                    StatAmountType.Rating,
                    scData: new ScalingData(-1, 2.353487))
            );
        }
    }
    internal class LivingFlame : StatBuff
    {
        public const int id = 64712;
        public const string name = "Living Flame";


        public LivingFlame(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            SimImpurity = true;
            Duration = 20;
            SourceType = BuffSourceType.Item;
            SourceObjId = 155947;
            StatMods.Add(
                new StatMod(
                    StatName.Intellect,
                    StatAmountType.Rating,
                    scData: new ScalingData(-1, 1.9003))
            );
        }
    }

    internal class PandorasPlea : StatBuff
    {
        public const int id = 64741;
        public const string name = "Pandora's Plea";


        public PandorasPlea(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            SimImpurity = true;
            Duration = 10;
            SourceType = BuffSourceType.Item;
            SourceObjId = 156207;
            StatMods.Add(
                new StatMod(
                    StatName.Intellect,
                    StatAmountType.Rating,
                    scData: new ScalingData(-1, 1.561615))
            );
        }
    }
    internal class ScaleOfFates : StatBuff
    {
        public const int id = 64707;
        public const string name = "Scale of Fates";


        public ScaleOfFates(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            SimImpurity = true;
            Duration = 20;
            SourceType = BuffSourceType.Item;
            SourceObjId = 156187;
            StatMods.Add(
                new StatMod(
                    StatName.Haste,
                    StatAmountType.Rating,
                    scData: new ScalingData(-7, 2.39909))
            );
        }
    }

    internal class MemoriesOfLove : StatBuff
    // Sif's Remembrance
    {
        public const int id = 65003;
        public const string name = "Memories of Love";


        public MemoriesOfLove(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            SimImpurity = true;
            Duration = 15;
            SourceType = BuffSourceType.Item;
            SourceObjId = 156308;
            StatMods.Add(
                new StatMod(
                    StatName.Intellect,
                    StatAmountType.Rating,
                    scData: new ScalingData(-1, 1.125146))
            );
        }
    }

    internal class SoulFragment : StatBuff
    // Necromantic Focus
    {
        public const int id = 96962;
        public const string name = "Soul Fragment";


        public SoulFragment(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            SimImpurity = true;
            Duration = 10;
            MaxStacks = 10;
            SourceType = BuffSourceType.Item;
            SourceObjId = 171644;
            StatMods.Add(
                new StatMod(
                    StatName.Mastery,
                    StatAmountType.Rating,
                    scData: new ScalingData(-7, 0.0471))
            );
        }
    }

    /* --- *
     * WW3 *
     * --- */

    internal class AstralAntenna : StatBuff
    {
        public const int id = 1239641;
        public const string name = "Astral Antenna";
        

        public AstralAntenna(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            SimImpurity = true;
            Duration = 10;
            SourceType = BuffSourceType.Item;
            SourceObjId = 242395; 
            StatMods.Add(
                new StatMod(
                    StatName.Crit,
                    StatAmountType.Rating,
                    scData: new ScalingData(-7, 1.466488))
            );
        }
    }
    internal class FlaskOfTemperedAggression : StatBuff
    {
        public const int id = 431971;
        public const string name = "Flask of Tempered Aggression";


        public FlaskOfTemperedAggression(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            StatMods.Add(
                new StatMod(
                    StatName.Crit,
                    StatAmountType.Rating,
                    3323)
            );
        }
    }
    internal class FlaskOfTemperedSwiftness : StatBuff
    {
        public const int id = 431972;
        public const string name = "Flask of Tempered Swiftness";


        public FlaskOfTemperedSwiftness(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            StatMods.Add(
                new StatMod(
                    StatName.Haste,
                    StatAmountType.Rating,
                    3323)
            );
        }
    }
    internal class FlaskOfTemperedMastery : StatBuff
    {
        public const int id = 431974;
        public const string name = "Flask of Tempered Mastery";


        public FlaskOfTemperedMastery(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            StatMods.Add(
                new StatMod(
                    StatName.Mastery,
                    StatAmountType.Rating,
                    3323)
            );
        }
    }
    internal class FlaskOfTemperedVersatility : StatBuff
    {
        public const int id = 431973;
        public const string name = "Flask of Tempered Aggression";


        public FlaskOfTemperedVersatility(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
            StatMods.Add(
                new StatMod(
                    StatName.Vers,
                    StatAmountType.Rating,
                    3323)
            );
        }
    }
}
