using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class AutoPlayer : Player
{
    public float minSpeed = 0.25f;
    public float maxSpeed = 1.25f;

    public bool IsReversed = false;
    private List<float> animationSpeeds = new List<float>(); 
    protected override void Start()
    {
        animationSpeeds.Clear();
        base.Start(); 
        notePlayed = false;
        RandomizeAllAnimationSpeeds();
    }

    void Update()
    {
        AnimationEnd();
    }


    void RandomizeAllAnimationSpeeds()
    {
        float speed = dialAnimator.GetCurrentAnimatorStateInfo(0).speed;
        animationSpeeds.Add(speed); 
    }

    private void AnimationEnd()
    {
        AnimatorStateInfo stateInfo = dialAnimator.GetCurrentAnimatorStateInfo(0);

        
        if (stateInfo.normalizedTime >= 1 && !dialAnimator.IsInTransition(0))
        {
            
            if (!notePlayed) {
                latestOnsetTimes.Add(Time.time);
            }
            SetAnimationSpeedsign();
            ApplyStoredSpeed(); 
            PlayNote();
        }
    }

    public override void RecalculateOnsetInterval(float originalAnimationDuration,
                                                  List<Player> players,
                                                  List<float> alphas,
                                                  List<float> betas)
    {
        float alphaSum = 0f;
        float betaSum = 0f;
        float Animationlength = 0.5f;

        for (int i = 0; i < players.Count; ++i)
        {
            float async = GetLatestOnsetTime() - players[i].GetLatestOnsetTime();
            alphaSum += alphas[i] * async;
            betaSum += betas[i] * async;
        }

        
        timeKeeperMean -= betaSum;

        
        float hNoise = GenerateHNoise();

        
        onsetInterval = originalAnimationDuration - alphaSum + 0.01f*hNoise;

        
        if (onsetInterval > 0.1)
        {
            float calculatedSpeed = Animationlength / onsetInterval;
            animationSpeeds.Add(calculatedSpeed);
        }

    }

    public void SetAnimationSpeedsign()
    {
        IsReversed = !IsReversed;
        dialAnimator.SetBool("IsReversed", IsReversed);
    }

    
    public void ApplyStoredSpeed()
    {
        if (animationSpeeds.Count > 1)
        {
            dialAnimator.speed = animationSpeeds[animationSpeeds.Count - 1];
        }
        else
        {
            dialAnimator.speed = animationSpeeds[0];
        }
    }
}
