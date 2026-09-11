using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        TutorialBaseline,
        Exploring,
        Decision,
        Validation,
        Feedback,
        Progression,
        FailureReset,
        Victory,
        Paused
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameState CurrentState { get; private set; }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GameState.MainMenu:
               // EnterMainMenu();
                break;

            case GameState.TutorialBaseline:
               // EnterTutorialBaseline();
                break;

            case GameState.Exploring:
              //  EnterExploring();
                break;

            case GameState.Decision:
              //  EnterDecision();
                break;

            case GameState.Validation:
              //  EnterValidation();
                break;

            case GameState.Feedback:
              //  EnterFeedback();
                break;

            case GameState.Progression:
               // EnterProgression();
                break;

            case GameState.FailureReset:
               // EnterFailureReset();
                break;

            case GameState.Victory:
              //  EnterVictory();
                break;

            case GameState.Paused:
              //  EnterPaused();
                break;
        }
    }
}
