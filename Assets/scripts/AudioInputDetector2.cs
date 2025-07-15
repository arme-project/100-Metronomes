using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[System.Serializable]
public class AudioVisualizationSettings
{
    [Header("Audio Bar Settings")]
    public bool showAudioBar = true;
    public Color audioBarColor = Color.green;
    public Color audioBarPeakColor = Color.red;

    [Header("Waveform Settings")]
    public bool showWaveform = true;
    public int waveformResolution = 256;
    public float waveformHeight = 100f;
    public Color waveformColor = Color.cyan;
    public Color waveformBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
}

public class AudioInputDetector2 : MonoBehaviour
{
    [Header("Microphone Settings")]
    [SerializeField] private int selectedMicrophoneIndex = 0;
    [SerializeField] private string[] availableMicrophones;

    [Header("Audio Visualization")]
    [SerializeField] private AudioVisualizationSettings visualizationSettings = new AudioVisualizationSettings();

    [Header("Detection Settings")]
    [SerializeField] private float detectionThreshold = 0.005f;  // Threshold to trigger detection
    [SerializeField] private float releaseThreshold = 0.001f;   // Must go below this to reset
    [SerializeField] private float minTimeBetweenClaps = 0.2f;  // Increased default time
    [SerializeField] private float peakWindowTime = 0.05f;      // Time window to find peak
    [SerializeField] private int smoothingFrames = 5;           // Number of frames to smooth over

    [Header("Visual Feedback")]
    public GameObject clapDetectedDisplay;

    [Header("Debug Info")]
    [SerializeField] private float currentAmplitude = 0f;
    [SerializeField] private float smoothedAmplitude = 0f;
    [SerializeField] private float timeSinceLastClap = 0f;
    [SerializeField] private bool isArmed = true;
    [SerializeField] private string detectionState = "WAITING";

    // Audio visualization data
    private float[] waveformData;
    private float audioBarValue = 0f;
    private float audioBarPeak = 0f;
    private float peakDecayRate = 0.95f;

    // Audio system
    private AudioClip microphoneClip;
    private string microphoneName;
    private bool isInitialized = false;
    private const int FREQUENCY = 44100;
    private const int BUFFER_SIZE = 2046;

    // Detection tracking
    private float lastClapTime = -10f;  // Start with old time so first clap works
    private bool wasAboveThreshold = false;
    public int lastProcessedPosition = 0;

    // Amplitude smoothing
    private Queue<float> amplitudeHistory = new Queue<float>();
    private float peakAmplitude = 0f;
    private float peakTime = 0f;

    public Action<float> clapDetectedEvent;

    // State machine for detection
    private enum DetectionStates
    {
        WAITING,        // Waiting for signal to go above threshold
        PEAK_SEARCH,    // Above threshold, looking for peak
        COOLDOWN        // Found peak, waiting for signal to drop
    }
    private DetectionStates currentState = DetectionStates.WAITING;

    public static AudioInputDetector2 Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDetector();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnValidate()
    {
        // Update available microphones in the inspector
        RefreshMicrophoneList();

        // Ensure waveform data array is properly sized
        if (visualizationSettings.waveformResolution > 0)
        {
            waveformData = new float[visualizationSettings.waveformResolution];
        }

        // Clamp threshold to reasonable values
        detectionThreshold = Mathf.Clamp(detectionThreshold, 0.001f, 1f);
        releaseThreshold = Mathf.Clamp(releaseThreshold, 0.0001f, detectionThreshold * 0.8f);
        minTimeBetweenClaps = Mathf.Max(0.1f, minTimeBetweenClaps);
        peakWindowTime = Mathf.Clamp(peakWindowTime, 0.02f, 0.2f);
        smoothingFrames = Mathf.Max(1, smoothingFrames);
    }

    private void RefreshMicrophoneList()
    {
        availableMicrophones = Microphone.devices;
        if (availableMicrophones.Length == 0)
        {
            availableMicrophones = new string[] { "No microphones found" };
        }

        selectedMicrophoneIndex = Mathf.Clamp(selectedMicrophoneIndex, 0,
            Mathf.Max(0, availableMicrophones.Length - 1));
    }

    private void InitializeDetector()
    {
        RefreshMicrophoneList();

        waveformData = new float[visualizationSettings.waveformResolution];

        if (Microphone.devices.Length > 0)
        {
            microphoneName = Microphone.devices[selectedMicrophoneIndex];
            microphoneClip = Microphone.Start(microphoneName, true, 1, FREQUENCY);

            // Wait for microphone to start
            while (!(Microphone.GetPosition(microphoneName) > 0)) { }

            isInitialized = true;
            Debug.Log($"Simple clap detector initialized: {microphoneName}");
        }
        else
        {
            Debug.LogError("No microphone found!");
        }
    }

    public void ChangeMicrophone(int newIndex)
    {
        if (newIndex < 0 || newIndex >= Microphone.devices.Length)
            return;

        if (isInitialized && microphoneName != null)
        {
            Microphone.End(microphoneName);
        }

        selectedMicrophoneIndex = newIndex;
        isInitialized = false;
        InitializeDetector();
    }

