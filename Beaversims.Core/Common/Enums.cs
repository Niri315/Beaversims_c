using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{
    public enum GainType {Eff, Dmg, Def, SupEff, SupDmg, MsEff, MsDmg, BalEff, BalDmg}
    public enum Race
    { 

    }
    public enum SpecName
    {
        DisciplinePriest,
        HolyPaladin,
        HolyPriest,
        MistweaverMonk,
        PreservationEvoker,
        RestorationDruid,
        RestorationShaman, 
    }

    public enum ScalingClass
    {
        
    }
    public enum BonusIds
    {
        Avoidance = 40,
        Leech = 41,
        Fireflash = 8790, // Crit / Haste
        Peerless = 8791, // Crit / Mastery
        Feverflare = 8792, // Haste / Vers
        Aurora = 8793, // Haste / Mastery
        Harmonious = 8794, // Vers / Mastery
        Quickblade = 8795, // Crit / Vers
    }
}
