using Beaversims.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Specs.Paladin.Holy
{
    internal static class MasteryTracker
    {
        private const double distanceMaxCap = 40;
        private const double distanceMinCap = 10;

        private static double CalcDistance(Coord targetLoc, Coord sourceLoc) => Math.Sqrt(Math.Pow(sourceLoc.x - targetLoc.x, 2) + Math.Pow(sourceLoc.y - targetLoc.y, 2));
        private static double CalcMasteryEff(double distance) => 1 -((distance - distanceMinCap) / (distanceMaxCap - distanceMinCap));
       
        public static void CleanUpCoords(UnitRepo allUnits)
        {
            foreach (var unit in allUnits)
            {
                unit.Coords = null;
            }
        }

        public static void FindCoords(List<Event> events)
        {
            //Reverse loop
            for (int i = events.Count - 1; i >= 0; i--)
            {
                var evt = events[i];
                UpdateCoords(evt);
            }
        }

        public static void UpdateCoords(Event evt)
        {
            if (evt.TargetCoords != null)
            {
                evt.TargetUnit.Coords = evt.TargetCoords;
            }
            if (evt.SourceCoords != null)
            {
                evt.SourceUnit.Coords = evt.SourceCoords;
            }
        }

        public static HashSet<Unit> FindStarterBeacons(UnitRepo allUnits)
        {
            var user = allUnits.GetUser();
            HashSet<Unit> beacons = [];
            foreach (var unit in allUnits)
            {
                if (HpalUtils.HasBeacon(unit, user))
                {
                    beacons.Add(unit);
                }
            }
            return beacons;
        }

        public static void TrackBeacons(Event evt, HashSet<Unit> beacons, User user)
        {
            if (evt is BuffEvent buffEvt && HpalUtils.beaconIds.Contains(evt.AbilityId) && evt.SourceUnit == user)
            {
                if (buffEvt.BuffApplyEvent)
                {
                    beacons.Add(evt.TargetUnit);
                }
                else if (buffEvt.BuffRemoveEvent)
                {
                    beacons.Remove(evt.TargetUnit);
                }
            }
            evt.BeaconCount = beacons.Count;
        }

        public static void SetMasteryEff(Event evt, HashSet<Unit> beacons, User user)
        {

            UpdateCoords(evt);
            if (evt is HealEvent healEvt && evt.Ability.ScalesWith(StatName.Mastery))
            {
                var masteryEff = 0.0;
                var minDistance = distanceMaxCap;
                if (evt.TargetUnit == user)
                {
                    masteryEff = 1.0;
                }
                else
                {

                    var userDistance = CalcDistance(evt.TargetUnit.Coords, user.Coords);
                    minDistance = Math.Min(minDistance, userDistance);
                    if (user.TalentRank(Talents.BeaconOfTheLightbringer.id) == 0)
                    {
                        masteryEff = CalcMasteryEff(minDistance);
 
                    }
                    else
                    {

                        if (beacons.Contains(evt.TargetUnit))
                        {
                            masteryEff = 1.0;
                       
                        }
                        else
                        {
                            Unit closestSource = user;
                            foreach (var beaconUnit in beacons)
                            {
                                var distance = CalcDistance(evt.TargetUnit.Coords, beaconUnit.Coords);
                                minDistance = Math.Min(minDistance, distance);
                                if (distance <= minDistance)
                                {
                                    closestSource = beaconUnit;
                                }
                            }
                            if (minDistance <= distanceMinCap)
                            {
                                masteryEff = 1.0;
                            }
                            else
                            {
                                masteryEff = CalcMasteryEff(minDistance);
                            }
                        }
                    }
                }
                healEvt.masteryEffectiveness = masteryEff;
    
            }
        }
        public static double MasteryGainCalc(Mastery mastery, double amount, double masteryEffectiveness)
    => (((amount / ((mastery.Eff * masteryEffectiveness) / (mastery.PercentRate * 100) + 1)) * masteryEffectiveness) / (mastery.PercentRate * 100)) * (1 - (mastery.Bracket * 0.1)) * mastery.Multi;

        public static void MasteryGains(HealEvent evt, User user)
        {
            var statName = StatName.Mastery;
            if (evt.Ability.ScalesWith(StatName.Mastery))
            {
                var stat = (Mastery)evt.UserStats.Get(statName);
                var gainType = GainType.Eff;
                var gainRaw = MasteryGainCalc(stat, evt.Amount.Raw, evt.masteryEffectiveness);
                var gain = evt.RawToEffConvert(gainRaw);
                evt.Gains[statName][gainType] += gain;
                user.Spec.DupliGainsHeal(evt, user, statName, gainRaw);

            }
        }
    }
}
