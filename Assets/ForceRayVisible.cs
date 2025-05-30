using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ForceRayVisible : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    public XRInteractorLineVisual rayVisual;
    public GameObject handVisual;

    void Start()
    {
        if (rayInteractor == null || rayVisual == null || handVisual == null)
        {
            Debug.LogError("[ForceRayVisible] One or more references are missing!");
            return;
        }

        rayInteractor.enabled = true;
        rayInteractor.gameObject.SetActive(true);
        rayInteractor.raycastMask = LayerMask.GetMask("UI");
        rayInteractor.maxRaycastDistance = 10f;

        rayVisual.enabled = true;
        rayVisual.gameObject.SetActive(true);
        rayVisual.lineLength = 3f;

        handVisual.SetActive(true);
        Debug.Log("[ForceRayVisible] Ray and hand visual forcibly enabled.");
    }

    void Update()
    {
        Debug.DrawRay(rayInteractor.transform.position, rayInteractor.transform.forward * 3f, Color.green);
    }
}