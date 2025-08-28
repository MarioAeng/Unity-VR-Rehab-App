using UnityEngine;
using TMPro;

public class TargetSpawner : MonoBehaviour
{
    public enum HandMode { Right, Left, Auto }

    [Header("Handedness")]
    public HandMode mode = HandMode.Auto;
    public Transform rightHandOrigin;   // assign MainSelectorHand here
    public Transform leftHandOrigin;    // assign LeftSelectorHand here
    public bool cameraRelativeSpawning = true; // keep ON for consistent behavior across hands

    [Header("References")]
    public GameObject targetPrefab;
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

    [Header("Layer Settings")]
    public int targetLayerIndex = 6;  // adjust to your Target layer index

    // State
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

    private Transform activeHand; // resolved from handedness

    void Awake()
    {
        ResolveActiveHand();
    }

    void Start()
    {
        UpdateUI();

        if (activeHand == null)
        {
            Debug.LogError("[TargetSpawner] No active hand could be resolved. Assign rightHandOrigin/leftHandOrigin and set mode.");
            return;
        }

        SpawnNewTarget();
    }

    void Update()
    {
        if (targetActive && level >= 3)
        {
            currentTimer -= Time.deltaTime;
            if (timerText != null) timerText.text = $"Timer: {currentTimer:F1}s";

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

    // --- Public API (optional) ---
    // Call this from your Handedness initializer after scene load if you prefer to explicitly set it.
    public void ApplyHandedness(bool leftHanded)
    {
        mode = leftHanded ? HandMode.Left : HandMode.Right;
        ResolveActiveHand();
        Debug.Log($"[TargetSpawner] ApplyHandedness called. Mode set to {mode}. Active hand = {(activeHand ? activeHand.name : "null")}");
    }

    // --- Internals ---
    private void ResolveActiveHand()
    {
        Transform pick = null;

        switch (mode)
        {
            case HandMode.Left:
                pick = leftHandOrigin != null ? leftHandOrigin : rightHandOrigin;
                break;
            case HandMode.Right:
                pick = rightHandOrigin != null ? rightHandOrigin : leftHandOrigin;
                break;
            case HandMode.Auto:
                // Prefer whichever is active in hierarchy; fall back to right, then left
                if (leftHandOrigin != null && leftHandOrigin.gameObject.activeInHierarchy &&
                    (rightHandOrigin == null || !rightHandOrigin.gameObject.activeInHierarchy))
                {
                    pick = leftHandOrigin;
                }
                else if (rightHandOrigin != null && rightHandOrigin.gameObject.activeInHierarchy)
                {
                    pick = rightHandOrigin;
                }
                else
                {
                    pick = rightHandOrigin != null ? rightHandOrigin : leftHandOrigin;
                }
                break;
        }

        activeHand = pick;

        Debug.Log($"[TargetSpawner] ResolveActiveHand -> Mode: {mode}, ActiveHand: {(activeHand ? activeHand.name : "null")}, " +
                  $"LeftActive: {(leftHandOrigin ? leftHandOrigin.gameObject.activeInHierarchy : false)}, " +
                  $"RightActive: {(rightHandOrigin ? rightHandOrigin.gameObject.activeInHierarchy : false)}");
    }

    private void ClearCurrentTarget()
    {
        if (currentTarget != null)
        {
            Destroy(currentTarget);
            currentTarget = null;
            targetActive = false;
        }
    }

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

    private void CheckLevelProgress()
    {
        if (repsThisLevel >= 10)
        {
            bool passed = hitsThisLevel >= 8;

            if (passed)
            {
                level++;
                if (trainerPromptText) trainerPromptText.text = $"Nice work! Level {level} starting...";
            }
            else
            {
                if (trainerPromptText) trainerPromptText.text = "Try again to pass!";
            }

            repsThisLevel = 0;
            hitsThisLevel = 0;
        }
    }

    private void UpdateUI()
    {
        if (repCounterText) repCounterText.text = $"Reps: {hitsThisLevel}/{repsThisLevel}";
        if (levelText) levelText.text = $"Level: {level}";

        if (level < 3 && timerText) timerText.text = "";

        if (level == 1 && !hasShownInstructions)
        {
            if (trainerPromptText)
                trainerPromptText.text = "Target Practice: Aim at the cube and press trigger. Complete all 10 reps. Hit at least 8 to advance.";
            hasShownInstructions = true;
        }
        else if (level > 1 && hasShownInstructions)
        {
            if (trainerPromptText) trainerPromptText.text = "";
        }
    }

    public void SpawnNewTarget()
    {
        ClearCurrentTarget();

        if (activeHand == null)
        {
            Debug.LogError("[TargetSpawner] Cannot spawn. Active hand is null.");
            return;
        }

        // --- Pick new offsets with required jump distance from last ---
        float xOffset = 0f;
        float yOffset = 0f;
        int attempts = 0;

        do
        {
            float dirX = Random.value < 0.5f ? -1f : 1f;
            float jumpX = Random.Range(minJumpDistanceX, Mathf.Abs(horizontalMax));
            xOffset = Mathf.Clamp(lastXOffset + dirX * jumpX, horizontalMin, horizontalMax);

            float dirY = Random.value < 0.5f ? -1f : 1f;
            float jumpY = Random.Range(minJumpDistanceY, Mathf.Abs(verticalMax));
            yOffset = Mathf.Clamp(lastYOffset + dirY * jumpY, verticalMin, verticalMax);

            attempts++;
            if (attempts > 10) break;

        } while (hasSpawnedBefore &&
                 Mathf.Abs(xOffset - lastXOffset) < minJumpDistanceX &&
                 Mathf.Abs(yOffset - lastYOffset) < minJumpDistanceY);

        // --- Compute spawn basis ---
        Vector3 spawnPos;
        Quaternion finalRotation;

        if (cameraRelativeSpawning && Camera.main != null)
        {
            // Use camera's horizontal frame for consistent behavior across hands
            Vector3 camFwd = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;

            // Flatten forward/right on the horizontal plane for predictable X/Z, keep world up for Y
            Vector3 fwd = Vector3.ProjectOnPlane(camFwd, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(camRight, Vector3.up).normalized;
            Vector3 up = Vector3.up;

            spawnPos = activeHand.position + fwd * forwardOffset + right * xOffset + up * yOffset;

            Quaternion lookRotation = Quaternion.LookRotation(camFwd);
            Quaternion correction = Quaternion.Euler(90f, 0f, 0f);
            finalRotation = lookRotation * correction;

            Debug.Log($"[TargetSpawner] Camera-relative spawn | Hand={activeHand.name} | fwd={fwd} right={right} up={up} | " +
                      $"xOffset={xOffset:F2} yOffset={yOffset:F2} forwardOffset={forwardOffset:F2}");
        }
        else
        {
            // Fallback: spawn in the hand's local axes (may differ per hand)
            Vector3 local = new Vector3(xOffset, yOffset, forwardOffset);
            spawnPos = activeHand.TransformPoint(local);

            Quaternion lookRotation = (Camera.main != null)
                ? Quaternion.LookRotation(Camera.main.transform.forward)
                : Quaternion.LookRotation(activeHand.forward);
            Quaternion correction = Quaternion.Euler(90f, 0f, 0f);
            finalRotation = lookRotation * correction;

            Debug.Log($"[TargetSpawner] Hand-relative spawn | Hand={activeHand.name} | local={local} " +
                      $"| xOff={xOffset:F2} yOff={yOffset:F2}");
        }

        // --- Instantiate target ---
        currentTarget = Instantiate(targetPrefab, spawnPos, finalRotation);
        currentTarget.tag = "TargetCube";
        currentTarget.layer = targetLayerIndex;

        // scale by level
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
        else
        {
            currentTimer = 0f;
            targetActive = false;
        }

        Debug.Log($"[TargetSpawner] Spawned target | Hand={activeHand.name} | Pos={spawnPos} | Scale={currentTarget.transform.localScale} | " +
                  $"Lifetime={currentTimer:F1}s | Level={level} | Hits={hitsThisLevel} Reps={repsThisLevel}");
    }
}
