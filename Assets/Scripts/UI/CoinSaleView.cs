using TMPro;
using UnityEngine;

public sealed class CoinSaleView : View
{
    [SerializeField] private TMP_Text customerText;
    [SerializeField] private TMP_InputField coinInput;
    [SerializeField] private TMP_Text feedbackText;

    private CoinCounter counter;
    private Customer customer;

    private void Update()
    {
        if (!IsVisible || counter == null) return;
        if (customer == null || !customer.IsWaitingForCoins)
        {
            Close();
            return;
        }
        UpdateCustomerText();
    }

    public void Open(CoinCounter targetCounter, Customer targetCustomer)
    {
        counter = targetCounter;
        customer = targetCustomer;
        if (coinInput != null)
        {
            coinInput.text = string.Empty;
            coinInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            coinInput.ActivateInputField();
        }

        Refresh();
    }

    public void SellCoins()
    {
        if (counter == null || customer == null || coinInput == null
            || !int.TryParse(coinInput.text, out int amount) || amount <= 0)
        {
            SetFeedback("Enter a valid coin amount.");
            return;
        }

        if (amount != customer.RequiredCoins)
        {
            SetFeedback("This customer wants exactly " + customer.RequiredCoins + " coins.");
            return;
        }

        if (!counter.SellToNextCustomer(amount, customer))
        {
            SetFeedback("The customer has left or the queue has changed. Reopen the counter.");
            return;
        }

        Close();
    }

    public void Close()
    {
        counter = null;
        customer = null;
        if (GameViewManager.Instance != null)
        {
            GameViewManager.Instance.HideView<CoinSaleView>();
        }
    }

    private void Refresh()
    {
        UpdateCustomerText();
        SetFeedback(string.Empty);
    }

    private void UpdateCustomerText()
    {
        if (customerText != null)
        {
            customerText.text = customer == null
                ? "No customer"
                : "Requested coins: " + customer.RequiredCoins
                    + "\nSatisfaction: " + Mathf.RoundToInt(customer.Satisfaction) + "%";
        }

    }

    private void SetFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }
}
