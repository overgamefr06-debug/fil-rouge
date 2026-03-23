using UnityEngine;
using System.Collections;

/// <summary>
/// Comportement de la plante virtuelle qui réagit à la météo
/// La plante change d'apparence, d'animation et de comportement selon la météo
/// </summary>
public class PlantBehavior : MonoBehaviour
{
    [Header("Transformations visuelles")]
    [SerializeField] private Transform plantRoot;
    [SerializeField] private Transform[] leaves;
    [SerializeField] private Transform stem;
    [SerializeField] private ParticleSystem weatherParticles;

    [Header("Matériaux par état")]
    [SerializeField] private Material sunnyMaterial;
    [SerializeField] private Material rainyMaterial;
    [SerializeField] private Material cloudyMaterial;
    [SerializeField] private Material stormMaterial;
    [SerializeField] private Material snowMaterial;

    [Header("Animations")]
    [SerializeField] private float sunnyMovementSpeed = 0.5f;
    [SerializeField] private float rainyMovementSpeed = 1.5f;
    [SerializeField] private float stormMovementSpeed = 3f;
    [SerializeField] private float snowMovementSpeed = 0.2f;
    [SerializeField] private float movementAmplitude = 0.1f;

    [Header("Informations météo")]
    [SerializeField] private bool showDetailedInfo = false;
    [SerializeField] private float observationDistance = 3f;
    [SerializeField] private string[] weatherDescriptions;

    private Renderer plantRenderer;
    private WeatherType currentWeather;
    private bool isBeingObserved = false;
    private float observationTimer = 0f;

    [Header("Audio")]
    [SerializeField] private AudioSource plantAudioSource;
    [SerializeField] private AudioClip sunnySound;
    [SerializeField] private AudioClip rainSound;
    [SerializeField] private AudioClip windSound;
    [SerializeField] private AudioClip[] ambientSounds;

    private System.Collections.Generic.Dictionary<Renderer, Material> originalMaterials;

    private void Start()
    {
        plantRenderer = GetComponentInChildren<Renderer>();
        if (plantRenderer == null)
            Debug.LogWarning("Pas de Renderer trouvé sur la plante!");

        // Sauvegarder les matériaux d'origine
        originalMaterials = new System.Collections.Generic.Dictionary<Renderer, Material>();
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allRenderers)
        {
            if (!(r is ParticleSystemRenderer) && r != null)
            {
                originalMaterials[r] = r.material;
            }
        }

