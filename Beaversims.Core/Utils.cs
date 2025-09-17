using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    internal static class Utils
    {
        public static UnitId GetPlayerId(int ownerId) => new UnitId(ownerId, 0);
        public static double ConvertLogTime(int curLogTime, int startLogTime) => (curLogTime - startLogTime) / 1000.0;
    }
}
