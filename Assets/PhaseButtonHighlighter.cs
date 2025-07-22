using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PhaseButtonHighlighter : MonoBehaviour
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
                sceneName = "MainMenuScene",
                centerLocalXY = new Vector2(0, 120f),
                size = new Vector2(600f, 220f),
                image = GameObject.Find("PhaseOneButton")?.GetComponent<Image>()
            },
            new ButtonBounds {
                sceneName = "Phase2MenuScene",
                centerLocalXY = new Vector2(0, 80f),
                size = new Vector2(600f, 200f),
                image = GameObject.Find("PhaseTwoButton")?.GetComponent<Image>()
            },
            new ButtonBounds {
                sceneName = "Phase3MenuScene",
                centerLocalXY = new Vector2(0, 40f),
                size = new Vector2(600f, 200f),
                image = GameObject.Find("PhaseThreeButton")?.GetComponent<Image>()
            }
        };
    }

    void Update()
    {
        if (sceneLoading || canvasTransform == null)
        {
            Debug.LogWarning("[PhaseButtonHighlighter] 🚫 Missing references or scene is loading.");
            return;
        }

        HandleHand(leftHandTransform, leftTriggerAction, "👈 Left");
        HandleHand(rightHandTransform, rightTriggerAction, "👉 Right");
    }

    void HandleHand(Transform handTransform, InputActionProperty triggerAction, string handLabel)
    {
        if (handTransform == null || triggerAction.action == null) return;

        Vector3 handLocal = canvasTransform.InverseTransformPoint(handTransform.position);
        handLocal.y *= yReachMultiplier;

        float triggerValue = triggerAction.action.ReadValue<float>();
        bool triggerPressed = triggerValue > 0.5f;

        Debug.Log($"[PhaseHighlight] {handLabel} Hand Local: {handLocal}, Trigger: {triggerValue:F2}");

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
                Debug.Log($"[PhaseHighlight] {handLabel} 🎯 Matched {btn.sceneName}");
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
            Debug.Log($"[PhaseHighlight] {handLabel} ✅ Loading {matchedButton.sceneName}");
            sceneLoading = true;
            SceneManager.LoadScene(matchedButton.sceneName);
        }
        else if (triggerPressed && matchedButton == null)
        {
            Debug.Log($"[PhaseHighlight] {handLabel} ❌ Trigger pressed but no button matched.");
        }
    }
}
