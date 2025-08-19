using System.Collections.Generic;
using UnityEngine;

public class HandGripAnimator : MonoBehaviour
{
    [Header("Grip Animation Settings")]
    public float maxCurlAngle = -45f;   // same as your right hand
    public bool rotateOnX = false;
    public bool rotateOnY = false;
    public bool rotateOnZ = true;
    public bool invertAngle = false;    // ← enable on LEFT if it curls the wrong way

    [Header("Optional: Auto-bind from OVR Custom Skeleton")]
    public OVRCustomSkeleton skeleton;  // drag the OVRCustomSkeleton here (on the same hand)
    public bool autoBindFromSkeleton = true;

    [Header("Finger Joints (manual if not auto-binding)")]
    public Transform thumbBase, thumbTip;
    public Transform indexBase, indexMid, indexTip;
    public Transform middleBase, middleMid, middleTip;
    public Transform ringBase, ringMid, ringTip;
    public Transform pinkyBase, pinkyMid, pinkyTip;

    [Header("Debug")]
    public bool showDebugSpheres = false;

    private Dictionary<string, List<Transform>> fingerJoints;

    void Start()
    {
        if (autoBindFromSkeleton && skeleton != null)
            AutoBindFromSkeleton();

        fingerJoints = new Dictionary<string, List<Transform>>()
        {
            { "Thumb",  new List<Transform> { thumbBase,  thumbTip } },
            { "Index",  new List<Transform> { indexBase,  indexMid,  indexTip } },
            { "Middle", new List<Transform> { middleBase, middleMid, middleTip } },
            { "Ring",   new List<Transform> { ringBase,   ringMid,   ringTip } },
            { "Pinky",  new List<Transform> { pinkyBase,  pinkyMid,  pinkyTip } },
        };

        if (showDebugSpheres)
        {
            foreach (var kvp in fingerJoints)
                foreach (var joint in kvp.Value)
                    if (joint != null)
                    {
                        var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        s.transform.position = joint.position;
                        s.transform.localScale = Vector3.one * 0.01f;
                        s.GetComponent<Renderer>().material.color = Color.magenta;
                        Destroy(s.GetComponent<Collider>());
                    }
        }
    }

    void Update()
    {
        float grip = 1.0f; // always fully gripping (same as your script)
        float angle = grip * maxCurlAngle * (invertAngle ? -1f : 1f);

        foreach (var kvp in fingerJoints)
        {
            foreach (var joint in kvp.Value)
            {
                if (!joint) continue;

                Vector3 rot = Vector3.zero;
                if (rotateOnX) rot.x = angle;
                if (rotateOnY) rot.y = angle;
                if (rotateOnZ) rot.z = angle;

                joint.localRotation = Quaternion.Euler(rot);
            }
        }
    }

    // --- Helpers ---
    void AutoBindFromSkeleton()
    {
        Transform Get(OVRSkeleton.BoneId id)
        {
            var bones = skeleton.Bones;
            if (bones == null) return null;
            for (int i = 0; i < bones.Count; i++)
                if (bones[i].Id == id) return bones[i].Transform;
            return null;
        }

        // Thumb uses 1 (base) and 3 (tip)
        thumbBase = thumbBase ? thumbBase : Get(OVRSkeleton.BoneId.Hand_Thumb1);
        thumbTip  = thumbTip  ? thumbTip  : Get(OVRSkeleton.BoneId.Hand_Thumb3);

        indexBase = indexBase ? indexBase : Get(OVRSkeleton.BoneId.Hand_Index1);
        indexMid  = indexMid  ? indexMid  : Get(OVRSkeleton.BoneId.Hand_Index2);
        indexTip  = indexTip  ? indexTip  : Get(OVRSkeleton.BoneId.Hand_Index3);

        middleBase = middleBase ? middleBase : Get(OVRSkeleton.BoneId.Hand_Middle1);
        middleMid  = middleMid  ? middleMid  : Get(OVRSkeleton.BoneId.Hand_Middle2);
        middleTip  = middleTip  ? middleTip  : Get(OVRSkeleton.BoneId.Hand_Middle3);

        ringBase = ringBase ? ringBase : Get(OVRSkeleton.BoneId.Hand_Ring1);
        ringMid  = ringMid  ? ringMid  : Get(OVRSkeleton.BoneId.Hand_Ring2);
        ringTip  = ringTip  ? ringTip  : Get(OVRSkeleton.BoneId.Hand_Ring3);

        pinkyBase = pinkyBase ? pinkyBase : Get(OVRSkeleton.BoneId.Hand_Pinky1);
        pinkyMid  = pinkyMid  ? pinkyMid  : Get(OVRSkeleton.BoneId.Hand_Pinky2);
        pinkyTip  = pinkyTip  ? pinkyTip  : Get(OVRSkeleton.BoneId.Hand_Pinky3);
    }
}
