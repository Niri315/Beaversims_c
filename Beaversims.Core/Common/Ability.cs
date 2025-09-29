using Beaversims.Core.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Beaversims.Core
{

    internal class HealData
    {
        public double Eff { get; set; } = 0;
        public double Raw { get; set; } = 0;
        public double Count { get; set; } = 0;
        public double Hypo {  get; set; } = 0;
    }
    internal class HealDataContainer : HealData
    {
        public HealData Crit { get; set; } = new();
        public HealData Hit { get; set; } = new();
    }
    internal class DmgData
    {
        public double Dmg { get; set; }  = 0;
        public double Count { get; set; }  = 0;
        public double Hypo { get; set; } = 0;
    }
    internal class DmgDataContainer : DmgData
    {
        public DmgData Crit { get; set; }  = new();
        public DmgData Hit { get; set; }  = new();
    }

    internal class Ability
    {
        public string Name { get; set; } = "Unnamed Ability";
        public double Cooldown { get; set; } = 0.0;
        public double ManaCost_p { get; set; } = 0.0;
        public double CastTime { get; set; } = 0.0;
        public double BonusCritIncHeal {  get; set; } = 0.0;
        public double BonusCritIncDmg { get; set; } = 0.0;
        public HashSet<StatName> Scalers { get; } = new();
        public bool SuppStamScaler { get; set; } = false;
        public HashSet<HasteScalerType> HasteScalers { get; } = new();
        public bool Direct {  get; set; } = false;
        public bool Spell {  get; set; } = false;
        public bool Gcd { get; set; } = false;  // unused currently. Could implement so gcd = true -> cast time = 1.5.
        public bool ForceTick { get; set; } = false; // For forcing tick in parser. Concecration etc.
        public bool ReverseEffect { get; set; } = false;  // For easily running certain reverse effects like AC as autoscalers. 
        public int Casts { get; set; } = 0;
        public double CastTimeGain { get; set; } = 0.0;
        public double HGCM { get; set; } = 1.0;

        public HealDataContainer Heal { get; } = new();
        public DmgDataContainer Damage { get; } = new();

        public bool ScalesWith(StatName statName) => Scalers.Contains(statName);
        public double CritUr()
        {
            //Defaults to normal UR if 0 crits
            if (Heal.Crit.Raw > 0)
            {
                return Heal.Crit.Eff / Heal.Crit.Raw;

            }
            else if (Heal.Raw > 0) 
            { 
                return Heal.Eff / Heal.Raw; 
            }
            else 
            {  
                return 0;
            }
        }
        public double HypoTrueUr()
        {
            if (Heal.Hypo == 0) { return 0; }
            return Heal.Eff / Heal.Hypo;
        }
        public double HypoTrueRawR()
        {
            if (Heal.Hypo == 0) { return 0; }
            return Heal.Raw / Heal.Hypo;
        }
        public double HypoTrueDmgR()
        {
            if (Damage.Hypo == 0) { return 0; }
            return Damage.Dmg / Damage.Hypo;
        }
    }
    internal abstract class SharedAbility : Ability
    {
    }
}
