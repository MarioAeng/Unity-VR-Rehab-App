using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FollowRayOrigin : MonoBehaviour
{
    [Header("Optional: assign if you know the ray")]
    public XRRayInteractor ray;              // leave null to auto-pick the active one

    [Header("Pose offset (local to the ray origin)")]
    public Vector3 positionOffset = Vector3.zero; // e.g. (0, -0.02f, -0.08f)
    public Vector3 eulerOffset = Vector3.zero;    // e.g. (0, 180, 0) if your model faces -Z

    [Range(0.01f, 1f)] public float followLerp = 1f; // 1 = snap, <1 = smooth

    Transform _rayOrigin;

    void OnEnable() => ResolveRay();

    void Update()
    {
        if (_rayOrigin == null) ResolveRay();
        if (_rayOrigin == null) return;

        Vector3 targetPos = _rayOrigin.TransformPoint(positionOffset);
        Quaternion targetRot = _rayOrigin.rotation * Quaternion.Euler(eulerOffset);

        if (followLerp >= 1f)
            transform.SetPositionAndRotation(targetPos, targetRot);
        else {
            float k = followLerp * 20f * Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, targetPos, k);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, k);
        }
    }

    void ResolveRay()
    {
        _rayOrigin = null;

        var chosen = ray;
        if (chosen == null)
        {
            foreach (var r in FindObjectsOfType<XRRayInteractor>(true))
            {
                if (!r.isActiveAndEnabled) continue;

                // Prefer ones actually drawing a line
                var vis = r.GetComponent<XRInteractorLineVisual>();
                var lr  = r.GetComponent<LineRenderer>();
                bool visible = (vis && vis.enabled && vis.gameObject.activeInHierarchy) ||
                               (lr && lr.enabled && lr.positionCount >= 2 && lr.gameObject.activeInHierarchy);
                if (visible) { chosen = r; break; }
            }
        }

        if (chosen != null)
        {
            ray = chosen;
            _rayOrigin = ray.rayOriginTransform != null ? ray.rayOriginTransform : ray.transform;
            Debug.Log($"[FollowRayOrigin] Following ray on '{ray.gameObject.name}', origin '{_rayOrigin.name}'.");
        }
        else
        {
            Debug.LogWarning("[FollowRayOrigin] No active XRRayInteractor with a visible line found.");
        }
    }
}
