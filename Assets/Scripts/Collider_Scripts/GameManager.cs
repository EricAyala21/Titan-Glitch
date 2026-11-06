
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
            // Currently the successful door.
            Debug.Log("Anomaly Door Selected");
            ChangeState(GameState.Progression);
        }
        else if (selectedDoor == DoorType.NoAnomaly)
        {
            // Currently the incorrect door.
            Debug.Log("No Anomaly Door Selected");
            ChangeState(GameState.Failure);
        }
    }

    private void EnterFailure()
    {
        Debug.Log("Level Failed - Retrying Same Anomaly");

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
            Debug.LogError("No active anomaly to complete.");
            return;
        }

        // Mark the current anomaly completed.
        bool allCompleted =
            anomalyManager.CompleteCurrentAnomaly();

        // Check if there are any anomalies remaining.
        if (allCompleted)
        {
            ChangeState(GameState.Victory);
            return;
        }

        Debug.Log("Level Passed - Loading Next Anomaly");

        
        CurrentState = GameState.Exploring;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    private void EnterVictory()
    {
        
        Debug.Log("You Win!");

      
    }

    // Use this when the player starts a completely new game.
    public void StartNewGame()
    {
        Time.timeScale = 1f;
        AnomalyManager.ResetRun();

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }


    public void RetryLevel()
    {
        if (CurrentState != GameState.Failure)
            return;

        Time.timeScale = 1f;

        // Reload without completing the current anomaly.
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
