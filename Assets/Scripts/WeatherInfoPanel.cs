using UnityEngine;

/// <summary>
/// Panneau d'information affichant les détails météo quand activé
/// S'affiche près de la plante pour donner des informations précises
/// </summary>
public class WeatherInfoPanel : MonoBehaviour
{
    [Header("Affichage")]
    [SerializeField] private GameObject panelObject;
    [SerializeField] private TMPro.TextMeshProUGUI titleText;
    [SerializeField] private TMPro.TextMeshProUGUI descriptionText;
    [SerializeField] private TMPro.TextMeshProUGUI temperatureText;
    [SerializeField] private TMPro.TextMeshProUGUI humidityText;
    [SerializeField] private TMPro.TextMeshProUGUI windText;

    [Header("Données météo simulées")]
    [SerializeField] private float baseTemperature = 20f;
    [SerializeField] private float temperatureVariation = 10f;

    private bool isActive = false;
    private WeatherType currentWeather;

    private void Start()
    {
        if (panelObject != null)
            panelObject.SetActive(false);
    }

    public void ShowWeatherInfo(WeatherType weather)
    {
        currentWeather = weather;
        UpdateWeatherInfo();

        if (panelObject != null)
        {
            panelObject.SetActive(true);
            isActive = true;
        }
    }

    public void HideWeatherInfo()
    {
        if (panelObject != null)
        {
            panelObject.SetActive(false);
            isActive = false;
        }
    }

    public void ToggleWeatherInfo()
    {
        if (isActive)
            HideWeatherInfo();
        else
            ShowWeatherInfo(currentWeather);
    }

    private void UpdateWeatherInfo()
    {
        if (titleText != null)
            titleText.text = GetWeatherTitle();

        if (descriptionText != null)
            descriptionText.text = GetWeatherDescription();

        // Simuler des données réalistes
        float temperature = SimulateTemperature();
        int humidity = SimulateHumidity();
        float wind = SimulateWind();

        if (temperatureText != null)
            temperatureText.text = $"{temperature:F1}°C";

        if (humidityText != null)
            humidityText.text = $"{humidity}%";

        if (windText != null)
            windText.text = $"{wind:F1} km/h";
    }

    private string GetWeatherTitle()
    {
        switch (currentWeather)
        {
            case WeatherType.Sunny: return "Ensoleillé";
            case WeatherType.Cloudy: return "Nuageux";
            case WeatherType.Rainy: return "Pluvieux";
            case WeatherType.Storm: return "Orageux";
            case WeatherType.Snow: return "Neigeux";
            default: return "Météo inconnue";
        }
    }

    private string GetWeatherDescription()
    {
        switch (currentWeather)
        {
            case WeatherType.Sunny:
                return "Ciel dégagé, luminosité maximale. Conditions idéales pour les activités extérieures.";
            case WeatherType.Cloudy:
                return "Ciel couvert, lumière diffuse. Température douce, humidité modérée.";
            case WeatherType.Rainy:
                return "Précipitations en cours. Humidité élevée, visibilité réduite.";
            case WeatherType.Storm:
                return "Conditions violentes. Fortes précipitations, rafales de vent, risque d'éclairs.";
            case WeatherType.Snow:
                return "Chutes de neige. Températures négatives, accumulation au sol possible.";
            default:
                return "Données non disponibles";
        }
    }

    private float SimulateTemperature()
    {
        float variation = Random.Range(-temperatureVariation, temperatureVariation);

        switch (currentWeather)
        {
            case WeatherType.Sunny: return baseTemperature + variation + 5f;
            case WeatherType.Cloudy: return baseTemperature + variation;
            case WeatherType.Rainy: return baseTemperature + variation - 3f;
            case WeatherType.Storm: return baseTemperature + variation - 5f;
            case WeatherType.Snow: return baseTemperature + variation - 15f;
            default: return baseTemperature;
        }
    }

    private int SimulateHumidity()
    {
        switch (currentWeather)
        {
            case WeatherType.Sunny: return Random.Range(30, 50);
            case WeatherType.Cloudy: return Random.Range(50, 70);
            case WeatherType.Rainy: return Random.Range(80, 95);
            case WeatherType.Storm: return Random.Range(85, 100);
            case WeatherType.Snow: return Random.Range(70, 90);
            default: return 50;
        }
    }

    private float SimulateWind()
    {
        switch (currentWeather)
        {
            case WeatherType.Sunny: return Random.Range(5f, 15f);
            case WeatherType.Cloudy: return Random.Range(10f, 25f);
            case WeatherType.Rainy: return Random.Range(20f, 40f);
            case WeatherType.Storm: return Random.Range(50f, 80f);
            case WeatherType.Snow: return Random.Range(15f, 30f);
            default: return 10f;
        }
    }
}
