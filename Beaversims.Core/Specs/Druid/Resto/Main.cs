
using Beaversims.Core.Common;
using Beaversims.Core.Shared;
using Beaversims.Core.Sim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Beaversims.Core.Specs.Druid.Resto
{
    internal class Main

    {


        public static void HasteDepMasteryGains(HealEvent evt, User user, int i)
        {
            if (evt.MasteryEffectiveness > 0)
            {
                var mastery = (Mastery)evt.UserStats.Get(StatName.Mastery);
                var haste = (Haste)evt.UserStats.Get(StatName.Haste);
                var altEvent = evt.AltEvents[i];
                var pureAmount = altEvent.Amount.Raw / (1 + (mastery.TrueEff() * evt.MasteryEffectiveness / (mastery.PercentRate * 100)));
                var masteryAmountVal = altEvent.Amount.Raw - pureAmount;
                var testAmount = pureAmount * (1 + ((mastery.TrueEff() * evt.MasteryEffWOQIM) / (mastery.PercentRate * 100)));
                var QIMRelAmount = altEvent.Amount.Raw - testAmount;
                foreach (var qimAbility in evt.QIMBuffSources)
                {
                    // Ignoring value already present in haste stat gains.
                    if (!evt.QIMBuffSources.Contains(evt.Ability))
                    {
                        Shared.StatGains.SecondaryAltAmount(evt, haste, i, amount: QIMRelAmount, mod: qimAbility.TrueQIM(user, i));
                    }
                }
            }
        }

        public static void TrackHotw(Event evt, User user)
        {
            if (user.HasBuff(Abilities.HeartOfTheWild.buffId))
            {
                evt.UserHasHotw = true;

            }
        }
        public static void TrackCIMBuffs(Event evt, User user)
        {
            if (evt.TargetUnit.HasBuff(Abilities.Regrowth.buffId))
            {
                var regrowth = (Abilities.Regrowth)user.Abilities.Get(Abilities.Regrowth.name);
                evt.QIMBuffSources.Add(regrowth);
                evt.TargetHasRegrowth = true;
               
            }
            if (evt.TargetUnit.HasBuff(Abilities.Rejuvenation.buffId))
            {
                var rejuv = (Abilities.Rejuvenation)user.Abilities.Get(Abilities.Rejuvenation.name);
                evt.QIMBuffSources.Add(rejuv);
                evt.TargetHasRejuv = true;
            }
            if (evt.TargetUnit.HasBuff(Abilities.RejuvenationGermination.buffId))
            {
                var germ = (Abilities.RejuvenationGermination)user.Abilities.Get(Abilities.RejuvenationGermination.name);
                evt.QIMBuffSources.Add(germ);
                evt.TargetHasGerms = true;
            }
            if (evt.TargetHasRegrowth || evt.TargetHasRejuv || evt.TargetHasGerms)
            {
                evt.TargetHasQIMBuff = true;
            }
        }
        public static void TrackAbundance(Event evt, User user)
        {
            var abundance = user.GetBuff(Talents.Abundance.buffId);
            if (abundance != null)
            {
                evt.AbundanceStacks = abundance.Stacks;
            }
        }

        public static void RegrowthCritGains(TpEvent evt, User user, int i)
        {
            // Todo doesnt take sim effects fully in account.
            if (evt.AbilityName == Abilities.Regrowth.name)
            {
                //var crit = (Crit)evt.UserStats.Get(StatName.Crit);
                var crit = (Crit)evt.AltEvents[i].UserStats.Get(StatName.Crit);
                var critChance = crit.TrueEff() / (crit.PercentRate * 100);
                if (evt.Tick && user.HasTalent(Talents.StrategicInfusion.id))
                {
                    critChance += Talents.StrategicInfusion.coef;
                }
                else
                {
                    if (evt.TargetHasRegrowth && user.HasTalent(Talents.ImprovedRegrowth.id))
                    {
                        critChance += Talents.ImprovedRegrowth.coef;
                    }
                }
                critChance += Math.Min(evt.AbundanceStacks * Talents.Abundance.coef, Talents.Abundance.cap);
                if (critChance < 1.0)
                {
                    StatGains.CritGainsHeal((HealEvent)evt, user, i);
                }
                else
                {
                    //Log
                }

            }
        }



        private static void SpecInit(User user)
        {
            // Lycara doesnt show up as init buff. If assumption is wrong, it will be corrected as soon as form is changed.
            user.AddBuff(Data.StatBuffs.LycarasTeachingsNoForm.name, Data.StatBuffs.LycarasTeachingsNoForm.id, user, 1, 0.0);
            user.CIMDepMastIncScalers =  user.Abilities
                .OfType<Abilities.RestoAbility>()
                .Where(a => a.CIMDepMastIncScaler && a.Casts > 0)
                .ToList();
            if (user.HasTalent(Talents.SymbioticRelationship.id)) 
            {
                user.DupliEffects.Add(new DupliEffects.SymbRel((Abilities.SymbioticRelationship)user.Abilities.Get(Abilities.SymbioticRelationship.name)));
            }
            var doc = user.Abilities.Get(Abilities.DreamOfCenarius.name);
            if (doc.Heal.Raw > 0)
            {
                user.DupliEffects.Add(new DupliEffects.DoC(doc));

            }

        }

        public static void SpecMain(List<Event> events, UnitRepo allUnits, Fight fight, int iterationCount)
        {


            //StatGainMathTest();
            //return;
            var user = allUnits.GetUser();
            var prepullRefStats = user.RefStats.Clone();
            SpecInit(user);
            MasteryTracker.InitHarmonyBuffs(allUnits, user);

            var nextRejuvInstaTick = false;
      
            var statLogger = new Logger("StatTracker", fight, user.Id.TypeId);
            var refStatLogger = new Logger("Ref Stat Tracker", fight, user.Id.TypeId);

            foreach (Event evt in events)
            {
                // Loop for tracking buffs and collecting data.
                BuffTracker.TrackBuffs(evt, allUnits, statLogger, refStatLogger);
                evt.CreateAltEvents(user, evt);
                CastProcessor.ProcessCast(evt, user);
                MasteryTracker.SetMasteryEff(evt, user);
                TrackCIMBuffs(evt, user);
                TrackAbundance(evt, user);
                TrackHotw(evt, user);
                HCGM.GatherData(evt, user);
                nextRejuvInstaTick = HCGM.SetInstaTick(evt, nextRejuvInstaTick);

                if (evt is TpEvent tEvt)
                {
                    ProcessEvents.OriginalTotals(tEvt, user);
                    user.StoreSharedDupliHypos(tEvt, user);
                    user.StoreDupliHypos(tEvt, user);
                    if (evt is HealEvent hEvt)
                    {
                        //DupliEffects.SymbRelHypo(hEvt, user);
                    }
                    if (evt is DamageEvent dEvt)
                    {
                        //DupliEffects.DocHypo(dEvt, user);
                    }

                }
            }
            Utils.CleanUp(allUnits, events); // To avoid accidental usage + remove events we no longer need.
            HCGM.ModifyHCGM(user, fight);
            Shared.CIM.SetCIM(user);
            Utils.RemoveImpurities(events, user);



            foreach (var ability in user.Abilities)
            {
                Console.WriteLine($"{ability.Name}: CIM: {ability.CIM}, Rest rel Ratio: {ability.RestRelCIMRatio}, CTG: {ability.CTGain}");
            }

            var degree = Environment.ProcessorCount;
            Parallel.For(0, user.AltGearSets.Count, new ParallelOptions { MaxDegreeOfParallelism = degree }, i =>
            {


                var altGearSet = user.AltGearSets[i];
                var altEventList = new List<Event>(events);
                altGearSet.AltEventList = altEventList;
                altGearSet.ProcEvents = Sim.EffectsSim.SimEffects(altEventList, user, fight, i, iterationCount);

                List<TpEvent> tpEvents = altGearSet.AltEventList.OfType<TpEvent>().Where(e => e.SimEvent == false).ToList();
                //Making sure we don't include on use dmg/heal events from sims.
                foreach (TpEvent evt in tpEvents)
                {
                    // Loop for setting gains.
                    var altEvent = evt.AltEvents[i];
                    evt.AltEvents[i].Amount = evt.Amount.Clone();
                    StatGains.AutoStatGains(evt, user, i);
                    if (evt.IsHealDoneEvent() && evt is HealEvent hEvt)
                    {
                        RegrowthCritGains(evt, user, i);
                        if (evt.Ability.ScalesWith(StatName.Mastery))
                        {
                            // OBS ! Mastery gains must be LAST
                            // We use part of gainRaw from it for full allocs.
                            MasteryTracker.MasteryGains((HealEvent)evt, user, i);
                            HasteDepMasteryGains(hEvt, user, i);
                        }
                    }
                }
                //DupliEffects.AltDoc(tpEvents, user, i);
                //DupliEffects.AltSymbRel(tpEvents, user, i);
                user.ApplyDupliAlts(tpEvents, user, i);

                // Only summer and leech will react to this.
                tpEvents = altGearSet.AltEventList.OfType<TpEvent>().ToList(); // Updating here to include non proc sim events
                Shared.ProcessEvents.AddProcEvents(tpEvents, altGearSet.ProcEvents, iterationCount, user);
                user.ApplyShareDupliAlts(tpEvents, user, i);


                Shared.ProcessEvents.StoreTotals(tpEvents, user, i, fight);
                
            });
        }
    }
}
