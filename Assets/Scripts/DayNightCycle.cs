using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance { get; private set; }

    [Header("Cycle Settings")]
    public float totalCycleDuration = 300f; // 5 minutes (2.5m Day, 2.5m Night)
    public float cycleTimer = 0f;           // Starts at midnight
    public bool isDay = false;

    [Header("Lighting")]
    public Color nightAmbient = new Color(0.04f, 0.04f, 0.08f);
    public Color dayAmbient = new Color(0.32f, 0.32f, 0.36f);
    public Color nightFog = new Color(0.03f, 0.03f, 0.07f);
    public Color dayFog = new Color(0.28f, 0.28f, 0.32f);

    private Light sunLight;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        FindOrCreateSun();
    }

    private void Start()
    {
        cycleTimer = 0f; // Start at night
        UpdateAtmosphere();
    }

    private void Update()
    {
        cycleTimer += Time.deltaTime;
        if (cycleTimer >= totalCycleDuration)
        {
            cycleTimer -= totalCycleDuration;
        }

        // 0 to 150s = Night, 150s to 300s = Day
        isDay = (cycleTimer >= (totalCycleDuration * 0.5f));

        UpdateAtmosphere();
    }

    private void UpdateAtmosphere()
    {
        float half = totalCycleDuration * 0.5f;
        float t;

        if (isDay)
        {
            // Daytime progression (0 to 1)
            t = (cycleTimer - half) / half;
            RenderSettings.ambientLight = Color.Lerp(nightAmbient, dayAmbient, Mathf.Sin(t * Mathf.PI));
            RenderSettings.fogColor = Color.Lerp(nightFog, dayFog, Mathf.Sin(t * Mathf.PI));
            RenderSettings.fogDensity = 0.018f;

            if (sunLight != null)
            {
                sunLight.enabled = true;
                float sunAngle = Mathf.Lerp(15f, 65f, Mathf.Sin(t * Mathf.PI));
                sunLight.transform.rotation = Quaternion.Euler(sunAngle, -35f, 0f);
                sunLight.intensity = Mathf.Lerp(0.3f, 1.4f, Mathf.Sin(t * Mathf.PI));
                sunLight.color = new Color(1f, 0.95f, 0.85f);
            }
        }
        else
        {
            // Nighttime progression (0 to 1)
            t = cycleTimer / half;
            RenderSettings.ambientLight = nightAmbient;
            RenderSettings.fogColor = nightFog;
            RenderSettings.fogDensity = 0.024f;

            if (sunLight != null)
            {
                sunLight.enabled = false;
                sunLight.intensity = 0f;
            }
        }
    }

    public string GetTimeOfDayStatus()
    {
        float remainingInPhase = isDay ? (totalCycleDuration - cycleTimer) : ((totalCycleDuration * 0.5f) - cycleTimer);
        int mins = Mathf.FloorToInt(remainingInPhase / 60f);
        int secs = Mathf.FloorToInt(remainingInPhase % 60f);

        if (isDay)
        {
            return $"DAYLIGHT [SEEK SHADOW!] - Night in {mins:D2}:{secs:D2}";
        }
        else
        {
            return $"NIGHT [SAFE] - Dawn in {mins:D2}:{secs:D2}";
        }
    }

    private void FindOrCreateSun()
    {
        Light[] allLights = FindObjectsByType<Light>(FindObjectsInactive.Exclude);
        foreach (Light l in allLights)
        {
            if (l.type == LightType.Directional)
            {
                sunLight = l;
                return;
            }
        }

        GameObject sunObj = new GameObject("CelestialSun");
        sunLight = sunObj.AddComponent<Light>();
        sunLight.type = LightType.Directional;
        sunLight.shadows = LightShadows.Soft;
    }
}
