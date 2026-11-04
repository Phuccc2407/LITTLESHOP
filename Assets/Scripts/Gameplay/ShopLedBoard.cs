using UnityEngine;

public sealed class ShopLedBoard : MonoBehaviour, IInteractable
{
    [SerializeField] private ShopDayCycle shopDayCycle;
    [SerializeField] private Renderer[] ledRenderers;
    [SerializeField] private Color emissionColor = Color.white;
    [SerializeField, Min(0f)] private float openEmissionIntensity = 4f;
    [SerializeField, Min(0f)] private float closedEmissionIntensity;

    private Material[] ledMaterials;

    public string InteractionPrompt
    {
        get
        {
            return shopDayCycle != null && shopDayCycle.State == ShopDayState.Open
                ? "E - Close shop"
                : shopDayCycle != null && shopDayCycle.CanOpenShop
                    ? "E - Open shop"
                    : "Shop closed until opening time";
        }
    }

    private void Awake()
    {
        if (shopDayCycle == null)
        {
            shopDayCycle = GetComponentInParent<ShopDayCycle>();
        }

        if (ledRenderers == null || ledRenderers.Length == 0)
        {
            ledRenderers = GetComponentsInChildren<Renderer>(true);
        }

        ledMaterials = new Material[ledRenderers.Length];
        for (int i = 0; i < ledRenderers.Length; i++)
        {
            if (ledRenderers[i] == null)
            {
                continue;
            }

            ledMaterials[i] = ledRenderers[i].material;
            ledMaterials[i].EnableKeyword("_EMISSION");
        }
    }

    private void OnEnable()
    {
        if (shopDayCycle != null)
        {
            shopDayCycle.StateChanged += OnShopStateChanged;
        }
    }

    private void Start()
    {
        if (shopDayCycle == null)
        {
            Debug.LogError("ShopLedBoard requires a ShopDayCycle reference.", this);
            return;
        }

        ApplyEmission(shopDayCycle.State);
    }

    private void OnDisable()
    {
        if (shopDayCycle != null)
        {
            shopDayCycle.StateChanged -= OnShopStateChanged;
        }
    }

    public bool CanInteract(PlayerInteractor interactor)
    {
        return shopDayCycle != null
            && (shopDayCycle.State == ShopDayState.Open || shopDayCycle.CanOpenShop);
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (shopDayCycle.State == ShopDayState.Open)
        {
            shopDayCycle.CloseShop();
        }
        else
        {
            shopDayCycle.OpenShop();
        }
    }

    private void OnShopStateChanged(ShopDayState state)
    {
        ApplyEmission(state);
    }

    private void ApplyEmission(ShopDayState state)
    {
        float intensity = state == ShopDayState.Open
            ? openEmissionIntensity
            : closedEmissionIntensity;
        Color targetColor = emissionColor * intensity;

        if (ledMaterials == null)
        {
            return;
        }

        for (int i = 0; i < ledMaterials.Length; i++)
        {
            if (ledMaterials[i] != null)
            {
                ledMaterials[i].SetColor("_EmissionColor", targetColor);
            }
        }
    }
}
