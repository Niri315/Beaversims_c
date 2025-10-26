using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    internal class Constants
    {
        public const double GCD = 1.5;
        public const double castTimeCap = GCD / 2;
        public const double BlEffectRating = 30 * Haste.percentRate;
        public const int curVantusId = 1236891; //Manaforge omega

        public const int iterationCount = 500;
        public const bool swOption = true;
        public const bool deactivateSims = false; // For testing.

        public const double defaultHealIncMod = 1.08;  // PFA replace (heal taken mod included)



    }
}
