using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ManualSceneTrigger : MonoBehaviour
{
    [System.Serializable]
    public class SceneTarget
    {
        public string sceneName;
        public Vector3 position;
        public float activationRadius = 1.6f; // Increased to match distance in logcat
    }

    [Header("Trigger Input")]
    public InputActionProperty triggerAction;

    [Header("Scene Targets")]
    public SceneTarget[] targets;

    private bool sceneLoading = false;

    void Update()
    {
        if (sceneLoading || triggerAction.action == null) return;

        float triggerValue = triggerAction.action.ReadValue<float>();
        bool triggerPressed = triggerValue > 0.5f;

        if (!triggerPressed) return;

        foreach (var target in targets)
        {
            float dist = Vector3.Distance(transform.position, target.position);
            Debug.Log($"[ManualTrigger] Checking '{target.sceneName}' → Dist: {dist} / Radius: {target.activationRadius}");

            if (dist <= target.activationRadius)
            {
                Debug.Log($"[ManualTrigger] TRIGGERED '{target.sceneName}'!");
                sceneLoading = true;
                SceneManager.LoadScene(target.sceneName);
                break;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (targets != null)
        {
            foreach (var target in targets)
            {
                Gizmos.DrawWireSphere(target.position, target.activationRadius);
            }
        }
    }
}
