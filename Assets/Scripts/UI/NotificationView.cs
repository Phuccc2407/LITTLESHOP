using TMPro;
using UnityEngine;

public sealed class NotificationView : View
{
    [SerializeField] private TMP_Text messageText;

    public void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }

        if (GameViewManager.Instance != null)
        {
            GameViewManager.Instance.ShowView(this);
        }
    }
}
