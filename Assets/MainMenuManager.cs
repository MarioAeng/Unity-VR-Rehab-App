using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names (Make sure they match Build Settings)")]
    public string armRaiseSceneName = "ArmRaiseScene";
    public string armRotationSceneName = "ArmRotationScene";
    public string targetPracticeSceneName = "TargetPracticeScene";
    public string elbowRotationSceneName = "ElbowRotationScene";
    public string cupTransferSceneName = "CupTransferScene"; // NEW

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

    public void LoadTargetPractice()
    {
        Debug.Log("[MainMenuManager] Loading Target Practice Scene...");
        SceneManager.LoadScene(targetPracticeSceneName);
    }

    public void LoadElbowRotation()
    {
        Debug.Log("[MainMenuManager] Loading Elbow Rotation Scene...");
        SceneManager.LoadScene(elbowRotationSceneName);
    }

    public void LoadCupTransfer()
    {
        Debug.Log("[MainMenuManager] Loading Cup Transfer Scene...");
        SceneManager.LoadScene(cupTransferSceneName);
    }

    public void QuitApp()
    {
        Debug.Log("[MainMenuManager] Quitting Application.");
        Application.Quit();
    }
}