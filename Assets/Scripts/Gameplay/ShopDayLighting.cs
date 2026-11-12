using UnityEngine;

[DisallowMultipleComponent]
public sealed class ShopDayLighting : MonoBehaviour
{
    [SerializeField] private ShopDayCycle clock;
    [SerializeField] private Light directionalLight;
    [SerializeField, Min(0f)] private float noonIntensity = 1.2f;
    [SerializeField, Min(0f)] private float nightIntensity = 0.03f;
    [SerializeField] private float sunYaw = -30f;
    [SerializeField] private bool rotateSun = true;

    private void Awake()
    {
        if (clock == null) clock = FindAnyObjectByType<ShopDayCycle>();
        if (directionalLight == null) directionalLight = RenderSettings.sun;
        if (directionalLight == null)
            foreach (Light candidate in FindObjectsByType<Light>())
                if (candidate.type == LightType.Directional) { directionalLight = candidate; break; }
    }

    private void LateUpdate()
    {
        if (clock == null || directionalLight == null) return;
        float hour = clock.CurrentTimeMinutes / 60f;
        float daylight = Mathf.Clamp01(Mathf.Sin((hour - 6f) * Mathf.PI / 12f));
        directionalLight.intensity = Mathf.Lerp(nightIntensity, noonIntensity, daylight);
        directionalLight.color = Color.Lerp(new Color(1f, 0.55f, 0.3f), new Color(1f, 0.96f, 0.88f), daylight);
        if (rotateSun) directionalLight.transform.rotation = Quaternion.Euler(hour * 15f - 90f, sunYaw, 0f);
    }
}
