using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// All UI objects and persistent button events are authored in MainMenu.unity.
public sealed class MainMenuController : MonoBehaviour
{
    [Header("Scene UI")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text feedback;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject confirmation;
    [SerializeField] private GameObject settings;
    [SerializeField] private Button continueButton;
    [SerializeField] private CanvasGroup buttons;
    [Header("Audio settings")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider effectsSlider;

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (menu == null || confirmation == null || settings == null || feedback == null || continueButton == null || buttons == null)
        {
            Debug.LogError("Assign the authored main menu UI in the Inspector.", this);
            enabled = false;
            return;
        }
        confirmation.SetActive(false);
        settings.SetActive(false);
        menu.SetActive(true);
        bool hasSave = GameSession.TryReadSave(out ShopSaveData data, out _);
        continueButton.interactable = hasSave;
        feedback.text = hasSave ? ShopDayCycle.StartDate.AddDays(data.DayNumber - 1)
            .ToString("dddd, dd/MM/yyyy", CultureInfo.InvariantCulture) + "  |  Cash: \u00A5" + data.Cash.ToString("N0")
            : "Start a new arcade. Your progress will save automatically.";
        if (GameSession.Instance != null && !string.IsNullOrEmpty(GameSession.Instance.LastMessage))
            feedback.text = GameSession.Instance.LastMessage;
        SyncSettings();
    }

    private void Update()
    {
        if (GameSession.Instance == null || !GameSession.Instance.IsLoading) return;
        buttons.interactable = false;
        feedback.text = "Loading your arcade...";
    }

    private static void Click() { ShopAudio.Play(ShopSound.Click); }
    public void ContinueGame()
    {
        Click();
        if (!GameSession.Instance.ContinueGame()) feedback.text = GameSession.Instance.LastMessage;
    }
    public void NewGame()
    {
        Click();
        if (GameSession.TryReadSave(out _, out _)) { menu.SetActive(false); confirmation.SetActive(true); }
        else StartNewGame();
    }
    public void StartNewGame()
    {
        confirmation.SetActive(false);
        settings.SetActive(false);
        menu.SetActive(true);
        GameSession.Instance.StartNewGame();
    }
    public void OpenSettings()
    {
        Click(); SyncSettings(); menu.SetActive(false); settings.SetActive(true);
    }
    public void Back()
    {
        Click(); PlayerPrefs.Save(); confirmation.SetActive(false); settings.SetActive(false); menu.SetActive(true);
    }
    public void Quit() { Click(); Application.Quit(); }
    private void SyncSettings()
    {
        AudioManager audio = AudioManager.Instance;
        if (audio == null) return;
        if (masterSlider != null) masterSlider.SetValueWithoutNotify(audio.MasterVolume);
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(audio.MusicVolume);
        if (effectsSlider != null) effectsSlider.SetValueWithoutNotify(audio.EffectsLevel);
    }
    public void SetMasterVolume(float value) { if (AudioManager.Instance != null) AudioManager.Instance.SetMasterVolume(value); }
    public void SetMusicVolume(float value) { if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(value); }
    public void SetEffectsVolume(float value) { if (AudioManager.Instance != null) AudioManager.Instance.SetEffectsVolume(value); }
}
