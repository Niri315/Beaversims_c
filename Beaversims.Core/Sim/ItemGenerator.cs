using Beaversims.Core.Data.Dbc;
using Beaversims.Core.Sim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{

    internal class GainItem : Item
    {
        //public GainDict Gains { get; set; }
        public Dictionary<StatName, double> Stats = [];
        public override object Clone()
        {
            var clone = (GainItem)base.Clone();
            clone.Stats = new Dictionary<StatName, double>(Stats);
            return clone;
        }
        public void addStatRating(StatName statName, double rating)
        {
            if (!Stats.ContainsKey(statName))
            {
                Stats[statName] = rating;
            }
            else
            {
                Stats[statName] += rating;
            }
        }
        public GainItem(string name, int ilvl, ItemSlot itemSlot)
            : base(ItemDatabase.Items[name].Id, name, ilvl, itemSlot)
        {
        //    Gains = Utils.InitGainDict();
        }
    }
}

namespace Beaversims.Core.Sim
{
    internal static class ItemGenerator
    {
        public static StatName? TertiaryExists(int bonusId) => bonusId switch
        {
            (int)BonusIds.Avoidance => StatName.Avoidance,
            (int)BonusIds.Leech => StatName.Leech,
            _ => null
        };

        public static List<int>? FindCraftedStats(int bonusId) => bonusId switch
        {
            8790 => [32, 36],
            8791 => [32, 49],
            8792 => [40, 36],
            8793 => [49, 36],
            8794 => [49, 40],
            8795 => [32, 40],
            _ => null
        };

        public static void SetCraftedStats(GainItem gainItem, int bonusId, double itemRpp, double secAlloc)
        {
            var stats = FindCraftedStats(bonusId);
            if (stats is not null)
            {
                foreach (var statId in stats)
                {
                    var statName = Utils.IdToStatName(statId);
                    if (statName is StatName _statName)
                    {
                        var amount = itemRpp * secAlloc;
                        amount = ScUtils.ApplyMults(amount, gainItem.ItemSlot, _statName, gainItem.Ilvl);
                        if (!gainItem.Stats.ContainsKey(_statName)) { gainItem.Stats[_statName] = 0; }
                        gainItem.Stats[_statName] += amount;
                    }

                }
            }
        }

        public static void ParseBonusIds(GainItem gainItem, List<int> bonusIds, double itemRpp, double secAlloc)
        {
            foreach (int bonusId in bonusIds)
            {
                if (TertiaryExists(bonusId) is StatName stat)
                {
                    var tertAmount = ScUtils.ApplyMults(itemRpp, gainItem.ItemSlot, stat, gainItem.Ilvl) * ScUtils.tertiaryStatAlloc;
                    if (!gainItem.Stats.ContainsKey(stat)) { gainItem.Stats[stat] = 0; }
                    gainItem.Stats[stat] = tertAmount;  // ??
                }
                SetCraftedStats(gainItem, bonusId, itemRpp, secAlloc);
            }
        }

        public static GainItem CreateItem(string itemName, int ilvl, ItemSlot itemSlot, List<int> bonusIds)
        {

            var gainItem = new GainItem(itemName, ilvl, itemSlot);

            var itemData = ItemDatabase.Items[itemName];
            var itemRpp = ScUtils.GetItemRPP(ilvl, itemSlot, itemData);
            //Console.WriteLine($"{itemData.Name} - Ilvl: {ilvl}, Itemclass: {itemData.ItemClass}");

            var craftedSecAlloc = 0.0;
            if (itemData.Stats != null)
            {
                foreach (var stat in itemData.Stats)
                {
                    if (stat.Id == 24 || stat.Id == 25)  // Need to change this incase there are crafts that are not 50/50 secondary stat split.
                    {
                        craftedSecAlloc = (double)stat.Alloc / 10000;
                    } 
                    if (Utils.IdToStatName(stat.Id) is not StatName statName)
                        continue;
                    if (!gainItem.Stats.ContainsKey(statName)) { gainItem.Stats[statName] = 0; }
                    double alloc = (double)stat.Alloc / 10000;
                    var amount = alloc * itemRpp;
                    amount = ScUtils.ApplyMults(amount, itemSlot, statName, ilvl);
                    gainItem.Stats[statName] += amount;
                }
            }
            ParseBonusIds(gainItem, bonusIds, itemRpp, craftedSecAlloc);
            //foreach (var stat in gainItem.Stats)
            //{
            //    Console.WriteLine($"{stat}");

            //}

            return gainItem;
        }
    }
}
