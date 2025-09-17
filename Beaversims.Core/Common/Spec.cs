using Beaversims.Core.Parser;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    internal abstract class Spec
    {
        public abstract void InitAbilities(AbilityRepo abilities);
        public void InitSharedAbilities(AbilityRepo abilities)
        {
            abilities.Add(new Leech());
        }
        public abstract void SpecIteration(List<Event> events, UnitRepo allUnits);
    }
}
