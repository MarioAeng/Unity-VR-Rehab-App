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
    private bool isCalibrated = false;

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

        gripAction = map.FindAction(gripActionName);
        triggerAction = map.FindAction(triggerActionName);
        resetAction = map.FindAction(resetActionName);
        positionAction = map.FindAction(positionActionName);
        rotationAction = map.FindAction(rotationActionName);

        gripAction?.Enable();
        triggerAction?.Enable();
        resetAction?.Enable();
        positionAction?.Enable();
        rotationAction?.Enable();

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

    private void Update()
    {
        if (positionAction == null || rotationAction == null)
        {
            Debug.LogWarning("[SimHand] 🚫 Position or rotation action missing.");
            return;
        }

        Vector3 rawPosition = positionAction.ReadValue<Vector3>();
        Quaternion rotation = rotationAction.ReadValue<Quaternion>();
        Vector3 adjustedPosition = rawPosition + offset;
        transform.SetPositionAndRotation(adjustedPosition, rotation);

        if (!isCalibrated)
        {
            trainerPrompt.text = "Raise your hand to the desired height and press trigger to calibrate.";

            if (triggerAction != null && triggerAction.triggered)
            {
                Vector3 handPos = transform.position;
                targetY = handPos.y;
                isCalibrated = true;
                trainerPrompt.text = "✅ Calibration complete. Start raising your hand.";
                Debug.Log($"[SimHand] 🎯 Calibrated height set to Y = {targetY:F2}");
            }
            return;
        }

        Vector3 currentPos = transform.position;
        float deltaX = currentPos.x - targetX;
        float deltaY = currentPos.y - targetY;
        float distance = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);

        if (waitingForReset)
        {
            if (deltaY < -positionThreshold)
            {
                waitingForReset = false;
                trainerPrompt.text = "Raise your hand to begin again.";
            }
            else
            {
                trainerPrompt.text = "Return to rest position.";
            }
            return;
        }

        if (distance <= positionThreshold)
        {
            holdTimer += Time.deltaTime;
            trainerPrompt.text = "Hold that position!";

            if (!isInCorrectPose && holdTimer >= requiredHoldTime)
            {
                isInCorrectPose = true;
                waitingForReset = true;
                repCount++;
                repCounterText.text = $"Reps: {repCount}";
                Debug.Log($"[SimHand] ✅ Rep #{repCount} complete.");
            }
        }
        else
        {
            holdTimer = 0f;
            isInCorrectPose = false;

            if (Mathf.Abs(deltaY) > positionThreshold)
                trainerPrompt.text = deltaY > 0 ? "Lower your hand!" : "Raise your hand higher!";
            else if (Mathf.Abs(deltaX) > positionThreshold)
                trainerPrompt.text = deltaX > 0 ? "Move left!" : "Move right!";
            else
                trainerPrompt.text = "Adjust position...";
        }

        if (resetAction != null && resetAction.triggered)
        {
            repCount = 0;
            holdTimer = 0f;
            isInCorrectPose = false;
            waitingForReset = false;
            isCalibrated = false;
            repCounterText.text = $"Reps: {repCount}";
            trainerPrompt.text = "🔁 Reset complete. Calibrate again.";
            Debug.Log("[SimHand] 🔁 Reset pressed.");
        }
    }
}
