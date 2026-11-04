using TMPro;
using UnityEngine;

public sealed class CustomerWorldStatus : MonoBehaviour
{
    [SerializeField] private Customer customer;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private bool faceCamera = true;

    private void Awake()
    {
        if (customer == null)
        {
            customer = GetComponentInParent<Customer>();
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
                + "\nSatisfaction: " + Mathf.RoundToInt(customer.Satisfaction) + "%";
        }

        if (faceCamera && Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }
    }
}
