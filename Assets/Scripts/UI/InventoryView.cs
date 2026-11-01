using UnityEngine;

public sealed class InventoryView : View
{
    [SerializeField] private PlacementSystem placementSystem;
    [SerializeField] private ItemDefinition itemToPlace;

    public void PlaceSelectedItem()
    {
        if (placementSystem != null)
        {
            placementSystem.BeginPlacement(itemToPlace);
        }
    }

    public void Close()
    {
        if (GameViewManager.Instance != null)
        {
            GameViewManager.Instance.HideView<InventoryView>();
        }
    }
}
