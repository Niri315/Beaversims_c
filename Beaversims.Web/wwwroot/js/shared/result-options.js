// /js/shared/result-options.js
// Simple enhancer for the Result Options block

export function initResultOptions(root = document.getElementById("result-options"), opts = {}) {
    if (!root) return null;

    const radios = [...root.querySelectorAll('input[type="radio"][name="ro-metric"]')];
    const dr = root.querySelector('#ro-dr');
    const support = root.querySelector('#ro-support');

    // Apply defaults if provided
    const def = opts.defaults || {};
    if (def.metric) {
        const r = radios.find(x => x.value === def.metric);
        if (r) r.checked = true;
    }
    if (typeof def.includeDR === "boolean") dr.checked = def.includeDR;
    if (typeof def.includeSupport === "boolean") support.checked = def.includeSupport;

    const get = () => ({
        metric: (radios.find(x => x.checked)?.value) || "total",
        includeDR: !!dr.checked,
        includeSupport: !!support.checked
    });

    const notify = () => opts.onChange?.(get());

    // Wire events
    radios.forEach(r => r.addEventListener("change", notify));
    dr.addEventListener("change", notify);
    support.addEventListener("change", notify);

    // Initial notify (optional)
    if (opts.emitInitial) notify();

    // Public API
    return {
        get,
        set(selection) {
            if (selection?.metric) {
                const r = radios.find(x => x.value === selection.metric);
                if (r) r.checked = true;
            }
            if ("includeDR" in (selection || {})) dr.checked = !!selection.includeDR;
            if ("includeSupport" in (selection || {})) support.checked = !!selection.includeSupport;
            notify();
        }
    };
}
