using Beaversims.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Beaversims.Core.Sim
{
    internal class ItemSim
    {
        //public static bool IsCompeteingItem(GainItem item, User user) => !user.Items.Contains(item) && ;

        public static void CheckResults(List<GainItem> allItems, User user)
        {
            foreach (var item in allItems)
            {
                GainItem gainItem = (GainItem)item;
                Console.WriteLine($"{gainItem.Name} Ilvl: {gainItem.Ilvl}");
                foreach (var gainType in gainItem.Gains)
                {
                    Console.WriteLine($"{gainType.Key}: {gainType.Value}");
                }
            }
        }

        public static void TopItems(List<Event> events, User user, Fight fight)
        {
            var simItems = new List<GainItem>
            {
                //new Sim.ItemGenerator("Soaring Behemoth's Greathelm", 720, ItemSlot.Head)
                Sim.ItemGenerator.CreateItem("Soaring Behemoth's Greathelm", 720, ItemSlot.Head, []),
                Sim.ItemGenerator.CreateItem("Soaring Behemoth's Greathelm", 710, ItemSlot.Head, [(int)BonusIds.Leech]),
                Sim.ItemGenerator.CreateItem("Charged Hexsword", 720, ItemSlot.MainHand, [(int)BonusIds.Harmonious])
            };

            var allItems = user.Items
                           .OfType<GainItem>()
                           .Concat(simItems)
                           .ToList();


            foreach (var evt in events)
            {
                foreach (var item in allItems)
                { 
                    GainItem gainItem = (GainItem)item;
                    foreach (var stat in gainItem.Stats)
                    {
                        var statName = stat.Key;
                        foreach (var gainType in evt.Gains[statName])
                        {
                            gainItem.Gains[gainType.Key] += evt.Gains[statName][gainType.Key] * gainItem.Stats[statName] / fight.TotalTime;

                        }
                    }
                }
            }
            CheckResults(allItems, user);
        } 
    }
}
