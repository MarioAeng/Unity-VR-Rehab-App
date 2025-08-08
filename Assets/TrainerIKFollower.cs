using UnityEngine;
using UnityEngine.InputSystem;

public class TrainerIKFollower : MonoBehaviour
{
    [Header("References")]
    public Animator animator; // Animator on Trainer
    public Transform controllerTransform; // Hand/controller position to follow

    [Header("Optional Input Actions (if no transform provided)")]
    public InputActionProperty handPositionAction;
    public InputActionProperty handRotationAction;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float ikWeight = 1f;

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null)
        {
            Debug.LogWarning("[TrainerIKFollower] ❌ Animator reference is missing.");
            return;
        }

        Vector3 handPos = Vector3.zero;
        Quaternion handRot = Quaternion.identity;

        if (controllerTransform != null)
        {
            handPos = controllerTransform.position;
            handRot = controllerTransform.rotation;
        }
        else
        {
            if (handPositionAction.action != null && handRotationAction.action != null)
            {
                handPos = handPositionAction.action.ReadValue<Vector3>();
                handRot = handRotationAction.action.ReadValue<Quaternion>();
            }
            else
            {
                Debug.LogWarning("[TrainerIKFollower] ❌ No controller transform or valid Input Actions assigned.");
                return;
            }
        }

        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);
        animator.SetIKPosition(AvatarIKGoal.RightHand, handPos);
        animator.SetIKRotation(AvatarIKGoal.RightHand, handRot);

        Debug.Log($"[TrainerIKFollower] ✅ Applied IK to right hand at {handPos}");
    }
}