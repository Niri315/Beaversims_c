using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    public enum BuffSourceType {None, Item, Talent, Race, Spec, Vantus}

    internal class StatMod
    {
        public StatName StatName { get; set; }
        public double Amount { get; set; }
        public StatAmountType AmountType { get; }
        public ScalingData? ScalingData { get; }


        public StatMod(StatName name, StatAmountType type, double amount=0, ScalingData? scData=null)
        {
            StatName = name;
            AmountType = type;
            Amount = amount;
            ScalingData = scData;
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
        public bool RefImpurity { get; set; } = false;
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
        public BuffSourceType SourceType { get; protected set; } = BuffSourceType.None;
        public int SourceObjId { get; protected set; } = 0;

        public StatBuff(int id, UnitId sourceId, string name, int stacks) 
            : base(id, sourceId, name, stacks)
        {
        }
    }
}
