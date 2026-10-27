using System.Collections;
using UnityEngine;

public sealed class DeliveryManager : MonoBehaviour
{
    [SerializeField] private Transform deliverySpawnPoint;
    [SerializeField] private DeliveryBox deliveryBoxPrefab;
    [SerializeField, Min(0f)] private float deliveryDuration = 5f;

    public bool CreateOrder(ItemDefinition item)
    {
        if (item == null || EconomyManager.Instance == null || deliveryBoxPrefab == null || deliverySpawnPoint == null)
        {
            Debug.LogError("DeliveryManager is missing an item, economy, box prefab, or spawn point.", this);
            return false;
        }

        if (!EconomyManager.Instance.TrySpend(item.PurchasePrice))
        {
            return false;
        }

        StartCoroutine(Deliver(item));
        return true;
    }

    private IEnumerator Deliver(ItemDefinition item)
    {
        yield return new WaitForSeconds(deliveryDuration);
        DeliveryBox box = Instantiate(deliveryBoxPrefab, deliverySpawnPoint.position, deliverySpawnPoint.rotation);
        box.Initialize(item);
    }
}
