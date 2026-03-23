using UnityEngine;
using System.Collections;

/// <summary>
/// Contrôleur des effets visuels météo (pluie, neige, éclair, etc.)
/// Gère les particules et les effets d'environnement
/// </summary>
public class WeatherEffectsController : MonoBehaviour
{
    [Header("Systèmes de particules")]
    [SerializeField] private ParticleSystem rainSystem;
    [SerializeField] private ParticleSystem snowSystem;
    [SerializeField] private ParticleSystem stormSystem;
    [SerializeField] private LightningEffect lightningEffect;

    [Header("Lumières dynamiques")]
    [SerializeField] private LightningController lightningLight;

    [Header("Paramètres pluie")]
    [SerializeField] private int rainRateMin = 100;
    [SerializeField] private int rainRateMax = 500;
    [SerializeField] private float rainSpeedMin = 5f;
    [SerializeField] private float rainSpeedMax = 10f;

    [Header("Paramètres neige")]
    [SerializeField] private int snowRateMin = 50;
    [SerializeField] private int snowRateMax = 200;
    [SerializeField] private float snowSpeedMin = 1f;
    [SerializeField] private float snowSpeedMax = 3f;

    [Header("Paramètres orage")]
    [SerializeField] private float lightningIntervalMin = 2f;
    [SerializeField] private float lightningIntervalMax = 8f;
    [SerializeField] private float lightningIntensity = 2f;

    [Header("Brouillard")]
    [SerializeField] private bool enableFog = true;
    [SerializeField] private Color sunnyFogColor = new Color(0.6f, 0.7f, 0.8f);
    [SerializeField] private Color rainyFogColor = new Color(0.3f, 0.35f, 0.4f);
    [SerializeField] private Color stormFogColor = new Color(0.2f, 0.2f, 0.25f);
    [SerializeField] private float rainyFogDensity = 0.02f;
    [SerializeField] private float stormFogDensity = 0.04f;

    [Header("FastSky")]
    [SerializeField] private bool useFastSky = true;
    [SerializeField] private float skyTransitionSpeed = 1f;

    [Header("Environnement (Sol)")]
    [SerializeField] private Renderer groundRenderer;
    [SerializeField] private Material defaultGroundMaterial;
    [SerializeField] private Material snowGroundMaterial;
    [SerializeField] private Material rainGroundMaterial;

    private Material skyboxMat;
    private Coroutine skyTransitionCoroutine;

    // FastSky Targets
    private float targetCloudDensity;
    private float targetCloudThickness;
    private Color targetCloudColor;
    private float targetSunIntensity;
    private Color targetDayColor;
    private float targetCloudBrightness;

    private WeatherType currentWeather;
    private Coroutine lightningCoroutine;

    private void Start()
    {
        if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_CloudDensity"))
        {
            skyboxMat = RenderSettings.skybox;
        }

        SetWeather(WeatherType.Sunny);
    }

    public void SetWeather(WeatherType weather)
    {
        currentWeather = weather;
        UpdateAllEffects();
    }

    private void UpdateAllEffects()
    {
        DisableAllEffects();

        switch (currentWeather)
        {
            case WeatherType.Sunny:
                SetSunnyWeather();
                break;
            case WeatherType.Cloudy:
                SetCloudyWeather();
                break;
            case WeatherType.Rainy:
                SetRainyWeather();
                break;
            case WeatherType.Storm:
                SetStormWeather();
                break;
            case WeatherType.Snow:
                SetSnowWeather();
                break;
        }

        if (useFastSky && skyboxMat != null)
        {
            if (skyTransitionCoroutine != null) StopCoroutine(skyTransitionCoroutine);
            skyTransitionCoroutine = StartCoroutine(SkyTransitionCoroutine());
        }
    }

