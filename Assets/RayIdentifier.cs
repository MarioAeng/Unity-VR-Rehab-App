using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RayIdentifier : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== Ray Identifier Report ===");

        // Check all LineRenderers
        var lineRenderers = FindObjectsOfType<LineRenderer>(true);
        foreach (var lr in lineRenderers)
        {
            Debug.Log($"[LineRenderer] {lr.gameObject.name} | Active: {lr.enabled} | Parent: {lr.transform.parent?.name}");
        }

        // Check XR Line Visuals
        var xrLineVisuals = FindObjectsOfType<XRInteractorLineVisual>(true);
        foreach (var visual in xrLineVisuals)
        {
            Debug.Log($"[XRInteractorLineVisual] {visual.gameObject.name} | Enabled: {visual.enabled} | Parent: {visual.transform.parent?.name}");
        }

        // Check XR Ray Interactors
        var xrRayInteractors = FindObjectsOfType<XRRayInteractor>(true);
        foreach (var ray in xrRayInteractors)
        {
            Debug.Log($"[XRRayInteractor] {ray.gameObject.name} | Enabled: {ray.enabled} | Parent: {ray.transform.parent?.name}");
        }

        Debug.Log("=== End of Ray Identifier Report ===");
    }
}