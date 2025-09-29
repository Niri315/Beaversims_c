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
    }
}
