using UnityEngine;

public class GuideManager : MonoBehaviour
{
    public static GuideManager Instance { get; private set; }

    public GameObject visualPrefab;  //  Prefab
    public GameObject emptyObject;  

    public int needvisual;  //  visualPrefab
    public int needaudio;  //  emptyObjectPrefab


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ReloadChildObject();
    }


    void LoadChildObject(int visual)
    {
        if (visualPrefab != null)
        {
            visualPrefab.SetActive(false);
        }
        if (emptyObject != null)
        {
            emptyObject.SetActive(false);
        }

        
        if (visual == 1 && visualPrefab != null)
        {
            visualPrefab.SetActive(true);
        }
        else if (visual == 0 && emptyObject != null)
        {
            emptyObject.SetActive(true);
        }
    }

    
    public void DestroyOnSceneLoad()
    {
        Destroy(gameObject);
    }

    // Keeps the metronomes based on user settings to the Ensemble scene
    public void ReloadChildObject()
    {
        needvisual = PlayerPrefs.GetInt("VisualGudance", 1);
        LoadChildObject(needvisual);
    }
}
