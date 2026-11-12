using System.Collections.Generic;
using UnityEngine;

public sealed class GameViewManager : MonoBehaviour
{
    public static GameViewManager Instance { get; private set; }

    [SerializeField] private List<View> allViews = new List<View>();
    [SerializeField] private View defaultView;

    private bool viewBlocksInput;
    private bool externalInputBlocked;
    public bool IsBlockingViewOpen { get { return viewBlocksInput || externalInputBlocked; } }

    public void SetExternalInputBlocked(bool blocked)
    {
        externalInputBlocked = blocked;
        RefreshBlockingState();
    }

    public void CloseBlockingViews()
    {
        foreach (View view in allViews)
            if (view != null && view.IsVisible && view.BlocksGameplayInput)
            {
                view.SetVisible(false);
                view.OnHide();
            }
        RefreshBlockingState();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        HideAll();
        if (defaultView != null)
        {
            ShowView(defaultView);
        }
    }

    public T ShowView<T>(bool hideOthers = true) where T : View
    {
        T view = FindView<T>();
        if (view == null)
        {
            Debug.LogError("View is not registered: " + typeof(T).Name, this);
            return null;
        }

        ShowView(view, hideOthers);
        return view;
    }

    public void HideView<T>() where T : View
    {
        T view = FindView<T>();
        if (view != null)
        {
            view.SetVisible(false);
            view.OnHide();
        }

        RefreshBlockingState();
    }

    public void ShowView(View view, bool hideOthers = true)
    {
        if (view == null)
        {
            return;
        }

        if (hideOthers)
        {
            HideAll();
        }

        view.SetVisible(true);
        view.OnShow();
        if (view.BlocksGameplayInput) ShopAudio.Play(ShopSound.Click);
        RefreshBlockingState();
    }

    private T FindView<T>() where T : View
    {
        for (int i = 0; i < allViews.Count; i++)
        {
            T view = allViews[i] as T;
            if (view != null)
            {
                return view;
            }
        }

        return null;
    }

    private void HideAll()
    {
        for (int i = 0; i < allViews.Count; i++)
        {
            if (allViews[i] != null)
            {
                allViews[i].SetVisible(false);
                allViews[i].OnHide();
            }
        }
    }

    private void RefreshBlockingState()
    {
        viewBlocksInput = false;
        for (int i = 0; i < allViews.Count; i++)
        {
            if (allViews[i] != null && allViews[i].IsVisible && allViews[i].BlocksGameplayInput)
            {
                viewBlocksInput = true;
                break;
            }
        }

        Cursor.visible = IsBlockingViewOpen;
        Cursor.lockState = IsBlockingViewOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
