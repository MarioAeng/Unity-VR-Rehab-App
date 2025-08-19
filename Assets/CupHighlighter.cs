using UnityEngine;

public class CupHighlighter : MonoBehaviour
{
    public Material normalMaterial;
    public Material highlightMaterial;
    private MeshRenderer meshRenderer;
    private bool isHighlighted = false;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            Debug.LogError("[CupHighlighter] No MeshRenderer found on object!");

        if (normalMaterial != null)
            meshRenderer.material = normalMaterial;
    }

    void Update()
    {
        // Log current material name every frame
        if (meshRenderer != null && meshRenderer.material != null)
        {
            Debug.Log($"[CupHighlighter] Current Material: {meshRenderer.material.name} | Highlighted: {isHighlighted}");
        }

        // TEMP: Toggle highlight with space bar for testing (remove later in VR)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleHighlight(true);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            ToggleHighlight(false);
        }
    }

    public void ToggleHighlight(bool highlight)
    {
        if (meshRenderer == null) return;

        isHighlighted = highlight;
        meshRenderer.material = highlight ? highlightMaterial : normalMaterial;

        Debug.Log($"[CupHighlighter] Material changed to: {meshRenderer.material.name}");
    }
}