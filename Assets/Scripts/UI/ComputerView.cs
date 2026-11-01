using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ComputerView : View
{
    [SerializeField] private ArcadeMachineDefinition catalogItem;
    [SerializeField] private ArcadeMachineDefinition[] catalogItems;
    [SerializeField] private DeliveryManager deliveryManager;
    [SerializeField] private Transform catalogContent;
    [SerializeField] private TMP_Text itemText;
    [SerializeField] private TMP_Text cashText;
    [SerializeField] private TMP_Text feedbackText;

    public override void OnShow()
    {
        Refresh();
    }

    public void PurchaseCatalogItem()
    {
        PurchaseCatalogItem(0);
    }

    public void PurchaseCatalogItem(int catalogIndex)
    {
        ArcadeMachineDefinition[] items = GetCatalogItems();
        if (catalogIndex < 0 || catalogIndex >= items.Length || items[catalogIndex] == null
            || deliveryManager == null)
        {
            SetFeedback("Catalog chưa được cấu hình.");
            return;
        }

        if (!deliveryManager.CreateOrder(items[catalogIndex]))
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

    public void OpenInventory()
    {
        if (GameViewManager.Instance != null)
        {
            GameViewManager.Instance.ShowView<InventoryView>();
        }
    }

    private void Refresh()
    {
        ArcadeMachineDefinition[] items = GetCatalogItems();
        RefreshCatalogCards(items);

        if (itemText != null && catalogContent == null)
        {
            if (items.Length == 0)
            {
                itemText.text = "Chưa có sản phẩm";
            }
            else
            {
                itemText.text = string.Empty;
                for (int i = 0; i < items.Length; i++)
                {
                    itemText.text += (i + 1) + ". " + items[i].DisplayName + " - ¥"
                        + items[i].PurchasePrice.ToString("N0") + "\n";
                }
            }
        }

        if (cashText != null && EconomyManager.Instance != null)
        {
            cashText.text = "Cash: ¥" + EconomyManager.Instance.CurrentCash.ToString("N0");
        }
    }

    private void RefreshCatalogCards(ArcadeMachineDefinition[] items)
    {
        if (catalogContent == null)
        {
            return;
        }

        for (int i = catalogContent.childCount - 1; i >= 0; i--)
        {
            Transform child = catalogContent.GetChild(i);
            if (child.GetComponent<CatalogItemCard>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        GridLayoutGroup grid = catalogContent.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = catalogContent.gameObject.AddComponent<GridLayoutGroup>();
        }

        grid.cellSize = new Vector2(210f, 280f);
        grid.spacing = new Vector2(16f, 16f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.childAlignment = TextAnchor.UpperLeft;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                continue;
            }

            GameObject cardObject = new GameObject("CatalogCard", typeof(RectTransform));
            cardObject.transform.SetParent(catalogContent, false);
            CatalogItemCard card = cardObject.AddComponent<CatalogItemCard>();
            int catalogIndex = i;
            card.Build(items[i], () => PurchaseCatalogItem(catalogIndex));
        }
    }

    private ArcadeMachineDefinition[] GetCatalogItems()
    {
        if (catalogItems != null && catalogItems.Length > 0)
        {
            return catalogItems;
        }

        return catalogItem == null ? new ArcadeMachineDefinition[0] : new[] { catalogItem };
    }

    private void SetFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }
}
