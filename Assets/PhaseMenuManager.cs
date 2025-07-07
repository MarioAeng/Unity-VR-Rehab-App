using UnityEngine;
using UnityEngine.SceneManagement;

public class PhaseMenuManager : MonoBehaviour
{
    [Header("Phase Scene Names (Match Build Settings)")]
    public string phase1Scene = "MainMenuScene";      // This loads your original exercise menu
    public string phase2Scene = "Phase2MenuScene";    // Placeholder for future phase menu
    public string phase3Scene = "Phase3MenuScene";    // Placeholder for future phase menu

    public void LoadPhase1()
    {
        Debug.Log("[PhaseMenuManager] Loading Phase 1 Menu...");
        SceneManager.LoadScene(phase1Scene);
    }

    public void LoadPhase2()
    {
        Debug.Log("[PhaseMenuManager] Loading Phase 2 Menu...");
        SceneManager.LoadScene(phase2Scene);
    }

    public void LoadPhase3()
    {
        Debug.Log("[PhaseMenuManager] Loading Phase 3 Menu...");
        SceneManager.LoadScene(phase3Scene);
    }

    public void QuitApp()
    {
        Debug.Log("[PhaseMenuManager] Quitting Application.");
        Application.Quit();
    }
}