using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class HandCupProximityGrabAndGrip : MonoBehaviour
{
    [Header("Input (same action names you already use)")]
    public InputActionAsset inputActions;
    public string rightTriggerActionName = "RightTriggerAction";
    public string leftTriggerActionName  = "LeftTriggerAction";
    public bool useLeftHand = false;

    [Header("Grabbable Filter")]
    public LayerMask grabbableMask;     // include the Cup layer
    public float maxGrabDistance = 0.20f;

    [Header("Hold/Attach")]
    public Transform holdAnchor;        // where the cup sits in the hand (e.g., palm)
    public float followSmoothing = 25f; // move/rotate speed while held

    [Header("Highlight")]
    public Color hoverHighlight = new Color(1f, 0.85f, 0.2f, 1f);
    public float emissionBoost = 1.2f;

    [Header("Grip Animation Settings")]
    public float maxCurlAngle = -60f;   // ← matches your inspector
    public bool rotateOnX = false;
    public bool rotateOnY = false;
    public bool rotateOnZ = true;       // ← matches your inspector
    public float gripSpeed = 12f;       // how quickly the hand closes/opens

    [Header("Finger Joints (auto-filled by name)")]
    public Transform thumbBase;
    public Transform thumbTip;
    public Transform indexBase, indexMid, indexTip;
    public Transform middleBase, middleMid, middleTip;
    public Transform ringBase, ringMid, ringTip;
    public Transform pinkyBase, pinkyMid, pinkyTip;

    // ----- internals -----
    InputAction trigger;
    readonly List<Rigidbody> inRange = new();
    Rigidbody held;
    float gripT = 0f;

    // grip caching
    struct Bone
    {
        public Transform t;
        public Quaternion start;
    }
    Bone[] bones;

    // highlight caching
    Renderer[] rends;
    Material[][] sharedMats;
    Material[][] instancedMats;
    bool highlighted;

    // ----- lifecycle -----
    void Awake()
    {
        // collider must be trigger for proximity
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        AutoWireBonesIfNeeded();
        CacheBones();
        CacheHighlightMaterials();
    }

    void OnEnable()
    {
        string action = useLeftHand ? leftTriggerActionName : rightTriggerActionName;
        trigger = inputActions ? inputActions.FindAction(action) : null;
        trigger?.Enable();
    }

    void OnDisable()
    {
        trigger?.Disable();
        SetHighlight(false);
        Release();
    }

    void Update()
    {
        // highlight / nearest cup
        Rigidbody nearest = FindNearestInRange();

        bool pressed = trigger != null && trigger.ReadValue<float>() > 0.5f;
        bool canGrab = nearest && Vector3.Distance(GetAnchor().position, nearest.worldCenterOfMass) <= maxGrabDistance;

        // hover highlight if can grab and not holding
        SetHighlight(!held && canGrab);

        // grab / release
        if (pressed && held == null && canGrab) Grab(nearest);
        if (!pressed && held != null) Release();

        // animate grip target
        float targetGrip = (pressed && canGrab) || held != null ? 1f : 0f;
        gripT = Mathf.MoveTowards(gripT, targetGrip, Time.deltaTime * gripSpeed);
        ApplyGrip(gripT);

        // keep held object aligned
        if (held != null)
        {
            Transform a = GetAnchor();
            held.MovePosition(Vector3.Lerp(held.position, a.position, Time.deltaTime * followSmoothing));
            held.MoveRotation(Quaternion.Slerp(held.rotation, a.rotation, Time.deltaTime * followSmoothing));
        }
    }

    // ----- grabbing -----
    void Grab(Rigidbody rb)
    {
        held = rb;

        // prefer a GrabPoint child if present
        Transform snap = rb.transform.Find("GrabPoint") ? rb.transform.Find("GrabPoint") : rb.transform;

        // parent to hand for consistent visuals (optional)
        rb.isKinematic = true;
        snap.SetParent(GetAnchor(), worldPositionStays: false);
        snap.localPosition = Vector3.zero;
        snap.localRotation = Quaternion.identity;
    }

    void Release()
    {
        if (held == null) return;

        // unparent snap point if we parented it
        var snap = held.transform.Find("GrabPoint");
        if (snap && snap.parent == GetAnchor()) snap.SetParent(held.transform, true);
        else if (held.transform.parent == GetAnchor()) held.transform.SetParent(null, true);

        // restore physics
        held.isKinematic = false;
        held = null;
    }

    Transform GetAnchor() => holdAnchor ? holdAnchor : transform;

    Rigidbody FindNearestInRange()
    {
        // purge nulls
        for (int i = inRange.Count - 1; i >= 0; i--)
            if (inRange[i] == null) inRange.RemoveAt(i);

        if (inRange.Count == 0) return null;

        Vector3 p = GetAnchor().position;
        float best = float.MaxValue;
        Rigidbody bestRb = null;

        foreach (var rb in inRange)
        {
            if (((1 << rb.gameObject.layer) & grabbableMask) == 0) continue;
            float d = Vector3.Distance(p, rb.worldCenterOfMass);
            if (d < best) { best = d; bestRb = rb; }
        }

        return bestRb;
    }

    void OnTriggerEnter(Collider other)
    {
        var rb = other.attachedRigidbody;
        if (rb != null && ((1 << rb.gameObject.layer) & grabbableMask) != 0)
            if (!inRange.Contains(rb)) inRange.Add(rb);
    }

    void OnTriggerExit(Collider other)
    {
        var rb = other.attachedRigidbody;
        if (rb != null) inRange.Remove(rb);
    }

    // ----- grip animation -----
    void CacheBones()
    {
        var list = new List<Bone>();
        void add(Transform t) { if (t) list.Add(new Bone { t = t, start = t.localRotation }); }

        add(thumbBase); add(thumbTip);
        add(indexBase); add(indexMid); add(indexTip);
        add(middleBase); add(middleMid); add(middleTip);
        add(ringBase); add(ringMid); add(ringTip);
        add(pinkyBase); add(pinkyMid); add(pinkyTip);

        bones = list.ToArray();
    }

    void ApplyGrip(float t)
    {
        // rotate each cached bone from its start by (t * maxCurlAngle) on chosen axes
        Vector3 axis = new Vector3(rotateOnX ? maxCurlAngle : 0f,
                                   rotateOnY ? maxCurlAngle : 0f,
                                   rotateOnZ ? maxCurlAngle : 0f);

        foreach (var b in bones)
        {
            if (!b.t) continue;
            var rot = Quaternion.Euler(axis * t);
            b.t.localRotation = b.start * rot;
        }
    }

    // ----- highlighting -----
    void CacheHighlightMaterials()
    {
        rends = GetComponentsInChildren<Renderer>(true);
        sharedMats = new Material[rends.Length][];
        instancedMats = new Material[rends.Length][];

        for (int i = 0; i < rends.Length; i++)
        {
            var r = rends[i];
            sharedMats[i] = r.sharedMaterials;
            instancedMats[i] = new Material[sharedMats[i].Length];

            for (int j = 0; j < sharedMats[i].Length; j++)
            {
                var src = sharedMats[i][j];
                if (src == null) continue;
                instancedMats[i][j] = new Material(src);
            }
            r.materials = instancedMats[i];
        }
    }

    void SetHighlight(bool on)
    {
        if (highlighted == on || instancedMats == null) return;
        highlighted = on;

        for (int i = 0; i < instancedMats.Length; i++)
        {
            for (int j = 0; j < instancedMats[i].Length; j++)
            {
                var m = instancedMats[i][j];
                if (!m) continue;

                string colorProp = m.HasProperty("_BaseColor") ? "_BaseColor" :
                                   (m.HasProperty("_Color") ? "_Color" : null);

                if (colorProp != null)
                {
                    if (on)
                    {
                        Color baseCol = sharedMats[i][j] && sharedMats[i][j].HasProperty(colorProp)
                            ? sharedMats[i][j].GetColor(colorProp) : Color.white;
                        m.SetColor(colorProp, Color.Lerp(baseCol, hoverHighlight, 0.6f));
                    }
                    else
                    {
                        if (sharedMats[i][j] && sharedMats[i][j].HasProperty(colorProp))
                            m.SetColor(colorProp, sharedMats[i][j].GetColor(colorProp));
                    }
                }

                if (m.HasProperty("_EmissionColor"))
                {
                    if (on)
                    {
                        m.EnableKeyword("_EMISSION");
                        var curr = sharedMats[i][j] && sharedMats[i][j].HasProperty("_EmissionColor")
                            ? sharedMats[i][j].GetColor("_EmissionColor") : Color.black;
                        m.SetColor("_EmissionColor", curr + hoverHighlight * emissionBoost);
                    }
                    else
                    {
                        var orig = sharedMats[i][j] && sharedMats[i][j].HasProperty("_EmissionColor")
                            ? sharedMats[i][j].GetColor("_EmissionColor") : Color.black;
                        m.SetColor("_EmissionColor", orig);
                    }
                }
            }
        }
    }

    // ----- auto-wiring by bone names so you don’t have to drag them -----
    [ContextMenu("Auto-Wire Finger Bones (by name)")]
    public void AutoWireBonesIfNeeded()
    {
        // Expected names from your screenshot:
        thumbBase  = thumbBase  ? thumbBase  : FindDeep("b_r_thumb1");
        thumbTip   = thumbTip   ? thumbTip   : FindDeep("b_r_thumb3");

        indexBase  = indexBase  ? indexBase  : FindDeep("b_r_index1");
        indexMid   = indexMid   ? indexMid   : FindDeep("b_r_index2");
        indexTip   = indexTip   ? indexTip   : FindDeep("b_r_index3");

        middleBase = middleBase ? middleBase : FindDeep("b_r_middle1");
        middleMid  = middleMid  ? middleMid  : FindDeep("b_r_middle2");
        middleTip  = middleTip  ? middleTip  : FindDeep("b_r_middle3");

        ringBase   = ringBase   ? ringBase   : FindDeep("b_r_ring1");
        ringMid    = ringMid    ? ringMid    : FindDeep("b_r_ring2");
        ringTip    = ringTip    ? ringTip    : FindDeep("b_r_ring3");

        pinkyBase  = pinkyBase  ? pinkyBase  : FindDeep("b_r_pinky1");
        pinkyMid   = pinkyMid   ? pinkyMid   : FindDeep("b_r_pinky2");
        pinkyTip   = pinkyTip   ? pinkyTip   : FindDeep("b_r_pinky3");
    }

    Transform FindDeep(string name)
    {
        var q = new Queue<Transform>();
        q.Enqueue(transform);
        while (q.Count > 0)
        {
            var t = q.Dequeue();
            if (t.name == name) return t;
            for (int i = 0; i < t.childCount; i++) q.Enqueue(t.GetChild(i));
        }
        return null;
    }
}
