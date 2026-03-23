using UnityEngine;
using Unity.XR.CoreUtils;

/// <summary>
/// Interface utilisateur spatialisée pour afficher les infos météo en VR
/// L'UI apparaît quand le joueur s'approche de la plante
/// </summary>
public class VRWeatherUI : MonoBehaviour
{
    [Header("Pan UI")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private float showDistance = 2f;
    [SerializeField] private float hideDistance = 4f;

    [Header("Contenu UI")]
    [SerializeField] private TMPro.TextMeshProUGUI weatherTitle;
    [SerializeField] private TMPro.TextMeshProUGUI weatherDescription;
    [SerializeField] private UnityEngine.UI.Image weatherIcon;

    [Header("Icônes")]
    [SerializeField] private Sprite sunnyIcon;
    [SerializeField] private Sprite cloudyIcon;
    [SerializeField] private Sprite rainyIcon;
    [SerializeField] private Sprite stormIcon;
    [SerializeField] private Sprite snowIcon;

    [Header("Animation")]
    [SerializeField] private float animationSpeed = 2f;
    [SerializeField] private Vector3 hiddenPosition = new Vector3(0, -0.5f, 0);
    [SerializeField] private Vector3 shownPosition = Vector3.zero;

    private bool isVisible = false;
    private bool isAnimating = false;
    private Transform playerCamera;

    [Header("Données météo")]
    [SerializeField] private string[] weatherTitles = new string[]
    {
        "Ensoleillé",
        "Nuageux",
        "Pluvieux",
        "Orageux",
        "Neigeux"
    };

    [SerializeField] private string[] weatherInfoDescriptions = new string[]
    {
        "Température agréable\nUV modérés\nParfait pour une sortie!",
        "Temps doux\nHumidité normale\nIdéal pour les activités extérieures.",
        "Pluie légère à modérée\nPrenez un parapluie!",
        "Fortes pluies\nVent intense\nRestez à l'intérieur!",
        "Températures froides\nNeige accumulée\nHabillez-vous chaudement!"
    };

    private void Start()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false);

        var xrOrigin = FindObjectOfType<XROrigin>();
        if (xrOrigin != null && xrOrigin.Camera != null)
            playerCamera = xrOrigin.Camera.transform;
        else
            playerCamera = Camera.main?.transform;
    }

    private void Update()
    {
        if (playerCamera != null && uiPanel != null)
        {
            float distance = Vector3.Distance(playerCamera.position, transform.position);

            if (distance < showDistance && !isVisible)
            {
                ShowUI();
            }
            else if (distance > hideDistance && isVisible)
            {
                HideUI();
            }

            if (isVisible)
            {
                Vector3 directionToPlayer = playerCamera.position - uiPanel.transform.position;
                directionToPlayer.y = 0;
                if (directionToPlayer.sqrMagnitude > 0.001f)
                    uiPanel.transform.rotation = Quaternion.LookRotation(-directionToPlayer);
            }
        }
    }

    public void UpdateWeatherUI(WeatherType weather)
    {
        int index = (int)weather;

        if (weatherTitle != null && index >= 0 && index < weatherTitles.Length)
            weatherTitle.text = weatherTitles[index];

        if (weatherDescription != null && index >= 0 && index < weatherInfoDescriptions.Length)
            weatherDescription.text = weatherInfoDescriptions[index];

        if (weatherIcon != null)
            weatherIcon.sprite = GetWeatherIcon(weather);
    }

    private Sprite GetWeatherIcon(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.Sunny: return sunnyIcon;
            case WeatherType.Cloudy: return cloudyIcon;
            case WeatherType.Rainy: return rainyIcon;
            case WeatherType.Storm: return stormIcon;
            case WeatherType.Snow: return snowIcon;
            default: return sunnyIcon;
        }
    }

    public void ShowUI()
    {
        if (!isVisible && !isAnimating)
        {
            StartCoroutine(AnimateUI(true));
        }
    }

    public void HideUI()
    {
        if (isVisible && !isAnimating)
        {
            StartCoroutine(AnimateUI(false));
        }
    }

    private System.Collections.IEnumerator AnimateUI(bool show)
    {
        isAnimating = true;

        if (uiPanel != null)
            uiPanel.SetActive(true);

        Vector3 startPos = show ? hiddenPosition : shownPosition;
        Vector3 targetPos = show ? shownPosition : hiddenPosition;
        float timer = 0f;

        while (timer < 1f)
        {
            timer += Time.deltaTime * animationSpeed;
            uiPanel.transform.localPosition = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, timer));
            yield return null;
        }

        uiPanel.transform.localPosition = targetPos;
        isVisible = show;
        isAnimating = false;

        if (!show && uiPanel != null)
            uiPanel.SetActive(false);
    }

    public void ForceShow(WeatherType weather)
    {
        UpdateWeatherUI(weather);
        ShowUI();
    }

    public void ForceHide()
    {
        HideUI();
    }
}
