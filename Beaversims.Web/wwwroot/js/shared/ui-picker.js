import { set } from "./state.js";
import { fetchFights, fetchHealers, getReportCodeFromUrl } from "./wcl.js";
import { msToMinSec, el } from "./utils.js";

export async function renderPicker(host) {
    const res = await fetch("/import-form.html", { cache: "no-cache" });
    if (!res.ok) throw new Error("Failed to load import-form.html");
    host.innerHTML = await res.text();

    const input = host.querySelector("#import-url");
    const fightsMount = host.querySelector("#fights-list");
    const healersMount = host.querySelector("#healers-list");

    input?.addEventListener("input", async () => {
        const code = getReportCodeFromUrl(input.value.trim());
        if (!code) return;
        set({ reportCode: code, fight: null, healer: null });

        try {
            const fights = await fetchFights(code);
            renderFights(fights, fightsMount, healersMount, code);
        } catch (err) {
            console.error("Failed to load fights:", err);
        }
    });
}

function renderFights(fights, mount, healersMount, reportCode, token) {
    mount.innerHTML = "";
    healersMount.innerHTML = "";

    const valid = fights.filter(f => (f.encounterID ?? 0) !== 0);
    const groups = new Map();

    for (const f of valid) {
        const key = f.encounterID ?? f.name;
        if (!groups.has(key)) groups.set(key, { name: f.name, pulls: [] });
        groups.get(key).pulls.push(f);
    }

    for (const group of groups.values()) {
        const wrap = el("div", { class: "fights-group" });
        const hdr = el("div", { class: "hdr" },
            el("span", { class: "name" }, group.name),
            el("span", { class: "meta" }, `${group.pulls.length} pulls`)
        );
        wrap.append(hdr);

        const pulls = el("div", { class: "pulls" });
        group.pulls.sort((a, b) => a.startTime - b.startTime);

        for (const p of group.pulls) {
            const dur = msToMinSec((p.endTime ?? 0) - (p.startTime ?? 0));
            const pct = Math.round(p.fightPercentage ?? 0);
            const label = p.kill ? "KILL" : `${pct}%`;

            const btn = el("button", {
                type: "button",
                class: `pull ${p.kill ? "kill" : "wipe"}`,
                onclick: async () => {
                    pulls.querySelectorAll(".pull").forEach(x => x.classList.remove("selected"));
                    btn.classList.add("selected");
                    set({ fight: p, healer: null });
                    healersMount.innerHTML = "Loading healers...";
                    try {
                        const healers = await fetchHealers(reportCode, p.id, token);
                        renderHealers(healers, healersMount, group.name, p);
                    } catch (err) {
                        healersMount.innerHTML = "Failed to load healers.";
                        console.error(err);
                    }
                }
            },
                `${label} (${dur})`);
            pulls.append(btn);
        }

        wrap.append(pulls);
        mount.append(wrap);
    }
}

function renderHealers(healers, mount, fightName, fight) {
    mount.innerHTML = "";
    const wrap = el("div", { class: "healers-wrap" });
    const hdr = el("div", { class: "hdr" },
        el("span", { class: "name" }, `Healers — ${fightName} #${fight.id}`),
        el("span", { class: "meta" }, healers.length)
    );
    wrap.append(hdr);

    const row = el("div", { class: "healers" });
    for (const h of healers) {
        const b = el("button", {
            type: "button",
            class: "healer",
            onclick: () => {
                row.querySelectorAll(".healer").forEach(x => x.classList.remove("selected"));
                b.classList.add("selected");
                set({ healer: h });
            }
        },
            `${h.name} — ${h.spec || h.class}`);
        row.append(b);
    }

    wrap.append(row);
    mount.append(wrap);
}
