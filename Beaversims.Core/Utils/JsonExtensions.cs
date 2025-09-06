using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Beaversims.Core.Utils
{
    public static class JsonExtensions
    {
        public static int GetInt32OrDefault(this JsonElement element, string propertyName, int defaultValue = 0)
        {
            if (element.TryGetProperty(propertyName, out JsonElement prop) &&
                prop.ValueKind == JsonValueKind.Number)
            {
                return prop.GetInt32();
            }
            return defaultValue;
        }
    }
}

