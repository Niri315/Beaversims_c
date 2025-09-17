using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Beaversims.Core.Parser;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;

namespace Beaversims.Core.Specs.Paladin.Holy
{
    internal class HolyPaladin : Spec
    {
        public override void InitAbilities(AbilityRepo abilities)
        {
            var abilityTypes = typeof(HolyPaladin).Assembly
                .GetTypes()
                .Where(t => t.Namespace == "Beaversims.Core.Specs.Paladin.Holy.Abilities"
                            && typeof(Ability).IsAssignableFrom(t)
                            && !t.IsAbstract);

            foreach (var type in abilityTypes)
                abilities.Add((Ability)Activator.CreateInstance(type)!);
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

