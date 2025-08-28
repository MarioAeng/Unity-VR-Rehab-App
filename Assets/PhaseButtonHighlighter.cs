using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class PhaseButtonHighlighter : MonoBehaviour
{
    private const string HandednessPrefKey = "HandednessLeft"; // 1 = Left, 0 = Right

    [System.Serializable]
    public class ButtonBounds
    {
        public string sceneName;
        public Image image;
        public RectTransform rect;
        public Vector2 centerLocalXY; // auto
        public Vector2 size;          // auto (+growth, clamped)
        public Vector2 baseSize;      // raw rect size (for reference)
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
    public float yReachMultiplier = 2f;
    [Tooltip("Grow each button's hitbox by this many pixels per side. Will be CLAMPED to avoid overlaps.")]
    public float boundsGrow = 60f;
    [Tooltip("Keep at least this many pixels of vertical gap between adjacent grown hitboxes.")]
    public float minVerticalGap = 8f;

    [Header("Ray Projection")]
    public bool useRayProjection = true;
    public enum RayAxis { Forward, Up, Down }
    public RayAxis rayAxis = RayAxis.Forward;
    public float maxRayDistance = 12f;

    [Header("Mode")]
    public bool leftHandedMode = false;

    [Header("Advanced")]
    public bool autoRecalcEachFrame = false;
    public bool verboseLogs = false;

    private bool sceneLoading = false;
    private ButtonBounds[] buttons;

    void Awake()
    {
        leftHandedMode = PlayerPrefs.GetInt(HandednessPrefKey, 0) == 1;
    }

    void Start()
    {
        TryEnable(leftTriggerAction);
        TryEnable(rightTriggerAction);

        buttons = new ButtonBounds[]
        {
            Make("MainMenuScene",   "PhaseOneButton"),
            Make("Phase2MenuScene", "PhaseTwoButton"),
            Make("Phase3MenuScene", "PhaseThreeButton"),
            Make("Phase4MenuScene", "PhaseFourButton"),
            Make("Phase5MenuScene", "PhaseFiveButton"),
        };

        RebuildButtonBounds();
    }

    void Update()
    {
        if (sceneLoading || canvasTransform == null) return;
        if (autoRecalcEachFrame) RebuildButtonBounds();

        var leftHover  = GetHoverForHand(leftHandTransform,  "Left");
        var rightHover = GetHoverForHand(rightHandTransform, "Right");

        ResetAllHighlights();
        if (leftHover  != null && leftHover.image  != null)  leftHover.image.color  = highlightColor;
        if (rightHover != null && rightHover.image != null) rightHover.image.color = highlightColor;

        HandleTriggerForHand(leftHover,  leftTriggerAction,  "Left");
        HandleTriggerForHand(rightHover, rightTriggerAction, "Right");
    }

    // -------- helpers --------

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
        // 1) Read base centers/sizes in canvas local space
        foreach (var b in buttons)
        {
            if (b?.rect == null) continue;

            Vector3 worldCenter = b.rect.TransformPoint(b.rect.rect.center);
            Vector3 localCenter = canvasTransform.InverseTransformPoint(worldCenter);
            b.centerLocalXY = new Vector2(localCenter.x, localCenter.y);
            b.baseSize = b.rect.rect.size;
        }

        // 2) Sort by Y so we can clamp vertical overlap
        var ordered = buttons
            .Where(b => b != null && b.rect != null)
            .OrderByDescending(b => b.centerLocalXY.y)
            .ToArray();

        // 3) Propose grown half-heights from base sizes
        float[] halfY = new float[ordered.Length];
        for (int i = 0; i < ordered.Length; i++)
            halfY[i] = ordered[i].baseSize.y * 0.5f + Mathf.Max(0f, boundsGrow);

        // 4) Clamp against neighbors to avoid overlap + keep a minimum gap
        for (int i = 0; i < ordered.Length; i++)
        {
            float myY = ordered[i].centerLocalXY.y;

            if (i > 0) // clamp against above neighbor
            {
                float neighborY = ordered[i - 1].centerLocalXY.y;
                float neighborBaseHalf = ordered[i - 1].baseSize.y * 0.5f;
                float maxHalfAllowed = Mathf.Max(0f, (Mathf.Abs(myY - neighborY) - neighborBaseHalf - minVerticalGap));
                halfY[i] = Mathf.Min(halfY[i], maxHalfAllowed);
            }
            if (i < ordered.Length - 1) // clamp against below neighbor
            {
                float neighborY = ordered[i + 1].centerLocalXY.y;
                float neighborBaseHalf = ordered[i + 1].baseSize.y * 0.5f;
                float maxHalfAllowed = Mathf.Max(0f, (Mathf.Abs(myY - neighborY) - neighborBaseHalf - minVerticalGap));
                halfY[i] = Mathf.Min(halfY[i], maxHalfAllowed);
            }
        }

        // 5) Write final (clamped) sizes back; keep width grown but unchanged
        for (int i = 0; i < ordered.Length; i++)
        {
            var b = ordered[i];
            float grownHalfX = b.baseSize.x * 0.5f + Mathf.Max(0f, boundsGrow);
            b.size = new Vector2(grownHalfX * 2f, halfY[i] * 2f);
        }

        if (verboseLogs)
        {
            for (int i = 0; i < ordered.Length; i++)
            {
                var b = ordered[i];
                Debug.Log($"[PhaseButtonHighlighter] {b.sceneName} base={b.baseSize} grown(clamped)={b.size}");
            }
        }
    }

    private void ResetAllHighlights()
    {
        foreach (var btn in buttons)
            if (btn?.image != null) btn.image.color = normalColor;
    }

    private ButtonBounds GetHoverForHand(Transform handTransform, string handLabel)
    {
        if (handTransform == null) return null;

        // Ray → canvas plane
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

        // Local point
        Vector3 local = canvasTransform.InverseTransformPoint(worldPoint);
        local.y *= yReachMultiplier;

        // Exclusive best match (closest center wins)
        ButtonBounds best = null;
        float bestScore = float.MaxValue;

        foreach (var btn in buttons)
        {
            if (btn?.rect == null) continue;

            Vector2 half = btn.size * 0.5f;
            float dx = Mathf.Abs(local.x - btn.centerLocalXY.x);
            float dy = Mathf.Abs(local.y - btn.centerLocalXY.y);

            if (dx <= half.x && dy <= half.y)
            {
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
