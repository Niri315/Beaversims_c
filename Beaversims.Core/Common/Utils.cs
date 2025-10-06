using Beaversims.Core.Specs.Paladin.Holy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Beaversims.Core
{

    internal static class Utils
    {
        public static UnitId GetPlayerId(int ownerId) => new UnitId(ownerId, 0);
        public static string ReadableTime(double timestamp)
        {
            int minutes = (int)(timestamp / 60);
            double remainingSeconds = timestamp % 60;
            return string.Format("{0:00}:{1:00.000}", minutes, remainingSeconds);
        }
        public static double ConvertLogTime(int curLogTime, int startLogTime) => (curLogTime - startLogTime) / 1000.0;
        public static GainMatrix InitGainMatrix()
        {
            return Enum.GetValues<StatName>()
                .ToDictionary(stat => stat, _ => InitGainDict());
        }

        public static GainDict InitGainDict()
        {
            return Enum.GetValues<GainType>()
                .ToDictionary(gain => gain, _ => 0.0);
        }
        public static string PrintObject(object obj)
        {
            if (obj == null) return "null";

            var type = obj.GetType();
            var sb = new StringBuilder();
            sb.AppendLine($"--- {type.Name} ---");

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                object value = prop.GetValue(obj);
                sb.AppendLine($"{prop.Name} = {value}");
            }

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object value = field.GetValue(obj);
                sb.AppendLine($"{field.Name} = {value}");
            }

            return sb.ToString();
        }
        public static void CleanUp(UnitRepo allUnits)
        {
            var user = allUnits.GetUser();
            user.Stats = null;
            foreach (var unit in allUnits)
            {
                unit.Buffs = null;
                unit.Hp = null;
                unit.MaxHp = null;
                unit.Coords = null;
            }
        }
        public static GainType ReverseGainType(GainType gainType) =>
            gainType switch
            {
                GainType.Eff => GainType.Dmg,
                GainType.BalEff => GainType.BalDmg,
                GainType.MsEff => GainType.MsDmg,
                GainType.Dmg => GainType.Eff,
                GainType.BalDmg => GainType.BalEff,
                GainType.MsDmg => GainType.MsEff,
                GainType.SupDmg => GainType.SupEff,
                _ => gainType
            };

        public static GainType GainTypeToHeal(GainType gainType) =>
            gainType switch
            {
                GainType.Dmg => GainType.Eff,
                GainType.BalDmg => GainType.BalEff,
                GainType.MsDmg => GainType.MsEff,
                GainType.SupDmg => GainType.SupEff,
                _ => gainType
            };

        public static string ProjectRoot() => Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\Beaversims.Core"));

        public static StatName? IdToStatName(int id) =>
            id switch
            {
                5 => StatName.Intellect,
                7 => StatName.Stamina,
                32 => StatName.Crit,
                36 => StatName.Haste,
                40 => StatName.Vers,
                49 => StatName.Mastery,
                //61 => StatName.Speed,
                62 => StatName.Leech,
                63 => StatName.Avoidance,
                //Prim2+
                71 => StatName.Intellect,
                73 => StatName.Intellect,
                74 => StatName.Intellect,
                _ => null
            };

        public static bool IsSecondaryStat(StatName statName) => statName == StatName.Crit || statName == StatName.Haste || statName == StatName.Vers || statName == StatName.Mastery;
        public static bool IsTertiaryStat(StatName statName) => statName == StatName.Avoidance || statName == StatName.Leech;  // + speed

    }
}
