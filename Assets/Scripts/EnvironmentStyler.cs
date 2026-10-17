using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentStyler : MonoBehaviour
{
    public static EnvironmentStyler Instance { get; private set; }

    private readonly Dictionary<string, Material> cachedMaterials = new Dictionary<string, Material>();
    private Shader litShader;
    private readonly List<Light> dynamicFlickerLights = new List<Light>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInit()
    {
        if (FindAnyObjectByType<EnvironmentStyler>() == null)
        {
            GameObject obj = new GameObject("NIGHTBOUND_EnvironmentStyler");
            obj.AddComponent<EnvironmentStyler>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");
        if (litShader == null) litShader = Shader.Find("Unlit/Color");

        StartCoroutine(LightFlickerRoutine());
    }

    // ================= LEVEL ATMOSPHERE & COLOR PALETTES =================
    public void ApplyLevelTheme(int levelIndex)
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        dynamicFlickerLights.Clear();

        Light mainLight = FindDirectionalLight();

        if (levelIndex == 1)
        {
            // LEVEL 1: GRITTY URBAN NOIR (Moody Navy & Sodium Amber)
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.04f, 0.05f, 0.09f);
            RenderSettings.ambientEquatorColor = new Color(0.03f, 0.04f, 0.06f);
            RenderSettings.ambientGroundColor = new Color(0.02f, 0.02f, 0.03f);

            RenderSettings.fogColor = new Color(0.025f, 0.035f, 0.065f);
            RenderSettings.fogDensity = 0.028f;

            if (mainLight != null)
            {
                mainLight.color = new Color(0.45f, 0.6f, 0.85f);
                mainLight.intensity = 0.25f;
                mainLight.shadows = LightShadows.Soft;
                mainLight.transform.rotation = Quaternion.Euler(50f, -40f, 0f);
            }
        }
        else if (levelIndex == 2)
        {
            // LEVEL 2: HAUNTED CEMETERY (Spectral Teal, Pale Moonlight & Fire)
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.03f, 0.065f, 0.06f);
            RenderSettings.ambientEquatorColor = new Color(0.02f, 0.04f, 0.04f);
            RenderSettings.ambientGroundColor = new Color(0.015f, 0.025f, 0.02f);

            RenderSettings.fogColor = new Color(0.02f, 0.055f, 0.05f);
            RenderSettings.fogDensity = 0.035f; // Dense eerie cemetery mist!

            if (mainLight != null)
            {
                mainLight.color = new Color(0.6f, 0.8f, 0.85f);
                mainLight.intensity = 0.35f;
                mainLight.shadows = LightShadows.Soft;
                mainLight.transform.rotation = Quaternion.Euler(60f, 130f, 0f);
            }
        }
        else if (levelIndex == 3)
        {
            // LEVEL 3: INDUSTRIAL WAREHOUSE (Smoky Charcoal & Harsh Halogen Contrast)
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.05f, 0.045f, 0.05f);
            RenderSettings.ambientEquatorColor = new Color(0.035f, 0.03f, 0.035f);
            RenderSettings.ambientGroundColor = new Color(0.02f, 0.02f, 0.025f);

            RenderSettings.fogColor = new Color(0.04f, 0.035f, 0.04f);
            RenderSettings.fogDensity = 0.018f;

            if (mainLight != null)
            {
                mainLight.color = new Color(0.85f, 0.8f, 0.75f);
                mainLight.intensity = 0.3f;
                mainLight.shadows = LightShadows.Soft;
                mainLight.transform.rotation = Quaternion.Euler(75f, 30f, 0f);
            }
        }
        else if (levelIndex == 4)
        {
            // LEVEL 4: BOSS PENTHOUSE (Decadent Crimson, Violet & Polished Gold)
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.08f, 0.025f, 0.045f);
            RenderSettings.ambientEquatorColor = new Color(0.05f, 0.02f, 0.035f);
            RenderSettings.ambientGroundColor = new Color(0.03f, 0.015f, 0.02f);

            RenderSettings.fogColor = new Color(0.06f, 0.02f, 0.035f);
            RenderSettings.fogDensity = 0.015f;

            if (mainLight != null)
            {
                mainLight.color = new Color(0.95f, 0.35f, 0.25f);
                mainLight.intensity = 0.45f;
                mainLight.shadows = LightShadows.Soft;
                mainLight.transform.rotation = Quaternion.Euler(45f, -120f, 0f);
            }
        }
    }

    // ================= SURFACE SHADERS & MATERIALS =================
    public Material GetStyledMaterial(string type, Color customColor = default)
    {
        string key = type + "_" + customColor.ToString();
        if (cachedMaterials.TryGetValue(key, out Material mat) && mat != null)
            return mat;

        Material m = new Material(litShader);

        switch (type.ToLower())
        {
            case "wet_asphalt":
                m.color = new Color(0.1f, 0.11f, 0.13f);
                SetProps(m, 0.75f, 0.1f);
                break;
            case "brick_dark":
                m.color = new Color(0.24f, 0.14f, 0.13f);
                SetProps(m, 0.15f, 0.05f);
                break;
            case "brick_shadow":
                m.color = new Color(0.12f, 0.13f, 0.16f);
                SetProps(m, 0.2f, 0.05f);
                break;
            case "graveyard_earth":
                m.color = new Color(0.08f, 0.075f, 0.07f);
                SetProps(m, 0.25f, 0.05f);
                break;
            case "mossy_stone":
                m.color = new Color(0.14f, 0.16f, 0.15f);
                SetProps(m, 0.18f, 0.05f);
                break;
            case "industrial_floor":
                m.color = new Color(0.13f, 0.13f, 0.14f);
                SetProps(m, 0.65f, 0.15f);
                break;
            case "container_rust":
                m.color = new Color(0.48f, 0.18f, 0.12f);
                SetProps(m, 0.4f, 0.65f);
                break;
            case "container_blue":
                m.color = new Color(0.14f, 0.28f, 0.46f);
                SetProps(m, 0.45f, 0.65f);
                break;
            case "container_gold":
                m.color = new Color(0.45f, 0.38f, 0.12f);
                SetProps(m, 0.45f, 0.65f);
                break;
            case "marble_black":
                m.color = new Color(0.06f, 0.06f, 0.08f);
                SetProps(m, 0.88f, 0.2f);
                break;
            case "velvet_crimson":
                m.color = new Color(0.42f, 0.06f, 0.08f);
                SetProps(m, 0.1f, 0.05f);
                break;
            case "gold_ornate":
                m.color = new Color(0.85f, 0.68f, 0.22f);
                SetProps(m, 0.85f, 0.9f);
                break;
            case "neon_red":
                m.color = Color.black;
                SetEmission(m, new Color(1f, 0.1f, 0.2f) * 2.5f);
                break;
            case "neon_cyan":
                m.color = Color.black;
                SetEmission(m, new Color(0.1f, 0.85f, 1f) * 2.5f);
                break;
            case "neon_amber":
                m.color = Color.black;
                SetEmission(m, new Color(1f, 0.7f, 0.15f) * 2.5f);
                break;
            default:
                m.color = (customColor != default) ? customColor : Color.gray;
                SetProps(m, 0.3f, 0.1f);
                break;
        }

        cachedMaterials[key] = m;
        return m;
    }

    private void SetProps(Material m, float smoothness, float metallic)
    {
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
    }

    private void SetEmission(Material m, Color emission)
    {
        m.EnableKeyword("_EMISSION");
        if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", emission);
    }

    public void RegisterFlickerLight(Light l)
    {
        if (l != null && !dynamicFlickerLights.Contains(l))
            dynamicFlickerLights.Add(l);
    }

    private IEnumerator LightFlickerRoutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(0.07f);
            for (int i = dynamicFlickerLights.Count - 1; i >= 0; i--)
            {
                Light l = dynamicFlickerLights[i];
                if (l == null)
                {
                    dynamicFlickerLights.RemoveAt(i);
                    continue;
                }
                float noise = Mathf.PerlinNoise(Time.time * 9f, l.transform.position.x * 11.3f + l.transform.position.z * 7.1f);
                l.intensity = Mathf.Lerp(2.8f, 4.2f, noise);
            }
        }
    }

    private Light FindDirectionalLight()
    {
        Light[] all = FindObjectsByType<Light>(FindObjectsInactive.Exclude);
        foreach (Light l in all)
        {
            if (l.type == LightType.Directional) return l;
        }
        return null;
    }
}
