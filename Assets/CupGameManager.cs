using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CupGameManager : MonoBehaviour
{
    [Header("Prefabs & Spawn")]
    public GameObject cupPrefab;
    public GameObject dropTablePrefab;
    public Transform spawnStartPoint;

    [Header("UI")]
    public TMP_Text repCounterText;
    public TMP_Text levelText;

    [Header("Gameplay Settings")]
    public int repsPerLevel = 10;
    public float dropStayTime = 0f; // set > 0 for delayed success

    private int currentReps = 0;
    private int currentLevel = 1;
    private List<GameObject> currentPlatforms = new List<GameObject>();

    private void Start()
    {
        Debug.Log("[CupGameManager] Starting game");
        UpdateUI();
        SpawnLevelPlatforms();
        SpawnCup();
    }

    private void UpdateUI()
    {
        if (repCounterText) repCounterText.text = $"Reps: {currentReps}/{repsPerLevel}";
        if (levelText) levelText.text = $"Level {currentLevel}";
    }

    public void RegisterCupDrop()
    {
        currentReps++;
        Debug.Log($"[CupGameManager] Cup dropped successfully. Total reps: {currentReps}");

        UpdateUI();

        if (currentReps >= repsPerLevel)
        {
            currentLevel++;
            currentReps = 0;
            Debug.Log($"[CupGameManager] Advancing to Level {currentLevel}");
            ClearLevel();
            SpawnLevelPlatforms();
        }

        SpawnCup();
    }

    private void SpawnCup()
    {
        if (cupPrefab == null || spawnStartPoint == null)
        {
            Debug.LogError("[CupGameManager] Cup prefab or spawn point is not assigned!");
            return;
        }

        Vector3 spawnPos = spawnStartPoint.position;
        GameObject cup = Instantiate(cupPrefab, spawnPos, Quaternion.identity);
        Debug.Log($"[CupGameManager] Spawned Cup at {spawnPos}");

        CupDropDetector detector = cup.GetComponent<CupDropDetector>();
        if (detector != null)
        {
            detector.manager = this;
            detector.requiredStayTime = dropStayTime;
            Debug.Log("[CupGameManager] Assigned manager to CupDropDetector");
        }
        else
        {
            Debug.LogWarning("[CupGameManager] Spawned Cup is missing CupDropDetector");
        }
    }

    private void SpawnLevelPlatforms()
    {
        if (dropTablePrefab == null)
        {
            Debug.LogError("[CupGameManager] Drop table prefab is missing!");
            return;
        }

        int platformCount = Mathf.Min(currentLevel + 1, 5);
        float spacing = 0.75f + currentLevel * 0.25f;

        for (int i = 0; i < platformCount; i++)
        {
            Vector3 pos = new Vector3(i * spacing, 0.1f, 1.5f);
            GameObject platform = Instantiate(dropTablePrefab, pos, Quaternion.identity);
            currentPlatforms.Add(platform);

            Debug.Log($"[CupGameManager] Spawned drop platform at {pos}");
        }
    }

    private void ClearLevel()
    {
        foreach (GameObject platform in currentPlatforms)
        {
            if (platform != null) Destroy(platform);
        }

        currentPlatforms.Clear();
    }
}
