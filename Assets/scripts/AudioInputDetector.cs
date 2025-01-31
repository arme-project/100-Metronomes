using UnityEngine;
using System.Collections;
public class AudioInputDetector : MonoBehaviour
{
    private AudioClip microphoneClip;
    private string microphoneName;
    private bool isInitialized = false;
    private const int FREQUENCY = 44100;
    private const float DETECTION_THRESHOLD = 0.06f;
    private const float MIN_INTERVAL = 0.25f;
    private float lastClapTime = 0f;
    public static AudioInputDetector Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMicrophone();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        // to show microphone devices
        Debug.Log("Available microphones:");
        foreach (string device in Microphone.devices)
        {
            Debug.Log("Microphone found: " + device);
        }
    }
    private void InitializeMicrophone()
    {
        if (Microphone.devices.Length > 0)
        {
            microphoneName = Microphone.devices[0];
            microphoneClip = Microphone.Start(microphoneName, true, 1, FREQUENCY);
            while (!(Microphone.GetPosition(microphoneName) > 0)) { }
            isInitialized = true;
            Debug.Log("Microphone initialized: " + microphoneName);
            Debug.Log("Recording started with frequency: " + FREQUENCY);
        }
        else
        {
            Debug.LogError("No microphone found! Please check your microphone connection.");
        }
    }
    private void OnDisable()
    {
        if (isInitialized)
        {
            Microphone.End(microphoneName);
            Debug.Log("Microphone recording ended");
        }
    }
    void Update()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Microphone not initialized!");
            return;
        }
        float[] samples = new float[256];
        int position = Microphone.GetPosition(microphoneName);
        if (position < samples.Length) return;
        microphoneClip.GetData(samples, position - samples.Length);
        float maxAmplitude = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            float absoluteValue = Mathf.Abs(samples[i]);
            if (absoluteValue > maxAmplitude)
            {
                maxAmplitude = absoluteValue;
            }
        }
        if (maxAmplitude > 0.001f)
        {
            Debug.Log("Current amplitude: " + maxAmplitude);
        }
        if (maxAmplitude > DETECTION_THRESHOLD && Time.time - lastClapTime > MIN_INTERVAL)
        {
            lastClapTime = Time.time;
            Debug.Log("CLAP DETECTED! Amplitude: " + maxAmplitude);
        }
    }
    public bool WasClapped()
    {
        if (!isInitialized) return false;
        return Time.time - lastClapTime < Time.deltaTime;
    }
}