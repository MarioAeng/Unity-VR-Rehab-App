using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ReturnToMainMenu : MonoBehaviour
{
    [Header("Input Action")]
    public InputActionProperty backAction;

    [Header("Scene Name")]
    public string mainMenuSceneName = "MainMenuScene";

    void OnEnable()
    {
        if (backAction.action != null)
        {
            backAction.action.Enable();
            backAction.action.performed += OnBackPressed;
        }
        else
        {
            Debug.LogWarning("[ReturnToMainMenu] BackAction is not assigned.");
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
                Debug.Log("[ReturnToMainMenu] B Press Detected (WasPressedThisFrame)");
                LoadMenu();
            }
        }
    }

    void OnBackPressed(InputAction.CallbackContext context)
    {
        Debug.Log("[ReturnToMainMenu] B Press Detected (performed callback)");
        LoadMenu();
    }

    void LoadMenu()
    {
        Debug.Log("[ReturnToMainMenu] Loading main menu scene...");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}