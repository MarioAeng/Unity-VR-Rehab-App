using UnityEngine;
using UnityEngine.InputSystem;

public class UnifiedRayInitializer : MonoBehaviour
{
    [Header("Input References")]
    public InputActionAsset inputAsset;

    [Header("Tracked References")]
    public InputActionProperty positionActionProperty;
    public InputActionProperty rotationActionProperty;
    public InputActionProperty triggerActionProperty;

    [Header("Action Names")]
    public string rightHandPosition = "RightHandPosition";
    public string rightHandRotation = "RightHandRotation";
    public string rightTrigger = "TriggerAction";
    public string leftHandPosition = "LeftHandPosition";
    public string leftHandRotation = "LeftHandRotation";
    public string leftTrigger = "LeftTriggerAction";

    [Header("Debug Options")]
    public Material rayLineMaterial;

    void Start()
    {
        if (inputAsset == null)
        {
            Debug.LogError("[UnifiedRayInitializer] ❌ InputAsset is not assigned.");
            return;
        }

        bool isLeft = PlayerSettings.IsLeftHanded;

        string posAction = isLeft ? leftHandPosition : rightHandPosition;
        string rotAction = isLeft ? leftHandRotation : rightHandRotation;
        string trigAction = isLeft ? leftTrigger : rightTrigger;

        var pos = inputAsset.FindAction(posAction);
        var rot = inputAsset.FindAction(rotAction);
        var trig = inputAsset.FindAction(trigAction);

        if (pos != null)
        {
            pos.Enable();
            positionActionProperty = new InputActionProperty(pos);
            Debug.Log($"[UnifiedRayInitializer] ✅ Bound position: {posAction}");
        }
        else
        {
            Debug.LogError($"[UnifiedRayInitializer] ❌ Could not find position action: {posAction}");
        }

        if (rot != null)
        {
            rot.Enable();
            rotationActionProperty = new InputActionProperty(rot);
            Debug.Log($"[UnifiedRayInitializer] ✅ Bound rotation: {rotAction}");
        }
        else
        {
            Debug.LogError($"[UnifiedRayInitializer] ❌ Could not find rotation action: {rotAction}");
        }

        if (trig != null)
        {
            trig.Enable();
            triggerActionProperty = new InputActionProperty(trig);
            Debug.Log($"[UnifiedRayInitializer] ✅ Bound trigger: {trigAction}");
        }
        else
        {
            Debug.LogError($"[UnifiedRayInitializer] ❌ Could not find trigger action: {trigAction}");
        }

        Debug.Log($"[UnifiedRayInitializer] 🎮 Finished assigning inputs for {(isLeft ? "Left" : "Right")} hand.");
    }
}
