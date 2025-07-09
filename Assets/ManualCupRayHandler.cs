using UnityEngine;
using UnityEngine.InputSystem;

public class ManualCupRayHandler : MonoBehaviour
{
    [Header("References")]
    public Transform handTransform;    // MainSelectorHand
    public Transform rayObject;
    public Transform holdPoint;
    public InputActionProperty triggerAction;
    public float rayLength = 15f;
    public LayerMask cupLayer;

    [Header("Ray Offset")]
    public Vector3 rayOffset = new Vector3(0f, -0.15f, 0.2f);

    [Header("Materials")]
    public Material cupVisibleMaterial;       // Default blue
    public Material cupHighlightMaterial;     // Green = safe
    public Material cupTooHighMaterial;       // Red = too high

    [Header("Drop Safety Settings")]
    public float safeDropHeight = 0.4f; // Adjustable in Inspector

    private GameObject heldCup = null;
    private Rigidbody heldCupRb;
    private Vector3 originalScale;
    private bool wasTriggerPressed = false;

    private GameObject lastHighlightedCup = null;

    void Update()
    {
        if (handTransform == null || rayObject == null || holdPoint == null || triggerAction.action == null)
        {
            Debug.LogWarning("[ManualCupRayHandler] ❌ Missing references.");
            return;
        }

        if (handTransform.position.y < 0.1f)
        {
            Debug.Log("[RayDebug] ✋ Hand is too low.");
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

            UpdateDropHeightVisual();
        }

        HighlightCupIfNeeded();

        Debug.DrawRay(rayObject.position, rayObject.forward * rayLength, Color.green);
    }

    void HighlightCupIfNeeded()
    {
        if (heldCup != null) return;

        Ray ray = new Ray(rayObject.position, rayObject.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, cupLayer))
        {
            if (hit.collider.CompareTag("Cup"))
            {
                GameObject hitCup = hit.collider.gameObject;
                if (lastHighlightedCup != hitCup)
                {
                    ResetLastHighlight();

                    Renderer cupRenderer = hitCup.GetComponent<Renderer>();
                    if (cupRenderer && cupHighlightMaterial != null)
                    {
                        cupRenderer.material = cupHighlightMaterial;
                        lastHighlightedCup = hitCup;
                    }
                }
            }
            else
            {
                ResetLastHighlight();
            }
        }
        else
        {
            ResetLastHighlight();
        }
    }

    void ResetLastHighlight()
    {
        if (lastHighlightedCup != null)
        {
            Renderer rend = lastHighlightedCup.GetComponent<Renderer>();
            if (rend && cupVisibleMaterial != null)
            {
                rend.material = cupVisibleMaterial;
            }
            lastHighlightedCup = null;
        }
    }

    void TryPickupCup()
    {
        Ray ray = new Ray(rayObject.position, rayObject.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, cupLayer))
        {
            if (hit.collider.CompareTag("Cup"))
            {
                heldCup = hit.collider.gameObject;
                heldCupRb = heldCup.GetComponent<Rigidbody>();
                originalScale = heldCup.transform.lossyScale;

                if (heldCupRb) heldCupRb.isKinematic = true;

                Debug.Log($"[Pickup] ✅ Picked up {heldCup.name}");

                ResetLastHighlight();
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

        Renderer rend = heldCup.GetComponent<Renderer>();
        if (rend && cupVisibleMaterial != null)
        {
            rend.material = cupVisibleMaterial;
        }

        Debug.Log($"[Drop] 🟨 Dropped {heldCup.name}");

        heldCup = null;
        heldCupRb = null;
    }

    void UpdateDropHeightVisual()
    {
        if (heldCup == null) return;

        float dropHeight = heldCup.transform.position.y;

        // Raycast down to see what's below the cup
        if (Physics.Raycast(heldCup.transform.position, Vector3.down, out RaycastHit hitInfo, 2f))
        {
            float heightAboveSurface = heldCup.transform.position.y - hitInfo.point.y;

            Renderer rend = heldCup.GetComponent<Renderer>();
            if (rend)
            {
                if (heightAboveSurface <= safeDropHeight && cupHighlightMaterial != null)
                {
                    rend.material = cupHighlightMaterial;
                    Debug.Log($"[DropHeight] ✅ Safe to drop ({heightAboveSurface:F2}m)");
                }
                else if (cupTooHighMaterial != null)
                {
                    rend.material = cupTooHighMaterial;
                    Debug.Log($"[DropHeight] ❌ Too high to drop ({heightAboveSurface:F2}m)");
                }
            }
        }
    }
}