    private void DisableAllEffects()
    {
        if (rainSystem != null) rainSystem.gameObject.SetActive(false);
        if (snowSystem != null) snowSystem.gameObject.SetActive(false);
        if (stormSystem != null) stormSystem.gameObject.SetActive(false);

        if (lightningCoroutine != null)
            StopCoroutine(lightningCoroutine);

        RenderSettings.fog = enableFog;
        RenderSettings.fogDensity = 0f;

        if (groundRenderer != null && defaultGroundMaterial != null)
            groundRenderer.sharedMaterial = defaultGroundMaterial;
    }

    private void SetFastSkyTargets(float density, float thickness, Color cloudColor, float sunIntensity, Color dayColor, float cloudBrightness = 60f)
    {
        targetCloudDensity = density;
        targetCloudThickness = thickness;
        targetCloudColor = cloudColor;
        targetSunIntensity = sunIntensity;
        targetDayColor = dayColor;
        targetCloudBrightness = cloudBrightness;
    }

    private void SetSunnyWeather()
    {
        if (enableFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = sunnyFogColor;
            RenderSettings.fogDensity = 0.005f;
        }
        SetFastSkyTargets(0.0f, 0.0f, new Color(1f, 1f, 1f, 0f), 100f, Color.white, 0f); // Brightness 0
    }

