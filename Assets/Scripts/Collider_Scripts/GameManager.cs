using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private AnomalyManager anomalyManager;

    public event System.Action<GameState> OnStateChanged;

    public enum GameState
    {
        Exploring,
        ChoosingAnomaly,
        Progression,
        Failure,
        Victory,
        Paused
    }

    public GameState CurrentState { get; private set; } = GameState.Exploring;

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.Progression:
                EnterProgression();
                break;
            case GameState.Failure:
                EnterFailure();
                break;
            case GameState.Victory:
                EnterVictory();
                break;
        }
    }

    // Called by the existing Door_win_loss trigger.
    public void DoorSelected(DoorType selectedDoor)
    {
        // Ignore repeated collision events while a menu is already open.
        if (CurrentState != GameState.Exploring)
            return;

        if (selectedDoor == DoorType.Anomaly)
        {
            Debug.Log("Anomaly door selected - showing anomaly menu.");
            ChangeState(GameState.ChoosingAnomaly);
        }
        else if (selectedDoor == DoorType.NoAnomaly)
        {
            // Preserve current behavior of the No Anomaly door.
            Debug.Log("No Anomaly door selected - level failed.");
            ChangeState(GameState.Failure);
        }
    }
    // Called only from one of the UIManager's answer buttons.
    public void SubmitAnomalyGuess(AnomalyCategory selectedCategory)
    {
        if (CurrentState != GameState.ChoosingAnomaly)
            return;

        if (anomalyManager == null)
        {
            Debug.LogError("AnomalyManager reference is missing!");
            return;
        }

        AnomalyCategory correctCategory;
        if (!anomalyManager.TryGetCurrentAnomalyCategory(out correctCategory))
        {
            // Leave the menu open rather than penalizing a setup mistake.
            return;
        }

        if (selectedCategory == correctCategory)
        {
            Debug.Log("Correct anomaly choice: " + selectedCategory);
            ChangeState(GameState.Progression);
        }
        else
        {
            Debug.Log(
                "Incorrect anomaly choice: " + selectedCategory +
                ". Actual anomaly: " + correctCategory);
            ChangeState(GameState.Failure);
        }
    }

    private void EnterFailure()
    {
        Debug.Log("Level Failed - Retrying Same Anomaly");
        // UIManager opens the existing loseMenu; its Retry button
        // calls RetryLevel(). The active anomaly remains unchanged.
    }

    private void EnterProgression()
    {
        if (anomalyManager == null)
        {
            Debug.LogError("AnomalyManager reference is missing!");
            return;
        }

        if (anomalyManager.CurrentAnomalyIndex < 0)
        {
            Debug.LogError("No active anomaly to complete.");
            return;
        }

        bool allCompleted = anomalyManager.CompleteCurrentAnomaly();

        if (allCompleted)
        {
            ChangeState(GameState.Victory);
            return;
        }

        Debug.Log("Level Passed - Loading Next Anomaly");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayRandomLevelSong();

        // UIManager paused the game while the answer menu was open.
        // Reset time scale before loading the next level.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void EnterVictory()
    {
        Debug.Log("You Win!");
    }

    public void StartNewGame()
    {
        Time.timeScale = 1f;
        AnomalyManager.ResetRun();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayRandomLevelSong();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RetryLevel()
    {
        if (CurrentState != GameState.Failure)
            return;

        Time.timeScale = 1f;

        // Reload without completing the anomaly: AnomalyManager
        // retains the same active anomaly and spawn point.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}