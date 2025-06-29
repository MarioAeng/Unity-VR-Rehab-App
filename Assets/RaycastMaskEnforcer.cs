using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class RaycastMaskEnforcer : MonoBehaviour
{
    public Canvas targetCanvas;

    void Start()
    {
        if (targetCanvas == null)
        {
            Debug.LogError("[RaycastMaskEnforcer] No Canvas assigned!");
            return;
        }

        var raycaster = targetCanvas.GetComponent<TrackedDeviceGraphicRaycaster>();
        if (raycaster == null)
        {
            Debug.LogError("[RaycastMaskEnforcer] Canvas is missing TrackedDeviceGraphicRaycaster.");
            return;
        }

        raycaster.blockingMask = LayerMask.GetMask("UI");
        Debug.Log("[RaycastMaskEnforcer] Set blockingMask to UI layer.");
    }
}