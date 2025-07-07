using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;

public class SimulatedHandMenuSelector : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty triggerAction;

    [Header("References")]
    public XRRayInteractor rayInteractor;
    public Transform handTransform;
    public Transform rayObject;

    [Header("Ray Offset Settings")]
    public Vector3 rayOffset = new Vector3(0f, -0.15f, 0.2f);

    [Header("Ray Settings")]
    public float extendedRayLength = 25f;

    private bool wasTriggerPressed = false;
    private bool isReady = false;

    void OnEnable()
    {
        isReady = true;

        // Input binding check
        if (triggerAction.action != null)
        {
            triggerAction.action.Enable();
        }
        else
        {
            Debug.LogError("[SimHand] ❌ Missing TriggerAction");
            isReady = false;
        }

        // Ray interactor check
        if (rayInteractor != null)
        {
            rayInteractor.maxRaycastDistance = extendedRayLength;
            Debug.Log($"[SimHand] ✅ Set XRRayInteractor maxRaycastDistance to {extendedRayLength}");

            // Apply visual settings
            var lineVisual = rayInteractor.GetComponent<XRInteractorLineVisual>();
            if (lineVisual != null)
            {
                lineVisual.lineLength = extendedRayLength;
                lineVisual.reticle = null;  // Disable reticle to avoid auto-clamping
                lineVisual.validColorGradient = MakeSolidColor(Color.cyan);
                lineVisual.invalidColorGradient = MakeSolidColor(Color.red);
                Debug.Log($"[SimHand] ✅ Set line visual to {extendedRayLength} units");
            }
            else
            {
                Debug.LogWarning("[SimHand] ⚠️ XRInteractorLineVisual not found.");
            }
        }
        else
        {
            Debug.LogError("[SimHand] ❌ Missing XRRayInteractor");
            isReady = false;
        }

        if (handTransform == null)
        {
            Debug.LogError("[SimHand] ❌ Missing HandTransform");
            isReady = false;
        }

        if (rayObject == null)
        {
            Debug.LogError("[SimHand] ❌ Missing RayObject");
            isReady = false;
        }
    }

    void Update()
    {
        if (!isReady) return;

        float triggerValue = triggerAction.action.ReadValue<float>();
        bool isTriggerPressed = triggerValue > 0.5f;

        // Apply ray offset
        rayObject.position = handTransform.TransformPoint(rayOffset);
        rayObject.rotation = handTransform.rotation;

        // Visual debug ray
        Debug.DrawRay(rayObject.position, rayObject.forward * extendedRayLength, Color.cyan);

        // UI interaction via XRRayInteractor
        if (rayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult result))
        {
            if (result.gameObject != null)
            {
                Debug.Log($"[SimHand] 🎯 Hovering over: {result.gameObject.name}");

                if (isTriggerPressed && !wasTriggerPressed)
                {
                    Debug.Log("[SimHand] 🔘 Trigger clicked!");
                    ExecuteEvents.Execute(result.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
                }
            }
        }

        wasTriggerPressed = isTriggerPressed;
    }

    // Helper to create solid color gradient for full-length ray
    private Gradient MakeSolidColor(Color color)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(color, 0.0f),
                new GradientColorKey(color, 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 1.0f)
            }
        );
        return gradient;
    }
}
