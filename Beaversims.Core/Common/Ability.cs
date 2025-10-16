using Beaversims.Core.Parser;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Beaversims.Core
{
    internal class HCCGMSource
    {
        public string Name { get; set; }
        public double HCCGMReliance { get; set; }
        public HCCGMSource(string name, double hcgmReliance)
        {
            Name = name;
            HCCGMReliance = hcgmReliance;
        }
    }
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
        public HealData Nsnsna { get; set; } = new();
        public HealData NonSummon { get; set; } = new();
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
        public DmgData Nsnsna { get; set; } = new();
        public DmgData NonSummon { get; set; } = new();


    }

    internal class Ability
    {
        public string Name { get; set; } = "Unnamed Ability";
        public double Cooldown { get; set; } = 0.0;
        public double ManaCost_p { get; set; } = 0.0;
        public double CastTime { get; set; } = 0.0;
        public double Cd { get; set; } = 0.0;
        public double BonusCritIncHeal { get; set; } = 0.0;
        public double BonusCritIncDmg { get; set; } = 0.0;
        public HashSet<StatName> Scalers { get; } = [];
        public bool SuppStamScaler { get; set; } = false;
        public bool DerivedCritScaler { get; set; } = false;
        public string SourceAbility {  get; set; } = string.Empty;
        public HashSet<HasteScalerType> HasteScalers { get; } = [];
        public bool Direct {  get; set; } = false;
        public bool Spell {  get; set; } = false;
        public bool Gcd { get; set; } = false;  // unused currently. Could implement so gcd = true -> cast time = 1.5.
        public bool ForceTick { get; set; } = false; // For forcing tick in parser. Concecration etc.
        public bool ReverseEffect { get; set; } = false;  // For easily running certain reverse effects like AC as autoscalers. 
        public double Duration { get; set; } = 0.0;
        public int Casts { get; set; } = 0;
        public double CdTimeHypo { get; set; } = 0.0;
        public double CdEnd {  get; set; } = 0.0;
        public double TrueCastTimeTotal { get; set; } = 0.0;
        public double CastTimeGain { get; set; } = 0.0;
        public double HCCGM { get; set; } = 1.0;
        public bool HCCGMInitDone { get; set; } = false;
        public double HCCGMRatio { get; set; } = 1.0;
        public double MaxHCCGM { get; set; } = 1.0;
        public double HasteCastGainMod { get; set; } = 1.0;
        public double HasteGainMod { get; set; } = 1.0;
        public HashSet<HCCGMSource> HCCGMSources { get; set; } = [];

        public bool IgnoreDr {  get; set; } = false;
        public bool LeechSource { get; set; } = true;
        public bool CanDupli {  get; set; } = true;

        public bool SimDupliAbility { get; set; } = false; // ONLY for abilities that we do dupli calcs for, not true for the ones we use derived source for.
        public double DupliNukeAmount { get; set; } = 0.0;

        public HealDataContainer Heal { get; } = new();
        public DmgDataContainer Damage { get; } = new();

        public List<HealDataContainer> AltHeal { get; } = [];
        public List<DmgDataContainer> AltDamage { get; } = [];


        //public virtual void ResetAlts(User user, int i)
        //{
        //    AltHeal[i] = new HealDataContainer();
        //    AltDamage[i] = new DmgDataContainer();
        //}

        public double TrueHCCGM(User user)
        {
            if (HCCGMSources.Count == 0) { return HCCGM; }
            else
            {
                var abilities = user.Abilities;
                var hccgm = 0.0;
                foreach (var source in HCCGMSources)
                {
                    var sourceAbility = abilities.Get(source.Name);
                    if (sourceAbility.Name == Name)
                    {
                        hccgm += source.HCCGMReliance * HCCGM;
                    }
                    else
                    {
                        hccgm += source.HCCGMReliance * sourceAbility.TrueHCCGM(user);

                    }

                }
                return hccgm;
            }

        }

        public void HCCGMSourceRelCheck()
        {
            if (HCCGMSources.Count > 0)
            {
                var tot = 0.0;
                foreach (var source in HCCGMSources)
                {
                    tot += source.HCCGMReliance;
                }
                const double tolerance = 1e-9;

                if (Math.Abs(tot - 1.0) > tolerance)
                {

                    throw new InvalidOperationException(
                        $"HCCGM Source alloc issue: {Name}."
                    );
                }
            }
        }

        public void RemoveHST(User user, HasteScalerType hst)
        {
            HasteScalers.Remove(hst);
            //Important that HCCGM sources are updated correctly when removing HST.CAST.
            if (hst == HST.Cast)
            {
                foreach (var ability in user.Abilities)
                {
                    HCCGMSource sourcetbr = null;
                    double rel = 0.0;
                    int remCount = 0;

                    foreach (var source in ability.HCCGMSources)
                    {
                        if (source.Name == Name)
                        {
                            sourcetbr = source;
                            rel = source.HCCGMReliance;
                            remCount = ability.HCCGMSources.Count - 1;
                            break;
                        }
                    }

                    if (sourcetbr != null && remCount > 0)
                    {
                        ability.HCCGMSources.Remove(sourcetbr);
                        foreach (var source in ability.HCCGMSources)
                            source.HCCGMReliance += rel / remCount;
                    }
                }
            }
        }

        public bool ScalesWith(StatName statName) => Scalers.Contains(statName);
        public virtual void AlterHCGM(User user)
        {

        }
        public double AvgCdHypo() => Casts == 0 ? 0 : CdTimeHypo / Casts;
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
        public double Ur()
        {
            if (Heal.Raw == 0) { return 0; }
            return Heal.Eff / Heal.Raw;
        }
        public virtual double HypoTrueUr()
        {
            if (Heal.Hypo == 0) { return 0; }
            return Heal.Eff / Heal.Hypo;
        }
        public virtual double HypoTrueRawR()
        {
            if (Heal.Hypo == 0) { return 0; }
            return Heal.Raw / Heal.Hypo;
        }
     
        public virtual double HypoTrueDmgR()
        {
            if (Damage.Hypo == 0) { return 0; }
            return Damage.Dmg / Damage.Hypo;
        }
        public virtual double AltHypoTrueDmgR(int i)
        {
            //             if (AltDamage[i].Hypo == 0) { return 1; }
            // Should do this for normal hypo trues as well (?)

            if (AltDamage[i].Hypo == 0) { return 1; }
            if (Damage.Dmg == 0) { return 0; }
            return HypoTrueDmgR() * AltDamage[i].Hypo / Damage.Dmg;
        }
        public virtual double AltHypoTrueUr(int i)
        {
            if (AltHeal[i].Hypo == 0) { return 1; }
            if (Heal.Eff == 0) { return 0; }
            return HypoTrueUr() * AltHeal[i].Hypo / Heal.Eff;
        }
        public virtual double AltHypoTrueRawR(int i)
        {
            if (AltHeal[i].Hypo == 0) { return 1; }
            if (Heal.Raw == 0) { return 0; }
            //Console.WriteLine($"{HypoTrueRawR()} vs {AltHeal[i].Hypo} vs {Heal.Raw}");
            return HypoTrueRawR() * AltHeal[i].Hypo / Heal.Raw;
        }
        public double RawToNsnsnarawConvert(double amount)
        {
            if (Heal.Raw == 0) { return 0; }
            return amount * (Heal.Nsnsna.Raw / Heal.Raw);
        }
        public double RawToEffConvert(double amount)
        {
            if (Heal.Raw == 0) { return 0; }
            return amount * (Heal.Eff / Heal.Raw);
        }
    }
    internal abstract class SharedAbility : Ability
    {
    }
}
