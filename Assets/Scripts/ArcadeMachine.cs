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
    [SerializeField, Min(1)] private int coinsPerPlay = 1;
    [SerializeField] private bool active = true;
    [SerializeField, Range(0, 100)] private int condition = 100;

    public ArcadeMachineState State
    {
        get
        {
            if (!active) return ArcadeMachineState.Inactive;
            if (condition <= 0) return ArcadeMachineState.OutOfService;
            return ArcadeMachineState.Available;
        }
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
    }

    public void SetCoinsPerPlay(int value)
    {
        coinsPerPlay = Mathf.Max(1, value);
    }

    public void Repair()
    {
        condition = 100;
    }

    public string GetStatus()
    {
        string machineName = definition == null ? name : definition.DisplayName;
        return machineName + "\nState: " + State + "\nPrice: " + coinsPerPlay + " coin(s)\nCondition: " + condition + "%";
    }
}
