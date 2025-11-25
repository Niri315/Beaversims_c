using Beaversims.Core;
using Beaversims.Core.Specs.Evoker.Pres.Abilities;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Beaversims.Core.Specs.Evoker.Pres
{
    internal class DupliEffects
    {
        // Not tracking lifebind buff count for now. Should do this for bugtesting.
        public static bool IsLifebindEvt(HealEvent evt, User user)
        {
           
            var lifebindBuff = user.GetBuff(Abilities.Lifebind.buffId);
            
            if (lifebindBuff is Buff
                && evt.LifebindCount > 0
                && lifebindBuff.SourceId == user.Id 
                && evt.IsHealDoneEvent()
                && evt.TargetUnit.HasBuff(Abilities.Lifebind.buffId)
                && evt.Ability.CanDupli
                && !evt.AbsorbAbility
                && evt.AbilityName != Abilities.Lifebind.name
                )
            {
                evt.LifebindEvent = true;
                
                return true;
             
            }
            return false;
        }


        public static void LifebindHypo(HealEvent evt, User user)
        {
            if (IsLifebindEvt(evt, user))
            {
      
                var lifebind = (Abilities.Lifebind)user.Abilities.Get(Abilities.Lifebind.name);
                var count = 0;
                if (evt.TargetUnit is User)
                {
                    count = evt.LifebindCount;
                }
                else
                {
                    count = 1;
                }
                lifebind.Heal.Hypo += evt.Amount.Raw * Abilities.Lifebind.coef * count;

            }

        }

        public static void AltLifebind(List<TpEvent> events, User user, int i)
        {
            var lifebind = (Abilities.Lifebind)user.Abilities.Get(Abilities.Lifebind.name);
            if (lifebind.Heal.Raw == 0) return;

            foreach (var evt in events)
            {

                if (evt is HealEvent hEvt && evt.LifebindEvent)
                {
                    var altEvent = evt.AltEvents[i];
                    var count = 0;
                    if (evt.TargetUnit is User)
                    {
                        count = evt.LifebindCount;
                    }
                    else
                    {
                        count = 1;
                    }
                    lifebind.AltHeal[i].Hypo += altEvent.Amount.Raw * Abilities.Lifebind.coef * count;

                }

            }
            foreach (var evt in events)
            {
                if (evt.AbilityName == lifebind.Name)
                {

                    var altEvent = evt.AltEvents[i];
                    var gainRaw = altEvent.Amount.Raw * lifebind.AltHypoIncRawR(i) - (altEvent.Amount.Raw + altEvent.NukeRaw);
                    altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);

                } 
            }
            Console.WriteLine($"Lifebind - Hypo: {lifebind.Heal.Hypo}, True Raw: {lifebind.Heal.Raw}");
        }
        public static bool IsEnkindledEvt(TpEvent evt, User user)
        {

            if (user.HasTalent(Talents.Enkindle.id) && evt.Ability.Spender)
            {

                return true;
            }
            return false;
           
        }



        public static void EnkindleHypo(TpEvent evt, User user)
        {
            if (IsEnkindledEvt(evt, user))
            {

                var enkindle = (Abilities.Enkindle)user.Abilities.Get(Abilities.Enkindle.name);
                Console.WriteLine($"{evt.AbilityName} - {evt.Timestamp}");
                if (evt.IsHealDoneEvent())
                {
                    enkindle.Heal.Hypo += Abilities.Enkindle.coef * evt.Amount.Raw;


                }
                else if (evt.IsDmgDoneEvent())
                {
                    enkindle.Damage.Hypo += Abilities.Enkindle.coef * evt.Amount.Raw;
                }
            }
        }


        public static void AltEnkindle(List<TpEvent> events, User user, int i)
        {
            var enkindle = (Abilities.Enkindle)user.Abilities.Get(Abilities.Enkindle.name);
            if (enkindle.Heal.Raw == 0 && enkindle.Damage.Dmg == 0) return;

            foreach (var evt in events)
            {

                if (evt is TpEvent tEvt && IsEnkindledEvt(evt, user))
                {
                    var altEvent = evt.AltEvents[i];
                    if (evt.IsHealDoneEvent())
                    {
                        enkindle.AltHeal[i].Hypo += Abilities.Enkindle.coef * altEvent.Amount.Raw;

                    }
                    else if (evt.IsDmgDoneEvent())
                    {
                        enkindle.AltDamage[i].Hypo += Abilities.Enkindle.coef * altEvent.Amount.Raw;
                    }
                }
            }
            foreach (var evt in events)
            {
                if (evt.AbilityName == enkindle.Name)
                {
                    var altEvent = evt.AltEvents[i];
                    if (evt.IsHealDoneEvent())
                    {
                        var gainRaw = altEvent.Amount.Raw * enkindle.AltHypoIncRawR(i) - (altEvent.Amount.Raw + altEvent.NukeRaw);
                        Console.WriteLine($"altEvent.Amount.Raw {altEvent.Amount.Raw} enkindle.AltHypoTrueRawR(i) {enkindle.AltHypoIncRawR(i)}");
                        altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);

                    }
                    else if (evt.IsDmgDoneEvent())
                    {
                        var gainDmg = altEvent.Amount.Raw * enkindle.AltHypoIncDmgR(i) - (altEvent.Amount.Raw + altEvent.NukeRaw);
                        altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainDmg, i);
                    }


                }
            }
            Console.WriteLine($"enkindle - Hypo Raw: {enkindle.Heal.Hypo}, True Raw: {enkindle.Heal.Raw}");
            Console.WriteLine($"enkindle - Hypo Dmg: {enkindle.Damage.Hypo}, True Raw: {enkindle.Damage.Dmg}");
        }
    }
}
