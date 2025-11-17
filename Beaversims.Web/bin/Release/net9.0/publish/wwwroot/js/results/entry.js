import { bootBase, moduleTitleFromQuery } from "./base.js";

const page = new URLSearchParams(location.search).get("m") || "sw";
document.getElementById("page-title").textContent = moduleTitleFromQuery(page);

// Boot shared pieces (decode, header, result options, helpers)
const { result, ro, mounts } = await bootBase(); // mounts: { mount }

switch (page.toLowerCase()) {
    case "tg": {
        const mod = await import("./tg.js");
        mod.render({ result, ro, mount: mounts.mount });
        break;
    }
    case "sw":
    default: {
        const mod = await import("./sw.js");
        mod.render({ result, ro, mount: mounts.mount });
        break;
    }
}
