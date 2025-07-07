using UnityEngine;

public class CupDropDebugger : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[CupDropDebugger] Cup collided with: {collision.gameObject.name} at {transform.position}");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[CupDropDebugger] Cup ENTERED TRIGGER: {other.gameObject.name} at {transform.position}");
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log($"[CupDropDebugger] Cup STAYING on TRIGGER: {other.gameObject.name} at {transform.position}");
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[CupDropDebugger] Cup EXITED TRIGGER: {other.gameObject.name} at {transform.position}");
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log($"[CupDropDebugger] Cup exited collision with: {collision.gameObject.name} at {transform.position}");
    }
}