
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Door_win_loss DoorSide;
    [SerializeField] private AnomalyManager anomalyManager;

    public event System.Action<GameState> OnStateChanged;

    public enum GameState
    {
        Exploring,
        Progression,
        Failure,
        Victory,
        Paused
    }

    public GameState CurrentState { get; private set; }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;

        // Notify UIManager that the state changed.
        OnStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.Exploring:
                break;

            case GameState.Progression:
                EnterProgression();
                break;

            case GameState.Failure:
                EnterFailure();
                break;

            case GameState.Victory:
                EnterVictory();
                break;

            case GameState.Paused:
                break;
        }
    }

    public void DoorSelected(DoorType selectedDoor)
    {
        if (selectedDoor == DoorType.Anomaly)
        {
            Debug.Log("Anomaly Door Selected");

            ChangeState(GameState.Progression);
        }
        else if (selectedDoor == DoorType.NoAnomaly)
        {
            Debug.Log("No Anomaly Door Selected");

            ChangeState(GameState.Failure);
        }
    }

    private void EnterFailure()
    {
        Debug.Log("Level Failed - Retrying Same Anomaly");

        // Do not change music.
        // The current song or music timer continues.
    }

    private void EnterProgression()
    {
        if (anomalyManager == null)
        {
            Debug.LogError(
                "AnomalyManager reference is missing!"
            );
            return;
        }

        if (anomalyManager.CurrentAnomalyIndex < 0)
        {
            Debug.LogError(
                "No active anomaly to complete."
            );
            return;
        }

        // Mark the current anomaly completed.
        bool allCompleted =
            anomalyManager.CompleteCurrentAnomaly();

        // Check if all anomalies have been completed.
        if (allCompleted)
        {
            ChangeState(GameState.Victory);
            return;
        }

        Debug.Log(
            "Level Passed - Loading Next Anomaly"
        );

        // Play a new random song for the next level.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRandomLevelSong();
        }

        CurrentState = GameState.Exploring;

        // Reload scene with a new unused anomaly.
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    private void EnterVictory()
    {
        Debug.Log("You Win!");

        // No new level song is triggered.
    }

    // Use when the player starts a new game.
    public void StartNewGame()
    {
        Time.timeScale = 1f;

        AnomalyManager.ResetRun();

        // Start fresh music for the new game.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRandomLevelSong();
        }

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void RetryLevel()
    {
        if (CurrentState != GameState.Failure)
            return;

        Time.timeScale = 1f;

        // Keep the current music playing.
        // Reload without completing the anomaly.
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
