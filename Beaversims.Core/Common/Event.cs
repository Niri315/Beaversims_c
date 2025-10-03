using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Beaversims.Core
{
    public enum EventType { Damage, Heal, Cast, Buff }

    public class AmountContainer
    {
        public double Eff { get; set; } = 0;
        public double Raw { get; set; } = 0;
        public double Absorb { get; set; } = 0;
        public double Overheal { get; set; } = 0;
        public double Naeff { get; set; } = 0;
        public double Naraw { get; set; } = 0;
    }


    internal class Event
    {
        public double Timestamp { get; set; }
        public Unit SourceUnit { get; set; }
        public Unit TargetUnit { get; set; }
        public int AbilityId { get; set; }
        public string AbilityName { get; set; }
        public Ability Ability { get; set; }
        public bool UserSuperSource { get; set; } = false;
        public Coord? TargetCoords { get; set; }
        public Coord? SourceCoords { get; set; }
        public Coord? UserCoords { get; set; }

        // Hp is set AFTER damage/heal amount takes place.
        public long? TargetHp { get; set; }
        public long? TargetMaxHp { get; set; }
        public long? SourceHp { get; set; }
        public long? SourceMaxHp { get; set; }
        public long? UserHp { get; set; }
        public long? UserMaxHp { get; set; }
        public GainMatrix Gains { get; set; } = new();
        public StatTracker UserStats { get; set; }
        public bool SummerActive { get; set; } = false;

        //Paladin
        public int BeaconCount { get; set; }
        public bool AwakenedJudgment { get; set; } = false;
        public bool AwakenedCast {  get; set; } = false;

        // TODO implement preEvent option.
        public double? SourceHp_p()//(bool preEvent=false) 
        {
            if (SourceHp == null) return 1.0;  // Default to assuming percent is 100 if it cant be found.
            return (double?) SourceHp / SourceMaxHp; 
        }
        public double? TargetHp_p()//(bool preEvent = false)
        {
            if (TargetHp == null) return 1.0;  // Default to assuming percent is 100 if it cant be found.
            return (double?)TargetHp / TargetMaxHp;
        }

        public bool IsDamageTakenEvent() => TargetUnit is User && this is DamageEvent;
        public bool IsHealDoneEvent() => this is HealEvent && UserSuperSource;
        public bool IsDmgDoneEvent() => this is DamageEvent && UserSuperSource && TargetUnit is not User;
  
        public Event()
        {
            Gains = Utils.InitGainMatrix();
        }
    }

    internal abstract class ThroughputEvent : Event
    {
        public bool Tick { get; set; } = false;
        public bool Crit { get; set; } = false;
        public bool Aoe { get; set; } = false;
        public AmountContainer Amount { get; set; } = new();
        public bool FullyAbsorbed { get; set; } = false;
        public bool AbsorbAbility { get; set; } = false;


        // Non DR abilities
        // Spirit Link, 
        public bool IsDrEvent() => IsDamageTakenEvent() && !Ability.IgnoreDr;
        public bool IsAvoidanceEvent() => IsDrEvent() && Aoe;

        public double RawToNarawConvert(double value)
        {
            if (Amount.Raw == 0) return 0;
            return value * (Amount.Naraw / Amount.Raw);
        }
        public double EffToNaeffConvert(double value)
        {
            if (Amount.Eff == 0) return 0;
            return value * (Amount.Naeff / Amount.Eff);
        }

    }

    internal sealed class DamageEvent : ThroughputEvent
    {

    }

    internal sealed class HealEvent : ThroughputEvent
    {


        public double masteryEffectiveness { get; set; }
        public double RawToEffConvert(double value)
        {
            if (Amount.Raw == 0) return 0;
            return value * (Amount.Eff / Amount.Raw);
        }
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
