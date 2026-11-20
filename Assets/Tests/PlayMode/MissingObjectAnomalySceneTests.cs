using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// SCRUM-83: Test MissingObjectAnomaly against a real object in the MainLevel scene.
// MissingObjectAnomaly is not currently spawned by AnomalyManager, so the test attaches it
// at runtime to an existing scene prop. The scene file itself is not modified.
public class MissingObjectAnomalySceneTests
{
    private const string SceneName = "MainLevel";
    private const string TargetName = "(Prb)OfficeChair";

    private GameObject target;
    private GameObject controller;
    private MissingObjectAnomaly anomaly;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
        Assert.IsNotNull(load, $"Could not load '{SceneName}'. Is it in the Build Profiles scene list?");
        while (!load.isDone) yield return null;
        yield return null; // let the scene's own Awake/Start run

        target = GameObject.Find(TargetName);
        Assert.IsNotNull(target, $"'{TargetName}' was not found in {SceneName}.");

        controller = new GameObject("SceneTest_MissingObjectAnomaly");
        anomaly = controller.AddComponent<MissingObjectAnomaly>();
        // targetObject is a private SerializeField, so assign it via reflection
        typeof(MissingObjectAnomaly)
            .GetField("targetObject", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(anomaly, target);

        yield return null; // Start() runs and puts the anomaly in its normal state
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (controller != null) Object.Destroy(controller);
        yield return CleanUpScene();
    }

    // Leaves a clean, empty scene behind so later test classes don't see MainLevel's
    // Player/GameManager/PlayerInput, and undoes any Time.timeScale change.
    private static IEnumerator CleanUpScene()
    {
        Time.timeScale = 1f;
        Scene clean = SceneManager.CreateScene("TestCleanScene_" + System.Guid.NewGuid().ToString("N"));
        SceneManager.SetActiveScene(clean);
        Scene level = SceneManager.GetSceneByName(SceneName);
        if (level.IsValid() && level.isLoaded)
            yield return SceneManager.UnloadSceneAsync(level);
        yield return null;
    }

    // TC-S83-01: Scene loads and the real prop starts visible
    [UnityTest]
    public IEnumerator SceneLoaded_TargetStartsVisible()
    {
        Assert.AreEqual(SceneName, SceneManager.GetActiveScene().name);
        Assert.IsFalse(anomaly.IsAnomalous, "Anomaly should start in the normal state.");
        Assert.IsTrue(target.activeInHierarchy, "Target prop should be visible at start.");
        yield return null;
    }

    // TC-S83-02: Activating hides the real prop, including its meshes and colliders
    [UnityTest]
    public IEnumerator ActivateAnomaly_HidesSceneObjectRenderersAndColliders()
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        Collider[] colliders = target.GetComponentsInChildren<Collider>();
        Assert.IsNotEmpty(renderers, "Target prop should have at least one renderer.");

        anomaly.ActivateAnomaly();
        yield return null;

        Assert.IsTrue(anomaly.IsAnomalous, "Anomaly should be active.");
        Assert.IsFalse(target.activeInHierarchy, "Target prop should be hidden.");
        foreach (Renderer r in renderers)
            Assert.IsFalse(r.gameObject.activeInHierarchy, $"Renderer on '{r.name}' should be hidden.");
        foreach (Collider c in colliders)
            Assert.IsFalse(c.gameObject.activeInHierarchy, $"Collider on '{c.name}' should be disabled.");
    }

    // TC-S83-03: Reset brings the prop back in the same place, unchanged
    [UnityTest]
    public IEnumerator ResetAnomaly_RestoresSceneObjectUnchanged()
    {
        Transform t = target.transform;
        Vector3 pos = t.position;
        Quaternion rot = t.rotation;
        Vector3 scale = t.localScale;
        int childCount = t.childCount;
        int rendererCount = target.GetComponentsInChildren<Renderer>().Length;

        anomaly.ActivateAnomaly();
        yield return null;
        anomaly.ResetAnomaly();
        yield return null;

        Assert.IsFalse(anomaly.IsAnomalous, "Anomaly should be back to normal.");
        Assert.IsTrue(target.activeInHierarchy, "Target prop should be visible again.");
        Assert.Less(Vector3.Distance(pos, t.position), 0.01f, "Position changed.");
        Assert.Less(Quaternion.Angle(rot, t.rotation), 0.5f, "Rotation changed.");
        Assert.AreEqual(scale, t.localScale, "Scale changed.");
        Assert.AreEqual(childCount, t.childCount, "Child count changed.");
        Assert.AreEqual(rendererCount, target.GetComponentsInChildren<Renderer>().Length,
            "Renderer count changed.");
    }

    // TC-S83-04: Repeated activate/reset across frames stays consistent in the live scene
    [UnityTest]
    public IEnumerator RepeatedActivateReset_StaysConsistentAcrossFrames()
    {
        for (int i = 0; i < 5; i++)
        {
            anomaly.ActivateAnomaly();
            yield return null;
            Assert.IsFalse(target.activeInHierarchy, $"Cycle {i}: target should be hidden.");

            anomaly.ResetAnomaly();
            yield return null;
            Assert.IsTrue(target.activeInHierarchy, $"Cycle {i}: target should be visible.");
        }
    }
}