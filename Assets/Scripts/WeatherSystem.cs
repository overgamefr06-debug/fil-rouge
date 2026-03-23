using UnityEngine;
using System.Collections;

/// <summary>
/// Système principal de gestion de la météo
/// Gère les différents états météo et notifie la plante des changements
/// </summary>
public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance { get; private set; }

    [Header("États météo disponibles")]
    [SerializeField] private WeatherType currentWeather = WeatherType.Sunny;

    [Header("Paramètres de changement")]
    [SerializeField] private float minWeatherDuration = 30f;
    [SerializeField] private float maxWeatherDuration = 60f;
    [SerializeField] private bool autoChangeWeather = false;

    [Header("Références")]
    [SerializeField] private Light sunLight;
    [SerializeField] private Material skyMaterial;

    [Header("Couleurs par état")]
    [SerializeField] private Color sunnyColor = new Color(1f, 0.95f, 0.8f);
    [SerializeField] private Color rainyColor = new Color(0.4f, 0.45f, 0.5f);
    [SerializeField] private Color cloudyColor = new Color(0.6f, 0.65f, 0.7f);
    [SerializeField] private Color stormColor = new Color(0.3f, 0.3f, 0.35f);
    [SerializeField] private Color snowColor = new Color(0.85f, 0.9f, 1f);

    public WeatherType CurrentWeather => currentWeather;
    public float WeatherProgress { get; private set; }
    public float WeatherDuration { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (autoChangeWeather)
            StartCoroutine(WeatherCycle());

        UpdateWeatherVisuals();
    }

    private IEnumerator WeatherCycle()
    {
        while (true)
        {
            WeatherDuration = Random.Range(minWeatherDuration, maxWeatherDuration);
            WeatherProgress = 0f;

            float timer = 0f;
            while (timer < WeatherDuration)
            {
                timer += Time.deltaTime;
                WeatherProgress = timer / WeatherDuration;
                yield return null;
            }

            ChangeWeather(GetNextWeatherType());
        }
    }

    private WeatherType GetNextWeatherType()
    {
        // Logique simple pour changer de météo
        switch (currentWeather)
        {
            case WeatherType.Sunny:
                return Random.value > 0.5f ? WeatherType.Cloudy : WeatherType.Rainy;
            case WeatherType.Cloudy:
                return Random.value > 0.3f ? WeatherType.Sunny : WeatherType.Rainy;
            case WeatherType.Rainy:
                if (Random.value > 0.7f) return WeatherType.Storm;
                return Random.value > 0.5f ? WeatherType.Cloudy : WeatherType.Sunny;
            case WeatherType.Storm:
                return WeatherType.Rainy;
            case WeatherType.Snow:
                return WeatherType.Sunny;
            default:
                return WeatherType.Sunny;
        }
    }

    public void ChangeWeather(WeatherType newWeather)
    {
        if (currentWeather != newWeather)
        {
            currentWeather = newWeather;
            UpdateWeatherVisuals();

            // Notifier la plante du changement
            var plant = FindObjectOfType<PlantBehavior>();
            if (plant != null)
                plant.OnWeatherChanged(currentWeather);

            // Notifier le contrôleur d'effets visuels
            var effects = FindObjectOfType<WeatherEffectsController>();
            if (effects != null)
                effects.SetWeather(currentWeather);
        }
    }

    private void UpdateWeatherVisuals()
    {
        Color targetColor = GetWeatherColor();
        float targetShadow = GetWeatherShadowStrength();
        StartCoroutine(TransitionLighting(targetColor, targetShadow));
    }

    private Color GetWeatherColor()
    {
        switch (currentWeather)
        {
            case WeatherType.Sunny: return sunnyColor;
            case WeatherType.Rainy: return rainyColor;
            case WeatherType.Cloudy: return cloudyColor;
            case WeatherType.Storm: return stormColor;
            case WeatherType.Snow: return snowColor;
            default: return sunnyColor;
        }
    }

    private float GetWeatherShadowStrength()
    {
        switch (currentWeather)
        {
            case WeatherType.Sunny: return 1.0f;
            case WeatherType.Cloudy: return 0.5f; // Ombre diminuée de moitié
            case WeatherType.Rainy: return 0.2f;
            case WeatherType.Storm: return 0.1f;
            case WeatherType.Snow: return 0.3f;
            default: return 1.0f;
        }
    }

    private IEnumerator TransitionLighting(Color targetColor, float targetShadow, float duration = 2f)
    {
        if (sunLight != null)
        {
            Color startColor = sunLight.color;
            float startShadow = sunLight.shadowStrength;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                sunLight.color = Color.Lerp(startColor, targetColor, t);
                sunLight.shadowStrength = Mathf.Lerp(startShadow, targetShadow, t);
                yield return null;
            }

            sunLight.color = targetColor;
            sunLight.shadowStrength = targetShadow;
        }
    }

    public void ForceWeatherChange(WeatherType newWeather)
    {
        StopAllCoroutines();
        ChangeWeather(newWeather);
        if (autoChangeWeather)
            StartCoroutine(WeatherCycle());
    }
}

public enum WeatherType
{
    Sunny,
    Cloudy,
    Rainy,
    Storm,
    Snow
}
