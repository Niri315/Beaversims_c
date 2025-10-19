using Beaversims.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Sim.SpecialEffects
{
    internal class AstralAntenna : SpecialEffect
    {
        public static readonly HashSet<string> Sources = ["Astral Antenna"];
        public Data.StatBuffs.AstralAntenna Buff { get; }
        public List<double> buffEnds = [];
        public StatName statName { get; protected set; }
        public Dictionary<int, double> Amounts { get; protected set; } = [];

        private static readonly Random random = new Random();
        public const double successRate = 0.5;

        //public const double Delay = 2;


        public override void Reset(User user, Fight fight)
        {
            base.Reset(user, fight);

            buffEnds = new List<double>();
        }

        public override void Init(List<Event> events, User user, Fight fight)
        {
            var statMod = Buff.StatMods[0];
            statName = statMod.StatName;
            foreach (var gearSet in Ilvls)
            {
                var id = gearSet.Key;
                var ilvl = gearSet.Value;
                RatingInc[id][statName] = 0.0;

                Amounts[id] = ScUtils.ScaledEffectValue(ilvl, ItemSlots[id], statName, statMod.ScalingData);
            }
        }
        public override void Call(Event evt, User user)
        {
            int expired = buffEnds.RemoveAll(end => end <= evt.Timestamp);

            if (expired > 0 )
            {
                foreach (var id in Amounts.Keys)
                {
                    user.AltGearSets[id].IncRatings[statName] -= Amounts[id] * expired;
                    RatingInc[id][statName] -= Amounts[id] * expired;
                }
            }
           
            if (evt.Heartbeat)
            {
                var isProc = Proc.ProcessProcAttempt(ref blp, Rppm, ref lastAttempt, evt.Timestamp);
                if (isProc)
                {
                    if (random.NextDouble() > successRate) { return; }
                    //Console.WriteLine()
                    var duration = evt.Timestamp + Buff.Duration;
                    buffEnds.Add(evt.Timestamp + duration);
                    foreach (var id in Amounts.Keys)
                    {
                        user.AltGearSets[id].IncRatings[statName] += Amounts[id];
                        RatingInc[id][statName] += Amounts[id];
                    }
                }

            }
        }
        public AstralAntenna() : base()
        {
            Name = "Astral Antenna";
            Rppm = 2.5;
            HasteScaling = false;
            Buff = new Data.StatBuffs.AstralAntenna(new UnitId(0, 0), 1);
        }
    }
}
