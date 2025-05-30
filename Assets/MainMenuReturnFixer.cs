using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem.XR;

public class MainMenuReturnFixer : MonoBehaviour
{
    [Header("References")]
    public XRRayInteractor rayInteractor;
    public XRInteractorLineVisual rayVisual;
    public ActionBasedController controller;

    [Header("Input Actions")]
    public InputActionProperty triggerAction;
    public InputActionProperty positionAction;
    public InputActionProperty rotationAction;

    [Header("Hand")]
    public Transform handVisual;

    [Header("Debug Options")]
    public bool reapplyOnUpdate = true;
    public Vector3 fallbackPosition = new Vector3(0, 1.5f, 0.4f);
    public Quaternion fallbackRotation = Quaternion.Euler(0, 180, 0);
    private float lastLogTime = 0f;

    void OnEnable()
    {
        Debug.Log("[Fixer] OnEnable");
        InitializeAll("OnEnable");
    }

    void Start()
    {
        Debug.Log("[Fixer] Start");

        foreach (var device in InputSystem.devices)
        {
            if (device.description.interfaceName?.Contains("XR") == true)
            {
                InputSystem.EnableDevice(device);
                Debug.Log($"[Fixer:Start] Enabled XR device: {device.name}");
            }
        }

        InitializeAll("Start");
    }

    void InitializeAll(string caller)
    {
        if (rayInteractor == null || rayVisual == null)
        {
            Debug.LogError($"[Fixer:{caller}] Ray components missing");
            return;
        }

        if (handVisual == null)
        {
            GameObject fallback = GameObject.FindWithTag("SimulatedHand");
            handVisual = fallback?.transform;
            Debug.LogWarning($"[Fixer:{caller}] handVisual is null, found by tag: {fallback?.name}");
        }

        if (controller != null)
        {
            controller.positionAction = positionAction;
            controller.rotationAction = rotationAction;
            controller.selectAction = triggerAction;
            controller.uiPressAction = triggerAction;
            controller.enableInputActions = true;
            controller.enableInputTracking = true;

            Debug.Log("[Fixer] Controller input actions assigned");
        }

        rayInteractor.enabled = true;
        rayInteractor.gameObject.SetActive(true);
        rayVisual.enabled = true;
        rayVisual.gameObject.SetActive(true);
        rayInteractor.raycastMask = LayerMask.GetMask("UI");

        if (rayVisual.reticle != null)
            rayVisual.reticle.SetActive(true);

        triggerAction.action?.Enable();
        positionAction.action?.Enable();
        rotationAction.action?.Enable();

        ApplyTransform(caller);
    }

    void Update()
    {
        if (!reapplyOnUpdate || handVisual == null) return;

        ApplyTransform("Update");

        if (Time.time - lastLogTime > 2f)
        {
            Debug.Log($"[Fixer:Update] handVisual.position = {handVisual.position}");
            Debug.Log($"[Fixer:Update] Trigger enabled = {triggerAction.action?.enabled}, Pos = {positionAction.action?.enabled}, Rot = {rotationAction.action?.enabled}");
            lastLogTime = Time.time;
        }
    }

    void ApplyTransform(string context)
    {
        if (handVisual != null)
        {
            rayInteractor.attachTransform = handVisual;
            rayInteractor.rayOriginTransform = handVisual;
            rayInteractor.transform.SetPositionAndRotation(handVisual.position, handVisual.rotation);
            Debug.Log($"[Fixer:{context}] Ray set to handVisual at {handVisual.position}");
        }
        else
        {
            rayInteractor.attachTransform = null;
            rayInteractor.rayOriginTransform = null;
            rayInteractor.transform.SetPositionAndRotation(fallbackPosition, fallbackRotation);
            Debug.LogWarning($"[Fixer:{context}] Using fallback transform");
        }

        Debug.DrawRay(rayInteractor.transform.position, rayInteractor.transform.forward * 5f, Color.cyan, 1f);
    }
}
