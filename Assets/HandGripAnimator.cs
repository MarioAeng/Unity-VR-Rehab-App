using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandGripAnimator : MonoBehaviour
{
    [Header("Grip Animation Settings")]
    public float maxCurlAngle = -45f; // Softened grip to prevent mesh clipping
    public bool rotateOnX = false;
    public bool rotateOnY = false;
    public bool rotateOnZ = true;

    [Header("Finger Joints")]
    public Transform thumbBase;
    public Transform thumbTip;
    public Transform indexBase;
    public Transform indexMid;
    public Transform indexTip;
    public Transform middleBase;
    public Transform middleMid;
    public Transform middleTip;
    public Transform ringBase;
    public Transform ringMid;
    public Transform ringTip;
    public Transform pinkyBase;
    public Transform pinkyMid;
    public Transform pinkyTip;

    [Header("Debug")]
    public bool showDebugSpheres = false;

    private Dictionary<string, List<Transform>> fingerJoints;

    private void Start()
    {
        fingerJoints = new Dictionary<string, List<Transform>>()
        {
            { "Thumb", new List<Transform> { thumbBase, thumbTip } },
            { "Index", new List<Transform> { indexBase, indexMid, indexTip } },
            { "Middle", new List<Transform> { middleBase, middleMid, middleTip } },
            { "Ring", new List<Transform> { ringBase, ringMid, ringTip } },
            { "Pinky", new List<Transform> { pinkyBase, pinkyMid, pinkyTip } },
        };

        if (showDebugSpheres)
        {
            foreach (var kvp in fingerJoints)
            {
                foreach (var joint in kvp.Value)
                {
                    if (joint != null)
                    {
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        sphere.transform.position = joint.position;
                        sphere.transform.localScale = Vector3.one * 0.01f;
                        sphere.GetComponent<Renderer>().material.color = Color.magenta;
                        Destroy(sphere.GetComponent<Collider>());
                    }
                }
            }
        }
    }

    private void Update()
    {
        float grip = 1.0f; // Always fully gripping

        foreach (var kvp in fingerJoints)
        {
            foreach (var joint in kvp.Value)
            {
                if (joint == null) continue;

                float angle = grip * maxCurlAngle;
                Vector3 rotation = Vector3.zero;

                if (rotateOnX) rotation.x = angle;
                if (rotateOnY) rotation.y = angle;
                if (rotateOnZ) rotation.z = angle;

                joint.localRotation = Quaternion.Euler(rotation);
            }
        }
    }
}
