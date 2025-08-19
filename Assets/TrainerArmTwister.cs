using UnityEngine;
using UnityEngine.InputSystem;

public class TrainerArmTwister : MonoBehaviour
{
    public enum HandMode { Right, Left, Auto }
    [Header("Mode")]
    public HandMode mode = HandMode.Auto;
    [Range(0f, 1f)] public float autoSwitchThreshold = 0.05f; // for Auto hand pick

    // ---------- RIGHT (your original fields) ----------
    [Header("References - RIGHT (original)")]
    public Transform trainerArm;                 // RIGHT forearm/upper arm to twist
    public Transform shoulderJoint;              // RIGHT shoulder to raise
    public InputActionProperty controllerPositionAction; // RIGHT position
    public InputActionProperty controllerRotationAction; // RIGHT rotation

    // ---------- LEFT (new) ----------
    [Header("References - LEFT")]
    public Transform leftTrainerArm;             // LEFT forearm/upper arm to twist
    public Transform leftShoulderJoint;          // LEFT shoulder to raise
    public InputActionProperty leftControllerPositionAction;  // LEFT position
    public InputActionProperty leftControllerRotationAction;  // LEFT rotation

    // ---------- Shared Raise/Twist Settings (UNCHANGED math) ----------
    [Header("Raise Settings")]
    public float minRaiseAngle = 0f;
    public float maxRaiseAngle = 75f;
    public float minY = 1.0f;
    public float maxY = 2.0f;

    [Header("Twist Settings")]
    public float minTwistAngle = -45f;
    public float maxTwistAngle = 45f;
    public float minZRotation = -0.6f;  // From wrist rotation (zDeg/90)
    public float maxZRotation = 0.6f;

    // ---------- Base offsets to keep arms near the sides ----------
    [Header("Base Shoulder Offsets (degrees)")]
    public float rightShoulderBaseAngle = -90f; // put 0 if your rig already rests at sides
    public float leftShoulderBaseAngle  = -90f; // typically same as right

    private HandMode _active;

    void OnEnable()
    {
        controllerPositionAction.action?.Enable();
        controllerRotationAction.action?.Enable();
        leftControllerPositionAction.action?.Enable();
        leftControllerRotationAction.action?.Enable();

        _active = (mode == HandMode.Auto) ? HandMode.Right : mode;
    }

    void OnDisable()
    {
        controllerPositionAction.action?.Disable();
        controllerRotationAction.action?.Disable();
        leftControllerPositionAction.action?.Disable();
        leftControllerRotationAction.action?.Disable();
    }

    void LateUpdate()
    {
        // Decide active hand (anatomical)
        if (mode == HandMode.Auto)
        {
            float rNorm = (controllerPositionAction.action != null)
                ? NormalizeY(controllerPositionAction.action.ReadValue<Vector3>().y) : 0f;
            float lNorm = (leftControllerPositionAction.action != null)
                ? NormalizeY(leftControllerPositionAction.action.ReadValue<Vector3>().y) : 0f;

            float diff = rNorm - lNorm; // >0 -> right higher
            if (_active == HandMode.Right && diff < -autoSwitchThreshold) _active = HandMode.Left;
            else if (_active == HandMode.Left && diff >  autoSwitchThreshold) _active = HandMode.Right;
        }
        else _active = mode;

        if (_active == HandMode.Right)
        {
            DriveRight();                                  // move RIGHT
            Park(leftShoulderJoint, leftTrainerArm, leftShoulderBaseAngle); // park LEFT
        }
        else
        {
            DriveLeft();                                   // move LEFT
            Park(shoulderJoint, trainerArm, rightShoulderBaseAngle);        // park RIGHT
        }
    }

    // ---------- RIGHT: original logic, plus base shoulder offset ----------
    void DriveRight()
    {
        if (controllerPositionAction == null || controllerRotationAction == null ||
            trainerArm == null || shoulderJoint == null ||
            controllerPositionAction.action == null || controllerRotationAction.action == null)
        {
            Debug.LogWarning("[TrainerArmTwister] ❌ Missing RIGHT references!");
            return;
        }

        Vector3 controllerPos = controllerPositionAction.action.ReadValue<Vector3>();
        float clampedY = Mathf.Clamp(controllerPos.y, minY, maxY);
        float normalizedY = Mathf.InverseLerp(minY, maxY, clampedY);
        float raiseAngle = Mathf.Lerp(minRaiseAngle, maxRaiseAngle, normalizedY);

        shoulderJoint.localRotation = Quaternion.Euler(0f, 0f, rightShoulderBaseAngle + raiseAngle);

        Quaternion controllerRot = controllerRotationAction.action.ReadValue<Quaternion>();
        float zRot = controllerRot.eulerAngles.z; if (zRot > 180f) zRot -= 360f;
        float clampedZ = Mathf.Clamp(zRot / 90f, minZRotation, maxZRotation);
        float normalizedZ = Mathf.InverseLerp(minZRotation, maxZRotation, clampedZ);
        float twistAngle = Mathf.Lerp(minTwistAngle, maxTwistAngle, normalizedZ);

        trainerArm.localRotation = Quaternion.Euler(0f, twistAngle, 0f);
    }

    // ---------- LEFT: exact same math on left references, with base offset ----------
    void DriveLeft()
    {
        if (leftControllerPositionAction == null || leftControllerRotationAction == null ||
            leftTrainerArm == null || leftShoulderJoint == null ||
            leftControllerPositionAction.action == null || leftControllerRotationAction.action == null)
        {
            Debug.LogWarning("[TrainerArmTwister] ❌ Missing LEFT references!");
            return;
        }

        Vector3 controllerPos = leftControllerPositionAction.action.ReadValue<Vector3>();
        float clampedY = Mathf.Clamp(controllerPos.y, minY, maxY);
        float normalizedY = Mathf.InverseLerp(minY, maxY, clampedY);
        float raiseAngle = Mathf.Lerp(minRaiseAngle, maxRaiseAngle, normalizedY);

        leftShoulderJoint.localRotation = Quaternion.Euler(0f, 0f, leftShoulderBaseAngle + raiseAngle);

        Quaternion controllerRot = leftControllerRotationAction.action.ReadValue<Quaternion>();
        float zRot = controllerRot.eulerAngles.z; if (zRot > 180f) zRot -= 360f;
        float clampedZ = Mathf.Clamp(zRot / 90f, minZRotation, maxZRotation);
        float normalizedZ = Mathf.InverseLerp(minZRotation, maxZRotation, clampedZ);
        float twistAngle = Mathf.Lerp(minTwistAngle, maxTwistAngle, normalizedZ);

        leftTrainerArm.localRotation = Quaternion.Euler(0f, twistAngle, 0f);
    }

    // Park inactive side at rest
    void Park(Transform shoulder, Transform arm, float baseShoulderAngle)
    {
        if (shoulder) shoulder.localRotation = Quaternion.Euler(0f, 0f, baseShoulderAngle + minRaiseAngle);
        if (arm)      arm.localRotation      = Quaternion.identity;
    }

    float NormalizeY(float y)
    {
        float clampedY = Mathf.Clamp(y, minY, maxY);
        return Mathf.InverseLerp(minY, maxY, clampedY);
    }
}
