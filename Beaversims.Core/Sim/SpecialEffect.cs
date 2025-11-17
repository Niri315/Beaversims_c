using Beaversims.Core.Common;
using Beaversims.Core.Sim;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{

    internal class ScalingData
    {
        public double Coef { get; set; }
        public int Class { get; set; }
        public ScalingData(int _class, double coef)
        {
            Class = _class;
            Coef = coef;
        }
    }



    internal abstract class SpecialEffect
    {
        public string Name { get; set; }

        public int Ilvl { get; set; } 
        public ItemSlot ItemSlot { get; set; }
        public double ManaInc { get; set; } = 0;

        public StatBuff Buff { get; protected set; }
        public double Amount { get; protected set; }
        public StatName StatName { get; protected set; }
        public double Duration { get; protected set; }
        public ScalingData ScalingData { get; protected set; }


        //public bool DoNotRun { get; set; } = false;



    }


    internal abstract class  OnUseEffect : SpecialEffect
    {
        public double Cd { get; set; }
        public const double sharedCd = 20;
        public List<double> UseTimings { get; protected set; }
        public int Priority { get; protected set; }  // Only for choosing the use timing events, not priority for pressing.
        public abstract List<Event> FindUseTimings(List<Event> events, User user, Fight fight, int i);
        public abstract void Run(List<Event> events, User user, Fight fight, int i);
    }

    internal abstract class SimpleOnUseStatEffect : OnUseEffect
    {
        public override List<Event> FindUseTimings(List<Event> events, User user, Fight fight, int i)
        {
            UseTimings = SimUtils.UseTimingsCalc(events, Buff.Duration, Cd, i);
            return SimUtils.AvailableUseEvents(events, UseTimings, sharedCd);
        }

        public override void Run(List<Event> events, User user, Fight fight, int i)
        {
            double amount;
            var statMod = Buff.StatMods[0];
            var statName = statMod.StatName;

            amount = ScUtils.ScaledEffectValue(Ilvl, ItemSlot, statMod.ScalingData, statName);
         
            var activeEvents = SimUtils.FetchTimingEvents(UseTimings, events, Buff.Duration, Cd, i);
            foreach (var evt in activeEvents)
            {
                //Console.WriteLine($"{evt.Timestamp}: {evt.AltEvents[id].UserStats.Get(statName).Eff}");
                evt.AltEvents[i].UserStats.Get(statName).ChangeAmount(amount, StatAmountType.Rating, false);
                //evt.AltEvents[i].UserStats.Get(statName).ChangeAmount(amount, StatAmountType.Rating, false);
                //Console.WriteLine($"Post: {evt.AltEvents[id].UserStats.Get(statName).Eff}");
            }
        }
    }

    internal abstract class ProcEffect : SpecialEffect
    {
        public double Rppm { get; set; }
        public bool HasteScaling { get; set; }
        public double ProcChance { get; set; }
        public HashSet<ProcFlag> ProcFlags { get; set; } = [];
        public double blp = Proc.onPullBlp;
        public double lastAttempt = Proc.initLastAttempt;
        public double lastProc = -999;
        public bool active = false;
        public double buffEnd = 0;
        public double icd = 0;


        public abstract void Init(List<Event> events, User user, Fight fight, int i);
        public abstract void Call(List<TpEvent> procEvents, List<Event> events, Event evt, User user, StatTracker curAltStats, int i, int iterationCount);

        //public abstract void Call(Event evt, User user, int i);
        public virtual void Reset()
        {
            blp = Proc.onPullBlp;
            lastAttempt = Proc.initLastAttempt;
            lastProc = -999;
            active = false;
            buffEnd = 0;
        }
    }



    internal abstract class SimpleProcStatEffect : ProcEffect
    {
        //public abstract void Call(Event evt, User user, int i);
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
            if (buffEnd < evt.Timestamp && active)
            {
                user.AltGearSets[i].SimIncRatings[StatName] -= Amount;
                active = false;
            }

            if (Proc.IsProcAttempt(evt, ProcFlags, lastProc, icd, evt.Timestamp))
            {
                var isProc = Proc.ProcessProcAttempt(ref blp, ScUtils.TrueRppm(curAltStats, Rppm, HasteScaling, i), ref lastAttempt, ref lastProc, evt.Timestamp);
                if (isProc)
                {
                    buffEnd = evt.Timestamp + Duration;
                    if (active) { return; }
                    active = true;
                    user.AltGearSets[i].SimIncRatings[StatName] += Amount;
                }
            }
        }
    }

    internal abstract class NoRppmStatStackEffect : ProcEffect
    {
        public int CurStacks { get; protected set; } = 0;

        public override void Reset()
        {
            base.Reset();
            CurStacks = 0;
        }

        public override void Init(List<Event> events, User user, Fight fight, int i)
        {
            var statMod = Buff.StatMods[0];
            StatName = statMod.StatName;
            Amount = ScUtils.ScaledEffectValue(Ilvl, ItemSlot, statMod.ScalingData, StatName);
        }
        public override void Call(List<TpEvent> procEvents, List<Event> events, Event evt, User user, StatTracker curAltStats, int i, int iterationCount)
        {
            var timestamp = evt.Timestamp;

            if (CurStacks > 0 && timestamp > buffEnd)
            {
                user.AltGearSets[i].SimIncRatings[StatName] -= Amount * CurStacks;
                CurStacks = 0;
            }

            if (Proc.IsProcAttempt(evt, ProcFlags, lastProc, icd, timestamp))
            {
                lastProc = timestamp;
                buffEnd = Buff.Duration + timestamp;
                if (CurStacks < Buff.MaxStacks)
                {
                    CurStacks++;
                    user.AltGearSets[i].SimIncRatings[StatName] += Amount;
                }
            }
        }
    }

    internal class ManaProcEffect : ProcEffect
    {
        public override void Init(List<Event> events, User user, Fight fight, int i)
        {
            Amount = ScUtils.ScaledEffectValue(Ilvl, ItemSlot, ScalingData);
        }
        public override void Call(List<TpEvent> procEvents, List<Event> events, Event evt, User user, StatTracker curAltStats, int i, int iterationCount)
        {
            var timestamp = evt.Timestamp;
            if (Proc.IsProcAttempt(evt, ProcFlags, lastProc, icd, timestamp))
            {
                var isProc = Proc.ProcessProcAttempt(ref blp, ScUtils.TrueRppm(curAltStats, Rppm, HasteScaling, i), ref lastAttempt, ref lastProc, timestamp);
                if (isProc)
                {
                    user.AltGearSets[i].ManaGain += Amount / iterationCount;
                }
            }
        }
    }

    internal static class SpecialEffectFactory
    {
        private static readonly List<Type> _effectTypes;

        static SpecialEffectFactory()
        {
            _effectTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(SpecialEffect)) && !t.IsAbstract)
                .ToList();
        }

        public static SpecialEffect? CreateFromName(string gearName, int ilvl, ItemSlot itemSlot)
        {
            foreach (var type in _effectTypes)
            {
                var sourcesField = type.GetField("Sources", BindingFlags.Public | BindingFlags.Static);
                if (sourcesField?.GetValue(null) is HashSet<string> sources && sources.Contains(gearName))
                {

                    var instance = (SpecialEffect?)Activator.CreateInstance(type);
                    instance.Ilvl = ilvl;
                    instance.ItemSlot = itemSlot;
                    return instance;
                }
            }
            return null;
        }
    }
}
