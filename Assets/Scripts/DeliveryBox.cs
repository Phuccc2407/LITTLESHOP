using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class DeliveryBox : MonoBehaviour, IInteractable
{
    [SerializeField] private Rigidbody body;
    private ItemDefinition contents;
    private bool opened;
    private bool carried;
    private Collider[] colliders;

    private void Awake()
    {
        colliders = GetComponentsInChildren<Collider>();
    }

    public string InteractionPrompt
    {
        get
        {
            if (opened)
            {
                return string.Empty;
            }

            return carried ? "E - Open box" : "E - Pick up box";
        }
    }

    public void Initialize(ItemDefinition item)
    {
        contents = item;
    }

    public bool CanInteract(PlayerInteractor interactor)
    {
        return !opened && contents != null;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (!carried)
        {
            interactor.TryCarryBox(this);
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("Cannot unpack delivery box without an InventoryManager.", this);
            return;
        }

        InventoryManager.Instance.Add(contents);
        opened = true;
        interactor.ClearCarriedBox(this);
        Destroy(gameObject);
    }

    public void SetCarried(Transform holdPoint)
    {
        carried = true;
        if (body != null)
        {
            body.isKinematic = true;
        }

        SetCollidersEnabled(false);
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void SetPlaced(Vector3 position)
    {
        carried = false;
        transform.SetParent(null);
        transform.position = position;
        SetCollidersEnabled(true);
        if (body != null)
        {
            body.isKinematic = false;
        }
    }

    private void SetCollidersEnabled(bool enabled)
    {
        if (colliders == null)
        {
            return;
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = enabled;
        }
    }
}
