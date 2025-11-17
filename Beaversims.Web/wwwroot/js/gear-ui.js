// gear-ui.js

import { el } from "/js/shared/utils.js";

const IGNORED_SLOTS = new Set([3, 17]);
const FINGER_SLOTS = new Set([10, 11]);
const TRINKET_SLOTS = new Set([12, 13]);

let __uid = 0;
const newUid = () => (++__uid);

// ─── Bonus IDs (only those we still use) ────────────────────────
const BONUS_IDS = {
    tertiary: {
        "Avoidance": 40,
        "Leech": 41,
    },
    crafted: {
        "Crit/Haste": 8790,     // Fireflash
        "Crit/Mastery": 8791,   // Peerless
        "Haste/Vers": 8792,     // Feverflare
        "Haste/Mastery": 8793,  // Aurora
        "Vers/Mastery": 8794,   // Harmonious
        "Crit/Vers": 8795,      // Quickblade
    }
};

function computeBonusIds(inst) {
    const existing = Array.isArray(inst.bonusIDs) ? inst.bonusIDs : [];

    const tertIds = new Set(Object.values(BONUS_IDS.tertiary));
    const craftedIds = new Set(Object.values(BONUS_IDS.crafted));

    // keep all non-tertiary / non-crafted bonus IDs from logs
    let out = existing.filter(id => !tertIds.has(id) && !craftedIds.has(id));

    // one tertiary max
    if (inst.tertiary && BONUS_IDS.tertiary[inst.tertiary]) {
        out.push(BONUS_IDS.tertiary[inst.tertiary]);
    }

    // one crafted combo max
    if (inst.craftedStats && BONUS_IDS.crafted[inst.craftedStats]) {
        out.push(BONUS_IDS.crafted[inst.craftedStats]);
    }

    inst.bonusIDs = Array.from(new Set(out)).sort((a, b) => a - b);
    return inst.bonusIDs;
}

// ────────────────────────────────────────────────────────────────
// Public API
// ────────────────────────────────────────────────────────────────
export function mountTopGearUI(container, itemsBySlot /* may be empty */) {
    container.innerHTML = "";

    const sections = toSections(itemsBySlot || new Map());
    const selection = new Map();

    const wrap = el("div", { class: "tg-wrap" });
    const count = el("div", { class: "count", id: "tg-count" }, "Combinations: 0");
    wrap.append(
        el("div", { class: "tg-head" }, el("h2", {}, "Top Gear"), count),
        el("div", { class: "tg-grid", id: "tg-grid" })
    );
    const grid = wrap.querySelector("#tg-grid");
    container.append(wrap);

    for (const sec of sections) {
        selection.set(sec.key, new Set());
        sec.el = renderSection(sec, selection, sections, () => updateCount(count, selection, sections));
        grid.append(sec.el);
    }
    updateCount(count, selection, sections);

    return {
        getSelection: () => selection,
        buildDirective: () => buildDirectiveFromSelection(selection, sections),

        addItem: (raw) => {
            const norm = normalizeItem(raw, "user");
            const key = sectionKeyForSlot(norm.slot);

            let sec = sections.find(s => s.key === key);
            if (!sec) {
                sec = { key, title: keyTitle(key, norm.slot), items: [] };
                sections.push(sec);
                sec.el = renderSection(sec, selection, sections, () => updateCount(count, selection, sections));
                grid.append(sec.el);
            }

            const inst = makeInstance(norm);
            sec.items.push(inst);

            if (!selection.has(sec.key)) selection.set(sec.key, new Set());
            selection.get(sec.key).add(inst.instanceId);

            rerenderSection(sec, selection, count, sections);
            updateCount(count, selection, sections);
        },

        replaceBaseline: (incomingBySlot) => {
            for (const sec of sections) {
                const before = sec.items.length;
                if (before) {
                    sec.items = sec.items.filter(it => it.source !== "baseline");
                    if (before !== sec.items.length) {
                        const keepIds = new Set(sec.items.map(i => i.instanceId));
                        const sel = selection.get(sec.key) || new Set();
                        for (const id of [...sel]) if (!keepIds.has(id)) sel.delete(id);
                        selection.set(sec.key, sel);
                    }
                }
            }

            const byKey = groupIncomingByKey(incomingBySlot);
            for (const [key, list] of byKey) {
                let sec = sections.find(s => s.key === key);
                if (!sec) {
                    sec = { key, title: keyTitle(key, typeof key === "number" ? key : undefined), items: [] };
                    sections.push(sec);
                    sec.el = renderSection(sec, selection, sections, () => updateCount(count, selection, sections));
                    grid.append(sec.el);
                }
                for (const raw of list) {
                    const inst = makeInstance(normalizeItem(raw, "baseline"));
                    sec.items.push(inst);
                }
                const sel = selection.get(sec.key) || new Set();
                if (key === "FINGERS" || key === "TRINKETS") {
                    for (const it of sec.items) if (it.source === "baseline") sel.add(it.instanceId);
                } else {
                    if (sel.size === 0) {
                        const first = sec.items.find(i => i.source === "baseline") || sec.items[0];
                        if (first) sel.add(first.instanceId);
                    }
                }
                selection.set(sec.key, sel);

                sec.items.sort(byItemLevelDescInst);
                rerenderSection(sec, selection, count, sections);
            }

            updateCount(count, selection, sections);
        }
    };
}

