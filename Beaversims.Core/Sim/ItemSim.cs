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

            int secStatMax = 4800;

            //var statAlloc0 = new GearSet();
            //statAlloc0.Name = "Even stats";
            //statAlloc0[ItemSlot.Head] = ItemGenerator.CreateItem("Soaring Behemoth's Greathelm", 1, ItemSlot.Head, []);
            //statAlloc0[ItemSlot.Head].Stats[StatName.Intellect] = 135000;
            //statAlloc0[ItemSlot.Head].Stats[StatName.Stamina] = 780000;
            //statAlloc0[ItemSlot.Head].Stats[StatName.Crit] = secStatMax / 4;
            //statAlloc0[ItemSlot.Head].Stats[StatName.Haste] = secStatMax / 4;
            //statAlloc0[ItemSlot.Head].Stats[StatName.Mastery] = secStatMax / 4;
            //statAlloc0[ItemSlot.Head].Stats[StatName.Vers] = secStatMax / 4;
            //user.altGearSets.Add(statAlloc0);

            var leechTest1 = DeepCloneGearset(user.Gear);
            leechTest1.Name = "1 leech/int";
            leechTest1[ItemSlot.Head].addStatRating(StatName.Leech, 1);
            leechTest1[ItemSlot.Head].addStatRating(StatName.Intellect, 1);
            user.altGearSets.Add(leechTest1);

            var hasteTest1 = DeepCloneGearset(user.Gear);
            hasteTest1.Name = "Haste + 500";
            hasteTest1[ItemSlot.Head].addStatRating(StatName.Haste, 500);
            user.altGearSets.Add(hasteTest1);

            var hasteTest2 = DeepCloneGearset(user.Gear);
            hasteTest2.Name = "Haste + 1000";
            hasteTest2[ItemSlot.Head].addStatRating(StatName.Haste, 1000);
            user.altGearSets.Add(hasteTest2);




            //double[] hasteGrid = { 0.00, 0.05, 0.10, 0.15, 0.20, 0.25, 0.30, 0.35, 0.4, 0.45, 0.5, 0.55, 0.6 };
            //double[] critGrid = { 0.00, 0.05, 0.10, 0.15, 0.20, 0.25, 0.30, 0.35, 0.4, 0.45, 0.5, 0.55, 0.6 };
            //double[] masteryGrid = { 0.00, 0.05, 0.10, 0.15, 0.20, 0.25, 0.30, 0.35, 0.4, 0.45, 0.5, 0.55, 0.6 };

            //foreach (var h in hasteGrid)
            //{
            //    foreach (var c in critGrid)
            //    {
            //        foreach (var m in masteryGrid)
            //        {
            //            double v = 1.0 - (h + c + m);

            //            // Must sum to 1 and satisfy caps
            //            if (v < 0) continue;
            //            if (h > 0.60 + 1e-9) continue;
            //            if (c > 0.60 + 1e-9) continue;
            //            if (m > 0.60 + 1e-9) continue;
            //            if (v > 0.60 + 1e-9) continue;

            //            // Build the gearset
            //            var gs = new GearSet();
            //            gs.Name = $"{Pct(h)} haste, {Pct(c)} crit, {Pct(m)} mastery, {Pct(v)} vers";

            //            gs[ItemSlot.Head] = ItemGenerator.CreateItem("Soaring Behemoth's Greathelm", 1, ItemSlot.Head, []);
            //            gs[ItemSlot.Head].Stats[StatName.Intellect] = 135000;
            //            gs[ItemSlot.Head].Stats[StatName.Stamina] = 780000;

            //            // Allocate secondaries with exact-total correction
            //            int haste = (int)Math.Round(secStatMax * h);
            //            int crit = (int)Math.Round(secStatMax * c);
            //            int mastery = (int)Math.Round(secStatMax * m);
            //            int vers = (int)Math.Round(secStatMax * v);

            //            int total = haste + crit + mastery + vers;
            //            int diff = secStatMax - total;
            //            if (diff != 0)
            //            {
            //                // Adjust the largest bucket so Crit+Haste+Mastery+Vers == secStatMax
            //                int k = ArgMax(new[] { h, c, m, v });
            //                switch (k)
            //                {
            //                    case 0: haste += diff; break;
            //                    case 1: crit += diff; break;
            //                    case 2: mastery += diff; break;
            //                    case 3: vers += diff; break;
            //                }
            //            }

            //            gs[ItemSlot.Head].Stats[StatName.Haste] = haste;
            //            gs[ItemSlot.Head].Stats[StatName.Crit] = crit;
            //            gs[ItemSlot.Head].Stats[StatName.Mastery] = mastery;
            //            gs[ItemSlot.Head].Stats[StatName.Vers] = vers;

            //            user.altGearSets.Add(gs);
            //        }
            //    }
            //}

            //// --- helpers ---
            //static string Pct(double r) => $"{(int)Math.Round(r * 100)}%";
            //static int ArgMax(double[] a)
            //{
            //    int idx = 0; double best = a[0];
            //    for (int i = 1; i < a.Length; i++) if (a[i] > best) { best = a[i]; idx = i; }
            //    return idx;
            //}

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

            //user.HCGM.AddRange(
            //    Enumerable.Range(0, user.altGearSets.Count)
            //              .Select(_ => 1.0)
            //);
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
                //ability.HasteGainMods.AddRange(
                //    Enumerable.Range(0, user.altGearSets.Count)
                //              .Select(_ => 1.0)
                //);
                //ability.HCGM.AddRange(
                //    Enumerable.Range(0, user.altGearSets.Count)
                //              .Select(_ => 1.0)
                //);
                //ability.CastTimeGains.AddRange(
                //    Enumerable.Range(0, user.altGearSets.Count)
                //              .Select(_ => 0.0)
                //);

            }
        }
    }
}
