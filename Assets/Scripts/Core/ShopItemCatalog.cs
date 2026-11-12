using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Little Shop/Save Item Catalog")]
public sealed class ShopItemCatalog : ScriptableObject
{
    [SerializeField] private ItemDefinition[] items;
    private Dictionary<string, ItemDefinition> lookup;

    public ItemDefinition Find(string id)
    {
        if (lookup == null)
        {
            lookup = new Dictionary<string, ItemDefinition>(StringComparer.Ordinal);
            if (items != null)
                foreach (ItemDefinition item in items)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.ItemId)) continue;
                    if (lookup.ContainsKey(item.ItemId)) throw new InvalidOperationException("Duplicate save item ID: " + item.ItemId);
                    lookup.Add(item.ItemId, item);
                }
        }
        return id != null && lookup.TryGetValue(id, out ItemDefinition value) ? value : null;
    }
}
