using Beaversims.Core.Common;
using Beaversims.Core.Shared.Abilities;
using Beaversims.Core.Specs.Evoker.Pres.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Evoker.Pres
{
    // TODO
    // Might be better to just hard set spenders to 1 CIM as well as 1 QIM.
    // A lot simpler and I dont think we're getting much if any accuracy out of over complicating it...


    // Stasis spells we care about for CIM.
    //Echo.name,
    //Reversion.name,
    //EmeraldBlossom.name,

    internal class HCGM
    {
        public const int essenceBurstId = 369299;
        public const int leapingFlamesBuffId = 370901;


 


        public static void GatherData(Event evt, User user)
        {
            if (evt is BuffEvent bEvt)
            {
                //if (evt.AbilityName == "Lifebind")
                //{
                //    Console.WriteLine(evt.Timestamp.ToString());
                //}
                if ((bEvt.BuffApplyEvent || bEvt.BuffRefreshEvent) && (evt.AbilityName == Abilities.Reversion.name || evt.AbilityName == Abilities.ReversionEcho.name) && evt.SourceUnit is User)
                {
                    var _ability = (Abilities.PresAbility)user.Abilities.Get(evt.AbilityName);
                    _ability.ApplyyRefreshCount++;
                }
                if (evt.AbilityId == Abilities.Lifebind.buffId && evt.SourceUnit is User)
                {
                    if (bEvt.BuffApplyEvent)
                    {
                        //Console.WriteLine(evt.Timestamp);
                        //Console.WriteLine(evt.AbilityName);
                        user.LifebindCount++;
                    }
                    else if (bEvt.BuffRemoveEvent)
                    {
                        user.LifebindCount = 0;
                    }
                    
                }
            }
            else if (evt is CastEvent cEvt)
            {

                if (user.HasBuff(essenceBurstId) && evt.Ability.Spender)
                {
                    var _ability = (Abilities.PresAbility)evt.Ability;
                    _ability.EssenceBurstCount++;
                }
                if (user.HasBuff(Abilities.Stasis.storeBuffId) && Abilities.Stasis.allowedSpells.Contains(evt.Ability.Name))
                {
                    if (user.StasisStore.Count >= 3)
                    {
                        Console.WriteLine($"Stasis Store full: {evt.AbilityName}");
                    }
                    else
                    {
                        user.StasisStore.Add(evt.AbilityName);
                    }
                }
                if (evt.AbilityId == Abilities.Stasis.releaseCastId)
                {
                    foreach (var ability in user.StasisStore)
                    {
                        var _ability = (Abilities.PresAbility)user.Abilities.Get(ability);
                        _ability.StasisCount++;
                        user.StasisStore = [];
                    }
                }

                if (evt.AbilityName == Abilities.FireBreath.name && user.HasTalent(Talents.LeapingFlames.id))
                {
                    var fb = (Abilities.FireBreath)user.Abilities.Get(Abilities.FireBreath.name);
                    user.LeapingFlamesLevel = cEvt.EmpCastLevel;
                }
                if ((evt.AbilityName == Abilities.LivingFlame.name || evt.AbilityName == Abilities.ChronoFlames.name) && user.HasBuff(Talents.LeapingFlames.buffId))
                {
                    var fb = (Abilities.FireBreath)user.Abilities.Get(Abilities.FireBreath.name);
                    fb.leapingFlamesCount += user.LeapingFlamesLevel;
                    user.LeapingFlamesLevel = 0;
                }
            }
            evt.LifebindCount = user.LifebindCount;
         
        }


        public static Tuple<double, double>? GetEchoSourceRatios(User user, Abilities.Echo echo, Abilities.TemporalAnomaly ta)
        {
            // [0] hardcast echo
            // [1] TA
            var taEchoCount = 0;
            if (user.HasTalent(Talents.ResonatingSphere.id))
            {
                taEchoCount += ta.Casts * Talents.ResonatingSphere.echoCount;
            }
            if (echo.Casts > 0 || taEchoCount > 0)
            {
                var hcEchoSourceRatio = (double)echo.Casts / (echo.Casts + (taEchoCount * Talents.ResonatingSphere.coef));
                return new Tuple<double, double>(hcEchoSourceRatio, 1 - hcEchoSourceRatio);
            }
            return null;
        }

        public static void ModifyCIMyHCGM(User user, Fight fight)
        {


            var essenceRegenDummy = (Abilities.EssenceRegenDummy)user.Abilities.Get(Abilities.EssenceRegenDummy.name);

            var stasis = (Abilities.Stasis)user.Abilities.Get(Abilities.Stasis.name);
            var blossom = (Abilities.EmeraldBlossom)user.Abilities.Get(Abilities.EmeraldBlossom.name);
            var zeroCIM = user.Abilities.Get(Shared.Abilities.ZeroCIMDummy.name);
            var panacea = (Abilities.Panacea)user.Abilities.Get(Abilities.Panacea.name);
            var lgf = (Abilities.LifeGiversFlame)user.Abilities.Get(Abilities.LifeGiversFlame.name);
            var lifebind = (Abilities.Lifebind)user.Abilities.Get(Abilities.Lifebind.name);
            var ve = (Abilities.VerdantEmbrace)user.Abilities.Get(Abilities.VerdantEmbrace.name);
            var fb = (Abilities.FireBreath)user.Abilities.Get(Abilities.FireBreath.name);
            var lf = (Abilities.LivingFlame)user.Abilities.Get(Abilities.LivingFlame.name);
            var cf = (Abilities.ChronoFlame)user.Abilities.Get(Abilities.ChronoFlame.name);
            var cfCast = (Abilities.ChronoFlames)user.Abilities.Get(Abilities.ChronoFlames.name);
            var echo = (Abilities.Echo)user.Abilities.Get(Abilities.Echo.name);
            var ta = (Abilities.TemporalAnomaly)user.Abilities.Get(Abilities.TemporalAnomaly.name);
            var rev = (Abilities.Reversion)user.Abilities.Get(Abilities.Reversion.name);
            var revEcho = (Abilities.ReversionEcho)user.Abilities.Get(Abilities.ReversionEcho.name);
            List<Abilities.PresAbility> echoEffects = user.Abilities
                .OfType<Abilities.PresAbility>()
                .Where(a => a.EchoEffect)
                .ToList();
            List<Abilities.PresAbility> spenders = user.Abilities
                .OfType<Abilities.PresAbility>()
                .Where(a => a.Spender)
                .ToList();
            Ability lfcfSource;
            if (user.Spec is PresChronowarden)
            {
                //lf.CIMSources.Add(new CIMSource(Abilities.ChronoFlames.name, 1.0));
                lfcfSource = cfCast;
            }
            else
            {
                lfcfSource = lf;
            }


            //Living / Chrono flames
            var lfcfCount = lf.Heal.Count + lf.Damage.Count;  // Only using living flame.
            var leapingFlamesRatio = (double)fb.leapingFlamesCount / lfcfCount;
            Console.WriteLine($"leapflames count: {fb.leapingFlamesCount} ratio {leapingFlamesRatio}, lfcfCount {lfcfCount}");
            lf.CIMSources.Add(new CIMSource(lfcfSource.Name, 1 - leapingFlamesRatio));
            lf.CIMSources.Add(new CIMSource(fb.Name, leapingFlamesRatio));
            cf.CIMSources.Add(new CIMSource(lfcfSource.Name, 1 - leapingFlamesRatio));
            cf.CIMSources.Add(new CIMSource(fb.Name, leapingFlamesRatio));

            //Essence burst source ratios.

            var revCount = rev.ApplyyRefreshCount;
            var revEchoCount = revEcho.ApplyyRefreshCount;

            var ebHypoCount = lfcfCount + revCount + revEchoCount;

            var lfcfRatio = lfcfCount / ebHypoCount;
            Console.WriteLine($"lfcfRatio{lfcfRatio}, ebHypoCount{ebHypoCount} , lfcfCount {lfcfCount}");
            var revRatio = revCount / ebHypoCount;

            var echoSourceRatios = GetEchoSourceRatios(user, echo, ta);

            double hcEchoRatioSpenderSubRatio = 0;
            double taEchoRatioSpenderSubRatio = 0;


            if (echoSourceRatios != null)
            {
                double hcEchoRatio = echoSourceRatios.Item1;
                double taEchoRatio = echoSourceRatios.Item2;

                foreach (var echoEffect in echoEffects)
                {

                    echoEffect.CIMSources.Add(new CIMSource(echo.Name, hcEchoRatio));
                    echoEffect.CIMSources.Add(new CIMSource(ta.Name, taEchoRatio));
                    Console.WriteLine($"Echo source CIM: {echoEffect.Name}:{hcEchoRatio}, {ta.Name}: {taEchoRatio}");
                }
                //Lifebind
                lifebind.CIMSources.Add(new CIMSource(echo.Name, hcEchoRatio));
                lifebind.CIMSources.Add(new CIMSource(ta.Name, taEchoRatio));

                hcEchoRatioSpenderSubRatio = hcEchoRatio * (1 - lfcfRatio - revRatio);
                taEchoRatioSpenderSubRatio = taEchoRatio * (1 - lfcfRatio - revRatio);
            }

            // Stasis & Spenders
            if (rev.Casts > 0)
            {
                var revStasisRatio = (double)rev.StasisCount / rev.Casts;
                rev.CIMSources.Add(new CIMSource(stasis.Name, revStasisRatio));
                rev.CIMSources.Add(new CIMSource(rev.Name, 1 - revStasisRatio));
            }


            foreach (var spender in spenders)
            {


                if (spender.Casts == 0) continue;
                var stasisRatio = (double)spender.StasisCount / spender.Casts;
                var nonStasisRatio = 1 - stasisRatio;
                var ebRatio = (double)spender.EssenceBurstCount / spender.Casts;
                var essenceRegenRatio = 1 - ebRatio;
                ebRatio *= nonStasisRatio;
                essenceRegenRatio *= nonStasisRatio;



                lfcfRatio *= ebRatio;
                revRatio *= ebRatio;
                hcEchoRatioSpenderSubRatio *= ebRatio;
                taEchoRatioSpenderSubRatio *= ebRatio;
                // TODO Echo fix recursive issue
                // Unsure what best solution is.
                // Using 1.0 CIM for now instead of echo CIM
                // Maybe its fine like this.

                //TEST
                //spender.CIMSources.Add(new CIMSource(essenceRegenDummy.Name, essenceRegenRatio));
                //spender.CIMSources.Add(new CIMSource(stasis.Name, stasisRatio));
                //spender.CIMSources.Add(new CIMSource(lfcfSource.Name, lfcfRatio));
                //spender.CIMSources.Add(new CIMSource(rev.Name, revRatio));
                //spender.CIMSources.Add(new CIMSource(ta.Name, taEchoRatioSpenderSubRatio));

                //TEST INSERT
                //spender.OneHardCIM = true;
                ////spender.ZeroCIM = true;
                //spender.MaxCIMPreSet = true;
                //spender.MaxCIM = 1.5;




                //if (spender.Name == Echo.name)
                //{
                //    spender.CIMSources.Add(new CIMSource(essenceRegenDummy.Name, hcEchoRatioSpenderSubRatio));
                //}
                //else
                //{
                //    spender.CIMSources.Add(new CIMSource(echo.Name, hcEchoRatioSpenderSubRatio));
                //}

                //TEST END

                Console.WriteLine($"{spender.Name} - Essence burst casts: {spender.EssenceBurstCount} non eb casts: {spender.Casts - spender.EssenceBurstCount}");
            }


            // Lifegiversflame automod
            lgf.HasteAutoModHeal = fb.Damage.Tick.Dmg / fb.Damage.Dmg;
            Console.WriteLine($"{fb.Name} Tick dmg: {fb.Damage.Tick.Dmg}, auto mod {fb.Damage.Tick.Dmg / fb.Damage.Dmg}");


            // Panacea
            var panaceaBlossomRatio = (double)blossom.Casts / (ve.Casts + blossom.Casts);
            panacea.CIMSources.Add(new CIMSource(blossom.Name, panaceaBlossomRatio));
            panacea.CIMSources.Add(new CIMSource(ve.Name, 1 - panaceaBlossomRatio));

            foreach (var ability in user.Abilities)
            {
                if (ability is Abilities.PresAbility)
                {
                    var _ability = (Abilities.PresAbility)ability;
                    Console.WriteLine($"{_ability.Name} stasis count: {_ability.StasisCount}");
                }

            }

            Console.WriteLine($"Panacea CIM - blossom: {panaceaBlossomRatio} verdan embrace: {1 - panaceaBlossomRatio}");

            Console.WriteLine($"{lf.Name} casts: {lf.Casts}");
            Console.WriteLine($"{cf.Name} casts: {cfCast.Casts}");

            Console.WriteLine($"{revEcho.Name} Apply count: {revEcho.ApplyyRefreshCount}");
            Console.WriteLine($"{rev.Name} Apply count: {rev.ApplyyRefreshCount}");
            Console.WriteLine($"{ve.Name} -  Casts: {ve.Casts}, Count: {ve.Heal.Count}");

        }
    }
}
