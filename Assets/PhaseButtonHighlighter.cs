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
    public Transform handTransform;
    public InputActionProperty triggerAction;
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
                sceneName = "MainMenuScene", // Phase 1
                centerLocalXY = new Vector2(0, 120f),
                size = new Vector2(600f, 220f),
                image = GameObject.Find("PhaseOneButton")?.GetComponent<Image>()
            },
            new ButtonBounds {
                sceneName = "Phase2MenuScene", // Phase 2
                centerLocalXY = new Vector2(0, 80f),
                size = new Vector2(600f, 200f),
                image = GameObject.Find("PhaseTwoButton")?.GetComponent<Image>()
            },
            new ButtonBounds {
                sceneName = "Phase3MenuScene", // Phase 3
                centerLocalXY = new Vector2(0, 40f),
                size = new Vector2(600f, 200f),
                image = GameObject.Find("PhaseThreeButton")?.GetComponent<Image>()
            }
        };
    }

    void Update()
    {
        if (sceneLoading || triggerAction.action == null || handTransform == null || canvasTransform == null)
        {
            Debug.LogWarning("[PhaseButtonHighlighter] 🚫 Missing references or scene is loading.");
            return;
        }

        Vector3 handLocal = canvasTransform.InverseTransformPoint(handTransform.position);
        handLocal.y *= yReachMultiplier;

        float triggerValue = triggerAction.action.ReadValue<float>();
        bool triggerPressed = triggerValue > 0.5f;

        Debug.Log($"[PhaseHighlight] ✋ Adjusted Hand Local: {handLocal}, 🔫 Trigger: {triggerValue:F2}");

        ButtonBounds matchedButton = null;

        foreach (var btn in buttons)
        {
            if (btn.image == null) continue;

            Vector2 half = btn.size * 0.5f;
            float dx = Mathf.Abs(handLocal.x - btn.centerLocalXY.x);
            float dy = Mathf.Abs(handLocal.y - btn.centerLocalXY.y);
            bool inX = dx <= half.x;
            bool inY = dy <= half.y;

            if (inX && inY && matchedButton == null)
            {
                matchedButton = btn;
                Debug.Log($"[PhaseHighlight] 🎯 Hit {btn.sceneName} | dx={dx:F1}, dy={dy:F1}");
            }
        }

        foreach (var btn in buttons)
        {
            if (btn.image != null)
                btn.image.color = (btn == matchedButton) ? highlightColor : normalColor;
        }

        if (triggerPressed && matchedButton != null)
        {
            Debug.Log($"[PhaseHighlight] ✅ Loading {matchedButton.sceneName}");
            sceneLoading = true;
            SceneManager.LoadScene(matchedButton.sceneName);
        }
    }
}
