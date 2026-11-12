using UnityEngine;

[CreateAssetMenu(menuName = "Little Shop/Customer Profile")]
public sealed class CustomerProfile : ScriptableObject
{
    public string DisplayName = "Casual visitor";
    [Min(1)] public int MinPlays = 1;
    [Min(1)] public int MaxPlays = 4;
    [Min(1)] public int CoinBudget = 12;
    [Min(1f)] public float PatienceSeconds = 60f;
    [Range(0f, 1f)] public float PriceSensitivity = 0.6f;
    [Range(0f, 1f)] public float ReviewChance = 0.8f;
}
