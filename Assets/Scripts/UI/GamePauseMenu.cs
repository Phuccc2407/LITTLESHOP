using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class GamePauseMenu : MonoBehaviour
{
    private GameObject screen;
    private TMP_Text message;
    private bool paused;
    private float previousTimeScale = 1f;

    private void Start()
    {
        Canvas canvas = ShopMenuUI.Canvas("PauseMenuCanvas");
        screen = canvas.gameObject;
        RectTransform shade = ShopMenuUI.Panel("Shade", canvas.transform, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.8f));
        ShopMenuUI.Stretch(shade);
        RectTransform card = ShopMenuUI.Panel("Pause", canvas.transform, Vector2.zero, new Vector2(600f, 700f), ShopMenuUI.Card);
        ShopMenuUI.Text(card, "TAKE A BREAK", new Vector2(0f, 260f), new Vector2(460f, 65f), 34f, ShopMenuUI.Accent);
        ShopMenuUI.Button(card, "Resume", new Vector2(0f, 155f), new Vector2(460f, 65f), Resume, true);
        ShopMenuUI.Button(card, "Save Game", new Vector2(0f, 65f), new Vector2(460f, 65f), Save);
        ShopMenuUI.Button(card, "Save & Main Menu", new Vector2(0f, -25f), new Vector2(460f, 65f), ReturnToMenu);
        ShopMenuUI.Button(card, "Save & Quit", new Vector2(0f, -115f), new Vector2(460f, 65f), Quit);
        message = ShopMenuUI.Text(card, "Autosave every 60 seconds.\nF5: quick save", new Vector2(0f, -240f), new Vector2(460f, 110f), 22f);
        screen.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null || screen == null || GameSession.IsRestoring) return;
        if (Keyboard.current.f5Key.wasPressedThisFrame) Save();
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;
        if (paused) { Resume(); return; }
        PlacementSystem placement = FindAnyObjectByType<PlacementSystem>();
        if (placement != null && placement.IsPlacing) return;
        if (GameViewManager.Instance != null && GameViewManager.Instance.IsBlockingViewOpen)
        {
            GameViewManager.Instance.CloseBlockingViews();
            return;
        }
        paused = true;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        if (GameViewManager.Instance != null) GameViewManager.Instance.SetExternalInputBlocked(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        screen.SetActive(true);
    }

    private void Resume()
    {
        paused = false;
        Time.timeScale = previousTimeScale;
        screen.SetActive(false);
        if (GameViewManager.Instance != null) GameViewManager.Instance.SetExternalInputBlocked(false);
    }
    private void Save()
    {
        GameSession.Instance.SaveGame();
        message.text = GameSession.Instance.LastMessage;
    }
    private void ReturnToMenu()
    {
        if (!GameSession.Instance.ReturnToMenu()) message.text = GameSession.Instance.LastMessage;
    }
    private void Quit()
    {
        if (GameSession.Instance.SaveGame()) Application.Quit();
        else message.text = GameSession.Instance.LastMessage;
    }
}
