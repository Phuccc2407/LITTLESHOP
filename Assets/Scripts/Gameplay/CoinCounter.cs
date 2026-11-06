using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class CoinCounter : MonoBehaviour, IInteractable
{
    [SerializeField] private string purchaseTrigger = "CustomerPurchase";
    [SerializeField] private Transform[] queuePoints;

    private readonly List<Customer> queue = new List<Customer>();

    public string InteractionPrompt
    {
        get
        {
            return queue.Count == 0
                ? "Quầy coin - không có khách"
                : "E - Bán coin cho khách (" + queue[0].RequiredCoins + " coin)";
        }
    }

    public bool CanInteract(PlayerInteractor interactor)
    {
        return queue.Count > 0 && queue[0] != null && queue[0].IsWaitingForCoins
            && GameViewManager.Instance != null;
    }

    public void Interact(PlayerInteractor interactor)
    {
        CoinSaleView view = GameViewManager.Instance.ShowView<CoinSaleView>();
        if (view != null)
        {
            view.Open(this, queue[0]);
        }
    }

    public bool RegisterCustomer(Customer customer)
    {
        if (customer == null || queue.Contains(customer))
        {
            return false;
        }

        queue.Add(customer);
        RefreshQueuePositions();
        return true;
    }

    public void UnregisterCustomer(Customer customer)
    {
        if (customer == null || !queue.Remove(customer))
        {
            return;
        }

        RefreshQueuePositions();
    }

    public bool SellToNextCustomer(int amount)
    {
        if (queue.Count == 0 || queue[0] == null || !queue[0].IsWaitingForCoins || amount <= 0)
        {
            return false;
        }

        Customer customer = queue[0];
        if (amount < customer.RequiredCoins || EconomyManager.Instance == null)
        {
            return false;
        }

        EconomyManager.Instance.AddCoinRevenue(amount);

        queue.RemoveAt(0);
        customer.ReceiveCoins(customer.RequiredCoins);
        RefreshQueuePositions();
        return true;
    }

    public Vector3 GetQueuePosition(int index)
    {
        if (queuePoints != null && index >= 0 && index < queuePoints.Length
            && queuePoints[index] != null)
        {
            return queuePoints[index].position;
        }

        return transform.position;
    }

    public Quaternion GetQueueRotation(int index)
    {
        if (queuePoints != null && index >= 0 && index < queuePoints.Length
            && queuePoints[index] != null)
        {
            return queuePoints[index].rotation;
        }

        return transform.rotation;
    }

    private void RefreshQueuePositions()
    {
        for (int i = 0; i < queue.Count; i++)
        {
            if (queue[i] != null)
            {
                queue[i].MoveToCoinQueuePosition(GetQueuePosition(i), GetQueueRotation(i));
            }
        }
    }
}