// ────────────────────────────────────────────────────────────────
// Instances & normalize
// ────────────────────────────────────────────────────────────────
function normalizeItem(i, source) {
    const inst = {
        itemId: Number(i.id),
        slot: Number(i.slot),
        name: i.name || `Item #${i.id}`,
        itemLevel: i.itemLevel ?? i.ilvl ?? null,
        icon: i.icon || null,
        badges: i.badges || [],
        // kept attributes:
        tertiary: i.tertiary ?? null,
        craftedStats: i.craftedStats ?? null,
        // derived:
        bonusIDs: Array.isArray(i.bonusIDs) ? i.bonusIDs.slice() : [],
        source
    };
    computeBonusIds(inst);
    return inst;
}

function makeInstance(n) {
    return { instanceId: newUid(), ...n };
}
const byItemLevelDescInst = (a, b) => (b.itemLevel ?? 0) - (a.itemLevel ?? 0);

// ────────────────────────────────────────────────────────────────
function sectionKeyForSlot(slot) {
    if (FINGER_SLOTS.has(slot)) return "FINGERS";
    if (TRINKET_SLOTS.has(slot)) return "TRINKETS";
    return slot;
}
function keyTitle(key, slot) {
    if (key === "FINGERS") return "FINGERS";
    if (key === "TRINKETS") return "TRINKETS";
    return displaySlot(slot);
}
function groupIncomingByKey(itemsBySlot) {
    const map = new Map();
    map.set("FINGERS", []);
    map.set("TRINKETS", []);

    for (const [slot, list] of itemsBySlot) {
        const s = Number(slot);

        // <<< add this so SHIRT (3) and TABARD (17) never show up >>>
        if (IGNORED_SLOTS.has(s)) continue;

        const key = sectionKeyForSlot(s);
        const arr = map.get(key) || [];
        for (const it of list) arr.push(it);
        map.set(key, arr);
    }
    return map;
}

// ────────────────────────────────────────────────────────────────
// Render
// ────────────────────────────────────────────────────────────────
function renderSection(section, selection, sections, notify) {
    const box = el("div", { class: "tg-slot" });
    box.append(el("h3", {}, section.title));

    const list = el("div", { class: "tg-list" });
    section.listEl = list;

    for (const inst of section.items) {
        const card = renderItemCard(
            inst,
            selection.get(section.key)?.has(inst.instanceId),
            section,
            selection,
            sections,
            notify
        );
        list.append(card);
    }

    box.append(list);
    section.el = box;
    return box;
}

