using System;
using UnityEngine;

public enum ShopDayState
{
    Closed,
    Open
}

public sealed class ShopDayCycle : MonoBehaviour
{
    [SerializeField] private CustomerManager customerManager;
    [SerializeField, Min(0.1f)] private float dayLengthMinutes = 10f;
    [SerializeField, Range(0, 23)] private int openingHour = 9;
    [SerializeField, Range(1, 23)] private int closingHour = 22;
    [SerializeField] private bool startsOpen;

    public ShopDayState State { get; private set; }
    public int DayNumber { get; private set; } = 1;
    public float CurrentTimeMinutes { get; private set; }
    public float RemainingTimeMinutes
    {
        get { return Mathf.Max(0f, closingHour * 60f - CurrentTimeMinutes); }
    }

    public int CurrentHour { get { return Mathf.FloorToInt(CurrentTimeMinutes / 60f); } }
    public int CurrentMinute { get { return Mathf.FloorToInt(CurrentTimeMinutes % 60f); } }
    public bool CanOpenShop
    {
        get
        {
            return CurrentTimeMinutes >= openingHour * 60f
                && CurrentTimeMinutes < closingHour * 60f;
        }
    }
    public event Action<ShopDayState> StateChanged;
    public event Action<int> DayChanged;
    public event Action<int> DayEnded;

    private void Start()
    {
        if (closingHour <= openingHour)
        {
            Debug.LogError("ShopDayCycle requires closing hour to be after opening hour.", this);
            closingHour = Mathf.Min(23, openingHour + 1);
        }

        CurrentTimeMinutes = openingHour * 60f;
        if (startsOpen)
        {
            OpenShop();
        }
        else
        {
            CloseShop();
        }
    }

    private void Update()
    {
        float minutesPerSecond = 1440f / (dayLengthMinutes * 60f);
        CurrentTimeMinutes += minutesPerSecond * Time.deltaTime;
        if (CurrentTimeMinutes >= 1440f)
        {
            DayEnded?.Invoke(DayNumber);
            CurrentTimeMinutes -= 1440f;
            DayNumber++;
            DayChanged?.Invoke(DayNumber);
        }

        if (State == ShopDayState.Open && !CanOpenShop)
        {
            CloseShop();
        }
    }

    public void OpenShop()
    {
        if (State == ShopDayState.Open)
        {
            return;
        }

        if (!CanOpenShop)
        {
            return;
        }

        State = ShopDayState.Open;
        if (customerManager != null)
        {
            customerManager.SetSpawningEnabled(true);
        }

        StateChanged?.Invoke(State);
    }

    public void CloseShop()
    {
        if (State == ShopDayState.Closed)
        {
            return;
        }

        State = ShopDayState.Closed;
        if (customerManager != null)
        {
            customerManager.SetSpawningEnabled(false);
        }

        StateChanged?.Invoke(State);
    }

}
