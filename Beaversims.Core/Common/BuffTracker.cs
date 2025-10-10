using Beaversims.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    internal static class BuffTracker
    {
        public static void TrackBuffs(Event evt, UnitRepo allUnits, Logger statLogger = null)
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
                    buffEvent.TargetUnit.AddBuff(buffName, buffId, sourceUnit, buffStacks, statLogger, timestamp);
                }
                else if (buffEvent.BuffRemoveEvent)
                {
                    buffEvent.TargetUnit.RemoveBuff(buffId, sourceUnit, statLogger, timestamp);
                }
                else if (buffEvent.BuffStackEvent)
                {
                    buffEvent.TargetUnit.ChangeBuffStack(buffName, buffId, sourceUnit, buffStacks, statLogger, timestamp);
                }
            }
            if (user.HasBuff(Shared.Abilities.BlessingOfSummer.buffId))
            {
                evt.SummerActive = true;
            }
            evt.UserStats = user.Stats.Clone();
        }
    }
}

