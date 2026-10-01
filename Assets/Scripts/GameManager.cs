using UnityEngine;

public class GameManager : MonoBehaviour
{
  [SerializeField] private Door_win_loss DoorSide;
  //[SerializeField] private AnomalyManager anomalyManager;

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
    /*---------------------------------------------------------------------------------------------
    DoorSelected(DoorType) selectedDoor
    - Takes in door type 
    - takes in anomalyState for the level
    -Checks anomaly state and door state
    - if door state is left and annomaly is incative (anomalyState && selectedDoor == Left)
      - Level failed
      -else if anomaly is active then passed
    - if door state is right and anomaly is inactive (!anomalyState && selectedDoor == Right)
      -passed 
      -else failed 
      --------------------------------------------------------------------------------------------
    */
    public void DoorSelected(DoorType selectedDoor)
    {
      //bool anomalyState = anomalyManager.AnomalyActive 
      if(selectedDoor == DoorType.Left)
      {
            //pass
        Debug.Log("Left");

        }else if(selectedDoor == DoorType.Right)
        {
          Debug.Log("Right");
        }else{
            //fail
        }


    }
}
