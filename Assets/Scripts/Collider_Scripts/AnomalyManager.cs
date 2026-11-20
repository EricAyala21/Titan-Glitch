using System.Collections.Generic;
using UnityEngine;

// Each choice in the anomaly-selection menu corresponds to one category.
// Multiple prefabs can share the same category.
public enum AnomalyCategory
{
    WrongFloorNumber,
    WrongObjectOrder,
    WrongSizeOrShape,
    SpookyElements,
    Chase,
    None
}

public class AnomalyManager : MonoBehaviour
{
    [Header("Anomaly Spawning")]
    public Transform[] spawnPoints;
    public GameObject[] anomalies;

    [Header("Menu Answer for Each Anomaly Prefab")]
    [Tooltip("Set one category for each element in Anomalies, in the same order.")]
    public AnomalyCategory[] anomalyCategories;

    // Completed anomalies persist across scene reloads.
    private static HashSet<int> completedAnomalies = new HashSet<int>();

    // Current anomaly and spawn location persist on failure.
    private static int activeAnomalyIndex = -1;
    private static int activeSpawnIndex = -1;
    public int CurrentAnomalyIndex { get; private set; } = -1;

    // Reset static data at the beginning of a new Play session.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitializeRun()
    {
        ResetRun();
    }

    private void Start()
    {
        if (anomalyCategories == null ||
            anomalies == null ||
            anomalyCategories.Length != anomalies.Length)
        {
            Debug.LogError(
                "AnomalyManager: Anomaly Categories must have exactly " +
                "one entry per Anomalies element, in the same order.");
        }

        SpawnAnomaly();
    }

    // Returns false for missing/misconfigured answers, so a setup mistake
    // cannot accidentally mark a player's choice as incorrect.
    public bool TryGetCurrentAnomalyCategory(out AnomalyCategory category)
    {
        category = default(AnomalyCategory);

        if (anomalies == null ||
            anomalyCategories == null ||
            anomalyCategories.Length != anomalies.Length ||
            CurrentAnomalyIndex < 0 ||
            CurrentAnomalyIndex >= anomalies.Length)
        {
            Debug.LogError(
                "AnomalyManager: No active anomaly or the Anomaly Categories " +
                "array is not configured to match the Anomalies array.");
            return false;
        }

        category = anomalyCategories[CurrentAnomalyIndex];
        return true;
    }
    private void SpawnAnomaly()
    {
        if (anomalies == null || anomalies.Length == 0 ||
            spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Anomalies or spawn points are missing.");
            return;
        }

        // Reuse the same anomaly on a retry if it is still valid.
        bool retryPrevious =
            activeAnomalyIndex >= 0 &&
            activeAnomalyIndex < anomalies.Length &&
            anomalies[activeAnomalyIndex] != null &&
            !completedAnomalies.Contains(activeAnomalyIndex);

        if (!retryPrevious)
        {
            List<int> availableAnomalies = new List<int>();

            for (int i = 0; i < anomalies.Length; i++)
            {
                if (anomalies[i] != null &&
                    !completedAnomalies.Contains(i))
                {
                    availableAnomalies.Add(i);
                }
            }

            if (availableAnomalies.Count == 0)
            {
                Debug.Log("All anomalies completed.");
                return;
            }

            int randomIndex = Random.Range(0, availableAnomalies.Count);
            activeAnomalyIndex = availableAnomalies[randomIndex];
            activeSpawnIndex = -1;
        }

        bool validPreviousSpawn =
            activeSpawnIndex >= 0 &&
            activeSpawnIndex < spawnPoints.Length &&
            spawnPoints[activeSpawnIndex] != null;

        if (!validPreviousSpawn)
        {
            List<int> validSpawnIndices = new List<int>();

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] != null)
                    validSpawnIndices.Add(i);
            }

            if (validSpawnIndices.Count == 0)
            {
                Debug.LogError("No valid spawn points found.");
                return;
            }

            activeSpawnIndex = validSpawnIndices[
                Random.Range(0, validSpawnIndices.Count)];
        }

        CurrentAnomalyIndex = activeAnomalyIndex;

        Instantiate(
            anomalies[activeAnomalyIndex],
            spawnPoints[activeSpawnIndex].position,
            Quaternion.identity);

        Debug.Log(
            (retryPrevious ? "Retrying anomaly: " : "New anomaly: ") +
            anomalies[activeAnomalyIndex].name);
    }
    // Called after a correct guess. Returns true if all valid anomalies
    // have been completed; otherwise the next scene load selects a new one.
    public bool CompleteCurrentAnomaly()
    {
        if (CurrentAnomalyIndex < 0)
        {
            Debug.LogError("No active anomaly to complete.");
            return false;
        }

        completedAnomalies.Add(CurrentAnomalyIndex);
        Debug.Log("Completed anomaly index: " + CurrentAnomalyIndex);

        activeAnomalyIndex = -1;
        activeSpawnIndex = -1;
        CurrentAnomalyIndex = -1;

        return !HasUnusedAnomalies();
    }

    public bool HasUnusedAnomalies()
    {
        if (anomalies == null)
            return false;

        for (int i = 0; i < anomalies.Length; i++)
        {
            if (anomalies[i] != null &&
                !completedAnomalies.Contains(i))
                return true;
        }

        return false;
    }

    public int GetCompletedAnomalyCount()
    {
        return completedAnomalies.Count;
    }

    public static void ResetRun()
    {
        completedAnomalies.Clear();
        activeAnomalyIndex = -1;
        activeSpawnIndex = -1;
    }

}



