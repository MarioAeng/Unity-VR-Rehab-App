using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ReturnToPhaseSelection : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionProperty leftBackAction;
    public InputActionProperty rightBackAction;

    [Header("Scene Name")]
    public string phaseSelectionSceneName = "PhaseSelectionScene";

    void OnEnable()
    {
        if (leftBackAction.action != null)
        {
            leftBackAction.action.Enable();
            leftBackAction.action.performed += OnBackPressed;
        }

        if (rightBackAction.action != null)
        {
            rightBackAction.action.Enable();
            rightBackAction.action.performed += OnBackPressed;
        }

        if (leftBackAction.action == null && rightBackAction.action == null)
        {
            Debug.LogWarning("[ReturnToPhaseSelection] ❌ Both BackActions are unassigned.");
        }
    }

    void OnDisable()
    {
        if (leftBackAction.action != null)
            leftBackAction.action.performed -= OnBackPressed;

        if (rightBackAction.action != null)
            rightBackAction.action.performed -= OnBackPressed;
    }

    void Update()
    {
        if (leftBackAction.action != null)
        {
            float val = leftBackAction.action.ReadValue<float>();
            if (leftBackAction.action.WasPressedThisFrame())
            {
                Debug.Log("[ReturnToPhaseSelection] 🟢 Left B pressed");
                LoadPhaseMenu();
            }
        }

        if (rightBackAction.action != null)
        {
            float val = rightBackAction.action.ReadValue<float>();
            if (rightBackAction.action.WasPressedThisFrame())
            {
                Debug.Log("[ReturnToPhaseSelection] 🟢 Right B pressed");
                LoadPhaseMenu();
            }
        }
    }

    void OnBackPressed(InputAction.CallbackContext context)
    {
        Debug.Log("[ReturnToPhaseSelection] 🔁 B Button Callback");
        LoadPhaseMenu();
    }

    void LoadPhaseMenu()
    {
        Debug.Log("[ReturnToPhaseSelection] 📂 Loading Phase Selection Scene...");
        SceneManager.LoadScene(phaseSelectionSceneName);
    }
}
