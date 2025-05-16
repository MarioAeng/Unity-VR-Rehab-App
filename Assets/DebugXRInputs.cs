using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.Controls;

public class DebugXRInputs : MonoBehaviour
{
    void Update()
    {
        foreach (var device in InputSystem.devices)
        {
            if (device is XRController xr && device.name.ToLower().Contains("right"))
            {
                foreach (var control in device.allControls)
                {
                    if (control is AxisControl axis)
                    {
                        float value = axis.ReadValue();
                        Debug.Log($"[XR Raw] {control.name} = {value:F2}");
                    }
                }
            }
        }
    }
}