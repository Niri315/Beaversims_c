using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    internal static class AbilityFactory
    {
        private static readonly Dictionary<string, Type> _abilityTypes;

        static AbilityFactory()
        {
            _abilityTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t =>
                    t.IsSubclassOf(typeof(SharedAbility))
                    && !t.IsAbstract)
                .ToDictionary(
                    t => (string)t.GetProperty(nameof(Ability.Name))?
                           .GetValue(Activator.CreateInstance(t))!,
                    t => t
                );
        }

        public static Ability? Create(string abilityName)
        {
            return _abilityTypes.TryGetValue(abilityName, out var type)
                ? (Ability)Activator.CreateInstance(type)!
                : null;
        }

        public static List<Ability> CreateMany(IEnumerable<string> names) =>
            names.Select(Create).Where(a => a is not null).ToList()!;
    }
}