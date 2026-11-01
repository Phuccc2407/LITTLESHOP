using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CatalogItemCard : MonoBehaviour
{
    private Action purchaseAction;

    public void Build(ArcadeMachineDefinition machine, Action onPurchase)
    {
        purchaseAction = onPurchase;

        Image background = gameObject.AddComponent<Image>();
        background.color = new Color(0.12f, 0.14f, 0.18f, 1f);

        VerticalLayoutGroup layout = gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;

        LayoutElement cardSize = gameObject.AddComponent<LayoutElement>();
        cardSize.minWidth = 210f;
        cardSize.minHeight = 280f;
        cardSize.preferredWidth = 210f;
        cardSize.preferredHeight = 280f;

        CreateIcon(machine);
        CreateText(machine.DisplayName, 22f);
        CreateText("¥" + machine.PurchasePrice.ToString("N0"), 18f);
        CreatePurchaseButton();
    }

    private void CreateIcon(ArcadeMachineDefinition machine)
    {
        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(transform, false);

        Image icon = iconObject.GetComponent<Image>();
        icon.sprite = machine.Icon;
        icon.color = machine.Icon == null ? new Color(0.25f, 0.28f, 0.34f, 1f) : Color.white;
        icon.preserveAspect = true;

        LayoutElement size = iconObject.AddComponent<LayoutElement>();
        size.minHeight = 150f;
        size.preferredHeight = 150f;
    }

    private void CreateText(string value, float fontSize)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(transform, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.color = Color.white;
    }

    private void CreatePurchaseButton()
    {
        GameObject buttonObject = new GameObject("BuyButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(transform, false);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.18f, 0.65f, 0.35f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(() => purchaseAction?.Invoke());

        LayoutElement size = buttonObject.AddComponent<LayoutElement>();
        size.minHeight = 42f;
        size.preferredHeight = 42f;

        CreateButtonText(buttonObject.transform);
    }

    private static void CreateButtonText(Transform parent)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = "Mua";
        text.fontSize = 18f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
    }
}
