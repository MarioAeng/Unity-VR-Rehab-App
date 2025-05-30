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

    private bool wasTriggerPressed = false;

    void OnEnable()
    {
        if (triggerAction.action != null)
        {
            triggerAction.action.Enable();
            Debug.Log("[SimHand] TriggerAction enabled manually.");
        }
        else
        {
            Debug.LogError("[SimHand] TriggerAction is null in OnEnable!");
        }
    }

    void Update()
    {
        if (triggerAction.action == null)
        {
            Debug.LogError("[SimHand] TriggerAction is not assigned.");
            return;
        }

        float triggerValue = triggerAction.action.ReadValue<float>();
        bool isTriggerPressed = triggerValue > 0.5f;

        Debug.Log($"[SimHand] Trigger Value = {triggerValue}");

        if (rayInteractor == null)
        {
            Debug.LogError("[SimHand] rayInteractor is not assigned.");
            return;
        }

        if (rayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult result))
        {
            if (result.gameObject != null)
            {
                Debug.Log($"[SimHand] Hovering over UI: {result.gameObject.name}");

                if (isTriggerPressed && !wasTriggerPressed)
                {
                    Debug.Log("[SimHand] Trigger pressed — submitting click.");
                    ExecuteEvents.Execute(result.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
                }
            }
            else
            {
                Debug.Log("[SimHand] Raycast hit nothing.");
            }
        }

        wasTriggerPressed = isTriggerPressed;
    }
}
