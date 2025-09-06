using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Utils
{
    internal static class TimeUtils
    {
        public static double ConvertLogTime(int curLogTime, int startLogTime)
        {
            return (curLogTime - startLogTime) / 1000.0;
        }
    }
}
