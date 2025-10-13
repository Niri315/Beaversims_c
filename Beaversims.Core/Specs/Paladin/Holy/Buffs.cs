using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Paladin.Holy.Buffs

{
    internal static class InfusionOfLight
    {
        public const int buffId = 54149;
        public static readonly HashSet<string> Abilities = [Holy.Abilities.HolyLight.name, Holy.Abilities.FlashOfLight.name, Holy.Abilities.Judgment.name];

    }
    internal static class AvengingCrusader
    {
        public const int buffId = 216331;

    }

    internal static class EmpyreanLegacy
    {
        public const int finalBuffId = 387178; 

    }
}
