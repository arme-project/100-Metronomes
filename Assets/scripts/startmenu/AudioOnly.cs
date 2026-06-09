using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioOnly : MonoBehaviour
{
    public AudioClip audioClips;
    private AudioSource audioSource;
    private float defaultBpm = 120f;
    private float bpm;
    private bool isInvokingPlayAudio = false;

    [Tooltip("Multiplier on the guide tick speed. <1 ticks slightly slower, >1 faster. Keep in sync with Activate.guideSpeedMultiplier.")]
    public float guideSpeedMultiplier = 0.95f;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClips;
        audioSource.pitch = 2f;
        float WaitTime = Updateinterval();

        InvokeRepeating("PlayAudioClip", 0f, WaitTime);
        isInvokingPlayAudio = true;
    }

    // Update is called once per frame
    void Update()
    {
        int UseAudio = PlayerPrefs.GetInt("AudioGudance", 0);
        int Usevisual = PlayerPrefs.GetInt("VisualGudance", 0);
        if (Usevisual == 0 )
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

        }
        if (Usevisual == 1 && isInvokingPlayAudio) 
        {
            CancelInvoke("PlayAudioClip");
            isInvokingPlayAudio = false;
        }
        if (UseAudio == 0 && Usevisual == 0)
        {
            SceneManager.sceneLoaded += OnSceneLoadednone;
        }

    }
    void PlayAudioClip()
    {
        audioSource.Play();
    }
    public float Updateinterval()
    {
        bpm = PlayerPrefs.GetFloat("BPM", defaultBpm);
        float Speed =  defaultBpm / bpm;
        // Divide by the multiplier so a value < 1 lengthens the interval (slower ticks).
        return (Speed * 0.5f) / guideSpeedMultiplier;
    }
    void OnSceneLoadednone(Scene scene, LoadSceneMode mode)
    {
        // ��鳡�������Ƿ�Ϊ "Ensemble"
        if (scene.name == "Ensemble")
        {
            // ֹͣ�ظ����� PlayAudioClip
            CancelInvoke("PlayAudioClip");
            isInvokingPlayAudio = false;
        }
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ��鳡�������Ƿ�Ϊ "Ensemble"
        if (scene.name == "StartMenu")
        {
            float WaitTime = Updateinterval();
            InvokeRepeating("PlayAudioClip", 0f, WaitTime);
            Debug.Log("here");
            isInvokingPlayAudio = true;
        }
    }
}
