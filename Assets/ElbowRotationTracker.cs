using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ElbowRotationTracker : MonoBehaviour
{
    [Header("Setup")]
    public TMP_Text trainerPrompt;
    public TMP_Text repCounterText;

    [Header("Rep Settings")]
    public float positionThreshold = 0.15f;
    public float requiredHoldTime = 1.0f;

    [Header("Target Positions (X axis)")]
    public float targetLeftX;
    public float targetRightX;

    [Header("Input")]
    public InputActionAsset inputActionAsset;
    public string positionActionName = "RightHandPosition";
    public string rotationActionName = "RightHandRotation";
    public string triggerActionName = "TriggerAction";
    public string resetActionName = "ResetAction";

    private InputAction positionAction;
    private InputAction rotationAction;
    private InputAction triggerAction;
    private InputAction resetAction;

    private int repCount = 0;
    private float holdTimer = 0f;
    private bool isHolding = false;
    private bool wasTriggerPressed = false;

    private enum Side { Right, Left }
    private Side currentSide = Side.Right;

    private Vector3 offset = new Vector3(0f, -0.4f, 0.3f);

    private enum CalibrationStage { None, RightSet, BothSet }
    private CalibrationStage calibrationStage = CalibrationStage.None;

    // ---------------- Anti-cheat (minimal) ----------------
    [Header("Anti-cheat")]
    public bool enableAntiCheat = true;

    [Tooltip("Allowed tilt change (degrees) vs per-side calibration (yaw-invariant).")]
    public float maxRotationDeltaDegrees = 30f;

    [Tooltip("Deadzone (degrees) so tiny jitter doesn't trip the rotation check.")]
    public float rotationDeadzoneDegrees = 8f;

    [Tooltip("Must leave the starting side by this much (X) before arming a rep.")]
    public float armStartWindowX = 0.12f;

    [Tooltip("Minimum horizontal travel (X) from the arm point to the opposite target.")]
    public float minHorizontalTravel = 0.25f;

    [Tooltip("Max allowed vertical drift (Y) from the arm point while moving.")]
    public float verticalDriftLimit = 0.15f;

    // runtime anti-cheat state
    private bool moveArmed = false;
    private float startXForRep = 0f;
    private float startYForRep = 0f;

    // per-side calibration rotations
    private Quaternion calibRotRight;
    private Quaternion calibRotLeft;

    // smoothed rotation delta
    private float rotDeltaEMA = 0f;
    private const float rotEmaAlpha = 0.25f;

    // first-rep grace so you can count right where you calibrated
    private bool justEnteredBothSet = false;
    private bool firstHoldGrace = false;
    // ------------------------------------------------------

    private void OnEnable()
    {
        var map = inputActionAsset.FindActionMap("SimulatedHandMap");

        positionAction = map.FindAction(positionActionName);
        rotationAction = map.FindAction(rotationActionName);
        triggerAction  = map.FindAction(triggerActionName);
        resetAction    = map.FindAction(resetActionName);

        positionAction?.Enable();
        rotationAction?.Enable();
        triggerAction?.Enable();
        resetAction?.Enable();
    }

    private void OnDisable()
    {
        positionAction?.Disable();
        rotationAction?.Disable();
        triggerAction?.Disable();
        resetAction?.Disable();
    }

    void Update()
    {
        if (positionAction == null || rotationAction == null)
        {
            Debug.LogWarning("[ElbowRotation] ❌ Missing Input Actions.");
            return;
        }

        Vector3 rawPos = positionAction.ReadValue<Vector3>();
        Quaternion rot = rotationAction.ReadValue<Quaternion>();
        Vector3 handPos = rawPos + offset;
        transform.SetPositionAndRotation(handPos, rot);

        float x = handPos.x;
        float y = handPos.y;
        bool triggerPressed = triggerAction.ReadValue<float>() > 0.5f;

        // Reset
        if (resetAction != null && resetAction.triggered)
        {
            calibrationStage = CalibrationStage.None;
            repCount = 0;
            currentSide = Side.Right;
            repCounterText.text = "Reps: 0";
            trainerPrompt.text = "🔁 Reset. Move hand to far RIGHT and press trigger.";
            // clear anti-cheat state
            moveArmed = false; firstHoldGrace = false; rotDeltaEMA = 0f;
            holdTimer = 0f; isHolding = false;
            return;
        }

        // Calibration logic
        if (calibrationStage == CalibrationStage.None)
        {
            trainerPrompt.text = "Move hand to far RIGHT and press trigger.";
            if (triggerPressed && !wasTriggerPressed)
            {
                targetRightX = x;
                calibRotRight = rot; // store RIGHT-side neutral
                calibrationStage = CalibrationStage.RightSet;
                trainerPrompt.text = "✅ Right side set. Now move hand to far LEFT and press trigger.";
                Debug.Log($"[ElbowRotation] ✅ RightX: {targetRightX}");
            }
            wasTriggerPressed = triggerPressed;
            return;
        }
        else if (calibrationStage == CalibrationStage.RightSet)
        {
            trainerPrompt.text = "Move hand to far LEFT and press trigger.";
            if (triggerPressed && !wasTriggerPressed)
            {
                targetLeftX = x;
                calibRotLeft = rot; // store LEFT-side neutral
                calibrationStage = CalibrationStage.BothSet;
                justEnteredBothSet = true;  // pick starting side & allow first-hold grace
                trainerPrompt.text = "✅ Calibration complete. Start moving LEFT and RIGHT.";
                Debug.Log($"[ElbowRotation] ✅ LeftX: {targetLeftX}");
            }
            wasTriggerPressed = triggerPressed;
            return;
        }

        // Rep logic (BothSet)
        if (calibrationStage == CalibrationStage.BothSet)
        {
            // choose initial side based on where you are right now; allow first-hold grace
            if (justEnteredBothSet)
            {
                float dL = Mathf.Abs(x - targetLeftX);
                float dR = Mathf.Abs(x - targetRightX);
                currentSide = (dL <= dR) ? Side.Left : Side.Right;
                moveArmed = false;
                holdTimer = 0f;
                isHolding = false;
                firstHoldGrace = true;
                justEnteredBothSet = false;
                rotDeltaEMA = 0f;
            }

            if (currentSide == Side.Right)
            {
                // moving to LEFT target
                float distToLeft = Mathf.Abs(x - targetLeftX);
                bool nearLeft = distToLeft <= positionThreshold;

                // First-rep grace: allow counting at target without travel gating (still require rotation OK)
                if (firstHoldGrace && nearLeft)
                {
                    if (RotationOK(calibRotLeft, rot))
                    {
                        trainerPrompt.text = "Hold on LEFT!";
                        holdTimer += Time.deltaTime;
                        if (!isHolding && holdTimer >= requiredHoldTime)
                        {
                            isHolding = true;
                            currentSide = Side.Left;
                            repCount++;
                            repCounterText.text = $"Reps: {repCount}";
                            firstHoldGrace = false;
                            moveArmed = false;
                            Debug.Log($"[ElbowRotation] ✅ Rep {repCount} (Left, grace)");
                        }
                    }
                    else
                    {
                        trainerPrompt.text = "Keep wrist neutral and hold.";
                        holdTimer = 0f; isHolding = false;
                    }
                    wasTriggerPressed = triggerPressed;
                    return;
                }

                // Arm a rep after clearly leaving the RIGHT side
                if (!moveArmed)
                {
                    if (x <= targetRightX - armStartWindowX)
                    {
                        moveArmed = true;
                        startXForRep = x;
                        startYForRep = y;
                        holdTimer = 0f; isHolding = false;
                        rotDeltaEMA = 0f;
                    }
                    trainerPrompt.text = "Move to LEFT target.";
                    wasTriggerPressed = triggerPressed;
                    return;
                }

                // Anti-cheat at LEFT
                bool rotationOK = RotationOK(calibRotLeft, rot);
                bool travelOK   = Mathf.Abs(x - startXForRep) >= minHorizontalTravel;
                bool driftYOK   = Mathf.Abs(y - startYForRep) <= verticalDriftLimit;

                if (nearLeft && rotationOK && travelOK && driftYOK)
                {
                    trainerPrompt.text = "Hold on LEFT!";
                    holdTimer += Time.deltaTime;
                    if (!isHolding && holdTimer >= requiredHoldTime)
                    {
                        isHolding = true;
                        currentSide = Side.Left;
                        repCount++;
                        repCounterText.text = $"Reps: {repCount}";
                        moveArmed = false;
                        Debug.Log($"[ElbowRotation] ✅ Rep {repCount} (Left)");
                    }
                }
                else
                {
                    trainerPrompt.text =
                        !nearLeft ? "Move further LEFT." :
                        !rotationOK ? "Keep wrist neutral and hold." :
                        !travelOK ? "Move further LEFT." :
                        "Keep elbow height steady.";
                    holdTimer = 0f; isHolding = false;
                }
            }
            else // currentSide == Side.Left (moving to RIGHT)
            {
                float distToRight = Mathf.Abs(x - targetRightX);
                bool nearRight = distToRight <= positionThreshold;

                if (firstHoldGrace && nearRight)
                {
                    if (RotationOK(calibRotRight, rot))
                    {
                        trainerPrompt.text = "Hold on RIGHT!";
                        holdTimer += Time.deltaTime;
                        if (!isHolding && holdTimer >= requiredHoldTime)
                        {
                            isHolding = true;
                            currentSide = Side.Right;
                            repCount++;
                            repCounterText.text = $"Reps: {repCount}";
                            firstHoldGrace = false;
                            moveArmed = false;
                            Debug.Log($"[ElbowRotation] ✅ Rep {repCount} (Right, grace)");
                        }
                    }
                    else
                    {
                        trainerPrompt.text = "Keep wrist neutral and hold.";
                        holdTimer = 0f; isHolding = false;
                    }
                    wasTriggerPressed = triggerPressed;
                    return;
                }

                if (!moveArmed)
                {
                    if (x >= targetLeftX + armStartWindowX)
                    {
                        moveArmed = true;
                        startXForRep = x;
                        startYForRep = y;
                        holdTimer = 0f; isHolding = false;
                        rotDeltaEMA = 0f;
                    }
                    trainerPrompt.text = "Move to RIGHT target.";
                    wasTriggerPressed = triggerPressed;
                    return;
                }

                bool rotationOK = RotationOK(calibRotRight, rot);
                bool travelOK   = Mathf.Abs(x - startXForRep) >= minHorizontalTravel;
                bool driftYOK   = Mathf.Abs(y - startYForRep) <= verticalDriftLimit;

                if (nearRight && rotationOK && travelOK && driftYOK)
                {
                    trainerPrompt.text = "Hold on RIGHT!";
                    holdTimer += Time.deltaTime;
                    if (!isHolding && holdTimer >= requiredHoldTime)
                    {
                        isHolding = true;
                        currentSide = Side.Right;
                        repCount++;
                        repCounterText.text = $"Reps: {repCount}";
                        moveArmed = false;
                        Debug.Log($"[ElbowRotation] ✅ Rep {repCount} (Right)");
                    }
                }
                else
                {
                    trainerPrompt.text =
                        !nearRight ? "Move further RIGHT." :
                        !rotationOK ? "Keep wrist neutral and hold." :
                        !travelOK ? "Move further RIGHT." :
                        "Keep elbow height steady.";
                    holdTimer = 0f; isHolding = false;
                }
            }

            wasTriggerPressed = triggerPressed;
        }
    }

    // ---- Yaw-invariant tilt check (per-side), with deadzone + smoothing ----
    private bool RotationOK(Quaternion calib, Quaternion current)
    {
        if (!enableAntiCheat) return true;

        // Compare arm 'up' tilt vs world up at calibration vs now (ignores yaw).
        float a0 = Vector3.Angle(calib * Vector3.up, Vector3.up);
        float a1 = Vector3.Angle(current * Vector3.up, Vector3.up);

        float delta = Mathf.Abs(a1 - a0);
        delta = Mathf.Max(0f, delta - Mathf.Max(0f, rotationDeadzoneDegrees)); // deadzone
        rotDeltaEMA = Mathf.Lerp(rotDeltaEMA, delta, rotEmaAlpha);             // smoothing

        return rotDeltaEMA <= maxRotationDeltaDegrees;
    }
}
