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

    private void OnEnable()
    {
        var map = inputActionAsset.FindActionMap("SimulatedHandMap");

        positionAction = map.FindAction(positionActionName);
        rotationAction = map.FindAction(rotationActionName);
        triggerAction = map.FindAction(triggerActionName);
        resetAction = map.FindAction(resetActionName);

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
        bool triggerPressed = triggerAction.ReadValue<float>() > 0.5f;

        // Reset
        if (resetAction != null && resetAction.triggered)
        {
            calibrationStage = CalibrationStage.None;
            repCount = 0;
            currentSide = Side.Right;
            repCounterText.text = "Reps: 0";
            trainerPrompt.text = "🔁 Reset. Move hand to far RIGHT and press trigger.";
            return;
        }

        // Calibration logic
        if (calibrationStage == CalibrationStage.None)
        {
            trainerPrompt.text = "Move hand to far RIGHT and press trigger.";
            if (triggerPressed && !wasTriggerPressed)
            {
                targetRightX = x;
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
                calibrationStage = CalibrationStage.BothSet;
                trainerPrompt.text = "✅ Calibration complete. Start moving LEFT and RIGHT.";
                Debug.Log($"[ElbowRotation] ✅ LeftX: {targetLeftX}");
            }
            wasTriggerPressed = triggerPressed;
            return;
        }

        // Rep logic (BothSet)
        if (calibrationStage == CalibrationStage.BothSet)
        {
            if (currentSide == Side.Right)
            {
                float distToLeft = Mathf.Abs(x - targetLeftX);
                if (distToLeft <= positionThreshold)
                {
                    trainerPrompt.text = "Hold on LEFT!";
                    holdTimer += Time.deltaTime;

                    if (!isHolding && holdTimer >= requiredHoldTime)
                    {
                        isHolding = true;
                        currentSide = Side.Left;
                        repCount++;
                        repCounterText.text = $"Reps: {repCount}";
                        Debug.Log($"[ElbowRotation] ✅ Rep {repCount} (Left)");
                    }
                }
                else
                {
                    trainerPrompt.text = "Move further LEFT.";
                    holdTimer = 0f;
                    isHolding = false;
                }
            }
            else // currentSide == Side.Left
            {
                float distToRight = Mathf.Abs(x - targetRightX);
                if (distToRight <= positionThreshold)
                {
                    trainerPrompt.text = "Hold on RIGHT!";
                    holdTimer += Time.deltaTime;

                    if (!isHolding && holdTimer >= requiredHoldTime)
                    {
                        isHolding = true;
                        currentSide = Side.Right;
                        repCount++;
                        repCounterText.text = $"Reps: {repCount}";
                        Debug.Log($"[ElbowRotation] ✅ Rep {repCount} (Right)");
                    }
                }
                else
                {
                    trainerPrompt.text = "Move further RIGHT.";
                    holdTimer = 0f;
                    isHolding = false;
                }
            }

            wasTriggerPressed = triggerPressed;
        }
    }
}
