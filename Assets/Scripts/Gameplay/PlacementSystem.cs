using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlacementSystem : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask floorLayers;
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField, Min(0.01f)] private float gridSize = 0.5f;
    [SerializeField] private float placementDistance = 5f;

    private ItemDefinition item;
    private GameObject preview;
    private Quaternion rotation;
    private bool hasValidSurface;
    private DeliveryBox sourceBox;
    private bool consumesInventory;
    private ArcadeMachine repositionedMachine;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private readonly Dictionary<Renderer, Color[]> originalRendererColors =
        new Dictionary<Renderer, Color[]>();

    public bool IsPlacing { get { return item != null; } }
    public void CancelActivePlacement() { if (IsPlacing) CancelPlacement(); }

    private void Update()
    {
        if (!IsPlacing || playerCamera == null)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            rotation *= Quaternion.Euler(0f, 90f, 0f);
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelPlacement();
            return;
        }

        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelPlacement();
            return;
        }

        UpdatePreview();
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && CanPlace())
        {
            ConfirmPlacement();
        }
    }

    public bool BeginPlacement(ItemDefinition definition)
    {
        if (IsPlacing || definition == null || !definition.CanBePlaced || definition.Prefab == null)
        {
            return false;
        }

        if (InventoryManager.Instance == null || !InventoryManager.Instance.Remove(definition))
        {
            return false;
        }

        item = definition;
        consumesInventory = true;
        sourceBox = null;
        CreatePreview(definition);
        return true;
    }

    public bool BeginDirectPlacement(ItemDefinition definition, DeliveryBox box)
    {
        if (IsPlacing || definition == null || box == null || !definition.CanBePlaced || definition.Prefab == null)
        {
            return false;
        }

        item = definition;
        consumesInventory = false;
        sourceBox = box;
        CreatePreview(definition);
        box.HideForPlacement();
        return true;
    }

    public bool BeginReposition(ArcadeMachine machine)
    {
        if (IsPlacing || machine == null || machine.Definition == null || machine.IsReserved
            || machine.State == ArcadeMachineState.Occupied)
        {
            return false;
        }

        item = machine.Definition;
        consumesInventory = false;
        sourceBox = null;
        repositionedMachine = machine;
        originalPosition = machine.transform.position;
        originalRotation = machine.transform.rotation;
        preview = machine.gameObject;
        rotation = machine.transform.rotation;
        SetPreviewState(true);
        return true;
    }

    private void CreatePreview(ItemDefinition definition)
    {
        rotation = Quaternion.identity;
        preview = Instantiate(definition.Prefab);
        ArcadeMachine machine = preview.GetComponent<ArcadeMachine>();
        if (machine != null)
        {
            machine.Initialize(definition as ArcadeMachineDefinition);
        }

        SetPreviewState(true);
        if (GameViewManager.Instance != null)
        {
            GameViewManager.Instance.HideView<InventoryView>();
        }

    }

    private void UpdatePreview()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, placementDistance, floorLayers, QueryTriggerInteraction.Ignore))
        {
            Vector3 position = Snap(hit.point);
            preview.transform.SetPositionAndRotation(position, rotation);
            hasValidSurface = true;
            return;
        }

        hasValidSurface = false;
    }

    private bool CanPlace()
    {
        if (preview == null || item == null || !hasValidSurface)
        {
            return false;
        }

        Vector3 halfExtents = Vector3.Scale(item.PlacementFootprint, preview.transform.lossyScale) * 0.5f;
        Collider[] overlaps = Physics.OverlapBox(preview.transform.position, halfExtents, preview.transform.rotation,
            obstacleLayers, QueryTriggerInteraction.Ignore);
        return overlaps.Length == 0;
    }

    private void ConfirmPlacement()
    {
        SetPreviewState(false);
        if (sourceBox != null)
        {
            sourceBox.DestroyAfterPlacement();
        }

        repositionedMachine = null;
        sourceBox = null;
        preview = null;
        item = null;
        consumesInventory = false;
    }

    private void CancelPlacement()
    {
        if (consumesInventory && item != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.Add(item);
        }

        if (sourceBox != null)
        {
            sourceBox.RestoreAfterCancelledPlacement();
        }

        if (repositionedMachine != null)
        {
            repositionedMachine.transform.SetPositionAndRotation(originalPosition, originalRotation);
            SetPreviewState(false);
        }

        if (preview != null && repositionedMachine == null)
        {
            Destroy(preview);
        }

        preview = null;
        item = null;
        sourceBox = null;
        repositionedMachine = null;
        consumesInventory = false;
    }

    private Vector3 Snap(Vector3 position)
    {
        position.x = Mathf.Round(position.x / gridSize) * gridSize;
        position.y = Mathf.Round(position.y / gridSize) * gridSize;
        position.z = Mathf.Round(position.z / gridSize) * gridSize;
        return position;
    }

    private void SetPreviewState(bool isPreview)
    {
        ArcadeMachine arcade = preview.GetComponent<ArcadeMachine>();
        if (arcade != null) arcade.SetPlacementPreview(isPreview);
        if (isPreview)
        {
            originalRendererColors.Clear();
        }

        Collider[] colliders = preview.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = !isPreview;
        }

        Renderer[] renderers = preview.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            Material[] materials = renderer.materials;
            if (isPreview)
            {
                Color[] originalColors = new Color[materials.Length];
                for (int j = 0; j < materials.Length; j++)
                {
                    originalColors[j] = materials[j].color;
                    materials[j].color = new Color(0.3f, 1f, 0.3f, 0.45f);
                }

                originalRendererColors[renderer] = originalColors;
                continue;
            }

            Color[] savedColors;
            if (!originalRendererColors.TryGetValue(renderer, out savedColors))
            {
                continue;
            }

            int colorCount = Mathf.Min(materials.Length, savedColors.Length);
            for (int j = 0; j < colorCount; j++)
            {
                materials[j].color = savedColors[j];
            }
        }

        if (!isPreview)
        {
            originalRendererColors.Clear();
        }
    }
}
