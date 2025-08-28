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
        public Image image;
        public RectTransform rect;
        public Vector2 centerLocalXY; // auto
        public Vector2 size;          // auto (+growth)
    }

    [Header("Canvas / Layout")]
    public Transform canvasTransform; // World-space canvas

    [Header("Hand References")]
    public Transform leftHandTransform;
    public Transform rightHandTransform;
    public InputActionProperty leftTriggerAction;
    public InputActionProperty rightTriggerAction;

    [Header("Visuals")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.green;

    [Header("Hover Tuning")]
    [Tooltip("Stretch Y reach in canvas local space (keeps the 'feel' of your old script).")]
    public float yReachMultiplier = 2f;

    [Tooltip("Grow each button's hitbox by this many pixels per side.")]
    public float boundsGrow = 60f;

    [Tooltip("Use ray-plane intersection instead of hand position (prevents sticky highlights).")]
    public bool useRayProjection = true;

    public enum RayAxis { Forward, Up, Down }
    public RayAxis rayAxis = RayAxis.Forward;
    public float maxRayDistance = 12f;

    [Tooltip("Recalculate rects each frame if UI animates.")]
    public bool autoRecalcEachFrame = false;

    [Tooltip("Debug logging.")]
    public bool verboseLogs = false;

    private bool sceneLoading = false;

    private ButtonBounds[] buttons;

    void Start()
    {
        TryEnable(leftTriggerAction);
        TryEnable(rightTriggerAction);

        buttons = new ButtonBounds[]
        {
            Make("ArmRaiseScene",       "ArmRaiseButton"),
            Make("ArmRotationScene",    "ArmRotationButton"),
            Make("TargetPracticeScene", "TargetPracticeButton"),
            Make("ElbowRotationScene",  "ElbowRotationButton"),
            Make("CupTransferScene",    "CupTransferButton"),
        };

        RebuildButtonBounds();
    }

    void Update()
    {
        if (sceneLoading || canvasTransform == null) return;
        if (autoRecalcEachFrame) RebuildButtonBounds();

        // Compute hover for both hands
        ButtonBounds leftHover  = GetHoverForHand(leftHandTransform,  "Left");
        ButtonBounds rightHover = GetHoverForHand(rightHandTransform, "Right");

        // Reset then apply highlights
        ResetAllHighlights();
        if (leftHover  != null && leftHover.image  != null)  leftHover.image.color  = highlightColor;
        if (rightHover != null && rightHover.image != null) rightHover.image.color = highlightColor;

        // Triggers
        HandleTriggerForHand(leftHover,  leftTriggerAction,  "Left");
        HandleTriggerForHand(rightHover, rightTriggerAction, "Right");
    }

    // ---------- helpers ----------

    private ButtonBounds Make(string scene, string goName)
    {
        var go = GameObject.Find(goName);
        return new ButtonBounds
        {
            sceneName = scene,
            image = go ? go.GetComponent<Image>() : null,
            rect  = go ? go.GetComponent<RectTransform>() : null
        };
    }

    private void RebuildButtonBounds()
    {
        int ok = 0;
        foreach (var b in buttons)
        {
            if (b?.rect == null) continue;

            // Center in canvas local space
            Vector3 worldCenter = b.rect.TransformPoint(b.rect.rect.center);
            Vector3 localCenter = canvasTransform.InverseTransformPoint(worldCenter);
            b.centerLocalXY = new Vector2(localCenter.x, localCenter.y);

            // Size from rect, grown to make hovering easier
            Vector2 sz = b.rect.rect.size;
            sz.x += boundsGrow * 2f;
            sz.y += boundsGrow * 2f;
            b.size = sz;
            ok++;
        }
        if (verboseLogs) Debug.Log($"[ManualButtonHighlighter] Bounds rebuilt for {ok}/{buttons.Length} buttons.");
    }

    private void ResetAllHighlights()
    {
        foreach (var btn in buttons)
            if (btn.image != null) btn.image.color = normalColor;
    }

    private ButtonBounds GetHoverForHand(Transform handTransform, string handLabel)
    {
        if (handTransform == null) return null;

        // World point: ray-plane intersection (default) or hand position
        Vector3 worldPoint;
        if (useRayProjection)
        {
            Vector3 origin = handTransform.position;
            Vector3 dir    = GetRayDir(handTransform);

            Vector3 planePoint  = canvasTransform.position;
            Vector3 planeNormal = canvasTransform.forward;

            float denom = Vector3.Dot(planeNormal, dir);
            if (Mathf.Abs(denom) < 1e-4f) return null;

            float t = Vector3.Dot(planeNormal, (planePoint - origin)) / denom;
            if (t <= 0f || t > maxRayDistance) return null;

            worldPoint = origin + dir * t;
        }
        else
        {
            worldPoint = handTransform.position;
        }

        // Canvas local
        Vector3 local = canvasTransform.InverseTransformPoint(worldPoint);
        local.y *= yReachMultiplier;

        // Exclusive best match inside the grown rects
        ButtonBounds best = null;
        float bestScore = float.MaxValue; // lower is better (closer to center)

        foreach (var btn in buttons)
        {
            if (btn.rect == null) continue;

            Vector2 half = btn.size * 0.5f;
            float dx = Mathf.Abs(local.x - btn.centerLocalXY.x);
            float dy = Mathf.Abs(local.y - btn.centerLocalXY.y);

            if (dx <= half.x && dy <= half.y)
            {
                // Normalize distance to center; prefer the closest center to avoid “top item wins”.
                float sx = dx / Mathf.Max(half.x, 1e-4f);
                float sy = dy / Mathf.Max(half.y, 1e-4f);
                float score = sx + sy;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = btn;
                }
            }
        }

        return best;
    }

    private Vector3 GetRayDir(Transform t)
    {
        switch (rayAxis)
        {
            case RayAxis.Up:   return t.up;
            case RayAxis.Down: return -t.up;
            default:           return t.forward;
        }
    }

    private void HandleTriggerForHand(ButtonBounds hover, InputActionProperty triggerAction, string handLabel)
    {
        var act = triggerAction.action;
        if (sceneLoading || act == null) return;

        if (act.ReadValue<float>() > 0.5f && hover != null)
        {
            sceneLoading = true;
            SceneManager.LoadScene(hover.sceneName);
        }
    }

    private void TryEnable(InputActionProperty prop)
    {
        if (prop.action != null && !prop.action.enabled) prop.action.Enable();
    }
}
