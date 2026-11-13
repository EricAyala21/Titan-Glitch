using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// SCRUM-87: Verify anomaly spawning/activation through the real AnomalyManager in MainLevel.
public class AnomalyManagerSceneTests
{
    private const string SceneName = "MainLevel";
    private static readonly MethodInfo SpawnMethod = typeof(AnomalyManager).GetMethod(
        "SpawnAnomaly", BindingFlags.NonPublic | BindingFlags.Instance);

    private AnomalyManager manager;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        AnomalyManager.ResetRun(); // static progress would otherwise carry over between tests
        AsyncOperation load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
        Assert.IsNotNull(load, $"Could not load '{SceneName}'. Is it in the Build Profiles scene list?");
        while (!load.isDone) yield return null;
        yield return null; // manager.Start() spawns an anomaly
        yield return null; // the spawned anomaly's own Start() runs

        manager = Object.FindFirstObjectByType<AnomalyManager>();
        Assert.IsNotNull(manager, "No AnomalyManager found in MainLevel.");
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        AnomalyManager.ResetRun();
        yield return null;
    }

    // TC-87-03: Loading MainLevel spawns exactly one anomaly, matching the manager's selection,
    // at one of its spawn points.
    [UnityTest]
    public IEnumerator MainLevel_SpawnsExactlyOneAnomalyAtASpawnPoint()
    {
        Assert.That(manager.CurrentAnomalyIndex, Is.InRange(0, manager.anomalies.Length - 1),
            "Manager did not select an anomaly.");
        GameObject expected = manager.anomalies[manager.CurrentAnomalyIndex];

        int cloneCount = 0;
        GameObject clone = null;
        foreach (GameObject prefab in manager.anomalies)
        {
            if (prefab == null) continue;
            foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (obj.name == prefab.name + "(Clone)") { cloneCount++; clone = obj; }
            }
        }

        Assert.AreEqual(1, cloneCount, "Expected exactly one spawned anomaly in the scene.");
        Assert.AreEqual(expected.name + "(Clone)", clone.name,
            "Spawned anomaly does not match CurrentAnomalyIndex.");

        // ChairAnomaly can start moving toward the player, so allow a small tolerance.
        bool atSpawnPoint = false;
        foreach (Transform point in manager.spawnPoints)
            if (point != null && Vector3.Distance(point.position, clone.transform.position) < 1f)
                atSpawnPoint = true;
        Assert.IsTrue(atSpawnPoint, "Spawned anomaly is not at one of the manager's spawn points.");
        yield return null;
    }

    // TC-87-04: Each real AnomalyBase prefab is active after the manager spawns it in MainLevel.
    // (Prefabs without AnomalyBase, e.g. ChairAnomaly, are skipped here; see TC-87-01.)
    [UnityTest]
    public IEnumerator EachRealAnomaly_IsActiveAfterManagerSpawnsIt()
    {
        Assert.IsNotNull(SpawnMethod, "AnomalyManager.SpawnAnomaly method not found.");
        GameObject[] realPrefabs = manager.anomalies;
        int checkedCount = 0;

        foreach (GameObject prefab in realPrefabs)
        {
            if (prefab == null || prefab.GetComponentInChildren<AnomalyBase>(true) == null) continue;

            DestroyClones(realPrefabs);
            yield return null; // let Destroy() finish before spawning again
            AnomalyManager.ResetRun();
            manager.anomalies = new[] { prefab };
            SpawnMethod.Invoke(manager, null);
            yield return null; // clone's Start() runs
            yield return null;

            GameObject clone = GameObject.Find(prefab.name + "(Clone)");
            Assert.IsNotNull(clone, $"Manager did not spawn '{prefab.name}'.");
            Assert.IsTrue(clone.GetComponentInChildren<AnomalyBase>().IsActive,
                $"'{prefab.name}' was spawned by the manager but is not active.");
            checkedCount++;
        }

        manager.anomalies = realPrefabs;
        Assert.Greater(checkedCount, 0, "No AnomalyBase prefabs were found in the manager's list.");
    }

    // Destroys every spawned anomaly clone once. No while-loop, so it can never spin forever.
    private static void DestroyClones(GameObject[] prefabs)
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (obj == null) continue;
            foreach (GameObject prefab in prefabs)
            {
                if (prefab != null && obj.name == prefab.name + "(Clone)")
                {
                    Object.Destroy(obj);
                    break;
                }
            }
        }
    }
}