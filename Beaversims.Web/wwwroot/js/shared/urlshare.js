export function encodeToUrl(obj) {
    return btoa(JSON.stringify(obj));
}
export function decodeFromUrl(search = location.search) {
    const p = new URLSearchParams(search).get("data");
    if (!p) return null;
    try { return JSON.parse(atob(p)); } catch { return null; }
}
