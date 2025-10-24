using Beaversims.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Sim.SpecialEffects
{
    internal class MementoOfTyrande : HasteProcEffect
    {
        public static readonly HashSet<string> Sources = ["Memento of Tyrande"];
        public double Amount { get; protected set; } = 0;
        public ScalingData ScalingData { get; protected set; }

        public override void Init(List<Event> events, User user, Fight fight)
        {
            Amount = ScUtils.ScaledEffectValue(Ilvl, ItemSlot, ScalingData); 
        }
        public override void Call(Event evt, User user, int i)
        {

            var timestamp = evt.Timestamp;
            if (Proc.IsProcAttempt(evt, ProcFlags, lastProc, icd, timestamp))
            {
                var isProc = Proc.ProcessProcAttempt(ref blp, ScUtils.TrueRppm(evt, Rppm, HasteScaling, i), ref lastAttempt, timestamp);
                if (isProc)
                {
                    user.AltGearSets[i].ManaGain += Amount / Constants.iterationCount;

                }

            }
        }
        public MementoOfTyrande() : base()
        {
            Name = "Memento of Tyrande";
            Rppm = 2.5;
            HasteScaling = true;
            ScalingData = new ScalingData(-7, 2.03251);
            ProcFlags.UnionWith([ProcFlag.HealOnly]);
        }
    }


    
    internal class TheSkullOfGuldan : OnUseEffect
    {
        public static readonly HashSet<string> Sources = ["The Skull of Gul'Dan"];
        public StatBuff Buff { get; }
        
        public override void Init(List<Event> events, User user, Fight fight)
        {
            StandardUseStatSim(events, Buff);
        }

        public TheSkullOfGuldan() : base()
        {
            Name = "The Skull of Gul'dan";
            Buff = new Data.StatBuffs.FelInfusion(new UnitId(0, 0), 1);
            Cd = 120;
        }
    }

    internal class ElementalFocusStone : StatProcEffect
    {
        public static readonly HashSet<string> Sources = ["Elemental Focus Stone"];
        public StatBuff Buff { get; }
        public Dictionary<int, double> Amount { get; protected set; } = [];

        public override void Reset(User user, Fight fight)
        {
            base.Reset(user, fight);
        }

        public override void Init(List<Event> events, User user, Fight fight)
        {
            StandardStatProcInit(Buff, Amount);
        }
        public override void Call(Event evt, User user)
        {
            StandardStatProcCall(evt, user, Buff, Amount);
        }
        public ElementalFocusStone() : base()
        {
            Name = "Elemental Focus Stone";
            Rppm = 1.5;
            HasteScaling = false;
            Buff = new Data.StatBuffs.AlacrityOfTheElements(new UnitId(0, 0), 1);
            ProcFlags.UnionWith([ProcFlag.DamageOnly, ProcFlag.SpellOnly]);
        }
    }

    internal class EnergySiphon : OnUseEffect
    {
        public static readonly HashSet<string> Sources = ["Energy Siphon"];
        public StatBuff Buff { get; }

        public override void Init(List<Event> events, User user, Fight fight)
        {
            StandardUseStatSim(events, Buff);
        }

        public EnergySiphon() : base()
        {
            Name = "Energy Siphon";
            Buff = new Data.StatBuffs.EnergySiphon(new UnitId(0, 0), 1);
            Cd = 120;
        }
    }

    internal class EyeOfTheBroodmother : StatProcEffect
    {
        public static readonly HashSet<string> Sources = ["Eye of the Broodmother"];
        public StatBuff Buff { get; }
        public Dictionary<int, double> Amount { get; protected set; } = [];
        public int CurStacks { get; protected set; } = 0;
        public StatName StatName { get; protected set; }

        public override void Reset(User user, Fight fight)
        {
            base.Reset(user, fight);
            CurStacks = 0;
        }

        public override void Init(List<Event> events, User user, Fight fight)
        {
            var statMod = Buff.StatMods[0];
            StatName = statMod.StatName;
            foreach (var gearSet in Ilvls)
            {
                var id = gearSet.Key;
                Amount[id] = ScUtils.ScaledEffectValue(gearSet.Value, ItemSlots[id], statMod.ScalingData, StatName);
            }
        }
        public override void Call(Event evt, User user)
        {
            var timestamp = evt.Timestamp;

            if (CurStacks > 0 && timestamp > buffEnd)
            {
                foreach (var id in Amount.Keys)
                {
                    user.AltGearSets[id].SimIncRatings[StatName] -= Amount[id] * CurStacks;
                    CurStacks = 0;
                }
            }

            if (Proc.IsProcAttempt(evt, ProcFlags, lastProc, icd, timestamp))
            {
                buffEnd = Buff.Duration + timestamp;
                if (CurStacks < Buff.MaxStacks)
                {
                    CurStacks++;
                    foreach (var id in Amount.Keys)
                    {
                        user.AltGearSets[id].SimIncRatings[StatName] += Amount[id];
                    }
                }

            }

        }
        public EyeOfTheBroodmother() : base()
        {
            Name = "Eye of the Broodmother";
            icd = 0.1;
            Buff = new Data.StatBuffs.EyeOfTheBroodmother(new UnitId(0, 0), 1);
            ProcFlags.UnionWith([ProcFlag.SpellOnly]);
        }
    }

    internal class FlareOfTheHeavens : StatProcEffect
    {
        public static readonly HashSet<string> Sources = ["Flare of the Heavens"];
        public StatBuff Buff { get; }
        public Dictionary<int, double> Amount { get; protected set; } = [];

        public override void Reset(User user, Fight fight)
        {
            base.Reset(user, fight);
        }

        public override void Init(List<Event> events, User user, Fight fight)
        {
            StandardStatProcInit(Buff, Amount);
        }
        public override void Call(Event evt, User user)
        {
            StandardStatProcCall(evt, user, Buff, Amount);
        }
        public FlareOfTheHeavens() : base()
        {
            Name = "Flare of the Heavens";
            Rppm = 1.25;
            HasteScaling = false;
            Buff = new Data.StatBuffs.FlameOfTheHeavens(new UnitId(0, 0), 1);
            ProcFlags.UnionWith([ProcFlag.DamageOnly, ProcFlag.SpellOnly]);
        }
    }

    internal class LivingFlame : OnUseEffect
    {
        public static readonly HashSet<string> Sources = ["Living Flame"];
        public StatBuff Buff { get; }

        public override void Init(List<Event> events, User user, Fight fight)
        {
            StandardUseStatSim(events, Buff);
        }

        public LivingFlame() : base()
        {
            Name = "Living Flame";
            Buff = new Data.StatBuffs.LivingFlame(new UnitId(0, 0), 1);
            Cd = 120;
        }
    }

    internal class PandorasPlea : StatProcEffect
    {
        public static readonly HashSet<string> Sources = ["Pandora's Plea"];
        public StatBuff Buff { get; }
        public Dictionary<int, double> Amount { get; protected set; } = [];

        public override void Init(List<Event> events, User user, Fight fight)
        {
            StandardStatProcInit(Buff, Amount);
        }
        public override void Call(Event evt, User user)
        {
            StandardStatProcCall(evt, user, Buff, Amount);
        }
        public PandorasPlea() : base()
        {
            Name = "Pandora's Plea";
            Rppm = 2;
            HasteScaling = false;
            Buff = new Data.StatBuffs.PandorasPlea(new UnitId(0, 0), 1);
            ProcFlags.UnionWith([ProcFlag.SpellOnly]);
        }
    }

    internal class ScaleOfFates : OnUseEffect
    {
        public static readonly HashSet<string> Sources = ["Scale of Fates"];
        public StatBuff Buff { get; }

        public override void Init(List<Event> events, User user, Fight fight)
        {
            StandardUseStatSim(events, Buff);
        }

        public ScaleOfFates() : base()
        {
            Name = "Scale of Fates";
            Buff = new Data.StatBuffs.ScaleOfFates(new UnitId(0, 0), 1);
            Cd = 120;
        }
    }

    internal class ShowOfFaith : StatProcEffect
    {
        public static readonly HashSet<string> Sources = ["Show of Faith"];
        public Dictionary<int, double> Amount { get; protected set; } = [];
        public ScalingData ScalingData { get; protected set; }

        public override void Init(List<Event> events, User user, Fight fight)
        {
            foreach (var gearSet in Ilvls)
            {
                var id = gearSet.Key;
                Amount[id] = ScUtils.ScaledEffectValue(gearSet.Value, ItemSlots[id], ScalingData);
            }
        }
        public override void Call(Event evt, User user)
        {
            var timestamp = evt.Timestamp;
            if (Proc.IsProcAttempt(evt, ProcFlags, lastProc, icd, timestamp))
            {
                var isProc = Proc.ProcessProcAttempt(ref blp, Rppm, ref lastAttempt, timestamp);
                if (isProc)
                {
                    foreach (var id in Amount.Keys)
                    {
                        user.AltGearSets[id].ManaGain += Amount[id] / Constants.iterationCount;
                    }
                }
            }
        }
        public ShowOfFaith() : base()
        {
            Name = "Show of Faith";
            Rppm = 2;
            HasteScaling = false;
            ScalingData = new ScalingData(-7, 1.595796);
            ProcFlags.UnionWith([ProcFlag.HealOnly]);
        }
    }

    internal class SifsRemembrance : StatProcEffect
    {
        public static readonly HashSet<string> Sources = ["Sif's Remembrance"];
        public StatBuff Buff { get; }
        public Dictionary<int, double> Amount { get; protected set; } = [];

        public override void Init(List<Event> events, User user, Fight fight)
        {
            StandardStatProcInit(Buff, Amount);
        }
        public override void Call(Event evt, User user)
        {
            StandardStatProcCall(evt, user, Buff, Amount);
        }
        public SifsRemembrance() : base()
        {
            Name = "Sif's Remembrance";
            Rppm = 2;
            HasteScaling = false;
            Buff = new Data.StatBuffs.PandorasPlea(new UnitId(0, 0), 1);
            ProcFlags.UnionWith([ProcFlag.SpellOnly, ProcFlag.HealOnly]);
        }
    }
}


