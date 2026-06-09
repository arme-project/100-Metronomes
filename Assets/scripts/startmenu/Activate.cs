using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Activate : MonoBehaviour
{
    private Animator animator;
    public bool Switch = false;
    private float defaultBpm = 120f;
    private float bpm;

    [Tooltip("Multiplier on the guide tick speed. <1 ticks slightly slower, >1 faster. Keep in sync with AudioOnly.guideSpeedMultiplier.")]
    public float guideSpeedMultiplier = 0.95f;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found!");
            return;
        }

        // ��ʼ��ʱ��ȡ BPM ֵ
        bpm = PlayerPrefs.GetFloat("BPM", defaultBpm);
        UpdateAnimationSpeed();
    }

    // Update is called once per frame

    void Update()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // ʵʱ��ȡ BPM ֵ�����¶����ٶ�
        float newBpm = PlayerPrefs.GetFloat("BPM", defaultBpm);
        if (newBpm != bpm)
        {
            bpm = newBpm;
            UpdateAnimationSpeed();
        }

        if (stateInfo.normalizedTime >= 1 && !animator.IsInTransition(0))
        {
            Switch = !Switch;
            animator.SetBool("Switch", Switch);

        }

    }
    void UpdateAnimationSpeed()
    {
        float animationSpeed = (bpm / defaultBpm) * guideSpeedMultiplier;
        animator.speed = animationSpeed;
    }

}
