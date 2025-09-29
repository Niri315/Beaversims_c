using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    internal class TalentCoef
    {
        public double Coef { get; }
        public HashSet<string>? Abilities { get; }
        public TalentCoef(double coef, HashSet<string>? abilities = null)
        {
            Coef = coef;
            Abilities = abilities;
        }
    }

    internal class Talent(int id, int rank)
    {
        public int Id { get; } = id;
        public int Rank { get; set; } = rank;
    }

    internal class GainTalent(int id, int rank) : Talent(id, rank)
    {
        public GainDict Gains { get; } = Utils.InitGainDict();
    }
}
