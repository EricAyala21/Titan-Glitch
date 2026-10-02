using NUnit.Framework;
using UnityEngine;

public class GameManagerTests
{
    
    private GameObject gameObject;
    private GameManager gameManager;

    [SetUp]
    public void SetUp()
    {
        gameObject = new GameObject("GameManager");
        gameManager = gameObject.AddComponent<GameManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void ChangeState_ToExploring_UpdatesCurrentState()
    {
        gameManager.ChangeState(GameManager.GameState.Exploring);

        Assert.AreEqual(
            GameManager.GameState.Exploring,
            gameManager.CurrentState
        );
    }

    [Test]
    public void ChangeState_ToPaused_UpdatesCurrentState()
    {
        gameManager.ChangeState(GameManager.GameState.Paused);

        Assert.AreEqual(
            GameManager.GameState.Paused,
            gameManager.CurrentState
        );
    }
    
}