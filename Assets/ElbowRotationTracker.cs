using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ElbowRotationTracker : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text trainerPrompt;
    public TMP_Text repCounterText;

    [Header("Rep Settings")]
    public float positionThreshold = 0.05f;
    public float requiredHoldTime = 1.2f;

    [Header("Input")]
    public string inputAssetName = "InputSystem_Actions";
    public string rightPositionActionName = "RightHandPosition";
    public string rightRotationActionName = "RightHandRotation";
    public string leftPositionActionName = "LeftHandPosition";
    public string leftRotationActionName = "LeftHandRotation";
    public string triggerActionName = "TriggerAction";
    public string resetActionName = "ResetAction";

    private InputActionAsset inputActionAsset;
    private InputAction positionAction;
    private InputAction rotationAction;
    private InputAction triggerAction;
    private InputAction resetAction;

    private int repCount = 0;
    private float holdTimer = 0f;
    private bool isHolding = false;
    private bool wasTriggerPressed = false;

    private enum Side { Left, Right }
    private Side lastSide = Side.Right;

    private float calibratedLeftX;
    private float calibratedRightX;
    private bool rightCalibrated = false;
    private bool leftCalibrated = false;

    private Vector3 offset = new Vector3(0f, -0.4f, 0.3f);

    void OnEnable()
    {
        inputActionAsset = Resources.Load<InputActionAsset>(inputAssetName);
        if (inputActionAsset == null)
        {
            Debug.LogError("[ElbowRotation] InputActionAsset not found.");
            return;
        }

        // Determine which hand is selected
        bool isLeft = PlayerSettings.IsLeftHanded;
        string posName = isLeft ? leftPositionActionName : rightPositionActionName;
        string rotName = isLeft ? leftRotationActionName : rightRotationActionName;

        Debug.Log($"[ElbowRotation] Handedness: {(isLeft ? "Left" : "Right")} | Position: {posName} | Rotation: {rotName}");

        positionAction = inputActionAsset.FindAction(posName);
        rotationAction = inputActionAsset.FindAction(rotName);
        triggerAction = inputActionAsset.FindAction(triggerActionName);
        resetAction = inputActionAsset.FindAction(resetActionName);

        inputActionAsset.Enable();
        positionAction?.Enable();
        rotationAction?.Enable();
        triggerAction?.Enable();
        resetAction?.Enable();

        trainerPrompt.text = "Move hand FAR RIGHT and press trigger to calibrate.";
        repCounterText.text = "Reps: 0";
    }

    void OnDisable()
    {
        inputActionAsset?.Disable();
    }

    void Update()
    {
        if (positionAction == null || triggerAction == null || rotationAction == null)
            return;

        // Read hand position and apply offset
        Vector3 rawPos = positionAction.ReadValue<Vector3>();
        Quaternion handRot = rotationAction.ReadValue<Quaternion>();
        Vector3 handPos = rawPos + offset;
        transform.SetPositionAndRotation(handPos, handRot);

        float x = handPos.x;
        bool triggerPressed = triggerAction.ReadValue<float>() > 0.5f;

        // Reset logic
        if (resetAction != null && resetAction.triggered)
        {
            repCount = 0;
            holdTimer = 0f;
            isHolding = false;
            lastSide = Side.Right;
            leftCalibrated = false;
            rightCalibrated = false;
            repCounterText.text = "Reps: 0";
            trainerPrompt.text = "Reset. Move hand FAR RIGHT and press trigger.";
            Debug.Log("[ElbowRotation] 🔁 Reset.");
            return;
        }

        // Calibration logic
        if (!rightCalibrated)
        {
            trainerPrompt.text = "Move hand FAR RIGHT and press trigger to calibrate.";
            if (triggerPressed && !wasTriggerPressed)
            {
                calibratedRightX = x;
                rightCalibrated = true;
                trainerPrompt.text = "Now move FAR LEFT and press trigger.";
                Debug.Log($"[ElbowRotation] ✅ Calibrated RIGHT X: {calibratedRightX}");
            }
            wasTriggerPressed = triggerPressed;
            return;
        }

        if (!leftCalibrated)
        {
            trainerPrompt.text = "Move hand FAR LEFT and press trigger to calibrate.";
            if (triggerPressed && !wasTriggerPressed)
            {
                calibratedLeftX = x;
                leftCalibrated = true;
                trainerPrompt.text = "Calibration complete! Begin rotating.";
                Debug.Log($"[ElbowRotation] ✅ Calibrated LEFT X: {calibratedLeftX}");
            }
            wasTriggerPressed = triggerPressed;
            return;
        }

        // Active rep logic (after calibration)
        if (lastSide == Side.Right)
        {
            float distToLeft = Mathf.Abs(x - calibratedLeftX);
            if (distToLeft <= positionThreshold)
            {
                trainerPrompt.text = "Hold on LEFT!";
                holdTimer += Time.deltaTime;

                if (!isHolding && holdTimer >= requiredHoldTime)
                {
                    isHolding = true;
                    lastSide = Side.Left;
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
        else // lastSide == Side.Left
        {
            float distToRight = Mathf.Abs(x - calibratedRightX);
            if (distToRight <= positionThreshold)
            {
                trainerPrompt.text = "Hold on RIGHT!";
                holdTimer += Time.deltaTime;

                if (!isHolding && holdTimer >= requiredHoldTime)
                {
                    isHolding = true;
                    lastSide = Side.Right;
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

        // Update trigger state for edge detection
        wasTriggerPressed = triggerPressed;
    }
}
