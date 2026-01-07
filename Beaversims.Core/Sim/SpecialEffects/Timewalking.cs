using Beaversims.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PF = Beaversims.Core.Sim.ProcFlag;

namespace Beaversims.Core.Sim.SpecialEffects
{
    internal class MementoOfTyrande : ManaProcEffect
    {
        public static readonly HashSet<string> Sources = ["Memento of Tyrande"];

        public MementoOfTyrande() : base()
        {
            Name = "Memento of Tyrande";
            Rppm = 2.5;
            HasteScaling = true;
            ScalingData = new ScalingData(-7, 2.03251);
            ProcFlags.UnionWith([PF.HealOnly, PF.ClassAbilityOnly]);
        }
    }


    
    internal class TheSkullOfGuldan : SimpleOnUseStatEffect
    {
        public static readonly HashSet<string> Sources = ["The Skull of Gul'Dan"];
        public TheSkullOfGuldan() : base()
        {
            Name = "The Skull of Gul'dan";
            Buff = new Data.StatBuffs.FelInfusion(new UnitId(0, 0), 1);
            Cd = 120;
            Priority = 5;
        }
    }

    internal class AlacrityOfTheElements : SimpleProcStatEffect
    {
        public static readonly HashSet<string> Sources = ["Elemental Focus Stone"];
        public AlacrityOfTheElements() : base()
        {
            Name = "Alacrity of the Elements";
            Rppm = 1.5;
            HasteScaling = false;
            Buff = new Data.StatBuffs.AlacrityOfTheElements(new UnitId(0, 0), 1);
            ProcFlags.UnionWith([PF.DamageOnly, PF.SpellOnly, PF.ClassAbilityOnly]);
        }
    }

    internal class EnergySiphon : SimpleOnUseStatEffect
    {
        public static readonly HashSet<string> Sources = ["Energy Siphon"];

        public EnergySiphon() : base()
        {
            Name = "Energy Siphon";
            Buff = new Data.StatBuffs.EnergySiphon(new UnitId(0, 0), 1);
            Cd = 120;
            Priority = 5;
        }
    }

    internal class EyeOfTheBroodmother : NoRppmStatStackEffect
    {
        public static readonly HashSet<string> Sources = ["Eye of the Broodmother"];
        public EyeOfTheBroodmother() : base()
        {
            Name = "Eye of the Broodmother";
            icd = 0.1;
            Buff = new Data.StatBuffs.EyeOfTheBroodmother(new UnitId(0, 0), 1);
            ProcFlags.UnionWith([PF.SpellOnly, PF.ClassAbilityOnly]);
        }
    }

    internal class FlameOfTheHeavens : SimpleProcStatEffect
    {
        public static readonly HashSet<string> Sources = ["Flare of the Heavens"];

        public FlameOfTheHeavens() : base()
        {
            Name = "Flame of the Heavens";
            Rppm = 1.25;
            HasteScaling = false;
            Buff = new Data.StatBuffs.FlameOfTheHeavens(new UnitId(0, 0), 1);
            ProcFlags.UnionWith([PF.DamageOnly, PF.SpellOnly, PF.ClassAbilityOnly]);
        }
    }

    internal class LivingFlame : SimpleOnUseStatEffect
    {
        public static readonly HashSet<string> Sources = ["Living Flame"];

        public LivingFlame() : base()
        {
            Name = "Living Flame";
            Buff = new Data.StatBuffs.LivingFlame(new UnitId(0, 0), 1);
            Cd = 120;
            Priority = 5;
        }
    }

    internal class PandorasPlea : SimpleProcStatEffect
    {
        public static readonly HashSet<string> Sources = ["Pandora's Plea"];

        public PandorasPlea() : base()
        {
            Name = "Pandora's Plea";
            Rppm = 2;
            HasteScaling = false;
            Buff = new Data.StatBuffs.PandorasPlea(new UnitId(0, 0), 1);
            ProcFlags.UnionWith([PF.SpellOnly, PF.ClassAbilityOnly]);
        }
    }

    internal class ScaleOfFates : SimpleOnUseStatEffect
    {
        public static readonly HashSet<string> Sources = ["Scale of Fates"];

        public ScaleOfFates() : base()
        {
            Name = "Scale of Fates";
            Buff = new Data.StatBuffs.ScaleOfFates(new UnitId(0, 0), 1);
            Cd = 120;
            Priority = 5;
        }
    }

    internal class ShowOfFaith : ManaProcEffect
    {
        public static readonly HashSet<string> Sources = ["Show of Faith"];

        public ShowOfFaith() : base()
        {
            Name = "Show of Faith";
            Rppm = 2;
            HasteScaling = false;
            ScalingData = new ScalingData(-7, 1.595796);
            ProcFlags.UnionWith([PF.HealOnly, PF.ClassAbilityOnly]);
        }
    }

    internal class SifsRemembrance : SimpleProcStatEffect
    {
        public static readonly HashSet<string> Sources = ["Sif's Remembrance"];

        public SifsRemembrance()
        {
            Name = "Sif's Remembrance";
            Rppm = 2;
            HasteScaling = false;
            Buff = new Data.StatBuffs.PandorasPlea(new UnitId(0, 0), 1);
            ProcFlags.UnionWith([PF.SpellOnly, PF.HealOnly, PF.ClassAbilityOnly]);
        }
    }

    internal class EyeOfBlazingPower : ProcEffect
    {
        public static readonly HashSet<string> Sources = ["Eye of Blazing Power"];
        private static readonly Random random = new();
        public const double defaultUhr = 0.9; //PFA replace

        public override void Init(List<Event> events, User user, Fight fight, int i)
        {
            Amount = ScUtils.ScaledEffectValue(Ilvl, ItemSlot, ScalingData);
        }
        public override void Call(List<TpEvent> procEvents, List<Event> events, Event evt, User user, StatTracker curAltStats, int i, int iterationCount)
        {
            if (Proc.IsProcAttempt(evt, ProcFlags, lastProc, icd, evt.Timestamp))
            {
                if (random.NextDouble() < ProcChance)
                {
                    lastProc = evt.Timestamp;
                    var newEvent = new SimHealEvent
                    {
                        Timestamp = evt.Timestamp + 0.1,
                        SimProcSource = true,
                        SimEvent = true,
                        Proc = true,
                        Ability = new Shared.Abilities.BlazeOfLife { SimImpurity = false },
                        AbilityName = Name,
                        SourceUnit = user,
                    };
                    var amountRaw = Amount;
                    var amountEff = amountRaw * defaultUhr;
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
        }

        public EyeOfBlazingPower()
        {
            Name = "Eye of Blazing Power";
            icd = 45;
            ProcChance = 0.1;
            ProcFlags.UnionWith([PF.HealOnly, PF.ClassAbilityOnly]);
            ScalingData = new ScalingData(-9, 118.3393); // TODO Delta 0.15
        }
    }
    internal class NecromanticFocus : NoRppmStatStackEffect
    {
        public static readonly HashSet<string> Sources = ["Necromantic Focus"];
        public NecromanticFocus() : base()
        {
            Name = "Necromantic Focus";
            Buff = new Data.StatBuffs.SoulFragment(new UnitId(0, 0), 1);
            ProcFlags.UnionWith([PF.SpellOnly, PF.ClassAbilityOnly, PF.TickOnly]);
        }
    }
}


