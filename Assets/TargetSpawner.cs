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
    public float spawnRadius = 0.6f;
    public float verticalMin = -0.5f;
    public float verticalMax = 0.2f;
    public float horizontalMin = -0.5f;
    public float horizontalMax = 0.5f;
    public float forwardOffset = 2f;

    [Header("Gameplay Settings")]
    public float baseTargetLifetime = 10f;

    private int repsThisLevel = 0;
    private int hitsThisLevel = 0;
    private int level = 1;
    private GameObject currentTarget;
    private float currentTimer = 0f;
    private bool targetActive = false;
    private bool hasShownInstructions = false;

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
            trainerPromptText.text = ""; // clear instructions after level 1
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

        Vector3 offset = new Vector3(
            Random.Range(horizontalMin, horizontalMax),
            Random.Range(verticalMin, verticalMax),
            forwardOffset
        );

        Vector3 spawnPosition = handOrigin.position + handOrigin.TransformDirection(offset);

        currentTarget = Instantiate(targetPrefab, spawnPosition, Quaternion.identity);
        currentTarget.tag = "TargetCube";

        float baseScale = 0.2f;
        float sizeMultiplier = Mathf.Max(0.5f, 1f - 0.1f * (level - 1)); // Shrinks with level
        currentTarget.transform.localScale = Vector3.one * baseScale * sizeMultiplier;

        if (level >= 3)
        {
            currentTimer = Mathf.Max(3f, baseTargetLifetime - level); // Decreasing timer
            targetActive = true;
        }

        Debug.Log($"[TargetSpawner] Spawned target at {spawnPosition} | Scale: {currentTarget.transform.localScale}");
    }
}
