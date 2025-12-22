using Beaversims.Core;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Beaversims.Core.Specs.Druid.Resto.DupliEffects
{
    internal class SymbRel : DupliEffect
    {

        public override bool IsProcEvt(TpEvent evt, User user)
        {
            // Checking for selfBuffId didnt work for some reason. This works just as fine.
            if (
                 evt.SourceUnit is User
                && (evt.TargetUnit is User || evt.TargetUnit.HasBuff(Abilities.SymbioticRelationship.targetBuffId))
                && evt.Ability.ClassAbility
                && evt.Ability.CanDupli
                && !evt.AbsorbAbility
                && evt.AbilityName != Abilities.SymbioticRelationship.name
                )
            {
                return true;
            }
            return false;
        }

        public override double HypoFormula(TpEvent evt, User user)
        {
            if (evt.TargetUnit is User)
            {
                return Talents.SymbioticRelationship.inCoef;
            }
            else
            {
                return Talents.SymbioticRelationship.outCoef;
            }
        }

        public SymbRel(Ability ability) : base(ability)
        {
        }
    }
    internal class DoC : DupliEffect
    {

        public override bool IsProcEvt(TpEvent evt, User user)
        {
            if (
                           evt.SourceUnit is User
                           && Abilities.DreamOfCenarius.abilityCoefs.ContainsKey(evt.AbilityName)
                           )
            {
                return true;
            }
            return false;
        }

        public override double HypoFormula(TpEvent evt, User user)
        {
            var doc = (Abilities.DreamOfCenarius)user.Abilities.Get(Abilities.DreamOfCenarius.name);
            var coef = Abilities.DreamOfCenarius.abilityCoefs[evt.AbilityName];
            if (evt.UserHasHotw)
            {
                coef *= 1 + Abilities.DreamOfCenarius.hotwInc;
            }
            //Console.WriteLine($"{evt.AbilityName}: {coef}");
            //Console.WriteLine(evt.TargetUnit.Name);
            return coef;
        }

        public DoC(Ability ability) : base(ability)
        {
        }
    }

    internal class DupliEffects
    {
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
                        var gainRaw = altEvent.Amount.Raw * doc.AltHypoIncRawR(i) - (altEvent.Amount.Raw + altEvent.NukeRaw);
                        altEvent.Amount.UpdateAltGainsFromEvtData(evt, gainRaw, i);

                    }
                }
                Console.WriteLine($"doc - HypoRaw: {doc.Heal.Hypo} True Raw: {doc.Heal.Raw}");
            }

        
    }
}
