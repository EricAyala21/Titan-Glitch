using System.Collections.Generic;
using UnityEngine;

public class AnomalyManager : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject[] anomalies;

    // Completed anomalies persist across scene reloads.
    private static HashSet<int> completedAnomalies =
        new HashSet<int>();

    // Current anomaly and spawn location persist on failure.
    private static int activeAnomalyIndex = -1;
    private static int activeSpawnIndex = -1;

    public int CurrentAnomalyIndex { get; private set; } = -1;

    // Reset static data at the beginning of a new Play session.
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitializeRun()
    {
        ResetRun();
    }

    void Start()
    {
        SpawnAnomaly();
    }

    private void SpawnAnomaly()
    {
        if (anomalies == null || anomalies.Length == 0 ||
            spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Anomalies or spawn points are missing.");
            return;
        }

        // Check if the previous anomaly is valid for a retry.
        // Is there a previous anomaly that exists, is still valid, and has not already been completed?
        bool retryPrevious =
            activeAnomalyIndex >= 0 &&
            activeAnomalyIndex < anomalies.Length &&
            anomalies[activeAnomalyIndex] != null &&
            !completedAnomalies.Contains(activeAnomalyIndex);

        // find a new anomaly to use.
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

            // Select a new anomaly that hasn't been completed.
            int randomIndex =
                Random.Range(0, availableAnomalies.Count);

            activeAnomalyIndex = availableAnomalies[randomIndex];
            activeSpawnIndex = -1;
        }

        // Reuse the previous spawn location when possible.
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
                {
                    validSpawnIndices.Add(i);
                }
            }

            if (validSpawnIndices.Count == 0)
            {
                Debug.LogError("No valid spawn points found.");
                return;
            }

            activeSpawnIndex = validSpawnIndices[
                Random.Range(0, validSpawnIndices.Count)
            ];
        }

        CurrentAnomalyIndex = activeAnomalyIndex;

        Instantiate(
            anomalies[activeAnomalyIndex],
            spawnPoints[activeSpawnIndex].position,
            Quaternion.identity
        );

        Debug.Log(
            (retryPrevious ? "Retrying anomaly: " : "New anomaly: ") +
            anomalies[activeAnomalyIndex].name
        );
    }

    // Called when the player successfully completes the level.
    // Returns true when all valid anomalies are completed.
    public bool CompleteCurrentAnomaly()
    {
        if (CurrentAnomalyIndex < 0)
        {
            Debug.LogError("No active anomaly to complete.");
            return false;
        }

        completedAnomalies.Add(CurrentAnomalyIndex);

        Debug.Log("Completed anomaly index: " +
                  CurrentAnomalyIndex);

        // Clear current selection so the next level gets a new one.
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
            {
                return true;
            }
        }

        return false;
    }

    public int GetCompletedAnomalyCount()
    {
        return completedAnomalies.Count;
    }

    // Start a completely new playthrough.
    public static void ResetRun()
    {
        completedAnomalies.Clear();
        activeAnomalyIndex = -1;
        activeSpawnIndex = -1;
    }
}
