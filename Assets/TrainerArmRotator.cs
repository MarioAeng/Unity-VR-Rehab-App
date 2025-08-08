using UnityEngine;
using UnityEngine.InputSystem;

public class TrainerArmRotator : MonoBehaviour
{
    [Header("References")]
    public Transform trainerArm;                 // The upper arm to rotate
    public Transform shoulderJoint;              // Shoulder pivot (parent of trainerArm)
    public InputActionProperty controllerPositionAction;

    [Header("Raise Settings")]
    public float minRaiseAngle = 0f;
    public float maxRaiseAngle = 75f;
    public float minY = 1.0f;        // Adjust based on starting hand height
    public float maxY = 2.0f;        // Adjust based on full raise height

    void LateUpdate()
    {
        if (controllerPositionAction == null || trainerArm == null || shoulderJoint == null)
        {
            Debug.LogWarning("[TrainerArmRotator] Missing references!");
            return;
        }

        Vector3 controllerWorldPos = controllerPositionAction.action.ReadValue<Vector3>();
        float controllerY = controllerWorldPos.y;

        float clampedY = Mathf.Clamp(controllerY, minY, maxY);
        float normalized = Mathf.InverseLerp(minY, maxY, clampedY);
        float targetAngle = Mathf.Lerp(minRaiseAngle, maxRaiseAngle, normalized);

        // ✅ Rotate around Z to raise arm forward (instead of backward or sideways)
        trainerArm.localRotation = Quaternion.Euler(0f, 0f, targetAngle);

        Debug.Log($"[TrainerArmRotator] Y = {controllerY:F2}, Angle = {targetAngle:F2}");
    }
}