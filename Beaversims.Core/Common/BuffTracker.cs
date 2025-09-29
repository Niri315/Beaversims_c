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
        public static void TrackBuffs(Event evt, UnitRepo allUnits)
        {
            var user = allUnits.GetUser();
            if (evt is BuffEvent buffEvent)
            {
                var buffId = evt.AbilityId;
                var sourceUnit = evt.SourceUnit;
                var buffStacks = buffEvent.BuffStacks;
                var buffName = evt.AbilityName;
                var sourceId = sourceUnit.Id;

                if (buffEvent.BuffApplyEvent)
                {
                    buffEvent.TargetUnit.AddBuff(buffName, buffId, sourceUnit, buffStacks);
                }
                else if (buffEvent.BuffRemoveEvent)
                {
                    buffEvent.TargetUnit.RemoveBuff(buffId, sourceUnit);
                }
                else if (buffEvent.BuffStackEvent)
                {
                    buffEvent.TargetUnit.ChangeBuffStack(buffName, buffId, sourceUnit, buffStacks);
                }
            }
            evt.UserStats = user.Stats.Clone();
        }
    }
}

