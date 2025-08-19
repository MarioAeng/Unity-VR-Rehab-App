using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class HandGrabDebugger : MonoBehaviour
{
    [Header("Input (optional, for test grab)")]
    public InputActionAsset inputActions;
    public string rightTriggerActionName = "RightTriggerAction";
    public string leftTriggerActionName  = "LeftTriggerAction";
    public bool useLeftHand = false;

    [Header("What can we grab?")]
    public LayerMask grabbableMask;     // include Cup layer (and/or UI if you're using it)
    public float maxGrabDistance = 0.25f;

    [Header("Where to hold from on the hand")]
    public Transform holdAnchor;        // use your HoldPoint here

    [Header("Behavior")]
    public bool performGrab = false;    // set true if you want to actually attach/detach for testing
    public float followSmoothing = 25f; // only used if performGrab = true

    [Header("Logging")]
    public string logTag = "[HandGrabDebugger]";
    public float logInterval = 0.25f;   // seconds between status logs
    public bool logOverlapSphereHits = true;
    public bool logTriggerEvents = true;

    private readonly List<Rigidbody> triggerRBs = new();
    private InputAction trigger;
    private Rigidbody held;
    private float nextLog;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true; // this script assumes trigger range
    }

    void OnEnable()
    {
        if (!holdAnchor) holdAnchor = transform;

        string action = useLeftHand ? leftTriggerActionName : rightTriggerActionName;
        trigger = inputActions ? inputActions.FindAction(action) : null;
        trigger?.Enable();

        Debug.Log($"{logTag} Enabled. LayerMask:{grabbableMask.value} maxGrab:{maxGrabDistance:F2} anchor:{holdAnchor.name}");
    }

    void OnDisable()
    {
        trigger?.Disable();
        if (held) Release("OnDisable");
        Debug.Log($"{logTag} Disabled.");
    }

    void Update()
    {
        // -- Input state (optional) --
        float trigVal = trigger != null ? trigger.ReadValue<float>() : 0f;
        bool pressed = trigVal > 0.5f;

        // -- Find nearest candidate --
        Vector3 anchorPos = holdAnchor ? holdAnchor.position : transform.position;

        // Physics query (doesn't require triggers to work)
        var cols = Physics.OverlapSphere(anchorPos, maxGrabDistance, grabbableMask, QueryTriggerInteraction.Ignore);

        if (logOverlapSphereHits && Time.time >= nextLog)
        {
            Debug.Log($"{logTag} OverlapSphere hits:{cols.Length} at {anchorPos} r={maxGrabDistance:F2}");
        }

        Rigidbody nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var c in cols)
        {
            var rb = c.attachedRigidbody;
            if (!rb) continue;
            float d = Vector3.Distance(rb.worldCenterOfMass, anchorPos);
            if (d < nearestDist) { nearest = rb; nearestDist = d; }
        }

        // Fallback: anything inside our trigger list
        for (int i = triggerRBs.Count - 1; i >= 0; i--)
            if (triggerRBs[i] == null) triggerRBs.RemoveAt(i);

        foreach (var rb in triggerRBs)
        {
            if (((1 << rb.gameObject.layer) & grabbableMask) == 0) continue;
            float d = Vector3.Distance(rb.worldCenterOfMass, anchorPos);
            if (d < nearestDist) { nearest = rb; nearestDist = d; }
        }

        bool inRange = nearest && nearestDist <= maxGrabDistance;

        // Status log (throttled)
        if (Time.time >= nextLog)
        {
            string nearestName = nearest ? nearest.name : "none";
            Debug.Log($"{logTag} pressed:{pressed} held:{(held?held.name:"null")} nearest:{nearestName} dist:{(nearest?nearestDist.ToString("F3"):"--")} inRange:{inRange}");
            nextLog = Time.time + logInterval;
        }

        // Optional grab/release to verify full pipeline
        if (!performGrab) return;

        if (pressed && held == null && inRange)
        {
            Grab(nearest, "pressed+inRange");
        }
        else if (!pressed && held != null)
        {
            Release("released");
        }

        if (held != null)
        {
            Transform a = holdAnchor ? holdAnchor : transform;
            held.MovePosition(Vector3.Lerp(held.position, a.position, Time.deltaTime * followSmoothing));
            held.MoveRotation(Quaternion.Slerp(held.rotation, a.rotation, Time.deltaTime * followSmoothing));
        }
    }

    private void Grab(Rigidbody rb, string reason)
    {
        held = rb;
        Debug.Log($"{logTag} GRAB {rb.name} reason:{reason}");
        // Make kinematic while held so it follows cleanly
        held.isKinematic = true;

        // If cup has a GrabPoint, snap that to anchor; else snap root
        Transform anchor = holdAnchor ? holdAnchor : transform;
        Transform snap = rb.transform.Find("GrabPoint") ? rb.transform.Find("GrabPoint") : rb.transform;
        snap.SetParent(anchor, worldPositionStays: false);
        snap.localPosition = Vector3.zero;
        snap.localRotation = Quaternion.identity;
    }

    private void Release(string reason)
    {
        Debug.Log($"{logTag} RELEASE {held.name} reason:{reason}");
        // Unparent any snap we parented
        var snap = held.transform.Find("GrabPoint");
        if (snap && snap.parent == holdAnchor) snap.SetParent(held.transform, true);
        else if (held.transform.parent == holdAnchor) held.transform.SetParent(null, true);

        held.isKinematic = false;
        held = null;
    }

    // ---- Trigger diagnostics (optional, requires hand collider set as IsTrigger) ----
    void OnTriggerEnter(Collider other)
    {
        var rb = other.attachedRigidbody;
        if (!rb) return;
        if (((1 << other.gameObject.layer) & grabbableMask) == 0) return;

        if (!triggerRBs.Contains(rb)) triggerRBs.Add(rb);
        if (logTriggerEvents) Debug.Log($"{logTag} TriggerEnter -> {rb.name} (layer {LayerMask.LayerToName(other.gameObject.layer)})");
    }

    void OnTriggerExit(Collider other)
    {
        var rb = other.attachedRigidbody;
        if (!rb) return;

        triggerRBs.Remove(rb);
        if (logTriggerEvents) Debug.Log($"{logTag} TriggerExit  -> {rb.name}");
    }

    void OnDrawGizmosSelected()
    {
        if (!holdAnchor) return;
        Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.25f);
        Gizmos.DrawSphere(holdAnchor.position, Mathf.Max(maxGrabDistance, 0.01f));
    }
}
