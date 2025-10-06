using Beaversims.Core.Parser;
using Beaversims.Core.Sim;
using System.Reflection;
using System.Xml.Linq;

namespace Beaversims.Core
{
    public enum Role { Tank, Healer, Dps }
    public readonly record struct UnitId(int TypeId, int InstanceId);

    internal class Unit(string name, UnitId id)
    {
        public string Name { get; } = name;
        public UnitId Id { get; } = id;

        public List<Buff>? Buffs { get; set; } = [];
        public long? Hp { get; set; }
        public long? MaxHp { get; set; }
        public Coord? Coords { get; set; }

        public bool IsUnit(Unit otherUnit) => Id == otherUnit.Id;
        public bool HasBuff(int buffId) => Buffs.Any(b => b.Id == buffId);
        public bool HasAnyBuff(HashSet<int> buffIds) => Buffs.Any(b => buffIds.Contains(b.Id));
        public bool HasAnyBuffFromPlayer(HashSet<int> buffIds, UnitId unitId) => Buffs.Any(b => buffIds.Contains(b.Id) && b.SourceId == unitId);

        protected Buff? FindBuff(int buffId, UnitId sourceId) =>
            Buffs.Find(b => b.Id == buffId && b.SourceId == sourceId);

        public virtual void AddBuff(string buffName, int buffId, Unit sourceUnit, int stacks, Logger statLogger = null, double timeStamp = 0)
        {
            var buff = new Buff(buffId, sourceUnit.Id, buffName, stacks);
            if (!buff.AllowMultiple)
            {
                RemoveBuff(buffId, sourceUnit, statLogger);
            }
            Buffs.Add(buff);
        }

        public virtual bool RemoveBuff(int buffId, Unit sourceUnit, Logger statLogger = null, double timeStamp = 0)
        {
            var idx = Buffs.FindIndex(b => b.Id == buffId && b.SourceId == sourceUnit.Id);
            if (idx < 0) return false;
            Buffs.RemoveAt(idx);
            return true;
        }

        public virtual void ChangeBuffStack(string buffName, int buffId, Unit sourceUnit, int newStacks, Logger statLogger = null, double timeStamp = 0)
        {
            var buff = FindBuff(buffId, sourceUnit.Id);
            if (buff is null)
            {
                AddBuff(buffName, buffId, sourceUnit, newStacks, statLogger);
                return;
            }

            buff.Stacks = newStacks;
        }
    }

    internal class Player : Unit
    {
        public Role Role { get; init; }
        public long DamageDone { get; set; }
        public long HealingDone { get; set; }

        public Dictionary<int, Talent> Talents { get; } = [];
        public List<Item> Items { get; } = [];
        public double HCGM { get; set; } = 1.0;  //Haste Cast Gain Mod

        public bool HasVantus { get; set; } = false;
       
        public int TalentRank(int id) => Talents.TryGetValue(id, out var talent) ? talent.Rank : 0;
        public bool HasTalent(int id) => Talents.ContainsKey(id);

        public Player(string name, UnitId id, Role role) : base(name, id)
        {
            Role = role;
        }
    }

    internal class User : Player
    {
        public Spec Spec { get; set; }
        public AbilityRepo Abilities { get; } = new();
        public HashSet<int> SummonIds { get; set; } = []; // Type Ids only
        public StatTracker? Stats { get; set; } = new();
        // If user doesnt have permanent leech for fight, revert to calculate leech value by leech data from other sims.
        public bool HasPermaLeech {  get; set; } = false;

        // Paladin
        public bool AwakeningActive { get; set; } = false;


        public virtual void InitCustomBuffs()
        {
            var buffTypes = GetType().Assembly
                .GetTypes()
                .Where(t => t.Namespace == "Beaversims.Core.Data.CustomBuffs"
                            && typeof(StatBuff).IsAssignableFrom(t)
                            && !t.IsAbstract);
            foreach (var type in buffTypes)
            {
                var buff = (StatBuff)Activator.CreateInstance(type, Id)!;
                ProcessStatBuff(buff, this);
                Buffs.Add(buff);
            }
        }

        // --- Reflection cache for StatBuff types by id ---
        private static readonly Lazy<Dictionary<int, Type>> _statBuffTypeById = new(() =>
            typeof(StatBuff).Assembly
                .GetTypes()
                .Where(t =>
                    t.Namespace == "Beaversims.Core.Data.StatBuffs" &&
                    !t.IsAbstract &&
                    t.GetField("id", BindingFlags.Public | BindingFlags.Static) != null)
                .Select(t => new
                {
                    Type = t,
                    Id = (int?)t.GetField("id", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)
                })
                .Where(x => x.Id.HasValue)
                .ToDictionary(x => x.Id!.Value, x => x.Type));

