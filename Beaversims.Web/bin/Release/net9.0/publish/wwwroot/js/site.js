//site.js

(async function () {
    const host = document.getElementById("site-header");
    if (!host) return;

    const res = await fetch("/header.html", { cache: "no-cache" });
    host.innerHTML = await res.text();

    // Mobile toggle
    const header = host.querySelector(".site-header");
    const btn = host.querySelector(".menu-toggle");
    if (btn) {
        btn.addEventListener("click", () => {
            const open = header.getAttribute("data-open") === "1" ? "0" : "1";
            header.setAttribute("data-open", open);
            btn.setAttribute("aria-expanded", open === "1" ? "true" : "false");
        });
    }

    // Active link based on URL
    const path = location.pathname.toLowerCase();
    const map = [
        ["/stat-weights.html", "stat-weights"],
        ["/top-gear.html", "top-gear"],
        ["/trinket-overview.html", "trinkets"],
        ["/stat-alloc.html", "stat-alloc"]
    ];
    const match = map.find(([p]) => path.endsWith(p));
    if (match) {
        const a = host.querySelector(`a[data-nav="${match[1]}"]`);
        if (a) a.classList.add("active");
    }
})();