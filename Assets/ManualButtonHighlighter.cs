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
    public Transform handTransform;
    public InputActionProperty triggerAction;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.green;

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
            }
        };
    }

    void Update()
    {
        if (sceneLoading || triggerAction.action == null || handTransform == null || canvasTransform == null)
        {
            Debug.LogWarning("[ManualButtonHighlighter] 🚫 Missing references or scene is loading.");
            return;
        }

        Vector3 handLocal = canvasTransform.InverseTransformPoint(handTransform.position);
        float triggerValue = triggerAction.action.ReadValue<float>();
        bool triggerPressed = triggerValue > 0.5f;

        Debug.Log($"[Input] ✋ Hand Local: {handLocal}, 🔫 Trigger: {triggerValue:F2} | Pressed: {triggerPressed}");

        ButtonBounds matchedButton = null;

        foreach (var btn in buttons)
        {
            if (btn.image == null) continue;

            Vector2 half = btn.size * 0.5f;
            float dx = Mathf.Abs(handLocal.x - btn.centerLocalXY.x);
            float dy = Mathf.Abs(handLocal.y - btn.centerLocalXY.y);
            bool inX = dx <= half.x;
            bool inY = dy <= half.y;

            bool isInside = inX && inY;
            if (isInside && matchedButton == null)
            {
                matchedButton = btn;
                Debug.Log($"[Match] 🎯 Inside {btn.sceneName} | dx={dx:F1}, dy={dy:F1}");
            }
        }

        // Highlighting logic – only one gets highlighted
        foreach (var btn in buttons)
        {
            if (btn.image != null)
                btn.image.color = (btn == matchedButton) ? highlightColor : normalColor;
        }

        if (triggerPressed && matchedButton != null)
        {
            Debug.Log($"[SceneLoad] ✅ Triggered {matchedButton.sceneName} at handLocal={handLocal}");
            sceneLoading = true;
            SceneManager.LoadScene(matchedButton.sceneName);
        }
        else if (triggerPressed && matchedButton == null)
        {
            Debug.Log("[SceneLoad] ❌ Trigger pressed but no button matched.");
        }
    }
}
