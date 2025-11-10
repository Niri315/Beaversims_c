import { initResultOptions } from "/js/shared/result-options.js";
import { HEALING_SPECS, HERO_TALENTS, CLASS_COLORS} from "/js/shared/constants.js";



const ALLOC_BY_SPEC = {
    hpal: {
        Lightsmith: {
            damage: { haste: 50, crit: 30, mastery: 0, vers: 20},
            heal: { haste: 30, crit: 50, mastery: 0, vers: 20},
            total: { haste: 30, crit: 50, mastery: 0, vers: 20},
            heal_dr: { haste: 20, crit: 40, mastery: 0, vers: 40},
            total_dr: { haste: 30, crit: 40, mastery: 0, vers: 30},
        },

        "Herald of the Sun": {
            damage: { haste: 0, crit: 30, mastery: 50, vers: 20, score: 0 },
            heal: { haste: 30, crit: 50, mastery: 0, vers: 20, score: 0 },
            total: { haste: 30, crit: 50, mastery: 0, vers: 20, score: 0 },
            heal_dr: { haste: 25, crit: 35, mastery: 0, vers: 40, score: 0 },
            total_dr: { haste: 30, crit: 40, mastery: 0, vers: 30, score: 0 },
        },
    },
};

function getAlloc(specId, heroName, selection) {
    const key = selection.metric + (selection.includeDR ? "_dr" : "");
    return ALLOC_BY_SPEC[specId][heroName][key];
}

document.addEventListener("DOMContentLoaded", () => {
    const specSel = document.getElementById("sa-spec");
    const heroSel = document.getElementById("sa-hero");

    const ro = initResultOptions(document.getElementById("result-options"), {
        defaults: { metric: "total", includeDR: true, includeSupport: true },
        onChange: (sel) => {
            recompute();
        },
        emitInitial: false
    });
    // Populate spec dropdown
    for (const s of HEALING_SPECS) {
        const opt = document.createElement("option");
        opt.value = s.id;
        opt.textContent = s.label;
        specSel.appendChild(opt);
    }

    // Radar setup
    const ctx = document.getElementById("sa-radar").getContext("2d");
    const radar = new Chart(ctx, {
        type: "radar",
        data: {
            labels: ["Haste", "Mastery", "Versatility", "Critical Strike"],
            datasets: [{
                label: "Best Allocation",
                data: [0, 0, 0, 0],
                borderWidth: 2,
                fill: true,
                backgroundColor: "rgba(100, 230, 255, 0)",
                borderColor: "rgba(120, 240, 255, 0)",
                pointBackgroundColor: "rgba(180, 255, 255, 0)"
            }]
        },
        options: {
            responsive: true,
            scales: {
                r: {
                    min: 0,
                    max: 100,
                    beginAtZero: true,
                    angleLines: { color: "rgba(255,255,255,.2)" },
                    grid: { color: "rgba(255,255,255,.15)" },
                    pointLabels: { color: "#dcdcee", font: { size: 16 } },
                    ticks: { display: false, showLabelBackdrop: false }
                }
            },
            plugins: { legend: { display: false } }
        }
    });

    specSel.addEventListener("change", () => {

        heroSel.innerHTML = `<option value="" disabled selected>Select a hero talent…</option>`;
        const list = HERO_TALENTS[specSel.value] || [];
        for (const h of list) {
            const opt = document.createElement("option");
            opt.value = h;
            opt.textContent = h;
            heroSel.appendChild(opt);
        }
        heroSel.disabled = list.length === 0;

        resetChart();
    });

    heroSel.addEventListener("change", () => {
        recompute();
    });

    function recompute() {
        const specId = specSel.value;
        const hero = heroSel.value;
        if (!specId || !hero) return; // need both before drawing

        const sel = ro.get();    
        const alloc = getAlloc(specId, hero, sel);

        updateValues(alloc);
        updateChart(alloc);
    }


    function resetChart() {
        document.getElementById("sa-best").textContent = "Best Allocation";
        set("#val-crit", 0); set("#val-haste", 0); set("#val-mastery", 0); set("#val-vers", 0);
        radar.data.datasets[0].data = [0, 0, 0, 0];
        radar.data.datasets[0].backgroundColor = "rgba(100, 230, 255, 0)";
        radar.data.datasets[0].borderColor = "rgba(120, 240, 255, 0)";
        radar.data.datasets[0].pointBackgroundColor = "rgba(180, 255, 255, 0)";
        radar.update();
    }

    function updateValues(alloc) {
        const best = document.getElementById("sa-best");
        best.textContent = "Top Allocation";
        set("#val-crit", alloc.crit);
        set("#val-haste", alloc.haste);
        set("#val-mastery", alloc.mastery);
        set("#val-vers", alloc.vers);
    }

    function updateChart(alloc) {
        const sum = alloc.crit + alloc.haste + alloc.mastery + alloc.vers;
        const pct = sum ? {
            haste: (alloc.haste / sum) * 100,
            mastery: (alloc.mastery / sum) * 100,
            vers: (alloc.vers / sum) * 100,
            crit: (alloc.crit / sum) * 100
        } : { haste: 0, mastery: 0, vers: 0, crit: 0 };

        radar.data.datasets[0].data = [pct.haste, pct.mastery, pct.vers, pct.crit];
        radar.data.datasets[0].backgroundColor = "rgba(100, 230, 255, 0.25)";
        radar.data.datasets[0].borderColor = "rgba(120, 240, 255, 0.95)";
        radar.data.datasets[0].pointBackgroundColor = "rgba(180, 255, 255, 0.9)";
        radar.update();
    }

    function set(sel, val) {
        const el = document.querySelector(sel);
        if (el) el.textContent = val;
    }
});
