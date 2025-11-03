using Beaversims.Core.Common;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    internal static class BuffTracker
    {
        public static void TrackBuffs(bool swOption, Event evt, UnitRepo allUnits, Logger statLogger = null, Logger refStatLogger = null)
        {
            var user = allUnits.GetUser();

            if (evt is BuffEvent buffEvent)
            {
                var buffId = evt.AbilityId;
                var sourceUnit = evt.SourceUnit;
                var buffStacks = buffEvent.BuffStacks;
                var buffName = evt.AbilityName;
                var sourceId = sourceUnit.Id;
                var timestamp = evt.Timestamp;

                if (buffEvent.BuffApplyEvent)
                {
                    buffEvent.TargetUnit.AddBuff(swOption, buffName, buffId, sourceUnit, buffStacks, timestamp, statLogger, refStatLogger);
                }
                else if (buffEvent.BuffRemoveEvent)
                {
                    buffEvent.TargetUnit.RemoveBuff(buffId, sourceUnit, statLogger, timestamp, refStatLogger);
                }
                else if (buffEvent.BuffStackEvent)
                {
                    buffEvent.TargetUnit.ChangeBuffStack(swOption, buffName, buffId, sourceUnit, buffStacks, statLogger, timestamp, refStatLogger);
                }
            }
            if (user.HasBuff(Shared.Abilities.BlessingOfSummer.buffId))
            {
                evt.SummerActive = true;
            }
            evt.UserStats = user.Stats.Clone();
            evt.RefStats = user.RefStats.Clone();
        }
    }
}

