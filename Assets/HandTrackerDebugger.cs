using UnityEngine;

public class HandTrackerDebugger : MonoBehaviour
{
    Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if (transform.position != lastPosition)
        {
            Debug.Log($"[HandTracker] Moved to {transform.position}");
            lastPosition = transform.position;
        }
    }
}