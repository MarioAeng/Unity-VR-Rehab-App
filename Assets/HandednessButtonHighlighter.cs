using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandednessButtonHighlighter : MonoBehaviour
{
    [System.Serializable]
    public class ButtonBounds
    {
        public GameObject buttonObject;     // was sceneName in your menu script
        public Vector2 centerLocalXY;
        public Vector2 size;
        public Image image;                 // For highlighting
    }

    public Transform canvasTransform;

    [Header("Hand References")]
    public Transform leftHandTransform;
    public Transform rightHandTransform;
    public InputActionProperty leftTriggerAction;
    public InputActionProperty rightTriggerAction;

    public Color normalColor = Color.white;
    public Color highlightColor = Color.green;

    [Header("Ray Reach Settings")]
    public float yReachMultiplier = 2f;

    private bool sceneLoading = false; // keep same flag to prevent double clicks
    private ButtonBounds[] buttons;

    void Start()
    {
        buttons = new ButtonBounds[]
        {
            new ButtonBounds {
                buttonObject = GameObject.Find("LeftHandButton"),
                centerLocalXY = new Vector2(0, 120f),
                size = new Vector2(600f, 220f),
                image = GameObject.Find("LeftHandButton")?.GetComponent<Image>()
            },
            new ButtonBounds {
                buttonObject = GameObject.Find("RightHandButton"),
                centerLocalXY = new Vector2(0, 80f),
                size = new Vector2(600f, 220f),
                image = GameObject.Find("RightHandButton")?.GetComponent<Image>()
            }
        };
    }

    void Update()
    {
        if (sceneLoading || canvasTransform == null)
        {
            Debug.LogWarning("[HandednessButtonHighlighter] 🚫 Missing canvas or click in progress.");
            return;
        }

        // IMPORTANT: call Right first, then Left so Left wins if both overlap
        HandleHand(rightHandTransform, rightTriggerAction, "👉 Right");
        HandleHand(leftHandTransform,  leftTriggerAction,  "👈 Left");
    }

    void HandleHand(Transform handTransform, InputActionProperty triggerAction, string handLabel)
    {
        if (sceneLoading || handTransform == null || triggerAction.action == null)
            return;

        Vector3 handLocal = canvasTransform.InverseTransformPoint(handTransform.position);
        handLocal.y *= yReachMultiplier;

        float triggerValue = triggerAction.action.ReadValue<float>();
        bool triggerPressed = triggerValue > 0.5f;

        Debug.Log($"[{handLabel}] ✋ Hand Local: {handLocal}, Trigger: {triggerValue:F2}");

        ButtonBounds matchedButton = null;

        foreach (var btn in buttons)
        {
            if (btn.image == null || btn.buttonObject == null) continue;

            Vector2 half = btn.size * 0.5f;
            float dx = Mathf.Abs(handLocal.x - btn.centerLocalXY.x);
            float dy = Mathf.Abs(handLocal.y - btn.centerLocalXY.y);
            bool inX = dx <= half.x;
            bool inY = dy <= half.y;

            if (inX && inY)
            {
                matchedButton = btn;
                Debug.Log($"[{handLabel}] 🎯 Hovered over {btn.buttonObject.name}");
                break;
            }
        }

        // Set colors based on THIS hand's match (the later hand call wins, like your original)
        foreach (var btn in buttons)
        {
            if (btn.image != null)
                btn.image.color = (btn == matchedButton) ? highlightColor : normalColor;
        }

        if (triggerPressed && matchedButton != null)
        {
            Debug.Log($"[{handLabel}] ✅ Clicking {matchedButton.buttonObject.name}");
            sceneLoading = true; // reuse to debounce double-press
            ExecuteEvents.Execute<IPointerClickHandler>(
                target: matchedButton.buttonObject,
                eventData: new PointerEventData(EventSystem.current),
                functor: ExecuteEvents.pointerClickHandler
            );
            // allow another click next frame
            sceneLoading = false;
        }
        else if (triggerPressed && matchedButton == null)
        {
            Debug.Log($"[{handLabel}] ❌ Trigger pressed but no button matched.");
        }
    }
}
