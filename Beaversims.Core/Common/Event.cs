using Beaversims.Core.Specs.Paladin.Holy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Beaversims.Core
{
    public enum EventType { Damage, Heal, Cast, Buff }

    internal class AmountContainer
    {
        public double Eff { get; set; } = 0;
        public double Raw { get; set; } = 0;
        public double Naeff { get; set; } = 0;
        public double Naraw { get; set; } = 0;
        public AmountContainer Clone()
        {
            return new AmountContainer
            {
                Eff = this.Eff,
                Raw = this.Raw,
                Naeff = this.Naeff,
                Naraw = this.Naraw
            };
        }

        public void UpdateAltGainsFromEvtData(ThroughputEvent evt, double gainRaw, int i)
        {
            // Note: Tested throughly. 100% correct to use alt converts for alt gains.
            // Currently doesnt seem to be any issue with negative eff amounts from crit, but this could change.

            var gainEff = evt.AltRawToEffConvert(gainRaw, i);
            var gainNaraw = evt.AltRawToNarawConvert(gainRaw, i);
            var gainNaeff = evt.AltEffToNaeffConvert(gainEff, i);
            Raw += gainRaw;
            Eff += gainEff;
            Naeff += gainNaeff;
            Naraw += gainNaraw;
        }
    }


    internal class AltEvent
    {
        public AmountContainer Amount { get; set; }
        public StatTracker UserStats { get; set; }

        public double leechNukeRaw { get; set; } = 0.0;
        //public StatTracker StatDiffs { get; set; } = new();
        public AltEvent(StatTracker userStats) 
        { 
            UserStats = userStats;
        }

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
        public List<AltEvent> AltEvents { get; set; } = [];
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

        public void CreateAltEvents(User user)
        {
            foreach (var altGearSet in user.altGearSets)
            {
                var statDiffs = Enum.GetValues<StatName>()
                    .ToDictionary(stat => stat, stat => 0.0);

                var altStats = user.PuredStats.Clone();

                foreach (var altGear in altGearSet)
                {
                    foreach (var stat in altGear.Value.Stats)
                    {
                        statDiffs[stat.Key] += stat.Value;
                    }
                }
                foreach (var gear in user.Gear)
                {
                    foreach (var stat in gear.Value.Stats)
                    {
                        statDiffs[stat.Key] -= stat.Value;
                    }
                }
                //foreach (var stat in statDiffs)
                //{
                //    Console.WriteLine($"{stat.Key}: {stat.Value}");
                //}
                foreach (var stat in statDiffs)
                {
                    bool removal;
                    var diff = stat.Value;
                    if (stat.Value < 0.0)
                    {
                        removal = true;
                        diff *= -1;
                    }
                    else
                    {
                        removal = false;
                    }
                    altStats.Get(stat.Key).ChangeAmount(diff, StatAmountType.Rating, removal);
                }
                altStats.UpdateAllStats();

                var altEvent = new AltEvent(altStats);
                AltEvents.Add(altEvent);
            }

            if (this is ThroughputEvent tpEvent)
            {
                foreach (var _altEvent in AltEvents)
                {
                    _altEvent.Amount = tpEvent.Amount.Clone();
                }
            }
        }

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
        public AmountContainer Amount { get; } = new();
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
        public double RawToEffConvert(double value)
        {
            if (Amount.Raw == 0) return 0;
            return value * (Amount.Eff / Amount.Raw);
        }
        public double AltRawToNarawConvert(double value, int i)
        {
            if (AltEvents[i].Amount.Raw == 0) return 0;
            return value * (AltEvents[i].Amount.Naraw / AltEvents[i].Amount.Raw);
        }
        public double AltEffToNaeffConvert(double value, int i)
        {
            if (AltEvents[i].Amount.Eff == 0) return 0;
            return value * (AltEvents[i].Amount.Naeff / AltEvents[i].Amount.Eff);
        }
        public double AltRawToEffConvert(double value, int i)
        {
            if (AltEvents[i].Amount.Raw == 0) return 0;
            return value * (AltEvents[i].Amount.Eff / AltEvents[i].Amount.Raw);
        }

    }

    internal sealed class DamageEvent : ThroughputEvent
    {

    }

    internal sealed class HealEvent : ThroughputEvent
    {


        public double masteryEffectiveness { get; set; }

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
