using System;
using UnityEngine;

public sealed class EconomyManager : MonoBehaviour
{
    [SerializeField, Min(0)] private int startingCash = 10000;
    [SerializeField, Min(1)] private int cashPerCoin = 100;

    public static EconomyManager Instance { get; private set; }
    public int CurrentCash { get; private set; }
    public int Revenue { get; private set; }
    public int Expenses { get; private set; }
    public int DailyRevenue { get; private set; }
    public int DailyExpenses { get; private set; }
    public int CashPerCoin { get { return cashPerCoin; } }

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
        DailyExpenses += amount;
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
        DailyRevenue += amount;
        CashChanged?.Invoke(CurrentCash);
    }

    public void AddCoinRevenue(int coins)
    {
        if (coins <= 0)
        {
            return;
        }

        AddRevenue(coins * cashPerCoin);
    }

    public void ResetDailyStats()
    {
        DailyRevenue = 0;
        DailyExpenses = 0;
    }

    public void RestoreProgress(ShopSaveData data)
    {
        CurrentCash = Mathf.Max(0, data.Cash);
        Revenue = Mathf.Max(0, data.Revenue);
        Expenses = Mathf.Max(0, data.Expenses);
        DailyRevenue = Mathf.Max(0, data.DailyRevenue);
        DailyExpenses = Mathf.Max(0, data.DailyExpenses);
        CashChanged?.Invoke(CurrentCash);
    }
}
