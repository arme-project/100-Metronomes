using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioInputDetector : MonoBehaviour
{
    private AudioClip microphoneClip;
    private string microphoneName;
    private bool isInitialized = false;
    private const int FREQUENCY = 44100;
    private const int BUFFER_SIZE = 1024;
    private const float BASE_THRESHOLD = 0.07f;  // Lower threshold for better sensitivity
    private float MIN_INTERVAL = 0.2f;          // Shortened to ~300 BPM max (0.2s = 5Hz)
    private const int NOISE_SAMPLE_SIZE = 30;
    private const float SPIKE_THRESHOLD = 1.0f;  // More sensitive

    private float lastClapTime = 0f;
    private Queue<float> noiseHistory;
    private float adaptiveThreshold;
    private float[] previousSamples;
    private float previousMaxAmplitude = 0f;

    // Optimized fields
    private bool UseFrequencyAnalysis = true;
    private float[] spectrum = new float[256];
    private float[] amplitudeHistory = new float[5];
    private int amplitudeHistoryIndex = 0;
    private List<float> interClapIntervals = new List<float>();

    public static AudioInputDetector Instance { get; private set; }

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

    private void InitializeDetector()
    {
        noiseHistory = new Queue<float>();
        adaptiveThreshold = BASE_THRESHOLD;
        previousSamples = new float[BUFFER_SIZE];

        // Initialize amplitude history
        for (int i = 0; i < amplitudeHistory.Length; i++)
            amplitudeHistory[i] = 0f;

        if (Microphone.devices.Length > 0)
        {
            microphoneName = Microphone.devices[0];
            microphoneClip = Microphone.Start(microphoneName, true, 1, FREQUENCY);

            while (!(Microphone.GetPosition(microphoneName) > 0)) { }

            isInitialized = true;
            Debug.Log($"High-tempo clap detector initialized: {microphoneName}");

            StartCoroutine(CalibrateNoiseFloor());
        }
        else
        {
            Debug.LogError("No microphone found!");
        }
    }

    private IEnumerator CalibrateNoiseFloor()
    {
        Debug.Log("Starting noise floor calibration...");

        // Wait for metronomes to start playing
        yield return new WaitForSeconds(1.0f);

        // Sample noise levels more frequently
        for (int i = 0; i < 20; i++)
        {
            UpdateNoiseFloor();
            yield return new WaitForSeconds(0.05f);
        }

        Debug.Log($"Noise floor calibration complete. Threshold: {adaptiveThreshold}");
    }

    private void UpdateNoiseFloor()
    {
        if (!isInitialized) return;

        float[] samples = new float[BUFFER_SIZE];
        int position = Microphone.GetPosition(microphoneName);

        if (position < BUFFER_SIZE) return;

        microphoneClip.GetData(samples, position - BUFFER_SIZE);
        float currentMax = 0f;

        // Calculate RMS value for more stable noise measurement
        float rms = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            rms += samples[i] * samples[i];
            float absoluteValue = Mathf.Abs(samples[i]);
            if (absoluteValue > currentMax)
            {
                currentMax = absoluteValue;
            }
        }
        rms = Mathf.Sqrt(rms / BUFFER_SIZE);

        // Update noise history using RMS value
        noiseHistory.Enqueue(rms);
        if (noiseHistory.Count > NOISE_SAMPLE_SIZE)
        {
            noiseHistory.Dequeue();
        }

        // Calculate new adaptive threshold
        if (noiseHistory.Count >= NOISE_SAMPLE_SIZE)
        {
            float avgNoise = 0f;
            foreach (float noise in noiseHistory)
            {
                avgNoise += noise;
            }
            avgNoise /= NOISE_SAMPLE_SIZE;

            // Set threshold relative to noise floor, moderate multiplier
            adaptiveThreshold = Mathf.Max(BASE_THRESHOLD, avgNoise * 3.0f);
        }
    }

    void Update()
    {
        if (!isInitialized) return;

        float[] samples = new float[BUFFER_SIZE];
        int position = Microphone.GetPosition(microphoneName);

        if (position < BUFFER_SIZE) return;

        microphoneClip.GetData(samples, position - BUFFER_SIZE);
        float maxAmplitude = 0f;

        // Use overlapping windows with more aggressive spike detection
        for (int i = 0; i < samples.Length - 128; i += 64)
        {
            float windowMax = 0f;
            for (int j = 0; j < 128; j++)
            {
                float absoluteValue = Mathf.Abs(samples[i + j]);
                if (absoluteValue > windowMax)
                {
                    windowMax = absoluteValue;
                }
            }
            if (windowMax > maxAmplitude)
            {
                maxAmplitude = windowMax;
            }
        }

        // Update noise floor less frequently during normal operation
        if (Time.frameCount % 60 == 0)
        {
            UpdateNoiseFloor();
        }

        // Shift in new amplitude to history
        amplitudeHistory[amplitudeHistoryIndex] = maxAmplitude;
        amplitudeHistoryIndex = (amplitudeHistoryIndex + 1) % amplitudeHistory.Length;

        // Dynamically adjust MIN_INTERVAL based on rhythm
        if (interClapIntervals.Count >= 2)
        {
            // Calculate average interval from recent claps
            float sum = 0;
            foreach (float interval in interClapIntervals)
                sum += interval;
            float avgInterval = sum / interClapIntervals.Count;

            // Set minimum interval to allow detection slightly before expected clap
            // This helps with faster tempos
            MIN_INTERVAL = Mathf.Min(0.2f, Mathf.Max(0.1f, avgInterval * 0.4f));
        }

        // Basic amplitude check - must exceed noise threshold
        bool amplitudeCheck = maxAmplitude > adaptiveThreshold * 0.75f;

        // Timing check - must be at least MIN_INTERVAL since last clap
        bool timingCheck = Time.time - lastClapTime > MIN_INTERVAL;

        // Short-term increase check - must be a spike compared to recent samples
        bool spikeCheck = maxAmplitude > previousMaxAmplitude * SPIKE_THRESHOLD;

        // If basic checks pass, do more detailed analysis
        if (amplitudeCheck && timingCheck)
        {
            bool isClap = false;

            // Strong signal - definitely a clap
            if (maxAmplitude > adaptiveThreshold * 1.5f && spikeCheck)
            {
                isClap = true;
            }
            // Medium signal - check frequency profile
            else if (maxAmplitude > adaptiveThreshold && UseFrequencyAnalysis)
            {
                AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

                float midFreqEnergy = 0;
                for (int i = 20; i < 80; i++) midFreqEnergy += spectrum[i];

                // Claps have distinctive mid-frequency energy
                if (midFreqEnergy > 0.005f)
                {
                    isClap = true;
                }
            }

            if (isClap)
            {
                // Update inter-clap intervals for rhythm tracking
                float currentInterval = Time.time - lastClapTime;
                if (lastClapTime > 0 && currentInterval < 2.0f) // Only store reasonable intervals
                {
                    interClapIntervals.Add(currentInterval);
                    if (interClapIntervals.Count > 8)
                        interClapIntervals.RemoveAt(0);
                }

                lastClapTime = Time.time;
                Debug.Log($"CLAP DETECTED! Amplitude: {maxAmplitude:F3}, Threshold: {adaptiveThreshold:F3}");
            }
        }

        previousMaxAmplitude = maxAmplitude;
        System.Array.Copy(samples, previousSamples, BUFFER_SIZE);
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
}