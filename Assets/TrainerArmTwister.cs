using UnityEngine;
using UnityEngine.InputSystem;

public class TrainerArmTwister : MonoBehaviour
{
    [Header("References")]
    public Transform trainerArm;                 // The upper arm or forearm to twist
    public Transform shoulderJoint;              // Rotates to simulate raising the arm
    public InputActionProperty controllerPositionAction;
    public InputActionProperty controllerRotationAction;

    [Header("Raise Settings")]
    public float minRaiseAngle = 0f;
    public float maxRaiseAngle = 75f;
    public float minY = 1.0f;
    public float maxY = 2.0f;

    [Header("Twist Settings")]
    public float minTwistAngle = -45f;
    public float maxTwistAngle = 45f;
    public float minZRotation = -0.6f;  // From wrist rotation
    public float maxZRotation = 0.6f;

    void LateUpdate()
    {
        if (controllerPositionAction == null || controllerRotationAction == null ||
            trainerArm == null || shoulderJoint == null)
        {
            Debug.LogWarning("[TrainerArmTwister] ❌ Missing references!");
            return;
        }

        // ✅ Get controller Y position to determine raise angle
        Vector3 controllerPos = controllerPositionAction.action.ReadValue<Vector3>();
        float controllerY = controllerPos.y;
        float clampedY = Mathf.Clamp(controllerY, minY, maxY);
        float normalizedY = Mathf.InverseLerp(minY, maxY, clampedY);
        float raiseAngle = Mathf.Lerp(minRaiseAngle, maxRaiseAngle, normalizedY);

        // ✅ Apply raise to shoulder joint (rotate around Z)
        shoulderJoint.localRotation = Quaternion.Euler(0f, 0f, raiseAngle);

        // ✅ Get controller Z rotation to determine twist angle
        Quaternion controllerRot = controllerRotationAction.action.ReadValue<Quaternion>();
        float zRot = controllerRot.eulerAngles.z;
        if (zRot > 180f) zRot -= 360f;

        float clampedZ = Mathf.Clamp(zRot / 90f, minZRotation, maxZRotation);
        float normalizedZ = Mathf.InverseLerp(minZRotation, maxZRotation, clampedZ);
        float twistAngle = Mathf.Lerp(minTwistAngle, maxTwistAngle, normalizedZ);

        // ✅ Apply twist to trainer arm (rotate around Y)
        trainerArm.localRotation = Quaternion.Euler(0f, twistAngle, 0f);

        Debug.Log($"[TrainerArmTwister] Raise: {raiseAngle:F1}°  |  Twist: {twistAngle:F1}°  |  ZRot: {zRot:F1}°");
    }
}
