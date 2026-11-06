using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// PlayMode: this fixture supplies ALL required references before OnEnable / Start.
// Scene-changing tests are Explicit so they cannot disrupt ordinary CI runs.
[Category("GameplayRegression")]
public class UIManagerFunctionalTests
{
    private GameObject gameManagerObject;
    private GameObject uiManagerObject;
    private GameObject pauseMenu;
    private GameObject winMenu;
    private GameObject loseMenu;
    private GameManager gameManager;
    private UIManager uiManager;

    private float originalTimeScale;
    private CursorLockMode originalCursorLock;
    private bool originalCursorVisible;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        originalTimeScale = Time.timeScale;
        originalCursorLock = Cursor.lockState;
        originalCursorVisible = Cursor.visible;
        Time.timeScale = 1f;
        AnomalyManager.ResetRun();

        gameManagerObject = new GameObject("TGCoverage_GameManager");
        gameManager = gameManagerObject.AddComponent<GameManager>();

        pauseMenu = new GameObject("TGCoverage_PauseMenu");
        winMenu = new GameObject("TGCoverage_WinMenu");
        loseMenu = new GameObject("TGCoverage_LoseMenu");
        // Verify Start actually initializes the menus, even if active in the scene.
        pauseMenu.SetActive(true);
        winMenu.SetActive(true);
        loseMenu.SetActive(true);

        // Add UIManager while inactive: its OnEnable must run after fields are assigned.
        uiManagerObject = new GameObject("TGCoverage_UIManager");
        uiManagerObject.SetActive(false);
        uiManager = uiManagerObject.AddComponent<UIManager>();
        uiManager.gameManager = gameManager;
        uiManager.pauseMenu = pauseMenu;
        uiManager.winMenu = winMenu;
        uiManager.loseMenu = loseMenu;
        uiManagerObject.SetActive(true);
        yield return null; // Allow UIManager.Start() to execute.
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        // Ensure cleanup proceeds even if a test stopped in-game time.
        Time.timeScale = 1f;
        if (uiManagerObject != null) Object.Destroy(uiManagerObject);
        if (gameManagerObject != null) Object.Destroy(gameManagerObject);
        if (pauseMenu != null) Object.Destroy(pauseMenu);
        if (winMenu != null) Object.Destroy(winMenu);
        if (loseMenu != null) Object.Destroy(loseMenu);
        yield return null;

