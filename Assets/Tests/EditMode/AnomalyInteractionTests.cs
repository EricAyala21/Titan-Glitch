
using NUnit.Framework;
using UnityEngine;
using System.Reflection;

// Tests anomaly completion and progression behavior.
public class AnomalyInteractionTests
{
    private GameObject testObject;
    private AnomalyManager anomalyManager;

    [SetUp]
    public void Setup()
    {
        // Reset progress before each test.
        AnomalyManager.ResetRun();

        testObject = new GameObject("Test Anomaly Manager");

        anomalyManager =
            testObject.AddComponent<AnomalyManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(testObject);

        // Prevent test data from affecting other tests.
        AnomalyManager.ResetRun();
    }

    // Set the current anomaly index for testing.
    private void SetCurrentAnomaly(int index)
    {
        FieldInfo field = typeof(AnomalyManager).GetField(
            "<CurrentAnomalyIndex>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        Assert.IsNotNull(
            field,
            "CurrentAnomalyIndex backing field not found."
        );

        field.SetValue(anomalyManager, index);
    }

    [Test]
    public void AnomalyStartsWithNoCompletion()
    {
        Assert.AreEqual(
            0,
            anomalyManager.GetCompletedAnomalyCount()
        );
    }

    [Test]
    public void CompletingAnomalyIncreasesCompletedCount()
    {
        // Arrange
        anomalyManager.anomalies = new GameObject[2];

        anomalyManager.anomalies[0] =
            new GameObject("Anomaly 1");

        anomalyManager.anomalies[1] =
            new GameObject("Anomaly 2");

        SetCurrentAnomaly(0);

        // Act
        anomalyManager.CompleteCurrentAnomaly();

        // Assert
        Assert.AreEqual(
            1,
            anomalyManager.GetCompletedAnomalyCount()
        );

        Assert.AreEqual(
            -1,
            anomalyManager.CurrentAnomalyIndex
        );

        // Clean up temporary anomaly objects.
        Object.DestroyImmediate(anomalyManager.anomalies[0]);
        Object.DestroyImmediate(anomalyManager.anomalies[1]);
    }

    [Test]
    public void CompletingAllAnomaliesReturnsTrue()
    {
        // Arrange
        GameObject anomaly =
            new GameObject("Test Anomaly");

        anomalyManager.anomalies =
            new GameObject[] { anomaly };

        SetCurrentAnomaly(0);

        // Act
        bool allCompleted =
            anomalyManager.CompleteCurrentAnomaly();

        // Assert
        Assert.IsTrue(allCompleted);

        Assert.AreEqual(
            1,
            anomalyManager.GetCompletedAnomalyCount()
        );

        Object.DestroyImmediate(anomaly);
    }

    [Test]
    public void ResetRunClearsCompletedAnomalies()
    {
        // Arrange
        GameObject anomaly =
            new GameObject("Test Anomaly");

        anomalyManager.anomalies =
            new GameObject[] { anomaly };

        SetCurrentAnomaly(0);

        anomalyManager.CompleteCurrentAnomaly();

        // Act
        AnomalyManager.ResetRun();

        // Assert
        Assert.AreEqual(
            0,
            anomalyManager.GetCompletedAnomalyCount()
        );

        Object.DestroyImmediate(anomaly);
    }
}