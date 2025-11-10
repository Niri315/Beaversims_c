import { initResultOptions } from "/js/shared/result-options.js";
import { STAT_COLORS } from "/js/stat-weights.js";

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
function labelDifficulty(v) {
    const map = { 1: "LFR", 2: "Normal", 3: "Heroic", 4: "Mythic", 5: "Mythic+" };
    return map[v] || (typeof v === "string" ? v : String(v ?? "—"));
}

// mm:ss
function fmtTimeSec(seconds) {
    const s = Math.max(0, Math.round(seconds || 0));
    const m = Math.floor(s / 60);
    const r = s % 60;
    return `${m}:${r.toString().padStart(2, "0")}`;
}

// nice number
function fmt(n, d = 1) {
    return (Number.isFinite(n) ? n : 0).toLocaleString(undefined, {
        minimumFractionDigits: d, maximumFractionDigits: d
    });
}

// Populate the Fight Info header
function renderFightInfo(res) {
    const t = Number(res.totalTime || 0); // seconds
    const ot = res.originalTotals || {};

    // You said HPS/DPS/DTPS can be derived from originalTotals + totalTime.
    // Use tolerant keys so this never explodes if names vary a bit.
    const totalHeal = ot.Eff ?? ot.Heal ?? 0;
    const totalDmg = ot.Dmg ?? ot.Damage ?? 0;
    const totalDtps = ot.Def ?? ot.DamageTaken ?? 0; // adjust if your core uses a different key

    const hps = t ? totalHeal / t : 0;
    const dps = t ? totalDmg / t : 0;
    const dtps = t ? totalDtps / t : 0;

    // Icons: set later — leave src empty or placeholder
    const specIco = document.getElementById("ri-spec-icon");
    const heroIco = document.getElementById("ri-hero-icon");
    const fightIco = document.getElementById("ri-fight-icon");
    if (specIco) specIco.src = "";   // TODO: set when you have real icons
    if (heroIco) heroIco.src = "";
    if (fightIco) fightIco.src = "";

    // Names
    document.getElementById("ri-spec-name").textContent = res.specName || "—";
    document.getElementById("ri-hero-name").textContent = res.heroTlName || "—";
    document.getElementById("ri-fight-name").textContent = res.fightName || "—";

    // Difficulty (will be added to results)
    document.getElementById("ri-diff").textContent = labelDifficulty(res.difficulty);

    // Player name (will be added to results)
    document.getElementById("ri-player").textContent = res.playerName || "—";

    // Result (kill / wipe)
    const resultLabel = res.kill === true ? "Kill" : (res.kill === false ? "Wipe" : "—");
    document.getElementById("ri-result").textContent = resultLabel;

    // Length + per-second metrics
    document.getElementById("ri-length").textContent = fmtTimeSec(t);
    document.getElementById("ri-hps").textContent = fmt(hps);
    document.getElementById("ri-dps").textContent = fmt(dps);
    document.getElementById("ri-dtps").textContent = fmt(dtps);
}

// Call it once after you have `result`
renderFightInfo(result);


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
