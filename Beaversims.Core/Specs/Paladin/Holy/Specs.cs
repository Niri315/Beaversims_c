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
        public const double masteryPr_s = Mastery.tooltipPercentRate / 1.5;
        public override double MasteryPr { get; } = masteryPr_s;
        protected override string SpecAbilityNamespace => "Beaversims.Core.Specs.Paladin.Holy.Abilities";
        protected override string SpecTalentNamespace => "Beaversims.Core.Specs.Paladin.Holy.Talents";
        protected override SpecName SpecName => SpecName.HolyPaladin;

        public override void DupliGainsHeal(ThroughputEvent evt, User user, StatName statName, double gainRaw, GainType gainType = GainType.Eff)
        {
            DupliEffects.BeaconGains(evt, user, statName, gainRaw, gainType);
            var gainNaraw = evt.RawToNarawConvert(gainRaw);
            if (Shared.DupliEffects.IsLeechSourceEvent(evt))
            {
                Shared.DupliEffects.LeechSourceGains(evt, user, statName, gainNaraw, gainType);

            }

            Shared.DupliEffects.SummerGains(evt, user, statName, gainRaw, evt.Ability, evt.SummerActive, evt.AbsorbAbility, evt.SourceUnit, gainType);
        }
        public override void DupliGainsDmg(ThroughputEvent evt, User user, StatName statName, double gain, GainType gainType = GainType.Dmg)
        {
            var gainNaeff = evt.EffToNaeffConvert(gain);
            if (Shared.DupliEffects.IsLeechSourceEvent(evt))
            {
                Shared.DupliEffects.LeechSourceGains(evt, user, statName, gainNaeff, gainType);
            }
            Shared.DupliEffects.SummerGains(evt, user, statName, gain, evt.Ability, evt.SummerActive, evt.AbsorbAbility, evt.SourceUnit, gainType);

        }
        public override void SpecIteration(List<Event> events, UnitRepo allUnits, Fight fight)
        {
            Main.SpecMain(events, allUnits, fight);
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


