using TMPro;
using UnityEngine;

public sealed class CustomerWorldStatus : MonoBehaviour
{
    [SerializeField] private Customer customer;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private bool faceCamera = true;

    private Canvas worldCanvas;
    private Transform billboardTransform;

    public void SetVisible(bool visible)
    {
        if (worldCanvas != null)
        {
            worldCanvas.enabled = visible;
        }
    }

    private void Awake()
    {
        if (customer == null)
        {
            customer = GetComponentInParent<Customer>();
        }

        worldCanvas = GetComponentInParent<Canvas>();
        if (worldCanvas != null)
        {
            worldCanvas.overrideSorting = true;
            worldCanvas.sortingOrder = 10;
            worldCanvas.enabled = false;
            billboardTransform = worldCanvas.transform;
        }

        if (statusText != null)
        {
            statusText.raycastTarget = false;
        }
    }

    private void LateUpdate()
    {
        if (customer == null)
        {
            return;
        }

        if (statusText != null)
        {
            statusText.text = "Coin: " + customer.RequiredCoins
                + "\n" + customer.Activity + " | Patience: " + Mathf.CeilToInt(customer.PatienceRemaining) + "s"
                + "\nSatisfaction: " + Mathf.RoundToInt(customer.Satisfaction) + "%";
        }

        if (faceCamera && Camera.main != null)
        {
            Transform target = billboardTransform == null ? transform : billboardTransform;
            Vector3 directionToCamera = Camera.main.transform.position - target.position;
            if (directionToCamera.sqrMagnitude > 0.001f)
            {
                target.rotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
            }
        }
    }
}
