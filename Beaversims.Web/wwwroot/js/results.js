import { initResultOptions } from "/js/shared/result-options.js";
import { STAT_COLORS } from "/js/stat-weights.js";
import { icon } from "/js/shared/utils.js";

// ---- decode
function decodeData() {
    const p = new URLSearchParams(location.search);
    const s = p.get("data");
    if (!s) return null;
    try { return JSON.parse(atob(s)); } catch (e) { console.error(e); return null; }
}
const result = decodeData();
if (!result) {
    document.body.innerHTML = "<h2>No result data provided.</h2>";
    throw new Error("No result data");
}
console.log(result);

// Map difficulty id → label (fallback to raw if unknown)
function difficultyLabel(v) {
    const map = { 1: "LFR", 2: "Normal", 3: "Heroic", 4: "Mythic", 5: "M+" };
    return map?.[v] ?? (v ?? "—");
}

function fmtTime(seconds) {
    const s = Math.max(0, Math.round(seconds || 0));
    const m = Math.floor(s / 60);
    const r = s % 60;
    return `${m}:${r.toString().padStart(2, "0")}`;
}

function int(n) {
    return Math.round(Number(n || 0)).toString();
}

function renderFightSummary(res) {
    // Icons (you’ll supply URLs later)
    document.getElementById("fs-spec-icon").src = icon(result.specName);
    document.getElementById("fs-hero-icon").src = icon(result.heroTlName);
    document.getElementById("fs-boss-icon").src = icon(result.fightName);

    // Names
    document.getElementById("fs-player-name").textContent = res.playerName || "—";
    document.getElementById("fs-spec-hero").textContent = [
        res.specName ?? "—",
        res.heroTlName ?? "—"
    ].join(" · ");

    document.getElementById("fs-boss-name").textContent = res.fightName || "—";

    // Totals → per-second (integers)
    const t = Number(res.totalTime || 0);
    const ot = res.originalTotals || {};
    const totalHeal = ot.Eff ?? ot.Heal ?? 0;
    const totalDmg = ot.Dmg ?? ot.Damage ?? 0;
    const totalTaken = ot.Dtps ?? ot.DamageTaken ?? 0;

    const hps = t ? totalHeal / t : 0;
    const dps = t ? totalDmg / t : 0;
    const dtps = t ? totalTaken / t : 0;

    document.getElementById("fs-hps").textContent = int(hps);
    document.getElementById("fs-dps").textContent = int(dps);
    document.getElementById("fs-dtps").textContent = int(dtps);

    // Meta values (no labels, just values)
    document.getElementById("fs-diff").textContent = difficultyLabel(res.difficulty);
    document.getElementById("fs-time").textContent = fmtTime(t);
    document.getElementById("fs-outcome").textContent = (res.success === true) ? "Kill" : "Wipe";
}


// Call it once after you have `result`
renderFightSummary(result);


// ---- result options
let ro;            // will init after chart is created
let chart;         // chart instance

function valueFromGains(g, sel) {
    const { metric, includeDR, includeSupport } = sel;

    const def = includeDR && (metric === "heal" || metric === "total") ? (g.Def || 0) : 0;
    const supEff = includeSupport ? (g.SupEff || 0) : 0;
    const supDmg = includeSupport ? (g.SupDmg || 0) : 0;

    if (metric === "heal") {
        return (g.Eff || 0) + def + supEff;
    }

    if (metric === "damage") {
        return (g.Dmg || 0) + supDmg;
    }

    return (g.Eff || 0) + (g.Dmg || 0) + def + supEff + supDmg;
}

function computeData(res, sel) {
    const rows = res.altGearSets.map(gs => ({
        name: gs.name || `Set ${gs.id}`,
        total: valueFromGains(gs.gains || {}, sel)
    }));
    rows.sort((a, b) => b.total - a.total);
    return rows;
}

// ---- chart bootstrap (empty; populated by updateChartFromOptions)
const ctx = document.getElementById("resultsChart").getContext("2d");
const colors = ["#4caf50", "#2196f3", "#9c27b0", "#ff9800", "#f44336", "#00bcd4"];

chart = new Chart(ctx, {
    type: "bar",
    data: {
        labels: [],
        datasets: [{ label: "Total Gains", data: [], backgroundColor: [] }]
    },
    options: {
        indexAxis: "y",
        scales: {
            x: { ticks: { color: "#eee" }, grid: { color: "rgba(255,255,255,0.1)" } },
            y: { ticks: { color: "#eee" }, grid: { display: false } }
        },
        plugins: {
            legend: { display: false },
            tooltip: { callbacks: { label: (c) => ` ${c.parsed.x.toFixed(2)} total` } },
            title: {
                display: true,
                text: `Total Time: ${result.totalTime.toFixed(1)}s`,
                color: "#ccc"
            }
        }
    }
});

// ---- init Result Options AFTER chart exists
ro = initResultOptions(document.getElementById("result-options"), {
    defaults: { metric: "total", includeDR: true, includeSupport: true },
    onChange: updateChartFromOptions,
    emitInitial: false
});

// initial render
updateChartFromOptions();

function updateChartFromOptions() {
    const sel = ro.get(); // { metric, includeDR, includeSupport }
    const rows = computeData(result, sel);

    chart.data.labels = rows.map(r => r.name);
    chart.data.datasets[0].data = rows.map(r => Number(r.total.toFixed(2)));
    chart.data.datasets[0].backgroundColor = rows.map(r => STAT_COLORS[r.name] || "#888");
    chart.update();
}
