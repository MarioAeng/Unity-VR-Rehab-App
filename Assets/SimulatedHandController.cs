using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SimulatedHandController : MonoBehaviour
{
    [Header("Setup")]
    public string inputAssetName = "InputSystem_Actions";
    public TMP_Text trainerPrompt;
    public TMP_Text repCounterText;

    [Header("Rep Settings")]
    public float positionThreshold = 0.2f;
    public float requiredHoldTime = 1.5f;

    [Header("Target Position (Arbitrary)")]
    public float targetX = 0f;
    public float targetY = 1.3f;

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

    private Vector3 offset = new Vector3(0f, -0.4f, 0.3f); // Lower & push forward

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
    }

    private void OnDisable()
    {
        inputActionAsset?.Disable();
    }

    private void Update()
    {
        // Update position and rotation
        if (positionAction != null && rotationAction != null)
        {
            Vector3 rawPosition = positionAction.ReadValue<Vector3>();
            Quaternion rotation = rotationAction.ReadValue<Quaternion>();
            Vector3 adjustedPosition = rawPosition + offset;

            transform.SetPositionAndRotation(adjustedPosition, rotation);
        }

        // Grip display for debug
        if (gripAction != null)
        {
            float grip = gripAction.ReadValue<float>();
            Debug.Log($"[SimHand] Grip: {grip:F2}");
        }

        Vector3 handPos = transform.position;
        float deltaX = handPos.x - targetX;
        float deltaY = handPos.y - targetY;
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
                Debug.Log($"[SimHand] Rep completed: {repCount}");
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
            repCounterText.text = $"Reps: {repCount}";
            trainerPrompt.text = "Reset complete.";
            Debug.Log("[SimHand] Reps reset.");
        }
    }
}
