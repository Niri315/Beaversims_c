let allItems = [];
let loaded;

export async function loadItems() {
    if (loaded) return loaded;
    loaded = (async () => {
        const res = await fetch("/data/equippable-items-full.json", { cache: "force-cache" });
        if (!res.ok) throw new Error("Failed to load items.json");
        allItems = await res.json();
    })();
    return loaded;
}

export function searchItems(query, ilvlMin = 0) {
    const q = String(query || "").toLowerCase();
    if (!q) return [];
    return allItems
        .filter(it => it.name?.toLowerCase().includes(q) && (!ilvlMin || (it.itemLevel ?? 0) >= ilvlMin))
        .slice(0, 50);
}