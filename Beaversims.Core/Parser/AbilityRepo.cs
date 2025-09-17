
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Parser
{
    internal sealed class AbilityRepo : IEnumerable<Ability>
    {

        private readonly Dictionary<string, Ability> _abilities = new();

        public void Add(Ability ability) => _abilities[ability.Name] = ability;
        public Ability Get(string abilityName) => _abilities[abilityName];
        public bool Contains(string abilityName) => _abilities.ContainsKey(abilityName);


        public IEnumerator<Ability> GetEnumerator() => _abilities.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
