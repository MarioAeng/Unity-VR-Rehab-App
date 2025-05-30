using UnityEngine;
using UnityEngine.SceneManagement;

public class TestSceneLoader : MonoBehaviour
{
    public void LoadSceneManually()
    {
        Debug.Log("Button was clicked — Scene should change now.");
        SceneManager.LoadScene("ArmRotationScene"); // Replace with your real scene name
    }
}
