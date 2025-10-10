using Beaversims.Core.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Beaversims.Core.Sim
{

    internal class GearSet : IEnumerable<KeyValuePair<ItemSlot, GainItem?>>
    {
        private readonly Dictionary<ItemSlot, GainItem?> _items;

        public string Name { get; set; }
        public int Id { get; set; }

        public GainDict Gains { get; set; }

        // Match the Dictionary ctor-overload your code depends on
        public GearSet(IEqualityComparer<ItemSlot>? comparer = null)
        {
            _items = new Dictionary<ItemSlot, GainItem?>(comparer);
            Gains = Utils.InitGainDict();

        }

        // Optional convenience ctor
        public GearSet(string name, int id, IEqualityComparer<ItemSlot>? comparer = null)
            : this(comparer)
        {
            Name = name;
            Id = id;
            Gains = Utils.InitGainDict();
        }

        // Preserve dictionary-like indexer usage: gearset[ItemSlot.Head]
        public GainItem? this[ItemSlot slot]
        {
            get => _items[slot];
            set => _items[slot] = value;
        }

        // Expose the comparer so cloning code can reuse it
        public IEqualityComparer<ItemSlot> Comparer => _items.Comparer;

        // Common dictionary members (optional but handy)
        public int Count => _items.Count;
        public bool ContainsKey(ItemSlot key) => _items.ContainsKey(key);
        public bool TryGetValue(ItemSlot key, out GainItem? value) => _items.TryGetValue(key, out value);
        public void Add(ItemSlot key, GainItem? value) => _items.Add(key, value);
        public bool Remove(ItemSlot key) => _items.Remove(key);
        public void Clear() => _items.Clear();
        public IEnumerable<ItemSlot> Keys => _items.Keys;
        public IEnumerable<GainItem?> Values => _items.Values;

        // Enumeration support so foreach (var kv in gearset) works
        public IEnumerator<KeyValuePair<ItemSlot, GainItem?>> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    internal static class ItemSim
    {
 
        public static GearSet DeepCloneGearset(this GearSet source)
        {
            // Create a new dictionary with the same comparer, if any
            var clone = new GearSet(source.Comparer)
            {
                Name = source.Name,
                Id = source.Id,
                Gains = CloneGains(source.Gains) // important
            };

            foreach (var kv in source)
            {
                // Deep-clone each GainItem (already defined)
                clone[kv.Key] = kv.Value is null ? null : (GainItem)kv.Value.Clone();
            }

            return clone;
        }
        private static GainDict CloneGains(GainDict gains)
        {
            if (gains is null) return Utils.InitGainDict();

            var copy = Utils.InitGainDict();
            foreach (var kv in gains)                // deep-copy if values are reference types
                copy[kv.Key] = kv.Value;             // for primitives/structs this is fine

            return copy;
        }


        public static void SwDummyItems(User user)
        {
            foreach (StatName stat in Enum.GetValues(typeof(StatName)))
            {
                var swGear = DeepCloneGearset(user.Gear);
                swGear.Name = stat.ToString();
                swGear[ItemSlot.Head].addStatRating(stat, 1);
                user.altGearSets.Add(swGear);
            }
        }

        public static void CreateGearSets(User user)
        {
            var refSet = DeepCloneGearset(user.Gear);
            refSet.Name = "Ref";
            user.altGearSets.Add(refSet);


            SwDummyItems(user);

            var altGearSet0 = DeepCloneGearset(user.Gear);
            altGearSet0.Name = "Wishlist";
            altGearSet0[ItemSlot.Head] = ItemGenerator.CreateItem("Soaring Behemoth's Greathelm", 717, ItemSlot.Head, [(int)BonusIds.Leech]);
            altGearSet0[ItemSlot.Neck] = ItemGenerator.CreateItem("Amulet of Earthen Craftsmanship", 727, ItemSlot.Neck, [(int)BonusIds.Quickblade]);
            altGearSet0[ItemSlot.Shoulders] = ItemGenerator.CreateItem("Chargers of the Lucent Battalion", 730, ItemSlot.Shoulders, []);
            altGearSet0[ItemSlot.Chest] = ItemGenerator.CreateItem("Cuirass of the Lucent Battalion", 730, ItemSlot.Chest, []);
            altGearSet0[ItemSlot.Wrist] = ItemGenerator.CreateItem("Everforged Vambraces", 727, ItemSlot.Wrist, [(int)BonusIds.Quickblade]);
            altGearSet0[ItemSlot.MainHand] = ItemGenerator.CreateItem("Voidglass Sovereign's Blade", 730, ItemSlot.MainHand, []);
            altGearSet0[ItemSlot.OffHand] = ItemGenerator.CreateItem("Ward of the Weaving-Beast", 730, ItemSlot.OffHand, []);
            altGearSet0[ItemSlot.Hands] = ItemGenerator.CreateItem("Protectors of the Lucent Battalion", 730, ItemSlot.Hands, []);
            altGearSet0[ItemSlot.Waist] = ItemGenerator.CreateItem("Seal of the Lucent Battalion", 730, ItemSlot.Waist, [(int)BonusIds.Leech]);
            altGearSet0[ItemSlot.Legs] = ItemGenerator.CreateItem("Cuisses of the Lucent Battalion", 730, ItemSlot.Legs, []);
            altGearSet0[ItemSlot.Feet] = ItemGenerator.CreateItem("Interloper's Plated Sabatons", 730, ItemSlot.Feet, []);
            altGearSet0[ItemSlot.Finger1] = ItemGenerator.CreateItem("Ring of Earthen Craftsmanship", 727, ItemSlot.Finger1, [(int)BonusIds.Quickblade]);
            altGearSet0[ItemSlot.Finger2] = ItemGenerator.CreateItem("Devout Zealot's Ring", 730, ItemSlot.Finger2, []);
            user.altGearSets.Add(altGearSet0); 

            //foreach (var gearset in user.altGearSets)
            //{
            //    gearset[ItemSlot.Head].addStatRating(StatName.Haste, 100);
            //}

            //var altGearSet1 = DeepCloneGearset(user.Gear);
            //altGearSet1.Name = "Devout Zealot ring slot 1";
            //altGearSet0[ItemSlot.Finger1] = ItemGenerator.CreateItem("Devout Zealot's Ring", 730, ItemSlot.Finger1, []);
            //altGearSet1[ItemSlot.Finger2] = ItemGenerator.CreateItem("Ring of Earthen Craftsmanship", 727, ItemSlot.Finger2, [(int)BonusIds.Quickblade]);
            //user.altGearSets.Add(altGearSet1);

            //var altGearSet1 = DeepCloneGearset(user.Gear);
            //altGearSet1.Name = "Dimensius ring slot 2";
            //altGearSet1[ItemSlot.Finger2] = ItemGenerator.CreateItem("Band of the Shattered Soul", 723, ItemSlot.Finger2, []);
            //user.altGearSets.Add(altGearSet1);

            //Console.WriteLine(altGearSet0[ItemSlot.Finger1].Name);
            //foreach (var stat in altGearSet0[ItemSlot.Finger1].Stats)
            //{
            //    Console.WriteLine($"{stat.Key}: {stat.Value}");
            //}
            //Console.WriteLine(user.Gear[ItemSlot.Finger1].Name);
            //foreach (var stat in user.Gear[ItemSlot.Finger1].Stats)
            //{
            //    Console.WriteLine($"{stat.Key}: {stat.Value}");
            //}


            //var altGearSet0 = DeepCloneGearset(user.Gear);
            //altGearSet0.Name = "Soulbinder neck 730";
            //altGearSet0[ItemSlot.Neck] = ItemGenerator.CreateItem("Chrysalis of Sundered Souls", 730, ItemSlot.Neck, []);
            //user.altGearSets.Add(altGearSet0);

            //var altGearSet1 = DeepCloneGearset(user.Gear);
            //altGearSet1.Name = "cur neck 8/8";
            //altGearSet1[ItemSlot.Neck] = ItemGenerator.CreateItem("Ornately Engraved Amplifier", 717, ItemSlot.Neck, []);
            //user.altGearSets.Add(altGearSet1);

            //var altGearSet2 = DeepCloneGearset(user.Gear);
            //altGearSet2.Name = "cur neck 4/8";
            //altGearSet2[ItemSlot.Neck] = ItemGenerator.CreateItem("Ornately Engraved Amplifier", 704, ItemSlot.Neck, []);
            //user.altGearSets.Add(altGearSet2);


            //var altGearSet2 = DeepCloneGearset(user.Gear);
            //altGearSet2.Name = "Belt: crit/vers + leech";
            //altGearSet2[ItemSlot.Waist] = ItemGenerator.CreateItem("Improvisational Girdle", 723, ItemSlot.Waist, [(int)BonusIds.Leech]);
            //user.altGearSets.Add(altGearSet2);

            //var altGearSet0 = DeepCloneGearset(user.Gear);
            //altGearSet0.Name = "Helm 710 + leech";
            //altGearSet0[ItemSlot.Head] = ItemGenerator.CreateItem("Soaring Behemoth's Greathelm", 710, ItemSlot.Head, [(int)BonusIds.Leech]);
            //user.altGearSets.Add(altGearSet0);

            //var altGearSet1 = DeepCloneGearset(user.Gear);
            //altGearSet1.Name = "Craft helm";
            //altGearSet1[ItemSlot.Head] = ItemGenerator.CreateItem("Everforged Helm", 720, ItemSlot.Head, [(int)BonusIds.Quickblade]);
            //user.altGearSets.Add(altGearSet1);

            //var altGearSet2 = DeepCloneGearset(user.Gear);
            //altGearSet2.Name = "Helm 723 shitstats";
            //altGearSet2[ItemSlot.Head] = ItemGenerator.CreateItem("Artoshion's Abyssal Stare", 723, ItemSlot.Head, []);
            //user.altGearSets.Add(altGearSet2);


            foreach (var ability in user.Abilities)
            {
                ability.AltHeal.AddRange(
                    Enumerable.Range(0, user.altGearSets.Count)
                              .Select(_ => new HealDataContainer())
                );

                ability.AltDamage.AddRange(
                    Enumerable.Range(0, user.altGearSets.Count)
                              .Select(_ => new DmgDataContainer())
                );
            }
        }
    }
}
