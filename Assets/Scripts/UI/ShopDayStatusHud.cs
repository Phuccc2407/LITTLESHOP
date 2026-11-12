using TMPro;
using UnityEngine;

public sealed class ShopDayStatusHud : MonoBehaviour
{
    [SerializeField] private ShopDayCycle shopDayCycle;
    [SerializeField] private CustomerManager customerManager;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text customerText;

    private void Awake()
    {
        if (shopDayCycle == null)
        {
            shopDayCycle = FindAnyObjectByType<ShopDayCycle>();
        }

        if (customerManager == null)
        {
            customerManager = FindAnyObjectByType<CustomerManager>();
        }
        if (timeText != null) { timeText.enableAutoSizing = true; timeText.fontSizeMin = 18f; timeText.fontSizeMax = 28f; }
    }

    private void OnEnable()
    {
        if (shopDayCycle != null)
        {
            shopDayCycle.StateChanged += OnShopStateChanged;
            shopDayCycle.DayChanged += OnDayChanged;
        }

        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void OnDisable()
    {
        if (shopDayCycle != null)
        {
            shopDayCycle.StateChanged -= OnShopStateChanged;
            shopDayCycle.DayChanged -= OnDayChanged;
        }
    }

    private void OnShopStateChanged(ShopDayState state)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (shopDayCycle == null)
        {
            if (stateText != null)
            {
                stateText.text = "SHOP: --";
            }

            if (timeText != null)
            {
                timeText.text = "TIME: --";
            }

            return;
        }

        if (stateText != null)
        {
            stateText.text = shopDayCycle.State == ShopDayState.Open
                ? "SHOP: OPEN"
                : "SHOP: CLOSED";
        }

        if (timeText != null)
        {
            timeText.text = shopDayCycle.DateLabel + "\n"
                + shopDayCycle.CurrentHour.ToString("00") + ":"
                + shopDayCycle.CurrentMinute.ToString("00");
        }

        if (customerText != null)
        {
            if (customerManager == null)
            {
                customerText.text = "CUSTOMERS: --";
            }
            else
            {
                customerText.text = "CUSTOMERS: " + customerManager.CurrentCustomerCount
                    + "\nENTERED TODAY: " + customerManager.CustomersSpawnedToday
                    + "\nSERVED TODAY: " + customerManager.CustomersServedToday;
            }
        }
    }

    private void OnDayChanged(int dayNumber)
    {
        Refresh();
    }
}
