using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField, Min(0.1f)] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactionLayers = ~0;
    [SerializeField] private InteractTextView interactTextView;

    private InputAction interactAction;
    private IInteractable currentInteractable;
    private Outline currentOutline;
    private CustomerWorldStatus currentCustomerStatus;

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
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.actionMap.Disable();
        }

        SetOutline(null);
        SetCustomerStatus(null);
    }

    private void Update()
    {
        if (GameSession.IsRestoring) return;
        Outline nextOutline;
        CustomerWorldStatus nextCustomerStatus;
        currentInteractable = FindInteractable(out nextOutline, out nextCustomerStatus);
        if (currentInteractable == null || !currentInteractable.CanInteract(this))
        {
            if (nextCustomerStatus == null)
            {
                nextOutline = null;
            }
        }

        SetOutline(nextOutline);
        SetCustomerStatus(nextCustomerStatus);
        if (interactTextView != null && currentInteractable != null && currentInteractable.CanInteract(this))
        {
            interactTextView.SetText(currentInteractable.InteractionPrompt);
            if (!interactTextView.IsVisible && GameViewManager.Instance != null)
            {
                GameViewManager.Instance.ShowView(interactTextView, false);
            }
        }
        else if (interactTextView != null && interactTextView.IsVisible && GameViewManager.Instance != null)
        {
            GameViewManager.Instance.HideView<InteractTextView>();
        }

        if (interactAction != null && interactAction.WasPressedThisFrame())
        {
            TryInteract();
        }
    }

    private void TryInteract()
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

    private IInteractable FindInteractable(
        out Outline outline,
        out CustomerWorldStatus customerStatus)
    {
        outline = null;
        customerStatus = null;
        if (playerCamera == null)
        {
            return null;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactionRange, interactionLayers, QueryTriggerInteraction.Ignore))
        {
            Customer customer = hit.collider.GetComponentInParent<Customer>();
            if (customer != null)
            {
                customerStatus = customer.GetComponentInChildren<CustomerWorldStatus>(true);
            }

            outline = hit.collider.GetComponentInParent<Outline>();
            return hit.collider.GetComponentInParent<IInteractable>();
        }

        return null;
    }

    private void SetOutline(Outline outline)
    {
        if (currentOutline == outline)
        {
            return;
        }

        if (currentOutline != null)
        {
            currentOutline.enabled = false;
        }

        currentOutline = outline;
        if (currentOutline != null)
        {
            currentOutline.enabled = true;
        }
    }

    private void SetCustomerStatus(CustomerWorldStatus status)
    {
        if (currentCustomerStatus == status)
        {
            return;
        }

        if (currentCustomerStatus != null)
        {
            currentCustomerStatus.SetVisible(false);
        }

        currentCustomerStatus = status;
        if (currentCustomerStatus != null)
        {
            currentCustomerStatus.SetVisible(true);
        }
    }
}
