
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SystemResourceMonitorTests
{
    private GameObject testObject;
    private SystemResourceMonitor monitor;

    [SetUp]
    public void Setup()
    {
        testObject = new GameObject("SOC2 Resource Test");
        monitor = testObject.AddComponent<SystemResourceMonitor>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(testObject);
    }

    [Test]
    public void SOC2_RES_01_ValidFPS()
    {
        // Normal FPS should not trigger a warning.
        bool result = monitor.CheckThresholds(60f, 100f);

        Assert.IsTrue(result);
        Assert.AreEqual(0, monitor.WarningCount);
    }

    [Test]
    public void SOC2_RES_02_LowFPSWarning()
    {
        LogAssert.Expect(
            LogType.Warning,
            "[SOC2 RESOURCE] Low FPS detected: 15"
        );

        bool result = monitor.CheckThresholds(15f, 100f);

        Assert.IsFalse(result);
        Assert.AreEqual(1, monitor.WarningCount);
    }

    [Test]
    public void SOC2_RES_03_ValidMemory()
    {
        // Normal managed memory should not trigger a warning.
        bool result = monitor.CheckThresholds(60f, 256f);

        Assert.IsTrue(result);
        Assert.AreEqual(0, monitor.WarningCount);
    }

    [Test]
    public void SOC2_RES_04_HighMemoryWarning()
    {
        LogAssert.Expect(
            LogType.Warning,
            "[SOC2 RESOURCE] High memory usage detected: 2048 MB"
        );

        bool result = monitor.CheckThresholds(60f, 2048f);

        Assert.IsFalse(result);
        Assert.AreEqual(1, monitor.WarningCount);
    }
}
