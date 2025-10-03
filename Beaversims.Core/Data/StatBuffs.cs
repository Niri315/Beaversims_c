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
                    1900,
                    StatAmountType.Rating)
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
                    0.03,
                    StatAmountType.Multi)
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
                    3 * Vers.percentRate,
                    StatAmountType.Base)
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
                    2 * Mastery.tooltipPercentRate,
                    StatAmountType.Base)
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
                    0.05,
                    StatAmountType.Multi)
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
                    Constants.BlEffectRating,
                    StatAmountType.Base)
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
                    Constants.BlEffectRating,
                    StatAmountType.Base)
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
                    Constants.BlEffectRating,
                    StatAmountType.Base)
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
                    Constants.BlEffectRating,
                    StatAmountType.Base)
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
                    Constants.BlEffectRating,
                    StatAmountType.Base)
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
                    20 * Haste.percentRate,
                    StatAmountType.Base)
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
                    4 * Leech.percentRate,
                    StatAmountType.Base)
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
                    1 * Haste.percentRate,
                    StatAmountType.Base)
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
                    2 * Haste.percentRate,
                    StatAmountType.Base)
            );
        }
    }
}
