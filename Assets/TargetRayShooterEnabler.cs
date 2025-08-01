using UnityEngine;

public class TargetRayShooterEnabler : MonoBehaviour
{
    public GameObject leftSelectorHand;
    public GameObject rightSelectorHand;

    void Start()
    {
        bool isLeft = PlayerSettings.IsLeftHanded;

        // Enable the correct hand GameObject
        leftSelectorHand.SetActive(isLeft);
        rightSelectorHand.SetActive(!isLeft);

        // Enable correct TargetRayShooter
        var leftShooter = leftSelectorHand.GetComponent<TargetRayShooter>();
        var rightShooter = rightSelectorHand.GetComponent<TargetRayShooter>();

        if (leftShooter != null) leftShooter.enabled = isLeft;
        if (rightShooter != null) rightShooter.enabled = !isLeft;

        Debug.Log("[ShooterEnabler] Active Hand: " + (isLeft ? "Left" : "Right"));
    }
}
