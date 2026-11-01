using UnityEngine;

public enum ItemCategory
{
    ArcadeMachine,
    Furniture,
    Decoration,
    Utility
}

[CreateAssetMenu(menuName = "Little Shop/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [TextArea, SerializeField] private string description;
    [SerializeField] private ItemCategory category;
    [SerializeField, Min(0)] private int purchasePrice;
    [SerializeField] private GameObject prefab;
    [SerializeField] private Sprite icon;
    [SerializeField] private Vector3 placementFootprint = Vector3.one;
    [SerializeField] private bool canBePlaced = true;
    [SerializeField] private bool canBeCarried = true;
    [SerializeField] private bool canBeUnpacked = true;

    public string ItemId { get { return itemId; } }
    public string DisplayName { get { return displayName; } }
    public string Description { get { return description; } }
    public ItemCategory Category { get { return category; } }
    public int PurchasePrice { get { return purchasePrice; } }
    public GameObject Prefab { get { return prefab; } }
    public Sprite Icon { get { return icon; } }
    public Vector3 PlacementFootprint { get { return placementFootprint; } }
    public bool CanBePlaced { get { return canBePlaced; } }
    public bool CanBeCarried { get { return canBeCarried; } }
    public bool CanBeUnpacked { get { return canBeUnpacked; } }
}