function rerenderSection(section, selection, countNode, sections) {
    const list = section.listEl;
    list.innerHTML = "";
    for (const inst of section.items) {
        const card = renderItemCard(
            inst,
            selection.get(section.key)?.has(inst.instanceId),
            section,
            selection,
            sections,
            () => updateCount(countNode, selection, sections)
        );
        list.append(card);
    }
}

function renderItemCard(inst, selected, section, selection, sections, notify) {
    const whData = buildWowheadData(inst);

    const ico = el("div", { class: "tg-ico" });
    if (inst.icon) {
        const iconLink = el("a", {
            href: `https://www.wowhead.com/item=${inst.itemId}`,
            target: "_blank",
            rel: "noopener",
            "data-wowhead": whData
        });
        iconLink.append(
            el("img", {
                src: inst.icon,
                alt: "",
                style: "width:100%;height:100%;object-fit:cover;border-radius:6px"
            })
        );
        ico.append(iconLink);
    }

    const nameLink = el("a", {
        href: `https://www.wowhead.com/item=${inst.itemId}`,
        target: "_blank",
        rel: "noopener",
        "data-wowhead": whData
    }, inst.name || `Item #${inst.itemId}`);

    const title = el("div", {},
        el("div", { class: "tg-name" }, nameLink),
        el("div", { class: "tg-sub tg-badges" },
            ...buildBadgesForInst(inst).map(t => el("span", { class: "tg-badge" }, t))
        )
    );

    const ilvl = el("div", { class: "tg-ilvl" }, inst.itemLevel != null ? `${inst.itemLevel}` : "");

    // Actions: Edit + Copy + (Remove for non-baseline)
    const actions = el("div", { class: "tg-actions" });
    const editBtn = el("button", { class: "tg-edit", title: "Edit item" }, "Edit");
    const copyBtn = el("button", { class: "tg-copy", title: "Copy item" }, "Copy");
    actions.append(editBtn, copyBtn);

    let removeBtn = null;
    if (inst.source !== "baseline") {
        removeBtn = el("button", { class: "tg-remove-btn", title: "Remove item" }, "Remove");
        actions.append(removeBtn);
    }

    const card = el("div", { class: "tg-item", "data-id": inst.instanceId });
    card.append(ico, title, ilvl, actions);
    if (selected) card.classList.add("is-selected");

    // selection toggle
    card.addEventListener("click", (e) => {
        if (e.target.closest(".tg-edit") ||
            e.target.closest(".tg-copy") ||
            e.target.closest(".tg-remove-btn") ||
            e.target.closest("a")) return;

        const set = selection.get(section.key) || new Set();
        if (set.has(inst.instanceId)) set.delete(inst.instanceId);
        else set.add(inst.instanceId);
        selection.set(section.key, set);
        reflectListSelection(section.listEl, set);
        notify?.();
    });

    // remove (non-baseline only)
    if (removeBtn) {
        removeBtn.addEventListener("click", (e) => {
            e.stopPropagation();
            section.items = section.items.filter(i => i.instanceId !== inst.instanceId);
            const set = selection.get(section.key) || new Set();
            set.delete(inst.instanceId);
            selection.set(section.key, set);
            rerenderSection(section, selection, document.querySelector("#tg-count"), sections);
            notify?.();
        });
    }

    // edit panel
    editBtn.addEventListener("click", (e) => {
        e.stopPropagation();
        toggleEditor(card, inst, section, selection, sections, notify);
    });

    // copy
    copyBtn.addEventListener("click", (e) => {
        e.stopPropagation();
        const clone = makeInstance({
            itemId: inst.itemId,
            slot: inst.slot,
            name: inst.name,
            itemLevel: inst.itemLevel ?? null,
            icon: inst.icon,
            badges: Array.from(inst.badges || []),
            tertiary: inst.tertiary ?? null,
            craftedStats: inst.craftedStats ?? null,
            bonusIDs: inst.bonusIDs ? [...inst.bonusIDs] : [],
            source: inst.source
        });

        const idx = section.items.findIndex(i => i.instanceId === inst.instanceId);
        if (idx >= 0) section.items.splice(idx + 1, 0, clone);
        else section.items.push(clone);

        const set = selection.get(section.key) || new Set();
        set.add(clone.instanceId);
        selection.set(section.key, set);

        rerenderSection(section, selection, document.querySelector("#tg-count"), sections);
        notify?.();
    });

    refreshWowheadLinks();
    return card;
}

