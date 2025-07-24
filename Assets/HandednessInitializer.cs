using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

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

        // Activate appropriate hands
        leftSelectorHand.SetActive(showBothHands || isLeft);
        rightSelectorHand.SetActive(showBothHands || !isLeft);

        // Refresh just the required visuals (ray + line)
        RefreshVisuals(leftSelectorHand);
        RefreshVisuals(rightSelectorHand);

        // Also re-enable InputSystem components to fix hand position tracking
        ReEnableInputActions(leftSelectorHand);
        ReEnableInputActions(rightSelectorHand);
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

    void RefreshVisuals(GameObject hand)
    {
        if (hand == null) return;

        var rayInteractor = hand.GetComponent<XRRayInteractor>();
        var lineVisual = hand.GetComponent<XRInteractorLineVisual>();

        if (rayInteractor != null)
        {
            rayInteractor.enabled = false;
            rayInteractor.enabled = true;
            Debug.Log($"[HandednessInitializer] ♻️ Refreshed XRRayInteractor on {hand.name}");
        }

        if (lineVisual != null)
        {
            lineVisual.enabled = false;
            lineVisual.enabled = true;
            Debug.Log($"[HandednessInitializer] ♻️ Refreshed XRInteractorLineVisual on {hand.name}");
        }
    }

    void ReEnableInputActions(GameObject hand)
    {
        if (hand == null) return;

        var inputBehaviours = hand.GetComponents<MonoBehaviour>();
        foreach (var comp in inputBehaviours)
        {
            if (comp == null) continue;

            var typeName = comp.GetType().Name;
            if (typeName.Contains("Input") || typeName.Contains("Pose") || typeName.Contains("Simulated"))
            {
                comp.enabled = false;
                comp.enabled = true;
                Debug.Log($"[HandednessInitializer] ♻️ Re-enabled input-related component: {typeName} on {hand.name}");
            }
        }
    }
}
