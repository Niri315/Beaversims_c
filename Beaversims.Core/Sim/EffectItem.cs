using Beaversims.Core.Common;
using Beaversims.Core.Sim;
using System;
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
      
        public Dictionary<int, int> Ilvls { get; } = []; //<GearSetId, ilvl>
        public Dictionary<int, ItemSlot> ItemSlots { get; } = []; //<GearSetId, ItemSlot>
        public Dictionary<int, double> ManaInc { get; } = [];


        //public bool DoNotRun { get; set; } = false;
       
        public abstract void Init(List<Event> events, User user, Fight fight);


    }


    internal abstract class  OnUseEffect : SpecialEffect
    {
        public double Cd { get; set; }

        public void StandardUseStatSim(List<Event> events, StatBuff buff)
        {
            Dictionary<int, double> amounts = [];
            var statMod = buff.StatMods[0];
            var statName = statMod.StatName;
            foreach (var gearSet in Ilvls)
            {
                var id = gearSet.Key;
                var ilvl = gearSet.Value;
                amounts[id] = ScUtils.ScaledEffectValue(ilvl, ItemSlots[id], statMod.ScalingData, statName);
            }
            var activeEvents = SimUtils.FetchTimingEvents(events, buff.Duration, Cd);
            foreach (var evt in activeEvents)
            {
                //Console.WriteLine(evt.Timestamp.ToString());
                foreach (var id in amounts.Keys)
                {
                    //Console.WriteLine($"{evt.Timestamp}: {evt.AltEvents[id].UserStats.Get(statName).Eff}");
                    evt.AltEvents[id].UserStats.Get(statName).ChangeAmount(amounts[id], StatAmountType.Rating, false);
                    //Console.WriteLine($"Post: {evt.AltEvents[id].UserStats.Get(statName).Eff}");
                }
            }
        }
    }

    internal abstract class ProcEffect : SpecialEffect
    {
        public double Rppm { get; set; }
        public bool HasteScaling { get; set; }
        public HashSet<ProcFlag> ProcFlags { get; set; } = [];
        public double blp = Proc.onPullBlp;
        public double lastAttempt = Proc.initLastAttempt;
        public double lastProc = -999;
        public bool active = false;
        public double buffEnd = 0;
        public double icd = 0;
        public virtual void Reset(User user, Fight fight)
        {
            blp = Proc.onPullBlp;
            lastAttempt = Proc.initLastAttempt;
            lastProc = -999;
           
        }
       
    }

    internal abstract class HasteProcEffect : ProcEffect 
    {
        public int Ilvl { get; set; } = 0;
        public ItemSlot ItemSlot { get; set; }
        public abstract void Call(Event evt, User user, int i);
    }

    internal abstract class NonHasteProcEffect : ProcEffect
    {
        public abstract void Call(Event evt, User user);
    }


    internal abstract class StatProcEffect : NonHasteProcEffect
    {
        public Dictionary<int, Dictionary<StatName, double>> RatingIncTracker { get; set; } = [];



        public override void Reset(User user, Fight fight)
        {
            base.Reset(user, fight);

            for (int x = 0; x < user.AltGearSets.Count; x++)
            {
                var gearSet = user.AltGearSets[x];
                // Resetting stat increases.
                gearSet.SimIncRatings = Utils.InitStatDict();
                gearSet.IncEffs = Utils.InitStatDict();
                active = false;
                buffEnd = 0;
            }
        }
        public void StandardStatProcInit(StatBuff buff, Dictionary<int, double> amount)
        {
            var statMod = buff.StatMods[0];
            var statName = statMod.StatName;
            foreach (var gearSet in Ilvls)
            {
                var id = gearSet.Key;
                var ilvl = gearSet.Value;
                //RatingIncTracker[id][statName] = 0.0;

                amount[id] = ScUtils.ScaledEffectValue(ilvl, ItemSlots[id], statMod.ScalingData, statName);
            }
        }
        public void StandardStatProcCall(Event evt, User user, StatBuff buff, Dictionary<int, double> amount)
        {
            var statName = buff.StatMods[0].StatName;
            if (buffEnd < evt.Timestamp && active)
            {
                foreach (var id in amount.Keys)
                {
                    //Console.WriteLine($"{evt.Timestamp} -Removing: {amount[id]}");
                    user.AltGearSets[id].SimIncRatings[statName] -= amount[id];
                }
                active = false;
            }

            if (Proc.IsProcAttempt(evt, ProcFlags, lastProc, icd, evt.Timestamp))
            {
                var isProc = Proc.ProcessProcAttempt(ref blp, Rppm, ref lastAttempt, evt.Timestamp);
                if (isProc)
                {
                    var duration = buff.Duration;
                    buffEnd = evt.Timestamp + duration;
                    if (active) { return; }
                    active = true;
                    foreach (var id in amount.Keys)
                    {
                        //Console.WriteLine($"{evt.Timestamp} - Adding: {amount[id]}");
                        user.AltGearSets[id].SimIncRatings[statName] += amount[id];
                    }
                }
            }
        }
 
        public void StoreRatingChange(User user)
        {

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

        public static SpecialEffect? CreateFromName(string gearName)
        {
            foreach (var type in _effectTypes)
            {
                var sourcesField = type.GetField("Sources", BindingFlags.Public | BindingFlags.Static);
                if (sourcesField?.GetValue(null) is HashSet<string> sources && sources.Contains(gearName))
                {

                    var instance = (SpecialEffect?)Activator.CreateInstance(type);
                    return instance;
                }
            }
            return null;
        }
    }
}
