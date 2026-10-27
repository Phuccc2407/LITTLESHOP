using UnityEngine;

public abstract class View : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    public bool IsVisible { get; private set; }

    protected virtual void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        SetVisible(false);
    }

    public virtual void OnShow()
    {
    }

    public virtual void OnHide()
    {
    }

    public void SetVisible(bool visible)
    {
        IsVisible = visible;
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }
}
