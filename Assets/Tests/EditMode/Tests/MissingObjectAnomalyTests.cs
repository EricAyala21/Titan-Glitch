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

}