using Beaversims.Core.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Paladin.Holy
{
    internal static class HpalUtils
    {
        public static readonly HashSet<int> beaconIds = [Abilities.BeaconOfLight.buffId, Abilities.BeaconOfVirtue.buffId, Abilities.BeaconOfFaith.buffId];

        public static bool HasBeacon(Unit unit, User user) => unit.HasAnyBuffFromPlayer(beaconIds, user.Id);

        public static HashSet<Ability> GetSpenderAbilities(AbilityRepo abilities) => [abilities.Get(Abilities.ShieldOfTheRighteous.name), abilities.Get(Abilities.LightOfDawn.name), abilities.Get(Abilities.EternalFlame.name), abilities.Get(Abilities.WordOfGlory.name)];

    }
}
