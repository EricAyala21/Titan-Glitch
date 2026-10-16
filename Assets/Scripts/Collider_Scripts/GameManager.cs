
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Door_win_loss DoorSide;
    [SerializeField] private AnomalyManager anomalyManager;

    public enum GameState
    {
        MainMenu,
        TutorialBaseline,
        Exploring,
        Decision,
        Validation,
        Feedback,
        Progression,
        Failure,
        Victory,
        Paused
    }

    public GameState CurrentState { get; private set; }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GameState.MainMenu:
                break;

            case GameState.TutorialBaseline:
                break;

            case GameState.Exploring:
                break;

            case GameState.Decision:
                break;

            case GameState.Validation:
                break;

            case GameState.Feedback:
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
       
        if (selectedDoor == DoorType.Left)
        {
            // Currently the successful door.
            Debug.Log("Left Door Selected");
            ChangeState(GameState.Progression);
        }
        else if (selectedDoor == DoorType.Right)
        {
            // Currently the incorrect door.
            Debug.Log("Right Door Selected");
            ChangeState(GameState.Failure);
        }
    }

    private void EnterFailure()
    {
        Debug.Log("Level Failed - Retrying Same Anomaly");

        // Do NOT mark the anomaly completed.
        // AnomalyManager remembers which one was active.
        CurrentState = GameState.Exploring;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
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

        // TODO: Display victory UI or load victory scene.
    }

    // Use this when the player starts a completely new game.
    public void StartNewGame()
    {
        AnomalyManager.ResetRun();

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
