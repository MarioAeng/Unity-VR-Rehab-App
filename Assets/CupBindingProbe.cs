using UnityEngine;
using UnityEngine.InputSystem;

public class CupBindingProbe : MonoBehaviour
{
    public InputActionReference leftPosition, leftRotation, leftTrigger;
    public InputActionReference rightPosition, rightRotation, rightTrigger;

    void Start()
    {
        Dump("LEFT-pos", leftPosition);
        Dump("LEFT-rot", leftRotation);
        Dump("LEFT-trg", leftTrigger);
        Dump("RIGHT-pos", rightPosition);
        Dump("RIGHT-rot", rightRotation);
        Dump("RIGHT-trg", rightTrigger);
    }

    void Dump(string label, InputActionReference r)
    {
        if (r == null || r.action == null) { Debug.Log($"{label}: (null)"); return; }
        var a = r.action;
        for (int i = 0; i < a.bindings.Count; i++)
        {
            var b = a.bindings[i];
            if (b.isComposite || b.isPartOfComposite) continue;
            var ep = a.bindings[i].effectivePath ?? a.bindings[i].path;
            Debug.Log($"{label} binding[{i}] path={ep}");
        }
        foreach (var c in a.controls)
            Debug.Log($"{label} control={c.path}   device={c.device}");
    }
}