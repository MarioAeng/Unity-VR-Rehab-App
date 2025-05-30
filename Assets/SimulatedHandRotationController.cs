using UnityEngine;
using TMPro;

public class SimulatedHandRotationController : MonoBehaviour
{
    [Header("Setup")]
    public Transform handTransform;
    public TMP_Text trainerPrompt;
    public TMP_Text repCounterText;

    [Header("Rep Settings")]
    public float rotationThreshold = 45f; // degrees
    public float holdTime = 1.5f;

    private int reps = 0;
    private bool inRotatedPosition = false;
    private float holdTimer = 0f;

    void Update()
    {
        if (handTransform == null) return;

        float currentAngle = handTransform.localEulerAngles.z;

        // Normalize angle (handle 0–360 wraparound)
        if (currentAngle > 180f)
            currentAngle -= 360f;

        if (Mathf.Abs(currentAngle) >= rotationThreshold)
        {
            if (!inRotatedPosition)
            {
                inRotatedPosition = true;
                holdTimer = 0f;
                trainerPrompt.text = "Hold that rotated pose!";
                Debug.Log("[ArmRotation] Rotation exceeded threshold");
            }

            holdTimer += Time.deltaTime;

            if (holdTimer >= holdTime)
            {
                reps++;
                repCounterText.text = $"Reps: {reps}";
                trainerPrompt.text = "Good job! Reset your arm.";
                Debug.Log($"[ArmRotation] Rep #{reps} completed");
                inRotatedPosition = false; // wait for reset
            }
        }
        else
        {
            if (inRotatedPosition)
            {
                trainerPrompt.text = "Rotate your forearm!";
                Debug.Log("[ArmRotation] Rotation fell below threshold - resetting");
            }

            inRotatedPosition = false;
            holdTimer = 0f;
        }
    }
}