// top-gear.js

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

const TERTIARY_BY_BONUSID = {
    40: "Avoidance",
    41: "Leech",
};

const CRAFTED_BY_BONUSID = {
    8790: "Crit/Haste",     // Fireflash
    8791: "Crit/Mastery",   // Peerless
    8792: "Haste/Vers",     // Feverflare
    8793: "Haste/Mastery",  // Aurora
    8794: "Vers/Mastery",   // Harmonious
    8795: "Crit/Vers",      // Quickblade
};

function inferCraftedFromBonusIDs(bonusIDs) {
    if (!Array.isArray(bonusIDs)) return null;
    for (const id of bonusIDs) {
        const c = CRAFTED_BY_BONUSID[id];
        if (c) return c;    // one crafted combo per item
    }
    return null;
}

function inferTertiaryFromBonusIDs(bonusIDs) {
    if (!Array.isArray(bonusIDs)) return null;
    for (const id of bonusIDs) {
        const t = TERTIARY_BY_BONUSID[id];
        if (t) return t;   // only one tertiary per item
    }
    return null;
}

let ui = null;
let itemsReady; // single cached preload promise

document.addEventListener("DOMContentLoaded", async () => {



    ui = mountTopGearUI(gearMount);

    itemsReady = prefetchItems();

    let searchTimer;
    function scheduleSearch() {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(() => doSearch(searchInput.value.trim()), 150);
    }

    searchInput.addEventListener("input", scheduleSearch);
    ilvlInput.addEventListener("input", scheduleSearch);

    async function doSearch(q) {
        resultBox.innerHTML = "";
        if (!q) return;

        await itemsReady;
        const matches = searchItems(q);

        const ilvlOverride = getIlvlOverride();
        const frag = document.createDocumentFragment();

        for (const raw of matches) {
            const norm = normalizeSearchItem(raw, ilvlOverride);
            if (!norm) continue;

            const whData = buildWowheadData({ itemId: norm.id, ilvl: norm.itemLevel });

            const row = document.createElement("div");
            row.className = "tg-search-item";

            const iconLink = document.createElement("a");
            iconLink.href = `https://www.wowhead.com/item=${norm.id}`;
            iconLink.target = "_blank";
            iconLink.rel = "noopener";
            iconLink.setAttribute("data-wowhead", whData);
            const img = document.createElement("img");
            img.src = norm.icon || "";
            img.alt = "";
            iconLink.appendChild(img);

            const name = document.createElement("a");
            name.className = "name";
            name.href = `https://www.wowhead.com/item=${norm.id}`;
            name.target = "_blank";
            name.rel = "noopener";
            name.setAttribute("data-wowhead", whData);
            name.textContent = norm.name || `Item #${norm.id}`;

            const meta = document.createElement("span");
            meta.className = "meta";
            meta.textContent = norm.itemLevel ?? "";

            row.append(iconLink, name, meta);

            row.addEventListener("click", (e) => {
                if (e.target.closest("a")) return;
                ui.addItem(norm);
                resultBox.innerHTML = "";
            });

            frag.appendChild(row);
        }

        resultBox.appendChild(frag);

        if (window.$WowheadPower?.refreshLinks) {
            window.$WowheadPower.refreshLinks();
        } else if (window.WH?.Tooltips?.refreshLinks) {
            window.WH.Tooltips.refreshLinks();
        }
    }

    await renderPicker(pickerHost);

    onChange(({ reportCode, fight, healer }) => {
        const ready = !!(reportCode && fight && healer);
        runBtn.disabled = !ready;
        if (!ready) return;

        const itemsBySlot = new Map();
        for (const g of (healer.gear || [])) {
            const slot = Number(g.slot);
            const list = itemsBySlot.get(slot) || [];
            list.push({
                id: Number(g.id),
                slot,
                name: g.name || `Item #${g.id}`,
                itemLevel: g.itemLevel ?? g.ilvl ?? null,
                icon: iconUrl(g.icon?.replace(/\.jpg$/i, "")),
                badges: buildBadges(g),
                bonusIDs: Array.isArray(g.bonusIDs) ? g.bonusIDs.slice() : [],
                tertiary: inferTertiaryFromBonusIDs(g.bonusIDs),
                craftedStats: inferCraftedFromBonusIDs(g.bonusIDs),
            });
            itemsBySlot.set(slot, list);
        }
        ui.replaceBaseline(itemsBySlot);
    });

    runBtn.addEventListener("click", async () => {
        const { reportCode, fight, healer } = state;
        if (!ui || !reportCode || !fight || !healer) return;

        runBtn.disabled = true;
        runBtn.textContent = "Running Top Gear…";
        try {
            const logsRaw = await fetchLogsRaw(reportCode, fight.id, healer.id);
            const directive = ui.buildDirective();
            console.log(directive);
            const result = await runTopGear(logsRaw, healer.id, reportCode, JSON.stringify(directive));
            console.log(result);
            window.open(`/results.html?data=${encodeToUrl(result)}`, "_blank");
        } catch (err) {
            console.error(err);
            alert("Top Gear failed: " + (err.message || err));
        } finally {
            runBtn.textContent = "Run Top Gear";
            runBtn.disabled = false;
        }
    });
});

function buildWowheadData({ itemId, ilvl }) {
    const parts = [`item=${itemId}`];
    if (ilvl) parts.push(`ilvl=${ilvl}`);
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
    return iconName
        ? `https://wow.zamimg.com/images/wow/icons/large/${String(iconName).toLowerCase().replace(/\.jpg$/i, "")}.jpg`
        : "";
}

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
    // keep generic "Bonuses" from logs if present; gems/enchant removed
    if (Array.isArray(g.bonusIDs) && g.bonusIDs.length) out.push("Bonuses");
    return out;
}
