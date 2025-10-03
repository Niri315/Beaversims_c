using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Beaversims.Core
{
    internal class GainItem : Item
    {

        public Dictionary<StatName, double> Stats;

        public GainItem(int id, string name, int ilvl, ItemSlot itemSlot)
            : base(id, name, ilvl, itemSlot)
        {
        }
    }
}

namespace Beaversims.Core.Sim
{
    internal static class ItemGenerator
    {
        public static GainItem CreateItem(int itemId, string itemName, int ilvl, ItemSlot itemSlot)
        {
            var item = new GainItem(itemId, itemName, ilvl, itemSlot);

            return item;
        }
    }
}
