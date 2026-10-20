using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class MissingObjectAnomalyTests
{
    private GameObject anomalyGameObject;
    private GameObject targetObject;
    private MissingObjectAnomaly anomaly;

    [SetUp]
    public void Setup()
    {   // Cr
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
        Object.DestroyImmediate(anomalyGameObject);
        Object.DestroyImmediate(targetObject);
    }

    [Test]
    public void ActivateAnomaly_HidesTargetObject()
    {
        anomaly.ActivateAnomaly();
        Assert.IsFalse(targetObject.activeSelf, "Target object should be inactive after ActivateAnomaly().");
    }

    [Test]
    public void ResetAnomaly_ShowsTargetObject()
    {
        anomaly.ActivateAnomaly();
        anomaly.ResetAnomaly();
        Assert.IsTrue(targetObject.activeSelf, "Target object should be active again after ResetAnomaly().");
    }

    [Test]
    public void Toggle_SwitchesState()
    {
        bool initialState = targetObject.activeSelf;
        anomaly.Toggle();
        Assert.AreNotEqual(initialState, targetObject.activeSelf, "Toggle() should flip the target object's active state.");
    }

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

    [Test]
    public void ActivateTwice_TargetObjectStaysHidden()
    {
        anomaly.ActivateAnomaly();
        anomaly.ActivateAnomaly();
        Assert.IsFalse(targetObject.activeSelf, "Target object should stay hidden after activating twice.");
    }

    [Test]
    public void ResetWhileInactive_TargetObjectStaysVisible()
    {
        anomaly.ResetAnomaly();
        Assert.IsTrue(targetObject.activeSelf, "Target object should stay visible when reset while already normal.");
    }

}