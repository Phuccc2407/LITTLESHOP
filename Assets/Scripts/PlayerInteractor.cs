using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField, Min(0.1f)] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactionLayers = ~0;
    [SerializeField] private InteractTextView interactTextView;
    [SerializeField] private Transform holdPoint;

    private InputAction interactAction;
    private IInteractable currentInteractable;
    private DeliveryBox carriedBox;

    public Transform HoldPoint { get { return holdPoint; } }

    private void Awake()
    {
        if (inputActions != null)
        {
            interactAction = inputActions.FindActionMap("Player", true).FindAction("Interact", true);
        }
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.actionMap.Enable();
            interactAction.performed += OnInteract;
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.performed -= OnInteract;
        }
    }

    private void Update()
    {
        currentInteractable = FindInteractable();
        if (interactTextView == null)
        {
            return;
        }

        if (currentInteractable != null && currentInteractable.CanInteract(this))
        {
            interactTextView.SetText(currentInteractable.InteractionPrompt);
            if (!interactTextView.IsVisible && GameViewManager.Instance != null)
            {
                GameViewManager.Instance.ShowView(interactTextView, false);
            }
        }
        else if (interactTextView.IsVisible && GameViewManager.Instance != null)
        {
            GameViewManager.Instance.HideView<InteractTextView>();
        }
    }

    public bool TryCarryBox(DeliveryBox box)
    {
        if (carriedBox != null || holdPoint == null)
        {
            return false;
        }

        carriedBox = box;
        box.SetCarried(holdPoint);
        return true;
    }

    public void ReleaseBox()
    {
        if (carriedBox != null)
        {
            carriedBox.SetPlaced(transform.position + transform.forward);
            carriedBox = null;
        }
    }

    public void ClearCarriedBox(DeliveryBox box)
    {
        if (carriedBox == box)
        {
            carriedBox = null;
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (GameViewManager.Instance != null && GameViewManager.Instance.IsBlockingViewOpen)
        {
            return;
        }

        if (currentInteractable != null && currentInteractable.CanInteract(this))
        {
            currentInteractable.Interact(this);
        }
    }

    private IInteractable FindInteractable()
    {
        if (playerCamera == null)
        {
            return null;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactionRange, interactionLayers, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.GetComponentInParent<IInteractable>();
        }

        return null;
    }
}
