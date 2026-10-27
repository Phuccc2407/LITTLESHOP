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

    public bool IsPlacing { get { return item != null; } }

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
        rotation = Quaternion.identity;
        preview = Instantiate(definition.Prefab);
        SetPreviewState(true);
        return true;
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
        preview = null;
        item = null;
    }

    private void CancelPlacement()
    {
        if (item != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.Add(item);
        }

        if (preview != null)
        {
            Destroy(preview);
        }

        preview = null;
        item = null;
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
        Collider[] colliders = preview.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = !isPreview;
        }

        Renderer[] renderers = preview.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = isPreview
                ? new Color(0.3f, 1f, 0.3f, 0.45f)
                : Color.white;
        }
    }
}
