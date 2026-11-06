using NUnit.Framework;
using System.Reflection;
using UnityEngine;

public class LightOffAnomalyTests
{
    private GameObject anomalyGO;
    private GameObject lightGO;
    private Light light;
    private LightOffAnomaly anomaly;

    [SetUp]
    public void Setup()
    {
        anomalyGO = new GameObject("LightOffAnomaly");
        lightGO = new GameObject("TestLight");
        light = lightGO.AddComponent<Light>();
        anomaly = anomalyGO.AddComponent<LightOffAnomaly>();

        // targetLight is private, so set it the way the Inspector would
        typeof(LightOffAnomaly)
            .GetField("targetLight", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(anomaly, light);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(anomalyGO);
        Object.DestroyImmediate(lightGO);
    }

    [Test]
    public void Activate_TurnsLightOff()
    {
        anomaly.ActivateAnomaly();
        Assert.IsFalse(light.enabled, "Light should be off when the anomaly is active.");
        Assert.IsTrue(anomaly.IsAnomalous);
    }

    [Test]
    public void Reset_TurnsLightOn()
    {
        anomaly.ActivateAnomaly();
        anomaly.ResetAnomaly();
        Assert.IsTrue(light.enabled, "Light should be on after reset.");
        Assert.IsFalse(anomaly.IsAnomalous);
    }

    [Test]
    public void Toggle_SwitchesState()
    {
        anomaly.Toggle();
        Assert.IsFalse(light.enabled);
        anomaly.Toggle();
        Assert.IsTrue(light.enabled);
    }

    [Test]
    public void Start_ResetsToNormal()
    {
        anomaly.ActivateAnomaly();
        MethodInfo start = typeof(LightOffAnomaly).GetMethod(
            "Start", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(start, "Start() method not found.");
        start.Invoke(anomaly, null);
        Assert.IsTrue(light.enabled, "Start() should turn the light back on.");
        Assert.IsFalse(anomaly.IsAnomalous);
    }
}