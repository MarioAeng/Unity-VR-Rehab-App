using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class CupHandInit : MonoBehaviour
{
    [Header("Hand Roots")]
    public GameObject leftHandRoot;
    public GameObject rightHandRoot;

    [Header("InputActionReferences (Left)")]
    public InputActionReference leftPosition;
    public InputActionReference leftRotation;
    public InputActionReference leftTrigger;

    [Header("InputActionReferences (Right)")]
    public InputActionReference rightPosition;
    public InputActionReference rightRotation;
    public InputActionReference rightTrigger;

    [Header("Optional (Shared Ray Handler)")]
    public MonoBehaviour sharedRayHandler; // e.g., ManualCupRayHandler (single script used in scene)
    public Transform leftRayOrigin;        // optional, if your handler exposes a 'rayOrigin' Transform
    public Transform rightRayOrigin;

    private Vector3 _leftStartPos, _rightStartPos;
    private Quaternion _leftStartRot, _rightStartRot;

    void Awake()
    {
        if (leftHandRoot)
        {
            _leftStartPos = leftHandRoot.transform.position;
            _leftStartRot = leftHandRoot.transform.rotation;
        }
        if (rightHandRoot)
        {
            _rightStartPos = rightHandRoot.transform.position;
            _rightStartRot = rightHandRoot.transform.rotation;
        }
    }

    void Start()
    {
        // Uses your existing flag that other scenes set
        bool useLeft = PlayerSettings.IsLeftHanded;
        Debug.Log($"[CupHandInit] Handedness: {(useLeft ? "LEFT" : "RIGHT")}");

        // Reset transforms so returns to menu don’t leave offset hands
        ResetTransforms();

        // Show only the active hand
        if (leftHandRoot)  leftHandRoot.SetActive(useLeft);
        if (rightHandRoot) rightHandRoot.SetActive(!useLeft);

        // Rebind inputs for the active hand
        if (useLeft) ApplyFor(leftHandRoot, true);
        else         ApplyFor(rightHandRoot, false);

        // Make sure the inactive hand cannot steal input
        if (useLeft) DisableControllers(rightHandRoot);
        else         DisableControllers(leftHandRoot);
    }

    void ResetTransforms()
    {
        if (leftHandRoot)
        {
            leftHandRoot.transform.position = _leftStartPos;
            leftHandRoot.transform.rotation = _leftStartRot;
        }
        if (rightHandRoot)
        {
            rightHandRoot.transform.position = _rightStartPos;
            rightHandRoot.transform.rotation = _rightStartRot;
        }
    }

    void ApplyFor(GameObject activeRoot, bool isLeft)
    {
        if (!activeRoot)
        {
            Debug.LogWarning("[CupHandInit] Active hand root not assigned.");
            return;
        }

        // 1) Rebind Action-Based Controller (position/rotation/select)
        RebindActionBasedController(activeRoot, isLeft);

        // 2) Rebind common custom handler fields if present on components under active root
        RebindCommonHandlerFields(activeRoot, isLeft);

        // 3) Rebind a single shared handler if you use one
        if (sharedRayHandler) RebindSharedHandler(sharedRayHandler, isLeft);

        // 4) Refresh ray and line visual once after rebind
        RefreshRayVisuals(activeRoot);

        // 5) Debug final bindings
        LogEffective(isLeft);
    }

    void RebindActionBasedController(GameObject root, bool isLeft)
    {
        var ctrl = root.GetComponentInChildren<ActionBasedController>(true);
        if (ctrl == null) return;

        var P = new InputActionProperty(isLeft ? leftPosition : rightPosition);
        var R = new InputActionProperty(isLeft ? leftRotation : rightRotation);
        var T = new InputActionProperty(isLeft ? leftTrigger  : rightTrigger);

        ctrl.positionAction = P;
        ctrl.rotationAction = R;
        ctrl.selectAction   = T;
        ctrl.enableInputTracking = true;
        ctrl.enableInputActions  = true;

        ctrl.enabled = false; // force-apply
        ctrl.enabled = true;

        Debug.Log($"[CupHandInit] Rebound ActionBasedController on {root.name} to {(isLeft ? "LEFT" : "RIGHT")} actions.");
    }

    void RebindCommonHandlerFields(GameObject root, bool isLeft)
    {
        // Look for fields named exactly: positionAction, rotationAction, triggerAction
        // Accepts either InputActionProperty or InputActionReference
        var comps = root.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var c in comps)
        {
            if (c == null) continue;
            var t = c.GetType();

            var posProp = t.GetField("positionAction");
            var rotProp = t.GetField("rotationAction");
            var trgProp = t.GetField("triggerAction");

            var posRef = t.GetField("positionActionRef");
            var rotRef = t.GetField("rotationActionRef");
            var trgRef = t.GetField("triggerActionRef");

            // Optional ray origin
            var rayOriginField = t.GetField("rayOrigin");
            if (rayOriginField != null)
            {
                rayOriginField.SetValue(c, isLeft ? leftRayOrigin : rightRayOrigin);
            }

            bool touched = false;

            if (posProp != null && rotProp != null && trgProp != null &&
                posProp.FieldType == typeof(InputActionProperty) &&
                rotProp.FieldType == typeof(InputActionProperty) &&
                trgProp.FieldType == typeof(InputActionProperty))
            {
                var P = new InputActionProperty(isLeft ? leftPosition : rightPosition);
                var R = new InputActionProperty(isLeft ? leftRotation : rightRotation);
                var T = new InputActionProperty(isLeft ? leftTrigger  : rightTrigger);

                posProp.SetValue(c, P);
                rotProp.SetValue(c, R);
                trgProp.SetValue(c, T);
                touched = true;
            }
            else if (posRef != null && rotRef != null && trgRef != null &&
                     posRef.FieldType == typeof(InputActionReference) &&
                     rotRef.FieldType == typeof(InputActionReference) &&
                     trgRef.FieldType == typeof(InputActionReference))
            {
                posRef.SetValue(c, isLeft ? leftPosition : rightPosition);
                rotRef.SetValue(c, isLeft ? leftRotation : rightRotation);
                trgRef.SetValue(c, isLeft ? leftTrigger  : rightTrigger);
                touched = true;
            }

            if (touched)
            {
                c.enabled = false; // re-apply
                c.enabled = true;
                Debug.Log($"[CupHandInit] Rebound {t.Name} on {root.name}");
            }
        }
    }

    void RebindSharedHandler(MonoBehaviour handler, bool isLeft)
    {
        var t = handler.GetType();

        var posProp = t.GetField("positionAction");
        var rotProp = t.GetField("rotationAction");
        var trgProp = t.GetField("triggerAction");

        var posRef = t.GetField("positionActionRef");
        var rotRef = t.GetField("rotationActionRef");
        var trgRef = t.GetField("triggerActionRef");

        var rayOriginField = t.GetField("rayOrigin");
        if (rayOriginField != null)
            rayOriginField.SetValue(handler, isLeft ? leftRayOrigin : rightRayOrigin);

        bool rebound = false;

        if (posProp != null && rotProp != null && trgProp != null &&
            posProp.FieldType == typeof(InputActionProperty) &&
            rotProp.FieldType == typeof(InputActionProperty) &&
            trgProp.FieldType == typeof(InputActionProperty))
        {
            posProp.SetValue(handler, new InputActionProperty(isLeft ? leftPosition : rightPosition));
            rotProp.SetValue(handler, new InputActionProperty(isLeft ? leftRotation : rightRotation));
            trgProp.SetValue(handler, new InputActionProperty(isLeft ? leftTrigger  : rightTrigger));
            rebound = true;
        }
        else if (posRef != null && rotRef != null && trgRef != null &&
                 posRef.FieldType == typeof(InputActionReference) &&
                 rotRef.FieldType == typeof(InputActionReference) &&
                 trgRef.FieldType == typeof(InputActionReference))
        {
            posRef.SetValue(handler, isLeft ? leftPosition : rightPosition);
            rotRef.SetValue(handler, isLeft ? leftRotation : rightRotation);
            trgRef.SetValue(handler, isLeft ? leftTrigger  : rightTrigger);
            rebound = true;
        }

        if (rebound)
        {
            handler.enabled = false;
            handler.enabled = true;
            Debug.Log($"[CupHandInit] Rebound shared handler {t.Name} to {(isLeft ? "LEFT" : "RIGHT")}.");
        }
        else
        {
            Debug.Log("[CupHandInit] Shared handler present but no recognized fields to rebind (position/rotation/trigger).");
        }
    }

    void DisableControllers(GameObject root)
    {
        if (!root) return;

        var ctrl = root.GetComponentInChildren<ActionBasedController>(true);
        if (ctrl) ctrl.enabled = false;

        var ray = root.GetComponentInChildren<XRRayInteractor>(true);
        if (ray) ray.enabled = false;

        var line = root.GetComponentInChildren<XRInteractorLineVisual>(true);
        if (line) line.enabled = false;
    }

    void RefreshRayVisuals(GameObject root)
    {
        if (!root) return;

        var ray = root.GetComponentInChildren<XRRayInteractor>(true);
        if (ray) { ray.enabled = false; ray.enabled = true; }

        var line = root.GetComponentInChildren<XRInteractorLineVisual>(true);
        if (line) { line.enabled = false; line.enabled = true; }
    }

    void LogEffective(bool left)
    {
        var p = (left ? leftPosition : rightPosition);
        var r = (left ? leftRotation : rightRotation);
        var t = (left ? leftTrigger  : rightTrigger);

        string pos = SafePath(p);
        string rot = SafePath(r);
        string trg = SafePath(t);

        Debug.Log($"[CupHandInit] Active={(left ? "LEFT" : "RIGHT")}\n Position: {pos}\n Rotation: {rot}\n Trigger:  {trg}");

        if (left && (pos.Contains("{RightHand}") || rot.Contains("{RightHand}") || trg.Contains("{RightHand}")))
            Debug.LogWarning("[CupHandInit] Left selected but a binding still targets {RightHand}.");
        if (!left && (pos.Contains("{LeftHand}") || rot.Contains("{LeftHand}") || trg.Contains("{LeftHand}")))
            Debug.LogWarning("[CupHandInit] Right selected but a binding still targets {LeftHand}.");
    }

    string SafePath(InputActionReference r)
    {
        if (r == null || r.action == null) return "(null)";
        if (r.action.bindings.Count == 0) return "(no bindings)";
        return r.action.bindings[0].effectivePath ?? r.action.bindings[0].path;
    }
}
