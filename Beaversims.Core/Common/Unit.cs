using Beaversims.Core.Parser;
using Beaversims.Core.Sim;
using System;
using System.Net.Security;
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

        //Druid
        public int HarmonyLevel { get; set; } = 0;
        public int QIMIncHarmonyCount { get; set; } = 0;


        //Evoker

        public Dictionary<Ability, double[]> ReversionTracker { get; set; } = [];
        //public double? RevBuffEnd { get; set; } = null;
        //public double? RevEchoBuffEnd {  get; set; } = null;

        public bool IsUnit(Unit otherUnit) => Id == otherUnit.Id;
        public bool HasBuff(int buffId) => Buffs.Any(b => b.Id == buffId);
        public Buff? GetBuff(int buffId) => Buffs.Find(b => b.Id == buffId);
        public bool HasAnyBuff(HashSet<int> buffIds) => Buffs.Any(b => buffIds.Contains(b.Id));
        public bool HasAnyBuffFromPlayer(HashSet<int> buffIds, UnitId unitId) => Buffs.Any(b => buffIds.Contains(b.Id) && b.SourceId == unitId);

        protected Buff? FindBuff(int buffId, UnitId sourceId) =>
            Buffs.Find(b => b.Id == buffId && b.SourceId == sourceId);

        public virtual void AddBuff(string buffName, int buffId, Unit sourceUnit, int stacks, double timeStamp, BuffEvent evt = null, Logger statLogger = null, Logger refStatLogger = null)
        {
            var buff = new Buff(buffId, sourceUnit.Id, buffName, stacks);
            if (!buff.AllowMultiple)
            {
                RemoveBuff(buffId, sourceUnit, evt, statLogger);
            }
            if (buff.Duration > 0)
            {
                buff.BuffEnd = timeStamp + buff.Duration;
            }
            Buffs.Add(buff);
        }

        public virtual bool RemoveBuff(int buffId, Unit sourceUnit, BuffEvent evt = null, Logger statLogger = null, double timeStamp = 0, Logger refStatLogger = null)
        {
            var idx = Buffs.FindIndex(b => b.Id == buffId && b.SourceId == sourceUnit.Id);
            if (idx < 0) return false;
            Buffs.RemoveAt(idx);
            return true;
        }

        public virtual void ChangeBuffStack(string buffName, int buffId, Unit sourceUnit, int newStacks, BuffEvent evt = null, Logger statLogger = null, double timeStamp = 0, Logger refStatLogger = null)
        {
            var buff = FindBuff(buffId, sourceUnit.Id);
            if (buff is null)
            {
                AddBuff(buffName, buffId, sourceUnit, newStacks, timeStamp, evt, statLogger);
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
        public Dictionary<ItemSlot, Item> Items { get; } = [];
        public GearSet Gear { get; } = [];


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
        //public bool SwMode { get; set; }
        public SimMode SimMode { get; set; }
        public Spec Spec { get; set; }
        public AbilityRepo Abilities { get; } = new();
        public HashSet<int> SummonIds { get; set; } = []; // Type Ids only
        public GainDict OriginalTotals { get; set; } = Utils.InitGainDict();
        public StatTracker Stats { get; set; } = new();
        public Dictionary<StatName, double> TotalGearRatings { get; set; } = Utils.InitStatDict();

        //public HashSet<NonHasteProcEffect> NonHasteProcEffects { get; } = new();
        //public HashSet<OnUseEffect> OnUseEffects { get; } = new();
        //public List<SpecialEffect> AllEffects {  get; } = new();
        public StatTracker? RefStats { get; set; }
        // If user doesnt have permanent leech for fight, revert to calculate leech value by leech data from other sims.
        public bool HasPermaLeech {  get; set; } = false;
        public List<GearSet> AltGearSets { get; set; } = [];
        // Don't need alt versions of this, math works out with calculating it based on original log data.
        //public double HCGM { get; set; } = 1;
        public double CastTimeGain { get; set; } = 0;
        public double TrueCastTimeTotal { get; set; } = 0;
        public double HasteCapCTLoss { get; set; } = 0;
        public int Casts {  get; set; } = 0;


        // Paladin
        public bool AwakeningActive { get; set; } = false;
        public bool BanCritScaleJudgAC { get; set; } = false;
        public int AwakeningCount { get; set; } = 0;
        public double AvengingUseEnd { get; set; } = 0; // Only the active use avenging, not the awakening effect.
        public int ArmamentsBuffCount { get; set; } = 0;

        // Druid
        public List<Specs.Druid.Resto.Abilities.RestoAbility> CIMDepMastIncScalers = [];

        // Evoker
        public int MaxEmpLevel { get; set; } = 3;
        private int _lifebindCount = 0;
        public int LifebindCount
        {
            get => _lifebindCount;
            set => _lifebindCount = Math.Max(0, value);
        }
        public int LeapingFlamesLevel { get; set; } = 0;
        public List<string> StasisStore { get; set; } = [];
        public double LastStasisRelease { get; set; } = -99;
        public int MasteryTest1 { get; set; } = 0;
        public int MasteryTest2 { get; set; } = 0;

        public double HasteCapCTGLossMod(int i)
        {
            // Loss value is already baked into CTG
            return (CastTimeGain - AltGearSets[i].HasteCapCTGLoss) / CastTimeGain;
        }

        public void SetTotalGearRatings()
        {
            TotalGearRatings = Utils.InitStatDict();
            foreach (var gear in Gear.Values)
            {
                foreach (var stat in gear.Stats)
                {
                    TotalGearRatings[stat.Key] += stat.Value;
                }
            }
        }

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
                bool inactive = true;
                foreach (var mod in buff.StatMods)
                {
                    if (mod.Amount > 0)
                    {
                        inactive = false; break;
                    }
                }
                if (!inactive)
                {
                    Buffs.Add(buff);
                }
              
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

        public void ProcessStatBuff(StatBuff buff, Unit sourceUnit, BuffEvent evt = null)
        {
            
            foreach (var mod in buff.StatMods)
            {
                var amount = mod.Amount;

                if (buff.SourceType == BuffSourceType.Talent)
                {
                    amount *= TalentRank(buff.SourceObjId);
                }
                else if (buff.SourceType == BuffSourceType.Item && sourceUnit is Player sourcePlayer)
                {
                    var sourceItem = sourcePlayer.Items.Values.FirstOrDefault(i => i.Id == buff.SourceObjId);
                    if (sourceItem != null && mod.ScalingData != null)
                    {
                        amount = ScUtils.ScaledEffectValue(sourceItem.Ilvl, sourceItem.ItemSlot, mod.ScalingData, mod.StatName);
                    }
                    else
                    {
                        continue;
                    }
                   
                }
                else if (buff.SourceType == BuffSourceType.Spec)
                {
                    if ((int)Spec.SpecName != buff.SourceObjId)
                    {
                        amount = 0;
                    }
                }
                mod.Amount = amount;
                var stat = Stats.Get(mod.StatName);
                stat.ChangeAmount(amount * buff.Stacks, mod.AmountType, removal: false);
                // if PuredStats is null its before event parsing. Stats are copied after init and before vantus. Only need to track PuredStats during events.
                if (RefStats != null && !buff.SimImpurity)
                {
                    if (evt is BuffEvent)
                    {
                        evt.StoreRefStatChange(mod.StatName, amount * buff.Stacks, mod.AmountType, removal: false);
                    }
                    var refStat = RefStats.Get(mod.StatName);
                    refStat.ChangeAmount(amount * buff.Stacks, mod.AmountType, removal: false);

                }
            }
        }

        public override void AddBuff(string buffName, int buffId, Unit sourceUnit, int stacks, double timeStamp, BuffEvent evt = null, Logger statLogger = null, Logger refStatLogger = null)
        {
            var sourceId = sourceUnit.Id;
            Buff buff = TryCreateStatBuff(buffId, sourceUnit, stacks, out var created)
                ? created
                : new Buff(buffId, sourceId, buffName, stacks);

            if (buff is StatBuff statBuff)
            {
                if (statBuff.SimImpurity && (!Utils.SimsActive(this) || sourceUnit is not User))
                {
                    statBuff.SimImpurity = false;
                }
                
                ProcessStatBuff(statBuff, sourceUnit, evt);
                if (statLogger != null) { Stats.LogStats(statLogger, timeStamp); }
                if (refStatLogger != null && RefStats != null) { RefStats.LogStats(refStatLogger, timeStamp); }
            }
            if (!buff.AllowMultiple)
            {
                RemoveBuff(buffId, sourceUnit, evt, statLogger);
            }
            if (buff.Duration > 0)
            {
                buff.BuffEnd = timeStamp + buff.Duration;
            }
            Buffs.Add(buff);
        }

        public override bool RemoveBuff(int buffId, Unit sourceUnit, BuffEvent evt = null, Logger statLogger = null, double timeStamp = 0, Logger refStatLogger = null)
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
                    if (!statBuff.SimImpurity)
                    {
                        if (evt is BuffEvent)
                        {
                            evt.StoreRefStatChange(mod.StatName, mod.Amount * buff.Stacks, mod.AmountType, removal: true);
                        }
                        RefStats.Get(mod.StatName).ChangeAmount(mod.Amount * buff.Stacks, mod.AmountType, removal: true);
                       
                    }
                    if (statLogger != null) { Stats.LogStats(statLogger, timeStamp); }
                    if (refStatLogger != null) { RefStats.LogStats(refStatLogger, timeStamp); }
                }
            }

            Buffs.RemoveAt(idx);
            return true;
        }

        public override void ChangeBuffStack(string buffName, int buffId, Unit sourceUnit, int newStacks, BuffEvent evt = null, Logger statLogger = null, double timeStamp = 0, Logger refStatLogger = null)
        {
            var sourceId = sourceUnit.Id;
            var buff = Buffs.Find(b => b.Id == buffId && b.SourceId == sourceId);
            if (buff is null)
            {
                AddBuff(buffName, buffId, sourceUnit, newStacks, timeStamp, evt, statLogger);
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
                       
                        if (!statBuff.SimImpurity)
                        {
                            if (evt is BuffEvent)
                            {
                                evt.StoreRefStatChange(mod.StatName, mod.Amount * magnitude, mod.AmountType, removal);
                            }
                            RefStats.Get(mod.StatName).ChangeAmount(mod.Amount * magnitude, mod.AmountType, removal);
                            
                        }
                        if (statLogger != null) { Stats.LogStats(statLogger, timeStamp); }
                        if (refStatLogger != null) { RefStats.LogStats(refStatLogger, timeStamp); }
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
