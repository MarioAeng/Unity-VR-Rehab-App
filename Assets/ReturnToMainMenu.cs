using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ReturnToMainMenu : MonoBehaviour
{
    public InputActionProperty rightBackAction;
    public InputActionProperty leftBackAction;
    public string mainMenuSceneName = "MainMenuScene";

    void OnEnable()
    {
        if (rightBackAction.action != null)
        {
            rightBackAction.action.Enable();
            rightBackAction.action.performed += OnBackPressed;
        }

        if (leftBackAction.action != null)
        {
            leftBackAction.action.Enable();
            leftBackAction.action.performed += OnBackPressed;
        }
    }

    void OnDisable()
    {
        if (rightBackAction.action != null)
            rightBackAction.action.performed -= OnBackPressed;

        if (leftBackAction.action != null)
            leftBackAction.action.performed -= OnBackPressed;
    }

    private void OnBackPressed(InputAction.CallbackContext ctx)
    {
        Debug.Log("[ReturnToMainMenu] ⏪ B Press Detected, returning...");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}