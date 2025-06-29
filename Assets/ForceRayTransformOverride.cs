using UnityEngine;

public class ForceRayTransformOverride : MonoBehaviour
{
    [Header("Reference for Offset")]
    public Transform positionSource;  // Assign something like RightController or MainSelectorHand

    [Header("Position Offset (Local)")]
    public Vector3 localOffset = new Vector3(0f, -0.3f, 0.1f);

    void LateUpdate()
    {
        if (positionSource != null)
        {
            // Apply offset from the XR-tracked source but keep the source's original rotation
            transform.position = positionSource.TransformPoint(localOffset);
            transform.rotation = positionSource.rotation;

            Debug.Log($"[ForceRay] Base Pos: {positionSource.position} | Offset Pos: {transform.position}");
        }
        else
        {
            Debug.LogWarning("[ForceRay] Position source not assigned!");
        }
    }
}