using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private bool isPaused = false;

    public GameObject pauseMenu;
    public GameObject winMenu;
    public GameObject loseMenu;
    public GameObject anomalyMenu;

    public GameManager gameManager;



    private void OnEnable()
    {
        if (gameManager != null)
            gameManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        if (gameManager != null)
            gameManager.OnStateChanged -= HandleStateChanged;
    }

    private void Start()
    {
        if (gameManager == null)
        {
            Debug.LogError("UIManager: GameManager reference is missing!");
            return;
        }

        HandleStateChanged(gameManager.CurrentState);
    }
    private void Update()
    {
        if (gameManager == null)
            return;

        // Escape cannot open the pause menu during anomaly selection,
        // victory, or failure.
        if (gameManager.CurrentState != GameManager.GameState.Exploring &&
            gameManager.CurrentState != GameManager.GameState.Paused)
            return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void HandleStateChanged(GameManager.GameState newState)
    {
        if (winMenu != null)
            winMenu.SetActive(newState == GameManager.GameState.Victory);

        if (loseMenu != null)
            loseMenu.SetActive(newState == GameManager.GameState.Failure);

        if (anomalyMenu != null)
            anomalyMenu.SetActive(newState == GameManager.GameState.ChoosingAnomaly);

        if (newState == GameManager.GameState.ChoosingAnomaly ||
            newState == GameManager.GameState.Failure ||
            newState == GameManager.GameState.Victory ||
            newState == GameManager.GameState.Progression)
        {
            isPaused = false;
            if (pauseMenu != null)
                pauseMenu.SetActive(false);

            SetGameplayBlocked(true);
        }
        else if (newState == GameManager.GameState.Paused)
        {
            isPaused = true;
            if (pauseMenu != null)
                pauseMenu.SetActive(true);

            SetGameplayBlocked(true);
        }
        else if (newState == GameManager.GameState.Exploring)
        {
            isPaused = false;
            if (pauseMenu != null)
                pauseMenu.SetActive(false);

            SetGameplayBlocked(false);
        }
    }
    private void SetGameplayBlocked(bool blocked)
    {
        Time.timeScale = blocked ? 0f : 1f;
        Cursor.lockState = blocked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = blocked;

        // PlayerController otherwise can still rotate the camera
        // while the game is paused, even with Time.timeScale == 0.
 
    }

    public void TogglePause()
    {
        if (gameManager == null)
            return;

        if (gameManager.CurrentState == GameManager.GameState.Exploring)
        {
            gameManager.ChangeState(GameManager.GameState.Paused);
        }
        else if (gameManager.CurrentState == GameManager.GameState.Paused)
        {
            gameManager.ChangeState(GameManager.GameState.Exploring);
        }
    }

    // Assign these methods to the five Button > On Click() events
    // inside the anomalyMenu GameObject.
    public void SelectWrongFloorNumber()
    {
        SubmitAnomaly(AnomalyCategory.WrongFloorNumber);
    }

    public void SelectWrongObjectOrder()
    {
        SubmitAnomaly(AnomalyCategory.WrongObjectOrder);
    }

    public void SelectWrongSizeOrShape()
    {
        SubmitAnomaly(AnomalyCategory.WrongSizeOrShape);
    }

    public void SelectSpookyElements()
    {
        SubmitAnomaly(AnomalyCategory.SpookyElements);
    }

    public void SelectChase()
    {
        SubmitAnomaly(AnomalyCategory.Chase);
    }

    public void SelectNoAnomaly()
    {
        SubmitAnomaly(AnomalyCategory.None);
    }

    private void SubmitAnomaly(AnomalyCategory category)
    {
        if (gameManager != null)
            gameManager.SubmitAnomalyGuess(category);
    }

    public void RetryButton()
    {
        if (gameManager != null)
            gameManager.RetryLevel();
    }

    public void PlayAgainButton()
    {
        if (gameManager != null)
            gameManager.StartNewGame();
    }

    public void MainMenuButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main-Menu");
    }

    public void QuitButton()
    {
        Time.timeScale = 1f;
        Debug.Log("Quit!");
        Application.Quit();
    }
}