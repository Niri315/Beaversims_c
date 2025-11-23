using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Beaversims.Core.Common;

namespace Beaversims.Core.Specs.Shaman.Resto
{

    internal class RestorationShaman : Spec
    {
        public const double masteryPr_s = Mastery.tooltipPercentRate / 3;
        public override double MasteryPr { get; } = masteryPr_s;
        protected override string SpecAbilityNamespace => "Beaversims.Core.Specs.Shaman.Resto.Abilities";
        protected override string SpecTalentNamespace => "Beaversims.Core.Specs.Shaman.Resto.Talents";
        public override SpecName SpecName => SpecName.RestorationShaman;



       
        public override void SpecIteration(List<Event> events, UnitRepo allUnits, Fight fight, int iterationCount)
        {
            Main.SpecMain(events, allUnits, fight, iterationCount);
        }

    }
    

    internal class RestoFarseer : RestorationShaman
    {
        public const int idTalent = 117485;  
        public override HeroTlName HeroTlName => HeroTlName.Farseer;

    }
    internal class RestoTotemic : RestorationShaman
    {
        public const int idTalent = 117474;  
        public override HeroTlName HeroTlName => HeroTlName.Totemic;

    }
}


