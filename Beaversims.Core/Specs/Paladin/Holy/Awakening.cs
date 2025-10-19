using Beaversims.Core.Shared;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Beaversims.Core.Specs.Paladin.Holy
{
    internal class Awakening
    {

        public static bool IsAwakenedAc(Event evt, User user) =>
            evt.Timestamp > user.AvengingUseEnd && user.HasBuff(Abilities.AvengingCrusader.buffId);

        public static bool IsAwakenedAw(Event evt, User user) =>
            evt.Timestamp > user.AvengingUseEnd && user.HasBuff(Abilities.AvengingWrath.buffId);

        public static void TrackAwakening(Event evt, User user)
        {
            var judg = (Abilities.Judgment)user.Abilities.Get(Abilities.Judgment.name);
            var ac = (Abilities.AvengingCrusader)user.Abilities.Get(Abilities.AvengingCrusader.name);
            var cs = (Abilities.CrusaderStrike)user.Abilities.Get(Abilities.CrusaderStrike.name);
            var aw = (Abilities.AvengingWrath)user.Abilities.Get(Abilities.AvengingWrath.name);

            if (evt is CastEvent)
            {
                if (evt.AbilityName == ac.Name)
                {
                    user.AvengingUseEnd = evt.Timestamp + ac.onUseDur;
                }
                else if (evt.AbilityName == aw.Name)
                {
                    user.AvengingUseEnd = evt.Timestamp + aw.onUseDur;
                }
            }

            if (IsAwakenedAw(evt, user)) {
                if (evt.AbilityName == Abilities.SunsAvatar.name && evt.IsHealDoneEvent() && evt is ThroughputEvent tEvt)
                {
                    var sunsAvatar = (Abilities.SunsAvatar)user.Abilities.Get(Abilities.SunsAvatar.name);
                    sunsAvatar.AwakeningHealRaw += tEvt.Amount.Raw;
                }                
            }

            if (user.HasBuff(Abilities.Judgment.awakening15Id) && evt is CastEvent && evt.AbilityName == judg.Name)
            {
                user.AwakeningActive = true;
                evt.AwakenedCast = true;
                user.AwakeningCount += 1;
            }

            if (evt.IsDmgDoneEvent() && evt.AbilityName == judg.Name && user.AwakeningActive)
            {
                evt.AwakenedJudgment = true;
                user.AwakeningActive = false;
            }
            if (evt is CastEvent && (evt.AbilityName == judg.Name || evt.AbilityName == cs.Name))
            {
                if (evt.AwakenedCast)
                {
                   
                    user.BanCritScaleJudgAC = true;
                    user.BanCritScaleJudgAC = true;

                }
                else
                {
                    user.BanCritScaleJudgAC = false;
                    user.BanCritScaleJudgAC = false;
                }
            }
            evt.BanCritScaleJudgAC = user.BanCritScaleJudgAC;
        }
        public static void JudgAcCritGains(ThroughputEvent tEvt, User user, int i)
        {

            if ((tEvt.AbilityName == Abilities.Judgment.name || tEvt.AbilityName == AvengingCrusader.name) && !tEvt.BanCritScaleJudgAC)
            {

                if (tEvt.IsDmgDoneEvent())
                {
                    StatGains.CritGainsDmg(tEvt, user, i);

                }
                if (tEvt is HealEvent hEvt && hEvt.IsHealDoneEvent())
                {
                    StatGains.CritGainsHeal(hEvt, user, i);
                }
            }
        }
    }
}
