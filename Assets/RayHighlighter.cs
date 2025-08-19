using UnityEngine;

public class RayHighlighter : MonoBehaviour
{
    [Header("Ray settings")]
    public LayerMask interactableMask;   // set to Cup/Target layers
    public float maxDistance = 10f;

    [Header("Optional: line color swap")]
    public LineRenderer line;
    public Color normalLineColor = Color.white;
    public Color hoverLineColor = Color.yellow;

    HoverHighlight current;

    void Update()
    {
        Vector3 origin = transform.position;
        Vector3 dir = transform.forward;

        if (line)
        {
            line.SetPosition(0, origin);
            line.SetPosition(1, origin + dir * maxDistance);
        }

        if (Physics.Raycast(origin, dir, out var hit, maxDistance, interactableMask))
        {
            var h = hit.collider.GetComponentInParent<HoverHighlight>();
            if (h != current)
            {
                if (current) current.SetHighlighted(false);
                current = h;
                if (current) current.SetHighlighted(true);
            }

            if (line) SetLineColor(hoverLineColor);
        }
        else
        {
            if (current) { current.SetHighlighted(false); current = null; }
            if (line) SetLineColor(normalLineColor);
        }
    }

    void SetLineColor(Color c)
    {
        if (!line) return;
        if (line.colorGradient != null)
        {
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
                new[] { new GradientAlphaKey(c.a, 0f), new GradientAlphaKey(c.a, 1f) }
            );
            line.colorGradient = g;
        }
        else
        {
            line.startColor = c; line.endColor = c;
        }
    }

    void OnDisable()
    {
        if (current) current.SetHighlighted(false);
        current = null;
        if (line) SetLineColor(normalLineColor);
    }
}