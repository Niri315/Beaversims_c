using Beaversims.Core.Shared;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Paladin.Holy
{
    internal class CastProcessor
    {



        public static bool IsIolEvent(CastEvent evt, User user)
        {
            if (user.HasBuff(Buffs.InfusionOfLight.buffId) && Buffs.InfusionOfLight.Abilities.Contains(evt.AbilityName))
            {
                return true;
            }
            return false;
        }
        private static void TrackCdTime(CastEvent evt, User user)
            // We don't really care how technically accurate this is.
            // We are only interested in knowing what filler abilities are capped by users rotation and to what degree.
            // For this we can make the assumption that all cd fillers are always on CD. 
            // Technically incorrect, but the math will fit.

        {
            var timestamp = evt.Timestamp;
            var ability = evt.Ability;
            var haste = (Haste)evt.UserStats.Get(StatName.Haste);
            var cd = ability.Cd;
            cd = Calc.TrueCdCalc(haste, cd);
            var autumn = user.GetBuff(Data.StatBuffs.BlessingOfAutumn.id);
            if (autumn != null)
            { 
                var autumnRemDur = autumn.BuffEnd - evt.Timestamp;
                var relTime = Math.Min(cd, autumnRemDur);
                var cdrCoef = 0.3; //Todo get from buff property.
                var autumnReduct = relTime * cdrCoef;
                cd -= autumnReduct;
            }

            var judg = user.Abilities.Get(Abilities.Judgment.name);
            var cs = user.Abilities.Get(Abilities.CrusaderStrike.name);
            var holyShock = user.Abilities.Get(Abilities.HolyShock.name);

            ability.CdEnd = timestamp + cd;

            if (user.HasTalent(Talents.CrusadersMight.id) && evt.AbilityName == Abilities.CrusaderStrike.name)
            {
                var crusMight = (Talents.CrusadersMight)user.Talents[Talents.CrusadersMight.id];
                //judg.CdEnd -= crusMight.CdReduct;
                //holyShock.CdEnd -= crusMight.CdReduct;
                judg.CdTimeHypo -= crusMight.CdReduct;
                holyShock.CdTimeHypo -= crusMight.CdReduct;
            }

            if (evt.AbilityName == Abilities.ShieldOfTheRighteous.name)
            {
                cs.CdTimeHypo -= Abilities.ShieldOfTheRighteous.csReduct;
            }
            if (evt.AbilityName == Abilities.AvengingCrusader.name)
            {
                // AC gives an extra CS, need to make up for it.
                cs.CdTimeHypo -= Calc.TrueCdCalc(haste, cs.Cd); // cba to do it proper this is good enough.
            }

            if (IsIolEvent(evt, user) && user.HasTalent(Talents.ImbuedInfusions.id))
            {
                var imbuedInf = (Talents.ImbuedInfusions)user.Talents[Talents.ImbuedInfusions.id];
                holyShock.CdTimeHypo -= imbuedInf.CdReduct;
            }


            ability.CdTimeHypo += cd;

        }
        private static double ApplyReductEffects(CastEvent cEvt, User user, double castTime)
        {
            return castTime;
        }



        public static void ProcessCast(Event evt, User user)
        {
            if (evt is CastEvent cEvt && evt.SourceUnit is User)
            {
                HolyPaladin userSpec = (HolyPaladin) user.Spec;
                var ability = cEvt.Ability;
                var castTime = ability.CastTime;

                if (castTime > 0)
                {
                    castTime = ApplyReductEffects(cEvt, user, castTime);
                    Shared.HCCGM.CastTimeGains(cEvt, user, castTime);
                }

                TrackCdTime(cEvt, user);

            }
        }
    }
}
