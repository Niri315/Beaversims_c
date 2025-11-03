import { el } from "/js/shared/utils.js";

const IGNORED_SLOTS = new Set([3, 17]);
const FINGER_SLOTS = new Set([10, 11]);
const TRINKET_SLOTS = new Set([12, 13]);

let __uid = 0;
const newUid = () => (++__uid);

// ────────────────────────────────────────────────────────────────
// Public API
// ────────────────────────────────────────────────────────────────
export function mountTopGearUI(container, itemsBySlot /* may be empty */) {
    container.innerHTML = "";

    const sections = toSections(itemsBySlot || new Map()); // [{key,title,items:[]}]
    const selection = new Map(); // Map<sectionKey, Set<instanceId>>

    const wrap = el("div", { class: "tg-wrap" });
    const count = el("div", { class: "count", id: "tg-count" }, "Combinations: 0");
    wrap.append(
        el("div", { class: "tg-head" }, el("h2", {}, "Top Gear"), count),
        el("div", { class: "tg-grid", id: "tg-grid" })
    );
    const grid = wrap.querySelector("#tg-grid");
    container.append(wrap);

    // render existing sections empty initially
    for (const sec of sections) {
        selection.set(sec.key, new Set());
        sec.el = renderSection(sec, selection, () => updateCount(count, selection));
        grid.append(sec.el);
    }
    updateCount(count, selection);

    return {
        getSelection: () => selection,
        buildDirective: () => buildDirectiveFromSelection(selection, sections),

        // Add a single item from search (duplicates allowed)
        addItem: (raw) => {
            const norm = normalizeItem(raw, "user");
            const key = sectionKeyForSlot(norm.slot);

            let sec = sections.find(s => s.key === key);
            if (!sec) {
                sec = { key, title: keyTitle(key, norm.slot), items: [] };
                sections.push(sec);
                sec.el = renderSection(sec, selection, () => updateCount(count, selection));
                grid.append(sec.el);
            }

            // push duplicate instance
            const inst = makeInstance(norm);
            sec.items.push(inst);

            if (!selection.has(sec.key)) selection.set(sec.key, new Set());
            selection.get(sec.key).add(inst.instanceId);

            rerenderSection(sec, selection);
            updateCount(count, selection);
        },

        // Replace baseline items when healer changes
        replaceBaseline: (incomingBySlot) => {
            // 1) Remove all existing baseline instances from all sections
            for (const sec of sections) {
                const before = sec.items.length;
                if (before) {
                    // remove baseline instances
                    sec.items = sec.items.filter(it => it.source !== "baseline");
                    // clean selection of removed instances
                    if (before !== sec.items.length) {
                        const keepIds = new Set(sec.items.map(i => i.instanceId));
                        const sel = selection.get(sec.key) || new Set();
                        for (const id of [...sel]) if (!keepIds.has(id)) sel.delete(id);
                        selection.set(sec.key, sel);
                    }
                }
            }

            // 2) Add new baseline items
            const byKey = groupIncomingByKey(incomingBySlot);
            for (const [key, list] of byKey) {
                let sec = sections.find(s => s.key === key);
                if (!sec) {
                    sec = { key, title: keyTitle(key, typeof key === "number" ? key : undefined), items: [] };
                    sections.push(sec);
                    sec.el = renderSection(sec, selection, () => updateCount(count, selection));
                    grid.append(sec.el);
                }
                for (const raw of list) {
                    const inst = makeInstance(normalizeItem(raw, "baseline")); // baseline source
                    sec.items.push(inst);
                }
                // 3) Auto-select baseline:
                const sel = selection.get(sec.key) || new Set();
                if (key === "FINGERS" || key === "TRINKETS") {
                    // select ALL baseline instances in this section
                    for (const it of sec.items) if (it.source === "baseline") sel.add(it.instanceId);
                } else {
                    // select first if section currently empty selection
                    if (sel.size === 0) {
                        const first = sec.items.find(i => i.source === "baseline") || sec.items[0];
                        if (first) sel.add(first.instanceId);
                    }
                }
                selection.set(sec.key, sel);

                // sort display by ilvl desc
                sec.items.sort(byIlvlDescInst);
                rerenderSection(sec, selection);
            }

            updateCount(count, selection);
        }
    };
}

