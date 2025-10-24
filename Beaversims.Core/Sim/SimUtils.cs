using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Sim
{
    internal class SimUtils
    {

        public static List<Event> FetchTimingEvents(List<Event> events, double dur, double cd)
        {
            if (events == null || events.Count == 0)
                return new List<Event>();

            var timings = UseTimingsCalc(events, dur, cd);
            var activeEvents = new List<Event>();

            foreach (var ev in events)
            {
                var timestamp = ev.Timestamp;
                foreach (var window in timings)
                {
                    // strict inequalities to mirror: if timestamp > window > timestamp - dur
                    if (window < timestamp && window > timestamp - dur)
                    {
                        activeEvents.Add(ev);
                        break; // avoid duplicates if multiple windows fall in the same event window
                    }
                }
            }

            return activeEvents;
        }

        public static List<double> UseTimingsCalc(
            List<Event> events,
            double duration,
            double cd,
            Stat? statTarget = null)
        {
            if (events == null || events.Count == 0)
                return new List<double>();

            // Assumes events are sorted by time; if not, sort them.
            // events = events.OrderBy(e => e.Timestamp).ToList();

            var samples = new Dictionary<int, double>();
            int intervalNum = 5; // May need to scale with encounter time
            double endTime = events[^1].Timestamp;
            var intervals = new List<int>();

            // Build interval sample points every 'intervalNum' seconds up to round(end_time)
            for (int i = 0; i < Math.Round(endTime); i++)
            {
                if (i % intervalNum == 0)
                {
                    samples[i] = 0.0;
                    intervals.Add(i);
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


                    foreach (int i in intervals)
                    {
                        if (i > timestamp - duration && i < timestamp)
                        {
                            samples[i] += amount;
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
            for (int i = 0; i < n; i++)
            {
                int timestamp = sortedData[i].Key;
                double amount = sortedData[i].Value;

                // Option 1: don't use at i
                if (i > 0) maxGain[i] = maxGain[i - 1];

                // Option 2: use at i
                double maxGainWithUse = amount;

                // find the last index j whose timestamp <= timestamp - cd
                int j = i - 1;
                while (j >= 0 && sortedData[j].Key > timestamp - cd)
                    j--;

                if (j >= 0)
                    maxGainWithUse += maxGain[j];

                maxGain[i] = Math.Max(maxGain[i], maxGainWithUse);
            }

            // Backtrack to recover chosen timestamps
            var optimalTimestamps = new List<double>();
            {
                int i = n - 1;
                while (i >= 0)
                {
                    if (i == 0 || maxGain[i] != maxGain[i - 1])
                    {
                        // we used the timestamp at i
                        optimalTimestamps.Add(sortedData[i].Key);

                        // skip indices within cooldown of this choice
                        int j = i - 1;
                        while (j >= 0 && sortedData[j].Key > sortedData[i].Key - cd)
                            j--;
                        i = j;
                    }
                    else
                    {
                        i--;
                    }
                }
            }

            optimalTimestamps.Reverse();
            return optimalTimestamps;
        }
    }
}
