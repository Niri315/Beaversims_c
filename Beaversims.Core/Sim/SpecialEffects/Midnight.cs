using Beaversims.Core;
using Beaversims.Core.Common;
using Beaversims.Core.Sim;

using PF = Beaversims.Core.Sim.ProcFlag;

namespace Beaversims.Core.Sim.SpecialEffects.Midnight
{
    internal class GiftOfLight : SimpleProcStatEffect
    {
        public static readonly HashSet<string> Sources = ["Gift of Light"];
        public const double healthPercent = 0.4;
        public const double incStat_p = 2.0;
        public bool empBuff = false;

        public override void Init(List<Event> events, User user, Fight fight, int i)
        {
            var statMod = Buff.StatMods[0];
            StatName = statMod.StatName;
            Duration = Buff.Duration;
            ScalingData = statMod.ScalingData;
            Amount = ScUtils.ScaledEffectValue(Ilvl, ItemSlot, ScalingData, StatName);

        }
        public override void Call(List<TpEvent> procEvents, List<Event> events, Event evt, User user, StatTracker curAltStats, int i, int iterationCount)
        {
            var altGearSet = user.AltGearSets[i];
            if (buffEnd < evt.Timestamp && active)
            {
                var amount = Amount;
                if (empBuff)
                {
                    amount *= incStat_p;
                }
                altGearSet.SimIncRatings[StatName] -= amount;
                active = false;
            }

            if (Proc.IsProcAttempt(evt, ProcFlags, lastProc, icd, evt.Timestamp))
            {
                var isProc = Proc.ProcessProcAttempt(ref blp, ScUtils.TrueRppm(curAltStats, Rppm, HasteScaling, i), ref lastAttempt, ref lastProc, evt.Timestamp);
                if (isProc)
                {
                    var amount = Amount;
                    if (evt.TargetHp_p(preEvent:true) < healthPercent)
                    {
                        empBuff = true;
                        amount *= incStat_p;
                    }
                    else
                    {
                        empBuff = false;
                    }
                    buffEnd = evt.Timestamp + Duration;
                    if (active) { return; }
                    active = true;
                    altGearSet.SimIncRatings[StatName] += amount;
                }
            }
        }

        public GiftOfLight() : base()
        {
            Name = "Gift of Light";
            Rppm = 2;
            icd = 60;  // todo make sure its not just for the special effect (if 40%)
            HasteScaling = false;
            Buff = new Data.StatBuffs.GiftOfLight(new UnitId(0, 0), 1);
            ProcFlags.UnionWith([PF.HealOnly]);
        }
    }
}

internal class ConsecratedChalice : ProcEffect
{
    public static readonly HashSet<string> Sources = ["Consecrated Chalice"];
    public int stacks = 0;
    public const int stackCap = 15;
    public const int cd = 120;
    public double lastUse = -999;
    public override void Init(List<Event> events, User user, Fight fight, int i)
    {

        Amount = ScUtils.ScaledEffectValue(Ilvl, ItemSlot, ScalingData, StatName);

    }
    public override void Reset()
    {
        base.Reset();
        stacks = 0;
        lastUse = -999;
    }

    public override void Call(List<TpEvent> procEvents, List<Event> events, Event evt, User user, StatTracker curAltStats, int i, int iterationCount)
    {
        if (Proc.IsProcAttempt(evt, ProcFlags, lastProc, icd, evt.Timestamp))
        {
            var isProc = Proc.ProcessProcAttempt(ref blp, ScUtils.TrueRppm(curAltStats, Rppm, HasteScaling, i), ref lastAttempt, ref lastProc, evt.Timestamp);
            if (isProc)
            {
                lastProc = evt.Timestamp;
                if (stacks < stackCap)
                {
                    stacks += 1;
                }
            }
        }

        if (evt.Timestamp - lastUse > cd && (evt.Timestamp > 60 || stacks == 15))  // First use, after than use on cd. Not perfect but cba.
        {
            Console.WriteLine(evt.Timestamp);
            lastUse = evt.Timestamp;
            var newEvent = new SimHealEvent
            {
                Timestamp = evt.Timestamp + 0.1,
                SimProcSource = true,
                SimEvent = true,
                Ability = Ability,
                AbilityName = Name,
                SourceUnit = user,
                AbsorbAbility = true,
                FullyAbsorbed = true,
            };

            var amountRaw = Amount * stacks;
            var amountEff = amountRaw * DefaultUhr;

            newEvent.Amount.Raw = amountRaw;
            newEvent.Amount.Naraw = amountRaw;
            newEvent.Amount.Eff = amountEff;
            newEvent.Amount.Naeff = amountEff;

            int insertIndex = events.FindIndex(e => e.Timestamp > newEvent.Timestamp);
            if (insertIndex != -1)
            {
                events.Insert(insertIndex, newEvent);
            }

        }


    }

    public ConsecratedChalice() : base()
    {
        Name = "Consecrated Chalice";
        Rppm = 5;
        HasteScaling = true;
        Ability = new Beaversims.Core.Shared.Abilities.ConsecratedChalice { SimImpurity = false };
        ProcFlags.UnionWith([PF.HealOnly]);
        DefaultUhr = 0.9; // PFA
        ScalingData = new ScalingData(-9, 22.34271);
    }
}