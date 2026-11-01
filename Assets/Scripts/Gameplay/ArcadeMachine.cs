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
    private bool occupied;

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

    public void Initialize(ArcadeMachineDefinition machineDefinition)
    {
        definition = machineDefinition;
        if (machineDefinition != null)
        {
            coinsPerPlay = machineDefinition.CoinsPerPlay;
        }

        occupied = false;
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

    public void Repair()
    {
        condition = 100;
        StateChanged?.Invoke();
    }

    public bool TryStartPlay()
    {
        if (State != ArcadeMachineState.Available)
        {
            return false;
        }

        occupied = true;
        StateChanged?.Invoke();
        return true;
    }

    public bool CompletePlay()
    {
        if (!occupied)
        {
            return false;
        }

        if (EconomyManager.Instance == null)
        {
            Debug.LogError("Cannot complete arcade play because EconomyManager is missing.", this);
            return false;
        }

        occupied = false;
        Revenue += coinsPerPlay;
        EconomyManager.Instance.AddCoinRevenue(coinsPerPlay);
        StateChanged?.Invoke();
        return true;
    }

    public void CancelPlay()
    {
        if (!occupied)
        {
            return;
        }

        occupied = false;
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
            + "\nCondition: " + condition + "%";
    }
}
