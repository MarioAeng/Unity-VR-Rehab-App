using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ManualButtonHighlighter : MonoBehaviour
{
    [System.Serializable]
    public class SceneTarget
    {
        public string sceneName;
        public Transform targetTransform;
        public float activationRadius = 1.6f;
        public Image targetImage;  // Reference to the UI Image component
        public Color normalColor = Color.white;
        public Color highlightColor = Color.green;
    }

    public InputActionProperty triggerAction;
    public SceneTarget[] targets;
    private bool sceneLoading = false;

    void Update()
    {
        if (sceneLoading || triggerAction.action == null)
            return;

        float triggerValue = triggerAction.action.ReadValue<float>();
        bool triggerPressed = triggerValue > 0.5f;

        foreach (var target in targets)
        {
            if (target.targetTransform == null || target.targetImage == null)
                continue;

            float dist = Vector3.Distance(transform.position, target.targetTransform.position);

            // Highlight if within range
            bool inRange = dist <= target.activationRadius;
            target.targetImage.color = inRange ? target.highlightColor : target.normalColor;

            if (inRange)
                Debug.Log($"[Highlight] Within range of {target.sceneName} (dist={dist})");

            if (inRange && triggerPressed)
            {
                Debug.Log($"[ManualTrigger] TRIGGERED {target.sceneName}");
                sceneLoading = true;
                SceneManager.LoadScene(target.sceneName);
                break;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (targets != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var target in targets)
            {
                if (target.targetTransform != null)
                    Gizmos.DrawWireSphere(target.targetTransform.position, target.activationRadius);
            }
        }
    }
}
