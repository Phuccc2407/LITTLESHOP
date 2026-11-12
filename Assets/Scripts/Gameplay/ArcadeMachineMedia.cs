using UnityEngine;
using UnityEngine.Video;

[DisallowMultipleComponent, RequireComponent(typeof(ArcadeMachine))]
public sealed class ArcadeMachineMedia : MonoBehaviour
{
    [SerializeField] private Renderer[] screenRenderers;
    [SerializeField, Range(0f, 1f)] private float volume = 0.35f;
    [SerializeField, Min(1f)] private float audibleDistance = 8f;
    [SerializeField] private bool useDefaultGame = true;
    private ArcadeMachine machine;
    private AudioSource loopSource;
    private AudioSource videoAudio;
    private VideoPlayer video;
    private RenderTexture videoTexture;
    private MaterialPropertyBlock block;
    private Texture2D defaultScreen;
    private Color32[] pixels;
    private AudioClip defaultSound;
    private bool playing;
    private float elapsed;
    private float nextFrame;
    private bool videoFailed;

    private void Awake()
    {
        machine = GetComponent<ArcadeMachine>();
        block = new MaterialPropertyBlock();
        loopSource = CreateSpatialSource();
        videoAudio = CreateSpatialSource();
        video = gameObject.AddComponent<VideoPlayer>();
        video.playOnAwake = false;
        video.isLooping = true;
        video.renderMode = VideoRenderMode.RenderTexture;
        video.audioOutputMode = VideoAudioOutputMode.AudioSource;
        video.prepareCompleted += OnVideoPrepared;
        video.errorReceived += OnVideoError;
        machine.StateChanged += Refresh;
    }

    private void Start() { Refresh(); }
    private void OnEnable() { if (machine != null) Refresh(); }

    private AudioSource CreateSpatialSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 1f;
        source.maxDistance = audibleDistance;
        source.loop = true;
        return source;
    }

    private void Refresh()
    {
        bool shouldPlay = machine.State == ArcadeMachineState.Occupied && !machine.IsPlacementPreview;
        if (shouldPlay == playing)
        {
            if (!playing) SetScreen(machine.Definition == null ? null : machine.Definition.IdleScreen);
            return;
        }
        playing = shouldPlay;
        elapsed = nextFrame = 0f;
        videoFailed = false;
        if (!playing)
        {
            loopSource.Stop();
            videoAudio.Stop();
            video.Stop();
            SetScreen(machine.Definition == null ? null : machine.Definition.IdleScreen);
            return;
        }
        ArcadeMachineDefinition definition = machine.Definition;
        if (definition != null && definition.GameplayVideo != null)
        {
            if (videoTexture == null) { videoTexture = new RenderTexture(1024, 576, 0); videoTexture.Create(); }
            video.targetTexture = videoTexture;
            video.clip = definition.GameplayVideo;
            if (video.clip.audioTrackCount > 0)
            {
                video.audioOutputMode = VideoAudioOutputMode.AudioSource;
                video.controlledAudioTrackCount = 1;
                video.SetTargetAudioSource(0, videoAudio);
            }
            else video.audioOutputMode = VideoAudioOutputMode.None;
            video.Prepare();
        }
        loopSource.clip = definition == null ? null : definition.GameplayAudio;
        if (loopSource.clip == null && useDefaultGame && (definition == null || definition.GameplayVideo == null))
        {
            if (defaultSound == null) defaultSound = CreateDefaultSound();
            loopSource.clip = defaultSound;
        }
        if (loopSource.clip != null) loopSource.Play();
    }

    private void OnVideoPrepared(VideoPlayer player)
    {
        if (!playing || !isActiveAndEnabled) return;
        // A separately assigned game loop takes priority over the video's own audio.
        videoAudio.mute = loopSource.clip != null;
        SetScreen(videoTexture);
        player.Play();
    }

    private void OnVideoError(VideoPlayer player, string message)
    {
        videoFailed = true;
        Debug.LogWarning("Arcade video could not play; using images instead. " + message, this);
    }

    private void Update()
    {
        float gain = volume * AudioManager.EffectsVolume;
        loopSource.volume = videoAudio.volume = gain;
        if (!playing) return;
        elapsed += Time.deltaTime;
        ArcadeMachineDefinition definition = machine.Definition;
        if (definition != null && definition.GameplayVideo != null && !videoFailed && video.isPrepared) return;
        if (elapsed < nextFrame) return;
        nextFrame = elapsed + (definition == null ? 0.15f : Mathf.Max(0.05f, definition.ImageInterval));
        Texture2D[] images = definition == null ? null : definition.GameplayImages;
        if (images != null && images.Length > 0)
            SetScreen(images[Mathf.FloorToInt(elapsed / Mathf.Max(0.05f, definition.ImageInterval)) % images.Length]);
        else if (useDefaultGame) DrawDefaultGame();
    }

    private void DrawDefaultGame()
    {
        const int width = 128, height = 72;
        if (defaultScreen == null)
        {
            defaultScreen = new Texture2D(width, height, TextureFormat.RGBA32, false);
            defaultScreen.filterMode = FilterMode.Point;
            pixels = new Color32[width * height];
        }
        int ballX = Mathf.RoundToInt(8f + Mathf.PingPong(elapsed * 22f, 110f));
        int ballY = Mathf.RoundToInt(8f + Mathf.PingPong(elapsed * 16f, 54f));
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                bool ball = Mathf.Abs(x - ballX) < 2 && Mathf.Abs(y - ballY) < 2;
                bool paddle = (x < 7 || x > 120) && Mathf.Abs(y - ballY) < 9;
                bool line = x == width / 2 && y % 8 < 4;
                pixels[y * width + x] = ball || paddle ? new Color32(70, 255, 195, 255)
                    : line ? new Color32(40, 75, 100, 255) : new Color32(4, 8, 20, 255);
            }
        defaultScreen.SetPixels32(pixels);
        defaultScreen.Apply(false);
        SetScreen(defaultScreen);
    }

    private void SetScreen(Texture texture)
    {
        if (screenRenderers == null) return;
        block.Clear();
        if (texture != null)
        {
            block.SetTexture("_BaseMap", texture);
            block.SetTexture("_MainTex", texture);
            block.SetColor("_BaseColor", Color.white);
            block.SetColor("_Color", Color.white);
        }
        foreach (Renderer renderer in screenRenderers)
            if (renderer != null) renderer.SetPropertyBlock(block);
    }

    private static AudioClip CreateDefaultSound()
    {
        const int rate = 22050;
        float[] samples = new float[rate * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            float time = (float)i / rate;
            float phase = time % 0.25f;
            float note = (Mathf.FloorToInt(time * 4f) % 4) * 110f + 330f;
            samples[i] = Mathf.Sin(2f * Mathf.PI * note * phase) * Mathf.Exp(-phase * 35f)
                * Mathf.Min(1f, phase * 500f) * 0.12f;
        }
        AudioClip clip = AudioClip.Create("Arcade - Local Game Loop", samples.Length, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnDisable()
    {
        playing = false;
        if (loopSource != null) loopSource.Stop();
        if (video != null) video.Stop();
        if (videoAudio != null) videoAudio.Stop();
    }

    private void OnDestroy()
    {
        if (machine != null) machine.StateChanged -= Refresh;
        if (videoTexture != null) { videoTexture.Release(); Destroy(videoTexture); }
        if (defaultScreen != null) Destroy(defaultScreen);
        if (defaultSound != null) Destroy(defaultSound);
    }
}
