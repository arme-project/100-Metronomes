using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UserPlayer : Player
{
    // 用于存储多个 meanOnset 和 meanInterval 的列表
    private List<float> meanOnsetList = new List<float>();
    private List<float> meanIntervalList = new List<float>();

    private bool clapDetected = false;

    protected override void Start()
    {
        AudioInputDetector2.Instance.clapDetectedEvent += clapDetectedEventSubscribe;
        base.Start(); // 调用基类的 Start 方法
        notePlayed = false;
    }

    public void clapDetectedEventSubscribe(float time)
    {
        clapDetected = true;
    }

    void Update()
    {
        //if (InputChecker.IsTouchBegan())
        if (clapDetected)
        {
            clapDetected = false;
            Debug.LogWarning("UserPlayer: Input detected, playing note.");
            float currentTime = Time.time;
            AddTimestamp(currentTime);
            if (latestOnsetTimes.Count > 1)
            {
                onsetInterval = currentTime - latestOnsetTimes[latestOnsetTimes.Count - 2];
            }
            PlayNote();
        }
    }

    public override void RecalculateOnsetInterval(float originalAnimationDuration,
                                                  List<Player> players,
                                                  List<float> alphas,
                                                  List<float> betas)
    {
        float meanOnset = 0.0f;
        float meanInterval = 0.0f;
        int nOtherPlayers = 0;

        for (int i = 0; i < players.Count; ++i)
        {
            if (players[i] != this)
            {
                meanOnset += players[i].GetLatestOnsetTime();
                meanInterval += players[i].onsetInterval;
                ++nOtherPlayers;
            }
        }

        if (nOtherPlayers > 0)
        {
            meanOnset /= nOtherPlayers;
            meanInterval /= nOtherPlayers;
        }
        else
        {
            onsetInterval = meanOnset - GetLatestOnsetTime() + 1.5f * meanInterval;
        }

        // 将最新的 meanOnset 和 meanInterval 添加到列表中
        meanOnsetList.Add(meanOnset);
        meanIntervalList.Add(meanInterval);

        // 如果需要，将最新值存储在PlayerPrefs中（例如用于保存最终计算结果）
        PlayerPrefs.SetFloat("meanOnset", meanOnset);
        PlayerPrefs.SetFloat("meanInterval", meanInterval);
        PlayerPrefs.SetFloat("onsetInterval", onsetInterval);
        PlayerPrefs.Save(); // 保存更改
    }

    // 如果需要，添加方法来访问 meanOnsetList 和 meanIntervalList
    public List<float> GetMeanOnsetList()
    {
        return meanOnsetList;
    }

    public List<float> GetMeanIntervalList()
    {
        return meanIntervalList;
    }

    public void Clearuserlist()
    {
        meanOnsetList.Clear();
        meanIntervalList.Clear();
    }
}
