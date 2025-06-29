using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CupResetAfterDrop : MonoBehaviour
{
    private XRGrabInteractable interactable;
    private Vector3 originalScale;

    void Awake()
    {
        interactable = GetComponent<XRGrabInteractable>();
        if (interactable != null)
        {
            interactable.selectExited.AddListener(ResetScale);
        }

        originalScale = transform.localScale; // Save the prefab scale
    }

    private void ResetScale(SelectExitEventArgs args)
    {
        transform.localScale = originalScale;
        Debug.Log("[CupResetAfterDrop] Scale reset to: " + originalScale);
    }

    void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.selectExited.RemoveListener(ResetScale);
        }
    }
}