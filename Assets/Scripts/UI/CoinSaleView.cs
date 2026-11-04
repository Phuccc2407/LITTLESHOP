using TMPro;
using UnityEngine;

public sealed class CoinSaleView : View
{
    [SerializeField] private TMP_Text customerText;
    [SerializeField] private TMP_InputField coinInput;
    [SerializeField] private TMP_Text feedbackText;

    private CoinCounter counter;
    private Customer customer;

    public void Open(CoinCounter targetCounter, Customer targetCustomer)
    {
        counter = targetCounter;
        customer = targetCustomer;
        if (coinInput != null)
        {
            coinInput.text = customer == null ? string.Empty : customer.RequiredCoins.ToString();
        }

        Refresh();
    }

    public void SellCoins()
    {
        if (counter == null || customer == null || coinInput == null
            || !int.TryParse(coinInput.text, out int amount) || amount <= 0)
        {
            SetFeedback("Nhập số coin hợp lệ.");
            return;
        }

        if (amount < customer.RequiredCoins)
        {
            SetFeedback("Khách cần ít nhất " + customer.RequiredCoins + " coin.");
            return;
        }

        if (!counter.SellToNextCustomer(amount))
        {
            SetFeedback("Không thể hoàn tất giao dịch.");
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
        if (customerText != null)
        {
            customerText.text = customer == null
                ? "No customer"
                : "Coin cần bán: " + customer.RequiredCoins
                    + "\nSatisfaction: " + Mathf.RoundToInt(customer.Satisfaction) + "%";
        }

        SetFeedback(string.Empty);
    }

    private void SetFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }
}