        InitializeWeatherDescriptions();
    }

    private void InitializeWeatherDescriptions()
    {
        weatherDescriptions = new string[]
        {
            "Ensoleillé: Température agréable, UV modérés. Parfait pour une sortie!",
            "Nuageux: Temps doux, humidité normale. Idéal pour les activités extérieures.",
            "Pluvieux: Pluie légère à modérée. Prenez un parapluie!",
            "Orageux: Fortes pluies, vent intense. Restez à l'intérieur!",
            "Neigeux: Températures froides, neige accumulée. Habillez-vous chaudement!"
        };
    }

    public void OnWeatherChanged(WeatherType newWeather)
    {
        currentWeather = newWeather;
        UpdatePlantAppearance();
        UpdatePlantMovement();
        UpdateWeatherParticles();
        UpdateAudio();

        Debug.Log($"Plante: Météo changée vers {newWeather}");
    }

    private void UpdatePlantAppearance()
    {
        if (originalMaterials == null) return;

        bool isSnowing = (currentWeather == WeatherType.Snow && snowMaterial != null);

        foreach (var kvp in originalMaterials)
        {
            Renderer r = kvp.Key;
            Material originalMat = kvp.Value;

            if (r != null)
            {
                r.material = isSnowing ? snowMaterial : originalMat;
            }
        }
    }

    private Material GetWeatherMaterial()
    {
        switch (currentWeather)
        {
            case WeatherType.Sunny: return sunnyMaterial;
            case WeatherType.Rainy: return rainyMaterial;
            case WeatherType.Cloudy: return cloudyMaterial;
            case WeatherType.Storm: return stormMaterial;
            case WeatherType.Snow: return snowMaterial;
            default: return sunnyMaterial;
        }
    }

    private void UpdatePlantMovement()
    {
        // La vitesse de mouvement est gérée dans Update()
    }

    private void UpdateWeatherParticles()
    {
        if (weatherParticles != null)
        {
            var main = weatherParticles.main;

            switch (currentWeather)
            {
                case WeatherType.Sunny:
                    weatherParticles.gameObject.SetActive(false);
                    break;
                case WeatherType.Rainy:
                    weatherParticles.gameObject.SetActive(true);
                    main.startColor = new Color(0.6f, 0.7f, 1f);
                    break;
                case WeatherType.Storm:
                    weatherParticles.gameObject.SetActive(true);
                    main.startColor = new Color(0.4f, 0.5f, 0.8f);
                    main.startLifetime = 0.5f;
                    break;
                case WeatherType.Snow:
                    weatherParticles.gameObject.SetActive(true);
                    main.startColor = Color.white;
                    break;
            }
        }
    }

    private void UpdateAudio()
    {
        if (plantAudioSource == null)
        {
            plantAudioSource = GetComponent<AudioSource>();
        }

        if (plantAudioSource != null)
        {
            AudioClip targetClip = GetWeatherAudioClip();
            if (targetClip != null)
            {
                if (plantAudioSource.clip != targetClip)
                {
                    plantAudioSource.clip = targetClip;
                    plantAudioSource.loop = true;
                    plantAudioSource.Play();
                }
            }
            else
            {
                plantAudioSource.Stop();
                plantAudioSource.clip = null;
            }
        }
    }

    private AudioClip GetWeatherAudioClip()
    {
        switch (currentWeather)
        {
            case WeatherType.Sunny: return sunnySound;
            case WeatherType.Rainy: return rainSound;
            case WeatherType.Cloudy: return sunnySound;
            case WeatherType.Storm: return windSound;
            default: return null;
        }
    }

    private void Update()
    {
        AnimatePlant();
        CheckForObservation();
    }

    private void AnimatePlant()
    {
        if (plantRoot != null)
        {
            float speed = GetMovementSpeed();
            float time = Time.time * speed;

            // Effet de vent en tempête (réduit pour plus de réalisme)
            if (currentWeather == WeatherType.Storm)
            {
                float windTime = Time.time * 1.5f;
                // Oscillations minimisées sur l'axe X (avant-arrière) pour limiter l'effet "haut/bas"
                float windAngleX = Mathf.Sin(windTime) * 1f + Mathf.Sin(windTime * 0.7f) * 0.5f; 
                float windAngleZ = Mathf.Cos(windTime * 1.3f) * 1f;
                plantRoot.localRotation = Quaternion.Euler(windAngleX, 0f, windAngleZ);
            }
            else
            {
                // Retour graduel à la position droite
                plantRoot.localRotation = Quaternion.Lerp(plantRoot.localRotation, Quaternion.identity, Time.deltaTime * 3f);
            }

            // Mouvement oscillant des feuilles
            if (leaves != null && leaves.Length > 0)
            {
                foreach (var leaf in leaves)
                {
                    if (leaf != null)
                    {
                        leaf.localRotation = Quaternion.Euler(
                            Mathf.Sin(time) * movementAmplitude * 10f,
                            Mathf.Cos(time * 0.7f) * movementAmplitude * 5f,
                            Mathf.Sin(time * 1.3f) * movementAmplitude * 15f
                        );
                    }
                }
            }

            // Mouvement de la tige
            if (stem != null)
            {
                stem.localRotation = Quaternion.Euler(
                    Mathf.Sin(time * 0.5f) * movementAmplitude * 5f,
                    0f,
                    Mathf.Cos(time * 0.8f) * movementAmplitude * 3f
                );
            }
        }
    }

    private float GetMovementSpeed()
    {
        switch (currentWeather)
        {
            case WeatherType.Sunny: return sunnyMovementSpeed;
            case WeatherType.Rainy: return rainyMovementSpeed;
            case WeatherType.Storm: return stormMovementSpeed;
            case WeatherType.Snow: return snowMovementSpeed;
            default: return sunnyMovementSpeed;
        }
    }

    private void CheckForObservation()
    {
        // Vérifier si le joueur VR observe la plante
        var camera = Camera.main;
        if (camera != null)
        {
            float distance = Vector3.Distance(camera.transform.position, transform.position);
            bool isLooking = Vector3.Angle(camera.transform.forward, transform.position - camera.transform.position) < 30f;

            if (distance < observationDistance && isLooking)
            {
                if (!isBeingObserved)
                {
                    isBeingObserved = true;
                    OnPlayerStartedObserving();
                }
                observationTimer += Time.deltaTime;
            }
            else
            {
                if (isBeingObserved)
                {
                    isBeingObserved = false;
                    OnPlayerStoppedObserving();
                }
                observationTimer = 0f;
            }
        }
    }

    private void OnPlayerStartedObserving()
    {
        Debug.Log("Joueur commence à observer la plante");
        // Activer l'affichage des infos détaillées après un certain temps
        StartCoroutine(ShowDetailedInfoAfterDelay());
    }

    private void OnPlayerStoppedObserving()
    {
        Debug.Log("Joueur arrête d'observer la plante");
        showDetailedInfo = false;
    }

    private IEnumerator ShowDetailedInfoAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        if (isBeingObserved)
        {
            showDetailedInfo = true;
            Debug.Log($"Infos météo: {GetWeatherDescription()}");
        }
    }

    private string GetWeatherDescription()
    {
        int index = (int)currentWeather;
        if (index >= 0 && index < weatherDescriptions.Length)
            return weatherDescriptions[index];
        return "Information non disponible";
    }

    public void InteractWithPlant()
    {
        Debug.Log("Interaction avec la plante!");

        // Action spéciale lors de l'interaction directe
        if (plantAudioSource != null && ambientSounds != null && ambientSounds.Length > 0)
        {
            AudioClip randomSound = ambientSounds[Random.Range(0, ambientSounds.Length)];
            plantAudioSource.PlayOneShot(randomSound);
        }

        // Afficher immédiatement les informations
        showDetailedInfo = true;
        Debug.Log($"Météo actuelle: {currentWeather}\n{GetWeatherDescription()}");
    }

    private void OnGUI()
    {
        if (showDetailedInfo)
        {
            GUI.color = Color.white;
            GUI.Box(new Rect(10, 10, 400, 100), "🌱 Station Météo - Plante Virtuelle");
            GUI.Label(new Rect(20, 40, 380, 60), GetWeatherDescription());
        }
    }
}
