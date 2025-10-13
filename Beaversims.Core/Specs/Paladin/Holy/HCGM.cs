using Beaversims.Core.Common;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Paladin.Holy
{
    internal class HCGM
    {
        public static void TrackACSource(ThroughputEvent evt, User user)
        {
            var ac = (Abilities.AvengingCrusader)user.Abilities.Get(Abilities.AvengingCrusader.name);
            if (evt.IsDmgDoneEvent())
            {
                if (evt.AbilityName == Abilities.Judgment.name)
                {
                    ac.judgSourceDmg += evt.Amount.Raw;
                }

                if (evt.AbilityName == Abilities.CrusaderStrike.name)
                {
                    ac.csSourceDmg += evt.Amount.Raw;
                }

            }
        }
        public static void TrackEmpLod(Event evt, User user)
        {
            var lod = (Abilities.LightOfDawn)user.Abilities.Get(Abilities.LightOfDawn.name);
            if (evt is CastEvent && user.HasBuff(Buffs.EmpyreanLegacy.finalBuffId) && evt.AbilityName == Abilities.WordOfGlory.name)
            {
                lod.EmpCasts += 1;
            }
        }




        private static void AcHCCGMSource(AvengingCrusader ac, Ability judg, Ability cs)
        {
            var acJudgRatio = ac.judgSourceDmg / (ac.csSourceDmg + ac.judgSourceDmg);
            var acCsRatio = ac.csSourceDmg / (ac.judgSourceDmg + ac.csSourceDmg);
            ac.HCCGMSources.Add(new HCCGMSource(judg.Name, acJudgRatio));
            ac.HCCGMSources.Add(new HCCGMSource(cs.Name, acCsRatio));
        }

        private static void RemoveJudgCastScaling(User user, Fight fight, Ability judg)
        {
 
            // If user is casting judg this seldom, they are only interested in empyrean in which case it should not be a haste scaler.
            // If this is not true to any degree no matter how small, it should be a full cast scaler with max rest HCCGM.
            if (user.HasTalent(Talents.EmpyreanLegacy.id))
            {
                if (judg.Casts <= 1 + (fight.TotalTime / Talents.EmpyreanLegacy.cd))
                {
                    judg.RemoveHST(user, HST.Cast);
                }

            }
        }

        private static void HolyShockCdTime(Ability holyShock)
        {
            // Far from ideal but can't track glorious dawn procs... TODO find a better solution.
            // Seems to be able to proc from all hits
            holyShock.CdTimeHypo -= (holyShock.Heal.Count + holyShock.Damage.Count) * Talents.GloriousDawn.procChance * holyShock.AvgCdHypo();
        }


        private static void ConsecHCCGMSource(User user, Ability judg, Ability consec)
        {
            if (user.HasTalent(Talents.RighteousJudgment.id))
            {
                double consecJudgRatio = (double)judg.Casts / (judg.Casts + consec.Casts);
                double consecRatio = (double)consec.Casts / (judg.Casts + consec.Casts);

                consec.HCCGMSources.Add(new HCCGMSource(judg.Name, consecJudgRatio));
                consec.HCCGMSources.Add(new HCCGMSource(consec.Name, consecRatio));
            }
        }

        public static void ModifyHCCGMSources(User user, Fight fight)
        {
     
            var judg = user.Abilities.Get(Abilities.Judgment.name);
            var cs = user.Abilities.Get(Abilities.CrusaderStrike.name);
            var ac = (Abilities.AvengingCrusader)user.Abilities.Get(Abilities.AvengingCrusader.name);
            var holyShock = user.Abilities.Get(Abilities.HolyShock.name);
            var consec = user.Abilities.Get(Abilities.Consecration.name);
            var lod = (Abilities.LightOfDawn)user.Abilities.Get(Abilities.LightOfDawn.name);


            AcHCCGMSource(ac, judg, cs);
            RemoveJudgCastScaling(user, fight, judg);
            HolyShockCdTime(holyShock);
            ConsecHCCGMSource(user, judg, consec);

        }

        private static void EmpyreanLod(LightOfDawn lod)
        {
            var pureLodhpc = lod.Heal.Raw / (lod.Casts + (lod.EmpCasts * Talents.EmpyreanLegacy.coef));
            lod.HasteCastGainMod *= pureLodhpc * lod.Casts / lod.Heal.Raw;
        }

        public static void ModifyHCGM(User user, Fight fight)
        {
            var judg = user.Abilities.Get(Abilities.Judgment.name);
            var holyShock = user.Abilities.Get(Abilities.HolyShock.name);
            var cs = user.Abilities.Get(Abilities.CrusaderStrike.name);
            var ac = (Abilities.AvengingCrusader)user.Abilities.Get(Abilities.AvengingCrusader.name);
            var lod = (Abilities.LightOfDawn)user.Abilities.Get(Abilities.LightOfDawn.name);

            EmpyreanLod(lod);
        }
    }
}
