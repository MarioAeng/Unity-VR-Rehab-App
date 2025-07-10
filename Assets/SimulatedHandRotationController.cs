using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SimulatedHandRotationController : MonoBehaviour
{
    [Header("Setup")]
    public string inputAssetName = "InputSystem_Actions";
    public TMP_Text trainerPrompt;
    public TMP_Text repCounterText;

    [Header("Rep Settings")]
    public float rotationThreshold = 15f;
    public float requiredHoldTime = 1.5f;

    [Header("Input Action Names")]
    public string gripActionName = "GripAction";
    public string triggerActionName = "TriggerAction";
    public string resetActionName = "ResetAction";
    public string positionActionName = "RightHandPosition";
    public string rotationActionName = "RightHandRotation";

    private InputActionAsset inputActionAsset;
    private InputAction gripAction;
    private InputAction triggerAction;
    private InputAction resetAction;
    private InputAction positionAction;
    private InputAction rotationAction;

    private int repCount = 0;
    private float holdTimer = 0f;
    private bool isInCorrectPose = false;
    private bool waitingForReset = false;

    private Vector3 offset = new Vector3(0f, -0.4f, 0.3f);
    private Quaternion targetRotation;
    private bool isCalibrated = false;
    private bool wasTriggerPressed = false;

    private void OnEnable()
    {
        inputActionAsset = Resources.Load<InputActionAsset>(inputAssetName);

        if (inputActionAsset == null)
        {
            Debug.LogError("[SimHand] InputActionAsset not found.");
            return;
        }

        inputActionAsset.Enable();

        gripAction = inputActionAsset.FindAction(gripActionName);
        triggerAction = inputActionAsset.FindAction(triggerActionName);
        resetAction = inputActionAsset.FindAction(resetActionName);
        positionAction = inputActionAsset.FindAction(positionActionName);
        rotationAction = inputActionAsset.FindAction(rotationActionName);

        gripAction?.Enable();
        triggerAction?.Enable();
        resetAction?.Enable();
        positionAction?.Enable();
        rotationAction?.Enable();

        trainerPrompt.text = "Rotate your arm to max position, then press trigger to calibrate.";
        repCounterText.text = "Reps: 0";
    }

    private void OnDisable()
    {
        inputActionAsset?.Disable();
    }

    private void Update()
    {
        if (positionAction == null || rotationAction == null || triggerAction == null || resetAction == null)
            return;

        Vector3 rawPosition = positionAction.ReadValue<Vector3>();
        Quaternion currentRotation = rotationAction.ReadValue<Quaternion>();
        Vector3 adjustedPosition = rawPosition + offset;
        transform.SetPositionAndRotation(adjustedPosition, currentRotation);

        float triggerValue = triggerAction.ReadValue<float>();
        bool triggerPressed = triggerValue > 0.5f;

        if (!isCalibrated)
        {
            if (triggerPressed && !wasTriggerPressed)
            {
                targetRotation = currentRotation;
                isCalibrated = true;
                trainerPrompt.text = "Calibration complete! Rotate to begin.";
                Debug.Log("[SimHand] ✅ Rotation calibrated.");
            }
            else
            {
                trainerPrompt.text = "Rotate arm to target position, then press trigger.";
            }

            wasTriggerPressed = triggerPressed;
            return;
        }

        float angleDifference = Quaternion.Angle(currentRotation, targetRotation);

        if (waitingForReset)
        {
            if (angleDifference > rotationThreshold * 2f)
            {
                waitingForReset = false;
                trainerPrompt.text = "Rotate to begin again.";
            }
            else
            {
                trainerPrompt.text = "Return to rest position.";
            }
            return;
        }

        if (angleDifference <= rotationThreshold)
        {
            holdTimer += Time.deltaTime;
            trainerPrompt.text = "Hold that rotation!";

            if (!isInCorrectPose && holdTimer >= requiredHoldTime)
            {
                isInCorrectPose = true;
                waitingForReset = true;
                repCount++;
                repCounterText.text = $"Reps: {repCount}";
                Debug.Log($"[SimHand] ✅ Rotation rep completed: {repCount}");
            }
        }
        else
        {
            holdTimer = 0f;
            isInCorrectPose = false;
            trainerPrompt.text = "Rotate your arm!";
        }

        // Reset logic
        if (resetAction.triggered)
        {
            repCount = 0;
            holdTimer = 0f;
            isInCorrectPose = false;
            waitingForReset = false;
            isCalibrated = false;
            repCounterText.text = "Reps: 0";
            trainerPrompt.text = "Reset. Rotate and press trigger to recalibrate.";
            Debug.Log("[SimHand] 🔄 Reset complete.");
        }

        wasTriggerPressed = triggerPressed;
    }
}
