using UnityEngine;
using UnityEngine.Video;

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
    [Header("Screen and nearby audio")]
    [SerializeField] private VideoClip gameplayVideo;
    [SerializeField] private Texture2D[] gameplayImages;
    [SerializeField] private Texture2D idleScreen;
    [SerializeField] private AudioClip gameplayAudio;
    [SerializeField, Min(0.05f)] private float imageInterval = 0.5f;

    public VideoClip GameplayVideo { get { return gameplayVideo; } }
    public Texture2D[] GameplayImages { get { return gameplayImages; } }
    public Texture2D IdleScreen { get { return idleScreen; } }
    public AudioClip GameplayAudio { get { return gameplayAudio; } }
    public float ImageInterval { get { return imageInterval; } }

    public ArcadeMachineType MachineType { get { return machineType; } }
    public int CoinsPerPlay { get { return coinsPerPlay; } }
    public float PlayDuration { get { return playDuration; } }
    public int CustomerCapacity { get { return customerCapacity; } }
    public int Attractiveness { get { return attractiveness; } }
    public int MaintenanceCost { get { return maintenanceCost; } }
}
