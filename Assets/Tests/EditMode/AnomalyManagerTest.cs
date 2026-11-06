using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// EditMode: requires the test assembly to reference the AnomalyManager runtime assembly.
// These tests intentionally invoke spawning through reflection because MonoBehaviour.Start
// does not run automatically in EditMode tests.
[Category("GameplayRegression")]
public class AnomalyManagerTests
{
    private const string Prefix = "TGCoverage_Anomaly_";
    private static readonly MethodInfo SpawnMethod = typeof(AnomalyManager).GetMethod(
        "SpawnAnomaly", BindingFlags.NonPublic | BindingFlags.Instance);

    private readonly List<GameObject> owned = new List<GameObject>();
    private readonly List<GameObject> clones = new List<GameObject>();
    private GameObject[] prefabs;
    private Transform[] spawnPoints;

    [SetUp]
    public void SetUp()
    {
        AnomalyManager.ResetRun();
        Assert.IsNotNull(SpawnMethod, "AnomalyManager.SpawnAnomaly method not found.");
        prefabs = MakePrefabs(3);
        spawnPoints = MakeSpawnPoints(2);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject clone in clones)
            if (clone != null) Object.DestroyImmediate(clone);
        // Also remove untracked clones if a negative duplicate-spawn test was run.
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>())
            if (obj != null && obj.name.StartsWith(Prefix) && obj.name.EndsWith("(Clone)"))
                Object.DestroyImmediate(obj);
        foreach (GameObject obj in owned)
            if (obj != null) Object.DestroyImmediate(obj);
        clones.Clear();
        owned.Clear();
        AnomalyManager.ResetRun();
    }

    [Test]
    public void NewRun_HasZeroCompletedAnomaliesAndUnusedOptions()
    {
        AnomalyManager manager = CreateManager();
        Assert.AreEqual(-1, manager.CurrentAnomalyIndex);
        Assert.AreEqual(0, manager.GetCompletedAnomalyCount());
        Assert.IsTrue(manager.HasUnusedAnomalies());
    }

    [Test]
    public void Spawn_ChoosesValidAnomalyAndAllowedSpawnLocation()
    {
        AnomalyManager manager = CreateManager();
        GameObject clone = Spawn(manager);
        Assert.IsNotNull(clone);
        Assert.That(manager.CurrentAnomalyIndex, Is.InRange(0, 2));
        Assert.IsTrue(AtValidSpawn(clone.transform.position));
        Assert.AreEqual(0, manager.GetCompletedAnomalyCount());
    }

    [Test]
    public void FailureReload_RetainsAnomalyAndLocationWithoutCompletingIt()
    {
        AnomalyManager first = CreateManager();
        GameObject firstClone = Spawn(first);
        int selected = first.CurrentAnomalyIndex;
        Vector3 position = firstClone.transform.position;
        Unload(first, firstClone);

        AnomalyManager retry = CreateManager();
        GameObject retryClone = Spawn(retry);
        Assert.IsNotNull(retryClone);
        Assert.AreEqual(selected, retry.CurrentAnomalyIndex);
        Assert.AreEqual(position, retryClone.transform.position);
        Assert.AreEqual(0, retry.GetCompletedAnomalyCount());
    }

    [Test]
    public void SuccessfulProgression_ExcludesPreviouslyCompletedAnomaly()
    {
        AnomalyManager first = CreateManager();
        GameObject clone = Spawn(first);
        int original = first.CurrentAnomalyIndex;
        Assert.IsFalse(first.CompleteCurrentAnomaly());
        Assert.AreEqual(-1, first.CurrentAnomalyIndex);
        Assert.AreEqual(1, first.GetCompletedAnomalyCount());
        Unload(first, clone);

        AnomalyManager second = CreateManager();
        Assert.IsNotNull(Spawn(second));
        Assert.AreNotEqual(original, second.CurrentAnomalyIndex);
    }

    [Test]
    public void FullRun_EachValidAnomalyOccursExactlyOnceAndLastCompletesGame()
    {
        HashSet<int> visited = new HashSet<int>();
        for (int i = 0; i < 3; i++)
        {
            AnomalyManager manager = CreateManager();
            GameObject clone = Spawn(manager);
            Assert.IsNotNull(clone);
            Assert.IsTrue(visited.Add(manager.CurrentAnomalyIndex),
                "A completed anomaly was reused during a successful run.");
            bool done = manager.CompleteCurrentAnomaly();
            Assert.AreEqual(i == 2, done, "Only the final completion should end the run.");
            Assert.AreEqual(i + 1, manager.GetCompletedAnomalyCount());
            Unload(manager, clone);
        }
        Assert.AreEqual(3, visited.Count);
        AnomalyManager afterCompletion = CreateManager();
        Assert.IsFalse(afterCompletion.HasUnusedAnomalies());
        LogAssert.Expect(LogType.Log, "All anomalies completed.");
        Assert.IsNull(Spawn(afterCompletion));
    }

    [Test]
    public void CompletingSingleRemainingAnomaly_ReturnsTrue()
    {
        prefabs = new[] { prefabs[0] };
        AnomalyManager manager = CreateManager();
        Assert.IsNotNull(Spawn(manager));
        Assert.IsTrue(manager.CompleteCurrentAnomaly());
        Assert.AreEqual(-1, manager.CurrentAnomalyIndex);
        Assert.IsFalse(manager.HasUnusedAnomalies());
    }

    [Test]
    public void CompletingWithoutSpawn_LogsErrorAndDoesNotAdvance()
    {
        AnomalyManager manager = CreateManager();
        LogAssert.Expect(LogType.Error, "No active anomaly to complete.");
        Assert.IsFalse(manager.CompleteCurrentAnomaly());
        Assert.AreEqual(0, manager.GetCompletedAnomalyCount());
    }

    [Test]
    public void DoubleCompletion_IsRejectedInsteadOfIncreasingProgress()
    {
        AnomalyManager manager = CreateManager();
        Assert.IsNotNull(Spawn(manager));
        manager.CompleteCurrentAnomaly();
        LogAssert.Expect(LogType.Error, "No active anomaly to complete.");
        Assert.IsFalse(manager.CompleteCurrentAnomaly());
        Assert.AreEqual(1, manager.GetCompletedAnomalyCount());
    }

    [Test]
    public void ResetRun_ClearsCompletedHistoryAndPermitsFreshSelection()
    {
        prefabs = new[] { prefabs[0] };
        AnomalyManager first = CreateManager();
        GameObject clone = Spawn(first);
        Assert.IsTrue(first.CompleteCurrentAnomaly());
        Unload(first, clone);

        AnomalyManager.ResetRun();
        AnomalyManager next = CreateManager();
        Assert.AreEqual(0, next.GetCompletedAnomalyCount());
        Assert.IsNotNull(Spawn(next));
        Assert.AreEqual(0, next.CurrentAnomalyIndex);
    }

    [Test]
    public void ResetRun_AlsoDiscardsPendingRetrySelection()
    {
        AnomalyManager first = CreateManager();
        GameObject clone = Spawn(first);
        Unload(first, clone);
        AnomalyManager.ResetRun();

        // After reset, previously active anomaly is no longer specially reserved.
        AnomalyManager next = CreateManager();
        Assert.AreEqual(0, next.GetCompletedAnomalyCount());
        Assert.IsNotNull(Spawn(next));
        Assert.IsTrue(next.HasUnusedAnomalies());
    }

    [TestCase(true)]
    [TestCase(false)]
    public void MissingDependencyArray_LogsErrorWithoutSpawning(bool missingAnomalies)
    {
        AnomalyManager manager = CreateManager();
        if (missingAnomalies) manager.anomalies = null;
        else manager.spawnPoints = null;
        LogAssert.Expect(LogType.Error, "Anomalies or spawn points are missing.");
        Assert.IsNull(Spawn(manager));
        Assert.AreEqual(-1, manager.CurrentAnomalyIndex);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void EmptyDependencyArray_LogsErrorWithoutSpawning(bool emptyAnomalies)
    {
        AnomalyManager manager = CreateManager();
        if (emptyAnomalies) manager.anomalies = new GameObject[0];
        else manager.spawnPoints = new Transform[0];
        LogAssert.Expect(LogType.Error, "Anomalies or spawn points are missing.");
        Assert.IsNull(Spawn(manager));
        Assert.AreEqual(-1, manager.CurrentAnomalyIndex);
    }

    [Test]
    public void AllNullPrefabs_AreConsideredUnavailable()
    {
        AnomalyManager manager = CreateManager();
        manager.anomalies = new GameObject[] { null, null };
        Assert.IsFalse(manager.HasUnusedAnomalies());
        LogAssert.Expect(LogType.Log, "All anomalies completed.");
        Assert.IsNull(Spawn(manager));
    }

    [Test]
    public void AllNullSpawnPoints_ProduceExplicitError()
    {
        AnomalyManager manager = CreateManager();
        manager.spawnPoints = new Transform[] { null, null };
        LogAssert.Expect(LogType.Error, "No valid spawn points found.");
        Assert.IsNull(Spawn(manager));
        Assert.AreEqual(-1, manager.CurrentAnomalyIndex);
    }

    [Test]
    public void NullPrefabEntries_AreSkipped()
    {
        AnomalyManager manager = CreateManager();
        manager.anomalies = new GameObject[] { null, prefabs[1], null };
        Assert.IsNotNull(Spawn(manager));
        Assert.AreEqual(1, manager.CurrentAnomalyIndex);
    }

    [Test]
    public void NullSpawnEntries_AreSkipped()
    {
        AnomalyManager manager = CreateManager();
        manager.spawnPoints = new Transform[] { null, spawnPoints[1] };
        GameObject clone = Spawn(manager);
        Assert.IsNotNull(clone);
        Assert.AreEqual(spawnPoints[1].position, clone.transform.position);
    }

    [Test]
    public void DeletedPendingAnomaly_IsReplacedByRemainingValidAnomaly()
    {
        prefabs = new[] { prefabs[0], prefabs[1] };
        AnomalyManager first = CreateManager();
        GameObject clone = Spawn(first);
        int selected = first.CurrentAnomalyIndex;
        Unload(first, clone);
        Object.DestroyImmediate(prefabs[selected]);
        prefabs[selected] = null;

        AnomalyManager next = CreateManager();
        Assert.IsNotNull(Spawn(next));
        Assert.AreEqual(1 - selected, next.CurrentAnomalyIndex);
    }

    [Test]
    public void HasUnusedAnomalies_WithNullArray_ReturnsFalse()
    {
        AnomalyManager manager = CreateManager();
        manager.anomalies = null;
        Assert.IsFalse(manager.HasUnusedAnomalies());
    }

    // This documents a desired anti-duplication requirement; the current implementation
    // instantiates again when SpawnAnomaly() is called twice without a scene reload.
    [Test, Explicit("Known gap: SpawnAnomaly currently permits two active clones.")]
    [Category("KnownGap")]
    public void RepeatedSpawnCall_ShouldNotCreateTwoActiveClones()
    {
        AnomalyManager manager = CreateManager();
        Assert.IsNotNull(Spawn(manager));
        int selected = manager.CurrentAnomalyIndex;
        GameObject firstClone = GameObject.Find(prefabs[selected].name + "(Clone)");
        SpawnMethod.Invoke(manager, null);
        GameObject[] all = Object.FindObjectsByType<GameObject>();
        int count = 0;
        foreach (GameObject obj in all)
            if (obj.name == prefabs[selected].name + "(Clone)") count++;
        Assert.AreEqual(1, count, "Repeated spawning created a duplicate instance.");
        Assert.IsNotNull(firstClone);
    }

    private AnomalyManager CreateManager()
    {
        GameObject obj = Own(new GameObject(Prefix + "Manager"));
        AnomalyManager manager = obj.AddComponent<AnomalyManager>();
        manager.anomalies = prefabs;
        manager.spawnPoints = spawnPoints;
        return manager;
    }

    private GameObject Spawn(AnomalyManager manager)
    {
        SpawnMethod.Invoke(manager, null);
        if (manager.anomalies == null) return null;
        foreach (GameObject prefab in manager.anomalies)
        {
            if (prefab == null) continue;
            GameObject clone = GameObject.Find(prefab.name + "(Clone)");
            if (clone != null && !clones.Contains(clone))
            {
                clones.Add(clone);
                return clone;
            }
        }
        return null;
    }

    private void Unload(AnomalyManager manager, GameObject clone)
    {
        if (clone != null) Object.DestroyImmediate(clone);
        if (manager != null) Object.DestroyImmediate(manager.gameObject);
    }

    private bool AtValidSpawn(Vector3 position)
    {
        foreach (Transform point in spawnPoints)
            if (point != null && point.position == position) return true;
        return false;
    }

    private GameObject[] MakePrefabs(int count)
    {
        GameObject[] values = new GameObject[count];
        for (int i = 0; i < count; i++) values[i] = Own(new GameObject(Prefix + "Prefab" + i));
        return values;
    }

    private Transform[] MakeSpawnPoints(int count)
    {
        Transform[] values = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Own(new GameObject(Prefix + "Spawn" + i));
            obj.transform.position = new Vector3(i * 10f, 0f, i * 4f);
            values[i] = obj.transform;
        }
        return values;
    }

    private GameObject Own(GameObject obj)
    {
        owned.Add(obj);
        return obj;
    }
}
