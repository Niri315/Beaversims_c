import { initResultOptions } from "/js/shared/result-options.js";
import { icon } from "/js/shared/utils.js";

// --- helpers
export function moduleTitleFromQuery(m) {
    switch ((m || "").toLowerCase()) {
        case "sw": return "Stat Weights";
        case "tg": return "Top Gear";
        case "tr": return "Trinkets Overview";
        default: return "Simulation Results";
    }
}

function decodeData() {
    const s = new URLSearchParams(location.search).get("data");
    if (!s) return null;
    try { return JSON.parse(atob(s)); } catch { return null; }
}

export function difficultyLabel(v) {
    const map = { 1: "LFR", 2: "Normal", 3: "Heroic", 4: "Mythic", 5: "M+", 10: "Dungeon" };
    return map[v] ?? (v ?? "—");
}
export function fmtTime(seconds) {
    const s = Math.max(0, Math.round(seconds || 0));
    const m = Math.floor(s / 60);
    return `${m}:${String(s % 60).padStart(2, "0")}`;
}
export function fmtNumber(n) {
    n = Number(n || 0);
    if (n >= 1_000_000) return (n / 1_000_000).toFixed(2) + " mil";
    if (n >= 1_000) return (n / 1_000).toFixed(1) + " k";
    return Math.round(n).toString();
}

// --- shared boot
export async function bootBase() {
    const result = decodeData();
    if (!result) {
        document.body.innerHTML = "<h2>No result data provided.</h2>";
        throw new Error("No result data");
    }

    // render fight header (icons + numbers)
    renderHeader(result);

    // init shared result options (default to total + DR + Support)
    const ro = initResultOptions(document.getElementById("result-options"), {
        defaults: { metric: "total", includeDR: true, includeSupport: true },
        onChange: () => { /* module will subscribe */ },
        emitInitial: false
    });

    return {
        result,
        ro,
        mounts: { mount: document.getElementById("results-mount") }
    };
}

// --- shared header (same as you built)
function renderHeader(res) {
    const wrap = document.getElementById("fight-summary");
    wrap.innerHTML = `
    <div class="fs-card fs-player">
      <div class="fs-left">
        <div class="fs-icon-row">
          <img class="fs-icon" id="fs-spec-icon" alt="">
          <img class="fs-icon" id="fs-hero-icon" alt="">
        </div>
      </div>
      <div class="fs-main">
        <div class="fs-title" id="fs-player-name"></div>
        <div class="fs-metrics">
          <div><span class="label">HPS</span><span class="val" id="fs-hps">0</span></div>
          <div><span class="label">DPS</span><span class="val" id="fs-dps">0</span></div>
          <div><span class="label">DTPS</span><span class="val" id="fs-dtps">0</span></div>
        </div>
      </div>
    </div>
    <div class="fs-card fs-fight">
      <div class="fs-left">
        <img class="fs-icon fs-icon-lg" id="fs-boss-icon" alt="">
      </div>
      <div class="fs-main">
        <div class="fs-title">
          <span id="fs-boss-name"></span>
          <span id="fs-diff" class="fs-chip">—</span>
        </div>
        <div class="fs-sub">
          <span id="fs-time">0:00</span> ·
          <span id="fs-outcome">—</span>
        </div>
      </div>
    </div>`;

    // icons + text
    document.getElementById("fs-spec-icon").src = icon(res.specName);
    document.getElementById("fs-hero-icon").src = icon(res.heroTlName);
    document.getElementById("fs-boss-icon").src = icon(res.fightName);

    document.getElementById("fs-player-name").textContent = res.playerName || "—";
    document.getElementById("fs-boss-name").textContent = res.fightName || "—";

    const ot = res.originalTotals || {};
    const hps = ot.Eff ?? ot.Heal ?? 0;
    const dps = ot.Dmg ?? ot.Damage ?? 0;
    const dtps = Math.abs(ot.Def ?? ot.DamageTaken ?? 0);

    document.getElementById("fs-hps").textContent = fmtNumber(hps);
    document.getElementById("fs-dps").textContent = fmtNumber(dps);
    document.getElementById("fs-dtps").textContent = fmtNumber(dtps);

    document.getElementById("fs-diff").textContent = difficultyLabel(res.difficulty);
    document.getElementById("fs-time").textContent = fmtTime(res.totalTime);
    document.getElementById("fs-outcome").textContent = (res.success === true) ? "Kill" : "Wipe";
}
