
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AnomalyStateMonitorTests
{
    private GameObject testObject;
    private TestAnomaly anomaly;
    private AnomalyStateMonitor monitor;

    [SetUp]
    public void Setup()
    {
        testObject = new GameObject("SOC2 Test Anomaly");

        anomaly = testObject.AddComponent<TestAnomaly>();
        monitor = testObject.AddComponent<AnomalyStateMonitor>();

        monitor.Initialize(anomaly);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(testObject);
    }

    [Test]
    public void SOC2_MON_01_InitialState()
    {
        Assert.IsFalse(anomaly.IsActive);
        Assert.IsTrue(monitor.CheckState());
        Assert.AreEqual(0, monitor.WarningCount);
    }

    [Test]
    public void SOC2_MON_02_ValidActivation()
    {
        monitor.ExpectState(true);
        anomaly.ActivateAnomaly();

        Assert.IsTrue(monitor.CheckState());
        Assert.AreEqual(0, monitor.WarningCount);
    }

    [Test]
    public void SOC2_MON_03_ValidReset()
    {
        monitor.ExpectState(true);
        anomaly.ActivateAnomaly();
        Assert.IsTrue(monitor.CheckState());

        monitor.ExpectState(false);
        anomaly.ResetAnomaly();

        Assert.IsTrue(monitor.CheckState());
        Assert.AreEqual(0, monitor.WarningCount);
    }

    [Test]
    public void SOC2_MON_04_RepeatedActivation()
    {
        monitor.ExpectState(true);

        anomaly.ActivateAnomaly();
        anomaly.ActivateAnomaly();

        Assert.IsTrue(anomaly.IsActive);
        Assert.IsTrue(monitor.CheckState());
        Assert.AreEqual(0, monitor.WarningCount);
    }

    [Test]
    public void SOC2_MON_05_UnexpectedStateDetected()
    {
        // No approved activation was registered.
        anomaly.ActivateAnomaly();

        LogAssert.Expect(
            LogType.Warning,
            "[SOC2 MONITOR] Unexpected anomaly state detected. " +
            "Expected: False, Actual: True"
        );

        Assert.IsFalse(monitor.CheckState());
        Assert.AreEqual(1, monitor.WarningCount);
    }
}
