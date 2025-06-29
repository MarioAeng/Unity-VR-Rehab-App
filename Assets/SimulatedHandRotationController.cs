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
    public float rotationThreshold = 15f; // degrees
    public float requiredHoldTime = 1.5f;

    [Header("Target Rotation (Euler angles)")]
    public Vector3 targetEuler = new Vector3(0f, 0f, 90f); // Example: 90-degree z-rotation

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

        targetRotation = Quaternion.Euler(targetEuler);
    }

    private void OnDisable()
    {
        inputActionAsset?.Disable();
    }

    private void Update()
    {
        if (positionAction != null && rotationAction != null)
        {
            Vector3 rawPosition = positionAction.ReadValue<Vector3>();
            Quaternion rotation = rotationAction.ReadValue<Quaternion>();
            Vector3 adjustedPosition = rawPosition + offset;
            transform.SetPositionAndRotation(adjustedPosition, rotation);

            float angleDifference = Quaternion.Angle(rotation, targetRotation);

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
                    Debug.Log($"[SimHand] Rotation rep completed: {repCount}");
                }
            }
            else
            {
                holdTimer = 0f;
                isInCorrectPose = false;
                trainerPrompt.text = "Rotate your arm!";
            }
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
