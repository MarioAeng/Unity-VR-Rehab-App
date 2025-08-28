using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[DefaultExecutionOrder(-10000)]
[DisallowMultipleComponent]
public class CupHandSelect : MonoBehaviour
{
    public enum Hand { Right, Left }

    [Header("Mode")]
    public bool readFromPlayerPrefs = true;
    public Hand editorHand = Hand.Left;

    [Header("Roots")]
    public GameObject leftRoot;                // LeftHand
    public GameObject rightRoot;               // MainSelectorHand (right)

    [Header("Controllers/Interactors")]
    public ActionBasedController leftController;
    public ActionBasedController rightController;
    public XRRayInteractor leftRay;
    public XRRayInteractor rightRay;
    public XRInteractorLineVisual leftLine;
    public XRInteractorLineVisual rightLine;

    [Header("HandVisual / Ray Origin (tip)")]
    public Transform leftHandVisual;
    public Transform rightHandVisual;

    [Header("Input Actions")]
    public InputActionReference leftPos, leftRot, leftTrig;
    public InputActionReference rightPos, rightRot, rightTrig;

    [Header("Gameplay Scripts (optional)")]
    public MonoBehaviour manualCupRayHandler;     // your ManualCupRayHandler (single active)
    public MonoBehaviour menuSelector;            // SimulatedHandMenuSelector if present

    [Header("Scene Hygiene (optional)")]
    public bool killPersistentHijackers = true;
    public string[] hijackerTypeNames = new[] {
        "MainMenuReturnFixer",
        "HandReachExtender",
        "UnifiedRayInitializer",
        "ForceRayVisible"
    };

    [Header("Offsets")]
    public bool mirrorLeftRayOffsetX = true;      // auto-mirror X on handler.rayOffset for left

    void Awake()
    {
        if (killPersistentHijackers) KillByTypeNames(hijackerTypeNames);

        bool useLeft = readFromPlayerPrefs ? PlayerSettings.IsLeftHanded : (editorHand == Hand.Left);

        if (leftRoot)  leftRoot.SetActive(useLeft);
        if (rightRoot) rightRoot.SetActive(!useLeft);

        if (useLeft)
        {
            BindController(leftController, leftPos, leftRot, leftTrig);
            EnableActions(leftPos, leftRot, leftTrig);
            DisableActions(rightPos, rightRot, rightTrig);
            ToggleSet(true,  leftController, leftRay,  leftLine);
            ToggleSet(false, rightController, rightRay, rightLine);
        }
        else
        {
            BindController(rightController, rightPos, rightRot, rightTrig);
            EnableActions(rightPos, rightRot, rightTrig);
            DisableActions(leftPos,  leftRot,  leftTrig);
            ToggleSet(false, leftController, leftRay, leftLine);
            ToggleSet(true,  rightController, rightRay, rightLine);
        }

        var activeRay = useLeft ? leftRay : rightRay;
        var hv        = useLeft ? leftHandVisual : rightHandVisual;
        if (activeRay && hv)
        {
            activeRay.attachTransform = hv;
            activeRay.rayOriginTransform = hv;
        }

        RewireScript(manualCupRayHandler, useLeft, hv, activeRay);
        RewireScript(menuSelector,       useLeft, hv, activeRay);

        if (mirrorLeftRayOffsetX && useLeft && manualCupRayHandler) MirrorLeftOffsetX(manualCupRayHandler);

        // ✅ fixed log line (null-safe)
        Debug.Log($"[CupHandSelect] Using {(useLeft ? "LEFT" : "RIGHT")} | Ray={(activeRay ? activeRay.name : "(none)")} | HV={(hv ? hv.name : "(none)")}");
    }

    // ----- helpers -----

    void BindController(ActionBasedController ctrl, InputActionReference p, InputActionReference r, InputActionReference t)
    {
        if (!ctrl) return;
        ctrl.positionAction = new InputActionProperty(p);
        ctrl.rotationAction = new InputActionProperty(r);
        ctrl.selectAction   = new InputActionProperty(t);
        ctrl.enableInputTracking = true;
        ctrl.enableInputActions  = true;
        ctrl.enabled = false; ctrl.enabled = true;
    }

    void ToggleSet(bool on, ActionBasedController c, XRRayInteractor ray, XRInteractorLineVisual line)
    {
        if (c)   c.enabled   = on;
        if (ray) ray.enabled = on;
        if (line) line.enabled = on;
    }

    void EnableActions(params InputActionReference[] arr)  { foreach (var a in arr) if (a && a.action != null) a.action.Enable(); }
    void DisableActions(params InputActionReference[] arr) { foreach (var a in arr) if (a && a.action != null) a.action.Disable(); }

    void RewireScript(MonoBehaviour mb, bool useLeft, Transform hv, XRRayInteractor ray)
    {
        if (!mb) return;
        var t = mb.GetType();

        // Assign matching objects with type safety
        AssignField(t, mb, "handTransform", hv);                        // Transform
        AssignField(t, mb, "rayOrigin",     hv);                        // Transform
        AssignField(t, mb, "rayObject",     hv ? hv.gameObject : null); // GameObject
        AssignField(t, mb, "rayInteractor", ray);                       // XRRayInteractor

        // Trigger action property/reference
        var trgProp = t.GetField("triggerAction");
        if (trgProp != null && trgProp.FieldType == typeof(InputActionProperty))
            trgProp.SetValue(mb, new InputActionProperty(useLeft ? leftTrig : rightTrig));

        var trgRef = t.GetField("triggerActionRef");
        if (trgRef != null && trgRef.FieldType == typeof(InputActionReference))
            trgRef.SetValue(mb, useLeft ? leftTrig : rightTrig);

        // Optional: position/rotation properties if your script uses them
        var posProp = t.GetField("positionAction");
        if (posProp != null && posProp.FieldType == typeof(InputActionProperty))
            posProp.SetValue(mb, new InputActionProperty(useLeft ? leftPos : rightPos));

        var rotProp = t.GetField("rotationAction");
        if (rotProp != null && rotProp.FieldType == typeof(InputActionProperty))
            rotProp.SetValue(mb, new InputActionProperty(useLeft ? leftRot : rightRot));

        mb.enabled = false; mb.enabled = true;
    }

    // Only assign if the UnityEngine.Object types are compatible
    void AssignField(System.Type t, object obj, string name, Object value)
    {
        var f = t.GetField(name);
        if (f == null || value == null) return;
        if (f.FieldType.IsAssignableFrom(value.GetType()))
            f.SetValue(obj, value);
    }

    void MirrorLeftOffsetX(MonoBehaviour mb)
    {
        var f = mb.GetType().GetField("rayOffset");
        if (f != null && f.FieldType == typeof(Vector3))
        {
            var v = (Vector3)f.GetValue(mb);
            v.x = -Mathf.Abs(v.x);
            f.SetValue(mb, v);
        }
    }

    void KillByTypeNames(string[] names)
    {
        if (names == null || names.Length == 0) return;
        var all = Object.FindObjectsOfType<MonoBehaviour>(true);
        foreach (var mb in all)
        {
            if (!mb) continue;
            foreach (var n in names)
            {
                if (mb.GetType().Name == n)
                {
                    Debug.Log($"[CupHandSelect] Disabling {n} on {GetPath(mb.transform)}");
                    mb.enabled = false;
                    mb.gameObject.SetActive(false);
                }
            }
        }
    }

    static string GetPath(Transform tr)
    {
        string p = tr.name;
        while (tr && tr.parent) { tr = tr.parent; p = tr.name + "/" + p; }
        return p;
    }
}
