using System;
using System.Collections.Generic;
using UnityEngine;

public enum ShopSound { Click, Arrival, Coins, Play, Win, Unhappy, Order, Repair, Door, Unpack }

// Optional clips override the generated arcade sounds, so existing scenes work immediately.
public sealed class ShopAudio : MonoBehaviour
{
    [Serializable] public sealed class SoundEntry
    {
        public ShopSound Sound;
        public AudioClip[] Clips;
        [Range(0f, 1f)] public float Volume = 0.5f;
    }
    [SerializeField] private SoundEntry[] sounds;
    [SerializeField, Range(0f, 1f)] private float masterVolume = 0.35f;
    private static ShopAudio instance;
    private AudioSource source;
    private readonly Dictionary<ShopSound, AudioClip> generated = new Dictionary<ShopSound, AudioClip>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession() { instance = null; }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(this); return; }
        instance = this;
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = AudioManager.EffectsVolume;
    }

    private void Update() { if (source != null) source.volume = AudioManager.EffectsVolume; }

    public static void Play(ShopSound sound)
    {
        if (instance == null)
        {
            instance = FindAnyObjectByType<ShopAudio>();
            if (instance == null) instance = new GameObject("ShopAudio").AddComponent<ShopAudio>();
        }
        if (instance.source == null) return;
        if (instance.sounds != null)
            foreach (SoundEntry entry in instance.sounds)
                if (entry != null && entry.Sound == sound && entry.Clips != null && entry.Clips.Length > 0)
                {
                    AudioClip clip = entry.Clips[UnityEngine.Random.Range(0, entry.Clips.Length)];
                    if (clip != null) { instance.source.PlayOneShot(clip, instance.masterVolume * entry.Volume); return; }
                }
        if (!instance.generated.TryGetValue(sound, out AudioClip fallback))
        {
            const int rate = 22050;
            float duration = sound == ShopSound.Click ? 0.06f : 0.25f;
            float[] samples = new float[(int)(rate * duration)];
            float frequency = sound == ShopSound.Unhappy ? 180f : 440f + (int)sound * 90f;
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / rate;
                float envelope = Mathf.Sin(Mathf.PI * i / samples.Length);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.25f;
            }
            fallback = AudioClip.Create(sound.ToString(), samples.Length, 1, rate, false);
            fallback.SetData(samples, 0);
            instance.generated.Add(sound, fallback);
        }
        instance.source.PlayOneShot(fallback, instance.masterVolume);
    }

    private void OnDestroy()
    {
        foreach (AudioClip clip in generated.Values) Destroy(clip);
        if (instance == this) instance = null;
    }
}
