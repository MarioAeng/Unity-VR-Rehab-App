using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RayResetter : MonoBehaviour
{
    [Header("Ray Components")]
    public XRRayInteractor rayInteractor;
    public XRInteractorLineVisual rayVisual;
    public Material fallbackMaterial;

    [Header("Settings")]
    public float fixedRayLength = 10f;
    public Vector3 localOffset = new Vector3(0f, 1.6f, 0.4f);

    private LineRenderer lineRenderer;

    void OnEnable()
    {
        Debug.Log("[RayResetter] OnEnable triggered.");
        ResetRay();
        Invoke(nameof(AlignAndForceLine), 0.2f); // Delay to let scene settle
    }

    void ResetRay()
    {
        if (rayInteractor == null || rayVisual == null)
        {
            Debug.LogError("[RayResetter] ERROR: Missing rayInteractor or rayVisual!");
            return;
        }

        rayInteractor.enabled = true;
        rayInteractor.lineType = XRRayInteractor.LineType.StraightLine;
        rayInteractor.maxRaycastDistance = fixedRayLength;
        rayInteractor.gameObject.SetActive(true);
        Debug.Log("[RayResetter] rayInteractor enabled and active.");

        rayVisual.enabled = true;
        rayVisual.gameObject.SetActive(true);
        Debug.Log("[RayResetter] rayVisual enabled and visible.");

        lineRenderer = rayVisual.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("[RayResetter] LineRenderer is MISSING!");
            return;
        }

        if (lineRenderer.material == null && fallbackMaterial != null)
        {
            lineRenderer.material = fallbackMaterial;
            Debug.LogWarning("[RayResetter] Fallback material assigned to LineRenderer.");
        }

        if (lineRenderer.widthCurve.keys.Length == 0 || lineRenderer.widthCurve.keys[0].value <= 0f)
        {
            lineRenderer.widthCurve = AnimationCurve.Linear(0, 0.01f, 1, 0.01f);
            Debug.LogWarning("[RayResetter] Width curve was flat — fixed.");
        }

        lineRenderer.enabled = true;
        Debug.Log($"[RayResetter] LineRenderer confirmed active. Material: {lineRenderer.material?.name}");
    }

    void AlignAndForceLine()
    {
        if (Camera.main == null || rayInteractor == null || lineRenderer == null)
        {
            Debug.LogError("[RayResetter] Cannot align — missing components.");
            return;
        }

        Vector3 newPos = Camera.main.transform.position + Camera.main.transform.TransformDirection(localOffset);
        Quaternion newRot = Camera.main.transform.rotation;

        rayInteractor.transform.position = newPos;
        rayInteractor.transform.rotation = newRot;

        Vector3 start = rayInteractor.transform.position;
        Vector3 end = start + rayInteractor.transform.forward * fixedRayLength;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.enabled = true;

        Debug.DrawRay(start, rayInteractor.transform.forward * fixedRayLength, Color.cyan, 2f);
        Debug.Log($"[RayResetter] Ray aligned to camera: start={start}, end={end}, forward={rayInteractor.transform.forward}");
    }

    void LateUpdate()
    {
        // Catch unexpected disable
        if (rayVisual != null && !rayVisual.enabled)
        {
            rayVisual.enabled = true;
            rayVisual.gameObject.SetActive(true);
            Debug.LogWarning("[RayResetter] rayVisual was disabled — re-enabled.");
        }

        if (rayInteractor != null && !rayInteractor.enabled)
        {
            rayInteractor.enabled = true;
            Debug.LogWarning("[RayResetter] rayInteractor was disabled — re-enabled.");
        }

        if (lineRenderer != null && !lineRenderer.enabled)
        {
            lineRenderer.enabled = true;
            Debug.LogWarning("[RayResetter] LineRenderer was disabled — re-enabled in LateUpdate.");
        }
    }
}
