using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserTimeManager : MonoBehaviour
{
    // 单例实例
    public static UserTimeManager Instance { get; private set; }
    public Text TimecountText;
    //public Text FollowText;

    private int count = 10;

    // 存储时间戳的列表
    private List<float> Timestamps = new List<float>();

    private void Awake()
    {
        // 确保只有一个实例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        //if (FollowText != null)
        //{
        //    FollowText.enabled = true;
        //}
        UpdateText(count);
    }

    // 添加时间戳的方法
    public void AddTimestamp(float time)
    {
        if (Timestamps.Count > 0)
        {
            float lastTime = Timestamps[Timestamps.Count - 1];
            if (time - lastTime < 0.05f)
            {
                //Debug.LogWarning("Timestamp ignored: Less than 0.05 seconds since the last timestamp.");
                return;
            }
        }

        Timestamps.Add(time);                 
    }

    public List<float> GetClapTimestamps()
    {
        return new List<float>(Timestamps);
    }

    public int GetsatampNum()
    {
        return Timestamps.Count;
    }
    public float GetLatestClapTimestamp()
    {
        if (Timestamps.Count > 0)
        {
            return Timestamps[Timestamps.Count - 1];
        }
        else
        {
            Debug.LogWarning("No space key timestamps recorded yet");
            return 0f;
        }
    }

    public bool GetTenTimestamps()
    {
        int count = 10 - Timestamps.Count;
        TimecountText.text = count.ToString();
        //if (Timestamps.Count > 4 && FollowText != null)
        //{
        //    FollowText.enabled = true;
        //}
        if (Timestamps.Count > 9 && TimecountText != null)
        {
            TimecountText.enabled = false;
        }

        return (Timestamps.Count > 9); 
    }

    public float GetlastonsetInterval()
    {
        if (Timestamps.Count < 3)
        {
            Debug.LogWarning("Not enough timestamps to calculate the time difference.");
            return 0f;
        }

        float lastTimestamp = Timestamps[(Timestamps.Count - 1)];
        //Debug.Log("lastTimestamp: " + Timestamps.Count);
        //Debug.Log("lastTimestamp: " + lastTimestamp);
        float secondLastTimestamp = Timestamps[(Timestamps.Count - 2)];
        //Debug.Log("secondLastTimestamp: " + secondLastTimestamp);
        //Debug.Log("lastTimestamp: " + Timestamps.Count);
        //Debug.Log("Timestamps: " + Timestamps[0]);

        return lastTimestamp - secondLastTimestamp;
    }
    public void clearTimestamps()
    {

        Timestamps.Clear();
        //FollowText.enabled = false;
        TimecountText.enabled = true;
    }
    private void UpdateText(int Count)
    {
        if (TimecountText != null)
        {
            TimecountText.text = Count.ToString();
        }
    }
}