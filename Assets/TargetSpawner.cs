using UnityEngine;
using TMPro;

public class TargetSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject targetPrefab;
    public Transform handOrigin;
    public TMP_Text repCounterText;
    public TMP_Text trainerPromptText;
    public TMP_Text levelText;
    public TMP_Text timerText;

    [Header("Spawn Settings")]
    public float verticalMin = -0.1f;
    public float verticalMax = 0.1f;
    public float horizontalMin = -0.05f;
    public float horizontalMax = 0.05f;
    public float forwardOffset = 2f;

    [Header("Gameplay Settings")]
    public float baseTargetLifetime = 10f;
    public float baseMinSpacing = 0.3f;

    private int repsThisLevel = 0;
    private int hitsThisLevel = 0;
    private int level = 1;
    private GameObject currentTarget;
    private float currentTimer = 0f;
    private bool targetActive = false;
    private bool hasShownInstructions = false;

    private Vector3 lastSpawnPosition;
    private bool hasSpawnedBefore = false;

    void Start()
    {
        UpdateUI();
        SpawnNewTarget();
    }

    void Update()
    {
        if (targetActive && level >= 3)
        {
            currentTimer -= Time.deltaTime;
            timerText.text = $"Timer: {currentTimer:F1}s";

            if (currentTimer <= 0f)
            {
                Debug.Log("[TargetSpawner] Timer expired, target missed.");
                ClearCurrentTarget();
                repsThisLevel++;
                UpdateUI();
                CheckLevelProgress();
                SpawnNewTarget();
            }
        }
    }

    void ClearCurrentTarget()
    {
        if (currentTarget != null)
        {
            Destroy(currentTarget);
            currentTarget = null;
            targetActive = false;
        }
    }

    public void RegisterHit()
    {
        Debug.Log("[TargetSpawner] Registering hit");
        hitsThisLevel++;
        repsThisLevel++;
        UpdateUI();

        ClearCurrentTarget();
        CheckLevelProgress();
        SpawnNewTarget();
    }

    void CheckLevelProgress()
    {
        if (repsThisLevel >= 10)
        {
            bool passed = hitsThisLevel >= 8;

            if (passed)
            {
                level++;
                trainerPromptText.text = $"Nice work! Level {level} starting...";
            }
            else
            {
                trainerPromptText.text = "Try again to pass!";
            }

            repsThisLevel = 0;
            hitsThisLevel = 0;
        }
    }

    void UpdateUI()
    {
        repCounterText.text = $"Reps: {hitsThisLevel}/{repsThisLevel}";
        levelText.text = $"Level: {level}";

        if (level < 3)
            timerText.text = "";

        if (level == 1 && !hasShownInstructions)
        {
            trainerPromptText.text = "Target Practice: Aim at the cube and press trigger. Complete all 10 reps. Hit at least 8 to advance.";
            hasShownInstructions = true;
        }
        else if (level > 1 && hasShownInstructions)
        {
            trainerPromptText.text = "";
        }
    }

    public void SpawnNewTarget()
    {
        ClearCurrentTarget();

        if (handOrigin == null)
        {
            Debug.LogError("[TargetSpawner] Hand origin not assigned.");
            return;
        }

        float minDistanceFromLastSpawn = baseMinSpacing + 0.05f * (level - 1);
        float maxSpacing = 0.2f; // since horizontalMin/Max are narrow
        minDistanceFromLastSpawn = Mathf.Min(minDistanceFromLastSpawn, maxSpacing);

        Vector3 spawnPosition;
        int tries = 0;

        do
        {
            float xOffset = Random.Range(horizontalMin, horizontalMax);
            float yOffset = Random.Range(verticalMin, verticalMax);
            Vector3 offset = new Vector3(xOffset, yOffset, forwardOffset);
            spawnPosition = handOrigin.position + handOrigin.TransformDirection(offset);

            tries++;
            if (tries > 15) break;

        } while (hasSpawnedBefore && Vector3.Distance(spawnPosition, lastSpawnPosition) < minDistanceFromLastSpawn);

        currentTarget = Instantiate(targetPrefab, spawnPosition, Quaternion.identity);
        currentTarget.tag = "TargetCube";

        float baseScale = 0.2f;
        float sizeMultiplier = Mathf.Max(0.5f, 1f - 0.1f * (level - 1));
        currentTarget.transform.localScale = Vector3.one * baseScale * sizeMultiplier;

        lastSpawnPosition = spawnPosition;
        hasSpawnedBefore = true;

        if (level >= 3)
        {
            float difficultyAdjustedLifetime = Mathf.Max(1.5f, baseTargetLifetime - (level * 0.5f));
            currentTimer = difficultyAdjustedLifetime;
            targetActive = true;
        }

        Debug.Log($"[TargetSpawner] Spawned target at {spawnPosition} | Level: {level}");
    }
}
