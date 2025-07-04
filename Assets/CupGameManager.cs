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

    private int totalReps = 10;
    private int repsCompleted = 0;
    private int currentLevel = 1;

    private List<GameObject> activeTables = new();
    private GameObject activeCup;
    private GameObject targetTable;
    private GameObject startTable;

    private float platformSpacing = 1.4f;
    private float zClampMin = -1.5f, zClampMax = 1.5f;

    void Start()
    {
        InitializeLevel();
    }

    void InitializeLevel()
    {
        Debug.Log($"[Manager] Level {currentLevel} Init");
        ClearPreviousObjects();
        repsCompleted = 0;
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
        while (activeTables.Count < count && tries < 100)
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
            }

            tries++;
        }

        targetTable = activeTables[Random.Range(0, activeTables.Count)];

        do {
            startTable = activeTables[Random.Range(0, activeTables.Count)];
        } while (startTable == targetTable);

        foreach (var table in activeTables)
        {
            var rend = table.GetComponent<Renderer>();
            if (rend)
            {
                rend.material = (table == targetTable) ? dropTableVisibleMat : defaultPlatformMat;
            }
        }

        Debug.Log($"[Tables] Start: {startTable.name}, Target: {targetTable.name}");
    }

    void SpawnCup()
    {
        Vector3 pos = startTable.transform.position + Vector3.up * 0.5f;
        activeCup = Instantiate(Cup, pos, Quaternion.identity);
        activeCup.SetActive(true);

        // Find CupDropDetector on the child (TriggerZone)
        CupDropDetector detector = activeCup.GetComponentInChildren<CupDropDetector>();
        if (detector)
        {
            detector.manager = this;
            detector.targetTable = targetTable;
            detector.requiredStayTime = currentLevel >= 2 ? 0.6f : 0f;

            Debug.Log("[SpawnCup] Detector set on TriggerZone child.");
        }
        else
        {
            Debug.LogWarning("[SpawnCup] CupDropDetector not found in child.");
        }

        var rend = activeCup.GetComponent<Renderer>();
        if (rend) rend.material = cupVisibleMat;

        Debug.Log("[SpawnCup] Cup spawned and visible.");
    }

    public void RegisterSuccessfulDrop()
    {
        Debug.Log($"[Progress] Rep {repsCompleted + 1} / {totalReps}");

        if (activeCup != null)
        {
            Destroy(activeCup);
            activeCup = null;
        }

        repsCompleted++;
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
            TrainerPrompt.text = $"Level {currentLevel} complete!";
            currentLevel++;
            InitializeLevel();
        }
    }

    void UpdateRepCounter()
    {
        RepCounterText.text = $"Delivered: {repsCompleted}/{totalReps}";
    }
}
