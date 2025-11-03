

function decodeData() {
    const params = new URLSearchParams(window.location.search);
    const encoded = params.get("data");
    if (!encoded) return null;
    try {
        return JSON.parse(atob(encoded));
    } catch (e) {
        console.error("Invalid data in URL", e);
        return null;
    }
}

const result = decodeData();
if (!result) {
    document.body.innerHTML = "<h2>No result data provided.</h2>";
    throw new Error("No result data");
}

const gearsets = result.altGearSets.map(gs => ({
    name: gs.name || `Set ${gs.id}`,
    total: Object.values(gs.gains).reduce((a, b) => a + b, 0)
}));

gearsets.sort((a, b) => b.total - a.total);

const ctx = document.getElementById("resultsChart").getContext("2d");

const colors = [
    "#4caf50", "#2196f3", "#9c27b0",
    "#ff9800", "#f44336", "#00bcd4"
];

new Chart(ctx, {
    type: "bar",
    data: {
        labels: gearsets.map(g => g.name),
        datasets: [{
            label: "Total Gains",
            data: gearsets.map(g => g.total.toFixed(2)),
            backgroundColor: colors.slice(0, gearsets.length)
        }]
    },
    options: {
        indexAxis: "y",
        scales: {
            x: {
                ticks: { color: "#eee" },
                grid: { color: "rgba(255,255,255,0.1)" }
            },
            y: {
                ticks: { color: "#eee" },
                grid: { display: false }
            }
        },
        plugins: {
            legend: { display: false },
            tooltip: {
                callbacks: {
                    label: (ctx) => ` ${ctx.parsed.x.toFixed(2)} total`
                }
            },
            title: {
                display: true,
                text: `Total Time: ${result.totalTime.toFixed(1)}s`,
                color: "#ccc"
            }
        }
    }
});
