using UnityEngine;

public class RayOffsetController : MonoBehaviour
{
    [Header("Ray Offset Settings")]
    public Transform originalParent;  // Usually MainSelectorHand or XR controller
    public Vector3 offset = new Vector3(0f, -0.2f, 0f); // Adjust this Y value to move the ray lower

    [Header("Debugging")]
    public bool enableDebugLogs = true;

    void LateUpdate()
    {
        if (originalParent == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[RayOffset] No parent assigned.");
            return;
        }

        Vector3 newPosition = originalParent.position + originalParent.TransformVector(offset);
        transform.position = newPosition;

        if (enableDebugLogs)
        {
            Debug.Log($"[RayOffset] Parent Pos: {originalParent.position} | Offset: {offset} | Result: {transform.position}");
        }
    }
}