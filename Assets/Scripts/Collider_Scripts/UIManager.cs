using UnityEngine;
using UnityEngine.SceneManagement;
public class UIManager : MonoBehaviour
{
    private bool isPaused = false;
    public GameObject pauseMenu;
    public GameManager gameManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            gameManager.ChangeState(GameManager.GameState.Paused);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            pauseMenu.SetActive(false);
            gameManager.ChangeState(GameManager.GameState.Exploring);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
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
