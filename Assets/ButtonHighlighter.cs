using UnityEngine;
using UnityEngine.UI;

public class ButtonHighlighter : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Transform simulatedHand; // Match this with ManualButtonHighlighter’s GameObject
    public float highlightDistance = 1.6f;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.green;

    private Image image;

    void Start()
    {
        image = GetComponent<Image>();

        if (image == null)
            Debug.LogWarning($"[ButtonHighlighter] Missing Image component on {gameObject.name}");
    }

    void Update()
    {
        if (simulatedHand == null || image == null)
            return;

        float dist = Vector3.Distance(transform.position, simulatedHand.position);
        bool highlight = dist <= highlightDistance;

        image.color = highlight ? highlightColor : normalColor;

        if (highlight)
        {
            Debug.Log($"[ButtonHighlighter] Highlighting {gameObject.name} (dist = {dist:F2})");
        }
    }
}