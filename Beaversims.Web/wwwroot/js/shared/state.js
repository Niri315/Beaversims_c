// simple global state + pub/sub
export const state = {
    reportCode: null,
    fight: null,
    healer: null,
};

const subs = new Set();
export function set(partial) {
    Object.assign(state, partial);
    subs.forEach((fn) => fn(state));
}
export function onChange(fn) {
    subs.add(fn);
    return () => subs.delete(fn);
}
export function reset() {
    set({ reportCode: null, fight: null, healer: null });
}