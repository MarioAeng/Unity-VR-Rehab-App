using UnityEngine;
using UnityEngine.SceneManagement;

public class HandednessInitializer : MonoBehaviour
{
    public GameObject leftSelectorHand;
    public GameObject rightSelectorHand;

    private Vector3 initialLeftPos;
    private Quaternion initialLeftRot;
    private Vector3 initialRightPos;
    private Quaternion initialRightRot;

    void Awake()
    {
        if (leftSelectorHand != null)
        {
            initialLeftPos = leftSelectorHand.transform.position;
            initialLeftRot = leftSelectorHand.transform.rotation;
        }

        if (rightSelectorHand != null)
        {
            initialRightPos = rightSelectorHand.transform.position;
            initialRightRot = rightSelectorHand.transform.rotation;
        }
    }

    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        bool showBothHands = currentScene == "HandednessSelectorScene";
        bool isLeft = PlayerSettings.IsLeftHanded;

        Debug.Log($"[HandednessInitializer] Scene: {currentScene} | ShowBoth: {showBothHands} | Active: {(isLeft ? "Left" : "Right")}");

        ResetHandTransforms();

        // Only show both in selector, otherwise respect chosen hand
        leftSelectorHand.SetActive(showBothHands || isLeft);
        rightSelectorHand.SetActive(showBothHands || !isLeft);

        // Generic controller reinitialization
        RefreshAllComponents(leftSelectorHand);
        RefreshAllComponents(rightSelectorHand);
    }

    void ResetHandTransforms()
    {
        if (leftSelectorHand != null)
        {
            leftSelectorHand.transform.position = initialLeftPos;
            leftSelectorHand.transform.rotation = initialLeftRot;
            Debug.Log($"[HandednessInitializer] 🟣 Left hand reset to initial transform.");
        }

        if (rightSelectorHand != null)
        {
            rightSelectorHand.transform.position = initialRightPos;
            rightSelectorHand.transform.rotation = initialRightRot;
            Debug.Log($"[HandednessInitializer] 🔵 Right hand reset to initial transform.");
        }
    }

    void RefreshAllComponents(GameObject hand)
    {
        if (hand == null) return;

        var components = hand.GetComponents<Behaviour>();
        foreach (var comp in components)
        {
            if (comp == null) continue;

            comp.enabled = false;
            comp.enabled = true;
            Debug.Log($"[HandednessInitializer] ♻️ Re-enabled component: {comp.GetType().Name} on {hand.name}");
        }
    }
}
