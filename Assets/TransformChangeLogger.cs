using UnityEngine;

public class TransformChangeLogger : MonoBehaviour
{
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    void Start()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    void Update()
    {
        if (transform.position != lastPosition || transform.rotation != lastRotation)
        {
            Debug.LogWarning($"[TransformChangeLogger] Detected transform change on '{gameObject.name}'");
            Debug.LogWarning($"New Position: {transform.position}, New Rotation: {transform.rotation}");
            Debug.LogWarning($"Call stack:\n{System.Environment.StackTrace}");

            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }
    }
}