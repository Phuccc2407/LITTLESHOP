using TMPro;
using UnityEngine;

public sealed class ShopDaySummaryView : View
{
    [SerializeField] private ShopDayCycle shopDayCycle;
    [SerializeField] private CustomerManager customerManager;
    [SerializeField] private TMP_Text summaryText;

    protected override void Awake()
    {
        base.Awake();
        if (shopDayCycle == null)
        {
            shopDayCycle = FindObjectOfType<ShopDayCycle>();
        }

        if (customerManager == null)
        {
            customerManager = FindObjectOfType<CustomerManager>();
        }
    }

    private void OnEnable()
    {
        if (shopDayCycle != null)
        {
            shopDayCycle.DayEnded += ShowSummary;
        }
    }

    private void OnDisable()
    {
        if (shopDayCycle != null)
        {
            shopDayCycle.DayEnded -= ShowSummary;
        }
    }

    public void Close()
    {
        if (GameViewManager.Instance != null)
        {
            GameViewManager.Instance.HideView<ShopDaySummaryView>();
        }
    }

    private void ShowSummary(int dayNumber)
    {
        if (EconomyManager.Instance == null)
        {
            Debug.LogError("Cannot show shop day summary because EconomyManager is missing.", this);
            return;
        }

        int dailyRevenue = EconomyManager.Instance.DailyRevenue;
        int dailyExpenses = EconomyManager.Instance.DailyExpenses;
        int servedCustomers = customerManager == null
            ? 0
            : customerManager.CustomersServedToday;
        int profit = dailyRevenue - dailyExpenses;

        if (summaryText != null)
        {
            summaryText.text = "Day " + dayNumber + " summary\n"
                + "Revenue: ¥" + dailyRevenue.ToString("N0") + "\n"
                + "Expenses: ¥" + dailyExpenses.ToString("N0") + "\n"
                + "Profit: ¥" + profit.ToString("N0") + "\n"
                + "Customers served: " + servedCustomers;
        }

        if (GameViewManager.Instance != null)
        {
            GameViewManager.Instance.ShowView(this);
        }

        EconomyManager.Instance.ResetDailyStats();
    }
}
