//utils.js

export function msToMinSec(ms) {
    const s = Math.max(0, Math.round(ms / 1000));
    const m = Math.floor(s / 60);
    const r = s % 60;
    return `${m}:${r.toString().padStart(2, "0")}`;
}

export const $ = (sel, root = document) => root.querySelector(sel);
export const $$ = (sel, root = document) => [...root.querySelectorAll(sel)];

export const fmt = (n, d = 2) =>
    (Number.isFinite(n) ? n : 0).toLocaleString(undefined, {
        minimumFractionDigits: d,
        maximumFractionDigits: d,
    });

export function el(tag, attrs = {}, ...children) {
    const node = document.createElement(tag);
    for (const [k, v] of Object.entries(attrs || {})) {
        if (k === "class") node.className = v;
        else if (k === "dataset") Object.assign(node.dataset, v);
        else if (k.startsWith("on") && typeof v === "function") node.addEventListener(k.slice(2), v);
        else node.setAttribute(k, v);
    }
    for (const c of children) node.append(c);
    return node;
}

export function icon(name) {
    const id = name?.toLowerCase().replace(/\s+/g, "_") || "default";
    return `/icons/${id}.jpg`;
}