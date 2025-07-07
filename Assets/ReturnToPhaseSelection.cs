using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ReturnToPhaseSelection : MonoBehaviour
{
    [Header("Input Action")]
    public InputActionProperty backAction;

    [Header("Scene Name")]
    public string phaseSelectionSceneName = "PhaseSelectionScene"; // Default target

    void OnEnable()
    {
        if (backAction.action != null)
        {
            backAction.action.Enable();
            backAction.action.performed += OnBackPressed;
        }
        else
        {
            Debug.LogWarning("[ReturnToPhaseSelection] BackAction is not assigned.");
        }
    }

    void OnDisable()
    {
        if (backAction.action != null)
        {
            backAction.action.performed -= OnBackPressed;
        }
    }

    void Update()
    {
        if (backAction.action != null)
        {
            float val = backAction.action.ReadValue<float>();
            Debug.Log($"[BackAction Raw Value] {val}");

            if (backAction.action.WasPressedThisFrame())
            {
                Debug.Log("[ReturnToPhaseSelection] B Press Detected (WasPressedThisFrame)");
                LoadPhaseMenu();
            }
        }
    }

    void OnBackPressed(InputAction.CallbackContext context)
    {
        Debug.Log("[ReturnToPhaseSelection] B Press Detected (performed callback)");
        LoadPhaseMenu();
    }

    void LoadPhaseMenu()
    {
        Debug.Log("[ReturnToPhaseSelection] Loading Phase Selection Scene...");
        SceneManager.LoadScene(phaseSelectionSceneName);
    }
}