        Time.timeScale = originalTimeScale;
        Cursor.lockState = originalCursorLock;
        Cursor.visible = originalCursorVisible;
        AnomalyManager.ResetRun();
    }

    [Test]
    public void Start_HidesAllMenus()
    {
        Assert.IsFalse(pauseMenu.activeSelf);
        Assert.IsFalse(winMenu.activeSelf);
        Assert.IsFalse(loseMenu.activeSelf);
        Assert.AreEqual(GameManager.GameState.Exploring, gameManager.CurrentState);
    }

    [Test]
    public void TogglePause_EntersPausedStateAndStopsTime()
    {
        uiManager.TogglePause();
        Assert.AreEqual(GameManager.GameState.Paused, gameManager.CurrentState);
        Assert.IsTrue(pauseMenu.activeSelf);
        Assert.AreEqual(0f, Time.timeScale);
        Assert.IsFalse(winMenu.activeSelf);
        Assert.IsFalse(loseMenu.activeSelf);
    }

    [Test]
    public void TogglePause_Twice_ResumesExplorationAndTime()
    {
        uiManager.TogglePause();
        uiManager.TogglePause();
        Assert.AreEqual(GameManager.GameState.Exploring, gameManager.CurrentState);
        Assert.IsFalse(pauseMenu.activeSelf);
        Assert.AreEqual(1f, Time.timeScale);
    }

    [Test]
    public void TogglePause_ThreeTimes_EndsPaused()
    {
        uiManager.TogglePause();
        uiManager.TogglePause();
        uiManager.TogglePause();
        Assert.AreEqual(GameManager.GameState.Paused, gameManager.CurrentState);
        Assert.IsTrue(pauseMenu.activeSelf);
        Assert.AreEqual(0f, Time.timeScale);
    }

    [Test]
    public void GameManagerVictoryEvent_OpensOnlyWinMenuAndFreezesTime()
    {
        LogAssert.Expect(LogType.Log, "You Win!");
        gameManager.ChangeState(GameManager.GameState.Victory);
        Assert.IsTrue(winMenu.activeSelf);
        Assert.IsFalse(loseMenu.activeSelf);
        Assert.IsFalse(pauseMenu.activeSelf);
        Assert.AreEqual(0f, Time.timeScale);
    }

    [Test]
    public void GameManagerFailureEvent_OpensOnlyLoseMenuAndFreezesTime()
    {
        LogAssert.Expect(LogType.Log, "Level Failed - Retrying Same Anomaly");
        gameManager.ChangeState(GameManager.GameState.Failure);
        Assert.IsTrue(loseMenu.activeSelf);
        Assert.IsFalse(winMenu.activeSelf);
        Assert.IsFalse(pauseMenu.activeSelf);
        Assert.AreEqual(0f, Time.timeScale);
    }

    [Test]
    public void PausedThenVictory_HidesPauseAndShowsWin()
    {
        uiManager.TogglePause();
        LogAssert.Expect(LogType.Log, "You Win!");
        gameManager.ChangeState(GameManager.GameState.Victory);
        Assert.IsFalse(pauseMenu.activeSelf);
        Assert.IsTrue(winMenu.activeSelf);
        Assert.IsFalse(loseMenu.activeSelf);
        Assert.AreEqual(0f, Time.timeScale);
    }

    [Test]
    public void PausedThenFailure_HidesPauseAndShowsLose()
    {
        uiManager.TogglePause();
        LogAssert.Expect(LogType.Log, "Level Failed - Retrying Same Anomaly");
        gameManager.ChangeState(GameManager.GameState.Failure);
        Assert.IsFalse(pauseMenu.activeSelf);
        Assert.IsTrue(loseMenu.activeSelf);
        Assert.IsFalse(winMenu.activeSelf);
        Assert.AreEqual(0f, Time.timeScale);
    }

    [Test]
    public void ChangingFromVictoryToFailure_SwapsVisibleEndMenu()
    {
        LogAssert.Expect(LogType.Log, "You Win!");
        LogAssert.Expect(LogType.Log, "Level Failed - Retrying Same Anomaly");
        gameManager.ChangeState(GameManager.GameState.Victory);
        gameManager.ChangeState(GameManager.GameState.Failure);
        Assert.IsFalse(winMenu.activeSelf);
        Assert.IsTrue(loseMenu.activeSelf);
    }

    [TestCase(GameManager.GameState.Victory)]
    [TestCase(GameManager.GameState.Failure)]
    public void TogglePause_AfterGameOver_CannotDismissEndMenu(GameManager.GameState endState)
    {
        if (endState == GameManager.GameState.Victory)
            LogAssert.Expect(LogType.Log, "You Win!");
        else
            LogAssert.Expect(LogType.Log, "Level Failed - Retrying Same Anomaly");

        gameManager.ChangeState(endState);
        uiManager.TogglePause();
        Assert.AreEqual(endState, gameManager.CurrentState);
        Assert.IsFalse(pauseMenu.activeSelf);
        Assert.AreEqual(0f, Time.timeScale);
        Assert.AreEqual(endState == GameManager.GameState.Victory, winMenu.activeSelf);
        Assert.AreEqual(endState == GameManager.GameState.Failure, loseMenu.activeSelf);
    }

    [Test]
    public void DisablingUIManager_UnsubscribesFromEvents()
    {
        uiManagerObject.SetActive(false);
        LogAssert.Expect(LogType.Log, "You Win!");
        gameManager.ChangeState(GameManager.GameState.Victory);
        Assert.IsFalse(winMenu.activeSelf, "Disabled UI should not process a state event.");
        Assert.AreEqual(1f, Time.timeScale);
    }

    [Test]
    public void ReEnablingUIManager_ResubscribesToStateEvents()
    {
        uiManagerObject.SetActive(false);
        uiManagerObject.SetActive(true);
        LogAssert.Expect(LogType.Log, "Level Failed - Retrying Same Anomaly");
        gameManager.ChangeState(GameManager.GameState.Failure);
        Assert.IsTrue(loseMenu.activeSelf);
        Assert.AreEqual(0f, Time.timeScale);
    }

#if UNITY_EDITOR
    [Test]
    public void QuitButton_InEditor_LogsIntentAndResumesTime()
    {
        uiManager.TogglePause();
        LogAssert.Expect(LogType.Log, "Quit!");
        uiManager.QuitButton();
        Assert.AreEqual(1f, Time.timeScale);
    }
#endif

    // Run separately in a disposable scene with Main-Menu enabled in Build Profiles.
    [UnityTest, Explicit("Run separately: loads the Main-Menu scene."), Category("SceneIntegration")]
    public IEnumerator MainMenuButton_ActivatesMainMenuScene()
    {
        Assert.IsTrue(Application.CanStreamedLevelBeLoaded("Main-Menu"));
        uiManager.MainMenuButton();
        float deadline = Time.realtimeSinceStartup + 5f;
        while (SceneManager.GetActiveScene().name != "Main-Menu")
        {
            if (Time.realtimeSinceStartup > deadline)
                Assert.Fail("Main-Menu was not loaded within 5 seconds.");
            yield return null;
        }
        Assert.AreEqual("Main-Menu", SceneManager.GetActiveScene().name);
    }

    // Desired hardening: any transition back to Exploring should unfreeze gameplay.
    // Currently HandleStateChanged only changes timeScale on Victory/Failure.
    [Test, Explicit("Known gap: changing to Exploring after Victory leaves time frozen.")]
    [Category("KnownGap")]
    public void ExploringAfterVictory_ShouldUnfreezeGameplay()
    {
        LogAssert.Expect(LogType.Log, "You Win!");
        gameManager.ChangeState(GameManager.GameState.Victory);
        gameManager.ChangeState(GameManager.GameState.Exploring);
        Assert.IsFalse(winMenu.activeSelf);
        Assert.AreEqual(1f, Time.timeScale);
    }
}
