export function mapInventoryTypeToSlot(inventoryType) {
    const map = { 1: 0, 2: 1, 3: 2, 4: 14, 5: 4, 6: 5, 7: 6, 8: 7, 9: 8, 10: 9, 11: 10, 12: 12, 13: 15, 14: 16 };
    return map[inventoryType] ?? null;
}