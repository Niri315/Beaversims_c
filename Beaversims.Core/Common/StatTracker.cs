using Beaversims.Core.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    internal class StatTracker : IEnumerable<Stat>
    {
        private readonly Dictionary<StatName, Stat> _stats = new();
        public Stat Get(StatName statName) => _stats[statName];
        public void UpdateAllStats()
        {
            foreach (var stat in _stats.Values) 
            {
                stat.FullUpdate();
            }
        }
        public void InitMastery(double percentRate)
        {
            _stats[StatName.Mastery] = new Mastery(percentRate);
        }

        public IEnumerator<Stat> GetEnumerator() => _stats.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public StatTracker()
        {
            _stats[StatName.Intellect] = new Intellect();
            _stats[StatName.Stamina] = new Stamina();
            _stats[StatName.Crit] = new Crit();
            _stats[StatName.Haste] = new Haste();
            _stats[StatName.Vers] = new Vers();
            _stats[StatName.Leech] = new Leech();
            _stats[StatName.Avoidance] = new Avoidance();
        }

        public void LogStats(Logger statLogger, double timestamp)
        {
            statLogger.Log("----------------------");
            statLogger.Log(Utils.ReadableTime(timestamp) + "\n");
            foreach (var stat in _stats.Values)
            {
                statLogger.Log(stat.Name.ToString());
                statLogger.Log($"\t Rating: {Math.Round(stat.Rating)}");
                var multiFactor = Math.Round((stat.Multi - 1) * 100, 2);
                statLogger.Log($"\t Multiplicative Factor: {multiFactor}%");


                if (stat is NonPrimaryStat npStat) 
                {
                    var basePercent = Math.Round(npStat.Base / npStat.PercentRate, 2);
                    var effPercent = Math.Round(npStat.Eff / npStat.PercentRate, 2);
                    statLogger.Log($"\t DR Bracket: {npStat.Bracket}");
                    statLogger.Log($"\t Base: {basePercent}%");
                    statLogger.Log($"\t Effective Percent: {effPercent}%");
                    if (stat is Crit crit)
                    {
                        statLogger.Log($"\t Crit Inc Heal: {Math.Round(crit.IncHeal, 2)}");
                        statLogger.Log($"\t Crit Inc Damage: {Math.Round(crit.IncDmg, 2)}");

                    }

                }

            }

        }

        public StatTracker Clone()
        {
            var clone = new StatTracker();

            foreach (var kvp in _stats)
            {
                clone._stats[kvp.Key] = kvp.Value.Clone();
            }

            return clone;
        }
    }
}
