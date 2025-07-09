using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CupGameManager : MonoBehaviour
{
    [Header("Prefabs & Setup")]
    public GameObject Cup;
    public GameObject DropTable;
    public Transform SpawnStartPoint;

    public Material dropTableVisibleMat;
    public Material cupVisibleMat;
    public Material defaultPlatformMat;

    [Header("UI")]
    public TextMeshProUGUI TrainerPrompt;
    public TextMeshProUGUI RepCounterText;
    public TextMeshProUGUI TimeText;

    private int totalReps = 10;
    private int repsCompleted = 0;
    private int repsSuccessful = 0;
    private int currentLevel = 1;
    private int requiredSuccesses = 8;

    private List<GameObject> activeTables = new();
    private GameObject activeCup;
    private GameObject targetTable;
    private GameObject startTable;

    private float platformSpacing = 1.4f;
    private float zClampMin = -1.5f, zClampMax = 1.5f;

    private float repTimer = 0f;
    private float repTimeLimit = 10f;
    private bool isTiming = false;

    void Start()
    {
        InitializeLevel();
    }

    void Update()
    {
        if (currentLevel >= 2 && isTiming)
        {
            repTimer -= Time.deltaTime;
            TimeText.text = $"Time Left: {Mathf.Ceil(repTimer)}s";

            if (repTimer <= 0f)
            {
                Debug.Log("[Timer] Time's up! Resetting cup.");
                ResetCupWithoutCountingRep();
            }
        }
    }

    void InitializeLevel()
    {
        Debug.Log($"[Manager] Level {currentLevel} Init");
        ClearPreviousObjects();
        repsCompleted = 0;
        repsSuccessful = 0;
        UpdateRepCounter();
        TrainerPrompt.text = $"Deliver {totalReps} cups!";
        SpawnTables(currentLevel);
        SpawnCup();
    }

    void ClearPreviousObjects()
    {
        foreach (var t in activeTables)
            if (t != null) Destroy(t);
        activeTables.Clear();

        if (activeCup != null) Destroy(activeCup);
    }

    void SpawnTables(int level)
    {
        int count = Mathf.Clamp(2 + level, 2, 6);
        float radius = 1.5f + level * 0.4f;
        int tries = 0;

        while (activeTables.Count < count && tries < 200)
        {
            Vector3 offset = new Vector3(Random.Range(-radius, radius), 0f, Random.Range(zClampMin, zClampMax));
            Vector3 pos = SpawnStartPoint.position + offset;

            bool tooClose = false;
            foreach (var table in activeTables)
            {
                if (Vector3.Distance(pos, table.transform.position) < platformSpacing)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                GameObject table = Instantiate(DropTable, pos, Quaternion.identity);
                activeTables.Add(table);
                Debug.Log($"[SpawnTables] Placed table at {pos}");
            }

            tries++;
        }

        if (activeTables.Count < 2)
        {
            Debug.LogError("[SpawnTables] Not enough tables placed. Try increasing spacing or radius.");
            return;
        }

        int safetyTries = 0;
        do
        {
            startTable = activeTables[Random.Range(0, activeTables.Count)];
            targetTable = activeTables[Random.Range(0, activeTables.Count)];
            safetyTries++;
        } while (startTable == targetTable && safetyTries < 100);

        foreach (var table in activeTables)
        {
            var rend = table.GetComponent<Renderer>();
            if (rend)
                rend.material = (table == targetTable) ? dropTableVisibleMat : defaultPlatformMat;
        }

        Debug.Log($"[Tables] Start: {startTable.name} | Target: {targetTable.name}");
    }

    void SpawnCup()
    {
        Vector3 pos = startTable.transform.position + Vector3.up * 0.5f;
        activeCup = Instantiate(Cup, pos, Quaternion.identity);
        activeCup.SetActive(true);

        CupDropDetector detector = activeCup.GetComponentInChildren<CupDropDetector>();
        if (detector)
        {
            detector.manager = this;
            detector.targetTable = targetTable;
            detector.requiredStayTime = currentLevel >= 2 ? 0.6f : 0f;
            Debug.Log("[SpawnCup] Detector configured.");
        }
        else
        {
            Debug.LogWarning("[SpawnCup] No CupDropDetector found!");
        }

        var rend = activeCup.GetComponent<Renderer>();
        if (rend) rend.material = cupVisibleMat;

        if (currentLevel >= 2)
        {
            repTimer = repTimeLimit;
            isTiming = true;
        }
        else
        {
            TimeText.text = "";
            isTiming = false;
        }
    }

    public void RegisterSuccessfulDrop()
    {
        Debug.Log($"[Progress] Rep {repsCompleted + 1} / {totalReps}");
        isTiming = false;

        if (activeCup != null)
        {
            Destroy(activeCup);
            activeCup = null;
        }

        repsCompleted++;
        repsSuccessful++;
        UpdateRepCounter();

        if (repsCompleted < totalReps)
        {
            if (currentLevel > 1)
            {
                ClearPreviousObjects();
                SpawnTables(currentLevel);
            }
            SpawnCup();
        }
        else
        {
            if (repsSuccessful >= requiredSuccesses)
            {
                TrainerPrompt.text = $"Level {currentLevel} complete!";
                currentLevel++;
                InitializeLevel();
            }
            else
            {
                TrainerPrompt.text = $"Only {repsSuccessful}/{totalReps} correct. Try again!";
                InitializeLevel();
            }
        }
    }

    void ResetCupWithoutCountingRep()
    {
        isTiming = false;

        if (activeCup != null)
        {
            Destroy(activeCup);
            activeCup = null;
        }

        repsCompleted++;
        UpdateRepCounter();

        if (repsCompleted < totalReps)
        {
            SpawnCup();
        }
        else
        {
            if (repsSuccessful >= requiredSuccesses)
            {
                TrainerPrompt.text = $"Level {currentLevel} complete!";
                currentLevel++;
                InitializeLevel();
            }
            else
            {
                TrainerPrompt.text = $"Only {repsSuccessful}/{totalReps} correct. Try again!";
                InitializeLevel();
            }
        }
    }

    void UpdateRepCounter()
    {
        RepCounterText.text = $"Delivered: {repsSuccessful}/{totalReps}";
    }
}