// ────────────────────────────────────────────────────────────────
// Instances & normalize
// ────────────────────────────────────────────────────────────────
function normalizeItem(i, source) {
    return {
        itemId: Number(i.id),
        slot: Number(i.slot),
        name: i.name || `Item #${i.id}`,
        ilvl: i.itemLevel ?? i.ilvl ?? null,
        icon: i.icon || null,
        badges: i.badges || [],
        source // "user" | "baseline"
    };
}
function makeInstance(n) {
    return {
        instanceId: newUid(), // unique per card → allows duplicates
        ...n
    };
}
const byIlvlDescInst = (a, b) => (b.ilvl ?? 0) - (a.ilvl ?? 0);

// ────────────────────────────────────────────────────────────────
function sectionKeyForSlot(slot) {
    if (FINGER_SLOTS.has(slot)) return "FINGERS";
    if (TRINKET_SLOTS.has(slot)) return "TRINKETS";
    return slot; // numeric
}
function keyTitle(key, slot) {
    if (key === "FINGERS") return "FINGERS";
    if (key === "TRINKETS") return "TRINKETS";
    return displaySlot(slot);
}
function groupIncomingByKey(itemsBySlot) {
    const map = new Map();
    // ensure grouped keys exist
    map.set("FINGERS", []);
    map.set("TRINKETS", []);
    for (const [slot, list] of itemsBySlot) {
        const key = sectionKeyForSlot(Number(slot));
        const arr = map.get(key) || [];
        for (const it of list) arr.push(it);
        map.set(key, arr);
    }
    return map;
}

// ────────────────────────────────────────────────────────────────
// Render
// ────────────────────────────────────────────────────────────────
function renderSection(section, selection, notify) {
    const box = el("div", { class: "tg-slot" });
    box.append(el("h3", {}, section.title));

    const list = el("div", { class: "tg-list" });
    section.listEl = list;

    for (const inst of section.items) {
        const card = renderItemCard(inst, selection.get(section.key)?.has(inst.instanceId), section, selection, notify);
        list.append(card);
    }

    box.append(list);
    section.el = box;
    return box;
}

function rerenderSection(section, selection) {
    const list = section.listEl;
    list.innerHTML = "";
    for (const inst of section.items) {
        const card = renderItemCard(inst, selection.get(section.key)?.has(inst.instanceId), section, selection, () => {
            const countNode = document.querySelector("#tg-count");
            updateCount(countNode, selection);
        });
        list.append(card);
    }
}

