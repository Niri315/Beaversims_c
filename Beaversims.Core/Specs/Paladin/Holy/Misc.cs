using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
