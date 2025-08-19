using UnityEngine;
using UnityEngine.InputSystem;

public class TrainerArmRotator : MonoBehaviour
{
    public enum HandMode { Right, Left, Auto }

    [Header("Active Hand")]
    public HandMode mode = HandMode.Auto;
    [Range(0f, 1f)] public float autoSwitchThreshold = 0.05f; // hysteresis for Auto

    [Header("Controller Positions (Input System)")]
    public InputActionProperty rightControllerPositionAction; // <XRController>{RightHand}/devicePosition
    public InputActionProperty leftControllerPositionAction;  // <XRController>{LeftHand}/devicePosition

    [Header("Trainer Arm Bones")]
    public Transform rightTrainerArm;      // upper-arm (right)
    public Transform leftTrainerArm;       // upper-arm (left)

    [Header("Raise Settings (same as your original)")]
    public float minRaiseAngle = 0f;       // fully down
    public float maxRaiseAngle = 75f;      // full raise
    public float minY = 1.0f;              // start height
    public float maxY = 2.0f;              // end height

    [Header("Direction Tweaks")]
    public bool invertLeftArm = true;      // ← turn ON if left moved down when controller went up
    public bool invertRightArm = false;    // usually off

    private HandMode _active = HandMode.Right;

    void OnEnable()
    {
        rightControllerPositionAction.action?.Enable();
        leftControllerPositionAction.action?.Enable();
        _active = mode == HandMode.Auto ? HandMode.Right : mode;
    }

    void OnDisable()
    {
        rightControllerPositionAction.action?.Disable();
        leftControllerPositionAction.action?.Disable();
    }

    void LateUpdate()
    {
        if (!rightTrainerArm || !leftTrainerArm) return;

        Vector3 rPos = rightControllerPositionAction.action != null
            ? rightControllerPositionAction.action.ReadValue<Vector3>() : Vector3.zero;
        Vector3 lPos = leftControllerPositionAction.action != null
            ? leftControllerPositionAction.action.ReadValue<Vector3>() : Vector3.zero;

        float rNorm = NormalizeY(rPos.y);
        float lNorm = NormalizeY(lPos.y);

        if (mode == HandMode.Auto)
        {
            float diff = rNorm - lNorm;
            if (_active == HandMode.Right && diff < -autoSwitchThreshold) _active = HandMode.Left;
            else if (_active == HandMode.Left && diff > autoSwitchThreshold) _active = HandMode.Right;
        }
        else _active = mode;

        float rAngle = Mathf.Lerp(minRaiseAngle, maxRaiseAngle, rNorm) * (invertRightArm ? -1f : 1f);
        float lAngle = Mathf.Lerp(minRaiseAngle, maxRaiseAngle, lNorm) * (invertLeftArm  ? -1f : 1f);

        if (_active == HandMode.Right)
        {
            SetArmAngle(rightTrainerArm, rAngle);
            SetArmAngle(leftTrainerArm,  minRaiseAngle);   // keep unused arm fully down
        }
        else // Left
        {
            SetArmAngle(leftTrainerArm,  lAngle);
            SetArmAngle(rightTrainerArm, minRaiseAngle);   // keep unused arm fully down
        }
    }

    float NormalizeY(float y)
    {
        float clampedY = Mathf.Clamp(y, minY, maxY);
        return Mathf.InverseLerp(minY, maxY, clampedY);
    }

    void SetArmAngle(Transform arm, float angle)
    {
        // Your original axis: raise around Z
        arm.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
