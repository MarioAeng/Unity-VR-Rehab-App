using UnityEngine;

public class ObjectLifecycleLogger : MonoBehaviour
{
    void OnEnable()
    {
        Debug.Log($"[{gameObject.name}] ENABLED in scene {gameObject.scene.name}");
    }

    void OnDisable()
    {
        Debug.LogWarning($"[{gameObject.name}] DISABLED in scene {gameObject.scene.name}");
    }

    void OnDestroy()
    {
        Debug.LogError($"[{gameObject.name}] DESTROYED in scene {gameObject.scene.name}");
    }
}
