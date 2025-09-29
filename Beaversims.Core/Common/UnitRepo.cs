using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    internal sealed class UnitRepo : IEnumerable<Unit>
    {
        private UnitId UserId { get; }

        private readonly Dictionary<UnitId, Unit> _units = new();
        public UnitRepo(UnitId userId)
        {
            UserId = userId;
        }
        public void Add(Unit unit) => _units[unit.Id] = unit;
        public Unit? Get(UnitId id) => _units.TryGetValue(id, out var u) ? u : null;
        public User GetUser() => (User)_units[UserId];
        public bool Contains(UnitId id) => _units.ContainsKey(id);

        public IEnumerator<Unit> GetEnumerator() => _units.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
