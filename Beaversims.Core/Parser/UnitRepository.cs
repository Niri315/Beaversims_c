using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Parser
{
    internal sealed class UnitRepository
    {
        private readonly Dictionary<(int id, int instanceId), Unit> _units = new();
        public void Add(Unit unit) => _units[(unit.Id, unit.InstanceId)] = unit;
        public Unit? Get(int id, int instanceId) => _units.TryGetValue((id, instanceId), out var u) ? u : null;
    }
}
