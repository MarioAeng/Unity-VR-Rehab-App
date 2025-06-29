using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

public class RaycastSceneLoader : MonoBehaviour
{
    [System.Serializable]
    public class SceneButton
    {
        public string sceneName;
        public GameObject buttonObject;
        public Image buttonImage;
        public Color normalColor = Color.white;
        public Color highlightColor = Color.green;
    }

    [Header("References")]
    public XRRayInteractor rayInteractor;
    public InputActionProperty triggerAction;
    public SceneButton[] buttons;

    private bool sceneLoading = false;
    private bool wasPressedLastFrame = false;

    void Start()
    {
        Debug.Log("[RaycastSceneLoader] Start() called");

        if (rayInteractor == null)
            Debug.LogError("[RaycastSceneLoader] XRRayInteractor is NOT assigned!");

        if (triggerAction.action == null)
            Debug.LogError("[RaycastSceneLoader] Trigger InputAction is NOT assigned!");

        if (buttons == null || buttons.Length == 0)
            Debug.LogWarning("[RaycastSceneLoader] No buttons configured!");

        XRRayInteractor[] allInteractors = FindObjectsOfType<XRRayInteractor>();
        if (allInteractors.Length > 1)
            Debug.LogWarning($"[RaycastSceneLoader] Multiple XRRayInteractors found in scene ({allInteractors.Length}) - possible conflict!");

        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (var c in canvases)
        {
            if (c.renderMode == RenderMode.WorldSpace && c.worldCamera == null)
                Debug.LogWarning($"[RaycastSceneLoader] Canvas '{c.name}' has World Space render mode but no Event Camera assigned.");
        }
    }

    void Update()
    {
        Debug.Log("[RaycastSceneLoader] Update() called");

        if (sceneLoading)
        {
            Debug.Log("[RaycastSceneLoader] Scene is already loading. Skipping update.");
            return;
        }

        if (rayInteractor == null || triggerAction.action == null)
        {
            Debug.LogWarning("[RaycastSceneLoader] Missing reference(s). Update skipped.");
            return;
        }

        var action = triggerAction.action;
        float triggerValue = 0f;

        try { triggerValue = action.ReadValue<float>(); }
        catch { Debug.LogWarning("[RaycastSceneLoader] Trigger action is not float-based."); }

        bool isCurrentlyPressed = action.IsPressed();
        bool justPressed = isCurrentlyPressed && !wasPressedLastFrame;

        Debug.Log($"[RaycastSceneLoader] Action Phase: {action.phase}, Value: {triggerValue}, IsPressed: {isCurrentlyPressed}, JustPressed: {justPressed}");

        bool raycastSuccess = rayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult result);

        if (raycastSuccess)
        {
            Debug.Log(result.gameObject != null
                ? $"[RaycastSceneLoader] UI Raycast hit: {result.gameObject.name}"
                : "[RaycastSceneLoader] Raycast result has no gameObject!");

            bool matchedAny = false;

            foreach (var btn in buttons)
            {
                if (btn == null)
                {
                    Debug.LogError("[RaycastSceneLoader] One of the buttons is null!");
                    continue;
                }

                if (btn.buttonObject == null || btn.buttonImage == null)
                {
                    Debug.LogWarning($"[RaycastSceneLoader] Missing reference on button '{btn.sceneName}'");
                    continue;
                }

                bool isHovering = result.gameObject == btn.buttonObject
                    || result.gameObject.transform.IsChildOf(btn.buttonObject.transform)
                    || btn.buttonObject.transform.IsChildOf(result.gameObject.transform);

                btn.buttonImage.color = isHovering ? btn.highlightColor : btn.normalColor;

                if (isHovering)
                {
                    matchedAny = true;
                    Debug.Log($"[RaycastSceneLoader] Hovering over: {btn.sceneName}");

                    if (justPressed)
                    {
                        Debug.Log($"[RaycastSceneLoader] Triggered load for scene: {btn.sceneName}");
                        sceneLoading = true;
                        SceneManager.LoadScene(btn.sceneName);
                        break;
                    }
                    else
                    {
                        Debug.Log("[RaycastSceneLoader] Hovered but trigger not pressed.");
                    }
                }
            }

            if (!matchedAny)
                Debug.Log("[RaycastSceneLoader] Raycast hit UI, but no button matched.");
        }
        else
        {
            Debug.LogWarning("[RaycastSceneLoader] No UI raycast result.");
            foreach (var btn in buttons)
            {
                if (btn.buttonImage != null)
                    btn.buttonImage.color = btn.normalColor;
            }
        }

        wasPressedLastFrame = isCurrentlyPressed;
    }
}
