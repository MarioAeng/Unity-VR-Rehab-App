using UnityEngine;
using UnityEngine.InputSystem;

public class TrainerElbowRotator : MonoBehaviour
{
    [Header("References")]
    public Transform trainerElbow;  // The arm bone to rotate
    public InputActionProperty controllerPositionAction;

    [Header("Horizontal Movement Settings")]
    public float minX = -0.3f;  // Leftmost hand X position
    public float maxX = 0.3f;   // Rightmost hand X position

    [Header("Rotation Settings")]
    public float minYRotation = -45f; // Arm rotated left
    public float maxYRotation = 45f;  // Arm rotated right
    public float smoothSpeed = 5f;

    private float currentYRotation;

    void LateUpdate()
    {
        if (controllerPositionAction == null || trainerElbow == null)
        {
            Debug.LogWarning("[TrainerElbowRotator] ❌ Missing references!");
            return;
        }

        Vector3 handPosition = controllerPositionAction.action.ReadValue<Vector3>();
        float clampedX = Mathf.Clamp(handPosition.x, minX, maxX);
        float normalizedX = Mathf.InverseLerp(minX, maxX, clampedX);
        float targetYRotation = Mathf.Lerp(minYRotation, maxYRotation, normalizedX);

        currentYRotation = Mathf.Lerp(currentYRotation, targetYRotation, Time.deltaTime * smoothSpeed);
        trainerElbow.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);

        Debug.Log($"[TrainerElbowRotator] Hand X: {handPosition.x:F2}, Rot Y: {currentYRotation:F2}");
    }
}