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
                stateText.text = "Shop: --";
            }

            if (timeText != null)
            {
                timeText.text = "Time: --";
            }

            return;
        }

        if (stateText != null)
        {
            stateText.text = shopDayCycle.State == ShopDayState.Open
                ? "Shop: OPEN"
                : "Shop: CLOSED";
        }

        if (timeText != null)
        {
            timeText.text = "Day " + shopDayCycle.DayNumber + " - "
                + shopDayCycle.CurrentHour.ToString("00") + ":"
                + shopDayCycle.CurrentMinute.ToString("00");
        }

        if (customerText != null)
        {
            if (customerManager == null)
            {
                customerText.text = "Customers: --";
            }
            else
            {
                customerText.text = "Customers: " + customerManager.CurrentCustomerCount
                    + "\nEntered today: " + customerManager.CustomersSpawnedToday
                    + "\nServed today: " + customerManager.CustomersServedToday
                    + "\nSatisfaction: " + Mathf.RoundToInt(customerManager.AverageSatisfaction) + "%";
            }
        }
    }

    private void OnDayChanged(int dayNumber)
    {
        Refresh();
    }
}