function renderItemCard(inst, selected, section, selection, notify) {
    // Build the tooltip payload once
    const whData = buildWowheadData(inst); // e.g. "item=12345&ilvl=717"

    const ico = el("div", { class: "tg-ico" });
    if (inst.icon) {
        // wrap the icon with a wowhead link so hovering the icon also shows tooltip
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

    // Clickable name with tooltip
    const nameLink = el("a", {
        href: `https://www.wowhead.com/item=${inst.itemId}`,
        target: "_blank",
        rel: "noopener",
        "data-wowhead": whData
    }, inst.name || `Item #${inst.itemId}`);

    const title = el("div", {},
        el("div", { class: "tg-name" }, nameLink),
        el("div", { class: "tg-sub tg-badges" },
            ...(inst.badges || []).map(t => el("span", { class: "tg-badge" }, t))
        )
    );

    const ilvl = el("div", { class: "tg-ilvl" }, inst.ilvl != null ? `${inst.ilvl}` : "");

    const card = el("div", { class: "tg-item", "data-id": inst.instanceId });
    card.append(ico, title, ilvl);
    if (selected) card.classList.add("is-selected");

    // selection toggle
    card.addEventListener("click", () => {
        const set = selection.get(section.key) || new Set();
        if (set.has(inst.instanceId)) set.delete(inst.instanceId);
        else set.add(inst.instanceId);
        selection.set(section.key, set);
        reflectListSelection(section.listEl, set);
        notify?.();
    });

    // remove (non-baseline only)
    if (inst.source !== "baseline") {
        const removeBtn = el("button", { class: "tg-remove", title: "Remove" }, "×");
        removeBtn.addEventListener("click", (e) => {
            e.stopPropagation();
            section.items = section.items.filter(i => i.instanceId !== inst.instanceId);
            const set = selection.get(section.key) || new Set();
            set.delete(inst.instanceId);
            selection.set(section.key, set);
            rerenderSection(section, selection);
            notify?.();
        });
        card.append(removeBtn);
    }

    // Ask Wowhead to rescan newly-added links
    if (window.$WowheadPower?.refreshLinks) {
        window.$WowheadPower.refreshLinks();
    } else if (window.WH?.Tooltips?.refreshLinks) {
        window.WH.Tooltips.refreshLinks();
    }

    return card;
}

// Helper: build Wowhead tooltip query from your instance fields
function buildWowheadData(inst) {
    const parts = [`item=${inst.itemId}`];
    if (inst.ilvl) parts.push(`ilvl=${inst.ilvl}`);
    // If you have these on your inst, uncomment as you add support:
    // if (inst.bonusIds?.length) parts.push(`bonus=${inst.bonusIds.join(":")}`);
    // if (inst.gems?.length)     parts.push(`gems=${inst.gems.join(":")}`);
    // if (inst.enchant)          parts.push(`ench=${inst.enchant}`);
    return parts.join("&");
}


// but since we already maintain section.listEl in the section object,
// we can avoid findSectionByListEl entirely by closing over `section` where we render.
// Simpler approach: move the remove handler into renderSection/rerenderSection where `section` is in scope.
// (For brevity above, we did the minimal approach; feel free to wire it with closure.)
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
    const used = new Set();

    // force-create grouped sections even if empty
    sections.push({ key: "FINGERS", title: "FINGERS", items: [] });
    sections.push({ key: "TRINKETS", title: "TRINKETS", items: [] });

    // seed from initial map (rarely used now; baseline comes via replaceBaseline)
    for (const [slot, list] of itemsBySlot) {
        if (IGNORED_SLOTS.has(slot)) continue;
        const key = sectionKeyForSlot(slot);
        const sec = sections.find(s => s.key === key) || (sections.push({ key, title: keyTitle(key, slot), items: [] }), sections[sections.length - 1]);
        for (const raw of list) sec.items.push(makeInstance(normalizeItem(raw, "baseline")));
    }
    return sections;
}

function updateCount(node, selection) {
    let combos = 1;
    for (const set of selection.values()) {
        combos *= Math.max(1, set.size);
    }
    node.textContent = `Combinations: ${combos}`;
}

// Build directive from current selection (allow duplicates)
function buildDirectiveFromSelection(selection, sections) {
    const out = {
        gearSets: [{
            id: 1,
            name: "Top Gear",
            slots: {},                   // normal slots → array of itemIds (duplicates preserved)
            groups: { fingers: [], trinkets: [] },
            enchants: {},
            gems: {}
        }],
        limits: { embellishments: 2 }
    };

    // index instances by instanceId for quick lookup
    const instIndex = new Map();
    for (const sec of sections) for (const inst of sec.items) instIndex.set(inst.instanceId, inst);

    for (const [key, set] of selection.entries()) {
        const ids = [...set].map(id => instIndex.get(id)?.itemId).filter(Boolean);
        if (key === "FINGERS") out.gearSets[0].groups.fingers = ids;
        else if (key === "TRINKETS") out.gearSets[0].groups.trinkets = ids;
        else out.gearSets[0].slots[String(key)] = ids;
    }
    return out;
}

function displaySlot(slot) {
    const m = {
        0: "HEAD", 1: "NECK", 2: "SHOULDER", 3: "SHIRT", 4: "CHEST", 5: "WAIST", 6: "LEGS",
        7: "FEET", 8: "WRIST", 9: "HANDS", 10: "FINGER 1", 11: "FINGER 2", 12: "TRINKET 1",
        13: "TRINKET 2", 14: "BACK", 15: "MAIN HAND", 16: "OFF HAND"
    };
    return m[slot] ?? `SLOT ${slot}`;
}
