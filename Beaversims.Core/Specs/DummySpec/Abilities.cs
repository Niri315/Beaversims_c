using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Beaversims.Core.Specs.DummySpec
{
    internal class PrismaticBarrier : HpalAbility
    {
        public const string name = "Prismatic Barrier";
        public PrismaticBarrier()
        {
            Name = name;
            CastTime = Constants.GCD;
            Scalers.UnionWith([SN.Intellect, SN.Vers]);
        }
    }
    internal class TempestBarrier : HpalAbility
    {
        public const string name = "Tempest Barrier";
        public TempestBarrier()
        {
            Name = name;
            Scalers.UnionWith([SN.Stamina]);
        }
    }
    internal class DivertedEnergy : HpalAbility
    {
        public const string name = "Diverted Energy";
        public DivertedEnergy()
        {
            Name = name;
            Scalers.UnionWith([SN.Intellect, SN.Vers]);
        }
    }
}
