using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ElbowRotationTracker : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text trainerPrompt;
    public TMP_Text repCounterText;

    [Header("Rep Settings")]
    public float targetLeftX = -0.25f;
    public float targetRightX = 0.25f;
    public float positionThreshold = 0.05f;
    public float requiredHoldTime = 1.2f;

    [Header("Input")]
    public string inputAssetName = "InputSystem_Actions";
    public string positionActionName = "RightHandPosition";
    public string rotationActionName = "RightHandRotation";
    public string resetActionName = "ResetAction";

    private InputActionAsset inputActionAsset;
    private InputAction positionAction;
    private InputAction rotationAction;
    private InputAction resetAction;

    private int repCount = 0;
    private bool isHolding = false;
    private float holdTimer = 0f;
    private enum Side { Left, Right }
    private Side lastSide = Side.Right;  // Start by moving to the left

    private Vector3 offset = new Vector3(0f, -0.4f, 0.3f); // Hand offset for better alignment

    void OnEnable()
    {
        inputActionAsset = Resources.Load<InputActionAsset>(inputAssetName);
        if (inputActionAsset == null)
        {
            Debug.LogError("[ElbowRotation] InputActionAsset not found.");
            return;
        }

        positionAction = inputActionAsset.FindAction(positionActionName);
        rotationAction = inputActionAsset.FindAction(rotationActionName);
        resetAction = inputActionAsset.FindAction(resetActionName);

        inputActionAsset.Enable();
        positionAction?.Enable();
        rotationAction?.Enable();
        resetAction?.Enable();
    }

    void OnDisable()
    {
        inputActionAsset?.Disable();
    }

    void Update()
    {
        // Update hand transform
        if (positionAction != null && rotationAction != null)
        {
            Vector3 rawPosition = positionAction.ReadValue<Vector3>();
            Quaternion rotation = rotationAction.ReadValue<Quaternion>();
            transform.SetPositionAndRotation(rawPosition + offset, rotation);
        }

        Vector3 handPos = transform.position;
        float x = handPos.x;

        // Handle reset
        if (resetAction != null && resetAction.triggered)
        {
            repCount = 0;
            holdTimer = 0f;
            isHolding = false;
            lastSide = Side.Right;
            repCounterText.text = "Reps: 0";
            trainerPrompt.text = "Reset.";
            Debug.Log("[ElbowRotation] Reps reset.");
            return;
        }

        // Determine if we're aiming for left or right
        if (lastSide == Side.Right)
        {
            float distToLeft = Mathf.Abs(x - targetLeftX);
            if (distToLeft <= positionThreshold)
            {
                trainerPrompt.text = "Hold on Left!";
                holdTimer += Time.deltaTime;

                if (!isHolding && holdTimer >= requiredHoldTime)
                {
                    isHolding = true;
                    lastSide = Side.Left;
                    repCount++;
                    repCounterText.text = $"Reps: {repCount}";
                    Debug.Log($"[ElbowRotation] Rep {repCount} (Left)");
                }
            }
            else
            {
                trainerPrompt.text = "Move further left";
                holdTimer = 0f;
                isHolding = false;
            }
        }
        else  // lastSide == Side.Left
        {
            float distToRight = Mathf.Abs(x - targetRightX);
            if (distToRight <= positionThreshold)
            {
                trainerPrompt.text = "Hold on Right!";
                holdTimer += Time.deltaTime;

                if (!isHolding && holdTimer >= requiredHoldTime)
                {
                    isHolding = true;
                    lastSide = Side.Right;
                    repCount++;
                    repCounterText.text = $"Reps: {repCount}";
                    Debug.Log($"[ElbowRotation] Rep {repCount} (Right)");
                }
            }
            else
            {
                trainerPrompt.text = "Move further right";
                holdTimer = 0f;
                isHolding = false;
            }
        }
    }
}
