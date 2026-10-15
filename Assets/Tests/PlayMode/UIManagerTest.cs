using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Place this script in Assets/Tests/PlayMode/.
// The Play Mode test assembly must reference the runtime assembly containing UIManager.
public class UIManagerFunctionalTests
{
    private GameObject managerObject;
    private GameObject pauseMenu;
    private UIManager uiManager;

    private float originalTimeScale;
    private CursorLockMode originalCursorLockState;
    private bool originalCursorVisibility;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Protect the rest of the test suite from UIManager's global changes.
        originalTimeScale = Time.timeScale;
        originalCursorLockState = Cursor.lockState;
        originalCursorVisibility = Cursor.visible;

        Time.timeScale = 1f;
        pauseMenu = new GameObject("TestPauseMenu");
        pauseMenu.SetActive(false);

        managerObject = new GameObject("TestUIManager");
        uiManager = managerObject.AddComponent<UIManager>();
        uiManager.pauseMenu = pauseMenu;

        // Allow UIManager.Start() to execute before assertions.
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        // Reset time before yielding, even if a test paused the game.
        Time.timeScale = originalTimeScale;
        Cursor.lockState = originalCursorLockState;
        Cursor.visible = originalCursorVisibility;

        if (managerObject != null)
            Object.Destroy(managerObject);

        if (pauseMenu != null)
            Object.Destroy(pauseMenu);

        yield return null;
    }

    [Test]
    public void TogglePause_FirstCall_ShowsPauseMenu()
    {
        uiManager.TogglePause();

        Assert.IsTrue(pauseMenu.activeSelf,
            "The pause menu should be active when the game is paused.");
    }

    [Test]
    public void TogglePause_FirstCall_StopsGameTime()
    {
        uiManager.TogglePause();

        Assert.AreEqual(0f, Time.timeScale,
            "Time.timeScale should be zero while paused.");
    }

    [Test]
    public void TogglePause_SecondCall_HidesPauseMenu()
    {
        uiManager.TogglePause();
        uiManager.TogglePause();

        Assert.IsFalse(pauseMenu.activeSelf,
            "The pause menu should be inactive after resuming.");
    }

    [Test]
    public void TogglePause_SecondCall_ResumesGameTime()
    {
        uiManager.TogglePause();
        uiManager.TogglePause();

        Assert.AreEqual(1f, Time.timeScale,
            "Time.timeScale should return to one after resuming.");
    }

    [Test]
    public void TogglePause_ThreeCalls_PausesAgain()
    {
        uiManager.TogglePause();
        uiManager.TogglePause();
        uiManager.TogglePause();

        Assert.IsTrue(pauseMenu.activeSelf);
        Assert.AreEqual(0f, Time.timeScale);
    }

    [Test]
    public void TogglePause_FourCalls_ReturnsToRunningState()
    {
        for (int i = 0; i < 4; i++)
            uiManager.TogglePause();

        Assert.IsFalse(pauseMenu.activeSelf);
        Assert.AreEqual(1f, Time.timeScale);
    }

    [Test]
    public void TogglePause_DoesNotDestroyPauseMenu()
    {
        uiManager.TogglePause();
        uiManager.TogglePause();

        Assert.IsNotNull(pauseMenu,
            "Toggling the pause menu must not destroy it.");
    }

#if UNITY_EDITOR
    [Test]
    public void QuitButton_InEditor_LogsQuitMessage()
    {
        // Application.Quit() is ignored in the Unity Editor; do not run this
        // test in a standalone player, where it would close the test process.
        LogAssert.Expect(LogType.Log, "Quit!");

        uiManager.QuitButton();
    }
#endif

    // Opt-in integration test: run this test individually, NOT in your
    // regular Run All / CI suite. It replaces the active scene.
    // Requires a scene named "Main-Menu" in Build Settings / Build Profiles.
    [UnityTest]
    [Explicit("Run separately with Main-Menu in the build scene list; this changes scenes.")]
    [Category("SceneIntegration")]
    public IEnumerator MainMenuButton_LoadsMainMenuScene()
    {
        Assert.IsTrue(Application.CanStreamedLevelBeLoaded("Main-Menu"),
            "Add the Main-Menu scene to the build scene list before running this test.");

        uiManager.MainMenuButton();

        float deadline = Time.realtimeSinceStartup + 5f;
        while (SceneManager.GetActiveScene().name != "Main-Menu")
        {
            if (Time.realtimeSinceStartup > deadline)
                Assert.Fail("MainMenuButton did not activate Main-Menu within 5 seconds.");

            yield return null;
        }

        Assert.AreEqual("Main-Menu", SceneManager.GetActiveScene().name);
    }

}