    void Update()
    {
        if (!isInitialized) return;

        int currentPosition = Microphone.GetPosition(microphoneName);

        // Handle microphone position wrap-around
        if (currentPosition < lastProcessedPosition)
        {
            lastProcessedPosition = 0;
        }

        // Only process if we have new data
        if (currentPosition <= lastProcessedPosition) return;

        // Calculate how many new samples we have
        int samplesToProcess = currentPosition - lastProcessedPosition;
        samplesToProcess = Mathf.Min(samplesToProcess, BUFFER_SIZE * 2);

        if (samplesToProcess <= 0) return;

        // Get the new audio data
        float[] samples = new float[samplesToProcess];
        microphoneClip.GetData(samples, lastProcessedPosition);

        // Update waveform data for visualization
        UpdateWaveformData(samples);

        // Find the maximum amplitude in the new samples
        float maxAmplitude = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            float absoluteValue = Mathf.Abs(samples[i]);
            if (absoluteValue > maxAmplitude)
            {
                maxAmplitude = absoluteValue;
            }
        }

        // Add to amplitude history for smoothing
        amplitudeHistory.Enqueue(maxAmplitude);
        while (amplitudeHistory.Count > smoothingFrames)
        {
            amplitudeHistory.Dequeue();
        }

        // Calculate smoothed amplitude (average of recent frames)
        smoothedAmplitude = amplitudeHistory.Count > 0 ? amplitudeHistory.Average() : 0f;

        // Update visualization values
        audioBarValue = maxAmplitude;
        if (maxAmplitude > audioBarPeak)
        {
            audioBarPeak = maxAmplitude;
        }
        else
        {
            audioBarPeak *= peakDecayRate;
        }

        // Update debug info
        currentAmplitude = maxAmplitude;
        timeSinceLastClap = Time.time - lastClapTime;

        // State machine for detection
        switch (currentState)
        {
            case DetectionStates.WAITING:
                detectionState = "WAITING";
                isArmed = true;

                // Check if we can start detecting (enough time passed since last clap)
                if (smoothedAmplitude > detectionThreshold && timeSinceLastClap > minTimeBetweenClaps)
                {
                    currentState = DetectionStates.PEAK_SEARCH;
                    peakAmplitude = smoothedAmplitude;
                    peakTime = Time.time;
                    wasAboveThreshold = true;
                    Debug.Log($"Starting peak search. Initial amplitude: {smoothedAmplitude:F3}");
                }
                break;

            case DetectionStates.PEAK_SEARCH:
                detectionState = "PEAK_SEARCH";
                isArmed = false;

                // Update peak if we find a higher value
                if (smoothedAmplitude > peakAmplitude)
                {
                    peakAmplitude = smoothedAmplitude;
                    peakTime = Time.time;
                }

                // Check if we've been searching for peak too long or signal dropped significantly
                bool peakWindowExpired = Time.time - peakTime > peakWindowTime;
                bool signalDropped = smoothedAmplitude < peakAmplitude * 0.5f; // Signal dropped to half of peak

                if (peakWindowExpired || signalDropped)
                {
                    // We found our peak! Register the clap
                    lastClapTime = peakTime; // Use peak time, not current time
                    Debug.LogWarning($"CLAP DETECTED! Peak: {peakAmplitude:F3} at time offset: {(Time.time - peakTime):F3}s");
                    ShowDetected();
                    currentState = DetectionStates.COOLDOWN;
                    clapDetectedEvent?.Invoke(Time.time);
                }
                break;

            case DetectionStates.COOLDOWN:
                detectionState = "COOLDOWN";
                isArmed = false;

                // Wait for signal to drop below release threshold
                if (smoothedAmplitude < releaseThreshold)
                {
                    currentState = DetectionStates.WAITING;
                    wasAboveThreshold = false;
                    Debug.Log($"Released. Ready for next clap.");
                }
                break;
        }

        // Update our position tracker
        lastProcessedPosition = currentPosition;
    }

    private void UpdateWaveformData(float[] samples)
    {
        if (waveformData == null || waveformData.Length != visualizationSettings.waveformResolution)
        {
            waveformData = new float[visualizationSettings.waveformResolution];
        }

        int samplesPerPoint = samples.Length / visualizationSettings.waveformResolution;

        for (int i = 0; i < visualizationSettings.waveformResolution; i++)
        {
            float sum = 0;
            int startIndex = i * samplesPerPoint;

            for (int j = 0; j < samplesPerPoint && startIndex + j < samples.Length; j++)
            {
                sum += Mathf.Abs(samples[startIndex + j]);
            }

            waveformData[i] = sum / samplesPerPoint;
        }
    }

    private void ShowDetected()
    {
        if (clapDetectedDisplay != null)
        {
            clapDetectedDisplay.SetActive(true);
            Invoke("HideDetected", 0.1f);
        }
    }

    private void HideDetected()
    {
        if (clapDetectedDisplay != null)
        {
            clapDetectedDisplay.SetActive(false);
        }
    }

    public bool WasClapped()
    {
        if (!isInitialized) return false;
        return Time.time - lastClapTime < Time.deltaTime;
    }

    private void OnDisable()
    {
        if (isInitialized)
        {
            Microphone.End(microphoneName);
            Debug.Log("Microphone recording ended");
        }
    }

    // Public methods for accessing visualization data
    public float GetAudioBarValue()
    {
        return audioBarValue;
    }

    public float GetAudioBarPeak()
    {
        return audioBarPeak;
    }

    public float[] GetWaveformData()
    {
        return waveformData;
    }

    public AudioVisualizationSettings GetVisualizationSettings()
    {
        return visualizationSettings;
    }

    public float GetDetectionThreshold()
    {
        return detectionThreshold;
    }

    public float GetReleaseThreshold()
    {
        return releaseThreshold;
    }

    public bool IsArmed()
    {
        return isArmed;
    }
}