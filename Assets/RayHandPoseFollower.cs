using UnityEngine;
using UnityEngine.InputSystem;

public class RayHandPoseFollower : MonoBehaviour
{
    [Header("Input References")]
    public InputActionProperty leftHandPositionAction;
    public InputActionProperty leftHandRotationAction;
    public InputActionProperty rightHandPositionAction;
    public InputActionProperty rightHandRotationAction;

    [Header("Hand Transforms")]
    public Transform leftHandTransform;
    public Transform rightHandTransform;

    void Start()
    {
        // Log action enabled status
        Debug.Log($"[Start] LeftHandPositionAction enabled? {leftHandPositionAction.action.enabled}");
        Debug.Log($"[Start] LeftHandRotationAction enabled? {leftHandRotationAction.action.enabled}");
        Debug.Log($"[Start] RightHandPositionAction enabled? {rightHandPositionAction.action.enabled}");
        Debug.Log($"[Start] RightHandRotationAction enabled? {rightHandRotationAction.action.enabled}");

        // Manually enable if not already
        if (!leftHandPositionAction.action.enabled)
        {
            leftHandPositionAction.action.Enable();
            Debug.Log("[Start] LeftHandPositionAction manually enabled.");
        }

        if (!leftHandRotationAction.action.enabled)
        {
            leftHandRotationAction.action.Enable();
            Debug.Log("[Start] LeftHandRotationAction manually enabled.");
        }

        if (!rightHandPositionAction.action.enabled)
        {
            rightHandPositionAction.action.Enable();
            Debug.Log("[Start] RightHandPositionAction manually enabled.");
        }

        if (!rightHandRotationAction.action.enabled)
        {
            rightHandRotationAction.action.Enable();
            Debug.Log("[Start] RightHandRotationAction manually enabled.");
        }
    }

    void Update()
    {
        if (leftHandTransform != null && leftHandPositionAction != null && leftHandRotationAction != null)
        {
            Vector3 leftPos = leftHandPositionAction.action.ReadValue<Vector3>();
            Quaternion leftRot = leftHandRotationAction.action.ReadValue<Quaternion>();

            Debug.Log($"[Update] Left Hand Position: {leftPos}, Rotation: {leftRot}");

            leftHandTransform.localPosition = leftPos;
            leftHandTransform.localRotation = leftRot;
        }
        else
        {
            Debug.LogWarning("[Update] Left hand input or transform not assigned.");
        }

        if (rightHandTransform != null && rightHandPositionAction != null && rightHandRotationAction != null)
        {
            Vector3 rightPos = rightHandPositionAction.action.ReadValue<Vector3>();
            Quaternion rightRot = rightHandRotationAction.action.ReadValue<Quaternion>();

            Debug.Log($"[Update] Right Hand Position: {rightPos}, Rotation: {rightRot}");

            rightHandTransform.localPosition = rightPos;
            rightHandTransform.localRotation = rightRot;
        }
        else
        {
            Debug.LogWarning("[Update] Right hand input or transform not assigned.");
        }
    }
}
