using Beaversims.Core.Common;
using Beaversims.Core.Shared.Abilities;
using Beaversims.Core.Specs.Druid.Resto.Abilities;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Druid.Resto
{
    internal class HCGM
    {

        public static bool SetInstaTick(Event evt, bool nextRejuvInsta)
        {
            if (evt is BuffEvent bEvt)
            {
                if (bEvt.BuffApplyEvent || bEvt.BuffRefreshEvent)
                {
                    if (evt.AbilityId == Abilities.Rejuvenation.buffId || evt.AbilityId == Abilities.RejuvenationGermination.buffId)
                    {
                        return true;
                    }
                }
            }
            if (evt.IsHealDoneEvent() && nextRejuvInsta && (evt.AbilityName == Abilities.Rejuvenation.name || evt.AbilityName == Abilities.RejuvenationGermination.name))
            {
                evt.NonScInstaTick = true;
                return false;
            }
            return nextRejuvInsta;
        }

        public static void GatherData(Event evt, User user)
        {
            if (evt is BuffEvent bEvt)
            {
                var convoke = (Abilities.ConvokeTheSpirits)user.Abilities.Get(Abilities.ConvokeTheSpirits.name);
                if ((bEvt.BuffApplyEvent || bEvt.BuffRefreshEvent) && convoke.Abilities.Contains(evt.AbilityName) && evt.SourceUnit is User)
                {
                    var ability = (Abilities.RestoAbility)user.Abilities.Get(evt.AbilityName);  
                    ability.ApplyyRefreshCount++;
                }
            }
            if (evt.IsHealDoneEvent() && evt.SourceUnit is User && evt is HealEvent hEvt)
            {
                if (evt.AbilityName == Abilities.Regrowth.name)
                {
                    var regrowth = (Abilities.Regrowth)user.Abilities.Get(evt.AbilityName);
                    if (!hEvt.Tick)
                    {
                        regrowth.NonSumDirectCount++;
                    }
                }
                if (evt.AbilityName == Abilities.EmbraceOfTheDream.name)
                {
                    foreach (var sourceAbility in Abilities.EmbraceOfTheDream.abilitySources)
                    {
                        var _sourceAbility = (Abilities.RestoAbility)user.Abilities.Get(sourceAbility);
                        if (evt.TargetUnit.HasBuff(_sourceAbility.BuffId))
                        {
                            _sourceAbility.EotdCount++;
                        }
                    }
                  
                }
            }
            if (evt is CastEvent cEvt)
            {
                if (Talents.SoulOfTheForest.abilities.Contains(evt.AbilityName) && user.HasBuff(Talents.SoulOfTheForest.buffId))
                {
                    var ability = (Abilities.RestoAbility)(cEvt.Ability);
                    ability.SotfCount++;
                    Console.WriteLine(evt.Timestamp);
                }
            }
        }

        public static double SotfPureHPC(double amountTotal, int castsTotal, int sotfCasts)
        {
            return amountTotal / (castsTotal - sotfCasts + (sotfCasts * (1 + Talents.SoulOfTheForest.coef)));
        }

        public static void ModifyHCGM(User user, Fight fight)
        {
            var convoke = (Abilities.ConvokeTheSpirits)user.Abilities.Get(Abilities.ConvokeTheSpirits.name);
            var regrowth = (Abilities.Regrowth)user.Abilities.Get(Abilities.Regrowth.name);
            var rejuv = (Abilities.Rejuvenation)user.Abilities.Get(Abilities.Rejuvenation.name);
            var germ = (Abilities.RejuvenationGermination)user.Abilities.Get(Abilities.RejuvenationGermination.name);
            var wrath = (Abilities.Wrath)user.Abilities.Get(Abilities.Wrath.name);
            var eotd = (Abilities.EmbraceOfTheDream)user.Abilities.Get(Abilities.EmbraceOfTheDream.name);   
            var zeroCIMDummy = user.Abilities.Get(Shared.Abilities.ZeroCIMDummy.name);

            // Tackling Convoke, Power of the archdruid and Sylvan Beckoning here.
            var regrowthQIM = (double)regrowth.Casts / (regrowth.NonSumDirectCount);
            regrowth.CIMSources.Add(new CIMSource(regrowth.Name, regrowthQIM));
            regrowth.CIMSources.Add(new CIMSource(zeroCIMDummy.Name, 1 - regrowthQIM));
            var wrathQIM = (double)wrath.Casts / wrath.Damage.Count;
            wrath.CIMSources.Add(new CIMSource(wrath.Name, wrathQIM));
            wrath.CIMSources.Add(new CIMSource(zeroCIMDummy.Name, 1 - wrathQIM));
            var rejuvCastSourceCount = rejuv.ApplyyRefreshCount + germ.ApplyyRefreshCount;
            var rejuvQIM = ((double)rejuv.Casts + (double)germ.Casts) / rejuvCastSourceCount;
            rejuv.CIMSources.Add(new CIMSource(rejuv.Name, rejuvQIM));
            rejuv.CIMSources.Add(new CIMSource(zeroCIMDummy.Name, 1 - rejuvQIM));
            germ.CIMSources.Add(new CIMSource(germ.Name, rejuvQIM));
            germ.CIMSources.Add(new CIMSource(zeroCIMDummy.Name, 1 - rejuvQIM));

            // Soul of the Forest
            // Fucked until midnight since we're not taking wild growth.
            // TODO Test after midnight is out.
            if (user.HasTalent(Talents.SoulOfTheForest.id))
            {
                var rejuvHpcPure = SotfPureHPC((rejuv.Heal.Raw + germ.Heal.Raw) * rejuvQIM, rejuvCastSourceCount, rejuv.SotfCount + germ.SotfCount);
                var rejuvSotfMod = rejuvHpcPure * rejuvCastSourceCount / ((rejuv.Heal.Raw + germ.Heal.Raw) * rejuvQIM);
                rejuv.HealHCGM *= rejuvSotfMod;
                germ.HealHCGM *= rejuvSotfMod;
                var regrowthHpcPure = SotfPureHPC(regrowth.Heal.Raw * regrowthQIM, regrowth.NonSumDirectCount, regrowth.SotfCount);
                regrowth.HealHCGM *= regrowthHpcPure * regrowth.NonSumDirectCount / (regrowth.Heal.Raw * regrowthQIM);
                Console.WriteLine($" Rejuv sotfCount: {rejuv.SotfCount}");
                Console.WriteLine($" Regrowth sotfCount: {regrowth.SotfCount}");
                Console.WriteLine($" Rejuv SotfMod: {rejuvSotfMod}");
                Console.WriteLine($" Regrowth SotfMod: {regrowthHpcPure * regrowth.NonSumDirectCount / (regrowth.Heal.Raw * regrowthQIM)}");

            }
            Console.WriteLine($"HCGM: Regrowth - {regrowth.HealHCGM} Rejuv - {rejuv.HealHCGM} Wrath - {wrath.DmgHCGM}");
            //Console.WriteLine($"Tick Count: {regrowth.Heal.Tick.Count} All: {regrowth.Heal.Count} Non Tick: {regrowth.Heal.Count - regrowth.Heal.Tick.Count} Casts: {regrowth.Casts}");
            Console.WriteLine($"{rejuv.Name}: Count: {rejuv.ApplyyRefreshCount + germ.ApplyyRefreshCount}, Casts: {rejuv.Casts}");

            // Embrace the dream
            var eotdRejuvRatio = (double)(rejuv.EotdCount + germ.EotdCount) / (regrowth.EotdCount + rejuv.EotdCount + germ.EotdCount);
            eotd.CIMSources.Add(new CIMSource(rejuv.Name, eotdRejuvRatio));
            eotd.CIMSources.Add(new CIMSource(regrowth.Name, 1 - eotdRejuvRatio));
            Console.WriteLine($" EotD Rejuv source: {eotdRejuvRatio} Regrowth: {1 - eotdRejuvRatio}");




        }



    }
}
