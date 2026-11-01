using UnityEngine;

public enum ArcadeMachineType
{
    Cabinet,
    Claw,
    Rhythm,
    Racing
}

[CreateAssetMenu(menuName = "Little Shop/Arcade Machine Definition")]
public sealed class ArcadeMachineDefinition : ItemDefinition
{
    [SerializeField] private ArcadeMachineType machineType;
    [SerializeField, Min(1)] private int coinsPerPlay = 1;
    [SerializeField, Min(1f)] private float playDuration = 30f;
    [SerializeField, Min(1)] private int customerCapacity = 1;
    [SerializeField, Min(0)] private int attractiveness = 1;
    [SerializeField, Min(0)] private int maintenanceCost;

    public ArcadeMachineType MachineType { get { return machineType; } }
    public int CoinsPerPlay { get { return coinsPerPlay; } }
    public float PlayDuration { get { return playDuration; } }
    public int CustomerCapacity { get { return customerCapacity; } }
    public int Attractiveness { get { return attractiveness; } }
    public int MaintenanceCost { get { return maintenanceCost; } }
}
