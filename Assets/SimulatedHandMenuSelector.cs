using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class SimulatedHandMenuSelector : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty triggerAction;

    [Header("References")]
    public XRRayInteractor rayInteractor;

    private bool wasTriggerPressed = false;

    void Update()
    {
        // Safety checks
        if (triggerAction.action == null)
        {
            Debug.LogWarning("[SimHand] Trigger Action not assigned.");
            return;
        }

        if (rayInteractor == null)
        {
            Debug.LogWarning("[SimHand] RayInteractor not assigned.");
            return;
        }

        // Read trigger state
        float triggerValue = triggerAction.action.ReadValue<float>();
        bool isTriggerPressed = triggerValue > 0.5f;

        // Raycast UI check
        if (rayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult result))
        {
            if (result.gameObject != null)
            {
                Debug.Log($"[SimHand] Hovering over UI: {result.gameObject.name}");

                if (isTriggerPressed && !wasTriggerPressed)
                {
                    Button button = result.gameObject.GetComponent<Button>();
                    if (button != null)
                    {
                        Debug.Log($"[SimHand] Trigger clicked: {button.name}");
                        button.onClick.Invoke();
                    }
                    else
                    {
                        Debug.LogWarning($"[SimHand] Raycast hit {result.gameObject.name}, but it has no Button component.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("[SimHand] Raycast result returned null gameObject.");
            }
        }
        else
        {
            Debug.LogWarning("[SimHand] Raycast did not hit any UI.");
        }

        wasTriggerPressed = isTriggerPressed;
    }
}
