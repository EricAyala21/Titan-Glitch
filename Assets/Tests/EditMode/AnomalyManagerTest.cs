using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AnomalyManagerTests
{
    private readonly List<GameObject> testObjects = new List<GameObject>();
    private readonly List<GameObject> spawnedClones = new List<GameObject>();
    private GameObject[] prefabs;
    private Transform[] spawnPoints;

    private static readonly MethodInfo SpawnMethod = typeof(AnomalyManager).GetMethod(
        "SpawnAnomaly", BindingFlags.Instance | BindingFlags.NonPublic);

    [SetUp]
    public void SetUp()
    {
        AnomalyManager.ResetRun();
        prefabs = CreatePrefabs(3);
        spawnPoints = CreateSpawnPoints(2);
        Assert.IsNotNull(SpawnMethod,
            "Expected a private SpawnAnomaly() method on AnomalyManager.");
    }
    [TearDown]
    public void TearDown()
    {
        // Destroy the objects created by Instantiate() before their source prefabs.
        foreach (GameObject clone in spawnedClones)
        {
            if (clone != null)
                Object.DestroyImmediate(clone);
        }

        foreach (GameObject obj in testObjects)
        {
            if (obj != null)
                Object.DestroyImmediate(obj);
        }

        spawnedClones.Clear();
        testObjects.Clear();
        AnomalyManager.ResetRun();
    }

    [Test]
    public void SpawnAnomaly_FirstLevel_SpawnsValidAnomaly()
    {
        AnomalyManager manager = CreateManager();

        GameObject clone = SpawnAndFindClone(manager);

        Assert.IsNotNull(clone, "An anomaly instance should be created.");
        Assert.That(manager.CurrentAnomalyIndex,
            Is.InRange(0, prefabs.Length - 1));
        Assert.AreEqual(0, manager.GetCompletedAnomalyCount());
        Assert.IsTrue(manager.HasUnusedAnomalies());
        Assert.IsTrue(IsAtValidSpawnPoint(clone.transform.position));
    }
    [Test]
    public void SpawnAnomaly_AfterFailedAttempt_RetriesSameAnomalyAndLocation()
    {
        AnomalyManager firstManager = CreateManager();
        GameObject firstClone = SpawnAndFindClone(firstManager);
        Assert.IsNotNull(firstClone);

        int originalIndex = firstManager.CurrentAnomalyIndex;
        Vector3 originalPosition = firstClone.transform.position;

        // Simulate the old scene being unloaded without completing the anomaly.
        Object.DestroyImmediate(firstClone);
        Object.DestroyImmediate(firstManager.gameObject);

        AnomalyManager reloadedManager = CreateManager();
        GameObject retryClone = SpawnAndFindClone(reloadedManager);

        Assert.IsNotNull(retryClone);
        Assert.AreEqual(originalIndex, reloadedManager.CurrentAnomalyIndex);
        Assert.AreEqual(originalPosition, retryClone.transform.position);
        Assert.AreEqual(0, reloadedManager.GetCompletedAnomalyCount());
    }

    [Test]
    public void CompleteCurrentAnomaly_ThenSpawn_SelectsDifferentUnusedAnomaly()
    {
        AnomalyManager firstManager = CreateManager();
        GameObject firstClone = SpawnAndFindClone(firstManager);
        Assert.IsNotNull(firstClone);
        int firstIndex = firstManager.CurrentAnomalyIndex;

        bool allCompleted = firstManager.CompleteCurrentAnomaly();
        Assert.IsFalse(allCompleted, "Two unused anomalies should still remain.");
        Assert.AreEqual(1, firstManager.GetCompletedAnomalyCount());

        // Simulate a successful level transition and scene reload.
        Object.DestroyImmediate(firstClone);
        Object.DestroyImmediate(firstManager.gameObject);

        AnomalyManager nextManager = CreateManager();
        GameObject nextClone = SpawnAndFindClone(nextManager);

        Assert.IsNotNull(nextClone);
        Assert.AreNotEqual(firstIndex, nextManager.CurrentAnomalyIndex,
            "A completed anomaly must not be selected again.");
    }

    [Test]
    public void CompleteCurrentAnomaly_WhenLastAnomaly_ReturnsTrue()
    {
        // Only one valid anomaly is available in this run.
        prefabs = new[] { prefabs[0] };
        AnomalyManager manager = CreateManager();
        Assert.IsNotNull(SpawnAndFindClone(manager));

        bool allCompleted = manager.CompleteCurrentAnomaly();

        Assert.IsTrue(allCompleted);
        Assert.AreEqual(1, manager.GetCompletedAnomalyCount());
        Assert.IsFalse(manager.HasUnusedAnomalies());
        Assert.AreEqual(-1, manager.CurrentAnomalyIndex);
    }

    [Test]
    public void SpawnAnomaly_AfterAllCompleted_DoesNotSpawnAgain()
    {
        prefabs = new[] { prefabs[0] };
        AnomalyManager firstManager = CreateManager();
        GameObject firstClone = SpawnAndFindClone(firstManager);
        Assert.IsNotNull(firstClone);
        Assert.IsTrue(firstManager.CompleteCurrentAnomaly());

        Object.DestroyImmediate(firstClone);
        Object.DestroyImmediate(firstManager.gameObject);

        AnomalyManager nextManager = CreateManager();
        LogAssert.Expect(LogType.Log, "All anomalies completed.");
        GameObject nextClone = SpawnAndFindClone(nextManager);

        Assert.IsNull(nextClone);
        Assert.AreEqual(-1, nextManager.CurrentAnomalyIndex);
    }
    [Test]
    public void ResetRun_AfterCompletion_AllowsSameAnomalyAgain()
    {
        prefabs = new[] { prefabs[0] };
        AnomalyManager firstManager = CreateManager();
        GameObject firstClone = SpawnAndFindClone(firstManager);
        Assert.IsNotNull(firstClone);
        Assert.IsTrue(firstManager.CompleteCurrentAnomaly());

        Object.DestroyImmediate(firstClone);
        Object.DestroyImmediate(firstManager.gameObject);
        AnomalyManager.ResetRun();

        AnomalyManager newRunManager = CreateManager();
        GameObject newClone = SpawnAndFindClone(newRunManager);

        Assert.IsNotNull(newClone);
        Assert.AreEqual(0, newRunManager.CurrentAnomalyIndex);
        Assert.AreEqual(0, newRunManager.GetCompletedAnomalyCount());
    }

    [Test]
    public void CompleteCurrentAnomaly_WithoutActiveAnomaly_ReturnsFalse()
    {
        AnomalyManager manager = CreateManager();
        LogAssert.Expect(LogType.Error, "No active anomaly to complete.");

        bool result = manager.CompleteCurrentAnomaly();

        Assert.IsFalse(result);
        Assert.AreEqual(0, manager.GetCompletedAnomalyCount());
    }

    [Test]
    public void SpawnAnomaly_WithNoAnomalies_LogsErrorAndDoesNotSpawn()
    {
        prefabs = new GameObject[0];
        AnomalyManager manager = CreateManager();
        LogAssert.Expect(LogType.Error,
            "Anomalies or spawn points are missing.");

        Assert.IsNull(SpawnAndFindClone(manager));
        Assert.AreEqual(-1, manager.CurrentAnomalyIndex);
    }
    [Test]
    public void SpawnAnomaly_WithNoSpawnPoints_LogsErrorAndDoesNotSpawn()
    {
        spawnPoints = new Transform[0];
        AnomalyManager manager = CreateManager();
        LogAssert.Expect(LogType.Error,
            "Anomalies or spawn points are missing.");

        Assert.IsNull(SpawnAndFindClone(manager));
        Assert.AreEqual(-1, manager.CurrentAnomalyIndex);
    }

    [Test]
    public void SpawnAnomaly_WithOnlyNullSpawnPoints_LogsError()
    {
        spawnPoints = new Transform[] { null };
        AnomalyManager manager = CreateManager();
        LogAssert.Expect(LogType.Error, "No valid spawn points found.");

        Assert.IsNull(SpawnAndFindClone(manager));
        Assert.AreEqual(-1, manager.CurrentAnomalyIndex);
    }

    [Test]
    public void SpawnAnomaly_IgnoresNullPrefabEntries()
    {
        prefabs = new GameObject[] { null, prefabs[1], null };
        AnomalyManager manager = CreateManager();

        GameObject clone = SpawnAndFindClone(manager);

        Assert.IsNotNull(clone);
        Assert.AreEqual(1, manager.CurrentAnomalyIndex);
    }
    [Test]
    public void SpawnAnomaly_IgnoresNullSpawnPoints()
    {
        spawnPoints = new Transform[] { null, spawnPoints[1] };
        AnomalyManager manager = CreateManager();

        GameObject clone = SpawnAndFindClone(manager);

        Assert.IsNotNull(clone);
        Assert.AreEqual(spawnPoints[1].position, clone.transform.position);
    }

    [Test]
    public void HasUnusedAnomalies_WithEmptyArray_ReturnsFalse()
    {
        prefabs = new GameObject[0];
        AnomalyManager manager = CreateManager();

        Assert.IsFalse(manager.HasUnusedAnomalies());
    }

    private AnomalyManager CreateManager()
    {
        GameObject obj = Track(new GameObject("AnomalyUnitTest_Manager"));
        AnomalyManager manager = obj.AddComponent<AnomalyManager>();
        manager.anomalies = prefabs;
        manager.spawnPoints = spawnPoints;
        return manager;
    }

    private GameObject[] CreatePrefabs(int count)
    {
        GameObject[] result = new GameObject[count];
        for (int i = 0; i < count; i++)
            result[i] = Track(new GameObject("AnomalyUnitTest_Prefab_" + i));
        return result;
    }

    private Transform[] CreateSpawnPoints(int count)
    {
        Transform[] result = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Track(new GameObject("AnomalyUnitTest_Spawn_" + i));
            obj.transform.position = new Vector3(i * 10f, 0f, i * 5f);
            result[i] = obj.transform;
        }
        return result;
    }

    private GameObject Track(GameObject obj)
    {
        testObjects.Add(obj);
        return obj;
    }
    // Unity doesn't call MonoBehaviour.Start() in Edit Mode tests,
    // so invoke the private spawning method explicitly.
    private GameObject SpawnAndFindClone(AnomalyManager manager)
    {
        SpawnMethod.Invoke(manager, null);

        // Each test ensures there's at most one live anomaly instance.
        foreach (GameObject prefab in manager.anomalies)
        {
            if (prefab == null)
                continue;

            GameObject clone = GameObject.Find(prefab.name + "(Clone)");
            if (clone != null && !spawnedClones.Contains(clone))
            {
                spawnedClones.Add(clone);
                return clone;
            }
        }
        return null;
    }

    private bool IsAtValidSpawnPoint(Vector3 position)
    {
        foreach (Transform point in spawnPoints)
        {
            if (point != null && point.position == position)
                return true;
        }
        return false;
    }
}