function toggleEditor(card, inst, section, selection, sections, notify) {
    const existing = card.querySelector(".tg-edit-panel");
    if (existing) {
        existing.remove();
        return;
    }
    const panel = renderEditPanel(inst, () => {
        // recompute bonuses + badges after edits
        computeBonusIds(inst);
        inst.badges = buildBadgesForInst(inst);
        rerenderSection(section, selection, document.querySelector("#tg-count"), sections);
        notify?.();
    }, () => {
        panel.remove();
    });
    card.append(panel);
}

function renderEditPanel(inst, onSave, onCancel) {
    const panel = el("div", { class: "tg-edit-panel" });

    // Item level
    const ilvlInput = el("input", {
        type: "number", min: "1", max: "2000", step: "1",
        value: inst.itemLevel ?? "",
        placeholder: "ilvl"
    });

    // Tertiary
    const tertiarySelect = makeSelect(
        [["", "None"], ["Leech", "Leech"], ["Avoidance", "Avoidance"]],
        inst.tertiary ?? ""
    );

    // Crafted Stats
    const craftedSelect = makeSelect(
        [
            ["", "None"],
            ["Crit/Haste", "Crit/Haste"],
            ["Crit/Mastery", "Crit/Mastery"],
            ["Crit/Vers", "Crit/Vers"],
            ["Haste/Mastery", "Haste/Mastery"],
            ["Haste/Vers", "Haste/Vers"],
            ["Mastery/Vers", "Mastery/Vers"]
        ],
        inst.craftedStats ?? ""
    );

    panel.append(
        row("Item level", ilvlInput),
        row("Tertiary", tertiarySelect),
        row("Crafted Stats", craftedSelect),
        el("div", { class: "tg-edit-actions" },
            el("button", { class: "tg-edit-save" }, "Save"),
            el("button", { class: "tg-edit-cancel" }, "Cancel")
        )
    );

    const saveBtn = panel.querySelector(".tg-edit-save");
    const cancelBtn = panel.querySelector(".tg-edit-cancel");

    saveBtn.addEventListener("click", (e) => {
        e.stopPropagation();

        const newIlvl = parseInt(ilvlInput.value, 10);
        inst.itemLevel = Number.isFinite(newIlvl) ? newIlvl : null;

        inst.tertiary = tertiarySelect.value || null;
        inst.craftedStats = craftedSelect.value || null;

        onSave?.();
    });

    cancelBtn.addEventListener("click", (e) => {
        e.stopPropagation();
        onCancel?.();
    });

    return panel;
}

function row(label, control) {
    return el("label", { class: "tg-edit-row" }, el("span", {}, label), control);
}

// ─── Utilities ──────────────────────────────────────────────────
function makeSelect(options, currentValue) {
    const sel = el("select", {});
    for (const [value, label] of options) {
        const opt = el("option", { value }, label);
        if (String(currentValue) === String(value)) opt.selected = true;
        sel.append(opt);
    }
    return sel;
}

function buildWowheadData(inst) {
    const parts = [`item=${inst.itemId}`];
    if (inst.itemLevel) parts.push(`ilvl=${inst.itemLevel}`);
    if (inst.bonusIDs?.length) parts.push(`bonus=${inst.bonusIDs.join(":")}`);
    return parts.join("&");
}

function buildBadgesForInst(inst) {
    const out = [];
    if (inst.craftedStats) out.push(inst.craftedStats);
    if (inst.tertiary) out.push(inst.tertiary);
    return out;
}

