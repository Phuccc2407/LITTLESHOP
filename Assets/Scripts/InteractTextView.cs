using TMPro;
using UnityEngine;

public sealed class InteractTextView : View
{
    [SerializeField] private TMP_Text text;

    public void SetText(string content)
    {
        if (text != null)
        {
            text.text = content ?? string.Empty;
        }
    }

    public override void OnHide()
    {
        SetText(string.Empty);
    }
}
