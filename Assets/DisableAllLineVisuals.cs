using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DisableAllLineVisuals : MonoBehaviour
{
    void Awake()
    {
        foreach (var v in FindObjectsOfType<XRInteractorLineVisual>(true))
        {
            v.enabled = false;
            var lr = v.GetComponent<LineRenderer>();
            if (lr) lr.enabled = false;
            Debug.Log($"[DisableAllLineVisuals] Disabled on {v.gameObject.name}");
        }
    }
}