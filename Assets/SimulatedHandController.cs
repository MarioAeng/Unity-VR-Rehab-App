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

    [Header("Input Thresholds")]
    [Tooltip("Axis >= this counts as pressed")]
    public float triggerPressThreshold = 0.5f;
    public float resetPressThreshold = 0.5f;

    [Header("Calibration Gate")]
    [Tooltip("Ignore input for a brief moment after scene load")]
    public float inputBlockSeconds = 0.35f;

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

    private void OnEnable()
    {
        if (inputActionAsset == null)
        {
            Debug.LogError("[SimHand] ❌ InputActionAsset not assigned in Inspector.");
            return;
        }

        var map = inputActionAsset.FindActionMap("SimulatedHandMap");
        if (map == null)
        {
            Debug.LogError("[SimHand] ❌ Could not find action map 'SimulatedHandMap'.");
            return;
        }

        gripAction    = map.FindAction(gripActionName);
        triggerAction = map.FindAction(triggerActionName);
        resetAction   = map.FindAction(resetActionName);
        positionAction = map.FindAction(positionActionName);
        rotationAction = map.FindAction(rotationActionName);

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

        if (repCounterText) repCounterText.text = "Reps: 0";
        if (trainerPrompt)  trainerPrompt.text = "Raise your hand to the desired height and press trigger to calibrate.";

        Debug.Log($"[SimHand] ✅ Using PositionAction: {positionAction?.name}, RotationAction: {rotationAction?.name}");
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
            Debug.LogWarning("[SimHand] 🚫 Position or rotation action missing.");
            return;
        }

        // Drive the simulated hand from input
        Vector3 rawPosition = positionAction.ReadValue<Vector3>();
        Quaternion rotation = rotationAction.ReadValue<Quaternion>();
        Vector3 adjustedPosition = rawPosition + offset;
        transform.SetPositionAndRotation(adjustedPosition, rotation);

        // --- CALIBRATION PHASE ---
        if (!HandleCalibrationGate()) return;  // returns false until armed; keeps prompt visible
        if (!isCalibrated)
        {
            if (trainerPrompt) trainerPrompt.text = "Raise your hand to the desired height and press trigger to calibrate.";
            if (TriggerPressedThisFrame())
            {
                targetY = transform.position.y;
                isCalibrated = true;
                holdTimer = 0f;
                waitingForReset = false;
                if (trainerPrompt) trainerPrompt.text = "✅ Calibration complete. Start raising your hand.";
                Debug.Log($"[SimHand] 🎯 Calibrated height set to Y = {targetY:F2}");
            }
            return; // wait here until calibrated
        }

        // --- REP LOGIC ---
        Vector3 currentPos = transform.position;
        float deltaX = currentPos.x - targetX;
        float deltaY = currentPos.y - targetY;
        float distance = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);

        if (waitingForReset)
        {
            if (deltaY < -positionThreshold)
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
            if (distance <= positionThreshold)
            {
                holdTimer += Time.deltaTime;
                if (trainerPrompt) trainerPrompt.text = "Hold that position!";

                if (!isInCorrectPose && holdTimer >= requiredHoldTime)
                {
                    isInCorrectPose = true;
                    waitingForReset = true;
                    repCount++;
                    if (repCounterText) repCounterText.text = $"Reps: {repCount}";
                    Debug.Log($"[SimHand] ✅ Rep #{repCount} complete.");
                }
            }
            else
            {
                holdTimer = 0f;
                isInCorrectPose = false;

                if (Mathf.Abs(deltaY) > positionThreshold)
                    if (trainerPrompt) trainerPrompt.text = deltaY > 0 ? "Lower your hand!" : "Raise your hand higher!";
                else if (Mathf.Abs(deltaX) > positionThreshold)
                    if (trainerPrompt) trainerPrompt.text = deltaX > 0 ? "Move left!" : "Move right!";
                else
                    if (trainerPrompt) trainerPrompt.text = "Adjust position...";
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

            // re-arm calibration (require release again)
            sceneStartTime = Time.unscaledTime;
            calibrationArmed = false;
            triggerWasDown = IsTriggerDown();

            if (repCounterText) repCounterText.text = "Reps: 0";
            if (trainerPrompt)   trainerPrompt.text = "🔁 Reset complete. Calibrate again.";
            Debug.Log("[SimHand] 🔁 Reset pressed.");
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
                Debug.Log("[SimHand] 🟢 Calibration armed (trigger released).");
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
