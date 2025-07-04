using UnityEngine;
using UnityEngine.InputSystem;

public class ManualCupRayHandler : MonoBehaviour
{
    [Header("References")]
    public Transform handTransform;  // MainSelectorHand
    public Transform rayObject;      // Same as above
    public Transform holdPoint;
    public InputActionProperty triggerAction;
    public float rayLength = 15f;
    public LayerMask cupLayer;

    [Header("Ray Offset")]
    public Vector3 rayOffset = new Vector3(0f, -0.15f, 0.2f);

    private GameObject heldCup = null;
    private bool wasTriggerPressed = false;

    void OnEnable()
    {
        if (triggerAction.action != null)
        {
            triggerAction.action.Enable();
            Debug.Log("[RayHandler] TriggerAction enabled in OnEnable.");
        }
        else
        {
            Debug.LogError("[RayHandler] TriggerAction is null!");
        }
    }

    void Update()
    {
        if (handTransform == null || rayObject == null || holdPoint == null || triggerAction.action == null)
        {
            Debug.LogWarning("[ManualCupRayHandler] ❌ One or more references are missing.");
            return;
        }

        if (handTransform.position.y < 0.1f)
        {
            Debug.Log("[RayDebug] ✋ Hand is too low or not tracked.");
            return;
        }

        // Update ray position and rotation
        rayObject.position = handTransform.TransformPoint(rayOffset);
        rayObject.rotation = handTransform.rotation;

        float triggerValue = triggerAction.action.ReadValue<float>();
        bool isPressed = triggerValue > 0.5f;

        if (isPressed && !wasTriggerPressed)
        {
            if (heldCup == null)
                TryPickupCup();
            else
                DropCup();
        }

        wasTriggerPressed = isPressed;

        Debug.DrawRay(rayObject.position, rayObject.forward * rayLength, Color.green);
    }

    void TryPickupCup()
    {
        Ray ray = new Ray(rayObject.position, rayObject.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, cupLayer))
        {
            Debug.Log($"[RayHandler] Ray hit: {hit.collider.name}");

            if (hit.collider.CompareTag("Cup"))
            {
                heldCup = hit.collider.gameObject;

                // Save world scale
                Vector3 worldScale = heldCup.transform.lossyScale;

                // Clear parent before reattaching to avoid distortion
                heldCup.transform.SetParent(null);
                heldCup.transform.SetPositionAndRotation(holdPoint.position + holdPoint.forward * 0.4f, holdPoint.rotation);
                heldCup.transform.SetParent(holdPoint, worldPositionStays: true);

                // Fix scale distortion by converting world to local
                Vector3 parentScale = holdPoint.lossyScale;
                heldCup.transform.localScale = new Vector3(
                    worldScale.x / parentScale.x,
                    worldScale.y / parentScale.y,
                    worldScale.z / parentScale.z
                );

                var rb = heldCup.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.isKinematic = true;
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                Debug.Log($"[Pickup] ✅ Picked up {heldCup.name} | World Scale Preserved: {worldScale}");
            }
            else
            {
                Debug.Log($"[RayHandler] Hit non-cup object: {hit.collider.tag}");
            }
        }
        else
        {
            Debug.Log("[RayHandler] ❌ Raycast did not hit anything.");
        }
    }

    void DropCup()
    {
        if (heldCup == null) return;

        heldCup.transform.SetParent(null);

        var rb = heldCup.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
        }

        Debug.Log($"[Drop] 🟨 Dropped {heldCup.name}");
        heldCup = null;
    }
}
