using System;
using UnityEngine;

public enum ArcadeMachineState
{
    Inactive,
    Available,
    Occupied,
    OutOfService
}

public sealed class ArcadeMachine : MonoBehaviour, IInteractable
{
    [SerializeField] private ArcadeMachineDefinition definition;
    [SerializeField] private Transform playPoint;
    [SerializeField, Min(1)] private int coinsPerPlay = 1;
    [SerializeField] private bool active = true;
    [SerializeField, Range(0, 100)] private int condition = 100;
    [SerializeField, Range(1, 100)] private int conditionLossPerPlay = 10;
    private bool occupied;
    private bool reserved;

    public event Action StateChanged;

    public ArcadeMachineState State
    {
        get
        {
            if (!active) return ArcadeMachineState.Inactive;
            if (condition <= 0) return ArcadeMachineState.OutOfService;
            if (occupied) return ArcadeMachineState.Occupied;
            return ArcadeMachineState.Available;
        }
    }

    public ArcadeMachineDefinition Definition { get { return definition; } }
    public Transform PlayPoint { get { return playPoint == null ? transform : playPoint; } }
    public int CoinsPerPlay { get { return coinsPerPlay; } }
    public int Revenue { get; private set; }
    public bool IsReserved { get { return reserved; } }
    public int Condition { get { return condition; } }
    public int MaintenanceCost
    {
        get { return definition == null ? 0 : definition.MaintenanceCost; }
    }

    public void Initialize(ArcadeMachineDefinition machineDefinition)
    {
        definition = machineDefinition;
        if (machineDefinition != null)
        {
            coinsPerPlay = machineDefinition.CoinsPerPlay;
        }

        occupied = false;
        reserved = false;
    }

    public string InteractionPrompt
    {
        get { return State == ArcadeMachineState.Available ? "E - Configure machine" : "E - Inspect machine"; }
    }

    public bool CanInteract(PlayerInteractor interactor)
    {
        return GameViewManager.Instance != null;
    }

    public void Interact(PlayerInteractor interactor)
    {
        MachineSettingsView view = GameViewManager.Instance.ShowView<MachineSettingsView>();
        if (view != null)
        {
            view.Open(this);
        }
    }

    public void SetActive(bool value)
    {
        active = value;
        StateChanged?.Invoke();
    }

    public void SetCoinsPerPlay(int value)
    {
        coinsPerPlay = Mathf.Max(1, value);
    }

    public bool Repair()
    {
        if (condition >= 100)
        {
            return true;
        }

        if (EconomyManager.Instance == null)
        {
            Debug.LogError("Cannot repair arcade machine because EconomyManager is missing.", this);
            return false;
        }

        if (!EconomyManager.Instance.TrySpend(MaintenanceCost))
        {
            return false;
        }

        condition = 100;
        StateChanged?.Invoke();
        return true;
    }

    public bool TryStartPlay()
    {
        if (State != ArcadeMachineState.Available)
        {
            return false;
        }

        reserved = false;
        occupied = true;
        StateChanged?.Invoke();
        return true;
    }

    public bool TryReserve()
    {
        if (State != ArcadeMachineState.Available || reserved)
        {
            return false;
        }

        reserved = true;
        StateChanged?.Invoke();
        return true;
    }

    public void ReleaseReservation()
    {
        if (!reserved)
        {
            return;
        }

        reserved = false;
        StateChanged?.Invoke();
    }

    public bool CompletePlay()
    {
        if (!occupied)
        {
            return false;
        }

        occupied = false;
        reserved = false;
        condition = Mathf.Max(0, condition - conditionLossPerPlay);
        StateChanged?.Invoke();
        return true;
    }

    public void RecordCoinRevenue(int coins)
    {
        if (coins <= 0)
        {
            return;
        }

        Revenue += coins;
        StateChanged?.Invoke();
    }

    public void CancelPlay()
    {
        if (!occupied)
        {
            return;
        }

        occupied = false;
        reserved = false;
        StateChanged?.Invoke();
    }

    public string GetStatus()
    {
        string machineName = definition == null ? name : definition.DisplayName;
        int cashRevenue = EconomyManager.Instance == null
            ? 0
            : Revenue * EconomyManager.Instance.CashPerCoin;
        return machineName + "\nState: " + State + "\nPrice: " + coinsPerPlay
            + " coin(s)\nRevenue: " + Revenue + " coin(s) / ¥" + cashRevenue
            + "\nCondition: " + condition + "%\nRepair: ¥" + MaintenanceCost;
    }
}
