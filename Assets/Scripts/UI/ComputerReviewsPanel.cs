using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Built under ComputerView at runtime; works with existing scenes and prefabs.
public sealed class ComputerReviewsPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button catalogButton;
    [SerializeField] private Button reviewsButton;
    private bool bound;

    private void OnEnable() { CustomerReviews.Changed += Refresh; }
    private void OnDisable() { CustomerReviews.Changed -= Refresh; }

    public void Build(Transform parent)
    {
        if (panel != null) { BindButtons(); Refresh(); return; }
        GameObject tabs = Rect("CustomerTabs", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(400f, 48f));
        RectTransform tabRect = (RectTransform)tabs.transform;
        tabRect.pivot = new Vector2(0f, 1f);
        tabRect.anchoredPosition = new Vector2(20f, -65f);
        HorizontalLayoutGroup layout = tabs.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        catalogButton = MakeButton(tabs.transform, "Catalog");
        reviewsButton = MakeButton(tabs.transform, "Customer Reviews");

        panel = Rect("ReviewsPanel", parent, Vector2.zero, Vector2.one, Vector2.zero);
        RectTransform panelRect = (RectTransform)panel.transform;
        panelRect.offsetMin = new Vector2(20f, 65f);
        panelRect.offsetMax = new Vector2(-20f, -115f);
        panel.AddComponent<Image>().color = new Color(0.08f, 0.1f, 0.15f, 1f);

        GameObject viewport = Rect("Viewport", panel.transform, Vector2.zero, Vector2.one, Vector2.zero);
        ((RectTransform)viewport.transform).offsetMin = new Vector2(20f, 12f);
        ((RectTransform)viewport.transform).offsetMax = new Vector2(-20f, -12f);
        viewport.AddComponent<RectMask2D>();
        GameObject content = Rect("ReviewText", viewport.transform, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, 600f));
        RectTransform contentRect = (RectTransform)content.transform;
        contentRect.pivot = new Vector2(0.5f, 1f);
        text = content.AddComponent<TextMeshProUGUI>();
        text.fontSize = 21f;
        text.color = Color.white;
        text.richText = false;
        text.raycastTarget = false;
        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        ScrollRect scroll = panel.AddComponent<ScrollRect>();
        scroll.viewport = (RectTransform)viewport.transform;
        scroll.content = contentRect;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        panel.SetActive(false);
        BindButtons();
    }

    private void BindButtons()
    {
        if (bound || !Application.isPlaying) return;
        if (panel != null && text != null && panel.GetComponent<ScrollRect>() == null)
        {
            RectTransform viewport = (RectTransform)text.transform.parent;
            if (viewport.GetComponent<RectMask2D>() == null) viewport.gameObject.AddComponent<RectMask2D>();
            if (text.GetComponent<ContentSizeFitter>() == null)
                text.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            ScrollRect scroll = panel.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = text.rectTransform;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
        }
        bound = true;
        catalogButton.onClick.AddListener(() => { panel.SetActive(false); ShopAudio.Play(ShopSound.Click); });
        reviewsButton.onClick.AddListener(() => { panel.SetActive(true); Refresh(); ShopAudio.Play(ShopSound.Click); });
    }

    private void Refresh()
    {
        if (text == null) return;
        var reviews = CustomerReviews.All;
        StringBuilder builder = new StringBuilder();
        builder.Append("CUSTOMER REVIEWS     ").Append(CustomerReviews.AverageStars.ToString("0.0"))
            .Append(" / 5     (").Append(reviews.Count).AppendLine(" reviews)");
        builder.AppendLine("Service, prices and machine condition affect your reputation.\n");
        if (reviews.Count == 0) builder.AppendLine("No reviews yet. Open the shop and serve your first visitors!");
        foreach (CustomerReview review in reviews)
            builder.Append(review.Visitor).Append(" | ").Append(review.Stars).Append("/5 | ")
                .Append(review.Plays).AppendLine(" games played").AppendLine(review.Comment).AppendLine();
        text.text = builder.ToString();
    }

    private static GameObject Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 size)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)obj.transform;
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
        return obj;
    }

    private static Button MakeButton(Transform parent, string label)
    {
        GameObject obj = Rect(label, parent, Vector2.zero, Vector2.one, Vector2.zero);
        Image image = obj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.35f, 0.5f);
        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;
        LayoutElement element = obj.AddComponent<LayoutElement>();
        element.preferredWidth = 180f;
        element.preferredHeight = 42f;
        GameObject caption = Rect("Label", obj.transform, Vector2.zero, Vector2.one, Vector2.zero);
        TMP_Text labelText = caption.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 20f;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.raycastTarget = false;
        return button;
    }
}
