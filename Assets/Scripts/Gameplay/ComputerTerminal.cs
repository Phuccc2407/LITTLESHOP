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
        ComputerView computerView = GameViewManager.Instance.ShowView<ComputerView>();
        if (computerView == null)
        {
            Debug.LogError("ComputerView is not registered in GameViewManager.All Views.", this);
        }
    }
}
