using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Beaversims.Core.Common;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;

namespace Beaversims.Core.Specs.Paladin.Holy
{

    internal class HolyPaladin : Spec
    {
        public const double masteryPr_s = 466.4691943;
        public override double MasteryPr { get; } = masteryPr_s;
        protected override string SpecAbilityNamespace => "Beaversims.Core.Specs.Paladin.Holy.Abilities";
        protected override string SpecTalentNamespace => "Beaversims.Core.Specs.Paladin.Holy.Talents";
        protected override SpecName SpecName => SpecName.HolyPaladin;
        public override void DupliGainsHeal(ThroughputEvent tEvt, User user, StatName statName, double gainRaw, GainType gainType = GainType.Eff)
        {
            DupliEffects.BeaconGains(tEvt, user, statName, gainRaw, gainType);
        }
        public override void DupliGainsDmg(ThroughputEvent tEvt, User user, StatName statName, double gain, GainType gainType = GainType.Eff)
        {
        }
        public override void SpecIteration(List<Event> events, UnitRepo allUnits)
        {
            Main.SpecMain(events, allUnits);
        }

    }
    
    internal class HolyLightsmith : HolyPaladin
    {
        public const int idTalent = 117882;  // Holy Armaments

    }

    internal class HolyHeraldOfTheSun : HolyPaladin
    {
        public const int idTalent = 117696;  // Dawnlight

    }
}

