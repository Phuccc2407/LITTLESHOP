using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class InventoryManager : MonoBehaviour
{
    [Serializable]
    private sealed class InventoryEntry
    {
        public ItemDefinition item;
        public int quantity;
    }

    [SerializeField] private List<InventoryEntry> entries = new List<InventoryEntry>();

    public static InventoryManager Instance { get; private set; }
    public event Action InventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Add(ItemDefinition item, int quantity = 1)
    {
        if (item == null || quantity <= 0)
        {
            return;
        }

        InventoryEntry entry = FindEntry(item);
        if (entry == null)
        {
            entry = new InventoryEntry { item = item, quantity = 0 };
            entries.Add(entry);
        }

        entry.quantity += quantity;
        InventoryChanged?.Invoke();
    }

    public bool Remove(ItemDefinition item, int quantity = 1)
    {
        InventoryEntry entry = FindEntry(item);
        if (entry == null || quantity <= 0 || entry.quantity < quantity)
        {
            return false;
        }

        entry.quantity -= quantity;
        if (entry.quantity == 0)
        {
            entries.Remove(entry);
        }

        InventoryChanged?.Invoke();
        return true;
    }

    public int GetQuantity(ItemDefinition item)
    {
        InventoryEntry entry = FindEntry(item);
        return entry == null ? 0 : entry.quantity;
    }

    private InventoryEntry FindEntry(ItemDefinition item)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].item == item)
            {
                return entries[i];
            }
        }

        return null;
    }
}
