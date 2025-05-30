using UnityEngine;

public class AutoPositionCanvas : MonoBehaviour
{
    public float distance = 1.5f;
    public float height = 1.0f;

    void Start()
    {
        Transform cam = Camera.main.transform;
        Vector3 forward = cam.forward;
        forward.y = 0; // flatten so it's always in front, not above/below
        forward.Normalize();

        transform.position = cam.position + forward * distance + Vector3.up * height;
        transform.LookAt(cam);
        transform.Rotate(0, 180f, 0);
    }
}