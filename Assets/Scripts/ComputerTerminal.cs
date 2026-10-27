using UnityEngine;

public sealed class ComputerTerminal : MonoBehaviour, IInteractable
{
    public string InteractionPrompt { get { return "E - Use computer"; } }

    public bool CanInteract(PlayerInteractor interactor)
    {
        return GameViewManager.Instance != null;
    }

    public void Interact(PlayerInteractor interactor)
    {
        GameViewManager.Instance.ShowView<ComputerView>();
    }
}
