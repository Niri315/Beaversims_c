using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Druid.Resto.Talents
{
    internal class HarmoniousBlooming : Talent
    {
        public const int id = 103121;
        public const string abilityName = "Lifebloom";
        public int HarmonyCount {  get; set; }
        public HarmoniousBlooming(int rank) : base(id, rank)
        {
            HarmonyCount = 3 * rank;
        }
    }
    internal class Abundance : Talent
    {
        public const int id = 103105;
        public const int buffId = 207640;
        public const string abilityName = Abilities.Regrowth.name;
        public const double coef = 0.08;
        public const double cap = 0.96;
        public Abundance(int rank) : base(id, rank)
        {
        }
    }
    internal class ImprovedRegrowth : Talent
    {
        public const int id = 103109;
        public const string abilityName = Abilities.Regrowth.name;
        public const double coef = 0.4;
        public ImprovedRegrowth(int rank) : base(id, rank)
        {
        }
    }

    internal class StrategicInfusion : Talent
    {
        public const int id = 117223;
        public const double coef = 0.04;
        public StrategicInfusion(int rank) : base(id, rank)
        {
        }
    }
    internal class SoulOfTheForest : Talent
    {
        public const int id = 103113;
        public const double coef = 0.6;
        public static readonly HashSet<string> abilities = [Abilities.Rejuvenation.name, Abilities.Regrowth.name];
        public const int buffId = 114108;
        public SoulOfTheForest(int rank) : base(id, rank)
        {
        }
    }
}


