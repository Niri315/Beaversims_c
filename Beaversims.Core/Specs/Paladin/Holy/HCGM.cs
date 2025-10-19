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
        // Currently in almost all situations both high and low priority casts are holy power generating.
        // More casts will not affect spenders more than the rest hccgm does.
        // It is appropriate to deal with holy power spenders as normal rest hccgm casts.
        // If this changes we should make spenders hccgm be affected and scale alongside hccgm of generators.

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
            ac.CIMSources.Add(new CIMSource(judg.Name, acJudgRatio));
            ac.CIMSources.Add(new CIMSource(cs.Name, acCsRatio));
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
            // TODO Sanctified Wrath
            holyShock.CdTimeHypo -= (holyShock.Heal.Count + holyShock.Damage.Count) * Talents.GloriousDawn.procChance * holyShock.AvgCdHypo();
        }


        private static void ConsecHCCGMSource(User user, Ability judg, Ability consec)
        {
            if (user.HasTalent(Talents.RighteousJudgment.id))
            {
                double consecJudgRatio = (double)judg.Casts / (judg.Casts + consec.Casts);
                double consecRatio = (double)consec.Casts / (judg.Casts + consec.Casts);

                consec.CIMSources.Add(new CIMSource(judg.Name, consecJudgRatio));
                consec.CIMSources.Add(new CIMSource(consec.Name, consecRatio));
            }
        }

        public static void SunsAvatarHCCGMSource(User user, SunsAvatar sunsAvatar)
        {
            if (sunsAvatar.Heal.Raw == 0) return;
            var abilities = user.Abilities;
            var awakeningRatio = sunsAvatar.AwakeningHealRaw / sunsAvatar.Heal.Raw;
            var avengingRatio = 1 - awakeningRatio;
            HashSet<Ability> spenders = HpalUtils.GetSpenderAbilities(abilities);
            // Don't need to worry about maxHCCGM here.
            sunsAvatar.CIMSources.Add(new CIMSource(sunsAvatar.Name, avengingRatio));
            var totalSpenderCasts = 0;
            foreach (var spender in spenders)
            {
                totalSpenderCasts += spender.Casts;
            }
            foreach (var spender in spenders)
            {
                sunsAvatar.CIMSources.Add(new CIMSource(spender.Name, ((double)spender.Casts / totalSpenderCasts) * awakeningRatio));

            }
        }
        public static void DivineGuidanceHCCGMSource(User user, DivineGuidance divineGuidance)
        {
            if (divineGuidance.Heal.Raw == 0) return;
            var abilities = user.Abilities;
            HashSet<Ability> spenders = HpalUtils.GetSpenderAbilities(abilities);
            var totalSpenderCasts = 0;
            foreach (var spender in spenders)
            {
                totalSpenderCasts += spender.Casts;
            }
            foreach (var spender in spenders)
            {
                divineGuidance.CIMSources.Add(new CIMSource(spender.Name, (double)spender.Casts / totalSpenderCasts));

            }
        }

        public static void SunSearHCCGMSource(User user, SunSear sunSear, LightOfDawn lod, HolyShock holyShock)
        {
            if (sunSear.Heal.Raw == 0) return;
            var abilities = user.Abilities;
            var total = lod.Heal.Crit.Count + holyShock.Heal.Crit.Count;
            var lodRatio = lod.Heal.Crit.Count / total;
            var holyShockRatio = 1 - lodRatio;
            HashSet<Ability> spenders = HpalUtils.GetSpenderAbilities(abilities);
            sunSear.CIMSources.Add(new CIMSource(lod.Name, lodRatio));
            sunSear.CIMSources.Add(new CIMSource(holyShock.Name, holyShockRatio));
        
        }
        public static bool IsIolEvent(Event evt, User user) => evt is CastEvent && Buffs.InfusionOfLight.Abilities.Contains(evt.AbilityName) && user.HasBuff(Buffs.InfusionOfLight.buffId);

        public static void TrackIoL(Event evt, User user)
        {
            if (IsIolEvent(evt, user))
            {
                evt.Ability.IolCount++;
                //Console.WriteLine(evt.AbilityName);
            }

        }


        public static void ModifyCIMSources(User user, Fight fight)
        {
     
            var judg = user.Abilities.Get(Abilities.Judgment.name);
            var cs = user.Abilities.Get(Abilities.CrusaderStrike.name);
            var ac = (Abilities.AvengingCrusader)user.Abilities.Get(Abilities.AvengingCrusader.name);
            var aw = (Abilities.AvengingWrath)user.Abilities.Get(Abilities.AvengingWrath.name);
            var sunsAvatar = (Abilities.SunsAvatar)user.Abilities.Get(Abilities.SunsAvatar.name);
            var sunSear = (Abilities.SunSear)user.Abilities.Get(Abilities.SunSear.name);
            var divineGuidance = (Abilities.DivineGuidance)user.Abilities.Get(Abilities.DivineGuidance.name);
            var holyShock = (HolyShock)user.Abilities.Get(Abilities.HolyShock.name);
            var consec = user.Abilities.Get(Abilities.Consecration.name);
            var lod = (Abilities.LightOfDawn)user.Abilities.Get(Abilities.LightOfDawn.name);


            AcHCCGMSource(ac, judg, cs);
            RemoveJudgCastScaling(user, fight, judg);
            HolyShockCdTime(holyShock);
            ConsecHCCGMSource(user, judg, consec);
            SunsAvatarHCCGMSource(user, sunsAvatar);
            SunSearHCCGMSource(user, sunSear, lod, holyShock);
            DivineGuidanceHCCGMSource(user, divineGuidance);
            HolyShockHCGM(user, holyShock);
            EmpyreanLod(lod);

        }

        private static void EmpyreanLod(LightOfDawn lod)
        {
            var pureLodhpc = lod.Heal.Raw / (lod.Casts + (lod.EmpCasts * Talents.EmpyreanLegacy.coef));
            //lod.HCGM *= pureLodhpc * lod.Casts / lod.Heal.Raw;
            var hcgm = pureLodhpc * lod.Casts / lod.Heal.Raw;
            lod.CIMSources.Add(new CIMSource(lod.Name, hcgm));
            lod.CIMSources.Add(new CIMSource(Shared.Abilities.ZeroCIMDummy.name, 1 - hcgm));

        }

        public static void HolyShockHCGM(User user, HolyShock holyShock)
        {
            // Note Second sunrise: Procs from all Holy Shocks EXCEPT for the 4/5 from divine toll, and the 2 extra from rising sunlight.

            var divineToll = (DivineToll)user.Abilities.Get(DivineToll.name);
            var aw = (AvengingWrath)user.Abilities.Get(AvengingWrath.name);
            var ac = (AvengingCrusader)user.Abilities.Get(AvengingCrusader.name);


            var castValue = 0.0;
            var nonCastValue = 0.0;

            castValue += holyShock.Casts;
            nonCastValue += divineToll.Casts * divineToll.holyShockCount;

            if (user.HasTalent(Talents.SecondSunrise.id))
            {
                var ssTal = (Talents.SecondSunrise)user.Talents[Talents.SecondSunrise.id];
                var ssCoef = ssTal.Coef;
                castValue += holyShock.Casts * ssCoef;
                nonCastValue += ssCoef * 1; // Only 1 extra from divine toll.
            }
            if (user.HasTalent(Talents.RisingSunlight.id))
            {
                var rs = (Talents.RisingSunlight)user.Talents[Talents.RisingSunlight.id];
                nonCastValue += aw.Casts * 2 * rs.HolyShockCount;
                nonCastValue += ac.Casts * rs.HolyShockCount;
                nonCastValue += divineToll.Casts * 2 * rs.HolyShockCount;
            }
            if (user.HasTalent(Talents.DivineResonance.id))
            {
                var dr = (Talents.DivineResonance)user.Talents[Talents.DivineResonance.id];
                nonCastValue += dr.HolyShockCount * divineToll.Casts;
                if (user.HasTalent(Talents.SecondSunrise.id))
                {
                    var ssTal = (Talents.SecondSunrise)user.Talents[Talents.SecondSunrise.id];
                    var ssCoef = ssTal.Coef;
                    nonCastValue += dr.HolyShockCount * ssCoef * divineToll.Casts;
                }
            }
            var totalValue = castValue + nonCastValue;
    
            holyShock.CIMSources.Add(new CIMSource(holyShock.Name, castValue / totalValue));
            holyShock.CIMSources.Add(new CIMSource(divineToll.Name, nonCastValue / totalValue));
            //foreach (var source in holyShock.CIMSources)
            //{
            //    Console.WriteLine(source.CIMReliance);
            //    Console.WriteLine(source.Name);
            //    Console.WriteLine(user.Abilities.Get(source.Name).CIMDerivedHCGM(user));

            //}
            //Console.WriteLine(holyShock.FullHCGM(user, 0));
            //Console.WriteLine(holyShock.HCGM);
            //Console.WriteLine(holyShock.CIM);
            var sunSear = (Abilities.SunSear)user.Abilities.Get(Abilities.SunSear.name);
            //Console.WriteLine(sunSear.FullHCGM(user, 0));



        }

        //public static void ModifyHCGM(User user, Fight fight)
        //{
        //    var judg = user.Abilities.Get(Abilities.Judgment.name);
        //    var holyShock = (HolyShock)user.Abilities.Get(Abilities.HolyShock.name);
        //    var cs = user.Abilities.Get(Abilities.CrusaderStrike.name);
        //    var ac = (Abilities.AvengingCrusader)user.Abilities.Get(Abilities.AvengingCrusader.name);
        //    var lod = (Abilities.LightOfDawn)user.Abilities.Get(Abilities.LightOfDawn.name);

        //    //holyShock.AlterHCGM(user);

        //}
    }
}
