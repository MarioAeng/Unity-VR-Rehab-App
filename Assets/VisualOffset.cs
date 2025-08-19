using UnityEngine;

[DefaultExecutionOrder(10000)]
public class VisualOffset : MonoBehaviour
{
    public Transform root;                 // leave null to use this transform
    public Transform[] targets;            // your hand + controller visuals
    public Vector3 localPos = new Vector3(0f, -0.08f, 0f);
    public Vector3 localEuler = Vector3.zero;

    Vector3[] basePos;
    Quaternion[] baseRot;

    void Awake()
    {
        if (root == null) root = transform;
        if (targets == null) return;

        basePos = new Vector3[targets.Length];
        baseRot = new Quaternion[targets.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            var t = targets[i];
            if (!t) continue;
            basePos[i] = root.InverseTransformPoint(t.position);
            baseRot[i] = Quaternion.Inverse(root.rotation) * t.rotation;
        }
    }

    void LateUpdate()
    {
        if (targets == null) return;

        for (int i = 0; i < targets.Length; i++)
        {
            var t = targets[i];
            if (!t) continue;

            var posLocal = basePos[i] + localPos;
            t.position = root.TransformPoint(posLocal);
            t.rotation = root.rotation * Quaternion.Euler(localEuler) * baseRot[i];
        }
    }
}