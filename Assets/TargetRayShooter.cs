using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class TargetRayShooter : MonoBehaviour
{
    public InputActionProperty triggerAction;
    public LayerMask targetLayer;
    public float rayDistance = 10f;
    public TargetSpawner spawner;

    private bool wasPressedLastFrame = false;

    void Update()
    {
        if (triggerAction.action == null || spawner == null)
            return;

        float value = triggerAction.action.ReadValue<float>();
        bool isPressed = value > 0.5f;

        if (isPressed && !wasPressedLastFrame)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, targetLayer))
            {
                if (hit.collider.CompareTag("TargetCube"))
                {
                    Debug.Log("[Shooter] Hit target cube!");
                    Destroy(hit.collider.gameObject);
                    spawner.RegisterHit();
                }
            }
        }

        wasPressedLastFrame = isPressed;
    }
}