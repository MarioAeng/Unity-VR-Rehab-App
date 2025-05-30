using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class MainMenuRayRestorer : MonoBehaviour
{
    [Header("References")]
    public XRRayInteractor rayInteractor;
    public XRInteractorLineVisual rayVisual;
    public GameObject handVisual;
    public InputActionProperty triggerAction;

    [Header("Settings")]
    public float rayLength = 10f;
    public Material fallbackMaterial;
    public Vector3 handOffset = new Vector3(0, 0, 0.3f);

    private LineRenderer lineRenderer;

    void OnEnable()
    {
        Debug.Log("[Restorer] OnEnable called.");

        if (rayInteractor == null || rayVisual == null)
        {
            Debug.LogError("[Restorer] Missing rayInteractor or rayVisual reference.");
            return;
        }

        if (handVisual != null)
        {
            handVisual.SetActive(true);
            handVisual.transform.localPosition = Vector3.zero;
            handVisual.transform.localRotation = Quaternion.identity;
            Debug.Log("[Restorer] HandVisual reactivated and reset.");
        }
        else
        {
            Debug.LogWarning("[Restorer] HandVisual reference not set.");
        }

        // Reset Ray Interactor
        rayInteractor.enabled = true;
        rayInteractor.lineType = XRRayInteractor.LineType.StraightLine;
        rayInteractor.maxRaycastDistance = rayLength;
        rayInteractor.raycastMask = LayerMask.GetMask("UI");
        Debug.Log($"[Restorer] RayInteractor enabled with length {rayLength}");

        // Enable ray visual
        rayVisual.enabled = true;
        rayVisual.gameObject.SetActive(true);
        Debug.Log("[Restorer] RayVisual re-enabled.");

        // Restore Line Renderer
        lineRenderer = rayVisual.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            if (lineRenderer.material == null && fallbackMaterial != null)
            {
                lineRenderer.material = fallbackMaterial;
                Debug.Log("[Restorer] Fallback material applied to LineRenderer.");
            }

            Debug.Log($"[Restorer] LineRenderer width: {lineRenderer.widthMultiplier}, material: {lineRenderer.material?.name}");
        }
        else
        {
            Debug.LogWarning("[Restorer] No LineRenderer on rayVisual.");
        }

        // Enable trigger
        if (triggerAction.action != null)
        {
            triggerAction.action.Enable();
            Debug.Log("[Restorer] Trigger action enabled.");
        }

        // Delay position fix
        Invoke(nameof(FinalizeTransform), 0.2f);
    }

    void FinalizeTransform()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[Restorer] No main camera found.");
            return;
        }

        Vector3 camFwd = cam.transform.forward;
        Vector3 newPos = cam.transform.position + cam.transform.TransformDirection(handOffset);
        Quaternion newRot = cam.transform.rotation;

        rayInteractor.transform.position = newPos;
        rayInteractor.transform.rotation = newRot;

        Debug.Log($"[Restorer] Ray position reset to {newPos}, direction: {rayInteractor.transform.forward}");

        Debug.DrawRay(newPos, rayInteractor.transform.forward * rayLength, Color.green, 2f);
    }
}
