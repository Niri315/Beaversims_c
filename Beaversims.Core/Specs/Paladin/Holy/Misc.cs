using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Beaversims.Core.Specs.Paladin.Holy.Abilities;

namespace Beaversims.Core.Specs.Paladin.Holy
{
    internal class Misc
    {

        public static void TrackArmaments(Event evt, User user)
        {
            if (evt is BuffEvent bEvt && (bEvt.BuffApplyEvent ||bEvt.BuffRefreshEvent) && evt.SourceUnit is User && evt.TargetUnit is User && (bEvt.AbilityId == Shared.Abilities.HolyBulwark.buffId || bEvt.AbilityId == Abilities.SacredWeapon.buffId))
            {
                user.ArmamentsBuffCount++;

            }
        }



        public static void SetArmamentsAutoMod(User user)
        {
            var bulwark = user.Abilities.Get(Shared.Abilities.HolyBulwark.name);
            if (!user.HasTalent(Talents.DivineInspiration.id))
            {
                bulwark.HasteAutoModHeal = 0;
                return; 
            }
            var sacredWep = user.Abilities.Get(Abilities.SacredWeapon.name);
          
            var ac = user.Abilities.Get(Abilities.AvengingCrusader.name);

            var procRatio = (user.ArmamentsBuffCount - (sacredWep.Casts + bulwark.Casts + ac.Casts + user.AwakeningCount)) / (double)user.ArmamentsBuffCount;
            // using += for wep since we want to retain the normal auto from actually proccing the heal/dmg
            // bulwark goes from 1.0 so using *=
            sacredWep.HasteAutoModHeal += procRatio;
            sacredWep.HasteAutoModDmg += procRatio;
            bulwark.HasteAutoModHeal *= procRatio;

        }

        public static void HolyPowerQIM(User user)
        {
            var abilities = user.Abilities;
            var judg = (Judgment)abilities.Get(Judgment.name);
            var holyShock = (HolyShock)abilities.Get(HolyShock.name);
            var cs = (CrusaderStrike)abilities.Get(CrusaderStrike.name);
            var fol = (FlashOfLight)abilities.Get(FlashOfLight.name);
            var hl = (HolyLight)abilities.Get(HolyLight.name);
            var how = (HammerOfWrath)abilities.Get(HammerOfWrath.name);

            double scaleHP;
            double nonScaleHP;
            scaleHP = how.Casts + cs.Casts + holyShock.HolyPowerScaleCount + judg.Casts + judg.IolCount;  // judg.IolCount only scales to a certain degree. todo
            nonScaleHP = holyShock.HolyPowerNonScaleCount; 
            if (user.HasTalent(Talents.TowerOfRadiance.id))
            {
                scaleHP += fol.Casts + hl.Casts;
            }
            var spenders = HpalUtils.GetSpenderAbilities(abilities);
            foreach (var spender in spenders)
            {
                //spender.CIMRatio = (scaleHP / (scaleHP + nonScaleHP));
                spender.RestRelCIMRatio = scaleHP / (scaleHP + nonScaleHP);
                spender.RestRelCIM = true;
                //spender.CIMRatio = (scaleHP / (scaleHP + nonScaleHP) - 1);
                //spender.CIMRatio = ((scaleHP / (scaleHP + nonScaleHP)) * (scaleGain + nonScaleGain) - scaleGain) / nonScaleGain;
            }
            Console.WriteLine($"Holy Shock Scale HP: {holyShock.HolyPowerScaleCount} Holy Shock Non Scale HP: {holyShock.HolyPowerNonScaleCount}");
            Console.WriteLine($"Scale HP: {scaleHP} Non Scale HP: {nonScaleHP}");
            Console.WriteLine($"Spender CIM Adjust: {scaleHP / (scaleHP + nonScaleHP)}");
        }
    }
}
