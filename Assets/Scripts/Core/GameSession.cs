using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameSession : MonoBehaviour
{
    public const string GameplayScene = "Assets/PolygonShops/Scenes/Demo_01.unity";
    public const string MainMenuScene = "Assets/Scenes/MainMenu.unity";
    public static GameSession Instance { get; private set; }
    public static bool IsRestoring { get { return Instance != null && Instance.restoring; } }
    public static string SavePath { get { return Path.Combine(Application.persistentDataPath, "shop-save-v1.json"); } }
    public bool IsLoading { get; private set; }
    public string LastMessage { get; private set; }
    private bool restoring = true;
    private bool ready;
    private bool newGame;
    private bool recoveredBackup;
    private ShopSaveData pending;
    private float nextAutosave;
    private ShopItemCatalog catalog;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession() { Instance = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null) new GameObject("GameSession").AddComponent<GameSession>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.path == GameplayScene) StartCoroutine(InitializeGameplay());
        else { ready = false; restoring = false; IsLoading = false; }
    }

    private IEnumerator InitializeGameplay()
    {
        restoring = true;
        ready = false;
        // All scene Awake/Start methods finish before restoring the saved state.
        yield return null;
        try
        {
            catalog = Resources.Load<ShopItemCatalog>("ShopItemCatalog");
            if (catalog == null) throw new InvalidOperationException("The save item catalog is missing.");
            ShopSaveData data = pending;
            if (!newGame && data == null && (File.Exists(SavePath) || File.Exists(SavePath + ".bak")))
            {
                if (!TryReadSave(out data, out string error)) throw new InvalidOperationException(error);
            }
            if (data != null) { Validate(data); Restore(data); }
            else CustomerReviews.RestoreProgress(new List<CustomerReview>());
            ready = true;
        }
        catch (Exception error)
        {
            LastMessage = "Could not load progress: " + error.Message;
            Debug.LogError(LastMessage);
        }
        pending = null;
        newGame = false;
        restoring = false;
        IsLoading = false;
        if (!ready)
        {
            SceneManager.LoadScene(MainMenuScene);
            yield break;
        }
        if (FindAnyObjectByType<GamePauseMenu>() == null) new GameObject("GamePauseMenu").AddComponent<GamePauseMenu>();
        nextAutosave = Time.unscaledTime + 60f;
        // Keep a recovered backup intact until a valid replacement has been written.
        SaveGame();
    }

    public bool StartNewGame()
    {
        if (IsLoading) return false;
        pending = null;
        newGame = true;
        StartCoroutine(LoadGameplay());
        return true;
    }

    public bool ContinueGame()
    {
        if (IsLoading) { LastMessage = "Please wait for loading to finish."; return false; }
        if (!TryReadSave(out ShopSaveData data, out string error))
        {
            LastMessage = error;
            return false;
        }
        catalog = Resources.Load<ShopItemCatalog>("ShopItemCatalog");
        try { if (catalog == null) throw new InvalidOperationException("The save item catalog is missing."); Validate(data); }
        catch (Exception exception) { LastMessage = "Cannot continue: " + exception.Message; return false; }
        pending = data;
        newGame = false;
        StartCoroutine(LoadGameplay());
        return true;
    }

    private IEnumerator LoadGameplay()
    {
        IsLoading = restoring = true;
        ready = false;
        Time.timeScale = 1f;
        AsyncOperation operation = SceneManager.LoadSceneAsync(GameplayScene);
        while (!operation.isDone) yield return null;
    }

    public bool ReturnToMenu()
    {
        if (!SaveGame()) return false;
        ready = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(MainMenuScene);
        return true;
    }

    public static bool TryReadSave(out ShopSaveData data, out string message)
    {
        data = null;
        message = "No saved game found.";
        foreach (string path in new[] { SavePath, SavePath + ".bak" })
        {
            if (!File.Exists(path)) continue;
            try
            {
                ShopSaveData candidate = JsonUtility.FromJson<ShopSaveData>(SaveFileStore.Read(path));
                if (candidate == null || string.IsNullOrEmpty(candidate.SavedAtUtc) || candidate.Format != "LittleShop.Save" || candidate.Version != 1
                    || candidate.DayNumber < 1 || candidate.DayNumber > 2900000 || candidate.Machines == null
                    || candidate.Inventory == null || candidate.Boxes == null || candidate.Orders == null || candidate.Reviews == null)
                    throw new InvalidDataException("The save format is invalid or unsupported.");
                data = candidate;
                bool backup = path.EndsWith(".bak", StringComparison.Ordinal);
                if (Instance != null) Instance.recoveredBackup = backup;
                message = backup ? "Recovered the previous backup save." : "Saved game found.";
                return true;
            }
            catch (Exception error) { message = "Cannot read saved progress: " + error.Message; }
        }
        return false;
    }

    public bool SaveGame()
    {
        if (!ready || restoring || SceneManager.GetActiveScene().path != GameplayScene) return false;
        PlacementSystem placement = FindAnyObjectByType<PlacementSystem>();
        if (placement != null && placement.IsPlacing)
        {
            LastMessage = "Finish or cancel placement before saving.";
            return false;
        }
        try
        {
            ShopSaveData data = Capture();
            Validate(data);
            // Do not rotate a known corrupt primary file over the recovered backup.
            if (recoveredBackup && File.Exists(SavePath))
            {
                File.Move(SavePath, SavePath + ".corrupt-" + DateTime.UtcNow.Ticks);
            }
            SaveFileStore.Write(SavePath, JsonUtility.ToJson(data, true));
            recoveredBackup = false;
            LastMessage = "Game saved.";
            nextAutosave = Time.unscaledTime + 60f;
            return true;
        }
        catch (Exception error)
        {
            LastMessage = "Could not save progress: " + error.Message;
            Debug.LogWarning(LastMessage);
            return false;
        }
    }

    private ShopSaveData Capture()
    {
        EconomyManager economy = EconomyManager.Instance;
        ShopDayCycle clock = FindAnyObjectByType<ShopDayCycle>();
        if (economy == null || clock == null) throw new InvalidOperationException("Shop managers are missing.");
        ShopSaveData data = new ShopSaveData
        {
            SavedAtUtc = DateTime.UtcNow.ToString("O"), Cash = economy.CurrentCash,
            Revenue = economy.Revenue, Expenses = economy.Expenses,
            DailyRevenue = economy.DailyRevenue, DailyExpenses = economy.DailyExpenses,
            DayNumber = clock.DayNumber, TimeMinutes = clock.CurrentTimeMinutes, ShopOpen = clock.State == ShopDayState.Open
        };
        if (InventoryManager.Instance != null) data.Inventory = InventoryManager.Instance.CaptureProgress();
        foreach (ArcadeMachine machine in FindObjectsByType<ArcadeMachine>())
        {
            if (machine.IsPlacementPreview) continue;
            if (machine.Definition == null) throw new InvalidOperationException("A placed machine has no definition: " + machine.name);
            data.Machines.Add(new SavedMachine
            {
                ItemId = machine.Definition.ItemId, Position = machine.transform.position, Rotation = machine.transform.rotation,
                Scale = machine.transform.lossyScale, Condition = machine.Condition, CoinsPerPlay = machine.CoinsPerPlay,
                Active = machine.IsPoweredOn, Revenue = machine.Revenue
            });
        }
        foreach (DeliveryBox box in FindObjectsByType<DeliveryBox>())
            if (box.Contents != null)
                data.Boxes.Add(new SavedBox { ItemId = box.Contents.ItemId, Position = box.transform.position, Rotation = box.transform.rotation });
        DeliveryManager delivery = FindAnyObjectByType<DeliveryManager>();
        if (delivery != null) data.Orders = delivery.CaptureProgress();
        CustomerManager customers = FindAnyObjectByType<CustomerManager>();
        if (customers != null) { data.CustomersSpawned = customers.CustomersSpawnedToday; data.CustomersServed = customers.CustomersServedToday; }
        foreach (CustomerReview review in CustomerReviews.All) data.Reviews.Add(review);
        CharacterController player = FindAnyObjectByType<CharacterController>();
        if (player != null)
        {
            data.HasPlayer = true; data.PlayerPosition = player.transform.position;
            data.PlayerRotation = player.transform.rotation; data.CameraPitch = player.CameraPitch;
        }
        return data;
    }

    private void Validate(ShopSaveData data)
    {
        if (data.Cash < 0 || data.Revenue < 0 || data.Expenses < 0 || data.DailyRevenue < 0 || data.DailyExpenses < 0)
            throw new InvalidDataException("Invalid saved finances.");
        if (data.DayNumber < 1 || data.DayNumber > 2900000 || !Finite(data.TimeMinutes) || data.TimeMinutes < 0f || data.TimeMinutes >= 1440f)
            throw new InvalidDataException("Invalid saved date or time.");
        foreach (SavedMachine machine in data.Machines)
        {
            ItemDefinition definition = catalog.Find(machine.ItemId);
            if (!(definition is ArcadeMachineDefinition) || definition.Prefab == null || definition.Prefab.GetComponent<ArcadeMachine>() == null)
                throw new InvalidDataException("Missing arcade prefab: " + machine.ItemId);
            if (machine.Condition < 0 || machine.Condition > 100 || machine.CoinsPerPlay < 1 || machine.Revenue < 0)
                throw new InvalidDataException("Invalid machine settings.");
            if (!Finite(machine.Position) || !Finite(machine.Scale) || !Finite(machine.Rotation)
                || machine.Scale.x <= 0f || machine.Scale.y <= 0f || machine.Scale.z <= 0f)
                throw new InvalidDataException("Invalid machine transform.");
        }
        foreach (SavedItemStack item in data.Inventory)
            if (catalog.Find(item.ItemId) == null || item.Quantity < 1) throw new InvalidDataException("Missing inventory item: " + item.ItemId);
        foreach (SavedBox box in data.Boxes)
            if (catalog.Find(box.ItemId) == null || !Finite(box.Position) || !Finite(box.Rotation)) throw new InvalidDataException("Invalid delivery box.");
        foreach (SavedOrder order in data.Orders)
            if (catalog.Find(order.ItemId) == null || !Finite(order.RemainingSeconds) || order.RemainingSeconds < 0f) throw new InvalidDataException("Invalid delivery order.");
        if (data.HasPlayer && (!Finite(data.PlayerPosition) || !Finite(data.PlayerRotation) || !Finite(data.CameraPitch)))
            throw new InvalidDataException("Invalid player position.");
    }

    private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    private static bool Finite(Vector3 value) { return Finite(value.x) && Finite(value.y) && Finite(value.z); }
    private static bool Finite(Quaternion value) { return Finite(value.x) && Finite(value.y) && Finite(value.z) && Finite(value.w) && Quaternion.Dot(value, value) > 0.01f; }

    private void Restore(ShopSaveData data)
    {
        foreach (Customer customer in FindObjectsByType<Customer>()) { customer.gameObject.SetActive(false); Destroy(customer.gameObject); }
        foreach (ArcadeMachine machine in FindObjectsByType<ArcadeMachine>()) { machine.gameObject.SetActive(false); Destroy(machine.gameObject); }
        foreach (DeliveryBox box in FindObjectsByType<DeliveryBox>()) { box.gameObject.SetActive(false); Destroy(box.gameObject); }
        foreach (SavedMachine saved in data.Machines)
        {
            ArcadeMachineDefinition definition = (ArcadeMachineDefinition)catalog.Find(saved.ItemId);
            GameObject root = Instantiate(definition.Prefab, saved.Position, saved.Rotation);
            root.transform.localScale = saved.Scale;
            ArcadeMachine machine = root.GetComponent<ArcadeMachine>();
            machine.Initialize(definition);
            machine.RestoreProgress(saved);
        }
        if (EconomyManager.Instance == null) throw new InvalidOperationException("EconomyManager is missing.");
        EconomyManager.Instance.RestoreProgress(data);
        if (InventoryManager.Instance != null) InventoryManager.Instance.RestoreProgress(data.Inventory, catalog);
        ShopDayCycle clock = FindAnyObjectByType<ShopDayCycle>();
        if (clock == null) throw new InvalidOperationException("ShopDayCycle is missing.");
        clock.RestoreProgress(data.DayNumber, data.TimeMinutes, data.ShopOpen);
        CustomerManager customers = FindAnyObjectByType<CustomerManager>();
        if (customers != null) customers.RestoreDailyStats(data.CustomersSpawned, data.CustomersServed);
        DeliveryManager delivery = FindAnyObjectByType<DeliveryManager>();
        if (delivery == null && (data.Orders.Count > 0 || data.Boxes.Count > 0)) throw new InvalidOperationException("DeliveryManager is missing.");
        if (delivery != null)
        {
            delivery.RestoreProgress(data.Orders, catalog);
            foreach (SavedBox box in data.Boxes) delivery.RestoreBox(catalog.Find(box.ItemId), box.Position, box.Rotation);
        }
        CustomerReviews.RestoreProgress(data.Reviews);
        CharacterController player = FindAnyObjectByType<CharacterController>();
        if (data.HasPlayer && player != null) player.RestorePose(data.PlayerPosition, data.PlayerRotation, data.CameraPitch);
    }

    private void Update()
    {
        if (!ready || Time.unscaledTime < nextAutosave) return;
        nextAutosave = Time.unscaledTime + 60f;
        SaveGame();
    }
    private void OnApplicationPause(bool paused) { if (paused) SaveGame(); }
    private void OnApplicationQuit()
    {
        PlacementSystem placement = FindAnyObjectByType<PlacementSystem>();
        if (ready && placement != null && placement.IsPlacing) placement.CancelActivePlacement();
        SaveGame();
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }
}