    private void SetCloudyWeather()
    {
        if (enableFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.5f, 0.55f, 0.6f);
            RenderSettings.fogDensity = 0.01f;
        }
        SetFastSkyTargets(5.0f, 0.0f, new Color(0.65f, 0.65f, 0.7f, 1f), 10f, new Color(0.55f, 0.55f, 0.6f), 80f); // Max thick clouds (0 thickness means full cover)
    }

    private void SetRainyWeather()
    {
        if (rainSystem != null)
        {
            rainSystem.gameObject.SetActive(true);
            var main = rainSystem.main;
            main.maxParticles = rainRateMax;
            main.startSpeed = new ParticleSystem.MinMaxCurve(rainSpeedMin, rainSpeedMax);
        }

        if (enableFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = rainyFogColor;
            RenderSettings.fogDensity = rainyFogDensity;
        }

        if (groundRenderer != null && rainGroundMaterial != null)
            groundRenderer.sharedMaterial = rainGroundMaterial;

        SetFastSkyTargets(9.0f, 0.0f, new Color(0.35f, 0.35f, 0.38f, 1f), 5f, new Color(0.35f, 0.35f, 0.4f), 60f); // Plus de nuages que nuageux, ciel sombre de pluie
    }

    private void SetStormWeather()
    {
        if (stormSystem != null)
        {
            stormSystem.gameObject.SetActive(true);
            var main = stormSystem.main;
            main.maxParticles = rainRateMax + 200;
        }

        if (enableFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = stormFogColor;
            RenderSettings.fogDensity = stormFogDensity;
        }

        if (groundRenderer != null && rainGroundMaterial != null)
            groundRenderer.sharedMaterial = rainGroundMaterial;

        SetFastSkyTargets(12.0f, 0.0f, new Color(0.15f, 0.15f, 0.18f, 1f), 0f, new Color(0.15f, 0.15f, 0.2f), 40f); // Ciel noir de tempête - plus dense que pluie

        lightningCoroutine = StartCoroutine(LightningCycle());
    }

    private void SetSnowWeather()
    {
        if (snowSystem != null)
        {
            snowSystem.gameObject.SetActive(true);
            var main = snowSystem.main;
            main.maxParticles = snowRateMax;
            main.startSpeed = new ParticleSystem.MinMaxCurve(snowSpeedMin, snowSpeedMax);
        }

        if (enableFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.85f, 0.9f, 0.95f);
            RenderSettings.fogDensity = 0.015f;
        }

        if (groundRenderer != null && snowGroundMaterial != null)
            groundRenderer.sharedMaterial = snowGroundMaterial;

        SetFastSkyTargets(0.0f, 0.0f, new Color(0.9f, 0.9f, 0.9f, 0f), 20f, new Color(0.8f, 0.8f, 0.85f), 0f);
    }

    private IEnumerator SkyTransitionCoroutine()
    {
        if (skyboxMat == null) yield break;

        float currentDensity = skyboxMat.GetFloat("_CloudDensity");
        float currentThickness = skyboxMat.GetFloat("_CloudThickness");
        Color currentCloudColor = skyboxMat.GetColor("_CloudColour");
        float currentSunIntensity = skyboxMat.GetFloat("_SunIntensity");
        Color currentDayColor = skyboxMat.GetColor("_DayColour");
        float currentCloudBrightness = skyboxMat.GetFloat("_CloudBrightness");

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * skyTransitionSpeed;
            if (t > 1f) t = 1f;

            skyboxMat.SetFloat("_CloudDensity", Mathf.Lerp(currentDensity, targetCloudDensity, t));
            skyboxMat.SetFloat("_CloudThickness", Mathf.Lerp(currentThickness, targetCloudThickness, t));
            skyboxMat.SetColor("_CloudColour", Color.Lerp(currentCloudColor, targetCloudColor, t));
            skyboxMat.SetFloat("_SunIntensity", Mathf.Lerp(currentSunIntensity, targetSunIntensity, t));
            skyboxMat.SetColor("_DayColour", Color.Lerp(currentDayColor, targetDayColor, t));
            skyboxMat.SetFloat("_CloudBrightness", Mathf.Lerp(currentCloudBrightness, targetCloudBrightness, t));

            yield return null;
        }
    }

    private IEnumerator LightningCycle()
    {
        while (currentWeather == WeatherType.Storm)
        {
            float interval = Random.Range(lightningIntervalMin, lightningIntervalMax);
            yield return new WaitForSeconds(interval);

            TriggerLightning();
        }
    }

    private void TriggerLightning()
    {
        if (lightningLight != null)
            lightningLight.TriggerLightning(lightningIntensity);

        if (lightningEffect != null)
            lightningEffect.Flash();

        // Flash skybox for lightning
        if (useFastSky && skyboxMat != null)
        {
            StartCoroutine(FlashSkybox());
        }
    }

    private IEnumerator FlashSkybox()
    {
        Color originalColor = skyboxMat.GetColor("_CloudColour");
        skyboxMat.SetColor("_CloudColour", Color.white);
        skyboxMat.SetFloat("_SunIntensity", 100f);
        
        yield return new WaitForSeconds(0.1f);
        
        skyboxMat.SetColor("_CloudColour", originalColor);
        skyboxMat.SetFloat("_SunIntensity", targetSunIntensity);
    }

    public void SetRainIntensity(float intensity)
    {
        if (rainSystem != null)
        {
            var main = rainSystem.main;
            main.maxParticles = Mathf.RoundToInt(Mathf.Lerp(rainRateMin, rainRateMax, intensity));
        }
    }
}

/// <summary>
/// Effet d'éclair pour les orages
/// </summary>
public class LightningEffect : MonoBehaviour
{
    [SerializeField] private Light flashLight;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float maxIntensity = 3f;
    [SerializeField] private int flashCount = 1;

    private Coroutine flashCoroutine;

    public void Flash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        for (int i = 0; i < flashCount; i++)
        {
            if (flashLight != null)
            {
                flashLight.intensity = maxIntensity;
                yield return new WaitForSeconds(flashDuration);
                flashLight.intensity = 0f;

                if (i < flashCount - 1)
                    yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            }
        }
    }
}

/// <summary>
/// Contrôleur pour la lumière d'éclair
/// </summary>
public class LightningController : MonoBehaviour
{
    [SerializeField] private Light lightningLight;
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine fadeCoroutine;

    public void TriggerLightning(float intensity)
    {
        if (lightningLight != null)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            lightningLight.intensity = intensity;
            fadeCoroutine = StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeOut()
    {
        float startIntensity = lightningLight.intensity;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            lightningLight.intensity = Mathf.Lerp(startIntensity, 0f, timer / fadeDuration);
            yield return null;
        }

        lightningLight.intensity = 0f;
    }
}
