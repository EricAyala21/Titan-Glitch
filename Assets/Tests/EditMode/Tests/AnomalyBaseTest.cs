using NUnit.Framework;
using UnityEngine;

//Test Version of Anomaly Base class to test the AnomalyBase class

public class TestAnomaly : AnomalyBase

{
    public override void ActivateAnomaly()
    {
        anomalyActive = true;
    }

    public override void ResetAnomaly()
    {
        anomalyActive = false;
    }
}

public class AnomalyBaseTest
{
    private GameObject testObject;
    private TestAnomaly testAnomaly;

    [SetUp]
    public void Setup()
    {
        testObject = new GameObject("Test Anomaly");
        testAnomaly = testObject.AddComponent<TestAnomaly>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(testObject);
    }

    [Test]
    public void ActivateAnomalySetsActive()
    {
        testAnomaly.ActivateAnomaly();
        Assert.IsTrue(testAnomaly.IsActive);
    }

    [Test]
    public void ResetAnomalySetsInactive()
    {
        testAnomaly.ActivateAnomaly();
        Assert.IsTrue(testAnomaly.IsActive);

        testAnomaly.ResetAnomaly();
        Assert.IsFalse(testAnomaly.IsActive);

        testAnomaly.ActivateAnomaly();
        Assert.IsTrue(testAnomaly.IsActive);
    }
    
    [Test]
    public void AnomalyStartsInactive()
    {
        Assert.IsFalse(testAnomaly.IsActive);
    }

    [Test]
    public void ActivateAnomalyTwiceRemainsActive()
    {
        testAnomaly.ActivateAnomaly();
        testAnomaly.ActivateAnomaly();

        Assert.IsTrue(testAnomaly.IsActive);

    }

    [Test]
    public void ResetAnomalyTwiceRemainsInactive()
    {
        testAnomaly.ActivateAnomaly();

        testAnomaly.ResetAnomaly();
        testAnomaly.ResetAnomaly();
        Assert.IsFalse(testAnomaly.IsActive);   
    }

    [Test]
    public void ResetBeforeActivationRemainsInactive()
    {
        testAnomaly.ResetAnomaly();
        Assert.IsFalse(testAnomaly.IsActive);
    }

    [Test]
    public void MultipleActivationResetCycles()
    {
        for (int i = 0; i < 10; i++)
        {
            testAnomaly.ActivateAnomaly();
            Assert.IsTrue(testAnomaly.IsActive);

            testAnomaly.ResetAnomaly();
            Assert.IsFalse(testAnomaly.IsActive);
        }
    }

    [Test]
    public void TwoAnomaliesOperateIndependently()
    {
        GameObject secondObject = new GameObject("Second Test Anomaly");

        try
        {
            TestAnomaly secondAnomaly = secondObject.AddComponent<TestAnomaly>();
            

            Assert.IsFalse(secondAnomaly.IsActive);
            testAnomaly.ActivateAnomaly();

            Assert.IsTrue(testAnomaly.IsActive);
            Assert.IsFalse(secondAnomaly.IsActive);
        }
    }
}