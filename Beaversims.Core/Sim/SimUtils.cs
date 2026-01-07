using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Sim
{
    internal class SimUtils
    {
        public static double VersMod(StatTracker curStats)
        {
            var vers = (Vers)curStats.Get(StatName.Vers);
            return 1 + (vers.SimTempEff() / vers.PercentRate / 100);
        }
        public static double CritMod(StatTracker curStats)
        {
            var crit = (Crit)curStats.Get(StatName.Crit);
            return (1 + ((crit.SimTempEff() / crit.PercentRate / 100) * (crit.IncHeal - 1)));
        }

        //public static double VersCritMod(StatTracker curStats)
        //{
        //    var crit = (Crit)curStats.Get(StatName.Crit);
        //    var vers = (Vers)curStats.Get(StatName.Vers);
        //    //Console.WriteLine(1 + ((crit.SimTempEff() / crit.PercentRate / 100)));
        //    //Console.WriteLine(VersMod(curStats));

        //    return (1 + ((crit.SimTempEff() / crit.PercentRate / 100) * (crit.IncHeal - 1))) * VersMod(curStats);
        //}

        public static List<Event> AvailableUseEvents(List<Event> events, List<double> useTimings, double sharedCd)
        {
            var availableEvents = new List<Event>();

            foreach (var evt in events)
            {
                var timestamp = evt.Timestamp;
                bool banned = false;

                foreach (var window in useTimings)
                {
                    if (timestamp >= window && timestamp < window + sharedCd)
                    {
                        banned = true;
                        break;
                    }
                }
                if (!banned)
                {
                    availableEvents.Add(evt);
                }
            }
            return availableEvents;
        }

        public static List<Event> FetchTimingEvents(List<double> useTimings, List<Event> events, double dur, double cd, int i)
        {

            var activeEvents = new List<Event>();
            //foreach (var timing in useTimings) { Console.WriteLine(timing.ToString()); }
            
            foreach (var ev in events)
            {
                var timestamp = ev.Timestamp;
                foreach (var window in useTimings)
                {
                    if (window < timestamp && window > timestamp - dur)
                    {
                        activeEvents.Add(ev);
                        break;
                    }
                }
            }

            return activeEvents;
        }



        public static List<double> UseTimingsCalc(List<Event> events, double duration, double cd, int i)
        {
            if (events == null || events.Count == 0)
                return new List<double>();

            var samples = new Dictionary<int, double>();
            int intervalNum = 1;
            double endTime = events[^1].Timestamp;
            var intervals = new List<int>();

            // Build interval sample points every 'intervalNum' seconds up to round(end_time)
            for (int z = 0; z < Math.Round(endTime); z++)
            {
                if (z % intervalNum == 0)
                {
                    samples[z] = 0.0;
                    intervals.Add(z);
                }
            }

            // Accumulate amounts into intervals that fall within (timestamp - duration, timestamp)
            foreach (var evt in events)
            {
                if (evt is TpEvent tEvt && (evt.IsHealDoneEvent() || evt.IsDmgDoneEvent()))
                {
                    double timestamp = evt.Timestamp;
                    double amount;


                    amount = tEvt.Amount.Eff;


                    foreach (int z in intervals)
                    {
                        if (z > timestamp - duration && z < timestamp)
                        {
                            samples[z] += amount;
                        }
                    }
                }
              
            }

            // Sort samples by time
            var sortedData = samples.OrderBy(kv => kv.Key).ToList();
            int n = sortedData.Count;
            if (n == 0)
                return new List<double>();

            var maxGain = new double[n];

            // DP to compute optimal total gain with cooldown constraint
            for (int z = 0; z < n; z++)
            {
                int timestamp = sortedData[z].Key;
                double amount = sortedData[z].Value;

                // Option 1: don't use at i
                if (z > 0) maxGain[z] = maxGain[z - 1];

                // Option 2: use at i
                double maxGainWithUse = amount;

                // find the last index j whose timestamp <= timestamp - cd
                int j = z - 1;
                while (j >= 0 && sortedData[j].Key > timestamp - cd)
                    j--;

                if (j >= 0)
                    maxGainWithUse += maxGain[j];

                maxGain[z] = Math.Max(maxGain[z], maxGainWithUse);
            }

            // Backtrack to recover chosen timestamps
            var optimalTimestamps = new List<double>();
            {
                int z = n - 1;
                while (z >= 0)
                {
                    if (z == 0 || maxGain[z] != maxGain[z - 1])
                    {
                        // we used the timestamp at i
                        optimalTimestamps.Add(sortedData[z].Key);

                        // skip indices within cooldown of this choice
                        int j = z - 1;
                        while (j >= 0 && sortedData[j].Key > sortedData[z].Key - cd)
                            j--;
                        z = j;
                    }
                    else
                    {
                        z--;
                    }
                }
            }

            optimalTimestamps.Reverse();
            return optimalTimestamps;
        }
    }
}
