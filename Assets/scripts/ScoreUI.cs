using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    public Text scoreText; // 引用 Unity 的 Text 组件
    public Text secondText; // 引用 Unity 的 Text 组件
    public Text BPMText;
    public Text AlphaText;
    public Text FinishText;
    public float timecount = 0;
    private int scoreCounter = 0;

    private void Start()
    {
        if (scoreText == null)
        {
            Debug.LogError("ScoreText is not assigned!");
        }
        else
        {
            // 初始化文本内容
            UpdateScoreText(scoreCounter);
            UpdateBPMText(PlayerPrefs.GetFloat("BPM", 60f));
            UpdateAlphaText(PlayerPrefs.GetString("Alpha", "High"));
        }
        if (FinishText != null)
        {
            FinishText.enabled = false;
        }
    }
    private void Update()
    {
        timecount =Time.time- PlayerPrefs.GetFloat("StartTime");
        UpdateTimeText(timecount);
    }

    public void UpdateScore(int newScore)
    {
        scoreCounter = newScore;
        UpdateScoreText(scoreCounter);
        //if (scoreCounter > 7)
        //{
        //    FinishText.enabled = true;
        //}
    }

    private void UpdateScoreText(int Counter)
    {
        if (scoreText != null)
        {
            scoreText.text = "Scorecount: " + Counter.ToString();
        }
    }
    private void UpdateTimeText(float SecondCounter)
    {
        if (secondText != null)
        {
            secondText.text = "Time: " + SecondCounter.ToString("F0")+" s";
        }
    }
    public void UpdateBPMText(float bpm)
    {
        if (BPMText != null)
        {
            BPMText.text = "BPM: " + bpm.ToString();
        }
    }

    public void UpdateAlphaText(string name)
    {
        if (AlphaText != null)
        {
            AlphaText.text = "Alpha: " + name;
        }
    }
}
