using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SimulatedHandController : MonoBehaviour
{
    [Header("Setup")]
    public TMP_Text trainerPrompt;
    public TMP_Text repCounterText;

    [Header("Rep Settings")]
    public float positionThreshold = 0.2f;
    public float requiredHoldTime = 1.5f;

    [Header("Target Position (Calibration-based)")]
    public float targetX = 0f;
    public float targetY = 1.3f;

    [Header("Input")]
    public InputActionAsset inputActionAsset;
    public string gripActionName = "GripAction";
    public string triggerActionName = "TriggerAction";
    public string resetActionName = "ResetAction";
    public string positionActionName = "RightHandPosition";
    public string rotationActionName = "RightHandRotation";

    // NEW: Left-hand bindings + handedness flag
    [Header("Left-hand Input (used when HandednessLeft == 1)")]
    public string leftTriggerActionName = "LeftTriggerAction";
    public string leftResetActionName = "LeftResetAction";
    public string leftPositionActionName = "LeftHandPosition";
    public string leftRotationActionName = "LeftHandRotation";
    [Tooltip("PlayerPrefs key set by your handedness selector: 1 = Left, 0 = Right")]
    public string handednessPrefKey = "HandednessLeft";

    [Header("Input Thresholds")]
    [Tooltip("Axis >= this counts as pressed")]
    public float triggerPressThreshold = 0.5f;
    public float resetPressThreshold = 0.5f;

    [Header("Calibration Gate")]
    [Tooltip("Ignore input for a brief moment after scene load")]
    public float inputBlockSeconds = 0.35f;

    [Header("Anti-cheat (Wrist curl / elbow-only raise)")]
    [Tooltip("Enable rotation and travel checks to prevent wrist-only 'cheats'.")]
    public bool enableAntiCheat = true;

    [Tooltip("Max allowed rotation change from calibration during the hold window (degrees). Lower = stricter.")]
    public float maxRotationDeltaDegrees = 30f;

    [Tooltip("User must drop this far below target before a rep can start (the 'start window').")]
    public float armStartWindow = 0.25f;

    [Tooltip("Minimum vertical travel from the armed start to near-target before a rep can be counted.")]
    public float minVerticalTravel = 0.25f;

    [Tooltip("Max allowed horizontal drift (x) from where the rep was armed.")]
    public float horizontalDriftLimit = 0.18f;

    [Tooltip("Optional: require that the approach to target is mostly upward (ignores short jitters). 0 disables.")]
    public float minUpwardVelocity = 0.0f;

    // actions
    private InputAction gripAction, triggerAction, resetAction, positionAction, rotationAction;

    // state
    private int repCount = 0;
    private float holdTimer = 0f;
    private bool isInCorrectPose = false;
    private bool waitingForReset = false;
    private Vector3 offset = new Vector3(0f, -0.4f, 0.3f);

    // calibration gating
    private bool isCalibrated = false;
    private bool calibrationArmed = false;   // becomes true once trigger has been released after load
    private float sceneStartTime;

    // edge detection for axis-type inputs
    private bool triggerWasDown = false;
    private bool resetWasDown = false;

    // anti-cheat helpers
    private Quaternion calibRotation;
    private bool repArmed = false;
    private float startY = 0f;
    private float startX = 0f;
    private float maxYDuringRep = 0f;
    private float lastY = 0f;
    private float upwardVelEMA = 0f;     // simple EMA for vertical velocity smoothing
    private const float velEmaAlpha = 0.25f;

    private void OnEnable()
    {
        if (inputActionAsset == null)
        {
            Debug.LogError("[SimHand] InputActionAsset not assigned in Inspector.");
            return;
        }

        var map = inputActionAsset.FindActionMap("SimulatedHandMap");
        if (map == null)
        {
            Debug.LogError("[SimHand] Could not find action map 'SimulatedHandMap'.");
            return;
        }

        // NEW: Decide which action names to bind based on handedness
        bool useLeft = PlayerPrefs.GetInt(handednessPrefKey, 0) == 1;

        string trigName = useLeft ? leftTriggerActionName  : triggerActionName;
        string rstName  = useLeft ? leftResetActionName    : resetActionName;
        string posName  = useLeft ? leftPositionActionName : positionActionName;
        string rotName  = useLeft ? leftRotationActionName : rotationActionName;

        gripAction     = map.FindAction(gripActionName);
        triggerAction  = map.FindAction(trigName);
        resetAction    = map.FindAction(rstName);
        positionAction = map.FindAction(posName);
        rotationAction = map.FindAction(rotName);

        gripAction?.Enable();
        triggerAction?.Enable();
        resetAction?.Enable();
        positionAction?.Enable();
        rotationAction?.Enable();

        sceneStartTime = Time.unscaledTime;
        calibrationArmed = false;         // force a fresh release before we accept a press
        isCalibrated = false;
        triggerWasDown = IsTriggerDown(); // capture current (likely pressed) so we don't edge-detect it
        resetWasDown = IsResetDown();

        repArmed = false;
        holdTimer = 0f;

        if (repCounterText) repCounterText.text = "Reps: 0";
        if (trainerPrompt)  trainerPrompt.text = "Raise your hand to the desired height and press trigger to calibrate.";

        Debug.Log($"[SimHand] Using {(useLeft ? "LEFT" : "RIGHT")} actions | " +
                  $"Pos:{positionAction?.name} Rot:{rotationAction?.name} Trig:{triggerAction?.name} Reset:{resetAction?.name}");
    }

    private void OnDisable()
    {
        gripAction?.Disable();
        triggerAction?.Disable();
        resetAction?.Disable();
        positionAction?.Disable();
        rotationAction?.Disable();
    }

    void Update()
    {
        if (positionAction == null || rotationAction == null)
        {
            Debug.LogWarning("[SimHand] Position or rotation action missing.");
            return;
        }

        // Drive the simulated hand from input
        Vector3 rawPosition = positionAction.ReadValue<Vector3>();
        Quaternion rotation = rotationAction.ReadValue<Quaternion>();
        Vector3 adjustedPosition = rawPosition + offset;
        transform.SetPositionAndRotation(adjustedPosition, rotation);

        // Track vertical velocity (EMA) for optional upward-motion requirement
        float curY = transform.position.y;
        float rawVel = (curY - lastY) / Mathf.Max(Time.deltaTime, 1e-4f);
        upwardVelEMA = Mathf.Lerp(upwardVelEMA, rawVel, velEmaAlpha);
        lastY = curY;

        // --- CALIBRATION PHASE ---
        if (!HandleCalibrationGate()) return;  // returns false until armed; keeps prompt visible
        if (!isCalibrated)
        {
            if (trainerPrompt) trainerPrompt.text = "Raise your hand to the desired height and press trigger to calibrate.";
            if (TriggerPressedThisFrame())
            {
                targetY = transform.position.y;
                calibRotation = rotation;   // store neutral wrist orientation at calibration
                isCalibrated = true;
                holdTimer = 0f;
                waitingForReset = false;
                repArmed = false;
                upwardVelEMA = 0f;
                if (trainerPrompt) trainerPrompt.text = "Calibration complete. Start raising your hand.";
                Debug.Log($"[SimHand] Calibrated height set to Y = {targetY:F2}.");
            }
            return; // wait here until calibrated
        }

        // --- REP LOGIC WITH ANTI-CHEAT ---
        Vector3 currentPos = transform.position;
        float deltaX = currentPos.x - targetX;
        float deltaY = currentPos.y - targetY;
        float distance = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);

        if (waitingForReset)
        {
            // Require a clear drop below target before allowing the next rep
            if (currentPos.y < targetY - (armStartWindow * 0.6f))
            {
                waitingForReset = false;
                if (trainerPrompt) trainerPrompt.text = "Raise your hand to begin again.";
            }
            else
            {
                if (trainerPrompt) trainerPrompt.text = "Return to rest position.";
            }
        }
        else
        {
            // Arm a rep only after the user has dropped sufficiently below target
            if (!repArmed)
            {
                if (currentPos.y <= targetY - armStartWindow)
                {
                    repArmed = true;
                    startY = currentPos.y;
                    startX = currentPos.x;
                    maxYDuringRep = currentPos.y;
                    holdTimer = 0f;
                    if (trainerPrompt) trainerPrompt.text = "Now raise your hand.";
                }
                else
                {
                    if (trainerPrompt) trainerPrompt.text = "Lower your hand to start the rep.";
                }
            }
            else
            {
                // Update travel metrics while rep is armed
                if (currentPos.y > maxYDuringRep) maxYDuringRep = currentPos.y;
                float verticalTravel = maxYDuringRep - startY;
                float horizontalDrift = Mathf.Abs(currentPos.x - startX);

                bool nearTarget = distance <= positionThreshold;
                bool rotationOK = true;
                bool travelOK = true;
                bool driftOK = true;
                bool upwardOK = true;

                if (enableAntiCheat)
                {
                    // 1) Rotation must stay close to calibration orientation
                    float rotDelta = Quaternion.Angle(rotation, calibRotation);
                    rotationOK = rotDelta <= maxRotationDeltaDegrees;

                    // 2) Must have actually traveled upward a minimum distance
                    travelOK = verticalTravel >= minVerticalTravel;

                    // 3) Avoid sliding sideways to reach target
                    driftOK = horizontalDrift <= horizontalDriftLimit;

                    // 4) Mostly-upward approach (optional; set minUpwardVelocity = 0 to disable)
                    if (minUpwardVelocity > 0f)
                        upwardOK = upwardVelEMA >= minUpwardVelocity;
                }

                if (nearTarget && rotationOK && travelOK && driftOK && upwardOK)
                {
                    holdTimer += Time.deltaTime;
                    if (trainerPrompt) trainerPrompt.text = "Hold that position!";
                    if (!isInCorrectPose && holdTimer >= requiredHoldTime)
                    {
                        isInCorrectPose = true;
                        waitingForReset = true;
                        repArmed = false; // arm again next cycle
                        repCount++;
                        if (repCounterText) repCounterText.text = $"Reps: {repCount}";
                        Debug.Log($"[SimHand] Rep #{repCount} complete.");
                    }
                }
                else
                {
                    // Not valid to count; reset hold window and guide the user
                    holdTimer = 0f;
                    isInCorrectPose = false;

                    if (!rotationOK)
                    {
                        if (trainerPrompt) trainerPrompt.text = "Keep your wrist neutral (avoid curling).";
                    }
                    else if (!travelOK)
                    {
                        if (trainerPrompt) trainerPrompt.text = "Raise from lower to higher using your shoulder.";
                    }
                    else if (!driftOK)
                    {
                        if (trainerPrompt) trainerPrompt.text = "Avoid moving sideways; lift mostly straight up.";
                    }
                    else if (!nearTarget)
                    {
                        if (Mathf.Abs(deltaY) > positionThreshold)
                            if (trainerPrompt) trainerPrompt.text = deltaY > 0 ? "Lower your hand slightly." : "Raise your hand higher.";
                        else if (Mathf.Abs(deltaX) > positionThreshold)
                            if (trainerPrompt) trainerPrompt.text = deltaX > 0 ? "Move left." : "Move right.";
                        else
                            if (trainerPrompt) trainerPrompt.text = "Adjust position...";
                    }
                    else if (!upwardOK)
                    {
                        if (trainerPrompt) trainerPrompt.text = "Lift upward steadily (no wrist flicks).";
                    }
                }
            }
        }

        // --- RESET ---
        if (ResetPressedThisFrame())
        {
            repCount = 0;
            holdTimer = 0f;
            isInCorrectPose = false;
            waitingForReset = false;
            isCalibrated = false;
            repArmed = false;

            // re-arm calibration (require release again)
            sceneStartTime = Time.unscaledTime;
            calibrationArmed = false;
            triggerWasDown = IsTriggerDown();

            if (repCounterText) repCounterText.text = "Reps: 0";
            if (trainerPrompt)   trainerPrompt.text = "Reset complete. Calibrate again.";
            Debug.Log("[SimHand] Reset pressed.");
        }
    }

    // --- Helpers ---

    private bool HandleCalibrationGate()
    {
        // wait a short grace period to avoid scene-load presses
        if (!calibrationArmed)
        {
            if (Time.unscaledTime - sceneStartTime < inputBlockSeconds)
                return false;

            // require a full release before we accept a calibration press
            if (!IsTriggerDown())
            {
                calibrationArmed = true;
                Debug.Log("[SimHand] Calibration armed (trigger released).");
            }
            return false;
        }
        return true;
    }

    private bool IsTriggerDown()
    {
        return triggerAction != null && triggerAction.ReadValue<float>() >= triggerPressThreshold;
    }
    private bool TriggerPressedThisFrame()
    {
        bool now = IsTriggerDown();
        bool pressed = !triggerWasDown && now;
        triggerWasDown = now;
        return pressed;
    }

    private bool IsResetDown()
    {
        return resetAction != null && resetAction.ReadValue<float>() >= resetPressThreshold;
    }
    private bool ResetPressedThisFrame()
    {
        bool now = IsResetDown();
        bool pressed = !resetWasDown && now;
        resetWasDown = now;
        return pressed;
    }
}
