using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class MainMenuRayInitializer : MonoBehaviour
{
    [Header("References")]
    public XRRayInteractor rayInteractor;
    public XRInteractorLineVisual rayVisual;
    public InputActionProperty triggerAction;
    public GameObject mainSelectorHand;     // assign in Inspector
    public GameObject handVisual;           // assign in Inspector (child of mainSelectorHand)

    void Start()
    {
        Debug.Log("[RayInit] Initializing ray components...");

        // Auto-assign if not set
        if (rayInteractor == null)
        {
            rayInteractor = GetComponent<XRRayInteractor>();
            Debug.LogWarning("[RayInit] XRRayInteractor auto-assigned from self.");
        }

        if (rayVisual == null)
        {
            rayVisual = GetComponent<XRInteractorLineVisual>();
            Debug.LogWarning("[RayInit] XRInteractorLineVisual auto-assigned from self.");
        }

        // Enable ray components
        if (rayInteractor != null)
        {
            rayInteractor.gameObject.SetActive(true);
            rayInteractor.enabled = true;
            rayInteractor.raycastMask = LayerMask.GetMask("UI");
            Debug.Log("[RayInit] XRRayInteractor enabled and UI layer mask set.");
        }

        if (rayVisual != null)
        {
            rayVisual.enabled = true;
            rayVisual.gameObject.SetActive(true);
            Debug.Log("[RayInit] XRInteractorLineVisual enabled.");
        }

        if (triggerAction.action != null)
        {
            triggerAction.action.Enable();
            Debug.Log("[RayInit] Trigger InputAction enabled.");
        }

        // Ensure hand is visible
        if (mainSelectorHand != null)
        {
            mainSelectorHand.SetActive(true);
            Debug.Log("[RayInit] MainSelectorHand activated.");
        }
        else
        {
            Debug.LogWarning("[RayInit] mainSelectorHand reference not set.");
        }

        if (handVisual != null)
        {
            handVisual.SetActive(true);
            Debug.Log("[RayInit] HandVisual activated.");
        }
        else
        {
            Debug.LogWarning("[RayInit] handVisual reference not set.");
        }
    }
}
