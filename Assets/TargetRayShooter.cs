using UnityEngine;
using UnityEngine.InputSystem;

public class TargetRayShooter : MonoBehaviour
{
    [Header("References")]
    public LineRenderer lineRenderer;
    public GameObject spawnerObject;
    public LayerMask targetLayer;
    public float maxDistance = 10f;

    [Header("Input")]
    public InputActionAsset inputActionAsset;
    public string rightTriggerActionName = "RightTriggerAction";
    public string leftTriggerActionName = "LeftTriggerAction";

    private InputAction triggerAction;
    private bool isLeftHanded = false;
    private TargetSpawner spawner;
    private bool wasPressedLastFrame = false;

    void Start()
    {
        isLeftHanded = PlayerSettings.IsLeftHanded;
        spawner = spawnerObject?.GetComponent<TargetSpawner>();

        if (spawner == null)
        {
            Debug.LogError("[TargetRayShooter] Missing TargetSpawner reference.");
        }

        string actionName = isLeftHanded ? leftTriggerActionName : rightTriggerActionName;
        triggerAction = inputActionAsset?.FindAction(actionName);

        if (triggerAction == null)
        {
            Debug.LogError($"[TargetRayShooter] Could not find action: {actionName}");
        }
        else
        {
            triggerAction.Enable();
            Debug.Log($"[TargetRayShooter] Using {(isLeftHanded ? "Left" : "Right")} trigger: {actionName}");
        }
    }

    void Update()
    {
        if (triggerAction == null || spawner == null) return;

        Vector3 rayStart = transform.position;
        Vector3 rayDirection = transform.forward;

        Ray ray = new Ray(rayStart, rayDirection);
        lineRenderer.SetPosition(0, rayStart);
        lineRenderer.SetPosition(1, rayStart + rayDirection * maxDistance);

        bool isPressed = triggerAction.ReadValue<float>() > 0.5f;

        if (isPressed && !wasPressedLastFrame)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, targetLayer))
            {
                if (hit.collider.CompareTag("TargetCube"))
                {
                    Debug.Log("[TargetRayShooter] Cube hit, destroying...");
                    Destroy(hit.collider.gameObject);
                    spawner.OnTargetHit(); // Use OnTargetHit(), not RegisterHit
                }
            }
        }

        wasPressedLastFrame = isPressed;
    }

    void OnDisable()
    {
        triggerAction?.Disable();
    }
}
