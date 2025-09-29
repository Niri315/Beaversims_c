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
                    0.05,
                    StatAmountType.Multi)
            );
        }
    }
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
                    12 * Specs.Paladin.Holy.HolyPaladin.masteryPr_s,
                    StatAmountType.Base)
            );
        }
    }
}
