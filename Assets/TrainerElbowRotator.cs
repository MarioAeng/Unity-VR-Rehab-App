using UnityEngine;
using UnityEngine.InputSystem;

public class TrainerElbowRotator : MonoBehaviour
{
    public enum HandMode { Right, Left, Auto }
    public enum Axis { X, Y, Z }

    [Header("Active Hand")]
    public HandMode mode = HandMode.Auto;
    [Range(0f, 0.2f)] public float autoSwitchThreshold = 0.02f;
    public bool swapSidesIfNeeded = false;

    [Header("Controller Positions (Input System)")]
    public InputActionProperty rightControllerPositionAction;
    public InputActionProperty leftControllerPositionAction;

    [Header("Bones (use UPPER ARM/SHOULDER pivots)")]
    public Transform rightShoulderPivot;
    public Transform leftShoulderPivot;
    public Transform rightTrainerElbow;   // fallback
    public Transform leftTrainerElbow;    // fallback

    [Header("Horizontal Input Range (hand X)")]
    public float minX = -0.30f;
    public float maxX =  0.30f;

    [Header("Yaw (across torso)")]
    public float minYRotation = -60f;
    public float maxYRotation =  60f;
    public float restYRotation =   0f;
    public float smoothSpeed   =   6f;

    [Header("Movement Gain")]
    public float inputGain = 1.6f;
    public float angleGain = 2.0f;
    public float maxAbsRotation = 120f;

    [Header("Axes")]
    public Axis rightYawAxis = Axis.Z;   // across-body sweep
    public Axis leftYawAxis  = Axis.Z;
    public bool invertRightAngle = false;
    public bool invertLeftAngle  = false;

    // ---------- NEW: constant forward bias + extra tilt when sweeping LEFT ----------
    [Header("Forward Tilt (keeps arm in front)")]
    public float baseForwardTiltDeg   = 12f;   // always on (10–18° works well)
    public float extraTiltAtLeftDeg   = 24f;   // added when hand goes left
    public Axis  rightTiltAxis        = Axis.Y; // the axis that pitches arm TOWARD camera
    public Axis  leftTiltAxis         = Axis.Y;
    public bool  invertRightTilt      = false; // tick if it tilts backward
    public bool  invertLeftTilt       = false;

    [Header("Debug")]
    public bool driveBothArms = false;
    public bool debugLogs = false;

    private HandMode _active = HandMode.Right;
    private Transform _rPivot, _lPivot;
    private Quaternion _rBaseRot, _lBaseRot;
    private float _curRightYaw, _curLeftYaw;
    private float _curRightTilt, _curLeftTilt;

    void Awake()
    {
        _rPivot = rightShoulderPivot ? rightShoulderPivot : rightTrainerElbow;
        _lPivot = leftShoulderPivot  ? leftShoulderPivot  : leftTrainerElbow;
        if (_rPivot) _rBaseRot = _rPivot.localRotation;
        if (_lPivot) _lBaseRot = _lPivot.localRotation;
    }

    void OnEnable(){ rightControllerPositionAction.action?.Enable(); leftControllerPositionAction.action?.Enable();
        _active = mode == HandMode.Auto ? HandMode.Right : mode; }
    void OnDisable(){ rightControllerPositionAction.action?.Disable(); leftControllerPositionAction.action?.Disable(); }

