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
    public List<GearSet> altGearSets { get; set; } = [];
    public string SpecName { get; set; }
    public string HeroTlName { get; set; }
    public string FightName { get; set; }
    public int FightId { get; set; }
    public string PlayerName { get; set; }
    public GainDict OriginalTotals { get; set; } = Utils.InitGainDict();
    public int Difficulty { get; set; }
    public double WipePercent { get; set; }
    public bool Success { get; set; }
    public void ToPerSec()
    {
        foreach (var altGearSet in altGearSets)
        {
            foreach (var gainEntry in altGearSet.Gains)
            {
                var gainType = gainEntry.Key;
                altGearSet.Gains[gainType] /= TotalTime;
            }
        }
        foreach (var gainEntry in OriginalTotals)
        {
            var gainType = gainEntry.Key;
            OriginalTotals[gainType] /= TotalTime;
        }
    }

    public Results() { }
}

namespace Beaversims.Core.Shared
{

    internal class ProcessEvents
    {

        public static void AddProcEvents(List<TpEvent> tpEvents, List<TpEvent> procEvents, int iterationCount, User user)
        {
            if (!Utils.SimsActive(user))
            {
                return;
            }
            for (int e = 0; e < procEvents.Count; e++)
            {
                var evt = procEvents[e];
                if (!evt.SimProcSource) { throw new DivideByZeroException(); }
                evt.Amount.Raw /= iterationCount;
                evt.Amount.Eff /= iterationCount;
                evt.Amount.Naraw /= iterationCount;
                evt.Amount.Naeff /= iterationCount;
            }
            tpEvents.AddRange(procEvents);
            tpEvents = tpEvents.OrderBy(e => e.Timestamp).ToList();
        }


        public static void StoreTotals(List<TpEvent> tpEvents, User user, int i, Fight fight)
        {
            var logging = Constants.TEST_MODE;
            Logger statLogger = new Logger($"{user.AltGearSets[i].Name} - Stat Tracker", fight, user.Id.TypeId);
            double lastLogTime = double.NegativeInfinity; // ensures first event always logs




            foreach (TpEvent evt in tpEvents)
            {
                // TODO Bug cba rn
                //if (logging && evt.Timestamp >= lastLogTime + 5.0)
                //{
                //    evt.AltEvents[i].UserStats.LogStats(statLogger, evt.Timestamp);
                //    lastLogTime = evt.Timestamp;
                //}
                
                AmountContainer amounts;


                if (evt.SimEvent)
                {
                    amounts = evt.Amount;
                }
                else
                {
                    amounts = evt.AltEvents[i].Amount;
                }

                var gainType = GainType.Eff;
                double amount;
                if (evt.IsDamageTakenEvent())
                {
                    gainType = GainType.Def;
                    amount = amounts.Eff;
                    user.AltGearSets[i].Gains[gainType] -= amount;

                    if (logging)
                    {
                        // Skipping logging DEF for now... The abilities are not in user.Abilities. 

                        //Console.WriteLine(i);
                        //Console.WriteLine(gainType);
                        //Console.WriteLine(evt.Ability.Gains.Count);
                        //Console.WriteLine(evt.Ability.Name);
                        //evt.Ability.Gains[i][gainType] -= amount - evt.Amount.Eff;
                    }

                    continue;
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
                amount = amounts.Eff;
                user.AltGearSets[i].Gains[gainType] += amount;

                // TODO Bug cba rn
                //if (logging)
                //{
                //    evt.Ability.Gains[i][gainType] += amount - evt.Amount.Eff;
                //}

            }
            if (logging)
            {
                Logger abilityLogger = new Logger($"{user.AltGearSets[i].Name} - Ability Gains", fight, user.Id.TypeId);
                foreach (var ability in user.Abilities)
                {
                    abilityLogger.Log("-----------");
                    abilityLogger.Log(ability.Name);
                    foreach (var gainType in ability.Gains[i])
                    {
                        if (gainType.Value == 0)
                        {
                            continue;
                        }
                        abilityLogger.Log($"\t{gainType.Key}: {gainType.Value}");

                    }

                }

            }

        }

        public static void OriginalTotals(TpEvent evt, User user)
        {
            var gainType = GainType.Eff;
            double amount;
            if (evt.IsDamageTakenEvent())
            {
                gainType = GainType.Def;
                amount = evt.Amount.Eff;

                user.OriginalTotals[gainType] -= amount;
                return;
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
                return;
            }
            amount = evt.Amount.Eff;
            //if (!Constants.swOption)
            //{
            //    amount /= Constants.iterationCount;
            //}
            user.OriginalTotals[gainType] += amount;
        }


        public static void SetFinalGains(User user)
        {
            for (int i = 0; i < user.AltGearSets.Count; i++)
            {
                var gearSet = user.AltGearSets[i];
                foreach (var gainEntry in gearSet.Gains)
                {
                    var gainType = gainEntry.Key;
                    var gains = gearSet.Gains;
                    if (user.SimMode == SimMode.SW)
                    {

                        gains[gainType] -= user.OriginalTotals[gainType];

                    }
                    if (user.SimMode == SimMode.TopGear)
                    {
                        //if (i == 0)
                        //{
                        //    //gains[gainType] -= user.Totals[gainType] / fight.TotalTime;
                        //}
                        //else
                        //{
                            //gains[gainType] -= user.AltGearSets[0].Gains[gainType];
                            //gains[gainType] /= fight.TotalTime;
                        //}
                    }
                    if (user.SimMode == SimMode.StatAlloc)
                    {

                    }

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



   

        public static void SharedIteration(List<Event> events, Fight fight, User user, Results results)
        {
            var abilityGainLogger = new Logger("StatGainByAbility", fight, user.Id.TypeId);
            var abilityGains = new Dictionary<string, GainMatrix>();

            SetFinalGains(user);
            results.altGearSets = user.AltGearSets;
            results.OriginalTotals = user.OriginalTotals;


        }
    }
}
