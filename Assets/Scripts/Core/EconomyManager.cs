using System;
using UnityEngine;

public sealed class EconomyManager : MonoBehaviour
{
    [SerializeField, Min(0)] private int startingCash = 10000;

    public static EconomyManager Instance { get; private set; }
    public int CurrentCash { get; private set; }
    public int Revenue { get; private set; }
    public int Expenses { get; private set; }

    public event Action<int> CashChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentCash = startingCash;
    }

    public bool TrySpend(int amount)
    {
        if (amount < 0 || CurrentCash < amount)
        {
            return false;
        }

        CurrentCash -= amount;
        Expenses += amount;
        CashChanged?.Invoke(CurrentCash);
        return true;
    }

    public void AddRevenue(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentCash += amount;
        Revenue += amount;
        CashChanged?.Invoke(CurrentCash);
    }
}
