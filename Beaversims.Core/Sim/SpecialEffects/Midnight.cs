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

        public override void Call(List<TpEvent> procEvents, List<Event> events, Event evt, User user, StatTracker curAltStats, int i, int iterationCount)
        {
            var altGearSet = user.AltGearSets[i];

            if (Proc.IsProcAttempt(evt, ProcFlags, lastProc, icd, evt.Timestamp))
            {
                var isProc = Proc.ProcessProcAttempt(ref blp, ScUtils.TrueRppm(curAltStats, Rppm, HasteScaling, i), ref lastAttempt, ref lastProc, evt.Timestamp);
                if (isProc)
                {
                    var amount = Amount;
                    if (evt.TargetHp_p(preEvent: true) < healthPercent)
                    {
                        amount *= incStat_p;
                    }
                    var simStatBuff = Buff;
                    simStatBuff.StatMods[0].Amount = amount;
                    altGearSet.AddSimStatBuff(simStatBuff, evt.Timestamp);
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


    internal class ConsecratedChalice : ProcEffect
    {
        public static readonly HashSet<string> Sources = ["Consecrated Chalice"];
        public int stacks = 0;
        public const int stackCap = 15;
        public const int cd = 120;
        public double lastUse = -999;
        public override void Init(List<Event> events, User user, Fight fight, int i)
        {

            Amount = ScUtils.ScaledEffectValue(Ilvl, ItemSlot, ScalingData);

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
                    if (stacks < stackCap)
                    {
                        stacks += 1;
                    }
                }
            }

            if (evt.Timestamp - lastUse > cd && (evt.Timestamp > 60 || stacks == 15))  // First use, after than use on cd. Not perfect but cba.
            {

                lastUse = evt.Timestamp;
                SimUtils.AddSimHealEvent(events, user, Ability, evt.Timestamp + 0.1, Amount * stacks, true);
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

    internal class LocusWalkersRibbon : ProcEffect
    {
        public static readonly HashSet<string> Sources = ["Locus-Walker's Ribbon"];
        private int stacks = 0;
        private const int stackCap = 10;
        private const double incMod = 0.05;


        public override void Init(List<Event> events, User user, Fight fight, int i)
        {
            var statMod = Buff.StatMods[0];
            StatName = statMod.StatName;
            Duration = Buff.Duration;
            ScalingData = statMod.ScalingData;
            Amount = ScUtils.ScaledEffectValue(Ilvl, ItemSlot, ScalingData, StatName);

        }
        public override void Reset()
        {
            base.Reset();
            stacks = 0;
        }

        public override void Call(List<TpEvent> procEvents, List<Event> events, Event evt, User user, StatTracker curAltStats, int i, int iterationCount)
        {
            var altGearSet = user.AltGearSets[i];


            if (Proc.IsProcAttempt(evt, ProcFlags, lastProc, icd, evt.Timestamp))
            {
                var isProc = Proc.ProcessProcAttempt(ref blp, ScUtils.TrueRppm(curAltStats, Rppm, HasteScaling, i), ref lastAttempt, ref lastProc, evt.Timestamp);
                if (isProc)
                {



                    var simStatBuff = new Beaversims.Core.Data.StatBuffs.LocusWalkersRibbon(new UnitId(0, 0), 1);

                    simStatBuff.StatMods[0].Amount = Amount * (1 + (stacks * incMod));
                    altGearSet.AddSimStatBuff(simStatBuff, evt.Timestamp);

                    if (stacks < stackCap)
                    {
                        stacks += 1;
                    }

                }
            }

        }

        public LocusWalkersRibbon() : base()
        {
            Name = "Locus-Walker's Ribbon";
            Rppm = 2.5;
            Buff = new Beaversims.Core.Data.StatBuffs.LocusWalkersRibbon(new UnitId(0, 0), 1);
            ProcFlags.UnionWith([PF.SpellOnly]);
        }
    }

    internal class HeartOfWind : SimpleProcStatEffect
    {
        public static readonly HashSet<string> Sources = ["Heart of Wind"];

        public HeartOfWind() : base()
        {
            Name = "Heart of Wind";
            Rppm = 3;
            HasteScaling = false;
            Buff = new Beaversims.Core.Data.StatBuffs.TheWindAwoken(new UnitId(0, 0), 1);
        }
    }

    internal class VolatileVoidSuffuser : SimpleProcStatEffect
    {
        // Looks like 1 % of scaled amount per 1% missing hp.

        public static readonly HashSet<string> Sources = ["Volatile Void Suffuser"];


        public override void Call(List<TpEvent> procEvents, List<Event> events, Event evt, User user, StatTracker curAltStats, int i, int iterationCount)
        {
            var altGearSet = user.AltGearSets[i];


            if (Proc.IsProcAttempt(evt, ProcFlags, lastProc, icd, evt.Timestamp))
            {
                var isProc = Proc.ProcessProcAttempt(ref blp, ScUtils.TrueRppm(curAltStats, Rppm, HasteScaling, i), ref lastAttempt, ref lastProc, evt.Timestamp);
                if (isProc)
                {
                    lastProc = evt.Timestamp;
                    var amount = Amount;
                    var targetHp_p = evt.TargetHp_p(preEvent: true);
                    if (targetHp_p is double d)
                    {
                        double percentMissing = 100 * (1 - d);
                        amount += percentMissing * (Amount / 100);
                    }

                    var simStatBuff = new Data.StatBuffs.VoidSuffusion(new UnitId(0, 0), 1);
                    simStatBuff.StatMods[0].Amount = amount;
                    altGearSet.AddSimStatBuff(simStatBuff, evt.Timestamp);
                }
            }
        }
        public VolatileVoidSuffuser() : base()
        {
            Name = "Volatile Void Suffuser";
            Rppm = 2.5;
            HasteScaling = false;
            Buff = new Data.StatBuffs.VoidSuffusion(new UnitId(0, 0), 1);
            ProcFlags.UnionWith([PF.HealOnly]);
        }
    }


    internal class VesselOfSouls : SimpleProcStatEffect
    {
        public static readonly HashSet<string> Sources = ["Vessel of Tortured Souls"];
        public int testCount = 0;

        public override void Call(List<TpEvent> procEvents, List<Event> events, Event evt, User user, StatTracker curAltStats, int i, int iterationCount)
        {
            //base.Call(procEvents, events, evt, user, curAltStats, i, iterationCount);
            testCount++;
            if (testCount % 30 == 0)
            {
                var gearSet = user.AltGearSets[i];
            }
            if (Proc.IsProcAttempt(evt, ProcFlags, lastProc, icd, evt.Timestamp))
            {
                var isProc = Proc.ProcessProcAttempt(ref blp, ScUtils.TrueRppm(curAltStats, Rppm, HasteScaling, i), ref lastAttempt, ref lastProc, evt.Timestamp);
                if (isProc)
                {
                    var buff = new Beaversims.Core.Data.StatBuffs.ARestlessSoul(new UnitId(0, 0), 1);
                    buff.StatMods[0].Amount = Amount;
                    user.AltGearSets[i].AddSimStatBuff(buff, evt.Timestamp);
                }
            }


        }

        public VesselOfSouls() : base()
        {
            Name = "Vessel of Souls";
            Rppm = 3;
            HasteScaling = false;
            Buff = new Beaversims.Core.Data.StatBuffs.ARestlessSoul(new UnitId(0, 0), 1);
            //ProcFlags.UnionWith([]);
        }
    }

    internal class GazeOfTheAlnseer : SimpleProcStatEffect
    {
        public static readonly HashSet<string> Sources = ["Gaze of the Alnseer"];
        public int testCount = 0;
        private double activeEnd = 0;
        private const double activeDur = 12;
        private double lastProc2 = -999;
        private const double icd2 = 0.75;
        private HashSet<ProcFlag> AlnsightProcFlags { get; set; } = new HashSet<ProcFlag>();

        public override void Reset()
        {
            base.Reset();
            active = false;
            activeEnd = 0;
            lastProc2 = -999;
        }

        public override void Call(List<TpEvent> procEvents, List<Event> events, Event evt, User user, StatTracker curAltStats, int i, int iterationCount)
        {
            //base.Call(procEvents, events, evt, user, curAltStats, i, iterationCount);
            testCount++;
            if (testCount % 30 == 0)
            {
                var gearSet = user.AltGearSets[i];
                //Console.WriteLine($"{evt.Timestamp}. {gearSet.SimIncRatings[StatName.Intellect]}");
            }
            if (Proc.IsProcAttempt(evt, ProcFlags, lastProc, icd, evt.Timestamp))
            {
                var isProc = Proc.ProcessProcAttempt(ref blp, ScUtils.TrueRppm(curAltStats, Rppm, HasteScaling, i), ref lastAttempt, ref lastProc, evt.Timestamp);
                if (isProc)
                {
                    //Console.WriteLine("Gaze Proc");
                    activeEnd = evt.Timestamp + activeDur;

                }
            }

            if (activeEnd > evt.Timestamp && Proc.IsProcAttempt(evt, AlnsightProcFlags, lastProc2, icd2, evt.Timestamp))
            {
                //Console.WriteLine(evt.Timestamp);
                lastProc2 = evt.Timestamp;
                var buff = new Beaversims.Core.Data.StatBuffs.AlnscornedEssence(new UnitId(0, 0), 1);
                buff.StatMods[0].Amount = Amount;
                user.AltGearSets[i].AddSimStatBuff(buff, evt.Timestamp);
            }


        }

        public GazeOfTheAlnseer() : base()
        {
            Name = "Gaze of the Alnseer";
            Rppm = 2;
            HasteScaling = false;
            Buff = new Beaversims.Core.Data.StatBuffs.AlnscornedEssence(new UnitId(0, 0), 1);
            AlnsightProcFlags.UnionWith([PF.NoMelee]);
            //ProcFlags.UnionWith([]);
        }
    }

    internal class SealedChaosUrn : SimpleOnUseStatEffect
    {
        public static readonly HashSet<string> Sources = ["Sealed Chaos Urn"];
        public SealedChaosUrn() : base()
        {
            Name = "Sealed Chaos Urn";
            Buff = new Data.StatBuffs.SealedChaosUrn(new UnitId(0, 0), 1);
            Cd = 120;
            Priority = 3;
        }
    }
    internal class DrumOfRenewedBonds : SimpleProcStatEffect
    {
        public static readonly HashSet<string> Sources = ["Drum of Renewed Bonds"];

        public override void Init(List<Event> events, User user, Fight fight, int i)
        {
            switch (user.HighestPullStat)
            {
                case StatName.Crit:
                    {
                        Buff = new Data.StatBuffs.AkilzonsClarity(new UnitId(0, 0), 1);
                        break;
                    }

                case StatName.Haste:
                    {
                        Buff = new Data.StatBuffs.HalazzisSwiftness(new UnitId(0, 0), 1);
                        break;
                    }

                case StatName.Mastery:
                    {
                        Buff = new Data.StatBuffs.JanalaisWarmth(new UnitId(0, 0), 1);
                        break;
                    }

                case StatName.Vers:
                    {
                        Buff = new Data.StatBuffs.NalorakksResolve(new UnitId(0, 0), 1);
                        break;
                    }

            }
            base.Init(events, user, fight, i);
        }
        public DrumOfRenewedBonds() : base()
        {
            Name = "Drum of Renewed Bonds";
            Rppm = 1.5;
            HasteScaling = false;
        }
    }

    internal class GladiatorsBadge : SimpleOnUseStatEffect
    {
        public static readonly HashSet<string> Sources = ["Galactic Gladiator's Badge of Ferocity"];
        public GladiatorsBadge() : base()
        {
            Name = "Gladiator's Badge";
            Buff = new Data.StatBuffs.GladiatorsBadge(new UnitId(0, 0), 1);
            Cd = 60;
            Priority = 4;
        }
    }

    internal class GladiatorsInsignia : SimpleProcStatEffect
    {
        public static readonly HashSet<string> Sources = ["Galactic Gladiator's Insignia of Alacrity"];
        public GladiatorsInsignia() : base()
        {
            Name = "Gladiator's Insignia";
            Rppm = 1.5;
            HasteScaling = false;
            Buff = new Data.StatBuffs.GladiatorsInsignia(new UnitId(0, 0), 1);
        }
    }
    internal class CosmicBell : SimpleOnUseHealEffect
    {
        public static readonly HashSet<string> Sources = ["Cosmic Bell"];
        public CosmicBell() : base()
        {
            Priority = 7;
            Name = "Cosmic Bell";
            Ability = new Shared.Abilities.CosmicBell();
            MaxRaidHp_p = 0.9;
            Cd = 150;
            
        }
    }
    internal class UltradonCuirass : SimpleOnUseHealEffect
    {
        public static readonly HashSet<string> Sources = ["Ultradon Cuirass"];
        public UltradonCuirass() : base()
        {
            Priority = 7;
            Name = "Ultradon Cuirass";
            Ability = new Shared.Abilities.UltradonCuirass();
            MaxRaidHp_p = 0.9;
            Cd = 150;

        }
    }

    internal class MycolicMedicine : ProcEffect
    {
        public static readonly HashSet<string> Sources = ["Mycolic Medicine"];
        private Ability MushroomAbility {  get; set; }
        private double MushroomAmount { get; set; }
        public double SuccessRate { get; set; } = 0.9; //PFA
        
        public override void Init(List<Event> events, User user, Fight fight, int i)
        {

            Amount = ScUtils.ScaledEffectValue(Ilvl, ItemSlot, Ability.ScalingData);
            MushroomAmount = ScUtils.ScaledEffectValue(Ilvl, ItemSlot, MushroomAbility.ScalingData);

        }

        public override void Call(List<TpEvent> procEvents, List<Event> events, Event evt, User user, StatTracker curAltStats, int i, int iterationCount)
        {
            if (Proc.IsProcAttempt(evt, ProcFlags, lastProc, icd, evt.Timestamp))
            {
                var isProc = Proc.ProcessProcAttempt(ref blp, ScUtils.TrueRppm(curAltStats, Rppm, HasteScaling, i), ref lastAttempt, ref lastProc, evt.Timestamp);
                if (isProc)
                {
                    SimUtils.AddSimHealEvent(events, user, Ability, evt.Timestamp + 0.1, Amount, true);
                    SimUtils.AddSimHealEvent(events, user, MushroomAbility, evt.Timestamp + 1, MushroomAmount * SuccessRate, true);
                }
            }
        }

        public MycolicMedicine() : base()
        {
            Name = "Mycolic Medicine";
            Rppm = 3;
            HasteScaling = true;
            Ability = new Beaversims.Core.Shared.Abilities.MycolicMedicine { SimImpurity = false };
            MushroomAbility = new Beaversims.Core.Shared.Abilities.GlowcapMushroomsRejuvenation { SimImpurity = false };
            ProcFlags.UnionWith([PF.HealOnly]);
        }
    }

    internal class UnstableFelheartCrystal : SimpleOnUseHealEffect
    {

        public static readonly HashSet<string> Sources = ["Unstable Felheart Crystal"];
        private ScalingData SelfDmgScalingData {  get; set; }
        

        public override void Run(List<Event> events, User user, Fight fight, int i)
        {

            Amount = ScUtils.ScaledEffectValue(Ilvl, ItemSlot, Ability.ScalingData);
            var selfDmgAmount = ScUtils.ScaledEffectValue(Ilvl, ItemSlot, SelfDmgScalingData);

            foreach (var useTiming in UseTimings)
            {
                SimUtils.AddSimHealEvent(events, user, Ability, useTiming, Amount, false);
                SimUtils.AddSimDmgEvent(events, user, Ability, useTiming, selfDmgAmount, false, dmgTaken:true);
                Console.WriteLine($"{Name}: use Timing: {useTiming}, adding amount: {Amount}");
            }
        }
        public UnstableFelheartCrystal() : base()
        {
            Priority = 7;
            Name = "Unstable Felheart Crystal";
            Ability = new Shared.Abilities.UnstableFelheartCrystal();
            SelfDmgScalingData = new ScalingData(-9, 0.375845);
            MaxRaidHp_p = 0.95;
            Cd = 120;

        }
    }

    internal class EyeOfTheDrowningVoid : SimpleProcDmgEffect
    {
        public static readonly HashSet<string> Sources = ["Eye of the Drowning Void"];
        public EyeOfTheDrowningVoid() : base()
        {
            Name = "Eye of the Drowning Void";
            Rppm = 2;
            HasteScaling = true;
            Ability = new Shared.Abilities.EyeOfTheDrowningVoid();
        }
    }
}

