using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ManualButtonHighlighter : MonoBehaviour
{
    [System.Serializable]
    public class ButtonBounds
    {
        public string sceneName;
        public Vector2 centerLocalXY;
        public Vector2 size;
        public Image image; // For highlighting
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

    private bool sceneLoading = false;
    private ButtonBounds[] buttons;

    void Start()
    {
        buttons = new ButtonBounds[]
        {
            new ButtonBounds {
                sceneName = "ArmRaiseScene",
                centerLocalXY = new Vector2(0, 120f),
                size = new Vector2(600f, 220f),
                image = GameObject.Find("ArmRaiseButton")?.GetComponent<Image>()
            },
            new ButtonBounds {
                sceneName = "ArmRotationScene",
                centerLocalXY = new Vector2(0, 80f),
                size = new Vector2(600f, 200f),
                image = GameObject.Find("ArmRotationButton")?.GetComponent<Image>()
            },
            new ButtonBounds {
                sceneName = "TargetPracticeScene",
                centerLocalXY = new Vector2(0, 40f),
                size = new Vector2(600f, 200f),
                image = GameObject.Find("TargetPracticeButton")?.GetComponent<Image>()
            },
            new ButtonBounds {
                sceneName = "ElbowRotationScene",
                centerLocalXY = new Vector2(0, -20f),
                size = new Vector2(600f, 220f),
                image = GameObject.Find("ElbowRotationButton")?.GetComponent<Image>()
            },
            new ButtonBounds {
                sceneName = "CupTransferScene",
                centerLocalXY = new Vector2(0, -60f),
                size = new Vector2(600f, 220f),
                image = GameObject.Find("CupTransferButton")?.GetComponent<Image>()
            }
        };
    }

    void Update()
    {
        if (sceneLoading || canvasTransform == null)
        {
            Debug.LogWarning("[ManualButtonHighlighter] 🚫 Missing canvas or scene is loading.");
            return;
        }

        HandleHand(leftHandTransform, leftTriggerAction, "👈 Left");
        HandleHand(rightHandTransform, rightTriggerAction, "👉 Right");
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
            if (btn.image == null) continue;

            Vector2 half = btn.size * 0.5f;
            float dx = Mathf.Abs(handLocal.x - btn.centerLocalXY.x);
            float dy = Mathf.Abs(handLocal.y - btn.centerLocalXY.y);
            bool inX = dx <= half.x;
            bool inY = dy <= half.y;

            if (inX && inY)
            {
                matchedButton = btn;
                Debug.Log($"[{handLabel}] 🎯 Hovered over {btn.sceneName}");
                break;
            }
        }

        foreach (var btn in buttons)
        {
            if (btn.image != null)
                btn.image.color = (btn == matchedButton) ? highlightColor : normalColor;
        }

        if (triggerPressed && matchedButton != null)
        {
            Debug.Log($"[{handLabel}] ✅ Loading {matchedButton.sceneName}");
            sceneLoading = true;
            SceneManager.LoadScene(matchedButton.sceneName);
        }
        else if (triggerPressed && matchedButton == null)
        {
            Debug.Log($"[{handLabel}] ❌ Trigger pressed but no button matched.");
        }
    }
}