        private static bool TryCreateStatBuff(int buffId, Unit sourceUnit, int stacks, out Buff buff)

        {
            if (_statBuffTypeById.Value.TryGetValue(buffId, out var type))
            {
                buff = (Buff)Activator.CreateInstance(type, sourceUnit.Id, stacks)!;
                return true;
            }

            buff = null!;
            return false;
        }

        public void ProcessStatBuff(StatBuff buff, Unit sourceUnit)
        {
            foreach (var mod in buff.StatMods)
            {
                var amount = mod.Amount;

                if(buff.SourceType == BuffSourceType.Talent)
                {
                    amount *= TalentRank(buff.SourceObjId);
                }
                else if (buff.SourceType == BuffSourceType.Item && sourceUnit is Player sourcePlayer)
                {
                    Console.WriteLine(buff.Name);
                    Console.WriteLine(buff.SourceType);
                    var sourceItem = sourcePlayer.Items.FirstOrDefault(i => i.Id == buff.SourceObjId);
                    if (sourceItem != null && mod.ScalingData != null)
                    {
                        amount = ScUtils.ScaledEffectValue(sourceItem.Ilvl, sourceItem.ItemSlot, mod.StatName, mod.ScalingData);
                        Console.WriteLine(amount.ToString());
                    }
                    else
                    {
                        continue;
                    }
                   
                }
                mod.Amount = amount;
                var stat = Stats.Get(mod.StatName);
                stat.ChangeAmount(amount * buff.Stacks, mod.AmountType, removal: false);
            }
        }

        public override void AddBuff(string buffName, int buffId, Unit sourceUnit, int stacks, Logger statLogger = null, double timeStamp = 0)
        {
            var sourceId = sourceUnit.Id;
            Buff buff = TryCreateStatBuff(buffId, sourceUnit, stacks, out var created)
                ? created
                : new Buff(buffId, sourceId, buffName, stacks);

            if (buff is StatBuff statBuff)
            {
                ProcessStatBuff(statBuff, sourceUnit);
                if (statLogger != null) { Stats.LogStats(statLogger, timeStamp); }
            }
            if (!buff.AllowMultiple)
            {
                RemoveBuff(buffId, sourceUnit, statLogger);
            }
            Buffs.Add(buff);
        }

        public override bool RemoveBuff(int buffId, Unit sourceUnit, Logger statLogger = null, double timeStamp = 0)
        {
            var sourceId = sourceUnit.Id;
            var idx = Buffs.FindIndex(b => b.Id == buffId && b.SourceId == sourceId);
            if (idx < 0) return false;

            var buff = Buffs[idx];
            if (buff is StatBuff statBuff)
            {
                foreach (var mod in statBuff.StatMods)
                {
                    Stats.Get(mod.StatName).ChangeAmount(mod.Amount * buff.Stacks, mod.AmountType, removal: true);
                    if (statLogger != null) { Stats.LogStats(statLogger, timeStamp); }
                }
            }

            Buffs.RemoveAt(idx);
            return true;
        }

        public override void ChangeBuffStack(string buffName, int buffId, Unit sourceUnit, int newStacks, Logger statLogger = null, double timeStamp = 0)
        {
            var sourceId = sourceUnit.Id;
            var buff = Buffs.Find(b => b.Id == buffId && b.SourceId == sourceId);
            if (buff is null)
            {
                AddBuff(buffName, buffId, sourceUnit, newStacks, statLogger);
                return;
            }

            if (buff is StatBuff statBuff)
            {
                int oldStacks = buff.Stacks;
                int diff = newStacks - oldStacks;
                if (diff != 0)
                {
                    bool removal = diff < 0;
                    int magnitude = Math.Abs(diff);

                    foreach (var mod in statBuff.StatMods)
                    {
                        Stats.Get(mod.StatName).ChangeAmount(mod.Amount * magnitude, mod.AmountType, removal);
                        if (statLogger != null) { Stats.LogStats(statLogger, timeStamp); }
                    }
                }
            }

            buff.Stacks = newStacks;
        }

        public User(string name, UnitId id, Role role) : base(name, id, role) 
        {
        }
    }
}
