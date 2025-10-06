using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Data.CustomBuffs
{

    internal class FullItems : StatBuff
    {
        public const string name = "Full Items";
        public FullItems(UnitId sourceId)
            : base(-1, sourceId, name, 1)
        {
            StatMods.Add(
                new StatMod(
                    StatName.Intellect,
                    StatAmountType.Multi,
                    0.05)
            );
        }
    }

    internal class BaseCrit : StatBuff
    {
        public const string name = "Base Crit";
        public BaseCrit(UnitId sourceId)
            : base(-1, sourceId, name, 1)
        {
            StatMods.Add(
                new StatMod(
                    StatName.Crit,
                    StatAmountType.Base,
                    5 * Crit.percentRate)
            );
        }
    }


    /* ------- *
     * Paladin *
     * ------- */
    internal class BaseMasteryHpal : StatBuff
    {
        public const string name = "Hpal Base Mastery";
        public BaseMasteryHpal(UnitId sourceId)
            : base(-1, sourceId, name, 1)
        {
            SourceType = BuffSourceType.Spec;
            SourceObjId = (int)SpecName.HolyPaladin;
            StatMods.Add(
                new StatMod(
                    StatName.Mastery,
                    StatAmountType.Base,
                    12 * Specs.Paladin.Holy.HolyPaladin.masteryPr_s)
            );
        }
    }
    internal class SanctifiedPlates : StatBuff
    {
        public const string name = "Sanctified Plates";
        public SanctifiedPlates(UnitId sourceId)
            : base(-1, sourceId, name, 1)
        {
            SourceType = BuffSourceType.Talent;
            SourceObjId = 115034;
            StatMods.Add(
                new StatMod(
                    StatName.Stamina,
                    StatAmountType.Multi,
                    0.03)
            );
        }
    }
    internal class HolyAegis : StatBuff
    {
        public const string name = "Holy Aegis";
        public HolyAegis(UnitId sourceId)
            : base(-1, sourceId, name, 1)
        {
            SourceType = BuffSourceType.Talent;
            SourceObjId = 102597;
            StatMods.Add(
                new StatMod(
                    StatName.Crit,
                    StatAmountType.Base,
                    4 * Crit.percentRate)
            );
        }
    }
    internal class SealOfMight : StatBuff
    {
        public const string name = "Seal of Might";
        public SealOfMight(UnitId sourceId)
            : base(-1, sourceId, name, 1)
        {
            SourceType = BuffSourceType.Talent;
            SourceObjId = 102612;
            StatMods.Add(
                new StatMod(
                    StatName.Mastery,
                    StatAmountType.Base,
                    2 * Mastery.tooltipPercentRate)
            );
            StatMods.Add(
                new StatMod(
                    StatName.Intellect, 
                    StatAmountType.Multi,
                    0.02)
            );
        }
    }
    internal class RiteOfSanctification : StatBuff
        // Does not show up in log so need to have as a custom buff.
        // Assuming user isnt a nitwit and is actually using it.
    {
        public const string name = "Rite of Sanctification";
        public RiteOfSanctification(UnitId sourceId)
            : base(-1, sourceId, name, 1)
        {
            SourceType = BuffSourceType.Talent;
            SourceObjId = 117881;
            StatMods.Add(
                new StatMod(
                    StatName.Intellect,
                    StatAmountType.Multi,
                    0.02)
            );
        }
    }
}
