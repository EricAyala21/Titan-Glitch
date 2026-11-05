using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private bool isPaused = false;

    public GameObject pauseMenu;
    public GameObject winMenu;
    public GameObject loseMenu;

    public GameManager gameManager;

    private void OnEnable()
    {
        if (gameManager != null)
        {
            gameManager.OnStateChanged += HandleStateChanged;
        }
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.OnStateChanged -= HandleStateChanged;
        }
    }

    void Start()
    {
        isPaused = false;

        pauseMenu.SetActive(false);
        winMenu.SetActive(false);
        loseMenu.SetActive(false);

        HandleStateChanged(gameManager.CurrentState);
    }

    void Update()
    {
        // Prevent pausing when the game is over.
        if (gameManager.CurrentState ==
                GameManager.GameState.Victory ||
            gameManager.CurrentState ==
                GameManager.GameState.Failure)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void HandleStateChanged(
        GameManager.GameState newState)
    {
        // Activate the appropriate end-game menu.
        winMenu.SetActive(
            newState == GameManager.GameState.Victory
        );

        loseMenu.SetActive(
            newState == GameManager.GameState.Failure
        );

        if (newState == GameManager.GameState.Victory ||
            newState == GameManager.GameState.Failure)
        {
            isPaused = false;
            pauseMenu.SetActive(false);

            // Freeze gameplay.
            Time.timeScale = 0f;

            // Allow interaction with UI buttons.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void TogglePause()
    {
        if (gameManager.CurrentState ==
                GameManager.GameState.Victory ||
            gameManager.CurrentState ==
                GameManager.GameState.Failure)
        {
            return;
        }

        isPaused = !isPaused;

        if (isPaused)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;

            gameManager.ChangeState(
                GameManager.GameState.Paused
            );

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            pauseMenu.SetActive(false);

            gameManager.ChangeState(
                GameManager.GameState.Exploring
            );

            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void RetryButton()
    {
        gameManager.RetryLevel();
    }

    public void PlayAgainButton()
    {
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
