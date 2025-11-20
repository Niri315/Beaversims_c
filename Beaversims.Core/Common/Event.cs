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

        public void UpdateAltGainsFromEvtData(TpEvent evt, double gainRaw, int i)
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
        public StatTracker? UserStats { get; set; }
        public Dictionary<StatName, double> SimStatEffInc { get; set; } = Utils.InitStatDict();
        //public List<StatChange> SimStatChanges { get; set; } = [];
        //public Dictionary<StatName, double> SimStatRatingInc { get; set; } = Utils.InitStatDict();
        public double NukeRaw { get; set; } = 0.0;
        //public StatTracker StatDiffs { get; set; } = new();
        //public void StoreSimStatChange(StatName statName, double amount, StatAmountType type, bool removal)
        //{
        //    SimStatChanges.Add(new StatChange(statName, amount, type, removal));
        //}
        public AltEvent() 
        { 

        }

    }



    internal class Event
    {
        public double Timestamp { get; set; }
        public bool Heartbeat { get; set; } = false;
        public bool Proc { get; set; } = false;
        public Unit SourceUnit { get; set; }
        public Unit TargetUnit { get; set; }
        public int AbilityId { get; set; }
        public string AbilityName { get; set; }
        public Ability Ability { get; set; }
        public virtual bool UserSuperSource { get; set; } = false;
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
        public StatTracker RefStats { get; set; }
        public List<AltEvent> AltEvents { get; set; } = [];
        public bool SummerActive { get; set; } = false;
        public string? HealAbsorbAbilityName { get; set; }
        public bool SimImpurity { get; set; } = false;
        public virtual bool SimEvent { get; set; } = false;
        public bool NonScInstaTick { get; set; } = false; // need setup in spec main


        //Paladin
        public int BeaconCount { get; set; }
        public bool AwakenedJudgment { get; set; } = false;
        public bool AwakenedCast {  get; set; } = false;
        public bool BanCritScaleJudgAC { get; set; } = false;

        //Druid
        public bool TargetHasRegrowth { get; set; } = false;
        public int AbundanceStacks { get; set; } = 0;
        public bool IsSymbRelEvent { get; set; } = false;
        public bool UserHasHotw {  get; set; } = false;

        // Evoker
        public bool LifebindEvent { get; set; } = false;
        public int LifebindCount { get; set; } = 0; 


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

        //public void CreateAltEvents(User user, Event evt)
        //{
        //    foreach (var altGearSet in user.AltGearSets)
        //    {
        //        //var statDiffs = Enum.GetValues<StatName>()
        //        //.ToDictionary(stat => stat, stat => 0.0);

        //        //var altStats = evt.RefStats.Clone();

        //        ////foreach (var altGear in altGearSet)
        //        ////{
        //        //foreach (var stat in altGearSet.TotalGearRatings)
        //        //{
        //        //    statDiffs[stat.Key] += stat.Value;
        //        //}
        //        ////}
        //        ////foreach (var gear in user.Gear)
        //        ////{
        //        //foreach (var stat in user.TotalGearRatings)
        //        //{
        //        //    statDiffs[stat.Key] -= stat.Value;
        //        //}
        //        ////}

        //        //foreach (var stat in statDiffs)
        //        //{
        //        //    bool removal;
        //        //    var diff = stat.Value;
        //        //    if (stat.Value < 0.0)
        //        //    {
        //        //        removal = true;
        //        //        diff *= -1;
        //        //    }
        //        //    else
        //        //    {
        //        //        removal = false;
        //        //    }
        //        //    altStats.Get(stat.Key).ChangeAmount(diff, StatAmountType.Rating, removal);
        //        //}
        //        //altStats.UpdateAllStats();
        //        var altEvent = new AltEvent();
        //        if (evt is TpEvent _tEvt)
        //        {
        //            altEvent.Amount = _tEvt.Amount.Clone();
        //        }
        //        //altEvent.UserStats = altStats;
        //        evt.AltEvents.Add(altEvent);

        //    }
        //}
        public void CreateAltEvents(User user, Event evt)
        {
            foreach (var altGearSet in user.AltGearSets)
            {
                var statDiffs = Enum.GetValues<StatName>()
                .ToDictionary(stat => stat, stat => 0.0);

                var altStats = evt.RefStats.Clone();

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
                var altEvent = new AltEvent();
                if (evt is TpEvent _tEvt)
                {
                    altEvent.Amount = _tEvt.Amount.Clone();
                }
                altEvent.UserStats = altStats;
                evt.AltEvents.Add(altEvent);

            }
        }


        public Event()
        {
            Gains = Utils.InitGainMatrix();
        }
    }

    internal abstract class TpEvent : Event
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

    internal class DamageEvent : TpEvent
    {

    }

    internal class HealEvent : TpEvent
    {


        public double MasteryEffectiveness { get; set; }
        public bool MasteryActive { get; set; } = false;

    }

    internal class SimDamageEvent : DamageEvent
    {
        public override bool UserSuperSource  { get; set; } = true;
        public override bool SimEvent { get; set; } = true;
    }
    internal class SimHealEvent : HealEvent
    {
        public override bool UserSuperSource { get; set; } = true;
        public override bool SimEvent { get; set; } = true;
    }

    internal sealed class CastEvent : Event
    {
        public int EmpCastLevel { get; set; }
        public double PostReductCT { get; set; }
        public double ScalingReductRatio { get; set; }
    }

    internal class StatChange
    {
        public StatName StatName { get; set; }
        public double Amount { get; set; }
        public StatAmountType Type { get; set; }
        public bool Removal { get; set; }
        public StatChange(StatName statName, double amount, StatAmountType type, bool removal)
        {
            StatName = statName;
            Amount = amount;
            Type = type;
            Removal = removal;
        }
    }

    internal sealed class BuffEvent : Event
    {
        public List<StatChange> RefStatChanges { get; set; } = [];
        public int BuffStacks { get; set; }
        public bool BuffApplyEvent { get; set; }
        public bool BuffRemoveEvent { get; set; }
        public bool DebuffEvent { get; set; }
        public bool BuffStackEvent { get; set; }
        public bool BuffIncEvent { get; set; }
        public bool BuffRefreshEvent { get; set; }
        public void StoreRefStatChange(StatName statName, double amount, StatAmountType type, bool removal)
        {
            RefStatChanges.Add(new StatChange(statName, amount, type, removal));
        }
    }

    
}
