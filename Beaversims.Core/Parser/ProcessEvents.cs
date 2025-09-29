using Beaversims.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    internal class ProcessEvents
    {
        public static GainMatrix GetStatWeights(List<Event> events, Fight fight)
        {
            GainMatrix swGains = Utils.InitGainMatrix();
            foreach (Event evt in events)
            {
                if (evt is ThroughputEvent tpEvent )
                {
                    foreach(var statEntry in tpEvent.Gains)
                    {
                        var stat = statEntry.Key;
                        foreach(var gainEntry in statEntry.Value)
                        {
                            var gainType = gainEntry.Key;
                            swGains[stat][gainType] += gainEntry.Value / fight.TotalTime;
                        }
                    }
                }
            }
            return swGains;
        }
    }
}
