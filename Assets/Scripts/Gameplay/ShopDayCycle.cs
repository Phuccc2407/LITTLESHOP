using System;
using System.Globalization;
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
    public static DateTime StartDate { get { return new DateTime(2026, 1, 1); } }
    public DateTime CurrentDate { get { return StartDate.AddDays(DayNumber - 1); } }
    public string DateLabel { get { return CurrentDate.ToString("dddd, dd/MM/yyyy", CultureInfo.InvariantCulture); } }
    public bool AwaitingSummary { get; private set; }
    private bool endingDay;
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
        if (customerManager != null) customerManager.SetSpawningEnabled(false);
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
        if (GameSession.IsRestoring || AwaitingSummary || endingDay) return;
        float minutesPerSecond = 1440f / (dayLengthMinutes * 60f);
        CurrentTimeMinutes += minutesPerSecond * Time.deltaTime;
        if (CurrentTimeMinutes >= closingHour * 60f)
        {
            EndDay();
        }
    }

    public void OpenShop()
    {
        if (AwaitingSummary || endingDay) return;
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

    public bool EndDay()
    {
        if (endingDay || AwaitingSummary) return false;
        endingDay = true;
        CloseShop();
        if (customerManager != null)
        {
            customerManager.SetSpawningEnabled(false);
            customerManager.FinishDay();
        }
        AwaitingSummary = true;
        bool hasSummary = DayEnded != null;
        // Subscribers read the completed date and financial totals before resets.
        DayEnded?.Invoke(DayNumber);
        if (EconomyManager.Instance != null) EconomyManager.Instance.ResetDailyStats();
        DayNumber++;
        CurrentTimeMinutes = openingHour * 60f;
        DayChanged?.Invoke(DayNumber);
        endingDay = false;
        if (!hasSummary) AwaitingSummary = false;
        if (GameSession.Instance != null) GameSession.Instance.SaveGame();
        return true;
    }

    public void DismissSummary() { AwaitingSummary = false; }

    public void RestoreProgress(int day, float minutes, bool open)
    {
        DayNumber = Mathf.Max(1, day);
        CurrentTimeMinutes = Mathf.Clamp(minutes, 0f, 1439.99f);
        AwaitingSummary = endingDay = false;
        State = ShopDayState.Closed;
        if (customerManager != null) customerManager.SetSpawningEnabled(false);
        DayChanged?.Invoke(DayNumber);
        StateChanged?.Invoke(State);
        if (open && CanOpenShop) OpenShop();
    }

}
