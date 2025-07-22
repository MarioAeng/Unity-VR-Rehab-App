using UnityEngine;

public class SelectorHandResetter : MonoBehaviour
{
    public GameObject leftHand;
    public GameObject rightHand;

    private static Vector3? initialLeftPosition;
    private static Quaternion? initialLeftRotation;
    private static Vector3? initialRightPosition;
    private static Quaternion? initialRightRotation;

    void Awake()
    {
        if (leftHand != null && leftHand.activeInHierarchy && initialLeftPosition == null)
        {
            initialLeftPosition = leftHand.transform.position;
            initialLeftRotation = leftHand.transform.rotation;
            Debug.Log("[SelectorHandResetter] ✅ Stored initial LEFT hand position.");
        }

        if (rightHand != null && rightHand.activeInHierarchy && initialRightPosition == null)
        {
            initialRightPosition = rightHand.transform.position;
            initialRightRotation = rightHand.transform.rotation;
            Debug.Log("[SelectorHandResetter] ✅ Stored initial RIGHT hand position.");
        }
    }

    public void ResetHands()
    {
        if (leftHand != null && initialLeftPosition.HasValue)
        {
            leftHand.transform.position = initialLeftPosition.Value;
            leftHand.transform.rotation = initialLeftRotation.Value;

            foreach (var renderer in leftHand.GetComponentsInChildren<Renderer>())
                renderer.enabled = true;

            Debug.Log($"[SelectorHandResetter] ✅ Left hand reset to {leftHand.transform.position}");
        }
        else
        {
            Debug.LogWarning("[SelectorHandResetter] ⚠️ Left hand not reset (no initial position stored).");
        }

        if (rightHand != null && initialRightPosition.HasValue)
        {
            rightHand.transform.position = initialRightPosition.Value;
            rightHand.transform.rotation = initialRightRotation.Value;

            foreach (var renderer in rightHand.GetComponentsInChildren<Renderer>())
                renderer.enabled = true;

            Debug.Log($"[SelectorHandResetter] ✅ Right hand reset to {rightHand.transform.position}");
        }
        else
        {
            Debug.LogWarning("[SelectorHandResetter] ⚠️ Right hand not reset (no initial position stored).");
        }
    }

    // Optional static bridge if needed
    public static void ResetHandsStatic()
    {
        var instance = FindObjectOfType<SelectorHandResetter>();
        if (instance != null)
        {
            Debug.Log("[SelectorHandResetter] 🔁 Calling ResetHands from static method.");
            instance.ResetHands();
        }
        else
        {
            Debug.LogWarning("[SelectorHandResetter] ⚠️ No instance found in scene for static reset.");
        }
    }
}
