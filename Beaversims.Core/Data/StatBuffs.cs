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
    /*--- *
    * WW3 *
    * --- */
    internal class AstralAntenna : StatBuff
    {
        public const int id = 1239641;
        public const string name = "Astral Antenna";

        public AstralAntenna(UnitId sourceId, int stacks)
            : base(id, sourceId, name, stacks)
        {
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
}
