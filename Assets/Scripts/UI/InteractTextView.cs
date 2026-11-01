using TMPro;
using UnityEngine;

public sealed class InteractTextView : View
{
    [SerializeField] private TMP_Text text;

    public override bool BlocksGameplayInput { get { return false; } }

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
