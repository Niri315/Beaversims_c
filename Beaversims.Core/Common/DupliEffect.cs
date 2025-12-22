using Beaversims.Core.Specs.Priest.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    internal abstract class DupliEffect
    {
        public Ability DupliAbility { get; set; }
        public Talent? Talent { get; set; }
        public abstract bool IsProcEvt(TpEvent evt, User user);
        public abstract double HypoFormula(TpEvent evt, User user);

        private bool CanProc(TpEvent evt, User user)
        {
            if (evt.Ability.CanDupli &&
               evt.AbilityName != DupliAbility.Name)
                return true;
            return false;
        }

        public void StoreHypo(TpEvent evt, User user)
        {

            if (CanProc(evt, user) && IsProcEvt(evt, user))
            {
                evt.ActiveDupliEffects.Add(this);
                var amount = evt.Amount.Raw * HypoFormula(evt, user);
                var det = DupliAbility.DupliEffectType;
                if (det == DupliEffectType.Reverse)
                {
                    if (evt.IsDmgDoneEvent())
                    {
                        DupliAbility.Heal.Hypo += amount;
                    }
                    if (evt.IsHealDoneEvent())
                    {
                        DupliAbility.Damage.Hypo += amount;
                    }
                }
                else if (det == DupliEffectType.Both)
                {
                    if (evt.IsDmgDoneEvent())
                    {
                        DupliAbility.Damage.Hypo += amount;
                    }
                    if (evt.IsHealDoneEvent())
                    {
                        DupliAbility.Heal.Hypo += amount;
                    }
                }
                else
                {
                    if (det == DupliEffectType.Heal)
                    {
                        DupliAbility.Heal.Hypo += amount;
                    }
                    if (det == DupliEffectType.Damage)
                    {
                        DupliAbility.Damage.Hypo += amount;
                    }
                }
            }
        }

        public void ApplyAlt(List<TpEvent> tpEvents, User user, int i) 
        {
            DupliAbility.AltDamage[i].Dmg = 0;
            DupliAbility.AltHeal[i].Raw = 0;
            if (DupliAbility.DupliEffectType == DupliEffectType.None)
            {
                throw new InvalidOperationException($"DupliEffectType not set for {DupliAbility.Name}.");
            }
            var det = DupliAbility.DupliEffectType;


            foreach (var evt in tpEvents)
            {
                if (evt.ActiveDupliEffects.Contains(this))
                {
                    var altEvent = evt.AltEvents[i];
                    var amount = altEvent.Amount.Raw * HypoFormula(evt, user);
                    if (det == DupliEffectType.Reverse)
                    {
                        if (evt.IsDmgDoneEvent())
                        {
                            DupliAbility.AltHeal[i].Hypo += amount;
                        }
                        if (evt.IsHealDoneEvent())
                        {
                            DupliAbility.AltDamage[i].Hypo += amount;
                        }
                    }
                    else if (det == DupliEffectType.Both)
                    {
                        if (evt.IsDmgDoneEvent())
                        {
                            DupliAbility.AltDamage[i].Hypo += amount;
                        }
                        if (evt.IsHealDoneEvent())
                        {
                            DupliAbility.AltHeal[i].Hypo += amount;
                        }
                    }
                    else
                    {
                        if (det == DupliEffectType.Heal)
                        {
                            DupliAbility.AltHeal[i].Hypo += amount;
                        }
                        if (det == DupliEffectType.Damage)
                        {
                            DupliAbility.AltDamage[i].Hypo += amount;
                        }
                    }
                  
                }
                if (evt.AbilityName == DupliAbility.Name)
                {
                    var altEvent = evt.AltEvents[i];
                    if (evt.IsDmgDoneEvent())
                    {
                        DupliAbility.AltDamage[i].Dmg += altEvent.Amount.Raw;
                    }
                    if (evt.IsHealDoneEvent())
                    {
                      
                        DupliAbility.AltHeal[i].Raw += altEvent.Amount.Raw;
                    }
                }
           
            }



            foreach (var evt in tpEvents)
            {
                if (evt.AbilityName == DupliAbility.Name)
                {
                    var altEvent = evt.AltEvents[i];
                    double altHypoIncCoef = 0;
                    double hypoTrueCoef = 0;

                    if (evt.IsDmgDoneEvent())
                    {
                        altHypoIncCoef = DupliAbility.AltHypoIncDmgR(i);
                        hypoTrueCoef = DupliAbility.HypoTrueDmgR();

                    }
                    else if (evt.IsHealDoneEvent())
                    {
                        altHypoIncCoef = DupliAbility.AltHypoIncRawR(i);
                        hypoTrueCoef = DupliAbility.HypoTrueRawR();

                        //test
                        //hypoTrueCoef = (DupliAbility.Heal.Raw / DupliAbility.Heal.Hypo) * (DupliAbility.AltHeal[i].Raw / DupliAbility.Heal.Raw);
                        //altHypoIncCoef = DupliAbility.AltHeal[i].Hypo / DupliAbility.Heal.Hypo;
                        //Console.WriteLine($"hypoTrueCoef: {hypoTrueCoef}");
                    }

                    //var gainRaw = altEvent.Amount.Raw * altHypoIncCoef - (altEvent.Amount.Raw); // + altEvent.NukeRaw); // OBS ! NEED REVIEW


                    //var gainRaw = hypoTrueCoef * (altEvent.Amount.Raw * altHypoIncCoef - (altEvent.Amount.Raw));
                    var gainRaw = hypoTrueCoef * altEvent.Amount.Raw * (altHypoIncCoef - 1);

                    altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);

                }
            }
            Console.WriteLine($"{DupliAbility.Name} Dupli Effect - Hypo Raw: {DupliAbility.Heal.Hypo}, True Raw: {DupliAbility.Heal.Raw}");
            Console.WriteLine($"DupliAbility.AltHeal[i].Raw: {DupliAbility.AltHeal[i].Raw}, True Raw: {DupliAbility.Heal.Raw}");
        }

        public DupliEffect(Ability ability, Talent? talent=null)
        {
            DupliAbility = ability;
            Talent = talent;
        }
    }
}
