using UnityEngine;

public class CupDropDetector : MonoBehaviour
{
    [HideInInspector] public CupGameManager manager;
    public float requiredStayTime = 0f;

    private bool hasRegistered = false;
    private float stayTimer = 0f;
    private bool inZone = false;

    void Start()
    {
        Debug.Log($"[CupDropDetector] Start - Attached to: {gameObject.name}");

        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("[CupDropDetector] No Collider found on this cup!");
        }
        else
        {
            Debug.Log($"[CupDropDetector] Collider info: IsTrigger={col.isTrigger}, Bounds={col.bounds}");
        }

        if (manager == null)
        {
            Debug.LogWarning("[CupDropDetector] Manager is not assigned!");
        }

        // Debug all scene colliders
        foreach (var sceneCol in FindObjectsOfType<Collider>())
        {
            Debug.Log($"[ColliderScan] {sceneCol.name} at {sceneCol.transform.position}, Trigger={sceneCol.isTrigger}, Layer={sceneCol.gameObject.layer}");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[CupDropDetector] TriggerEnter with: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("DropPlatform"))
        {
            inZone = true;
            stayTimer = 0f;
            Debug.Log("[CupDropDetector] Entered DropPlatform zone.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("DropPlatform"))
        {
            inZone = false;
            stayTimer = 0f;
            Debug.Log("[CupDropDetector] Exited DropPlatform zone.");
        }
    }

    void Update()
    {
        if (hasRegistered || !inZone || manager == null) return;

        if (requiredStayTime <= 0f)
        {
            Debug.Log("[CupDropDetector] Cup dropped and instantly counted (no stay time).");
            hasRegistered = true;
            manager.RegisterCupDrop();
        }
        else
        {
            stayTimer += Time.deltaTime;
            if (stayTimer >= requiredStayTime)
            {
                Debug.Log("[CupDropDetector] Cup stayed long enough — counting rep.");
                hasRegistered = true;
                manager.RegisterCupDrop();
            }
        }
    }
}
