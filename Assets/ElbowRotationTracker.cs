using UnityEngine;
using TMPro;

public class ElbowRotationTracker : MonoBehaviour
{
    [Header("Setup")]
    public Transform handTransform;
    public TMP_Text trainerPrompt;
    public TMP_Text repCounterText;

    [Header("Rep Settings")]
    public float leftThreshold = -30f;  // degrees relative to forward
    public float rightThreshold = 30f;
    public float holdTime = 1.0f;

    private int reps = 0;
    private string currentDirection = "Center";
    private float directionHoldTimer = 0f;
    private bool reachedLeft = false;
    private bool reachedRight = false;

    void Update()
    {
        if (handTransform == null) return;

        // Get angle relative to world forward in local XZ plane
        Vector3 flatForward = new Vector3(handTransform.forward.x, 0, handTransform.forward.z).normalized;
        float angle = Vector3.SignedAngle(Vector3.forward, flatForward, Vector3.up);

        Debug.Log($"[ElbowRotation] Flat Forward: {flatForward}, Angle: {angle}");

        // Determine direction
        string detectedDirection = "Center";
        if (angle <= leftThreshold) detectedDirection = "Left";
        else if (angle >= rightThreshold) detectedDirection = "Right";

        if (detectedDirection == currentDirection)
        {
            directionHoldTimer += Time.deltaTime;
            if (directionHoldTimer >= holdTime)
            {
                if (detectedDirection == "Left")
                {
                    trainerPrompt.text = "Swing right!";
                    reachedLeft = true;
                }
                else if (detectedDirection == "Right")
                {
                    trainerPrompt.text = "Swing left!";
                    reachedRight = true;
                }
            }
        }
        else
        {
            currentDirection = detectedDirection;
            directionHoldTimer = 0f;
        }

        // Rep completion
        if (reachedLeft && reachedRight)
        {
            reps++;
            repCounterText.text = $"Reps: {reps}";
            trainerPrompt.text = "Good! Go again!";
            Debug.Log($"[ElbowRotation] Completed rep #{reps}");

            reachedLeft = false;
            reachedRight = false;
            currentDirection = "Center";
            directionHoldTimer = 0f;
        }
    }
}
