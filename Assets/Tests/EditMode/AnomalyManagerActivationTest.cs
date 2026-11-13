using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// SCRUM-87: Verify anomaly activation via AnomalyManager (EditMode, known gaps).
public class AnomalyManagerActivationTests
{
    private const string AnomalyPrefabFolder = "Assets/Prefabs/Anomalies";
    private static readonly MethodInfo SpawnMethod = typeof(AnomalyManager).GetMethod(
        "SpawnAnomaly", BindingFlags.NonPublic | BindingFlags.Instance);

    private readonly List<GameObject> created = new List<GameObject>();

    [SetUp]
    public void SetUp()
    {
        AnomalyManager.ResetRun();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject obj in created)
            if (obj != null) Object.DestroyImmediate(obj);
        created.Clear();
        AnomalyManager.ResetRun();
    }

    // TC-87-01: Every anomaly prefab should be an AnomalyBase so the game can activate/reset it.
    [Test, Explicit("Known gap: ChairAnomaly does not derive from AnomalyBase.")]
    [Category("KnownGap")]
    public void AllAnomalyPrefabs_HaveAnomalyBaseComponent()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { AnomalyPrefabFolder });
        Assert.IsNotEmpty(guids, "No anomaly prefabs found in " + AnomalyPrefabFolder);

        List<string> missing = new List<string>();
        foreach (string guid in guids)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            if (prefab.GetComponentInChildren<AnomalyBase>(true) == null)
                missing.Add(prefab.name);
        }

        Assert.IsEmpty(missing,
            "Anomaly prefabs without an AnomalyBase component (cannot be activated or reset): "
            + string.Join(", ", missing));
    }

    // TC-87-02: The manager should activate what it spawns, not rely on the anomaly's own Start().
    [Test, Explicit("Known gap: AnomalyManager spawns anomalies but never calls ActivateAnomaly().")]
    [Category("KnownGap")]
    public void SpawnedAnomaly_IsActivatedByManager()
    {
        Assert.IsNotNull(SpawnMethod, "AnomalyManager.SpawnAnomaly method not found.");

        GameObject prefab = Track(new GameObject("SCRUM87_InactivePrefab"));
        prefab.AddComponent<InactiveTestAnomaly>();
        GameObject spawn = Track(new GameObject("SCRUM87_Spawn"));
        AnomalyManager manager = Track(new GameObject("SCRUM87_Manager")).AddComponent<AnomalyManager>();
        manager.anomalies = new[] { prefab };
        manager.spawnPoints = new[] { spawn.transform };

        SpawnMethod.Invoke(manager, null);

        GameObject clone = Track(GameObject.Find("SCRUM87_InactivePrefab(Clone)"));
        Assert.IsNotNull(clone, "Manager did not spawn the anomaly.");
        Assert.IsTrue(clone.GetComponent<AnomalyBase>().IsActive,
            "AnomalyManager spawned the anomaly but did not call ActivateAnomaly().");
    }

    private GameObject Track(GameObject obj)
    {
        created.Add(obj);
        return obj;
    }
}