using TMPro;
using UnityEngine;

public sealed class MachineSettingsView : View
{
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_InputField coinsInput;
    private ArcadeMachine machine;

    public void Open(ArcadeMachine target)
    {
        machine = target;
        Refresh();
    }

    public void ApplySettings()
    {
        if (machine == null)
        {
            return;
        }

        int coins;
        if (coinsInput != null && int.TryParse(coinsInput.text, out coins))
        {
            machine.SetCoinsPerPlay(coins);
        }

        Refresh();
    }

    public void ToggleMachine()
    {
        if (machine != null)
        {
            machine.SetActive(machine.State == ArcadeMachineState.Inactive);
            Refresh();
        }
    }

    public void RepairMachine()
    {
        if (machine != null)
        {
            machine.Repair();
            Refresh();
        }
    }

    public void Close()
    {
        machine = null;
        if (GameViewManager.Instance != null)
        {
            GameViewManager.Instance.HideView<MachineSettingsView>();
        }
    }

    private void Refresh()
    {
        if (machine != null && statusText != null)
        {
            statusText.text = machine.GetStatus();
        }
    }
}
