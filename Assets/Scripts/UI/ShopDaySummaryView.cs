using TMPro;
using UnityEngine;

public sealed class ShopDaySummaryView : View
{
    [SerializeField] private ShopDayCycle shopDayCycle;
    [SerializeField] private CustomerManager customerManager;
    [SerializeField] private TMP_Text summaryText;
    private bool presented;

    public override void OnShow() { presented = true; }
    public override void OnHide()
    {
        if (presented && shopDayCycle != null) shopDayCycle.DismissSummary();
        presented = false;
    }

    protected override void Awake()
    {
        base.Awake();
        if (shopDayCycle == null)
        {
            shopDayCycle = FindAnyObjectByType<ShopDayCycle>();
        }

        if (customerManager == null)
        {
            customerManager = FindAnyObjectByType<CustomerManager>();
        }
        if (summaryText != null) { summaryText.enableAutoSizing = true; summaryText.fontSizeMin = 22f; summaryText.fontSizeMax = 36f; }
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
        if (shopDayCycle != null) shopDayCycle.DismissSummary();
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
            summaryText.text = ShopDayCycle.StartDate.AddDays(dayNumber - 1)
                .ToString("dddd, dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) + " - Summary\n"
                + "Revenue: \u00A5" + dailyRevenue.ToString("N0") + "\n"
                + "Expenses: \u00A5" + dailyExpenses.ToString("N0") + "\n"
                + "Profit: \u00A5" + profit.ToString("N0") + "\n"
                + "Customers served: " + servedCustomers;
        }

        if (GameViewManager.Instance != null)
        {
            GameViewManager.Instance.ShowView(this);
        }

    }
}
