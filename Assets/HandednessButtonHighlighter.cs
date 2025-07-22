using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandednessButtonHighlighter : MonoBehaviour
{
    [System.Serializable]
    public class ButtonBounds
    {
        public GameObject buttonObject;
        public Vector2 centerLocalXY;
        public Vector2 size;
        public Image image;

        // New: track if currently highlighted
        public bool isHighlighted = false;
    }

    [Header("Left Hand Settings")]
    public Transform leftHandTransform;
    public InputActionProperty leftTriggerAction;

    [Header("Right Hand Settings")]
    public Transform rightHandTransform;
    public InputActionProperty rightTriggerAction;

    [Header("Shared Settings")]
    public Transform canvasTransform;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.green;
    public float yReachMultiplier = 2f;

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
        if (canvasTransform == null) return;

        // Reset highlight flags
        foreach (var btn in buttons)
            btn.isHighlighted = false;

        HandleHand(leftHandTransform, leftTriggerAction, "Left");
        HandleHand(rightHandTransform, rightTriggerAction, "Right");

        // Apply highlight colors based on flags
        foreach (var btn in buttons)
        {
            if (btn.image != null)
                btn.image.color = btn.isHighlighted ? highlightColor : normalColor;
        }
    }

    void HandleHand(Transform handTransform, InputActionProperty triggerAction, string handLabel)
    {
        if (handTransform == null || triggerAction.action == null) return;

        Vector3 handLocal = canvasTransform.InverseTransformPoint(handTransform.position);
        handLocal.y *= yReachMultiplier;

        bool triggerPressed = triggerAction.action.WasPressedThisFrame();

        foreach (var btn in buttons)
        {
            if (btn.image == null || btn.buttonObject == null) continue;

            Vector2 half = btn.size * 0.5f;
            float dx = Mathf.Abs(handLocal.x - btn.centerLocalXY.x);
            float dy = Mathf.Abs(handLocal.y - btn.centerLocalXY.y);
            bool inBounds = dx <= half.x && dy <= half.y;

            if (inBounds)
            {
                btn.isHighlighted = true;
                Debug.Log($"[Highlight] ✅ {handLabel} matched {btn.buttonObject.name}");

                if (triggerPressed)
                {
                    Debug.Log($"[UI Click] 🖱️ {handLabel} triggered OnClick for {btn.buttonObject.name}");

                    ExecuteEvents.Execute<IPointerClickHandler>(
                        target: btn.buttonObject,
                        eventData: new PointerEventData(EventSystem.current),
                        functor: ExecuteEvents.pointerClickHandler
                    );
                }
            }
        }
    }
}
