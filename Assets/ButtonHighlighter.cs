using UnityEngine;
using UnityEngine.UI;

public class ButtonHighlighter : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Transform rayOrigin;
    public float highlightDistance = 1.5f;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.green;

    private Button button;
    private Image image;

    void Start()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();

        if (button == null || image == null)
            Debug.LogWarning("[Highlighter] Missing Button or Image on " + gameObject.name);
    }

    void Update()
    {
        if (rayOrigin == null || button == null || image == null)
            return;

        float dist = Vector3.Distance(transform.position, rayOrigin.position);
        bool shouldHighlight = dist <= highlightDistance;

        image.color = shouldHighlight ? highlightColor : normalColor;

        Debug.Log($"[Highlighter] {gameObject.name} dist = {dist:F2} → {(shouldHighlight ? "HIGHLIGHTED" : "normal")}");
    }
}