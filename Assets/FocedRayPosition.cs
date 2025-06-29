using UnityEngine;

public class ForcedRayPosition : MonoBehaviour
{
    [Header("Fixed World Position")]
    public Vector3 fixedWorldPosition = new Vector3(0, 1.3f, 0.5f);

    [Header("Fixed World Rotation (Euler)")]
    public Vector3 fixedEulerRotation = new Vector3(0, 180, 0);

    [Header("Locking Options")]
    public bool overridePosition = false;
    public bool overrideRotation = false;

    [Header("Debug Options")]
    public bool logPositionChanges = true;
    public bool logRotationChanges = true;

    private Vector3 lastLoggedPosition;
    private Quaternion lastLoggedRotation;

    void LateUpdate()
    {
        if (overridePosition)
        {
            transform.position = fixedWorldPosition;

            if (logPositionChanges && transform.position != lastLoggedPosition)
            {
                Debug.Log($"[ForcedRayPosition] Position overridden to {transform.position}");
                lastLoggedPosition = transform.position;
            }
        }

        if (overrideRotation)
        {
            transform.rotation = Quaternion.Euler(fixedEulerRotation);

            if (logRotationChanges && transform.rotation != lastLoggedRotation)
            {
                Debug.Log($"[ForcedRayPosition] Rotation overridden to {transform.rotation.eulerAngles}");
                lastLoggedRotation = transform.rotation;
            }
        }

        // Visualize forward ray
        Debug.DrawRay(transform.position, transform.forward * 5f, Color.cyan);
    }
}