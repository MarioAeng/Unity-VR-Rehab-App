using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names (Make sure they match Build Settings)")]
    public string armRaiseSceneName = "ArmRaiseScene";
    public string armRotationSceneName = "ArmRotationScene";

    public void LoadArmRaise()
    {
        Debug.Log("[MainMenuManager] Loading Arm Raise Scene...");
        SceneManager.LoadScene(armRaiseSceneName);
    }

    public void LoadArmRotation()
    {
        Debug.Log("[MainMenuManager] Loading Arm Rotation Scene...");
        SceneManager.LoadScene(armRotationSceneName);
    }

    public void QuitApp()
    {
        Debug.Log("[MainMenuManager] Quitting Application.");
        Application.Quit();
    }
}