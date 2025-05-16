using UnityEngine;
using TMPro;
using UnityEngine.XR;

public class XRHandRepetitionTracker : MonoBehaviour
{
    public TMP_Text trainerPrompt;
    public TMP_Text repCounterText;
    public Transform targetPose;
    public float positionThreshold = 0.2f;
    public float maxHeightThreshold = 2.0f;
    public float requiredHoldTime = 1.5f;

    private int repCount = 0;
    private float holdTimer = 0f;
    private bool isInCorrectPose = false;

    void Start()
    {
        if (repCounterText != null)
            repCounterText.text = "Reps: 0";
    }

    void Update()
    {
        Vector3 handPosition = transform.position;

        float distance = Vector3.Distance(handPosition, targetPose.position);

        if (distance <= positionThreshold)
        {
            holdTimer += Time.deltaTime;
            trainerPrompt.text = "Hold that position!";

            if (!isInCorrectPose && holdTimer >= requiredHoldTime)
            {
                isInCorrectPose = true;
                repCount++;
                if (repCounterText != null)
                    repCounterText.text = "Reps: " + repCount;
            }
        }
        else
        {
            holdTimer = 0f;
            isInCorrectPose = false;

            if (handPosition.y > maxHeightThreshold)
                trainerPrompt.text = "Lower your hand!";
            else if (handPosition.y < targetPose.position.y - positionThreshold)
                trainerPrompt.text = "Raise your hand higher!";
            else
                trainerPrompt.text = "Adjust position...";
        }
    }
}
