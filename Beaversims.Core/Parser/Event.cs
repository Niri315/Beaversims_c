using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Beaversims.Core.Parser

{
    public enum EventType { 
        Damage, 
        Heal, 
        Cast, 
        Buff,
    }

    internal class Event
    {
        public double timestamp;
        public Unit? SourceUnit;
        public Unit? TargetUnit;
        public EventType? Type;

        public int TargetHp = 0;
        public int TargetMaxHp = 0;
        public int SourceHp = 0;
        public int SourceMaxHp = 0;

        public int EmpCastLevel = 0;

        public int BuffStacks = 0;
        public bool BuffApplyEvent = false;
        public bool BuffRemoveEvent = false;
        public bool DebuffEvent = false;
        public bool BuffStackEvent = false;
        public bool BuffIncEvent = false;
        public bool BuffRefreshEvent = false;
    }
    internal abstract class ThroughputEvent : Event
    {
        public bool FullyAbsorbed = false;
        public bool AbsorbAbility = false;

        public bool Tick = false;
        public bool Crit = false;
        public bool Aoe = false;

        public int AmountEff = 0;
        public int AmountRaw = 0;
        public int AmountAbsorb = 0;
        public int AmountOverheal = 0;
        public int AmountNaeff = 0;  // No Absorb Eff
        public int AmountNaraw = 0;  // No Absorb Raw
    }
    internal class DamageEvent : ThroughputEvent
    {
    }
    internal class HealEvent : ThroughputEvent
    {
    }
    internal class CastEvent : Event
    {
    }
    internal class BuffEvent : Event
    {
    }
}

