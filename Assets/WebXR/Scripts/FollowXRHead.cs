using UnityEngine;

public class FollowXRHead : MonoBehaviour
{
    public Transform xrHead;        // Assign the XR Camera (Main Camera)
    public float distance = 1.5f;   // Forward offset from head
    public float heightOffset = -0.2f; // Vertical offset

    void LateUpdate()
    {
        if (xrHead == null) return;

        Vector3 forwardPos = xrHead.position + xrHead.forward * distance;
        forwardPos.y = xrHead.position.y + heightOffset;

        transform.position = forwardPos;

        // Always face the user
        transform.rotation = Quaternion.LookRotation(transform.position - xrHead.position);
    }
}
