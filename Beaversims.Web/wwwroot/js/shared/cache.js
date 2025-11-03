// in-memory caches to avoid duplicate network calls
export const cache = {
    fights: new Map(),    // key: reportCode
    healers: new Map(),   // key: `${reportCode}:${fightId}`
    logs: new Map(),      // key: `${reportCode}:${fightId}:${userId}`
};

export async function getOrSet(map, key, factory) {
    if (map.has(key)) return map.get(key);
    const val = await factory();
    map.set(key, val);
    return val;
}