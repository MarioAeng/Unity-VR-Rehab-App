using UnityEngine;
using UnityEngine.InputSystem;

public class ManualCupRayHandler : MonoBehaviour
{
    [Header("References")]
    public Transform handTransform;    // MainSelectorHand
    public Transform rayObject;        // MainSelectorHand
    public Transform holdPoint;
    public InputActionProperty triggerAction;
    public float rayLength = 15f;
    public LayerMask cupLayer;

    [Header("Ray Offset")]
    public Vector3 rayOffset = new Vector3(0f, -0.15f, 0.2f);

    private GameObject heldCup = null;
    private Rigidbody heldCupRb;
    private Vector3 originalScale;
    private bool wasTriggerPressed = false;

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

        if (heldCup != null)
        {
            Vector3 tetherOffset = holdPoint.forward * 0.05f + holdPoint.up * -0.04f;
            heldCup.transform.position = holdPoint.position + tetherOffset;
            heldCup.transform.rotation = holdPoint.rotation;

            Debug.Log($"[Tether] CupPos: {heldCup.transform.position}, HoldPoint: {holdPoint.position}");
        }

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
                heldCupRb = heldCup.GetComponent<Rigidbody>();
                originalScale = heldCup.transform.lossyScale;

                if (heldCupRb) heldCupRb.isKinematic = true;

                Debug.Log($"[Pickup] ✅ Picked up {heldCup.name} (Scale: {originalScale})");
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

        if (heldCupRb)
        {
            heldCupRb.isKinematic = false;
            heldCupRb.velocity = Vector3.zero;
        }

        Debug.Log($"[Drop] 🟨 Dropped {heldCup.name}");

        heldCup = null;
        heldCupRb = null;
    }
}
