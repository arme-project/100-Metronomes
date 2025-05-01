using UnityEngine;

public class MetronomeLightController : MonoBehaviour
{
    private Light pointLight;
    private Animator animator;
    private bool lightEnabled = false;
    
    void Start()
    {
        // Get components including inactive objects
        pointLight = GetComponentInChildren<Light>(true);
        if (pointLight == null)
        {
            Debug.LogError($"No Light component found in children of {gameObject.name} - checking deeper...");
            
            // Manual deep search through hierarchy
            Transform objectsTransform = transform.Find("Objects");
            if (objectsTransform != null)
            {
                foreach (Transform child in objectsTransform)
                {
                    Light light = child.GetComponent<Light>();
                    if (light != null)
                    {
                        pointLight = light;
                        Debug.Log($"Found light in deeper hierarchy for {gameObject.name}");
                        break;
                    }
                }
            }
            
            if (pointLight == null)
            {
                Debug.LogError($"Still no Light component found for {gameObject.name}");
                return;
            }
        }
        
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError($"No Animator component found on {gameObject.name}");
            return;
        }

        // Make sure the light GameObject is active
        pointLight.gameObject.SetActive(true);
        // Start with light component disabled
        pointLight.enabled = false;
    }

    void Update()
    {
        if (animator != null && pointLight != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = stateInfo.normalizedTime;

            // Enable light when animation is at peak
            if (normalizedTime >= 0.9f && normalizedTime <= 1.0f && !lightEnabled)
            {
                pointLight.enabled = true;
                lightEnabled = true;
            }
            // Disable light for the rest of the animation cycle
            else if (normalizedTime < 0.9f && lightEnabled)
            {
                pointLight.enabled = false;
                lightEnabled = false;
            }
        }
    }
}