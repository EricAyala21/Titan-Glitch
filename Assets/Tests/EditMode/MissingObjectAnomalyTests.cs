using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MissingObjectAnomalyTests
{
    private GameObject anomalyGameObject;
    private GameObject targetObject;
    private MissingObjectAnomaly anomaly;

    [SetUp]
    public void Setup()
    {
        anomalyGameObject = new GameObject("TestAnomalyController");
        anomaly = anomalyGameObject.AddComponent<MissingObjectAnomaly>();
        targetObject = new GameObject("TestTargetObject");
        // targetObject is a private SerializeField, so we assign it via reflection
        FieldInfo field = typeof(MissingObjectAnomaly).GetField(
            "targetObject", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(anomaly, targetObject);
    }

    [TearDown]
    public void TearDown()
    {
        // Some tests destroy the target on purpose, so only destroy what still exists
        if (anomalyGameObject != null) Object.DestroyImmediate(anomalyGameObject);
        if (targetObject != null) Object.DestroyImmediate(targetObject);
    }

    // TC-01
    [Test]
    public void ActivateAnomaly_HidesTargetObject()
    {
        anomaly.ActivateAnomaly();
        Assert.IsFalse(targetObject.activeSelf, "Target object should be inactive after ActivateAnomaly().");
    }

    // TC-02
    [Test]
    public void ResetAnomaly_ShowsTargetObject()
    {
        anomaly.ActivateAnomaly();
        anomaly.ResetAnomaly();
        Assert.IsTrue(targetObject.activeSelf, "Target object should be active again after ResetAnomaly().");
    }

    // TC-03
    [Test]
    public void Toggle_SwitchesState()
    {
        bool initialState = targetObject.activeSelf;
        anomaly.Toggle();
        Assert.AreNotEqual(initialState, targetObject.activeSelf, "Toggle() should flip the target object's active state.");
    }

    // TC-04
    [Test]
    public void InitialState_StartResetsAnomalyToNormal()
    {
        // Default state
        Assert.IsFalse(anomaly.IsAnomalous, "Anomaly should start non-anomalous.");
        Assert.IsTrue(targetObject.activeSelf, "Target object should start visible.");

        // Arrange: put the anomaly into an abnormal state
        anomaly.ActivateAnomaly();
        Assert.IsTrue(anomaly.IsAnomalous);
        Assert.IsFalse(targetObject.activeSelf);

        // Act: invoke Unity's private Start() (EditMode doesn't call it automatically)
        MethodInfo startMethod = typeof(MissingObjectAnomaly).GetMethod(
            "Start", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(startMethod, "Start() method was not found.");
        startMethod.Invoke(anomaly, null);

        // Assert: Start() restored the normal state
        Assert.IsFalse(anomaly.IsAnomalous, "Anomaly should be inactive after Start().");
        Assert.IsTrue(targetObject.activeSelf, "Target object should be visible after Start().");
    }

    // TC-05
    [Test]
    public void ActivateTwice_TargetObjectStaysHidden()
    {
        anomaly.ActivateAnomaly();
        anomaly.ActivateAnomaly();
        Assert.IsFalse(targetObject.activeSelf, "Target object should stay hidden after activating twice.");
    }

    // TC-06
    [Test]
    public void ResetWhileInactive_TargetObjectStaysVisible()
    {
        anomaly.ResetAnomaly();
        Assert.IsTrue(targetObject.activeSelf, "Target object should stay visible when reset while already normal.");
    }

    // ---------- SCRUM-85: Edge cases and reset reliability ----------

    // TC-09: Unassigned target object -> warning logged, no exception
    [Test]
    public void UnassignedTarget_LogsWarningAndDoesNotThrow()
    {
        GameObject go = new GameObject("AnomalyWithoutTarget");
        MissingObjectAnomaly unassigned = go.AddComponent<MissingObjectAnomaly>();
        // targetObject is never assigned, so it stays null

        LogAssert.Expect(LogType.Warning, new Regex(".*"));
        Assert.DoesNotThrow(() => unassigned.ActivateAnomaly(),
            "ActivateAnomaly() should not throw when no target is assigned.");

        LogAssert.Expect(LogType.Warning, new Regex(".*"));
        Assert.DoesNotThrow(() => unassigned.ResetAnomaly(),
            "ResetAnomaly() should not throw when no target is assigned.");

        Object.DestroyImmediate(go);
    }

    // TC-10: Target destroyed at runtime -> warning logged, no exception
    [Test]
    public void TargetDestroyedAtRuntime_LogsWarningAndDoesNotThrow()
    {
        Object.DestroyImmediate(targetObject);

        LogAssert.Expect(LogType.Warning, new Regex(".*"));
        Assert.DoesNotThrow(() => anomaly.ActivateAnomaly(),
            "ActivateAnomaly() should not throw after the target is destroyed.");

        LogAssert.Expect(LogType.Warning, new Regex(".*"));
        Assert.DoesNotThrow(() => anomaly.ResetAnomaly(),
            "ResetAnomaly() should not throw after the target is destroyed.");
    }

    // TC-11: 100 rapid toggles -> ends in the correct state
    [Test]
    public void HundredToggles_EndsInNormalState()
    {
        for (int i = 0; i < 100; i++)
        {
            anomaly.Toggle();
        }

        // An even number of toggles should return to normal
        Assert.IsFalse(anomaly.IsAnomalous, "After 100 toggles the anomaly should be back to normal.");
        Assert.IsTrue(targetObject.activeSelf, "After 100 toggles the target should be visible.");

        // One more toggle should make it anomalous again
        anomaly.Toggle();
        Assert.IsTrue(anomaly.IsAnomalous, "After 101 toggles the anomaly should be active.");
        Assert.IsFalse(targetObject.activeSelf, "After 101 toggles the target should be hidden.");
    }

    // TC-12: Target already inactive at start -> reset makes it visible, no error
    [Test]
    public void TargetInactiveAtStart_ResetMakesItVisible()
    {
        targetObject.SetActive(false);

        Assert.DoesNotThrow(() => anomaly.ResetAnomaly(),
            "ResetAnomaly() should not throw when the target starts inactive.");
        Assert.IsTrue(targetObject.activeSelf, "Reset should make an initially inactive target visible.");
        Assert.IsFalse(anomaly.IsAnomalous, "Anomaly should be in the normal state after reset.");
    }
}