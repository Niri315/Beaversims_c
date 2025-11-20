using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Beaversims.Core.Common;

namespace Beaversims.Core.Specs.Evoker.Pres
{

    internal class PreservationEvoker : Spec
    {
        public const double masteryPr_s = Mastery.tooltipPercentRate / 1.8;
        public override double MasteryPr { get; } = masteryPr_s;
        protected override string SpecAbilityNamespace => "Beaversims.Core.Specs.Evoker.Pres.Abilities";
        protected override string SpecTalentNamespace => "Beaversims.Core.Specs.Evoker.Pres.Talents";
        public override SpecName SpecName => SpecName.PreservationEvoker;



       
        public override void SpecIteration(List<Event> events, UnitRepo allUnits, Fight fight, int iterationCount)
        {
            Main.SpecMain(events, allUnits, fight, iterationCount);
        }

    }
    

    internal class PresChronowarden : PreservationEvoker
    {
        public const int idTalent = 117551;  // [Chrono Flame]
        public override HeroTlName HeroTlName => HeroTlName.Chronowarden;

    }
    internal class PresFlameshaper : PreservationEvoker
    {
        public const int idTalent = 117547;  // [Engulf]
        public override HeroTlName HeroTlName => HeroTlName.Flameshaper;

    }
}


