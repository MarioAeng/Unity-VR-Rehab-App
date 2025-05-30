using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SimulatedRaySceneLoader : MonoBehaviour
{
    [Header("Ray Settings")]
    public InputActionProperty triggerAction;
    public Transform rayOrigin;
    public float rayDistance = 10f;
    public LayerMask uiLayer;

    private bool wasTriggerPressed = false;

    void Start()
    {
        // Try to auto-assign the rayOrigin if not set
        if (rayOrigin == null)
        {
            var rightController = GameObject.Find("RightHand Controller");
            if (rightController != null)
            {
                rayOrigin = rightController.transform;
                Debug.Log("[SimRayLoader] Auto-assigned rayOrigin to RightHand Controller.");
            }
            else
            {
                Debug.LogWarning("[SimRayLoader] Could not find RightHand Controller.");
            }
        }
    }

    void Update()
    {
        if (triggerAction.action == null || rayOrigin == null)
            return;

        float triggerValue = triggerAction.action.ReadValue<float>();
        bool isTriggerPressed = triggerValue > 0.5f;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);

        Debug.Log($"[SimRayLoader] Ray Origin: {ray.origin}, Dir: {ray.direction}");

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, uiLayer))
        {
            Debug.Log("[SimRayLoader] Ray hit: " + hit.collider.name);

            if (isTriggerPressed && !wasTriggerPressed)
            {
                if (hit.collider.name == "ArmRaiseButton")
                {
                    Debug.Log("Scene change: ArmRaiseScene");
                    SceneManager.LoadScene("ArmRaiseScene");
                }
                else if (hit.collider.name == "ArmRotationButton")
                {
                    Debug.Log("Scene change: ArmRotationScene");
                    SceneManager.LoadScene("ArmRotationScene");
                }
            }
        }
        else
        {
            Debug.LogWarning("[SimRayLoader] Raycast missed UI.");
        }

        wasTriggerPressed = isTriggerPressed;
    }
}
