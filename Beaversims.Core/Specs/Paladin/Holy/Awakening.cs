using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Paladin.Holy
{
    internal class Awakening
    {
        public static void TrackAwakening(Event evt, User user)
        {
            var judg = (Abilities.Judgment)user.Abilities.Get(Abilities.Judgment.name);

            if (user.HasBuff(Abilities.Judgment.awakening15Id) && evt is CastEvent && evt.AbilityName == judg.Name)
            {
                user.AwakeningActive = true;
                evt.AwakenedCast = true;

            }

            if (evt.IsDmgDoneEvent() && evt.AbilityName == judg.Name && user.AwakeningActive)
            {
                evt.AwakenedJudgment = true;
                user.AwakeningActive = false;
            }
        }

        public static void AwakeningScalers(Event evt, User user)
        {
            var judg = (Abilities.Judgment)user.Abilities.Get(Abilities.Judgment.name);
            var ac = (Abilities.AvengingCrusader)user.Abilities.Get(Abilities.AvengingCrusader.name);
            var cs = (Abilities.CrusaderStrike)user.Abilities.Get(Abilities.CrusaderStrike.name);

            if (evt is CastEvent && (evt.AbilityName == judg.Name || evt.AbilityName == cs.Name))
            {
                if (evt.AwakenedCast)
                {
                    judg.Scalers.Remove(StatName.Crit);
                    ac.Scalers.Remove(StatName.Crit);
                }
                else
                {
                    judg.Scalers.Add(StatName.Crit);
                    ac.Scalers.Add(StatName.Crit);
                }
            }

        }
    }
}
