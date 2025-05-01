using System.Collections.Generic;
using UnityEngine;

public class EnsembleModel : MonoBehaviour
{
    public static EnsembleModel Instance { get; private set; }

    public ScoreUI scoreUI; // 引用 ScoreUIController 组件

    public List<Player> players = new List<Player>();
    public List<List<float>> alphaParams = new List<List<float>>();
    public List<List<float>> betaParams = new List<List<float>>();

    public float alpha_self = 0.0f;
    public float beta_self = 0.0f;
    public float beta_user = 0.01f;
    public float beta_auto = 0.01f;

    private bool initialTempoSet = false;
    private int scoreCounter = 0;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ClearParam();
        Findplayer();
        InitalParam();
    }

    private void Update()
    {
        PlayScore();
    }

    public void SetInitialPlayerTempo()
    {
        if (!initialTempoSet)
        {
            foreach (var player in players)
            {
                player.onsetInterval = SetoriginalAnimationDuration();
            }

            initialTempoSet = true;
        }
    }

    public bool NewOnsetsAvailable()
    {
        foreach (var player in players)
        {
            if (!player.notePlayed)
            {
                return false;
            }
        }
        return true;
    }

    public void CalculateNewIntervals()
    {
        foreach (var player in players)
        {
            //float AnimationDuration = player.GetlastonsetInterval();
            player.RecalculateOnsetInterval(SetoriginalAnimationDuration(), players, alphaParams[player.Index], betaParams[player.Index]);
        }
    }

    public float SetoriginalAnimationDuration()
    {
        float bpm = PlayerPrefs.GetFloat("BPM", 120f);
        float originalAnimationDuration = (120f / bpm) * 0.5f;
        //Debug.Log("originalAnimationDuration"+ originalAnimationDuration);
        return originalAnimationDuration;

    }

    public void ClearOnsetsAvailable()
    {
        foreach (var player in players)
        {
            player.ResetNote();
            //Debug.Log("Note reset");
        }
    }

    public void PlayScore()
    {
        // If all players have played a note, update timings.
        if (NewOnsetsAvailable())
        {
            CalculateNewIntervals();
            ClearOnsetsAvailable();
            ++scoreCounter;
            scoreUI.UpdateScore(scoreCounter);
            PlayerPrefs.SetInt("scoreCounter", scoreCounter);
            PlayerPrefs.Save();
#if UNITY_EDITOR
            //  ScoreUIController 
            //Debug.Log("scorecount" + scoreCounter);          
#endif
        }
    }

    public void Findplayer()
    {
        // Find all Player objects in the scene and add them to the list
        Player[] foundPlayers = FindObjectsOfType<Player>();
        players.AddRange(foundPlayers);

    }
    public void ClearParam()
    {
        // Clear the players list
        players.Clear();
        alphaParams.Clear();
        betaParams.Clear();
        UserPlayer userPlayer = FindAnyObjectByType<UserPlayer>();
        if (userPlayer != null)
            userPlayer.Clearuserlist();
        scoreCounter = 0;
    }

    public void InitalParam()
    {
        float alpha_user = PlayerPrefs.GetFloat("alphaUser", 0.005f);
        float alpha_auto = PlayerPrefs.GetFloat("alphaAuto", 0.0001f);
        // Initialize alphaParams and betaParams lists
        for (int i = 0; i < players.Count; i++)
        {
            players[i].Index = i; // Set the index for each player
            alphaParams.Add(new List<float>());
            betaParams.Add(new List<float>());

            for (int j = 0; j < players.Count; j++)
            {
                if (i == j)
                {
                    // Set alpha and beta to 0 for the same player
                    alphaParams[i].Add(alpha_self);
                    betaParams[i].Add(beta_self);
                }
                else if (players[i] is UserPlayer || players[j] is UserPlayer)
                {
                    // If either i or j is a UserPlayer, set a higher alpha value
                    alphaParams[i].Add(alpha_user);
                    betaParams[i].Add(beta_user);
                }
                else
                {
                    // In other cases, set a lower alpha value
                    alphaParams[i].Add(alpha_auto);
                    betaParams[i].Add(beta_auto);
                }
            }
        }
    }

}