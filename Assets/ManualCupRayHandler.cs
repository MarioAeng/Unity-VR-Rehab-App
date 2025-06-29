using UnityEngine;
using UnityEngine.InputSystem;

public class ManualCupRayHandler : MonoBehaviour
{
    [Header("References")]
    public Transform handTransform;
    public Transform rayObject;
    public Transform holdPoint;
    public InputActionProperty triggerAction;
    public float rayLength = 15f;
    public LayerMask cupLayer;

    [Header("Ray Offset")]
    public Vector3 rayOffset = new Vector3(0f, -0.15f, 0.2f);

    private GameObject heldCup = null;
    private Vector3 originalScale;
    private bool wasTriggerPressed = false;

    void Update()
    {
        if (handTransform == null || rayObject == null || holdPoint == null || triggerAction.action == null)
        {
            Debug.LogWarning("[ManualCupRayHandler] ❌ Missing references.");
            return;
        }

        if (handTransform.position.y < 0.1f)
        {
            Debug.Log("[RayDebug] ✋ Hand not tracked yet.");
            return;
        }

        // Move rayObject to offset from hand
        rayObject.position = handTransform.TransformPoint(rayOffset);
        rayObject.rotation = handTransform.rotation;

        // Maintain held cup position manually
        if (heldCup != null)
        {
            heldCup.transform.position = holdPoint.position;
            heldCup.transform.rotation = holdPoint.rotation;
            Debug.Log($"[HeldCup] Updating position to HoldPoint: {holdPoint.position}");
        }

        // Trigger input
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
            if (hit.collider.CompareTag("Cup"))
            {
                heldCup = hit.collider.gameObject;
                originalScale = heldCup.transform.lossyScale;

                heldCup.transform.SetParent(null); // Detach from any old parent
                heldCup.transform.position = holdPoint.position;
                heldCup.transform.rotation = holdPoint.rotation;
                heldCup.transform.localScale = Vector3.one;

                var rb = heldCup.GetComponent<Rigidbody>();
                if (rb) rb.isKinematic = true;

                Debug.Log($"[Pickup] ✅ Picked up {heldCup.name} at {heldCup.transform.position}");
            }
        }
    }

    void DropCup()
    {
        if (heldCup == null) return;

        heldCup.transform.SetParent(null);
        heldCup.transform.localScale = originalScale;

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
