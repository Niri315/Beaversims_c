using Beaversims.Core.Common;
using Beaversims.Core.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Beaversims.Core
{
    internal abstract class Spec
    {
        public abstract double MasteryPr { get; }
        protected abstract string SpecAbilityNamespace { get; }
        protected abstract string SpecTalentNamespace { get; }
        protected abstract SpecName SpecName { get; }
        public virtual void InitAbilities(AbilityRepo abilities)
        {
            var abilityTypes = GetType().Assembly
                .GetTypes()
                .Where(t => t.Namespace == SpecAbilityNamespace
                            && typeof(Ability).IsAssignableFrom(t)
                            && !t.IsAbstract);

            foreach (var type in abilityTypes)
                abilities.Add((Ability)Activator.CreateInstance(type)!);
        }

        private readonly Lazy<Dictionary<int, Type>> _talentTypeById;

        protected Spec()
        {
            _talentTypeById = new Lazy<Dictionary<int, Type>>(() =>
                GetType().Assembly
                    .GetTypes()
                    .Where(t =>
                        t.Namespace == SpecTalentNamespace &&
                        !t.IsAbstract &&
                        t.GetField("id", BindingFlags.Public | BindingFlags.Static) != null)
                    .Select(t => new
                    {
                        Type = t,
                        Id = (int?)t.GetField("id", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)
                    })
                    .Where(x => x.Id.HasValue)
                    .ToDictionary(x => x.Id!.Value, x => x.Type));
        }

        private bool TryCreateSpecTalent(int id, int rank, out Talent talent)
        {
            if (_talentTypeById.Value.TryGetValue(id, out var type))
            {
                talent = (Talent)Activator.CreateInstance(type, rank)!;
                return true;
            }
            talent = null!;
            return false;
        }
        public Talent CreateTalent(int id, int rank)
        {
            return TryCreateSpecTalent(id, rank, out var t) ? t : new Talent(id, rank);
        }

        public abstract void SpecIteration(List<Event> events, UnitRepo allUnits);
        public abstract void DupliGainsHeal(ThroughputEvent tEvt, User user, StatName statName, double gainRaw, GainType gainType = GainType.Eff);
        public abstract void DupliGainsDmg(ThroughputEvent tEvt, User user, StatName statName, double gain, GainType gainType = GainType.Dmg);

        public void InitSharedAbilities(AbilityRepo abilities)
        {
            abilities.Add(new Abilities.Leech());
        }
    }
}
