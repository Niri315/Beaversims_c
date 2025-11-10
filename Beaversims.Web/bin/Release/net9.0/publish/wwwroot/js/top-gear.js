//top-gear.js

import { renderPicker } from "/js/shared/ui-picker.js";
import { state, onChange } from "/js/shared/state.js";
import { fetchLogsRaw } from "/js/shared/wcl.js";
import { runTopGear } from "/js/shared/wasm.js";
import { encodeToUrl } from "/js/shared/urlshare.js";
import { mountTopGearUI } from "/js/gear-ui.js";
import { loadItems, searchItems } from "/js/shared/item-search.js";
import { mapInventoryTypeToSlot } from "/js/shared/items.js";

const pickerHost = document.getElementById("import-form-container");
const gearMount = document.getElementById("top-gear-mount");
const runBtn = document.getElementById("run-top-gear-btn");
const searchInput = document.getElementById("item-search");
const ilvlInput = document.getElementById("item-ilvl");
const resultBox = document.getElementById("item-results");

let ui = null;
let itemsReady; // single cached preload promise

document.addEventListener("DOMContentLoaded", async () => {
    // 1) mount Top Gear UI immediately (empty; search works before logs)
    ui = mountTopGearUI(gearMount);

    // 2) preload item DB once (independent of logs)
    itemsReady = prefetchItems();

    // 3) wire global search (works with or without logs)
    let searchTimer;
    function scheduleSearch() {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(() => doSearch(searchInput.value.trim()), 150);
    }

    // Trigger search on both name *and* ilvl changes
    searchInput.addEventListener("input", scheduleSearch);
    ilvlInput.addEventListener("input", scheduleSearch);

    async function doSearch(q) {
        resultBox.innerHTML = "";
        if (!q) return;

        await itemsReady; // ensure DB loaded
        const matches = searchItems(q);

        const ilvlOverride = getIlvlOverride(); // from your field; may be null
        const frag = document.createDocumentFragment();

        for (const raw of matches) {
            // normalize for display + add
            const norm = normalizeSearchItem(raw, ilvlOverride);
            if (!norm) continue;

            const whData = buildWowheadData({ itemId: norm.id, ilvl: norm.itemLevel });

            const row = document.createElement("div");
            row.className = "tg-search-item";

            // Icon (clicking the icon opens wowhead; clicking the row adds the item)
            const iconLink = document.createElement("a");
            iconLink.href = `https://www.wowhead.com/item=${norm.id}`;
            iconLink.target = "_blank";
            iconLink.rel = "noopener";
            iconLink.setAttribute("data-wowhead", whData);
            const img = document.createElement("img");
            img.src = norm.icon || "";
            img.alt = "";
            iconLink.appendChild(img);

            // Name (wowhead tooltip)
            const name = document.createElement("a");
            name.className = "name";
            name.href = `https://www.wowhead.com/item=${norm.id}`;
            name.target = "_blank";
            name.rel = "noopener";
            name.setAttribute("data-wowhead", whData);
            name.textContent = norm.name || `Item #${norm.id}`;

            // Meta ilvl
            const meta = document.createElement("span");
            meta.className = "meta";
            meta.textContent = norm.itemLevel ?? "";

            // Assemble
            row.append(iconLink, name, meta);

            // Clicking the row adds the item (but don't add if user clicked the links)
            row.addEventListener("click", (e) => {
                // If clicked an <a>, let it open Wowhead instead of adding
                if (e.target.closest("a")) return;
                ui.addItem(norm);
                resultBox.innerHTML = "";
                // keep ilvl field value for rapid multiple adds
            });

            frag.appendChild(row);
        }

        resultBox.appendChild(frag);

        // refresh wowhead tooltips for the newly added links
        if (window.$WowheadPower?.refreshLinks) {
            window.$WowheadPower.refreshLinks();
        } else if (window.WH?.Tooltips?.refreshLinks) {
            window.WH.Tooltips.refreshLinks();
        }
    }

    // 4) render the WCL picker (separate concern)
    await renderPicker(pickerHost);

    // 5) when healer/fight picked, merge baseline into already-mounted UI
    onChange(({ reportCode, fight, healer }) => {
        const ready = !!(reportCode && fight && healer);
        runBtn.disabled = !ready;
        if (!ready) return;

        // Build itemsBySlot from healer.gear and replace baseline
        const itemsBySlot = new Map();
        for (const g of (healer.gear || [])) {
            const slot = Number(g.slot);
            const list = itemsBySlot.get(slot) || [];
            list.push({
                id: Number(g.id),
                slot,
                name: g.name || `Item #${g.id}`,
                ilvl: g.itemLevel ?? g.ilvl ?? null,
                icon: iconUrl(g.icon),
                badges: buildBadges(g),
            });
            itemsBySlot.set(slot, list);
        }
        ui.replaceBaseline(itemsBySlot); // wipes old baseline, injects new, autoselects
    });

    // 6) run Top Gear
    runBtn.addEventListener("click", async () => {
        const { reportCode, fight, healer } = state;
        if (!ui || !reportCode || !fight || !healer) return;

        runBtn.disabled = true;
        runBtn.textContent = "Running Top Gear…";
        try {
            const logsRaw = await fetchLogsRaw(reportCode, fight.id, healer.id);
            const directive = ui.buildDirective(); // grouped fingers/trinkets + arrays per slot
            const result = await runTopGear(logsRaw, healer.id, reportCode, JSON.stringify(directive));
            location.href = `/results.html?data=${encodeToUrl(result)}`;
        } catch (err) {
            console.error(err);
            alert("Top Gear failed: " + (err.message || err));
        } finally {
            runBtn.textContent = "Run Top Gear";
            runBtn.disabled = false;
        }
    });
});

/* ---------------- helpers ---------------- */
function buildWowheadData({ itemId, ilvl }) {
    const parts = [`item=${itemId}`];
    if (ilvl) parts.push(`ilvl=${ilvl}`);
    // extend later: bonus, gems, ench
    return parts.join("&");
}

function getIlvlOverride() {
    const n = parseInt(document.getElementById("item-ilvl")?.value ?? "", 10);
    return Number.isFinite(n) && n > 0 ? n : null;
}


function prefetchItems() {
    if ("requestIdleCallback" in window) {
        return new Promise((resolve) => {
            requestIdleCallback(async () => {
                await loadItems().catch(console.error);
                resolve();
            });
        });
    }
    return loadItems().catch(console.error);
}

function iconUrl(iconName) {
    // inputs like "inv_boots_06" → full wowhead/zam url
    return iconName
        ? `https://wow.zamimg.com/images/wow/icons/large/${String(iconName).toLowerCase()}.jpg`
        : "";
}

// Convert an entry from equippable-items-full.json into our UI item
function normalizeSearchItem(item, ilvlOverride) {
    const slot = mapInventoryTypeToSlot(item.inventoryType);
    if (slot == null) return null;

    const lvl = ilvlOverride ?? item.itemLevel ?? null;

    return {
        id: Number(item.id),
        slot,
        name: item.name || `Item #${item.id}`,
        itemLevel: lvl,
        icon: iconUrl(item.icon),
        badges: lvl ? [`★ ${lvl}`] : []
    };
}

function buildBadges(g) {
    const out = [];
    if (g.itemLevel ?? g.ilvl) out.push(`★ ${g.itemLevel ?? g.ilvl}`);
    if (g.permanentEnchant) out.push("Enchant");
    if (Array.isArray(g.gems) && g.gems.length) out.push(`${g.gems.length}× Gem`);
    if (Array.isArray(g.bonusIDs) && g.bonusIDs.length) out.push("Bonuses");
    return out;
}
