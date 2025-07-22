using UnityEngine;
using UnityEngine.SceneManagement;

public class HandednessSelectorManager : MonoBehaviour
{
    [Header("Selector Hands")]
    public GameObject rightSelectorHand;
    public GameObject leftSelectorHand;

    [Header("Scene")]
    public string nextSceneName = "PhaseSelectionScene"; // Set in Inspector

    private bool handednessChosen = false;

    public void SelectRightHanded()
    {
        Debug.Log("[🟢 SelectRightHanded] Method triggered.");
        if (handednessChosen) return;

        handednessChosen = true;
        PlayerSettings.IsLeftHanded = false;
        HideVisuals(leftSelectorHand);
        Debug.Log("[✅ SelectRightHanded] Right hand selected.");
        LoadNextScene();
    }

    public void SelectLeftHanded()
    {
        Debug.Log("[🟢 SelectLeftHanded] Method triggered.");
        if (handednessChosen) return;

        handednessChosen = true;
        PlayerSettings.IsLeftHanded = true;
        HideVisuals(rightSelectorHand);
        Debug.Log("[✅ SelectLeftHanded] Left hand selected.");
        LoadNextScene();
    }

    private void HideVisuals(GameObject handObject)
    {
        if (handObject == null)
        {
            Debug.LogWarning("[❗ HideVisuals] Null reference.");
            return;
        }

        foreach (var renderer in handObject.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }

        Debug.Log("[👋 HideVisuals] Hidden visuals for: " + handObject.name);
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log("[⏭️ LoadNextScene] Loading scene: " + nextSceneName);
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("[❌ LoadNextScene] Scene name is null or empty.");
        }
    }
}