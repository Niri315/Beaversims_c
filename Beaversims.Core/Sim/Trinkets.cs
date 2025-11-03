using Beaversims.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Sim.SpecialEffects
{
    internal class AstralAntenna : SimpleProcStatEffect
    {
        public static readonly HashSet<string> Sources = ["Astral Antenna"];
        public List<double> buffEnds = [];

        private static readonly Random random = new Random();
        public const double successRate = 0.95;

        //public const double Delay = 2;


        public override void Reset()
        {
            base.Reset();
            buffEnds = [];
        }

        public override void Call(List<TpEvent> procEvents, List<Event> events, Event evt, User user, StatTracker curAltStats, int i, int iterationCount)
        {
            int expired = buffEnds.RemoveAll(end => end <= evt.Timestamp);
            if (expired > 0)
            {
                 user.AltGearSets[i].SimIncRatings[StatName] -= Amount * expired;
            }

            if (evt.Heartbeat)
            {
                var isProc = Proc.ProcessProcAttempt(ref blp, ScUtils.TrueRppm(curAltStats, Rppm, HasteScaling, i), ref lastAttempt, ref lastProc, evt.Timestamp);
                if (isProc)
                {
                    if (random.NextDouble() > successRate) { return; }
                    var duration = Buff.Duration;
                    buffEnds.Add(evt.Timestamp + duration);
                    user.AltGearSets[i].SimIncRatings[StatName] += Amount;
                    
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
