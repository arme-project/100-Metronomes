using UnityEngine;
using System.Collections.Generic;

public class SyncCalculator : MonoBehaviour
{
    private List<Animator> dialAnimators = new List<Animator>();

    private void Start()
    {
        // Find all objects with "metro" tag
        GameObject[] metroObjects = GameObject.FindGameObjectsWithTag("Metro");
        Debug.Log($"Found {metroObjects.Length} metro objects");

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
                        dialAnimators.Add(animator);
                        Debug.Log($"Found animator on {child.name} in {metro.name}");
                    }
                }
            }
        }

        Debug.Log($"Found {dialAnimators.Count} total dial animators");
    }

    private void Update()
    {
        if (dialAnimators.Count > 0)
        {
            float syncPercentage = CalculateKuramotoSync();
            Debug.Log($"Synchronization: {syncPercentage:F2}%");
        }
    }

    private float CalculateKuramotoSync()
    {
        if (dialAnimators == null || dialAnimators.Count == 0)
        {
            Debug.LogWarning("No dial animators found!");
            return 0f;
        }

        float sumSin = 0f;
        float sumCos = 0f;
        int activeAnimators = 0;

        // Calculate sum of sines and cosines of phases
        foreach (Animator animator in dialAnimators)
        {
            if (animator != null && animator.isActiveAndEnabled)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                float phase = stateInfo.normalizedTime * 2f * Mathf.PI;

                sumSin += Mathf.Sin(phase);
                sumCos += Mathf.Cos(phase);
                activeAnimators++;
            }
        }

        if (activeAnimators == 0) return 0f;

        // Calculate order parameter r using only active animators
        float r = Mathf.Sqrt((sumSin * sumSin + sumCos * sumCos) / (activeAnimators * activeAnimators));

        // Convert to percentage
        return r * 100f;
    }
}