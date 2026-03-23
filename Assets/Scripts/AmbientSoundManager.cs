using UnityEngine;

/// <summary>
/// Gestionnaire des sons ambiants spatialisés selon la météo
/// Utilise l'audio 3D pour une immersion maximale en VR
/// </summary>
public class AmbientSoundManager : MonoBehaviour
{
    [Header("Sources audio")]
    [SerializeField] private AudioSource weatherAudioSource;
    [SerializeField] private AudioSource environmentAudioSource;
    [SerializeField] private AudioSource natureAudioSource;

    [Header("Clips audio")]
    [SerializeField] private AudioClip sunnyAmbience;
    [SerializeField] private AudioClip rainAmbience;
    [SerializeField] private AudioClip stormAmbience;
    [SerializeField] private AudioClip windAmbience;
    [SerializeField] private AudioClip snowAmbience;

    [Header("Paramètres")]
    [SerializeField] private float baseVolume = 0.5f;
    [SerializeField] private float transitionSpeed = 1f;
    [SerializeField] private bool useSpatialAudio = true;

    [Header("Sons ponctuels")]
    [SerializeField] private AudioClip[] thunderClaps;
    [SerializeField] private AudioClip[] birdCalls;
    [SerializeField] private float randomSoundInterval = 10f;

    private WeatherType currentWeather;
    private float targetVolume;
    private Coroutine randomSoundCoroutine;

    private void Start()
    {
        if (weatherAudioSource != null)
        {
            weatherAudioSource.loop = true;
            weatherAudioSource.spatialBlend = useSpatialAudio ? 1f : 0f;
        }

        if (environmentAudioSource != null)
        {
            environmentAudioSource.loop = true;
            environmentAudioSource.spatialBlend = useSpatialAudio ? 1f : 0f;
        }

        if (natureAudioSource != null)
        {
            natureAudioSource.loop = true;
            natureAudioSource.spatialBlend = useSpatialAudio ? 1f : 0f;
        }

        if (randomSoundCoroutine == null)
            randomSoundCoroutine = StartCoroutine(PlayRandomSounds());
    }

    public void SetWeather(WeatherType weather)
    {
        currentWeather = weather;
        UpdateAmbience();
    }

    private void UpdateAmbience()
    {
        AudioClip targetClip = GetWeatherAmbience();

        if (weatherAudioSource != null && targetClip != null)
        {
            if (weatherAudioSource.clip != targetClip)
            {
                StartCoroutine(TransitionAudioClip(weatherAudioSource, targetClip));
            }
        }

        // Ajuster les volumes
        UpdateVolumes();
    }

    private AudioClip GetWeatherAmbience()
    {
        switch (currentWeather)
        {
            case WeatherType.Sunny: return sunnyAmbience;
            case WeatherType.Cloudy: return windAmbience;
            case WeatherType.Rainy: return rainAmbience;
            case WeatherType.Storm: return stormAmbience;
            case WeatherType.Snow: return snowAmbience;
            default: return sunnyAmbience;
        }
    }

    private void UpdateVolumes()
    {
        switch (currentWeather)
        {
            case WeatherType.Sunny:
                targetVolume = baseVolume * 0.6f;
                break;
            case WeatherType.Cloudy:
                targetVolume = baseVolume * 0.5f;
                break;
            case WeatherType.Rainy:
                targetVolume = baseVolume * 0.8f;
                break;
            case WeatherType.Storm:
                targetVolume = baseVolume * 1f;
                break;
            case WeatherType.Snow:
                targetVolume = baseVolume * 0.4f;
                break;
        }

        if (weatherAudioSource != null)
        {
            StartCoroutine(TransitionVolume(weatherAudioSource, targetVolume));
        }
    }

    private System.Collections.IEnumerator TransitionAudioClip(AudioSource source, AudioClip newClip)
    {
        if (source == null) yield break;

        // Fade out
        float startVolume = source.volume;
        float timer = 0f;

        while (timer < transitionSpeed)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, timer / transitionSpeed);
            yield return null;
        }

        // Changer le clip
        source.clip = newClip;
        source.Play();

        // Fade in
        timer = 0f;
        while (timer < transitionSpeed)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, timer / transitionSpeed);
            yield return null;
        }

        source.volume = targetVolume;
    }

    private System.Collections.IEnumerator TransitionVolume(AudioSource source, float targetVol)
    {
        if (source == null) yield break;

        float startVolume = source.volume;
        float timer = 0f;

        while (timer < transitionSpeed)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVol, timer / transitionSpeed);
            yield return null;
        }

        source.volume = targetVol;
    }

    private System.Collections.IEnumerator PlayRandomSounds()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(randomSoundInterval * 0.5f, randomSoundInterval * 1.5f));

            PlayRandomWeatherSound();
        }
    }

    private void PlayRandomWeatherSound()
    {
        AudioClip clip = null;

        switch (currentWeather)
        {
            case WeatherType.Sunny:
                if (birdCalls != null && birdCalls.Length > 0)
                    clip = birdCalls[Random.Range(0, birdCalls.Length)];
                break;
            case WeatherType.Storm:
                if (thunderClaps != null && thunderClaps.Length > 0 && Random.value > 0.7f)
                    clip = thunderClaps[Random.Range(0, thunderClaps.Length)];
                break;
        }

        if (clip != null && environmentAudioSource != null)
        {
            environmentAudioSource.PlayOneShot(clip);
        }
    }

    public void PlayThunder()
    {
        if (thunderClaps != null && thunderClaps.Length > 0 && environmentAudioSource != null)
        {
            AudioClip clip = thunderClaps[Random.Range(0, thunderClaps.Length)];
            environmentAudioSource.PlayOneShot(clip, 1.2f);
        }
    }

    public void SetMasterVolume(float volume)
    {
        baseVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
}
