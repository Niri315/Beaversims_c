using Beaversims.Core.Specs.Paladin.Holy.Abilities;
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
            GjCoef = new TalentCoef(0.5 * rank, abilities: [Abilities.GreaterJudgment.name]);
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

    internal class SecondSunrise : Talent
    {
        public const int id = 117683;
        public double Coef { get; }
        public SecondSunrise(int rank) : base(id, rank)
        {
            Coef = 0.15 * rank;
        }
    }
    internal class HammerAndAnvil : Talent
    {
        public const int id = 117887;
        public HammerAndAnvil(int rank) : base(id, rank)
        {
        }
    }
    internal class SelflessHealer : Talent
    {
        public const int id = 128309;
        public double Coef { get; }
        public HashSet<string> SourceAbilities { get; set; } = [HolyLight.name, FlashOfLight.name];
        public SelflessHealer(int rank) : base(id, rank)
        {
            Coef = 0.1 * rank;
        }
    }
    internal class RisingSunlight : Talent
    {
        public const int id = 102581;
        public double HolyShockCount { get; }
        public RisingSunlight(int rank) : base(id, rank)
        {
            HolyShockCount = 2 * rank;
        }
    }

    internal class DivineResonance : Talent
    {
        public const int id = 115466;
        public double HolyShockCount { get; }
        public DivineResonance(int rank) : base(id, rank)
        {
            HolyShockCount = 3 * rank;
        }
    }
    internal class CrusadersMight : Talent
    {
        public const int id = 102580;
        public double CdReduct { get; }
        public CrusadersMight(int rank) : base(id, rank)
        {
            CdReduct = 2.0 * rank;
        }
    }
    internal class ImbuedInfusions : Talent
    {
        public const int id = 102536;
        public double CdReduct { get; }
        public ImbuedInfusions(int rank) : base(id, rank)
        {
            CdReduct = 1.0 * rank;
        }
    }
    internal class EmpyreanLegacy : Talent
    {
        public const int id = 102576;
        public const double cd = 20;
        public const double coef = 1.25;
        public EmpyreanLegacy(int rank) : base(id, rank)
        {

        }
    }
    internal class GloriousDawn : Talent
    {
        public const int id = 115873;
        public const double procChance = 0.12;
        public GloriousDawn(int rank) : base(id, rank)
        {

        }
    }
    internal class RighteousJudgment : Talent
    {
        public const int id = 115875;
        public const double procChance = 1.0;
        public const double coef = 1.25;
        public RighteousJudgment(int rank) : base(id, rank)
        {

        }
    }
    internal class DivineInspiration : Talent
    {
        public const int id = 117877;
        public DivineInspiration(int rank) : base(id, rank)
        {

        }
    }
    internal class BlessingOfAnshe : Talent
    {
        public const int id = 117668;
        public const int buffId = 445204;
        public double Coef { get; }
        public double HealValue { get; set; } = 0;
        public double DmgValue { get; set; } = 0;
        public bool Active { get; set; } = false;
        public double BuffDur { get; set; } = 20;
        public double BuffEnd { get; set; } = 0;
        public BlessingOfAnshe(int rank) : base(id, rank)
        {
            Coef = rank * 2.0;
        }
    }
    internal class TowerOfRadiance : Talent
    {
        public const int id = 102571;
        public TowerOfRadiance(int rank) : base(id, rank)
        {

        }
    }
}


