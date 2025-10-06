using Beaversims.Core.Data.Dbc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Sim
{
    public enum ItemScaleType { Armor, Weapon, Trinket, Jewelry }  // Ordered for scale data

    internal static class ScUtils
    {
        public const double tertiaryStatAlloc = 0.3; //Derived

        public static double GetEffectRPP(int ilvl, int scalingClass)
        {
            var rpp = Data.Dbc.RandomPropPoints.Data[ilvl - 1];
            double effectRpp = scalingClass switch
            {
                -1 => rpp.E0,
                -7 => rpp.E0, 
                -8 => rpp.DamageSecondary,
                -9 => rpp.DamageReplaceStat,
                _ => 0 
            };

            return effectRpp;
        }

        public static double GetItemRPP(int ilvl, ItemSlot itemSlot, ItemData itemData)
        {
            var inventoryType = itemData.InventoryType;
            var rpp = Data.Dbc.RandomPropPoints.Data[ilvl - 1];
            if (itemSlot == ItemSlot.MainHand)   //E0 for 2h, E3 for main hand
            {
                if (inventoryType == 13) { return rpp.E3; }
                else if (inventoryType == 17) { return rpp.E0; }
            }
            double itemRpp = itemSlot switch
            {
                ItemSlot.Head => rpp.E0,
                ItemSlot.Neck => rpp.E2,
                ItemSlot.Shoulders => rpp.E1,
                ItemSlot.Back => rpp.E2,
                ItemSlot.Chest => rpp.E0,
                ItemSlot.Wrist => rpp.E2,
                ItemSlot.MainHand => rpp.E3,
                ItemSlot.OffHand => rpp.E3,
                ItemSlot.Hands => rpp.E1,
                ItemSlot.Waist => rpp.E1,
                ItemSlot.Legs => rpp.E0,
                ItemSlot.Feet => rpp.E1,
                ItemSlot.Finger1 => rpp.E2,
                ItemSlot.Finger2 => rpp.E2,
                ItemSlot.Trinket1 => rpp.E1,
                ItemSlot.Trinket2 => rpp.E1,

                _ => 0
            };

            return itemRpp;
        }

        public static ItemScaleType ItemslotToScaleType(ItemSlot itemSlot) => itemSlot switch
        {
            ItemSlot.Neck => ItemScaleType.Jewelry,
            ItemSlot.Finger1 => ItemScaleType.Jewelry,
            ItemSlot.Finger2 => ItemScaleType.Jewelry,
            ItemSlot.Trinket1 => ItemScaleType.Trinket,
            ItemSlot.Trinket2 => ItemScaleType.Trinket,
            ItemSlot.MainHand => ItemScaleType.Weapon,
            ItemSlot.OffHand => ItemScaleType.Weapon,
            _ => ItemScaleType.Armor
        };


        public static double ApplyMults(double amount, ItemSlot itemSlot, StatName statName, int ilvl)
        {
            var itemST = ItemslotToScaleType(itemSlot);
            if (Utils.IsSecondaryStat(statName) || Utils.IsTertiaryStat(statName))
            {
                amount*= ScScaleData.CombatRatingsMultByIlvl[(int)itemST][ilvl - 1];
            }
            else if (statName == StatName.Stamina)
            {
                amount *= ScScaleData.StaminaMultByIlvl[(int)itemST][ilvl - 1];
            }
            return amount;
        }

        public static double ScaledEffectValue(int ilvl, ItemSlot itemSlot, StatName statName, ScalingData scalingData)
        {
            var amount = GetEffectRPP(ilvl, scalingData.Class);
            amount = ApplyMults(amount, itemSlot, statName, ilvl);
            amount *= scalingData.Coef;
            return amount;
        }
    }
}
