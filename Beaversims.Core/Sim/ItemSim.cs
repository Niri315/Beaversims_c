using Beaversims.Core.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        // For non hasted stat trinket sims.
        public Dictionary<StatName, double> IncRatings { get; set; } = new Dictionary<StatName, double>();
        public Dictionary<StatName, double> IncEffs{ get; set; } = new Dictionary<StatName, double>();
        public double HasteCapCTLoss { get; set; } = 0;

        // Match the Dictionary ctor-overload your code depends on
        public GearSet(IEqualityComparer<ItemSlot>? comparer = null)
        {
            _items = new Dictionary<ItemSlot, GainItem?>(comparer);
            Gains = Utils.InitGainDict();
            IncRatings = Utils.InitStatDict();
            IncEffs = Utils.InitStatDict();

        }

        // Optional convenience ctor
        public GearSet(string name, int id, IEqualityComparer<ItemSlot>? comparer = null)
            : this(comparer)
        {
            Name = name;
            Id = id;
            Gains = Utils.InitGainDict();
            IncRatings = Utils.InitStatDict();
            IncEffs = Utils.InitStatDict();
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
                Gains = CloneGains(source.Gains), // important
                IncRatings = new Dictionary<StatName, double>(),
                IncEffs = new Dictionary<StatName, double>()
            };

            foreach (var kv in source)
            {
                // Deep-clone each GainItem (already defined)
                clone[kv.Key] = kv.Value is null ? null : (GainItem)kv.Value.Clone();
                //Console.WriteLine(clone[kv.Key].Name);
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



        public static void SetGearSetIds(User user)
        {
            for (int i = 0; i < user.AltGearSets.Count; i++) 
            {
                user.AltGearSets[i].Id = i;
            }

        }


       public static void AddSpecialEffects(User user)
        {

            foreach (var gearSet in user.AltGearSets)
            {
                foreach (var gear in gearSet.Values)
                {
                    var effect = SpecialEffectFactory.CreateFromName(gear.Name);
                    if (effect == null) continue;

                    var existing = user.SimEffects.FirstOrDefault(e => e.GetType() == effect.GetType());
                    var eff = existing ?? effect;

                    eff.Ilvls[gearSet.Id] = gear.Ilvl;
                    eff.ItemSlots[gearSet.Id] = gear.ItemSlot;
                    eff.RatingInc[gearSet.Id] = new Dictionary<StatName, double>();

                    if (existing == null)
                    {
                        user.SimEffects.Add(eff);
                    }

                }

            }
        }
        public static void AddAltAbilityStuff(User user)
        {
            foreach (var ability in user.Abilities)
            {
                ability.AltHeal.AddRange(
                    Enumerable.Range(0, user.AltGearSets.Count)
                              .Select(_ => new HealDataContainer())
                );

                ability.AltDamage.AddRange(
                    Enumerable.Range(0, user.AltGearSets.Count)
                              .Select(_ => new DmgDataContainer())
                );

            }
        }


        public static void SwDummyItems(User user)
        {
            foreach (StatName stat in Enum.GetValues(typeof(StatName)))
            {
                var swGear = DeepCloneGearset(user.AltGearSets[0]);
                swGear.Name = stat.ToString();
                swGear[ItemSlot.Head].addStatRating(stat, 1);
                user.AltGearSets.Add(swGear);
            }
        }

        public static void StatAllocTest(User user)
        {
            int secStatMax = 4800;
            int intellectAmount = 135000;
            int staminaAmount = 780000;

            double[] hasteGrid = { 0.00, 0.05, 0.10, 0.15, 0.20, 0.25, 0.30, 0.35, 0.4, 0.45, 0.5, 0.55, 0.6 };
            double[] critGrid = { 0.00, 0.05, 0.10, 0.15, 0.20, 0.25, 0.30, 0.35, 0.4, 0.45, 0.5, 0.55, 0.6 };
            double[] masteryGrid = { 0.00, 0.05, 0.10, 0.15, 0.20, 0.25, 0.30, 0.35, 0.4, 0.45, 0.5, 0.55, 0.6 };

            foreach (var h in hasteGrid)
            {
                foreach (var c in critGrid)
                {
                    foreach (var m in masteryGrid)
                    {
                        double v = 1.0 - (h + c + m);

                        // Must sum to 1 and satisfy caps
                        if (v < 0) continue;
                        if (h > 0.60 + 1e-9) continue;
                        if (c > 0.60 + 1e-9) continue;
                        if (m > 0.60 + 1e-9) continue;
                        if (v > 0.60 + 1e-9) continue;

                        var gs = new GearSet();
                        gs.Name = $"{Pct(h)} haste, {Pct(c)} crit, {Pct(m)} mastery, {Pct(v)} vers";

                        gs[ItemSlot.Head] = ItemGenerator.CreateItem("Soaring Behemoth's Greathelm", 1, ItemSlot.Head, []);
                        gs[ItemSlot.Head].Stats[StatName.Intellect] = intellectAmount;
                        gs[ItemSlot.Head].Stats[StatName.Stamina] = staminaAmount;

                        // Allocate secondaries with exact-total correction
                        int haste = (int)Math.Round(secStatMax * h);
                        int crit = (int)Math.Round(secStatMax * c);
                        int mastery = (int)Math.Round(secStatMax * m);
                        int vers = (int)Math.Round(secStatMax * v);

                        int total = haste + crit + mastery + vers;
                        int diff = secStatMax - total;
                        if (diff != 0)
                        {
                            // Adjust the largest bucket so Crit+Haste+Mastery+Vers == secStatMax
                            int k = ArgMax(new[] { h, c, m, v });
                            switch (k)
                            {
                                case 0: haste += diff; break;
                                case 1: crit += diff; break;
                                case 2: mastery += diff; break;
                                case 3: vers += diff; break;
                            }
                        }

                        gs[ItemSlot.Head].Stats[StatName.Haste] = haste;
                        gs[ItemSlot.Head].Stats[StatName.Crit] = crit;
                        gs[ItemSlot.Head].Stats[StatName.Mastery] = mastery;
                        gs[ItemSlot.Head].Stats[StatName.Vers] = vers;

                        user.AltGearSets.Add(gs);
                    }
                }
            }

            // --- helpers ---
            static string Pct(double r) => $"{(int)Math.Round(r * 100)}%";
            static int ArgMax(double[] a)
            {
                int idx = 0; double best = a[0];
                for (int i = 1; i < a.Length; i++) if (a[i] > best) { best = a[i]; idx = i; }
                return idx;
            }
        }


        public static void CustomGearSets(User user)
        {
            var altGearSet0 = DeepCloneGearset(user.AltGearSets[0]);
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
            user.AltGearSets.Add(altGearSet0);



            var HasteTest1 = DeepCloneGearset(user.Gear);
            HasteTest1.Name = "Haste +1";
            HasteTest1[ItemSlot.Head].addStatRating(StatName.Haste, 1);
            user.AltGearSets.Add(HasteTest1);

            var HasteTest3 = DeepCloneGearset(user.Gear);
            HasteTest3.Name = "Haste - 10000";
            HasteTest3[ItemSlot.Head].addStatRating(StatName.Haste, -10000);
            user.AltGearSets.Add(HasteTest3);

            var HasteTest2 = DeepCloneGearset(user.Gear);
            HasteTest2.Name = "Haste + 10000";
            HasteTest2[ItemSlot.Head].addStatRating(StatName.Haste, 10000);
            user.AltGearSets.Add(HasteTest2);

            //var hasteTest1 = DeepCloneGearset(user.Gear);
            //hasteTest1.Name = "Haste + 500";
            //hasteTest1[ItemSlot.Head].addStatRating(StatName.Haste, 500);
            //user.AltGearSets.Add(hasteTest1);

            //var hasteTest2 = DeepCloneGearset(user.Gear);
            //hasteTest2.Name = "Haste + 1000";
            //hasteTest2[ItemSlot.Head].addStatRating(StatName.Haste, 1000);
            //user.AltGearSets.Add(hasteTest2);

            //AddSpecialEffects(user.Gear);
            //var trinketTest = DeepCloneGearset(user.Gear);
            //trinketTest.Name = "Trinket test +3ilvl";
            //trinketTest[ItemSlot.Trinket1] = ItemGenerator.CreateItem("Astral Antenna", 726, ItemSlot.Trinket1, []);
            //user.AltGearSets.Add(trinketTest);

            //var trinketTest2 = DeepCloneGearset(user.AltGearSets[0]);
            //trinketTest2.Name = "Trinket test +7ilvl";
            //trinketTest2[ItemSlot.Trinket1] = ItemGenerator.CreateItem("Astral Antenna", 730, ItemSlot.Trinket1, []);
            //user.AltGearSets.Add(trinketTest2);

            //var legTest = DeepCloneGearset(user.AltGearSets[0]);
            //legTest.Name = "Legs +7ilvl";
            //legTest[ItemSlot.Legs] = ItemGenerator.CreateItem("Cuisses of the Lucent Battalion", 737, ItemSlot.Legs, []);
            //user.AltGearSets.Add(legTest);

            //var trinkTestNeg = DeepCloneGearset(user.AltGearSets[0]);
            //trinkTestNeg.Name = "Trinket test -7ilvl";
            //trinkTestNeg[ItemSlot.Trinket1] = ItemGenerator.CreateItem("Astral Antenna", 716, ItemSlot.Trinket1, []);
            //user.AltGearSets.Add(trinkTestNeg);

            //var legTestNeg = DeepCloneGearset(user.AltGearSets[0]);
            //legTestNeg.Name = "Legs -7ilvl";
            //legTestNeg[ItemSlot.Legs] = ItemGenerator.CreateItem("Cuisses of the Lucent Battalion", 723, ItemSlot.Legs, []);
            //user.AltGearSets.Add(legTestNeg);

        }

        public static void CreateGearSets(User user)
        {
            // Obs ! For stat weights we cant currently change special effects in ref. To be able to do this we need to change
            // how we deal with SimImpurities.
            var refSet = DeepCloneGearset(user.Gear);
            refSet.Name = "Ref";
            user.AltGearSets.Add(refSet);

            if (Constants.swOption)
            {
                SwDummyItems(user);

            }
            else
            {
                CustomGearSets(user);
            }

            SetGearSetIds(user);
            AddSpecialEffects(user);
            AddAltAbilityStuff(user);

        }
    }
}
