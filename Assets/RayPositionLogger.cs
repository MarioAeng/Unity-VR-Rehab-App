using UnityEngine;

public class RayPositionLogger : MonoBehaviour
{
    void Update()
    {
        Debug.Log($"[RayPositionLogger] RayOriginOffset world Y: {transform.position.y}, local Y: {transform.localPosition.y}");
    }
}