function refreshWowheadLinks() {
    if (window.$WowheadPower?.refreshLinks) {
        window.$WowheadPower.refreshLinks();
    } else if (window.WH?.Tooltips?.refreshLinks) {
        window.WH.Tooltips.refreshLinks();
    }
}

function reflectListSelection(list, set) {
    list.querySelectorAll(".tg-item").forEach(n => {
        const id = Number(n.getAttribute("data-id"));
        if (set?.has(id)) n.classList.add("is-selected");
        else n.classList.remove("is-selected");
    });
}

// ────────────────────────────────────────────────────────────────
// Sections and counts
// ────────────────────────────────────────────────────────────────
function toSections(itemsBySlot) {
    const sections = [];
    sections.push({ key: "FINGERS", title: "FINGERS", items: [] });
    sections.push({ key: "TRINKETS", title: "TRINKETS", items: [] });

    for (const [slot, list] of itemsBySlot) {
        if (IGNORED_SLOTS.has(slot)) continue;
        const key = sectionKeyForSlot(slot);
        const sec =
            sections.find(s => s.key === key) ||
            (sections.push({ key, title: keyTitle(key, slot), items: [] }), sections[sections.length - 1]);
        for (const raw of list) sec.items.push(makeInstance(normalizeItem(raw, "baseline")));
    }
    return sections;
}

function updateCount(node, selection, sections) {
    const { count } = computeComboPlan(selection, sections, { countOnly: true });
    node.textContent = `Combinations: ${count}`;
}

// include full enriched item (id + itemLevel + bonusIDs)
function enrichItem(itemId, sections) {
    for (const sec of sections) {
        const inst = sec.items.find(i => i.itemId === itemId);
        if (inst) {
            return {
                id: inst.itemId,
                itemLevel: inst.itemLevel ?? null,
                bonusIDs: inst.bonusIDs ? [...inst.bonusIDs] : []
            };
        }
    }
    return {
        id: itemId,
        itemLevel: null,
        bonusIDs: []
    };
}

function buildDirectiveFromSelection(selection, sections) {
    const instIndex = indexInstances(sections); // instanceId -> inst
    const { normalChoices, fingerPairs, trinketPairs, count } =
        computeComboPlan(selection, sections, { countOnly: false });

    // If any required choice list is empty, return no gearSets (caller can block run)
    if (count === 0) {
        return { gearSets: [], limits: { embellishments: 2 } };
    }

    // Build arrays to product: one entry per "dimension"
    // For normals: each dimension is [ { slot, item }, ...choices ]
    const dims = [];
    for (const [slot, choices] of normalChoices.entries()) {
        dims.push(choices.map(instanceId => ({ type: "slot", slot, instanceId })));
    }

    dims.push(
        fingerPairs.map(pair => ({ type: "pair", slots: [10, 11], instances: pair }))
    );
    dims.push(
        trinketPairs.map(pair => ({ type: "pair", slots: [12, 13], instances: pair }))
    );

    const gearSets = [];
    let id = 1;

    for (const pick of product(dims)) {
        const slots = {}; // string slot -> [enrichedItem]
        for (const p of pick) {
            if (p.type === "slot") {
                const inst = instIndex.get(p.instanceId);
                if (!inst) continue;
                const key = String(p.slot);
                (slots[key] ||= []).push({
                    id: inst.itemId,
                    itemLevel: inst.itemLevel ?? null,
                    bonusIDs: inst.bonusIDs ? [...inst.bonusIDs] : []
                });
            } else if (p.type === "pair") {
                const [s1, s2] = p.slots;
                const [i1, i2] = p.instances;
                const inst1 = instIndex.get(i1);
                const inst2 = instIndex.get(i2);
                if (inst1) {
                    (slots[String(s1)] ||= []).push({
                        id: inst1.itemId,
                        itemLevel: inst1.itemLevel ?? null,
                        bonusIDs: inst1.bonusIDs ? [...inst1.bonusIDs] : []
                    });
                }
                if (inst2) {
                    (slots[String(s2)] ||= []).push({
                        id: inst2.itemId,
                        itemLevel: inst2.itemLevel ?? null,
                        bonusIDs: inst2.bonusIDs ? [...inst2.bonusIDs] : []
                    });
                }
            }
        }

        gearSets.push({
            id,
            name: `Set #${id}`,
            slots,
            enchants: {},
            gems: {}
        });
        id++;
    }

    return {
        gearSets,
        limits: { embellishments: 2 }
    };
}

