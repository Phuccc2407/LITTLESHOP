using UnityEngine;

public sealed class DeliveryBox : MonoBehaviour, IInteractable
{
    [SerializeField] private Rigidbody body;
    private ItemDefinition contents;
    private bool opened;
    private PlacementSystem placementSystem;
    public ItemDefinition Contents { get { return contents; } }

    public string InteractionPrompt
    {
        get { return opened ? string.Empty : "E - Open box"; }
    }

    public void Initialize(ItemDefinition item, PlacementSystem placement)
    {
        contents = item;
        placementSystem = placement;
    }

    public bool CanInteract(PlayerInteractor interactor)
    {
        return !opened && contents != null;
    }

    public void Interact(PlayerInteractor interactor)
    {
        Open();
    }

    private void Open()
    {
        if (opened || contents == null || placementSystem == null)
        {
            return;
        }

        if (placementSystem.BeginDirectPlacement(contents, this))
        {
            ShopAudio.Play(ShopSound.Unpack);
            opened = true;
            if (body != null)
            {
                body.isKinematic = true;
            }

            SetRenderersEnabled(false);
        }
    }

    public void HideForPlacement()
    {
        SetRenderersEnabled(false);
    }

    public void RestoreAfterCancelledPlacement()
    {
        opened = false;
        SetRenderersEnabled(true);
    }

    public void SetPlaced(Vector3 position)
    {
        transform.position = position;
    }

    public void DestroyAfterPlacement()
    {
        contents = null;
        Destroy(gameObject);
    }

    private void SetRenderersEnabled(bool enabled)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = enabled;
        }
    }

}
