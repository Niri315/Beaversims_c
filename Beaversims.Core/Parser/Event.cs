
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Beaversims.Core.Parser
{
    public enum EventType { Damage, Heal, Cast, Buff }

    public class AmountContainer
    {
        public int Eff = 0;
        public int Raw = 0;
        public int Absorb = 0;
        public int Overheal = 0;
        public int Naeff = 0;
        public int Naraw = 0;
    }

    internal class Event
    {
        public double Timestamp { get; set; }
        public Unit? SourceUnit { get; set; }
        public Unit? TargetUnit { get; set; }
        public int? AbilityId { get; set; }
        public string? AbilityName { get; set; }
        public Ability? Ability { get; set; }
        public bool UserSuperSource { get; set; } = false;
        public Coord? TargetCoords { get; set; }

        public int TargetHp { get; set; }
        public int TargetMaxHp { get; set; }
        public int SourceHp { get; set; }
        public int SourceMaxHp { get; set; }
    }

    internal abstract class ThroughputEvent : Event
    {
        public bool Tick { get; set; }
        public bool Crit { get; set; }
        public bool Aoe { get; set; }
        public AmountContainer Amount { get; set; } = new();

    }

    internal sealed class DamageEvent : ThroughputEvent { }

    internal sealed class HealEvent : ThroughputEvent
    {
        public bool FullyAbsorbed { get; set; }
        public bool AbsorbAbility { get; set; }
    }

    internal sealed class CastEvent : Event
    {
        public int EmpCastLevel { get; set; }
    }

    internal sealed class BuffEvent : Event
    {
        public int BuffStacks { get; set; }
        public bool BuffApplyEvent { get; set; }
        public bool BuffRemoveEvent { get; set; }
        public bool DebuffEvent { get; set; }
        public bool BuffStackEvent { get; set; }
        public bool BuffIncEvent { get; set; }
        public bool BuffRefreshEvent { get; set; }
    }
}
