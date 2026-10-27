using TMPro;
using UnityEngine;

public sealed class ComputerView : View
{
    [SerializeField] private ItemDefinition catalogItem;
    [SerializeField] private DeliveryManager deliveryManager;
    [SerializeField] private TMP_Text itemText;
    [SerializeField] private TMP_Text cashText;
    [SerializeField] private TMP_Text feedbackText;

    public override void OnShow()
    {
        Refresh();
    }

    public void PurchaseCatalogItem()
    {
        if (catalogItem == null || deliveryManager == null)
        {
            SetFeedback("Catalog chưa được cấu hình.");
            return;
        }

        if (!deliveryManager.CreateOrder(catalogItem))
        {
            SetFeedback("Không đủ tiền hoặc hệ thống giao hàng chưa sẵn sàng.");
            return;
        }

        SetFeedback("Đã đặt hàng. Hàng sẽ được giao tại khu nhận hàng.");
        Refresh();
    }

    public void Close()
    {
        if (GameViewManager.Instance != null)
        {
            GameViewManager.Instance.HideView<ComputerView>();
        }
    }

    private void Refresh()
    {
        if (itemText != null)
        {
            itemText.text = catalogItem == null
                ? "Chưa có sản phẩm"
                : catalogItem.DisplayName + "\n¥" + catalogItem.PurchasePrice.ToString("N0");
        }

        if (cashText != null && EconomyManager.Instance != null)
        {
            cashText.text = "Cash: ¥" + EconomyManager.Instance.CurrentCash.ToString("N0");
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
