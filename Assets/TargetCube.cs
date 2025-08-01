using UnityEngine;

public class TargetCube : MonoBehaviour
{
    private TargetSpawner spawner;

    void Start()
    {
        spawner = FindObjectOfType<TargetSpawner>();
        if (spawner == null)
        {
            Debug.LogError("[TargetCube] TargetSpawner not found in the scene.");
        }
    }

    public void HandleHit()
    {
        if (spawner != null)
        {
            Debug.Log("[TargetCube] Hit registered. Calling spawner.RegisterHit()");
            spawner.OnTargetHit();

        }
        else
        {
            Debug.LogWarning("[TargetCube] Cannot call RegisterHit - spawner is null.");
        }

        Destroy(gameObject);
    }
}