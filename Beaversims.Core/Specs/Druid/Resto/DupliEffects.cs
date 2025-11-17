using Beaversims.Core;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Beaversims.Core.Specs.Druid.Resto
{
    internal class DupliEffects
    {
        public static bool IsSymbRelEvt(HealEvent evt, User user)
        {   
            // Checking for selfBuffId didnt work for some reason. This works just as fine.
            if (user.HasTalent(Talents.SymbioticRelationship.id) 
                && evt.SourceUnit is User 
                && (evt.TargetUnit is User || evt.TargetUnit.HasBuff(Abilities.SymbioticRelationship.targetBuffId))
                && evt.Ability.ClassAbility
                )
            {
                return true;
            }
            return false;
        }


        public static void SymbRelHypo(HealEvent evt, User user)
        {
            if (IsSymbRelEvt(evt, user))
            {

                var symRel = (Abilities.SymbioticRelationship)user.Abilities.Get(Abilities.SymbioticRelationship.name);
                var coef = 0.0;
                if (evt.TargetUnit is User)
                {
                    coef = Talents.SymbioticRelationship.inCoef;
                }
                else
                {
                    coef = Talents.SymbioticRelationship.outCoef;
                }
                Console.WriteLine(evt.TargetUnit.Name);

                symRel.Heal.Hypo += evt.Amount.Raw * coef;
                evt.IsSymbRelEvent = true;
                
            }

        }

        public static void AltSymbRel(List<TpEvent> events, User user, int i)
        {
            if (user.HasTalent(Talents.SymbioticRelationship.id))
            {
                var symRel = (Abilities.SymbioticRelationship)user.Abilities.Get(Abilities.SymbioticRelationship.name);

                foreach (var evt in events)
                {

                    if (evt.IsSymbRelEvent)
                    {
                        var coef = 0.0;
                        if (evt.TargetUnit is User)
                        {
                            coef = Talents.SymbioticRelationship.inCoef;
                        }
                        else
                        {
                            coef = Talents.SymbioticRelationship.outCoef;
                        }

                        var altEvent = evt.AltEvents[i];
                        symRel.AltHeal[i].Hypo += altEvent.Amount.Raw * coef;

                    }

                }
                foreach (var evt in events)
                {
                    if (evt.AbilityName == symRel.Name)
                    {

                        var altEvent = evt.AltEvents[i];
                        var gainRaw = altEvent.Amount.Raw * symRel.AltHypoTrueRawR(i) - altEvent.Amount.Raw;
                        altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);

                    }
                }
                Console.WriteLine($"Symb Rel - HypoRaw: {symRel.Heal.Hypo} True Raw: {symRel.Heal.Raw}");
            }

        }
        // Hypo is a bit low for doc but should be fine.

        public static bool IsDocEvt(DamageEvent evt, User user)
        {
            if (user.HasTalent(Talents.DreamOfCenarius.id)
                && evt.SourceUnit is User
                && Abilities.DreamOfCenarius.abilityCoefs.ContainsKey(evt.AbilityName)
                )
            {
                return true;
            }
            return false;
        }


        public static void DocHypo(DamageEvent evt, User user)
        {

            if (!IsDocEvt(evt, user)) return;
            var doc = (Abilities.DreamOfCenarius)user.Abilities.Get(Abilities.DreamOfCenarius.name);
            var coef = Abilities.DreamOfCenarius.abilityCoefs[evt.AbilityName];
            if (evt.UserHasHotw)
            {
                coef *= 1 + Abilities.DreamOfCenarius.hotwInc;
            }
            //Console.WriteLine($"{evt.AbilityName}: {coef}");
            //Console.WriteLine(evt.TargetUnit.Name);
            doc.Heal.Hypo += evt.Amount.Raw * coef;
        }

        public static void AltDoc(List<TpEvent> events, User user, int i)
        {

            if (!user.HasTalent(Talents.DreamOfCenarius.id)) return;

            var doc = (Abilities.DreamOfCenarius)user.Abilities.Get(Abilities.DreamOfCenarius.name);

                foreach (var evt in events)
                {

                    if (evt is DamageEvent dEvt && IsDocEvt(dEvt, user))
                    {
                        var coef = Abilities.DreamOfCenarius.abilityCoefs[evt.AbilityName];
                        if (evt.UserHasHotw)
                        {
                            coef *= 1 + Abilities.DreamOfCenarius.hotwInc;
                        }

                    var altEvent = evt.AltEvents[i];
                        doc.AltHeal[i].Hypo += altEvent.Amount.Raw * coef;
                        

                    }

                }
                foreach (var evt in events)
                {
                    if (evt.AbilityName == doc.Name)
                    {

                        var altEvent = evt.AltEvents[i];
                        var gainRaw = altEvent.Amount.Raw * doc.AltHypoTrueRawR(i) - altEvent.Amount.Raw;
                        altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);

                    }
                }
                Console.WriteLine($"doc - HypoRaw: {doc.Heal.Hypo} True Raw: {doc.Heal.Raw}");
            }

        
    }
}
