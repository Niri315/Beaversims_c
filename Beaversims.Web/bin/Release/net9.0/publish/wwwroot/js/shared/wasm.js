//wasm.js

let __initPromise, __runSW;

export async function initSim() {
    if (__runSW) return __runSW;
    if (!__initPromise) {
        __initPromise = (async () => {
            const { dotnet } = await import("/wasm/_framework/dotnet.js");
            const runtime = await dotnet.create();
            const cfg = runtime.getConfig();
            const exp = await runtime.getAssemblyExports(cfg.mainAssemblyName);
            const sw = exp?.Beaversims?.Wasm?.Api?.RunStatWeights;
            const tg = exp?.Beaversims?.Wasm?.Api?.RunTopGear; // optional later
            if (!sw) throw new Error("Missing Beaversims.Wasm.Api.RunStatWeights");
            return { sw, tg };
        })();
    }
    __runSW = await __initPromise;
    return __runSW;
}

export async function runStatWeights(logsJson, userId, reportCode) {
    const { sw } = await initSim();
    const json = await sw(String(logsJson), (userId | 0), String(reportCode));
    try { return JSON.parse(json); } catch { return json; }
}

// placeholder for later
export async function runTopGear(logsJson, userId, reportCode, gearDirectiveJson) {
    const { tg } = await initSim();
    if (!tg) throw new Error("RunTopGear not exported yet");
    const json = await tg(String(logsJson), (userId | 0), String(reportCode), String(gearDirectiveJson));
    try { return JSON.parse(json); } catch { return json; }
}