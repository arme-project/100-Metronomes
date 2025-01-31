using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationAudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip defaultClick;
    //public AudioClip[] audioClips; // 包含多个音频片段的数组
    public float clickpitch = 1.0f;
    private AudioSource audioSource;
    private Animator animator;
    private bool audioPlayed = true; // 确保音频只播放一次
    private float previousNormalizedTime = 0f; // 记录上一次的 normalizedTime

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        animator = GetComponent<Animator>();
        audioSource.pitch = clickpitch;

        // 从数组中随机选择一个音频片段
        //if (audioClips.Length > 0)
        //{
        //    int randomIndex = Random.Range(0, audioClips.Length);
        //    audioSource.clip = audioClips[randomIndex];
        //}

        if (defaultClick == null)
        {
            defaultClick = Resources.Load<AudioClip>("click_0");
        }

        if (defaultClick != null) 
        {
            audioSource.clip = defaultClick;
        }
            
    }

    void Update()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float currentNormalizedTime = stateInfo.normalizedTime;

        // 检测动画循环的开始（normalizedTime 从 1 回到 0）
        if (currentNormalizedTime < previousNormalizedTime)
        {
            audioPlayed = false; // 重置 audioPlayed
        }

        // 在动画归一化时间达到0.8时播放音频
        if (currentNormalizedTime >= 0.99f && !audioPlayed)
        {
            PlayAudio();
            audioPlayed = true; // 确保音频只播放一次
        }

        // 更新 previousNormalizedTime
        previousNormalizedTime = currentNormalizedTime;
    }

    public void PlayAudio()
    {
        if (audioSource != null && audioSource.clip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
