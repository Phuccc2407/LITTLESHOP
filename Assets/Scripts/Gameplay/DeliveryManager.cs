using System.Collections.Generic;
using UnityEngine;

public sealed class DeliveryManager : MonoBehaviour
{
    [SerializeField] private Transform deliverySpawnPoint;
    [SerializeField] private DeliveryBox deliveryBoxPrefab;
    [SerializeField] private PlacementSystem placementSystem;
    [SerializeField, Min(0f)] private float deliveryDuration = 5f;
    private sealed class PendingOrder { public ItemDefinition Item; public float Remaining; }
    private readonly List<PendingOrder> orders = new List<PendingOrder>();

    public bool CreateOrder(ItemDefinition item)
    {
        if (item == null || EconomyManager.Instance == null || deliveryBoxPrefab == null
            || deliverySpawnPoint == null || placementSystem == null)
        {
            Debug.LogError("DeliveryManager is missing an item, economy, box prefab, or spawn point.", this);
            return false;
        }

        if (!EconomyManager.Instance.TrySpend(item.PurchasePrice))
        {
            return false;
        }

        orders.Add(new PendingOrder { Item = item, Remaining = deliveryDuration });
        return true;
    }

    private void Update()
    {
        if (GameSession.IsRestoring) return;
        for (int i = orders.Count - 1; i >= 0; i--)
        {
            orders[i].Remaining -= Time.deltaTime;
            if (orders[i].Remaining > 0f) continue;
            RestoreBox(orders[i].Item, deliverySpawnPoint.position, deliverySpawnPoint.rotation);
            orders.RemoveAt(i);
        }
    }

    public void RestoreBox(ItemDefinition item, Vector3 position, Quaternion rotation)
    {
        DeliveryBox box = Instantiate(deliveryBoxPrefab, position, rotation);
        box.Initialize(item, placementSystem);
    }

    public List<SavedOrder> CaptureProgress()
    {
        List<SavedOrder> saved = new List<SavedOrder>();
        foreach (PendingOrder order in orders)
            saved.Add(new SavedOrder { ItemId = order.Item.ItemId, RemainingSeconds = Mathf.Max(0f, order.Remaining) });
        return saved;
    }

    public void RestoreProgress(List<SavedOrder> saved, ShopItemCatalog catalog)
    {
        orders.Clear();
        foreach (SavedOrder order in saved)
            orders.Add(new PendingOrder { Item = catalog.Find(order.ItemId), Remaining = Mathf.Max(0f, order.RemainingSeconds) });
    }
}
