using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Sim
{
    public enum ProcFlag
    {
        SpellOnly,
        DamageOnly,
        HealOnly,
    }

    internal static class Proc
    {
        private static readonly Random random = new Random();
        public const int maxBlp = 1000;
        public const double maxInterval = 3.5;
        public const double onPullBlp = 90;
        public const double initLastAttempt = -10;
        

        public static double CalcProcChance(double trueRppm, double timestamp, double lastAttempt, double blp)
        {
            var seconds = Math.Min(timestamp - lastAttempt, maxInterval);
            var oldRppmChance = trueRppm * (seconds / 60);
            var lastSuccess = Math.Min(blp, maxBlp);
            var expAvgProcInterval = 60 / trueRppm;
            var procChance = Math.Max(1.0, 1 + ((lastSuccess / expAvgProcInterval - 1.5) * 3.0)) * oldRppmChance;
            return procChance;
        }


        public static bool ProcessProcAttempt(ref double blp,  double trueRppm, ref double lastAttempt, double timestamp)
        {
            var isProc = false;
            blp += Math.Min(timestamp - lastAttempt, maxInterval);
            var procChance = CalcProcChance(trueRppm, timestamp, lastAttempt, blp);
            if (random.NextDouble() < procChance)
            {
                blp = 0;
                isProc = true;
            }
            lastAttempt = timestamp;
            return isProc;
        }

        public static bool FilterProcFlags(Event evt, HashSet<ProcFlag> procFlags)
        {
            if (procFlags.Contains(ProcFlag.HealOnly) && !evt.IsHealDoneEvent()) { return false; }
            if (procFlags.Contains(ProcFlag.DamageOnly) && !evt.IsDmgDoneEvent()) { return false; }
            if (procFlags.Contains(ProcFlag.SpellOnly) && !evt.Ability.Spell) { return false; }
            return true;
        }

        public static bool IsProcAttempt(Event evt, HashSet<ProcFlag> procFlags, double lastProc, double icd, double timestamp)
        {
            // Gonna need to remove some of these and reorganize for procs from damage taken etc.
            // Will deal with that later.

            if (evt is not TpEvent) { return false; }
            if (evt.SourceUnit is not User) { return false; }
            if (lastProc + icd > timestamp) { return false; }
            return FilterProcFlags(evt, procFlags);
        }
    }
}
