using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Beaversims.Core.Common;

namespace Beaversims.Core.Specs.Druid.Resto
{

    internal class RestorationDruid : Spec
    {
        public const double masteryPr_s = Mastery.tooltipPercentRate / 0.728;
        public override double MasteryPr { get; } = masteryPr_s;
        protected override string SpecAbilityNamespace => "Beaversims.Core.Specs.Druid.Resto.Abilities";
        protected override string SpecTalentNamespace => "Beaversims.Core.Specs.Druid.Resto.Talents";
        public override SpecName SpecName => SpecName.RestorationDruid;



       
        public override void SpecIteration(List<Event> events, UnitRepo allUnits, Fight fight, int iterationCount)
        {
            Main.SpecMain(events, allUnits, fight, iterationCount);
        }

    }
    

    internal class RestoKeeperOfTheGrove : RestorationDruid
    {
        public const int idTalent = 117195;  // [Dream Surge]
        public override HeroTlName HeroTlName => HeroTlName.KeeperOfTheGrove;

    }
    internal class RestoWildstalker : RestorationDruid
    {
        public const int idTalent = 117226;  // [Thriving Growth]
        public override HeroTlName HeroTlName => HeroTlName.Wildstalker;

    }
}


