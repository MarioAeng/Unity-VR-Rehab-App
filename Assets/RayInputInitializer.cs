using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class RayInputInitializer : MonoBehaviour
{
    [Header("Input Action Asset")]
    public InputActionAsset inputAsset;

    [Header("Action Names")]
    public string rightPosition = "RightHandPosition";
    public string rightRotation = "RightHandRotation";
    public string rightTrigger = "TriggerAction";

    public string leftPosition = "LeftHandPosition";
    public string leftRotation = "LeftHandRotation";
    public string leftTrigger = "LeftTriggerAction";

    private ActionBasedController controller;
    private bool isLeft;

    void Awake()
    {
        controller = GetComponent<ActionBasedController>();
    }

    void Start()
    {
        if (controller == null)
        {
            Debug.LogError("[RayInputInitializer] ❌ Missing ActionBasedController component.");
            return;
        }

        if (inputAsset == null)
        {
            Debug.LogError("[RayInputInitializer] ❌ InputActionAsset is not assigned.");
            return;
        }

        isLeft = PlayerSettings.IsLeftHanded;
        Debug.Log($"[RayInputInitializer] 🧠 Player selected hand: {(isLeft ? "Left" : "Right")}");

        if (isLeft)
        {
            EnableAndLogAction(leftPosition);
            EnableAndLogAction(leftRotation);
            EnableAndLogAction(leftTrigger);
        }
        else
        {
            EnableAndLogAction(rightPosition);
            EnableAndLogAction(rightRotation);
            EnableAndLogAction(rightTrigger);
        }

        ApplyHandedness();
    }

    public void ApplyHandedness()
    {
        isLeft = PlayerSettings.IsLeftHanded;
        Debug.Log($"[RayInputInitializer] 🔁 Reapplying handedness at runtime: {(isLeft ? "Left" : "Right")}");

        AssignAction("Position", controller, inputAsset.FindAction(isLeft ? leftPosition : rightPosition), a => controller.positionAction = new InputActionProperty(a));
        AssignAction("Rotation", controller, inputAsset.FindAction(isLeft ? leftRotation : rightRotation), a => controller.rotationAction = new InputActionProperty(a));
        AssignAction("Trigger (Select)", controller, inputAsset.FindAction(isLeft ? leftTrigger : rightTrigger), a => controller.selectAction = new InputActionProperty(a));
        AssignAction("Trigger (UI Press)", controller, inputAsset.FindAction(isLeft ? leftTrigger : rightTrigger), a => controller.uiPressAction = new InputActionProperty(a));

        Debug.Log("[RayInputInitializer] ✅ Controller input mappings completed.");
    }

    private void EnableAndLogAction(string actionName)
    {
        var action = inputAsset.FindAction(actionName);
        if (action == null)
        {
            Debug.LogError($"[RayInputInitializer] ❌ Action not found: {actionName}. Check InputAction asset and spelling.");
        }
        else
        {
            action.Enable();
            Debug.Log($"[RayInputInitializer] ✅ Action enabled: {actionName}");
        }
    }

    private void AssignAction(string label, ActionBasedController target, InputAction action, System.Action<InputAction> assign)
    {
        if (action == null)
        {
            Debug.LogError($"[RayInputInitializer] ❌ Failed to assign {label} action. InputAction reference is null.");
            return;
        }

        assign.Invoke(action);
        Debug.Log($"[RayInputInitializer] 🎮 Assigned {label} action: {action.name}");
    }

    void Update()
    {
        if (controller.positionAction.action != null)
        {
            Vector3 pos = controller.positionAction.action.ReadValue<Vector3>();
            Debug.Log($"[RayInputInitializer] 📍 Position Value: {pos}");
        }

        if (controller.rotationAction.action != null)
        {
            Quaternion rot = controller.rotationAction.action.ReadValue<Quaternion>();
            Debug.Log($"[RayInputInitializer] 🔄 Rotation Value: {rot.eulerAngles}");
        }
    }

    void LateUpdate()
    {
        if (controller.positionAction.action == null)
            Debug.LogWarning("[RayInputInitializer] ⚠️ positionAction is still null!");

        if (controller.rotationAction.action == null)
            Debug.LogWarning("[RayInputInitializer] ⚠️ rotationAction is still null!");

        if (controller.selectAction.action == null)
            Debug.LogWarning("[RayInputInitializer] ⚠️ selectAction is still null!");
    }
}
