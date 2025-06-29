using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class DebugUIRaycast : MonoBehaviour
{
    public Camera uiCamera;
    public InputActionProperty triggerAction;

    void Update()
    {
        if (triggerAction.action == null || uiCamera == null)
            return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenCenter
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            Debug.Log($"[UIRaycast] Hit {results[0].gameObject.name}");
        }
        else
        {
            Debug.Log("[UIRaycast] No hit at screen center.");
        }
    }
}