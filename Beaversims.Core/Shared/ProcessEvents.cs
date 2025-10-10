using Beaversims.Core;
using Beaversims.Core.Common;
using Beaversims.Core.Sim;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Results
{
    public double TotalTime { get; set; } = 0;
    public GainMatrix StatGains { get; set; } = Utils.InitGainMatrix();
    public GainMatrix swGains { get; set; } = Utils.InitGainMatrix();
    public List<GearSet> altGearSets { get; set; } = [];
    public void ToPerSec()
    {
        foreach (var statEntry in StatGains)
        {
            var stat = statEntry.Key;
            foreach (var gainEntry in statEntry.Value)
            {
                var gainType = gainEntry.Key;
                swGains[stat][gainType] = gainEntry.Value / TotalTime;
            }
        }
        
        foreach (var altGearSet in altGearSets)
        {
            foreach (var gainEntry in altGearSet.Gains)
            {
                var gainType = gainEntry.Key;
                altGearSet.Gains[gainType] /= TotalTime;
            }
        }
    }

    public Results() { }
}

namespace Beaversims.Core.Shared
{

    internal class ProcessEvents
    {
        public static void StatGains(Event evt, GainMatrix statGains, AbilityGainMatrix abilityGains, Fight fight)
        {

            foreach (var statEntry in evt.Gains)
            {
                var stat = statEntry.Key;
                foreach (var gainEntry in statEntry.Value)
                {
                    if (gainEntry.Value == 0) {  continue; }
                    var gainType = gainEntry.Key;
                    var abilityName = evt.AbilityName;
                    statGains[stat][gainType] += gainEntry.Value;
                    if (!abilityGains.ContainsKey(abilityName)) {abilityGains[abilityName] = [];}
                    if (!abilityGains[abilityName].ContainsKey(stat)) { abilityGains[abilityName][stat] = []; }
                    if (!abilityGains[abilityName][stat].ContainsKey(gainType)) { abilityGains[abilityName][stat][gainType] = 0.0; }
                    abilityGains[abilityName][stat][gainType] += gainEntry.Value / fight.TotalTime;
                }
            }
        }

        public static void altAmounts(ThroughputEvent evt, Fight fight, User user)
        {

            for (int i = 0; i < evt.AltEvents.Count; i++)
            {
                var altEvent = evt.AltEvents[i];
                var gainType = GainType.Eff;
                if (evt.IsDamageTakenEvent())
                {
                    gainType = GainType.Def;
                    if (i == 0)
                    {
                        user.altGearSets[i].Gains[gainType] -= (altEvent.Amount.Eff - evt.Amount.Eff);
                    }
                    else
                    {
                        user.altGearSets[i].Gains[gainType] -= (altEvent.Amount.Eff - evt.AltEvents[0].Amount.Eff);
                    }
                }
                else if (evt.IsDmgDoneEvent())
                {
                    gainType = GainType.Dmg;
                }

                else if (evt.IsHealDoneEvent())
                {
                    gainType = GainType.Eff;
                }
                else if (evt.Ability.SuppStamScaler && evt.TargetUnit is User && evt is HealEvent)
                {
                    gainType = GainType.SupEff;
                }
                else if (evt.Ability.SuppStamScaler && evt.TargetUnit is not User && evt is DamageEvent)
                {
                    gainType = GainType.SupDmg;
                }
                else
                {
                    continue;
                }
                if (i == 0)
                {
                    user.altGearSets[i].Gains[gainType] += (altEvent.Amount.Eff - evt.Amount.Eff);
                }
                else
                {
                    user.altGearSets[i].Gains[gainType] += (altEvent.Amount.Eff - evt.AltEvents[0].Amount.Eff);
                }
            }
        }


        public static string TranslateGainType(GainType gainType) =>
            gainType switch
            {
                GainType.Eff => "Heal",
                GainType.Dmg => "Damage",
                GainType.Def => "Damage Reduction",
                GainType.SupEff => "Supportive Heal",
                GainType.SupDmg => "Supportive Damage",
                GainType.BalEff => "Moderate Mana Penalty Heal",
                GainType.BalDmg => "Moderate Mana Penalty Damage",
                GainType.MsEff => "Severe Mana Penalty Heal",
                GainType.MsDmg => "Severe Mana Penalty Damage",
                _ => "Unknown Gain"
            };

        public static void LogAbilityGains(User user, Logger logger, AbilityGainMatrix abilityGains)
        {

            foreach (var ability in abilityGains)
            {
                logger.Log(ability.Key);

                foreach (var stat in ability.Value)
                {
                    logger.Log($"\t{stat.Key}");

                    foreach (var gainType in stat.Value)
                    {
                        var gainType_n = TranslateGainType(gainType.Key);
                        logger.Log($"\t\t{gainType_n}: {gainType.Value}");
                    }

                }
                logger.Log("-------------------");

            }
        }

   

        public static void SharedIteration(List<Event> events, Fight fight, User user, Results results)
        {
            var statGains = results.StatGains;
            var abilityGainLogger = new Logger("StatGainByAbility", fight, user.Id.TypeId);
            var abilityGains = new Dictionary<string, GainMatrix>();


            //for (int i = 0; i < user.altGearSets.Count; i++)
            //{
            //    results.altGearGains.Add(Utils.InitGainDict());
            //}


            foreach (Event evt in events)
            {
                StatGains(evt, statGains, abilityGains, fight);
                if (evt is ThroughputEvent tEvt) 
                {
                    altAmounts(tEvt, fight, user);
                }
            }

            results.altGearSets = user.altGearSets;
            LogAbilityGains(user, abilityGainLogger, abilityGains);

            //Console.WriteLine($"AMOUNT COMP EFF: {test[0][GainType.Eff]} vs {test[1][GainType.Eff]}");
            //Console.WriteLine($"AMOUNT COMP DMG: {test[0][GainType.Dmg]} vs {test[1][GainType.Dmg]}");
            //Console.WriteLine($"AMOUNT COMP DEF: {test[0][GainType.Def]} vs {test[1][GainType.Def]}");
            //Console.WriteLine($"EFF: {test[1][GainType.Eff] - test[0][GainType.Eff]}");
            //Console.WriteLine($"DMG: {test[1][GainType.Dmg] - test[0][GainType.Dmg]}");
            //Console.WriteLine($"DEF: {test[0][GainType.Def] - test[1][GainType.Def]}");


        }
    }
}
