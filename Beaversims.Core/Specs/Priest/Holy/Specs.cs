using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Beaversims.Core.Common;

namespace Beaversims.Core.Specs.Priest.Holy
{

    internal class HolyPriest : Spec
    {
        public const double masteryPr_s = Mastery.tooltipPercentRate / 0.95625;
        public override double MasteryPr { get; } = masteryPr_s;
        protected override string SpecAbilityNamespace => "Beaversims.Core.Specs.Priest.Holy.Abilities";
        protected override string SpecTalentNamespace => "Beaversims.Core.Specs.Priest.Holy.Talents";
        public override SpecName SpecName => SpecName.HolyPriest;



       
        public override void SpecIteration(List<Event> events, UnitRepo allUnits, Fight fight, int iterationCount)
        {
            Main.SpecMain(events, allUnits, fight, iterationCount);
        }

    }
    

    internal class HolyArchon : HolyPriest
    {
        public const int idTalent = 117300;  //Power Surge
        public override HeroTlName HeroTlName => HeroTlName.Archon;

    }
    internal class HolyOracle : HolyPriest
    {
        public const int idTalent = 117301;  // Preventive Measures
        public override HeroTlName HeroTlName => HeroTlName.Oracle;

    }
}


