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
    public float verticalMin = -0.25f;
    public float verticalMax = 0.25f;
    public float horizontalMin = -0.3f;
    public float horizontalMax = 0.3f;
    public float forwardOffset = 2f;

    [Header("Gameplay Settings")]
    public float baseTargetLifetime = 10f;
    public float minJumpDistanceX = 0.25f;
    public float minJumpDistanceY = 0.12f;

    private int repsThisLevel = 0;
    private int hitsThisLevel = 0;
    private int level = 1;
    private GameObject currentTarget;
    private float currentTimer = 0f;
    private bool targetActive = false;
    private bool hasShownInstructions = false;

    private float lastXOffset = 0f;
    private float lastYOffset = 0f;
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

    // ✅ This is the method called via SendMessage from the ray shooter
    public void OnTargetHit()
    {
        Debug.Log("[TargetSpawner] Registering hit via OnTargetHit()");
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

        float xOffset = 0f;
        float yOffset = 0f;
        int attempts = 0;

        do
        {
            float dirX = Random.value < 0.5f ? -1f : 1f;
            float jumpX = Random.Range(minJumpDistanceX, horizontalMax);
            xOffset = Mathf.Clamp(lastXOffset + dirX * jumpX, horizontalMin, horizontalMax);

            float dirY = Random.value < 0.5f ? -1f : 1f;
            float jumpY = Random.Range(minJumpDistanceY, verticalMax);
            yOffset = Mathf.Clamp(lastYOffset + dirY * jumpY, verticalMin, verticalMax);

            attempts++;
            if (attempts > 10) break;

        } while (hasSpawnedBefore &&
                Mathf.Abs(xOffset - lastXOffset) < minJumpDistanceX &&
                Mathf.Abs(yOffset - lastYOffset) < minJumpDistanceY);

        Vector3 offset = new Vector3(xOffset, yOffset, forwardOffset);
        Vector3 spawnPosition = handOrigin.position + handOrigin.TransformDirection(offset);

        currentTarget = Instantiate(targetPrefab, spawnPosition, Quaternion.identity);
        currentTarget.tag = "TargetCube";

        float baseScale = 0.2f;
        float minScale = 0.06f;
        float scaleMultiplier = Mathf.Max(minScale, 1f - 0.1f * (level - 1));
        currentTarget.transform.localScale = Vector3.one * baseScale * scaleMultiplier;

        lastXOffset = xOffset;
        lastYOffset = yOffset;
        hasSpawnedBefore = true;

        if (level >= 3)
        {
            float difficultyAdjustedLifetime = Mathf.Max(0.7f, baseTargetLifetime - (level * 1.0f));
            currentTimer = difficultyAdjustedLifetime;
            targetActive = true;
        }

        Debug.Log($"[TargetSpawner] Spawned target at {spawnPosition} | Scale: {currentTarget.transform.localScale} | Lifetime: {currentTimer:F1}s");
    }
}
