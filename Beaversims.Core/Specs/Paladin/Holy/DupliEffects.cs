using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Beaversims.Core.Specs.Paladin.Holy
{
    internal class DupliEffects
    {
        
        public static void AddBeaconPolHeal(Event evt)
        {
            if (evt.AbilityId == Abilities.BeaconOfLight.polId && evt.IsHealDoneEvent())
            {
                var hEvt = (HealEvent)evt;
                var bol = (Abilities.BeaconOfLight)evt.Ability;
                bol.PolHeal.Eff += hEvt.Amount.Eff;
                bol.PolHeal.Raw += hEvt.Amount.Raw;
            }
        }
        public static void SetBeaconCoef(User user)
        {
            var beaconOfLight = (Abilities.BeaconOfLight)user.Abilities.Get(Abilities.BeaconOfLight.name);
            if (user.HasTalent(Talents.CommandingLight.id))
            {
                var commandingLight = (Talents.CommandingLight)user.Talents[Talents.CommandingLight.id];
                beaconOfLight.Coef += commandingLight.Coef;

            }
            if (user.HasTalent(Talents.BeaconOfFaith.id))
            {
                var beaconOfFaith = (Talents.BeaconOfFaith)user.Talents[Talents.BeaconOfFaith.id];
                beaconOfLight.Coef *= 1 - beaconOfFaith.Coef;
            }
        }

        public static bool IsBeaconEvent(HealEvent evt, User user)
            => evt.IsHealDoneEvent() && evt.Ability.Direct && evt.SourceUnit is User && !evt.Tick;
        
        public static double BeaconFormula(ThroughputEvent evt, User user)
        {
            var beaconOfLight = (Abilities.BeaconOfLight)user.Abilities.Get(Abilities.BeaconOfLight.name);
            return beaconOfLight.Coef * evt.BeaconCount;
        }

        public static void BeaconHypo(HealEvent evt, User user)
        {
            if (IsBeaconEvent(evt, user))
            {
                var beaconOfLight = (Abilities.BeaconOfLight)user.Abilities.Get(Abilities.BeaconOfLight.name);
                beaconOfLight.Heal.Hypo += evt.Amount.Raw * BeaconFormula(evt, user);
            }
           
        }
        public static void BeaconGains(ThroughputEvent evt, User user, StatName statName, double gainRaw, GainType gainType)
        {
            if (IsBeaconEvent((HealEvent)evt, user))
            {
                var beaconOfLight = (Abilities.BeaconOfLight)user.Abilities.Get(Abilities.BeaconOfLight.name);
                var hypoGain = gainRaw * BeaconFormula(evt, user);
                var dupliGainRaw = hypoGain * beaconOfLight.HypoTrueRawR();
                var dupliGainEff = hypoGain * beaconOfLight.HypoTrueUr();
                evt.Gains[statName][gainType] += dupliGainEff;

                var dupliGainNsnsnaraw = beaconOfLight.RawToNsnsnarawConvert(dupliGainRaw);
                Shared.DupliEffects.SummerGains(evt, user, statName, gainRaw, beaconOfLight, evt.SummerActive, false, evt.SourceUnit, gainType);

                Shared.DupliEffects.LeechSourceGains(evt, user, statName, dupliGainNsnsnaraw, gainType);
            }
        }
    }
}
