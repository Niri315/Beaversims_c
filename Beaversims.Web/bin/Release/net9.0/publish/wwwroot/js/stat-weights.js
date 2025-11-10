//stat-weights.js

import { renderPicker } from "./shared/ui-picker.js";
import { state, onChange } from "./shared/state.js";
import { fetchLogsRaw } from "./shared/wcl.js";
import { runStatWeights } from "./shared/wasm.js";
import { encodeToUrl } from "./shared/urlshare.js";

const runBtn = document.getElementById("run-sim-btn");
const pickerHost = document.getElementById("import-form-container");

document.addEventListener("DOMContentLoaded", async () => {
    await renderPicker(pickerHost);

    onChange(({ healer, fight }) => {
        runBtn.disabled = !(healer && fight);
    });

    runBtn.addEventListener("click", async () => {
        const { reportCode, fight, healer } = state;
        if (!reportCode || !fight || !healer) return;

        runBtn.disabled = true;
        runBtn.textContent = "Running Simulation...";

        try {
            const logs = await fetchLogsRaw(reportCode, fight.id, healer.id);
            const result = await runStatWeights(logs, healer.id, reportCode);
            const url = `/results.html?data=${encodeToUrl(result)}`;
            location.href = url;
        } catch (err) {
            console.error(err);
            alert("Simulation failed: " + (err.message || err));
        } finally {
            runBtn.textContent = "Run Simulation";
            runBtn.disabled = false;
        }
    });
});

export const STAT_COLORS = {
    Intellect: "#FFFFFF",   // white
    Stamina: "#00FF00",     // green
    Vers: "#A37F40",        // brown-ish
    Haste: "#FFFF00",       // yellow
    Crit: "#FF0000",        // red
    Mastery: "#0000FF",     // blue
    Avoidance: "#00CED1",   // teal
    Leech: "#800080"        // purple
};
