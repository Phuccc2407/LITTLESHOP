using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class ShopMenuUI
{
    public static readonly Color Background = new Color(0.025f, 0.045f, 0.075f);
    public static readonly Color Card = new Color(0.06f, 0.095f, 0.14f);
    public static readonly Color Accent = new Color(0.32f, 0.95f, 0.73f);

    public static Canvas Canvas(string name)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject events = new GameObject("MenuEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
        return canvas;
    }

    public static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    public static RectTransform Panel(string name, Transform parent, Vector2 position, Vector2 size, Color color)
    {
        RectTransform rect = Rect(name, parent, position, size);
        rect.gameObject.AddComponent<Image>().color = color;
        return rect;
    }

    public static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    public static TMP_Text Text(Transform parent, string content, Vector2 position, Vector2 size, float fontSize = 24f, Color? color = null)
    {
        RectTransform rect = Rect("Text", parent, position, size);
        TMP_Text text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = color ?? Color.white;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.raycastTarget = false;
        text.richText = false;
        return text;
    }

    public static Button Button(Transform parent, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action, bool primary = false)
    {
        RectTransform rect = Panel(label, parent, position, size, primary ? Accent : new Color(0.12f, 0.18f, 0.25f));
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();
        button.onClick.AddListener(() => { ShopAudio.Play(ShopSound.Click); action(); });
        TMP_Text caption = Text(rect, label, Vector2.zero, size, 24f, primary ? Background : Color.white);
        caption.alignment = TextAlignmentOptions.Center;
        return button;
    }

    public static void VolumeSlider(Transform parent, string label, float y, float initial, UnityEngine.Events.UnityAction<float> changed)
    {
        Text(parent, label, new Vector2(0f, y + 30f), new Vector2(420f, 35f), 23f);
        RectTransform track = Panel(label + " volume", parent, new Vector2(0f, y), new Vector2(420f, 12f), new Color(0.15f, 0.22f, 0.3f));
        Slider slider = track.gameObject.AddComponent<Slider>();
        RectTransform fill = Panel("Fill", track, Vector2.zero, Vector2.zero, Accent);
        Stretch(fill);
        RectTransform handleArea = Rect("Handle Area", track, Vector2.zero, new Vector2(-20f, 24f));
        handleArea.anchorMin = Vector2.zero; handleArea.anchorMax = Vector2.one;
        RectTransform handle = Panel("Handle", handleArea, Vector2.zero, new Vector2(22f, 26f), Color.white);
        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.minValue = 0f; slider.maxValue = 1f; slider.value = initial;
        slider.onValueChanged.AddListener(changed);
    }
}
