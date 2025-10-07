using Beaversims.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static void altAmounts(ThroughputEvent evt, List<Dictionary<GainType, double>> test, Fight fight)
        {
    
            foreach (var altEvent in evt.AltEvents)
            {
                var gainType = GainType.Eff;
                if (evt.IsDmgDoneEvent())
                {
                    gainType = GainType.Dmg;
                }
                else if (evt.IsDamageTakenEvent())
                {
                    gainType = GainType.Def;
                }
                else if (evt.IsHealDoneEvent())
                {
                    gainType = GainType.Eff;
                }
                else
                {
                    continue;
                }
                test[1][gainType] += altEvent.Amount.Eff / fight.TotalTime;
                test[0][gainType] += evt.Amount.Eff / fight.TotalTime;
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

        public static GainMatrix SharedIteration(List<Event> events, Fight fight, User user, Results results)
        {
            var statGains = results.StatGains;
            var abilityGainLogger = new Logger("StatGainByAbility", fight, user.Id.TypeId);
            var abilityGains = new Dictionary<string, GainMatrix>();

            List<Dictionary<GainType, double>> test = [];
            test.Add(Utils.InitGainDict());
            test.Add(Utils.InitGainDict());

            foreach (Event evt in events)
            {
                StatGains(evt, statGains, abilityGains, fight);
                if (evt is ThroughputEvent tEvt) 
                {
                    altAmounts(tEvt, test, fight);

                }
            }
            LogAbilityGains(user, abilityGainLogger, abilityGains);

            Console.WriteLine($"AMOUNT COMP EFF: {test[0][GainType.Eff]} vs {test[1][GainType.Eff]}");
            Console.WriteLine($"AMOUNT COMP DMG: {test[0][GainType.Dmg]} vs {test[1][GainType.Dmg]}");
            Console.WriteLine($"AMOUNT COMP DEF: {test[0][GainType.Def]} vs {test[1][GainType.Def]}");

            return statGains;
        }
    }
}
