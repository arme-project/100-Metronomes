using UnityEngine;
using System.Collections.Generic;

public class SyncCalculatorIndividual : MonoBehaviour
{
    private List<MetronomeData> metronomes = new List<MetronomeData>();

    // Class to store data for each metronome
    private class MetronomeData
    {
        public GameObject metroObject;
        public Animator dialAnimator;
        public float individualSync;

        public MetronomeData(GameObject obj, Animator anim)
        {
            metroObject = obj;
            dialAnimator = anim;
            individualSync = 0f;
        }
    }

    private void Start()
    {
        // Find all objects with "metro" tag
        GameObject[] metroObjects = GameObject.FindGameObjectsWithTag("Metro");
        //Debug.Log($"Found {metroObjects.Length} metro objects");

        foreach (GameObject metro in metroObjects)
        {
            // Get all children of this metro object
            Transform[] children = metro.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                // Check if this is a dial object
                if (child.CompareTag("dial"))
                {
                    Animator animator = child.GetComponent<Animator>();
                    if (animator != null)
                    {
                        metronomes.Add(new MetronomeData(metro, animator));
                       // Debug.Log($"Found animator on {child.name} in {metro.name}");
                    }
                }
            }
        }
        Debug.Log($"Found {metronomes.Count} total metronomes");
    }

    private void Update()
    {
        if (metronomes.Count > 0)
        {
            // Calculate overall synchronization
            float overallSync = CalculateOverallSync();
            //Debug.Log($"Overall Synchronization: {overallSync:F2}%");

            // Calculate individual synchronizations
            CalculateIndividualSyncs();

            // Log individual synchronizations
            //for (int i = 0; i < metronomes.Count; i++)
            //{
            //    Debug.Log($"Metronome {i}: {metronomes[i].individualSync:F2}%");
            //}
        }
    }

    private float CalculateOverallSync()
    {
        if (metronomes == null || metronomes.Count == 0) return 0f;

        float sumSin = 0f;
        float sumCos = 0f;
        int activeMetronomes = 0;

        foreach (MetronomeData metro in metronomes)
        {
            if (metro.dialAnimator != null && metro.dialAnimator.isActiveAndEnabled)
            {
                AnimatorStateInfo stateInfo = metro.dialAnimator.GetCurrentAnimatorStateInfo(0);
                float phase = stateInfo.normalizedTime * 2f * Mathf.PI;
                sumSin += Mathf.Sin(phase);
                sumCos += Mathf.Cos(phase);
                activeMetronomes++;
            }
        }

        if (activeMetronomes == 0) return 0f;

        float r = Mathf.Sqrt((sumSin * sumSin + sumCos * sumCos) / (activeMetronomes * activeMetronomes));
        return r * 100f;
    }

    private void CalculateIndividualSyncs()
    {
        if (metronomes == null || metronomes.Count <= 1) return;

        foreach (MetronomeData targetMetro in metronomes)
        {
            if (targetMetro.dialAnimator == null || !targetMetro.dialAnimator.isActiveAndEnabled)
            {
                targetMetro.individualSync = 0f;
                continue;
            }

            float targetPhase = targetMetro.dialAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime * 2f * Mathf.PI;
            float sumSin = 0f;
            float sumCos = 0f;
            int otherActiveMetronomes = 0;

            // Calculate phase difference with all other metronomes
            foreach (MetronomeData otherMetro in metronomes)
            {
                if (otherMetro != targetMetro &&
                    otherMetro.dialAnimator != null &&
                    otherMetro.dialAnimator.isActiveAndEnabled)
                {
                    float otherPhase = otherMetro.dialAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime * 2f * Mathf.PI;
                    float phaseDiff = otherPhase - targetPhase;
                    sumSin += Mathf.Sin(phaseDiff);
                    sumCos += Mathf.Cos(phaseDiff);
                    otherActiveMetronomes++;
                }
            }

            if (otherActiveMetronomes == 0)
            {
                targetMetro.individualSync = 100f; // Alone metronome is considered fully synchronized
            }
            else
            {
                float r = Mathf.Sqrt((sumSin * sumSin + sumCos * sumCos) / (otherActiveMetronomes * otherActiveMetronomes));
                targetMetro.individualSync = r * 100f;
            }
        }
    }
}