    void LateUpdate()
    {
        if (!_rPivot || !_lPivot) return;

        Vector3 rPos = rightControllerPositionAction.action?.ReadValue<Vector3>() ?? Vector3.zero;
        Vector3 lPos = leftControllerPositionAction.action?.ReadValue<Vector3>() ?? Vector3.zero;

        float rAmt = Mathf.Clamp(SignedNormalizeX(rPos.x) * inputGain, -1f, 1f); // -1 left, +1 right
        float lAmt = Mathf.Clamp(SignedNormalizeX(lPos.x) * inputGain, -1f, 1f);

        if (mode == HandMode.Auto)
        {
            if (_active == HandMode.Right && Mathf.Abs(lAmt) > Mathf.Abs(rAmt) + autoSwitchThreshold) _active = HandMode.Left;
            else if (_active == HandMode.Left && Mathf.Abs(rAmt) > Mathf.Abs(lAmt) + autoSwitchThreshold) _active = HandMode.Right;
        }
        else _active = mode;

        float rYaw = Amplified(minYRotation, maxYRotation, rAmt, angleGain) * (invertRightAngle ? -1f : 1f);
        float lYaw = Amplified(minYRotation, maxYRotation, lAmt, angleGain) * (invertLeftAngle  ? -1f : 1f);
        rYaw = Mathf.Clamp(rYaw, -maxAbsRotation, maxAbsRotation);
        lYaw = Mathf.Clamp(lYaw, -maxAbsRotation, maxAbsRotation);

        // constant forward bias + extra when moving left
        float rTiltTarget = (baseForwardTiltDeg + Mathf.Clamp01(-rAmt) * extraTiltAtLeftDeg) * (invertRightTilt ? -1f : 1f);
        float lTiltTarget = (baseForwardTiltDeg + Mathf.Clamp01(-lAmt) * extraTiltAtLeftDeg) * (invertLeftTilt  ? -1f : 1f);

        HandMode effActive = swapSidesIfNeeded ? (_active == HandMode.Right ? HandMode.Left : HandMode.Right) : _active;

        if (driveBothArms || effActive == HandMode.Right)
        {
            _curRightYaw  = Mathf.Lerp(_curRightYaw,  rYaw,        Time.deltaTime * smoothSpeed);
            _curRightTilt = Mathf.Lerp(_curRightTilt, rTiltTarget, Time.deltaTime * smoothSpeed);
        }
        else
        {
            _curRightYaw  = Mathf.Lerp(_curRightYaw,  restYRotation, Time.deltaTime * smoothSpeed);
            _curRightTilt = Mathf.Lerp(_curRightTilt, baseForwardTiltDeg * (invertRightTilt ? -1f : 1f), Time.deltaTime * smoothSpeed);
        }

        if (driveBothArms || effActive == HandMode.Left)
        {
            _curLeftYaw  = Mathf.Lerp(_curLeftYaw,  lYaw,        Time.deltaTime * smoothSpeed);
            _curLeftTilt = Mathf.Lerp(_curLeftTilt, lTiltTarget, Time.deltaTime * smoothSpeed);
        }
        else
        {
            _curLeftYaw  = Mathf.Lerp(_curLeftYaw,  restYRotation, Time.deltaTime * smoothSpeed);
            _curLeftTilt = Mathf.Lerp(_curLeftTilt, baseForwardTiltDeg * (invertLeftTilt ? -1f : 1f), Time.deltaTime * smoothSpeed);
        }

        if (_rPivot) _rPivot.localRotation = _rBaseRot
            * AxisDelta(rightYawAxis, _curRightYaw)
            * AxisDelta(rightTiltAxis, _curRightTilt);

        if (_lPivot) _lPivot.localRotation = _lBaseRot
            * AxisDelta(leftYawAxis, _curLeftYaw)
            * AxisDelta(leftTiltAxis, _curLeftTilt);

        if (debugLogs) Debug.Log($"[Elbow] rYaw={_curRightYaw:F1} rTilt={_curRightTilt:F1} | lYaw={_curLeftYaw:F1} lTilt={_curLeftTilt:F1}");
    }

    // helpers
    float SignedNormalizeX(float x){ float t = Mathf.InverseLerp(minX, maxX, Mathf.Clamp(x, minX, maxX)); return t * 2f - 1f; }
    float Amplified(float min, float max, float signed01, float gain)
    { float mid = 0.5f * (min + max); float baseAngle = Mathf.Lerp(min, max, 0.5f + 0.5f * signed01); return mid + (baseAngle - mid) * gain; }
    Quaternion AxisDelta(Axis a, float angle)
    { switch (a){ case Axis.X: return Quaternion.Euler(angle,0,0); case Axis.Y: return Quaternion.Euler(0,angle,0); default: return Quaternion.Euler(0,0,angle);} }
}
