using UnityEngine;
using UnityEngine.SceneManagement;
public class UIManager : MonoBehaviour
{
    private bool isPaused = false;
    public GameObject pauseMenu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                pauseMenu.SetActive(true);
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

            }
            else
            {
                pauseMenu.SetActive(false);
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }


    public void MainMenuButton()
    {
        SceneManager.LoadScene("Main-Menu");
    }

    public void QuitButton()
    {
        Debug.Log("Quit!");
        Application.Quit();
    }
}
