using UnityEngine;

[DisallowMultipleComponent]
public class HoverHighlight : MonoBehaviour
{
    [Header("Highlight Look")]
    public Color highlightColor = new Color(1f, 0.85f, 0.2f, 1f); // warm yellow
    public float emissionBoost = 1.5f; // set 0 to disable emission tweak

    Renderer[] renderers;
    Material[][] originalMats;    // keep originals so we can restore
    Material[][] instancedMats;   // per-object instances so we don’t edit shared

    bool isHighlighted;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        originalMats = new Material[renderers.Length][];
        instancedMats = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            originalMats[i] = r.sharedMaterials;

            // create instances to avoid changing shared project materials
            instancedMats[i] = new Material[originalMats[i].Length];
            for (int j = 0; j < originalMats[i].Length; j++)
            {
                if (originalMats[i][j] == null) continue;
                instancedMats[i][j] = new Material(originalMats[i][j]);
            }
            r.materials = instancedMats[i];
        }
    }

    public void SetHighlighted(bool on)
    {
        if (isHighlighted == on) return;
        isHighlighted = on;

        for (int i = 0; i < renderers.Length; i++)
        {
            var mats = renderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
            {
                var m = mats[j];
                if (!m) continue;

                // URP/Lit uses _BaseColor; Standard uses _Color — set both.
                var baseProp = m.HasProperty("_BaseColor") ? "_BaseColor" : (m.HasProperty("_Color") ? "_Color" : null);
                if (baseProp != null)
                {
                    if (on)
                    {
                        // tinted highlight
                        Color orig = originalMats[i][j] && originalMats[i][j].HasProperty(baseProp)
                            ? originalMats[i][j].GetColor(baseProp) : Color.white;
                        m.SetColor(baseProp, Color.Lerp(orig, highlightColor, 0.6f));
                    }
                    else
                    {
                        // restore original
                        if (originalMats[i][j] && originalMats[i][j].HasProperty(baseProp))
                            m.SetColor(baseProp, originalMats[i][j].GetColor(baseProp));
                    }
                }

                if (emissionBoost > 0 && m.HasProperty("_EmissionColor"))
                {
                    if (on)
                    {
                        m.EnableKeyword("_EMISSION");
                        var curr = m.GetColor("_EmissionColor");
                        m.SetColor("_EmissionColor", curr + highlightColor * emissionBoost);
                    }
                    else
                    {
                        // revert to original emission if present
                        if (originalMats[i][j] && originalMats[i][j].HasProperty("_EmissionColor"))
                            m.SetColor("_EmissionColor", originalMats[i][j].GetColor("_EmissionColor"));
                        else
                            m.SetColor("_EmissionColor", Color.black);
                    }
                }
            }
        }
    }

    void OnDisable() { SetHighlighted(false); }
}
