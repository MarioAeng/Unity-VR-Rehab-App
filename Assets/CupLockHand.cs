using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[DefaultExecutionOrder(-10000)] // run before others
public class CupLockHand : MonoBehaviour
{
    [Header("Roots")]
    public GameObject leftRoot;
    public GameObject rightRoot;

    [Header("Ray")]
    public XRRayInteractor ray;        // the interactor you use in gameplay
    public Transform leftRayOrigin;    // tip/hand visual on left
    public Transform rightRayOrigin;   // tip/hand visual on right

    [Header("Input Action Refs")]
    public InputActionReference leftPos, leftRot, leftTrig;
    public InputActionReference rightPos, rightRot, rightTrig;

    void Awake()
    {
        bool useLeft = PlayerSettings.IsLeftHanded;

        // Force correct binding paths for the active hand
        if (useLeft)
        {
            Force(leftPos, "<XRController>{LeftHand}/devicePosition");
            Force(leftRot, "<XRController>{LeftHand}/deviceRotation");
            Force(leftTrig,"<XRController>{LeftHand}/trigger");
            Enable(leftPos, leftRot, leftTrig);
            Disable(rightPos, rightRot, rightTrig);
        }
        else
        {
            Force(rightPos,"<XRController>{RightHand}/devicePosition");
            Force(rightRot,"<XRController>{RightHand}/deviceRotation");
            Force(rightTrig,"<XRController>{RightHand}/trigger");
            Enable(rightPos, rightRot, rightTrig);
            Disable(leftPos, leftRot, leftTrig);
        }

        // Point the ray at the correct hand tip
        if (ray)
        {
            var src = useLeft ? leftRayOrigin : rightRayOrigin;
            if (src) { ray.attachTransform = src; ray.rayOriginTransform = src; }
        }

        // Only keep controllers/rays under the active root enabled
        ToggleSceneComponents(useLeft);

        // Finally, hide the inactive hand
        if (leftRoot)  leftRoot.SetActive(useLeft);
        if (rightRoot) rightRoot.SetActive(!useLeft);
    }

    void ToggleSceneComponents(bool useLeft)
    {
        var ctrls = FindObjectsOfType<ActionBasedController>(true);
        foreach (var c in ctrls)
        {
            bool underLeft  = leftRoot  && c.transform.IsChildOf(leftRoot.transform);
            bool underRight = rightRoot && c.transform.IsChildOf(rightRoot.transform);
            c.enabled = useLeft ? underLeft : underRight;
        }
        var rays = FindObjectsOfType<XRRayInteractor>(true);
        foreach (var r in rays)
        {
            bool underLeft  = leftRoot  && r.transform.IsChildOf(leftRoot.transform);
            bool underRight = rightRoot && r.transform.IsChildOf(rightRoot.transform);
            r.enabled = useLeft ? underLeft : underRight;
        }
        var lines = FindObjectsOfType<XRInteractorLineVisual>(true);
        foreach (var l in lines)
        {
            bool underLeft  = leftRoot  && l.transform.IsChildOf(leftRoot.transform);
            bool underRight = rightRoot && l.transform.IsChildOf(rightRoot.transform);
            l.enabled = useLeft ? underLeft : underRight;
        }
    }

    void Force(InputActionReference ar, string path)
    {
        if (ar == null || ar.action == null) return;
        var a = ar.action;
        for (int i = 0; i < a.bindings.Count; i++)
        {
            var b = a.bindings[i];
            if (b.isComposite || b.isPartOfComposite) continue;
            a.ApplyBindingOverride(i, new InputBinding { overridePath = path });
        }
    }
    void Enable(params InputActionReference[] arr)  { foreach (var r in arr) if (r && r.action != null) r.action.Enable(); }
    void Disable(params InputActionReference[] arr) { foreach (var r in arr) if (r && r.action != null) r.action.Disable(); }
}
