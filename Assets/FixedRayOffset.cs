using UnityEngine;

public class FixedRayOffset : MonoBehaviour
{
    [Header("References")]
    public Transform baseTransform; // Assign MainSelectorHand here in Inspector

    [Header("Offset Settings")]
    public Vector3 offset = new Vector3(0f, -1.2f, 0.2f); // Drop Y heavily, adjust as needed
    public bool logPosition = true;

    void LateUpdate()
    {
        if (baseTransform == null)
        {
            Debug.LogWarning("[RayFix] No baseTransform assigned.");
            return;
        }

        transform.position = baseTransform.TransformPoint(offset);
        transform.rotation = baseTransform.rotation;

        if (logPosition)
        {
            Debug.Log($"[RayFix] Final Pos: {transform.position}, Y: {transform.position.y}");
        }

        Debug.DrawRay(transform.position, transform.forward * 3, Color.green);
    }
}