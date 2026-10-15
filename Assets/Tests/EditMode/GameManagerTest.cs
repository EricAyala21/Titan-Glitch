// Edit Mode tests for the UPDATED GameManager version with
// an AnomalyManager reference, CompleteCurrentAnomaly(), and StartNewGame().
// Save to Assets/Tests/EditMode/GameManagerTests.cs.
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GameManagerTests
{
    private GameObject managerObject;
    private GameManager gameManager;

    [SetUp]
    public void SetUp()
    {
        AnomalyManager.ResetRun();
        managerObject = new GameObject("GameManager_ForTest");
        gameManager = managerObject.AddComponent<GameManager>();
    }

    [TearDown]
    public void TearDown()
    {
        if (managerObject != null)
            Object.DestroyImmediate(managerObject);
        AnomalyManager.ResetRun();
    }

    [Test]
    public void InitialState_IsMainMenu()
    {
        // MainMenu is the zero-valued enum entry in the updated script.
        Assert.AreEqual(GameManager.GameState.MainMenu, gameManager.CurrentState);
    }

    [TestCase(GameManager.GameState.MainMenu)]
    [TestCase(GameManager.GameState.TutorialBaseline)]
    [TestCase(GameManager.GameState.Exploring)]
    [TestCase(GameManager.GameState.Decision)]
    [TestCase(GameManager.GameState.Validation)]
    [TestCase(GameManager.GameState.Feedback)]
    [TestCase(GameManager.GameState.Paused)]
    public void ChangeState_NonSceneLoadingState_UpdatesCurrentState(
        GameManager.GameState targetState)
    {
        gameManager.ChangeState(targetState);
        Assert.AreEqual(targetState, gameManager.CurrentState);
    }

    [Test]
    public void ChangeState_Twice_StoresLatestState()
    {
        gameManager.ChangeState(GameManager.GameState.Exploring);
        gameManager.ChangeState(GameManager.GameState.Decision);
        Assert.AreEqual(GameManager.GameState.Decision, gameManager.CurrentState);
    }

    [Test]
    public void ChangeState_Victory_LogsWinAndSetsVictory()
    {
        LogAssert.Expect(LogType.Log, "You Win!");
        gameManager.ChangeState(GameManager.GameState.Victory);
        Assert.AreEqual(GameManager.GameState.Victory, gameManager.CurrentState);
    }

    [Test]
    public void ChangeState_ProgressionWithMissingAnomalyManager_LogsError()
    {
        // Missing dependency prevents scene reloading in the UPDATED GameManager.
        LogAssert.Expect(LogType.Error, "AnomalyManager reference is missing!");
        gameManager.ChangeState(GameManager.GameState.Progression);
        Assert.AreEqual(GameManager.GameState.Progression, gameManager.CurrentState);
    }

    [Test]
    public void DoorSelected_LeftWithMissingAnomalyManager_RequestsProgression()
    {
        LogAssert.Expect(LogType.Log, "Left Door Selected");
        LogAssert.Expect(LogType.Error, "AnomalyManager reference is missing!");
        gameManager.DoorSelected(DoorType.Left);
        Assert.AreEqual(GameManager.GameState.Progression, gameManager.CurrentState);
    }

    [Test]
    public void DoorSelected_UnknownDoor_LeavesStateUnchanged()
    {
        gameManager.ChangeState(GameManager.GameState.Exploring);
        gameManager.DoorSelected((DoorType)(-1));
        Assert.AreEqual(GameManager.GameState.Exploring, gameManager.CurrentState);
    }

    [Test]
    public void Progression_OnLastAnomaly_TransitionsToVictory()
    {
        // Set up an AnomalyManager with ONE valid anomaly. Simulate that
        // the currently spawned anomaly has index 0, without loading a scene.
        GameObject anomalyManagerObject = new GameObject("AnomalyManager_ForTest");
        GameObject anomalyPrefab = new GameObject("AnomalyPrefab_ForTest");

        try
        {
            var anomalyManager = anomalyManagerObject.AddComponent<AnomalyManager>();
            anomalyManager.anomalies = new[] { anomalyPrefab };

            // GameManager.anomalyManager is private and serialized.
            FieldInfo managerField = typeof(GameManager).GetField(
                "anomalyManager", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(managerField, "Expected updated GameManager.anomalyManager field.");
            managerField.SetValue(gameManager, anomalyManager);

            // CurrentAnomalyIndex has a private setter in the updated script.
            PropertyInfo indexProperty = typeof(AnomalyManager).GetProperty(
                "CurrentAnomalyIndex", BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(indexProperty);
            MethodInfo setter = indexProperty.GetSetMethod(true);
            Assert.IsNotNull(setter);
            setter.Invoke(anomalyManager, new object[] { 0 });

            LogAssert.Expect(LogType.Log, "Completed anomaly index: 0");
            LogAssert.Expect(LogType.Log, "You Win!");

            gameManager.ChangeState(GameManager.GameState.Progression);

            Assert.AreEqual(GameManager.GameState.Victory, gameManager.CurrentState);
            Assert.AreEqual(1, anomalyManager.GetCompletedAnomalyCount());
        }
        finally
        {
            Object.DestroyImmediate(anomalyManagerObject);
            Object.DestroyImmediate(anomalyPrefab);
        }
    }
}
