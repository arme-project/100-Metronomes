using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserAnimation : MonoBehaviour
{
    public AudioClip audioClips;
    private AudioSource audioSource;
    private Animator animator;
    public float clickpitch = 2.0f;
    public bool IsReversed = false;
    private float originalDuration = 0.5f; //原始时长（秒）
    private float targetDuration; // 目标播放时长

    private bool clapDetected = false;
    void Start()
    {
        AudioInputDetector2.Instance.clapDetectedEvent += clapDetectedEventSubscribe;
        animator = GetComponent<Animator>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClips;
        audioSource.pitch = clickpitch;
        if (animator == null)
        {
            Debug.LogError("Animator component not found on" + gameObject.name);
        }
    }

    public void clapDetectedEventSubscribe(float time)
    {
        clapDetected = true;
    }
    // Update is called once per frame
    void Update()
    {

        ChangeAnimationSpeed();
    }

    void ChangeAnimationSpeed()
    {
        //if (InputChecker.IsTouchBegan())
        if (clapDetected)
        {
            clapDetected = false;
            Debug.LogWarning("UserAnimation: Input detected, changing animation speed.");
            PlayAudio();
            UserTimeManager.Instance.AddTimestamp(Time.time);
            SetAnimationSpeedsign();
            if (UserTimeManager.Instance.GetsatampNum() > 2)
            {
                targetDuration = UserTimeManager.Instance.GetlastonsetInterval();
                UpdateAnimationSpeed();
            }

        }
    }

    void UpdateAnimationSpeed()
    {
        float newSpeed = originalDuration / targetDuration;
        //Debug.Log("target Duration: " + targetDuration);
        SetAnimationSpeed(newSpeed);

    }


    public void SetAnimationSpeed(float speed)
    {
        if (animator != null)
        {
            animator.speed = speed;
            //Debug.Log("Animation speed set to: " + speed);
        }
    }

    public void SetAnimationSpeedsign()
    {
        IsReversed = !IsReversed;
        animator.SetBool("IsReversed", IsReversed);
    }

    public void ResetToDefaultState()
    {
        IsReversed = false;

        // 使用淡入淡出的方式回到默认状态
        animator.SetTrigger("Reset");
    }

    public void PlayAudio()
    {
        if (audioSource != null && audioSource.clip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

}

