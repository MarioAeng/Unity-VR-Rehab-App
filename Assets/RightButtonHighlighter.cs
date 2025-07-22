using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RightButtonHighlighter : MonoBehaviour
{
    public Transform canvasTransform;
    public Transform handTransform;
    public InputActionProperty triggerAction;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.green;

    [Header("Ray Reach Settings")]
    public float yReachMultiplier = 2f;

    private bool sceneLoading = false;
    private Image buttonImage;
    private readonly Vector2 centerLocalXY = new Vector2(0, 80f);
    private readonly Vector2 size = new Vector2(600f, 220f);
    private readonly string sceneName = "PhaseMenu_Right";

    void Start()
    {
        buttonImage = GameObject.Find("RightHandButton")?.GetComponent<Image>();
    }

    void Update()
    {
        if (sceneLoading || triggerAction.action == null || handTransform == null || canvasTransform == null)
        {
            Debug.LogWarning("[RightHighlighter] 🚫 Missing references or scene loading.");
            return;
        }

        Vector3 handLocal = canvasTransform.InverseTransformPoint(handTransform.position);
        handLocal.y *= yReachMultiplier;

        if (buttonImage == null)
        {
            Debug.LogWarning("[RightHighlighter] ❌ Button image not found.");
            return;
        }

        Vector2 half = size * 0.5f;
        float dx = Mathf.Abs(handLocal.x - centerLocalXY.x);
        float dy = Mathf.Abs(handLocal.y - centerLocalXY.y);
        bool inX = dx <= half.x;
        bool inY = dy <= half.y;

        bool isInside = inX && inY;
        buttonImage.color = isInside ? highlightColor : normalColor;

        float triggerValue = triggerAction.action.ReadValue<float>();
        bool triggerPressed = triggerValue > 0.5f;

        if (triggerPressed)
        {
            if (isInside)
            {
                Debug.Log($"[RightHighlighter] ✅ Triggered {sceneName}");
                sceneLoading = true;
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.Log("[RightHighlighter] ❌ Trigger pressed but not inside button.");
            }
        }
    }
}
