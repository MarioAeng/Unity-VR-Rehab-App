using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DebugRayLogger : MonoBehaviour
{
    public XRRayInteractor rayInteractor;

    void Start()
    {
        if (rayInteractor == null)
        {
            rayInteractor = GetComponent<XRRayInteractor>();
        }

        if (rayInteractor == null)
        {
            Debug.LogError("[DebugRayLogger] XRRayInteractor not assigned or found.");
        }
        else
        {
            Debug.Log("[DebugRayLogger] XRRayInteractor assigned: " + rayInteractor.name);
        }
    }

    void Update()
    {
        if (rayInteractor == null)
        {
            Debug.LogError("[DebugRayLogger] XRRayInteractor is missing.");
            return;
        }

        // Check component states
        Debug.Log($"[DebugRayLogger] Interactor Active: {rayInteractor.gameObject.activeSelf}, Enabled: {rayInteractor.enabled}");

        // Ray origin & direction
        Vector3 origin = rayInteractor.transform.position;
        Vector3 direction = rayInteractor.transform.forward;
        Debug.DrawRay(origin, direction * rayInteractor.maxRaycastDistance, Color.yellow);

        Debug.Log($"[DebugRayLogger] Origin: {origin}, Direction: {direction}");

        // Try hit info
        if (rayInteractor.TryGetHitInfo(out Vector3 hitPos, out Vector3 hitNormal, out int _, out bool validTarget))
        {
            Debug.Log($"[DebugRayLogger] Hit Position: {hitPos}, Valid: {validTarget}");
        }
        else
        {
            Debug.LogWarning("[DebugRayLogger] No object hit by XRRayInteractor.");
        }

        // Try UI hit
        if (rayInteractor.TryGetCurrentUIRaycastResult(out UnityEngine.EventSystems.RaycastResult result) && result.gameObject != null)
        {
            Debug.Log($"[DebugRayLogger] UI Hover: {result.gameObject.name}");
        }
        else
        {
            Debug.Log("[DebugRayLogger] UI Raycast: no hit.");
        }
    }
}
