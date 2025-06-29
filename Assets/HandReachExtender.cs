using UnityEngine;
using UnityEngine.InputSystem;

public class HandReachExtender : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionProperty positionAction;
    public InputActionProperty rotationAction;

    [Header("Reach Settings")]
    public float exaggerationX = 6.0f;   // MASSIVELY exaggerated X (left/right)
    public float exaggerationY = 2.5f;
    public float exaggerationZ = 1.5f;
    public Vector3 fixedOffset = new Vector3(0, -0.4f, 0.2f); // Optional origin shift

    private Vector3 initialPosition;
    private bool initialized = false;

    void Update()
    {
        if (positionAction.action == null || rotationAction.action == null)
            return;

        Vector3 inputPos = positionAction.action.ReadValue<Vector3>();
        Quaternion inputRot = rotationAction.action.ReadValue<Quaternion>();

        if (!initialized)
        {
            initialPosition = inputPos;
            initialized = true;
            Debug.Log("[HandReachExtender] Initialized at: " + initialPosition);
        }

        Vector3 delta = inputPos - initialPosition;

        // Per-axis exaggeration
        Vector3 exaggerated = new Vector3(
            initialPosition.x + delta.x * exaggerationX,
            initialPosition.y + delta.y * exaggerationY,
            initialPosition.z + delta.z * exaggerationZ
        ) + fixedOffset;

        transform.position = exaggerated;
        transform.rotation = inputRot;

        Debug.DrawRay(transform.position, transform.forward * 3f, Color.cyan);
        Debug.Log($"[HandReachExtender] exaggeratedPos: {exaggerated:F2}, deltaX: {delta.x:F2}, rawX: {inputPos.x:F2}");
    }
}