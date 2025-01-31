using UnityEngine;

public class MetronomeSpatialAudio : MonoBehaviour
{
    private AudioSource audioSource;
    private Transform mainCamera;

    [Header("Distance Settings")]
    public float maxDistance = 100f;
    public float minDistance = 0.1f;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float baseVolume = 1f;
    public float basePitch = 1f;
    public float maxPitchIncrease = 0.15f;
    public float volumeDropoff = 2f;

    private void Start()
    {
        Invoke("InitializeAudioSource", 0.1f);
    }

    private void InitializeAudioSource()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length > 0)
        {
            audioSource = sources[sources.Length - 1];
            mainCamera = Camera.main.transform;
            ConfigureAudioSource();
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] No AudioSource found!");
        }
    }

    private void ConfigureAudioSource()
    {
        audioSource.spatialBlend = 1f;
        audioSource.spatialize = true;
        audioSource.spatializePostEffects = true;

        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = maxDistance;
        audioSource.minDistance = minDistance;
        audioSource.spread = 60f;
        audioSource.dopplerLevel = 0f;

        // Set the volume directly
        audioSource.volume = baseVolume;

        Debug.Log($"[{gameObject.name}] Audio Source Configured - Volume: {audioSource.volume}");
    }

    private void Update()
    {
        if (!mainCamera || !audioSource) return;

        float distance = Vector3.Distance(transform.position, mainCamera.position);
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        float normalizedDistance = (distance - minDistance) / (maxDistance - minDistance);

        // Set volume directly from baseVolume
        float distanceVolume = Mathf.Lerp(1f, 0.1f, Mathf.Pow(normalizedDistance, volumeDropoff));
        audioSource.volume = baseVolume * distanceVolume;

        float distancePitch = 1f + (normalizedDistance * maxPitchIncrease);
        audioSource.pitch = basePitch * distancePitch;

        Vector3 directionToCamera = mainCamera.position - transform.position;
        Vector3 localDirection = mainCamera.InverseTransformDirection(directionToCamera);
        float stereoPosition = Mathf.Clamp(localDirection.x / maxDistance, -1f, 1f);
        audioSource.panStereo = -stereoPosition * (1 - normalizedDistance);

        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[{gameObject.name}] Distance: {distance:F2}m, Volume: {audioSource.volume:F2}, " +
                     $"Pitch: {audioSource.pitch:F2}, Pan: {audioSource.panStereo:F2}");
        }
    }
}