using UnityEngine;

[DisallowMultipleComponent]
public sealed class EndDayDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private ShopDayCycle shopDayCycle;
    public string InteractionPrompt { get { return "E - End business day and view revenue"; } }
    private void Awake()
    {
        if (shopDayCycle == null) shopDayCycle = FindAnyObjectByType<ShopDayCycle>();
    }
    public bool CanInteract(PlayerInteractor interactor)
    {
        return shopDayCycle != null && !shopDayCycle.AwaitingSummary;
    }
    public void Interact(PlayerInteractor interactor)
    {
        if (CanInteract(interactor)) shopDayCycle.EndDay();
    }
}
