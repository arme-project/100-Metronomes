using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public GameObject[] metronomeContainers;
    private bool metronomesInitialized = false;
    private EnsembleModel ensembleModel;

    void Start()
    {
        Time.timeScale = 1f;
        UserTimeManager.Instance.clearTimestamps();
        //GuideManager.Instance.ReloadChildObject();

        // Get reference to EnsembleModel
        ensembleModel = FindAnyObjectByType<EnsembleModel>();

        // Initially deactivate all metronomes
        if (metronomeContainers != null)
        {
            foreach (GameObject container in metronomeContainers)
            {
                if (container != null)
                {
                    container.SetActive(true);

                    // Disable any animators initially
                    Animator[] animators = container.GetComponentsInChildren<Animator>(true);
                    foreach (Animator animator in animators)
                    {
                        animator.enabled = false;
                    }
                }
            }
        }
    }

    void Update()
    {
        if (UserTimeManager.Instance.GetTenTimestamps() && !metronomesInitialized)
        {
            InitializeMetronomes();
        }
    }

    void InitializeMetronomes()
    {
        float startTime = Time.time;
        PlayerPrefs.SetFloat("StartTime", startTime);
        PlayerPrefs.Save();

        if (metronomeContainers != null)
        {
            foreach (GameObject container in metronomeContainers)
            {
                if (container != null)
                {

                    // Get all animators in the container and its children
                    Animator[] animators = container.GetComponentsInChildren<Animator>(true);
                    foreach (Animator animator in animators)
                    {
                        // Enable the animator
                        animator.enabled = true;

                        // Reset the animator to start from beginning
                        animator.Rebind();
                        animator.Update(0f);

                        // Start playing
                        animator.Play("MetronomeAnimation", 0, 0f);
                    }
                }
            }
        }

        // Initialize ensemble model if it exists
        if (ensembleModel != null)
        {
            ensembleModel.SetInitialPlayerTempo();
        }

        metronomesInitialized = true;
        Debug.Log("Metronomes initialized and started at: " + startTime);
    }
}