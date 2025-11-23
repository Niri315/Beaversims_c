using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Evoker.Pres.Talents
{
    internal class ResonatingSphere : Talent
    {
        public const int id = 115563;
        public const int echoCount = 5;
        public const double coef = 0.3;
        public ResonatingSphere(int rank) : base(id, rank)
        {
        }
    }
    internal class LeapingFlames : Talent
    {
        public const int id = 115657;
        public const int buffId = 370901;
        public LeapingFlames(int rank) : base(id, rank)
        {
        }
    }
    internal class Enkindle : Talent
    {
        public const int id = 117553;
        public Enkindle(int rank) : base(id, rank)
        {
        }
    }

    internal class NaturalConvergence : Talent
    {
        public const int id = 115621;
        public const double coef = 0.2;
        public NaturalConvergence(int rank) : base(id, rank)
        {
        }
    }
    internal class AncientFlame : Talent
    {
        public const int id = 115577;
        public const int buffId = 375583;
        public const double coef = 0.4;
        public static readonly HashSet<string> affectedSpells = [Abilities.LivingFlame.name, Abilities.ChronoFlames.name];
        public AncientFlame(int rank) : base(id, rank)
        {
        }
    }

    internal class TemporalCompression : Talent
    {
        public const int id = 115543;
        public const int buffId = 362877;
        public const double coef = 0.1;
        public TemporalCompression(int rank) : base(id, rank)
        {
        }
    }

    internal class FlowState : Talent
    {
        public const int id = 115560;
        public const int buffId = 390148;
        public double Coef {  get; set; }
        public FlowState(int rank) : base(id, rank)
        {
            Coef = 0.05 * rank;
        }
    }
    internal class Lifespark : Talent
    {
        public const int id = 123294;
        public const int buffId = 443176;
        public static readonly HashSet<string> affectedSpells = [Abilities.LivingFlame.name, Abilities.ChronoFlames.name];

        public Lifespark(int rank) : base(id, rank)
        {
       
        }
    }
    internal class TimelessMagic : Talent
    {
        public const int id = 115568;
        public double Coef { get; set;}

        public TimelessMagic(int rank) : base(id, rank)
        {
            Coef = 0.15 * rank;
        }
    }

    internal class DoubleTime : Talent
    {
        public const int id = 117529;

        public DoubleTime(int rank) : base(id, rank)
        {

        }
    }
    internal class FontOfMagic : Talent
    {
        public const int id = 115556;
        public const int levelInc = 1;

        public FontOfMagic(int rank) : base(id, rank)
        {

        }
    }
    internal class LifeforceMender : Talent
    {
        public const int id = 115538;
        public static readonly HashSet<string> abilities = [Abilities.LivingFlame.name, Abilities.FireBreath.name];
        public double Coef { get; set;}

        public LifeforceMender(int rank) : base(id, rank)
        {
            Coef = rank * 0.01;
        }
    }

}