function displaySlot(slot) {
    const m = {
        0: "HEAD", 1: "NECK", 2: "SHOULDER", 3: "SHIRT", 4: "CHEST", 5: "WAIST", 6: "LEGS",
        7: "FEET", 8: "WRIST", 9: "HANDS", 10: "FINGER 1", 11: "FINGER 2", 12: "TRINKET 1",
        13: "TRINKET 2", 14: "BACK", 15: "MAIN HAND", 16: "OFF HAND"
    };
    return m[slot] ?? `SLOT ${slot}`;
}

const NORMAL_SLOTS = new Set([0, 1, 2, 4, 5, 6, 7, 8, 9, 14, 15, 16]); // everything except 10/11/12/13 and ignored

function nC2(n) { return n < 2 ? 0 : (n * (n - 1)) / 2; }

// Build a quick index: instanceId -> instance object
function indexInstances(sections) {
    const idx = new Map();
    for (const sec of sections) for (const inst of sec.items) idx.set(inst.instanceId, inst);
    return idx;
}

// From the current selection, compute the choice lists per slot and ring/trinket pairs (for counting or building)
function computeComboPlan(selection, sections, { countOnly = false } = {}) {
    const instIdx = indexInstances(sections);

    // Gather selected instanceIds by section key
    const selectedByKey = new Map();
    for (const [key, set] of selection.entries()) {
        selectedByKey.set(key, [...set].map(id => instIdx.get(id)).filter(Boolean));
    }

    // Normal slots → array of choices (each choice is an itemId)
    const normalChoices = new Map();
    for (const slot of NORMAL_SLOTS) {
        const items = (selectedByKey.get(slot) || []);
        normalChoices.set(slot, items.map(i => i.instanceId));
    }

    // FINGERS/TRINKETS → unordered pairs (choose 2) as arrays of [instanceIdA, instanceIdB]
    const fingerItems = (selectedByKey.get("FINGERS") || []).map(i => i.instanceId);
    const trinketItems = (selectedByKey.get("TRINKETS") || []).map(i => i.instanceId);

    const fingerPairs = choosePairs(fingerItems);
    const trinketPairs = choosePairs(trinketItems);

    // Count
    let count = 1;
    for (const arr of normalChoices.values()) {
        if (arr.length === 0) { count = 0; break; }
        count *= arr.length;
    }
    if (count > 0) {
        const fc = nC2(fingerItems.length);
        const tc = nC2(trinketItems.length);
        count *= (fc === 0 ? 0 : fc);
        count *= (tc === 0 ? 0 : tc);
    }

    if (countOnly) return { count };

    return {
        normalChoices,  // Map<slot, number[]>
        fingerPairs,    // number[][]
        trinketPairs,   // number[][]
        count
    };
}

function choosePairs(items) {
    const out = [];
    for (let i = 0; i < items.length; i++) {
        for (let j = i + 1; j < items.length; j++) {
            out.push([items[i], items[j]]);
        }
    }
    return out;
}

// Cartesian product utility: takes array of arrays, yields each combination
function* product(arrays) {
    const n = arrays.length;
    if (n === 0) { yield []; return; }
    const idx = new Array(n).fill(0);
    while (true) {
        yield idx.map((i, k) => arrays[k][i]);
        let k = n - 1;
        while (k >= 0) {
            idx[k]++;
            if (idx[k] < arrays[k].length) break;
            idx[k] = 0; k--;
        }
        if (k < 0) break;
    }
}
