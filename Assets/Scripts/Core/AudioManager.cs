using UnityEngine;

[DefaultExecutionOrder(-1000), DisallowMultipleComponent]
public sealed class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [SerializeField] private AudioClip[] musicPlaylist;
    [SerializeField] private AudioClip ambience;
    [SerializeField] private bool useDefaultMusic = true;
    [SerializeField, Range(0f, 1f)] private float masterVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.2f;
    [SerializeField, Range(0f, 1f)] private float effectsVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float ambienceVolume = 0.2f;
    private AudioSource musicSource;
    private AudioSource ambienceSource;
    private AudioClip generatedMusic;
    private int trackIndex = -1;
    public static float EffectsVolume { get { return Instance == null ? 0.56f : Instance.masterVolume * Instance.effectsVolume; } }
    public float MasterVolume { get { return masterVolume; } }
    public float MusicVolume { get { return musicVolume; } }
    public float EffectsLevel { get { return effectsVolume; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession() { Instance = null; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        musicSource = gameObject.AddComponent<AudioSource>();
        ambienceSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = ambienceSource.playOnAwake = false;
        musicSource.spatialBlend = ambienceSource.spatialBlend = 0f;
        ambienceSource.loop = true;
        masterVolume = PlayerPrefs.GetFloat("LittleShop.Audio.Master", masterVolume);
        musicVolume = PlayerPrefs.GetFloat("LittleShop.Audio.Music", musicVolume);
        effectsVolume = PlayerPrefs.GetFloat("LittleShop.Audio.Effects", effectsVolume);
    }

    private void Start()
    {
        if (ambience != null) { ambienceSource.clip = ambience; ambienceSource.Play(); }
        PlayNextTrack();
    }

    private void Update()
    {
        musicSource.volume = masterVolume * musicVolume;
        ambienceSource.volume = masterVolume * ambienceVolume;
        if (!musicSource.isPlaying && musicPlaylist != null && musicPlaylist.Length > 0) PlayNextTrack();
    }

    public void PlayNextTrack()
    {
        if (musicPlaylist != null)
            for (int i = 0; i < musicPlaylist.Length; i++)
            {
                trackIndex = (trackIndex + 1) % musicPlaylist.Length;
                if (musicPlaylist[trackIndex] == null) continue;
                musicSource.loop = false;
                musicSource.clip = musicPlaylist[trackIndex];
                musicSource.Play();
                return;
            }
        if (!useDefaultMusic) return;
        if (generatedMusic == null) generatedMusic = CreateDefaultMusic();
        musicSource.clip = generatedMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    private static AudioClip CreateDefaultMusic()
    {
        const int rate = 22050;
        const float beat = 0.4f;
        int[] melody = { 60, 64, 67, 71, 69, 67, 64, 62, 57, 60, 64, 67, 65, 64, 60, 59,
                         53, 57, 60, 64, 62, 60, 57, 55, 55, 59, 62, 65, 64, 62, 59, 55 };
        float[] samples = new float[Mathf.RoundToInt(rate * beat * melody.Length)];
        for (int i = 0; i < samples.Length; i++)
        {
            float time = (float)i / rate;
            int note = Mathf.Min(melody.Length - 1, Mathf.FloorToInt(time / beat));
            float phase = time % beat;
            float frequency = 440f * Mathf.Pow(2f, (melody[note] - 69f) / 12f);
            float envelope = Mathf.Min(1f, phase * 100f) * Mathf.Exp(-phase * 8f);
            float fade = Mathf.Min(1f, (samples.Length - i) / (rate * 0.04f));
            samples[i] = (Mathf.Sin(2f * Mathf.PI * frequency * phase) * 0.18f
                + Mathf.Sin(Mathf.PI * frequency * phase) * 0.08f) * envelope * fade;
        }
        AudioClip clip = AudioClip.Create("Little Shop - Default Arcade Theme", samples.Length, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void SetMasterVolume(float value) { masterVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat("LittleShop.Audio.Master", masterVolume); }
    public void SetMusicVolume(float value) { musicVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat("LittleShop.Audio.Music", musicVolume); }
    public void SetEffectsVolume(float value) { effectsVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat("LittleShop.Audio.Effects", effectsVolume); }

    private void OnDestroy()
    {
        if (generatedMusic != null) Destroy(generatedMusic);
        if (Instance == this) Instance = null;
    }
}
