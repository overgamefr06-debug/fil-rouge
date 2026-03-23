using UnityEngine;
using UnityEngine.UI;

public class WeatherSelectionUI : MonoBehaviour
{
    [Header("Weather Buttons")]
    public Button btnSunny;
    public Button btnCloudy;
    public Button btnRainy;
    public Button btnStorm;
    public Button btnSnow;

    private void Start()
    {
        if (btnSunny != null) btnSunny.onClick.AddListener(() => ChangeWeather(WeatherType.Sunny));
        if (btnCloudy != null) btnCloudy.onClick.AddListener(() => ChangeWeather(WeatherType.Cloudy));
        if (btnRainy != null) btnRainy.onClick.AddListener(() => ChangeWeather(WeatherType.Rainy));
        if (btnStorm != null) btnStorm.onClick.AddListener(() => ChangeWeather(WeatherType.Storm));
        if (btnSnow != null) btnSnow.onClick.AddListener(() => ChangeWeather(WeatherType.Snow));
    }

    private void ChangeWeather(WeatherType type)
    {
        if (WeatherSystem.Instance != null)
        {
            WeatherSystem.Instance.ForceWeatherChange(type);
            Debug.Log($"Météo changée manuellement via UI : {type}");
        }

        var vrUI = GetComponent<VRWeatherUI>();
        if (vrUI != null) vrUI.UpdateWeatherUI(type);

        var infoPanel = GetComponent<WeatherInfoPanel>();
        if (infoPanel != null) infoPanel.ShowWeatherInfo(type);
    }
}
