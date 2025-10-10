using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{

    public enum ItemSlot
    {
        None = 0,
        Head = 1,
        Neck = 2,
        Shoulders = 3,
        Shirt = 4,
        Chest = 5,
        Waist = 6,
        Legs = 7,
        Feet = 8,
        Wrist = 9,
        Hands = 10,
        Finger1 = 11,
        Finger2 = 12,
        Trinket1 = 13,
        Trinket2 = 14,
        Back = 15,
        MainHand = 16,
        OffHand = 17,
        Tabard = 19
    }

    internal class Item : ICloneable
    {
        public int Id { get; }
        public string Name { get; }
        public int Ilvl { get; }
        public ItemSlot ItemSlot { get; }

        public virtual object Clone() => MemberwiseClone();
        
        public Item(int id, string name, int ilvl, ItemSlot itemSlot)
        {
            Id = id;
            Name = name;
            Ilvl = ilvl;
            ItemSlot = itemSlot;
        }
    }

  
}
