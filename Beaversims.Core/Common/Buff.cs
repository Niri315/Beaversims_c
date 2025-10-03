using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    public enum BuffSourceType {Item, Talent, Race, Spec, Vantus}

    internal class StatMod
    {
        public StatName StatName { get; }
        public double Amount { get; }
        public StatAmountType AmountType { get; }

        public StatMod(StatName name, double amount, StatAmountType type)
        {
            StatName = name;
            Amount = amount;
            AmountType = type;
        }
    }

    internal class Buff
    {
        public int Id { get; set; }
        public UnitId SourceId { get; set; }
        public string Name { get; set; }
        public int Stacks { get; set; }
        // There are bug in logs where buff refreshes are seen are buff applications
        // Can assume true, but add false on important stat buffs.
        public bool AllowMultiple { get; set; } = true;
        public Buff(int id, UnitId sourceId, string name, int stacks)
        {
            Id = id;
            SourceId = sourceId;
            Name = name;
            Stacks = stacks;
        }
       

    }

    internal class StatBuff : Buff 
    {
        public List<StatMod> StatMods { get; } = new();
        public BuffSourceType SourceType { get; protected set; }
        public int SourceObjId { get; protected set; }
        public StatBuff(int id, UnitId sourceId, string name, int stacks) 
            : base(id, sourceId, name, stacks)
        {
        }
    }
}
