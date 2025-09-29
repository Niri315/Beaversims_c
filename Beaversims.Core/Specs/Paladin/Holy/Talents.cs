using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Paladin.Holy.Talents
{
    internal class InflorescenceOfTheSunwell : GainTalent
    {
        public const int id = 102577;
        public TalentCoef GjCoef { get; }
        public InflorescenceOfTheSunwell(int rank) : base(id, rank)
        {
            GjCoef = new TalentCoef(0.5 *  rank, abilities: [Abilities.GreaterJudgment.name]);
        }
    }

    internal class BreakingDawn : GainTalent
    {
        public const int id = 102567;
        private const int nullRange = 15;
        public int Range { get; }
        public BreakingDawn(int rank) : base(id, rank)
        {
            if (rank == 1)
            {
                Range = 25;
            }
            else if (rank == 2)
            {
                Range = 40;
            }
        }
    }
    internal class BeaconOfFaith : Talent
    {
        public const int id = 102533;
        public double Coef { get; }
        public BeaconOfFaith(int rank) : base(id, rank)
        {
            Coef = 0.3 * rank;
        }
    }

    internal class CommandingLight : Talent
    {
        public const int id = 102564;
        public double Coef { get; }
        public CommandingLight(int rank) : base(id, rank)
        {
            Coef = 0.05 * rank;
        }
    }

    internal class BeaconOfTheLightbringer : Talent
    {
        public const int id = 102549;
        public BeaconOfTheLightbringer(int rank) : base(id, rank)
        {
        }
    }
    internal class Awestruck : Talent
    {
        public const int id = 102544;
        public double Coef { get; }
        private static readonly HashSet<string> abilities = [Abilities.HolyShock.name, Abilities.HolyLight.name, Abilities.FlashOfLight.name];
        public void SetCritInc(User user)
        {
            foreach (var ability in abilities)
            {
                user.Abilities.Get(ability).BonusCritIncHeal += Coef;
            }
        }
        public Awestruck(int rank) : base(id, rank)
        {
            Coef = 0.2 * rank;
        }
    }
}
