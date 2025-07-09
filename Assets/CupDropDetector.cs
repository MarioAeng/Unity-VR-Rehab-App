using UnityEngine;

public class CupDropDetector : MonoBehaviour
{
    [HideInInspector] public CupGameManager manager;
    [HideInInspector] public GameObject targetTable;
    public float requiredStayTime = 0.6f;
    public float safeDropHeight = 1.2f;

    private float stayTimer = 0f;
    private bool inZone = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == targetTable)
        {
            inZone = true;
            stayTimer = 0f;
            Debug.Log("[DropDetector] Entered target table.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == targetTable)
        {
            inZone = false;
            stayTimer = 0f;
            Debug.Log("[DropDetector] Exited target table.");
        }
    }

    void Update()
    {
        if (inZone && transform.position.y <= safeDropHeight + 0.1f)
        {
            stayTimer += Time.deltaTime;
            if (stayTimer >= requiredStayTime)
            {
                Debug.Log("[DropDetector] ✅ Cup stayed low on target. Counting rep.");
                manager?.RegisterSuccessfulDrop();
                stayTimer = 0f;
                inZone = false;
            }
        }
        else if (inZone)
        {
            Debug.Log("[DropDetector] ❌ Too high to register rep.");
        }
    }
}