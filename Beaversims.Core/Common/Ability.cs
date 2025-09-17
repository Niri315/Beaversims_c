using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Beaversims.Core
{

    internal class HealData
    {
        public int Eff = 0;
        public int Raw = 0;
        public int Overheal = 0;
        public int Count = 0;
    }
    internal class HealDataContainer : HealData
    {
        public HealData Crit = new();
        public HealData Hit = new();
    }
    internal class DmgData
    {
        public int Dmg = 0;
        public int Count = 0;
    }
    internal class DmgDataContainer : DmgData
    {
        public DmgData Crit = new();
        public DmgData Hit = new();
    }

    internal class Ability
    {
        public virtual string Name { get; set; } = "Unnamed Ability";
        public virtual double Cooldown { get; set; } = 0;
        public virtual double ManaCost_p { get; set; } = 0;
        public virtual double CastTime { get; set; } = 0;
        public virtual HashSet<ScalerType> Scalers { get; set;} = new();
        public virtual HashSet<HasteScalerType> HasteScalers { get; set; } = new();
        public virtual bool Direct {  get; set; } = false;
        public virtual bool ForceTick { get; set; } = false; // For forcing tick in parser. Concecration etc.

        public int Casts { get; set; } = 0;

        public HealDataContainer Heal { get; set; } = new();
        public DmgDataContainer Damage { get; set; } = new();
    }
}
