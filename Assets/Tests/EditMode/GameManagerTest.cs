using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// EditMode: scene-changing paths are intentionally excluded from Run All.
[Category("GameplayRegression")]
public class GameManagerTests
{
    private readonly List<GameObject> owned = new List<GameObject>();
    private GameManager gameManager;

    [SetUp]
    public void SetUp()
    {
        AnomalyManager.ResetRun();
        gameManager = Own(new GameObject("TGCoverage_GameManager")).AddComponent<GameManager>();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject obj in owned)
            if (obj != null) Object.DestroyImmediate(obj);
        owned.Clear();
        AnomalyManager.ResetRun();
    }

    [Test]
    public void InitialState_IsExploring()
    {
        Assert.AreEqual(GameManager.GameState.Exploring, gameManager.CurrentState);
    }

    [TestCase(GameManager.GameState.Exploring)]
    [TestCase(GameManager.GameState.Paused)]
    public void SafeStateChange_UpdatesStateAndNotifiesSubscriber(GameManager.GameState desired)
    {
        List<GameManager.GameState> notifications = Listen();
        gameManager.ChangeState(desired);
        Assert.AreEqual(desired, gameManager.CurrentState);
        CollectionAssert.AreEqual(new[] { desired }, notifications);
    }

    [Test]
    public void MultipleStateChanges_NotifySubscribersInOrder()
    {
        List<GameManager.GameState> notifications = Listen();
        gameManager.ChangeState(GameManager.GameState.Paused);
        gameManager.ChangeState(GameManager.GameState.Exploring);
        CollectionAssert.AreEqual(
            new[] { GameManager.GameState.Paused, GameManager.GameState.Exploring },
            notifications);
        Assert.AreEqual(GameManager.GameState.Exploring, gameManager.CurrentState);
    }

    [Test]
    public void UnsubscribedListener_ReceivesNoFurtherNotifications()
    {
        int received = 0;
        System.Action<GameManager.GameState> listener = _ => received++;
        gameManager.OnStateChanged += listener;
        gameManager.ChangeState(GameManager.GameState.Paused);
        gameManager.OnStateChanged -= listener;
        gameManager.ChangeState(GameManager.GameState.Exploring);
        Assert.AreEqual(1, received);
    }

    [Test]
    public void VictoryState_NotifiesAndRemainsVictory()
    {
        List<GameManager.GameState> notifications = Listen();
        LogAssert.Expect(LogType.Log, "You Win!");
        gameManager.ChangeState(GameManager.GameState.Victory);
        Assert.AreEqual(GameManager.GameState.Victory, gameManager.CurrentState);
        CollectionAssert.AreEqual(new[] { GameManager.GameState.Victory }, notifications);
    }

    [Test]
    public void FailureState_NotifiesAndDoesNotImmediatelyReload()
    {
        List<GameManager.GameState> notifications = Listen();
        LogAssert.Expect(LogType.Log, "Level Failed - Retrying Same Anomaly");
        gameManager.ChangeState(GameManager.GameState.Failure);
        Assert.AreEqual(GameManager.GameState.Failure, gameManager.CurrentState);
        CollectionAssert.AreEqual(new[] { GameManager.GameState.Failure }, notifications);
    }

    [Test]
    public void DoorSelected_IncorrectDoor_EntersFailure()
    {
        List<GameManager.GameState> notifications = Listen();
        LogAssert.Expect(LogType.Log, "No Anomaly Door Selected");
        LogAssert.Expect(LogType.Log, "Level Failed - Retrying Same Anomaly");
        gameManager.DoorSelected(DoorType.NoAnomaly);
        Assert.AreEqual(GameManager.GameState.Failure, gameManager.CurrentState);
        CollectionAssert.AreEqual(new[] { GameManager.GameState.Failure }, notifications);
    }

    [Test]
    public void FailedDoor_WithActiveAnomaly_DoesNotAwardCompletion()
    {
        AnomalyManager anomalyManager = MakeAnomalyManager();
        anomalyManager.anomalies = new[] { Own(new GameObject("TGCoverage_ActivePrefab")) };
        SetCurrentAnomalyIndex(anomalyManager, 0);
        InjectAnomalyManager(anomalyManager);

        LogAssert.Expect(LogType.Log, "No Anomaly Door Selected");
        LogAssert.Expect(LogType.Log, "Level Failed - Retrying Same Anomaly");
        gameManager.DoorSelected(DoorType.NoAnomaly);

        Assert.AreEqual(GameManager.GameState.Failure, gameManager.CurrentState);
        Assert.AreEqual(0, anomalyManager.GetCompletedAnomalyCount());
        Assert.AreEqual(0, anomalyManager.CurrentAnomalyIndex,
            "A failed choice must not clear the pending anomaly.");
    }

    [Test]
    public void DoorSelected_UnknownDoor_DoesNotChangeStateOrNotify()
    {
        List<GameManager.GameState> notifications = Listen();
        gameManager.DoorSelected((DoorType)(-1));
        Assert.AreEqual(GameManager.GameState.Exploring, gameManager.CurrentState);
        Assert.IsEmpty(notifications);
    }

    [Test]
    public void Progression_MissingAnomalyManager_ReportsErrorButDoesNotAwardProgress()
    {
        List<GameManager.GameState> notifications = Listen();
        LogAssert.Expect(LogType.Error, "AnomalyManager reference is missing!");
        gameManager.ChangeState(GameManager.GameState.Progression);
        Assert.AreEqual(GameManager.GameState.Progression, gameManager.CurrentState);
        CollectionAssert.AreEqual(new[] { GameManager.GameState.Progression }, notifications);
    }

    [Test]
    public void Progression_WithManagerButNoActiveAnomaly_ReportsError()
    {
        AnomalyManager anomalyManager = MakeAnomalyManager();
        InjectAnomalyManager(anomalyManager);
        List<GameManager.GameState> notifications = Listen();
        LogAssert.Expect(LogType.Error, "No active anomaly to complete.");
        gameManager.ChangeState(GameManager.GameState.Progression);
        Assert.AreEqual(0, anomalyManager.GetCompletedAnomalyCount());
        Assert.AreEqual(GameManager.GameState.Progression, gameManager.CurrentState);
        CollectionAssert.AreEqual(new[] { GameManager.GameState.Progression }, notifications);
    }

    [Test]
    public void DoorSelected_CorrectDoorWithMissingManager_ReportsProblem()
    {
        LogAssert.Expect(LogType.Log, "Anomaly Door Selected");
        LogAssert.Expect(LogType.Error, "AnomalyManager reference is missing!");
        gameManager.DoorSelected(DoorType.Anomaly);
        Assert.AreEqual(GameManager.GameState.Progression, gameManager.CurrentState);
    }

    [Test]
    public void FinalAnomalyCompletion_TriggersVictoryAndAwardsExactlyOne()
    {
        AnomalyManager anomalyManager = MakeAnomalyManager();
        anomalyManager.anomalies = new[] { Own(new GameObject("TGCoverage_SolePrefab")) };
        InjectAnomalyManager(anomalyManager);
        SetCurrentAnomalyIndex(anomalyManager, 0);
        List<GameManager.GameState> notifications = Listen();

        LogAssert.Expect(LogType.Log, "Anomaly Door Selected");
        LogAssert.Expect(LogType.Log, "Completed anomaly index: 0");
        LogAssert.Expect(LogType.Log, "You Win!");
        gameManager.DoorSelected(DoorType.Anomaly);

        Assert.AreEqual(1, anomalyManager.GetCompletedAnomalyCount());
        Assert.IsFalse(anomalyManager.HasUnusedAnomalies());
        Assert.AreEqual(GameManager.GameState.Victory, gameManager.CurrentState);
        CollectionAssert.AreEqual(
            new[] { GameManager.GameState.Progression, GameManager.GameState.Victory },
            notifications);
    }

    [Test]
    public void RetryLevel_WithoutFailureState_DoesNotChangeState()
    {
        gameManager.ChangeState(GameManager.GameState.Paused);
        gameManager.RetryLevel();
        Assert.AreEqual(GameManager.GameState.Paused, gameManager.CurrentState);
    }

    [Test]
    public void RetryLevel_AfterVictory_DoesNotOverrideWinState()
    {
        LogAssert.Expect(LogType.Log, "You Win!");
        gameManager.ChangeState(GameManager.GameState.Victory);
        gameManager.RetryLevel();
        Assert.AreEqual(GameManager.GameState.Victory, gameManager.CurrentState);
    }

    // Desired hardening: a door event must not reverse an already final state.
    // Currently the manager accepts a new door event after Victory.
    [Test, Explicit("Known gap: DoorSelected accepts inputs after game over.")]
    [Category("KnownGap")]
    public void DoorSelected_AfterVictory_ShouldNotEnterFailure()
    {
        LogAssert.Expect(LogType.Log, "You Win!");
        gameManager.ChangeState(GameManager.GameState.Victory);
        gameManager.DoorSelected(DoorType.NoAnomaly);
        Assert.AreEqual(GameManager.GameState.Victory, gameManager.CurrentState,
            "Further door selections must be ignored after victory.");
    }

    private AnomalyManager MakeAnomalyManager()
    {
        AnomalyManager manager = Own(new GameObject("TGCoverage_AnomalyManager"))
            .AddComponent<AnomalyManager>();
        manager.anomalies = new GameObject[0];
        manager.spawnPoints = new Transform[0];
        return manager;
    }

    private void InjectAnomalyManager(AnomalyManager manager)
    {
        FieldInfo field = typeof(GameManager).GetField(
            "anomalyManager", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field, "Missing serialized GameManager.anomalyManager field.");
        field.SetValue(gameManager, manager);
    }

    private static void SetCurrentAnomalyIndex(AnomalyManager manager, int index)
    {
        PropertyInfo property = typeof(AnomalyManager).GetProperty("CurrentAnomalyIndex");
        Assert.IsNotNull(property);
        MethodInfo setter = property.GetSetMethod(true);
        Assert.IsNotNull(setter);
        setter.Invoke(manager, new object[] { index });
    }

    private List<GameManager.GameState> Listen()
    {
        var states = new List<GameManager.GameState>();
        gameManager.OnStateChanged += state => states.Add(state);
        return states;
    }

    private GameObject Own(GameObject obj)
    {
        owned.Add(obj);
        return obj;
    }
}
